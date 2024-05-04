using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace StardewValley;

public class LocalMultiplayer
{
	public delegate void StaticInstanceMethod(object staticVarsHolder);

	internal static List<FieldInfo> staticFields;

	internal static List<object> staticDefaults;

	public static Type StaticVarHolderType;

	private static DynamicMethod staticDefaultMethod;

	private static DynamicMethod staticSaveMethod;

	private static DynamicMethod staticLoadMethod;

	public static StaticInstanceMethod StaticSetDefault;

	public static StaticInstanceMethod StaticSave;

	public static StaticInstanceMethod StaticLoad;

	public static bool IsLocalMultiplayer(bool is_local_only = false)
	{
		if (is_local_only)
		{
			return Game1.hasLocalClientsOnly;
		}
		return GameRunner.instance.gameInstances.Count > 1;
	}

	public static void Initialize()
	{
		GetStaticFieldsAndDefaults();
		GenerateDynamicMethodsForStatics();
	}

	private static void GetStaticFieldsAndDefaults()
	{
		staticFields = new List<FieldInfo>();
		staticDefaults = new List<object>();
		HashSet<string> ignored_assembly_roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Microsoft", "MonoGame", "mscorlib", "NetCode", "System", "xTile", "FAudio-CS" };
		List<Type> types = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (!ignored_assembly_roots.Contains(assembly.GetName().Name.Split('.')[0]))
			{
				Type[] types2 = assembly.GetTypes();
				foreach (Type type in types2)
				{
					types.Add(type);
				}
			}
		}
		foreach (Type type in types)
		{
			if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
			{
				continue;
			}
			bool include_by_default = type.GetCustomAttribute<InstanceStatics>() != null;
			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo field in fields)
			{
				if (!field.IsInitOnly && field.IsStatic && !field.IsLiteral && (include_by_default || field.GetCustomAttribute<InstancedStatic>() != null) && field.GetCustomAttribute<NonInstancedStatic>() == null)
				{
					RuntimeHelpers.RunClassConstructor(field.DeclaringType.TypeHandle);
					staticFields.Add(field);
					staticDefaults.Add(field.GetValue(null));
				}
			}
		}
	}

	private static void GenerateDynamicMethodsForStatics()
	{
		TypeBuilder typeBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("StardewValley.StaticInstanceVars"), AssemblyBuilderAccess.RunAndCollect).DefineDynamicModule("MainModule").DefineType("StardewValley.StaticInstanceVars", TypeAttributes.Public | TypeAttributes.AutoClass);
		foreach (FieldInfo field in staticFields)
		{
			typeBuilder.DefineField(field.DeclaringType.Name + "_" + field.Name, field.FieldType, FieldAttributes.Public);
		}
		StaticVarHolderType = typeBuilder.CreateType();
		staticDefaultMethod = new DynamicMethod("SetStaticVarsToDefault", null, new Type[1] { typeof(object) }, typeof(Game1).Module, skipVisibility: true);
		ILGenerator il = staticDefaultMethod.GetILGenerator();
		LocalBuilder local = il.DeclareLocal(StaticVarHolderType);
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Castclass, StaticVarHolderType);
		il.Emit(OpCodes.Stloc_0);
		FieldInfo defaultsField = typeof(LocalMultiplayer).GetField("staticDefaults", BindingFlags.Static | BindingFlags.NonPublic);
		MethodInfo listIndexOperator = typeof(List<object>).GetMethod("get_Item");
		for (int i = 0; i < staticFields.Count; i++)
		{
			FieldInfo field = staticFields[i];
			il.Emit(OpCodes.Ldloc, local.LocalIndex);
			il.Emit(OpCodes.Ldsfld, defaultsField);
			il.Emit(OpCodes.Ldc_I4, i);
			il.Emit(OpCodes.Callvirt, listIndexOperator);
			if (field.FieldType.IsValueType)
			{
				il.Emit(OpCodes.Unbox_Any, field.FieldType);
			}
			else
			{
				il.Emit(OpCodes.Castclass, field.FieldType);
			}
			il.Emit(OpCodes.Stfld, StaticVarHolderType.GetField(field.DeclaringType.Name + "_" + field.Name));
		}
		il.Emit(OpCodes.Ret);
		StaticSetDefault = (StaticInstanceMethod)staticDefaultMethod.CreateDelegate(typeof(StaticInstanceMethod));
		staticSaveMethod = new DynamicMethod("SaveStaticVars", null, new Type[1] { typeof(object) }, typeof(Game1).Module, skipVisibility: true);
		il = staticSaveMethod.GetILGenerator();
		local = il.DeclareLocal(StaticVarHolderType);
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Castclass, StaticVarHolderType);
		il.Emit(OpCodes.Stloc_0);
		foreach (FieldInfo field in staticFields)
		{
			il.Emit(OpCodes.Ldloc, local.LocalIndex);
			il.Emit(OpCodes.Ldsfld, field);
			il.Emit(OpCodes.Stfld, StaticVarHolderType.GetField(field.DeclaringType.Name + "_" + field.Name));
		}
		il.Emit(OpCodes.Ret);
		StaticSave = (StaticInstanceMethod)staticSaveMethod.CreateDelegate(typeof(StaticInstanceMethod));
		staticLoadMethod = new DynamicMethod("LoadStaticVars", null, new Type[1] { typeof(object) }, typeof(Game1).Module, skipVisibility: true);
		il = staticLoadMethod.GetILGenerator();
		local = il.DeclareLocal(StaticVarHolderType);
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Castclass, StaticVarHolderType);
		il.Emit(OpCodes.Stloc_0);
		foreach (FieldInfo field in staticFields)
		{
			il.Emit(OpCodes.Ldloc, local.LocalIndex);
			il.Emit(OpCodes.Ldfld, StaticVarHolderType.GetField(field.DeclaringType.Name + "_" + field.Name));
			il.Emit(OpCodes.Stsfld, field);
		}
		il.Emit(OpCodes.Ret);
		StaticLoad = (StaticInstanceMethod)staticLoadMethod.CreateDelegate(typeof(StaticInstanceMethod));
	}

	public static void SaveOptions()
	{
		if (Game1.player != null && (bool)Game1.player.isCustomized)
		{
			Game1.splitscreenOptions[Game1.player.UniqueMultiplayerID] = Game1.options;
		}
	}
}
