using System;

namespace StardewValley;

/// <summary>
/// When specified, this static field will be instanced for split screen multiplayer.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class InstancedStatic : Attribute
{
}
