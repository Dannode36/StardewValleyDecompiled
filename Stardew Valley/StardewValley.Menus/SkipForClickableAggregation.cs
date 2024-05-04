using System;

namespace StardewValley.Menus;

/// <summary>
/// When specified, this field will not be automatically added to the allClickableComponents list when it is populated.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SkipForClickableAggregation : Attribute
{
}
