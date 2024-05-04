using System;

namespace StardewValley.Internal;

/// <summary>A method attribute which indicates it has alternative names.</summary>
/// <remarks>This only applies to cases that specifically handle it, like debug commands or event preconditions registered by the game directly. For custom cases, you generally need to call a specific method like <see cref="M:StardewValley.GameStateQuery.RegisterAlias(System.String,System.String)" /> instead.</remarks>
[AttributeUsage(AttributeTargets.Method)]
public class OtherNamesAttribute : Attribute
{
	/// <summary>The alternate names for the method.</summary>
	public string[] Aliases { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="aliases">The alternate names for the method.</param>
	public OtherNamesAttribute(params string[] aliases)
	{
		Aliases = aliases;
	}
}
