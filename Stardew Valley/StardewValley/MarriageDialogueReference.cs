using System;
using System.Linq;
using Netcode;

namespace StardewValley;

public class MarriageDialogueReference : INetObject<NetFields>, IEquatable<MarriageDialogueReference>
{
	public const string ENDEARMENT_TOKEN = "%endearment";

	public const string ENDEARMENT_TOKEN_LOWER = "%endearmentlower";

	private readonly NetString _dialogueFile = new NetString("");

	private readonly NetString _dialogueKey = new NetString("");

	private readonly NetBool _isGendered = new NetBool(value: false);

	private readonly NetStringList _substitutions = new NetStringList();

	public NetFields NetFields { get; } = new NetFields("MarriageDialogueReference");


	public string DialogueFile => _dialogueFile.Value;

	public string DialogueKey => _dialogueKey.Value;

	public bool IsGendered => _isGendered.Value;

	public string[] Substitutions => _substitutions.ToArray();

	public MarriageDialogueReference()
	{
		NetFields.SetOwner(this).AddField(_dialogueFile, "_dialogueFile").AddField(_dialogueKey, "_dialogueKey")
			.AddField(_isGendered, "_isGendered")
			.AddField(_substitutions, "_substitutions");
	}

	public MarriageDialogueReference(string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
		: this()
	{
		_dialogueFile.Value = dialogue_file;
		_dialogueKey.Value = dialogue_key;
		_isGendered.Value = _isGendered;
		if (substitutions.Length != 0)
		{
			_substitutions.AddRange(substitutions);
		}
	}

	public string GetText()
	{
		return "";
	}

	public bool IsItemGrabDialogue(NPC n)
	{
		return GetDialogue(n).isItemGrabDialogue();
	}

	/// <summary>Replace any tokens in the dialogue text with their localized variants.</summary>
	/// <param name="dialogue">The dialogue to modify.</param>
	/// <param name="npc">The NPC for which to replace tokens.</param>
	protected void _ReplaceTokens(Dialogue dialogue, NPC npc)
	{
		for (int i = 0; i < dialogue.dialogues.Count; i++)
		{
			dialogue.dialogues[i].Text = _ReplaceTokens(dialogue.dialogues[i].Text, npc);
		}
	}

	/// <summary>Replace any tokens in the dialogue text with their localized variants.</summary>
	/// <param name="text">The dialogue text to modify.</param>
	/// <param name="npc">The NPC for which to replace tokens.</param>
	protected string _ReplaceTokens(string text, NPC npc)
	{
		text = text.Replace("%endearmentlower", npc.getTermOfSpousalEndearment().ToLower());
		text = text.Replace("%endearment", npc.getTermOfSpousalEndearment());
		return text;
	}

	public Dialogue GetDialogue(NPC n)
	{
		if (_dialogueFile.Value.Contains("Marriage"))
		{
			Dialogue dialogue = n.tryToGetMarriageSpecificDialogue(_dialogueKey.Value) ?? new Dialogue(n, null, "");
			dialogue.removeOnNextMove = true;
			_ReplaceTokens(dialogue, n);
			return dialogue;
		}
		string key = _dialogueFile.Value + ":" + _dialogueKey.Value;
		string rawText = (_isGendered.Value ? Game1.LoadStringByGender(n.Gender, key, _substitutions) : Game1.content.LoadString(key, _substitutions));
		return new Dialogue(n, key, _ReplaceTokens(rawText, n))
		{
			removeOnNextMove = true
		};
	}

	public bool Equals(MarriageDialogueReference other)
	{
		if (object.Equals(_dialogueFile.Value, other._dialogueFile.Value) && object.Equals(_dialogueKey.Value, other._dialogueKey.Value) && object.Equals(_isGendered.Value, other._isGendered.Value))
		{
			return _substitutions.SequenceEqual(other._substitutions);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is MarriageDialogueReference dialogue)
		{
			return Equals(dialogue);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hash = 13;
		hash = hash * 7 + ((_dialogueFile.Value != null) ? _dialogueFile.Value.GetHashCode() : 0);
		hash = hash * 7 + ((_dialogueKey.Value != null) ? _dialogueFile.Value.GetHashCode() : 0);
		hash = hash * 7 + ((!_isGendered.Value) ? 1 : 0);
		foreach (string substitution in _substitutions)
		{
			hash = hash * 7 + substitution.GetHashCode();
		}
		return hash;
	}
}
