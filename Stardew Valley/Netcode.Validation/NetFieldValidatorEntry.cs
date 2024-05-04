using System;
using System.Reflection;

namespace Netcode.Validation;

/// <summary>The metadata for a field being validated by <see cref="T:Netcode.Validation.NetFieldValidator" />.</summary>
public class NetFieldValidatorEntry
{
	/// <summary>The name of the net field being synced.</summary>
	public string Name { get; }

	/// <summary>The synchronized field value.</summary>
	public object Value { get; }

	/// <summary>The C# field on the owner.</summary>
	public FieldInfo FromField { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="name">The name of the net field being synced.</param>
	/// <param name="value">The raw net field.</param>
	/// <param name="fromField">The C# field or property on the owner.</param>
	public NetFieldValidatorEntry(string name, object value, FieldInfo fromField)
	{
		Name = name;
		Value = value;
		FromField = fromField;
	}

	/// <summary>Get a validator entry for a C# field or property, if it's a net field.</summary>
	/// <param name="owner">The object instance whose net fields are being read.</param>
	/// <param name="field">The C# field or property to read.</param>
	/// <param name="netField">The validator entry, if it's a net field.</param>
	public static bool TryGetNetField(INetObject<NetFields> owner, FieldInfo field, out NetFieldValidatorEntry netField)
	{
		if (field.Name != "NetFields" && field.Name[0] != '<')
		{
			Type valueType = field.FieldType;
			if (typeof(INetSerializable).IsAssignableFrom(valueType) && !IsMarkedNotImplicitNetField(valueType))
			{
				INetSerializable value = (INetSerializable)field.GetValue(owner);
				netField = new NetFieldValidatorEntry(value?.Name, value, field);
				return true;
			}
			if (typeof(INetObject<NetFields>).IsAssignableFrom(valueType) && !IsMarkedNotImplicitNetField(valueType))
			{
				INetObject<NetFields> value = (INetObject<NetFields>)field.GetValue(owner);
				netField = new NetFieldValidatorEntry(value?.NetFields.Name, value, field);
				return true;
			}
		}
		netField = null;
		return false;
	}

	/// <summary>Get whether a field is marked with <see cref="T:Netcode.Validation.NotNetFieldAttribute" />.</summary>
	public bool IsMarkedNotNetField()
	{
		return FromField.GetCustomAttribute<NotNetFieldAttribute>() != null;
	}

	/// <summary>Get whether a type is marked with <see cref="T:Netcode.Validation.NotImplicitNetFieldAttribute" />.</summary>
	/// <param name="type">The type to check.</param>
	public static bool IsMarkedNotImplicitNetField(Type type)
	{
		return type.GetCustomAttribute<NotImplicitNetFieldAttribute>(inherit: true) != null;
	}
}
