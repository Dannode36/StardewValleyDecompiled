using System;

namespace Netcode.Validation;

/// <summary>Indicates that the field isn't synchronized in multiplayer, so there's no need to validate it as such in <see cref="T:Netcode.Validation.NetFieldValidator" />.</summary>
[AttributeUsage(AttributeTargets.Field)]
public class NotNetFieldAttribute : Attribute
{
}
