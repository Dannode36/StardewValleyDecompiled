using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Pets;

/// <summary>The metadata for a pet type that can be selected by the player.</summary>
public class PetData
{
	/// <summary>A tokenizable string for the pet type's display name (like "cat"), which can be used in dialogue.</summary>
	public string DisplayName;

	/// <summary>The cue ID for the pet's occasional 'bark' sound.</summary>
	public string BarkSound;

	/// <summary>The cue ID for the sound which the pet makes when you pet it.</summary>
	public string ContentSound;

	/// <summary>The number of milliseconds until the ContentSound is repeated once. This is used by the dog, who pants twice when pet. Defaults to disabled.</summary>
	[ContentSerializer(Optional = true)]
	public int RepeatContentSoundAfter = -1;

	/// <summary>A pixel offset to apply to the emote position over the pet sprite.</summary>
	[ContentSerializer(Optional = true)]
	public Point EmoteOffset;

	/// <summary>The pixel offset for the pet when shown in events like Marnie's adoption event.</summary>
	[ContentSerializer(Optional = true)]
	public Point EventOffset;

	/// <summary>The location containing the event which lets the player adopt this pet, if they've selected it as their preferred type.</summary>
	[ContentSerializer(Optional = true)]
	public string AdoptionEventLocation = "Farm";

	/// <summary>The event ID in the <see cref="F:StardewValley.GameData.Pets.PetData.AdoptionEventLocation" /> which lets the player adopt this pet, if they've selected it as their preferred type.</summary>
	/// <remarks>If set, this forces the event to play after 20 days if the event's preconditions haven't been met yet.</remarks>
	[ContentSerializer(Optional = true)]
	public string AdoptionEventId;

	/// <summary>How to render the pet during the summit perfection slide-show.</summary>
	/// <remarks>If this isn't set, the pet won't be shown in the slide-show.</remarks>
	public PetSummitPerfectionEventData SummitPerfectionEvent;

	/// <summary>How quickly the pet can move.</summary>
	[ContentSerializer(Optional = true)]
	public int MoveSpeed = 2;

	/// <summary>The percentage chance that the pet sleeps on the player's bed at night, as a decimal value between 0 (never) and 1 (always).</summary>
	/// <remarks>The chances are checked in this order: <see cref="F:StardewValley.GameData.Pets.PetData.SleepOnBedChance" />, <see cref="F:StardewValley.GameData.Pets.PetData.SleepNearBedChance" />, and <see cref="F:StardewValley.GameData.Pets.PetData.SleepOnRugChance" />. The first match is used. If none match, the pet will choose a random empty spot in the farmhouse; if there's no empty spot, it'll sleep next to its pet bowl outside.</remarks>
	[ContentSerializer(Optional = true)]
	public float SleepOnBedChance = 0.05f;

	/// <summary>The percentage chance that the pet sleeps at the foot of the player's bed at night, as a decimal value between 0 (never) and 1 (always).</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetData.SleepOnBedChance" />.</remarks>
	[ContentSerializer(Optional = true)]
	public float SleepNearBedChance = 0.3f;

	/// <summary>The percentage chance that the pet sleeps on a random rug at night, as a decimal value between 0 (never) and 1 (always).</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetData.SleepOnBedChance" />.</remarks>
	[ContentSerializer(Optional = true)]
	public float SleepOnRugChance = 0.5f;

	/// <summary>The pet's possible actions and behaviors, defined as the states in a state machine. Essentially the pet will be in one state at any given time, which also determines which state they can transition to next. For example, a cat can transition from <c>Walk </c>to <c>BeginSitDown</c>, but it can't skip instantly from <c>Walk</c> to <c>SitDownLick</c>.</summary>
	public List<PetBehavior> Behaviors;

	/// <summary>The percentage chance that the pet will try to give a gift when pet each day.</summary>
	[ContentSerializer(Optional = true)]
	public float GiftChance = 0.2f;

	/// <summary>The list of gifts that this pet can give if the gift chance roll is successful, chosen by weight similar to the pet behaviors.</summary>
	[ContentSerializer(Optional = true)]
	public List<PetGift> Gifts = new List<PetGift>();

	/// <summary>The cosmetic breeds which can be selected in the character customization menu when creating a save.</summary>
	public List<PetBreed> Breeds;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;

	/// <summary>Get the breed from <see cref="F:StardewValley.GameData.Pets.PetData.Breeds" /> to use for a given ID.</summary>
	/// <param name="breedId">The preferred pet breed ID.</param>
	/// <param name="allowNull">Whether to return null if the ID isn't found. If false, default to the first breed in the list instead.</param>
	public PetBreed GetBreedById(string breedId, bool allowNull = false)
	{
		foreach (PetBreed breed in Breeds)
		{
			if (breed.Id == breedId)
			{
				return breed;
			}
		}
		if (!allowNull)
		{
			return Breeds[0];
		}
		return null;
	}
}
