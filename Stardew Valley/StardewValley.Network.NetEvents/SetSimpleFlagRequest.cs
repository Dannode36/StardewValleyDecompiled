using System.IO;
using StardewValley.Extensions;

namespace StardewValley.Network.NetEvents;

/// <summary>A request to set or unset a simple flag for a group of players.</summary>
public sealed class SetSimpleFlagRequest : BaseSetFlagRequest
{
	/// <summary>The flag to set for the matching players.</summary>
	public SimpleFlagType FlagType { get; private set; }

	/// <inheritdoc />
	public SetSimpleFlagRequest()
	{
	}

	/// <inheritdoc cref="M:StardewValley.Network.NetEvents.BaseSetFlagRequest.#ctor(StardewValley.Network.NetEvents.PlayerActionTarget,System.String,System.Boolean,System.Nullable{System.Int64})" />
	/// <param name="flagType">The flag to set for the matching players.</param>
	/// <param name="onlyPlayerId">The specific player ID to apply this event to, or <c>null</c> to apply it to all players matching <paramref name="target" />.</param>
	public SetSimpleFlagRequest(SimpleFlagType flagType, PlayerActionTarget target, string flagId, bool flagState, long? onlyPlayerId)
		: base(target, flagId, flagState, onlyPlayerId)
	{
		FlagType = flagType;
	}

	/// <inheritdoc />
	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		FlagType = (SimpleFlagType)reader.ReadByte();
	}

	/// <inheritdoc />
	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.Write((byte)FlagType);
	}

	/// <inheritdoc />
	public override void PerformAction(Farmer farmer)
	{
		switch (FlagType)
		{
		case SimpleFlagType.ActionApplied:
			farmer.triggerActionsRun.Toggle(base.FlagId, base.FlagState);
			break;
		case SimpleFlagType.CookingRecipeKnown:
			if (base.FlagState)
			{
				farmer.cookingRecipes.TryAdd(base.FlagId, 0);
			}
			else
			{
				farmer.cookingRecipes.Remove(base.FlagId);
			}
			break;
		case SimpleFlagType.CraftingRecipeKnown:
			if (base.FlagState)
			{
				farmer.craftingRecipes.TryAdd(base.FlagId, 0);
			}
			else
			{
				farmer.craftingRecipes.Remove(base.FlagId);
			}
			break;
		case SimpleFlagType.DialogueAnswerSelected:
			farmer.dialogueQuestionsAnswered.Toggle(base.FlagId, base.FlagState);
			break;
		case SimpleFlagType.EventSeen:
			farmer.eventsSeen.Toggle(base.FlagId, base.FlagState);
			break;
		case SimpleFlagType.HasQuest:
			if (base.FlagState)
			{
				farmer.addQuest(base.FlagId);
			}
			else
			{
				farmer.removeQuest(base.FlagId);
			}
			break;
		case SimpleFlagType.SongHeard:
			farmer.songsHeard.Toggle(base.FlagId, base.FlagState);
			break;
		}
	}
}
