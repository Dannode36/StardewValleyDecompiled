using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Force.DeepCloner.Helpers;

/// <summary>
/// Safe types are types, which can be copied without real cloning. e.g. simple structs or strings (it is immutable)
/// </summary>
internal static class DeepClonerSafeTypes
{
	internal static readonly ConcurrentDictionary<Type, bool> KnownTypes;

	static DeepClonerSafeTypes()
	{
		KnownTypes = new ConcurrentDictionary<Type, bool>();
		Type[] array = new Type[19]
		{
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(char),
			typeof(string),
			typeof(bool),
			typeof(DateTime),
			typeof(IntPtr),
			typeof(UIntPtr),
			typeof(Guid),
			Type.GetType("System.RuntimeType"),
			Type.GetType("System.RuntimeTypeHandle")
		};
		foreach (Type x in array)
		{
			KnownTypes.TryAdd(x, value: true);
		}
	}

	private static bool CanReturnSameType(Type type, HashSet<Type> processingTypes)
	{
		if (KnownTypes.TryGetValue(type, out var isSafe))
		{
			return isSafe;
		}
		if (type.IsEnum() || type.IsPointer)
		{
			KnownTypes.TryAdd(type, value: true);
			return true;
		}
		if (type.FullName.StartsWith("System.DBNull"))
		{
			KnownTypes.TryAdd(type, value: true);
			return true;
		}
		if (type.FullName.StartsWith("System.RuntimeType"))
		{
			KnownTypes.TryAdd(type, value: true);
			return true;
		}
		if (type.FullName.StartsWith("System.Reflection.") && object.Equals(type.GetTypeInfo().Assembly, typeof(PropertyInfo).GetTypeInfo().Assembly))
		{
			KnownTypes.TryAdd(type, value: true);
			return true;
		}
		if (type.IsSubclassOfTypeByName("CriticalFinalizerObject"))
		{
			KnownTypes.TryAdd(type, value: true);
			return true;
		}
		if (type.FullName.StartsWith("Microsoft.Extensions.DependencyInjection."))
		{
			KnownTypes.TryAdd(type, value: true);
			return true;
		}
		if (type.FullName == "Microsoft.EntityFrameworkCore.Internal.ConcurrencyDetector")
		{
			KnownTypes.TryAdd(type, value: true);
			return true;
		}
		if (!type.IsValueType())
		{
			KnownTypes.TryAdd(type, value: false);
			return false;
		}
		if (processingTypes == null)
		{
			processingTypes = new HashSet<Type>();
		}
		processingTypes.Add(type);
		List<FieldInfo> fi = new List<FieldInfo>();
		Type tp = type;
		do
		{
			fi.AddRange(tp.GetAllFields());
			tp = tp.BaseType();
		}
		while (tp != null);
		foreach (FieldInfo item in fi)
		{
			Type fieldType = item.FieldType;
			if (!processingTypes.Contains(fieldType) && !CanReturnSameType(fieldType, processingTypes))
			{
				KnownTypes.TryAdd(type, value: false);
				return false;
			}
		}
		KnownTypes.TryAdd(type, value: true);
		return true;
	}

	public static bool CanReturnSameObject(Type type)
	{
		return CanReturnSameType(type, null);
	}
}
