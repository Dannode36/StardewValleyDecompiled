using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StardewValley.Internal;

/// <summary>Handles creating delegates for static methods by their string method names.</summary>
public static class StaticDelegateBuilder
{
	/// <summary>A cached delegate creation.</summary>
	private struct CachedDelegate
	{
		/// <summary>The created delegate instance, if valid.</summary>
		public readonly object CreatedDelegate;

		/// <summary>An error phrase indicating why the delegate couldn't be created, if applicable.</summary>
		public readonly string Error;

		/// <summary>Construct an instance.</summary>
		/// <param name="createdDelegate">The created delegate instance, if valid.</param>
		/// <param name="error">An error phrase indicating why the delegate couldn't be created, if applicable.</param>
		public CachedDelegate(object createdDelegate, string error)
		{
			CreatedDelegate = createdDelegate;
			Error = error;
		}
	}

	/// <summary>A cache of delegate resolution results, indexed by delegate type and then full method name.</summary>
	private static readonly Dictionary<Type, Dictionary<string, CachedDelegate>> CachedDelegates = new Dictionary<Type, Dictionary<string, CachedDelegate>>();

	/// <summary>Create a delegate for a static method.</summary>
	/// <typeparam name="TDelegate">The delegate type.</typeparam>
	/// <param name="fullMethodName">The full method name in the form <c>fullTypeName.methodName</c> (like <c>StardewValley.Object.OutputDeconstructor</c>).</param>
	/// <param name="createdDelegate">The created delegate instance, if valid.</param>
	/// <param name="error">An error phrase indicating why the delegate couldn't be created, if applicable.</param>
	/// <returns>Returns whether the delegate was successfully created.</returns>
	public static bool TryCreateDelegate<TDelegate>(string fullMethodName, out TDelegate createdDelegate, out string error) where TDelegate : Delegate
	{
		if (string.IsNullOrWhiteSpace(fullMethodName))
		{
			error = "the method name can't be empty";
			createdDelegate = null;
			return false;
		}
		if (!CachedDelegates.TryGetValue(typeof(TDelegate), out var cacheByName))
		{
			cacheByName = (CachedDelegates[typeof(TDelegate)] = new Dictionary<string, CachedDelegate>());
		}
		if (!cacheByName.TryGetValue(fullMethodName, out var cached))
		{
			string[] parts = LegacyShims.SplitAndTrim(fullMethodName, ':');
			if (parts.Length != 2)
			{
				error = "invalid method name format, expected a type full name and method separated with a colon (:)";
				createdDelegate = null;
				return false;
			}
			string fullTypeName = parts[0];
			string methodName = parts[1];
			if (Game1.GameAssemblyName != "Stardew Valley" && fullTypeName.Contains("Stardew Valley"))
			{
				parts = LegacyShims.SplitAndTrim(fullTypeName, ',');
				if (ArgUtility.Get(parts, 1) == "Stardew Valley")
				{
					parts[1] = Game1.GameAssemblyName;
					fullTypeName = string.Join(", ", parts);
				}
			}
			Type type = Type.GetType(fullTypeName);
			if (type == null)
			{
				error = "could not find type '" + fullTypeName + "'";
				createdDelegate = null;
				return false;
			}
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null)
			{
				error = $"could not find method '{methodName}' on type '{fullTypeName}'";
				createdDelegate = null;
				return false;
			}
			if (!method.IsStatic)
			{
				error = $"found method '{methodName}' on type '{fullTypeName}', but the method isn't static";
				createdDelegate = null;
				return false;
			}
			try
			{
				createdDelegate = (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), null, method);
				error = null;
			}
			catch (ArgumentException)
			{
				MethodInfo delegateMethod = typeof(TDelegate).GetMethod("Invoke");
				createdDelegate = null;
				error = $"failed to bind method '{fullMethodName}': it didn't match the expected signature {delegateMethod.ReturnType} method({string.Join(", ", from p in delegateMethod.GetParameters()
					select $"{p.ParameterType} {p.Name}")})";
			}
			cached = (cacheByName[fullMethodName] = new CachedDelegate(createdDelegate, error));
		}
		createdDelegate = (TDelegate)cached.CreatedDelegate;
		error = cached.Error;
		return createdDelegate != null;
	}
}
