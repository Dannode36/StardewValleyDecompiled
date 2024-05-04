using System;
using System.Reflection;
using Sickhead.Engine.Util;

namespace StardewValley.Extensions;

public static class ReflectionExtensions
{
	/// <summary>Try to set the field or property's value from its string representation.</summary>
	/// <param name="info">The field or property to set.</param>
	/// <param name="obj">The object instance whose field or property to set.</param>
	/// <param name="rawValue">A string representation of the value to set. This will be converted to the property type if possible.</param>
	/// <param name="index">Optional index values for an indexed property. This should be null for fields or non-indexed properties.</param>
	/// <param name="error">An error indicating why the property value could not be set, if applicable.</param>
	public static bool TrySetValueFromString(this MemberInfo info, object obj, string rawValue, object[] index, out string error)
	{
		Type valueType;
		bool canWrite;
		if (!(info is FieldInfo field))
		{
			if (!(info is PropertyInfo property))
			{
				error = "the member is not a field or property";
				return false;
			}
			valueType = property.PropertyType;
			canWrite = property.CanWrite;
		}
		else
		{
			valueType = field.FieldType;
			canWrite = !field.IsLiteral && !field.IsLiteral;
		}
		if (!canWrite)
		{
			error = "the " + ((info is FieldInfo) ? "field" : "property") + " property is read-only";
			return false;
		}
		object value;
		try
		{
			value = Convert.ChangeType(rawValue, valueType);
		}
		catch (FormatException)
		{
			error = $"can't convert value '{rawValue}' to the '{valueType.FullName}' type";
			return false;
		}
		try
		{
			info.SetValue(obj, value, index);
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			return false;
		}
	}
}
