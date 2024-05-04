namespace StardewValley.GameData.Shops;

/// <summary>How to draw stack size numbers in the shop list.</summary>
public enum StackSizeVisibility
{
	/// <summary>Always hide the stack size.</summary>
	Hide,
	/// <summary>Always draw the stack size.</summary>
	Show,
	/// <summary>Draw the stack size if more than one.</summary>
	ShowIfMultiple
}
