using System;

namespace StardewValley;

/// <summary>As part of <see cref="T:StardewValley.Dialogue" />, a bit of dialogue shown in its own message box or an action to run when it's selected.</summary>
public class DialogueLine
{
	/// <summary>The text to display, or <see cref="F:System.String.Empty" /> to skip displaying text.</summary>
	public string Text;

	/// <summary>The action to perform when the dialogue is displayed.</summary>
	public Action SideEffects;

	/// <summary>Whether this entry has dialogue text to display.</summary>
	public bool HasText => !string.IsNullOrEmpty(Text);

	/// <summary>Construct an instance.</summary>
	/// <param name="text">The text to display, or <see cref="F:System.String.Empty" /> to skip displaying text.</param>
	/// <param name="sideEffects">The action to perform when the dialogue is displayed.</param>
	public DialogueLine(string text, Action sideEffects = null)
	{
		Text = text ?? "";
		SideEffects = sideEffects;
	}
}
