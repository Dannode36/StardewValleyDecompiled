using System;

namespace StardewValley;

/// <summary>
/// When specified, all static fields in this class will be instanced for split screen multiplayer by default.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class InstanceStatics : Attribute
{
}
