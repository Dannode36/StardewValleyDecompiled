using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FarmAnimals;

/// <summary>The metadata for a farm animal which can be bought from Marnie's ranch.</summary>
public class FarmAnimalData
{
	/// <summary>A tokenizable string for the animal type's display name.</summary>
	[ContentSerializer(Optional = true)]
	public string DisplayName;

	/// <summary>The ID for the main building type that houses this animal. The animal will also be placeable in buildings whose <see cref="F:StardewValley.GameData.Buildings.BuildingData.ValidOccupantTypes" /> field contains this value.</summary>
	[ContentSerializer(Optional = true)]
	public string House;

	/// <summary>The default gender for the animal type. This only affects the text shown after purchasing the animal.</summary>
	[ContentSerializer(Optional = true)]
	public FarmAnimalGender Gender;

	/// <summary>Half the cost to purchase the animal (the actual price is double this value), or a negative value to disable purchasing this animal type. Default -1.</summary>
	[ContentSerializer(Optional = true)]
	public int PurchasePrice = -1;

	/// <summary>The price when the player sells the animal, before it's adjusted for the animal's friendship towards the player.</summary>
	/// <remarks>The actual sell price will be this value multiplied by a number between 0.3 (zero friendship) and 1.3 (max friendship).</remarks>
	[ContentSerializer(Optional = true)]
	public int SellPrice;

	/// <summary>The asset name for the icon texture to show in shops.</summary>
	[ContentSerializer(Optional = true)]
	public string ShopTexture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShopTexture" /> to draw. This should be 32 pixels wide and 16 high. Ignored if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShopTexture" /> isn't set.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle ShopSourceRect;

	/// <summary>A tokenizable string for the display name shown in the shop menu. Defaults to the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DisplayName" /> field.</summary>
	[ContentSerializer(Optional = true)]
	public string ShopDisplayName;

	/// <summary>A tokenizable string for the tooltip description shown in the shop menu. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public string ShopDescription;

	/// <summary>A tokenizable string which overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShopDescription" /> if the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.RequiredBuilding" /> isn't built. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public string ShopMissingBuildingDescription;

	/// <summary>The building that needs to be built on the farm for this animal to be available to purchase. Buildings that are upgraded from this building are valid too. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public string RequiredBuilding;

	/// <summary>A game state query which indicates whether the farm animal is available in the shop menu. Default always unlocked.</summary>
	[ContentSerializer(Optional = true)]
	public string UnlockCondition;

	/// <summary>The possible variants for this farm animal (e.g. chickens can be Brown Chicken, Blue Chicken, or White Chicken). When the animal is purchased, of the available variants is chosen at random.</summary>
	[ContentSerializer(Optional = true)]
	public List<AlternatePurchaseAnimals> AlternatePurchaseTypes;

	/// <summary>A list of the object IDs that can be placed in the incubator or ostrich incubator to hatch this animal. If <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.House" /> doesn't match the current building, the entry will be ignored. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> EggItemIds;

	/// <summary>How long eggs incubate before they hatch, in in-game minutes. Defaults to 9000 minutes.</summary>
	[ContentSerializer(Optional = true)]
	public int IncubationTime = -1;

	/// <summary>An offset applied to the incubator's sprite index when it's holding an egg for this animal.</summary>
	[ContentSerializer(Optional = true)]
	public int IncubatorParentSheetOffset = 1;

	/// <summary>A tokenizable string for the message shown when entering the building after the egg hatched. Defaults to the text "???".</summary>
	[ContentSerializer(Optional = true)]
	public string BirthText;

	/// <summary>The number of days until a freshly purchased/born animal becomes an adult and begins producing items.</summary>
	[ContentSerializer(Optional = true)]
	public int DaysToMature = 1;

	/// <summary>Whether an animal can produce a child (regardless of gender).</summary>
	[ContentSerializer(Optional = true)]
	public bool CanGetPregnant;

	/// <summary>The number of days between item productions. For example, setting 1 will produce an item every other day.</summary>
	[ContentSerializer(Optional = true)]
	public int DaysToProduce = 1;

	/// <summary>How produced items are collected from the animal.</summary>
	[ContentSerializer(Optional = true)]
	public FarmAnimalHarvestType HarvestType;

	/// <summary>The tool ID with which produced items can be collected from the animal, if the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestType" /> is set to <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalHarvestType.HarvestWithTool" />. The values recognized by the vanilla tools are <c>MilkPail</c> and <c>Shears</c>. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public string HarvestTool;

	/// <summary>The items produced by the animal when it's an adult, if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceMinimumFriendship" /> does not match.</summary>
	[ContentSerializer(Optional = true)]
	public List<FarmAnimalProduce> ProduceItemIds = new List<FarmAnimalProduce>();

	/// <summary>The items produced by the animal when it's an adult, if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceMinimumFriendship" /> matches.</summary>
	[ContentSerializer(Optional = true)]
	public List<FarmAnimalProduce> DeluxeProduceItemIds = new List<FarmAnimalProduce>();

	/// <summary>Whether an item is produced on the day the animal becomes an adult (like sheep).</summary>
	[ContentSerializer(Optional = true)]
	public bool ProduceOnMature;

	/// <summary>The minimum friendship points needed to reduce the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DaysToProduce" /> by one. Defaults to no reduction.</summary>
	[ContentSerializer(Optional = true)]
	public int FriendshipForFasterProduce = -1;

	/// <summary>The minimum friendship points needed to produce the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceItemIds" />.</summary>
	[ContentSerializer(Optional = true)]
	public int DeluxeProduceMinimumFriendship = 200;

	/// <summary>A divisor which reduces the probability of producing <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceItemIds" />. Lower values produce deluxe items more often.</summary>
	/// <remarks>
	///   This is applied using this formula:
	///   <code>
	///     if happiness &gt; 200:
	///         happiness_modifier = happiness * 1.5
	///     else if happiness &gt; 100:
	///         happiness_modifier = 0
	///     else
	///         happiness_modifier = happiness - 100
	///
	///     ((friendship + happiness_modifier) / DeluxeProduceCareDivisor) + (daily_luck * DeluxeProduceLuckMultiplier)
	///   </code>
	///
	///   For example, given a friendship of 102 and happiness of 150, the probability with the default field values will be <c>((102 + 0) / 1200) + (daily_luck * 0) = (102 / 1200) = 0.085</c> or 8.5%.
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public float DeluxeProduceCareDivisor = 1200f;

	/// <summary>A multiplier which increases the bonus from daily luck on the probability of producing <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceItemIds" />.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DeluxeProduceCareDivisor" />.</remarks>
	[ContentSerializer(Optional = true)]
	public float DeluxeProduceLuckMultiplier;

	/// <summary>Whether players can feed this animal a golden cracker to double its normal output.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanEatGoldenCrackers = true;

	/// <summary>The internal ID of a profession which makes it easier to befriend this animal. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public int ProfessionForHappinessBoost = -1;

	/// <summary>The internal ID of a profession which increases the chance of higher-quality produce.</summary>
	[ContentSerializer(Optional = true)]
	public int ProfessionForQualityBoost = -1;

	/// <summary>The internal ID of a profession which reduces the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.DaysToProduce" /> by one. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public int ProfessionForFasterProduce = -1;

	/// <summary>The audio cue ID for the sound produced by the animal (e.g. when pet). Default none.</summary>
	[ContentSerializer(Optional = true)]
	public string Sound;

	/// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Sound" /> when the animal is a baby. Has no effect if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Sound" /> isn't set.</summary>
	[ContentSerializer(Optional = true)]
	public string BabySound;

	/// <summary>If set, the asset name for the animal's spritesheet. Defaults to <c>Animals/{ID}</c>, like Animals/Goat for a goat.</summary>
	public string Texture;

	/// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture" /> when the animal doesn't currently have an item ready to collect (like the sheep's sheared sprite).</summary>
	[ContentSerializer(Optional = true)]
	public string HarvestedTexture;

	/// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture" /> and <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestedTexture" /> when the animal is a baby.</summary>
	[ContentSerializer(Optional = true)]
	public string BabyTexture;

	/// <summary>When the animal is facing left, whether to use a flipped version of their right-facing sprite.</summary>
	[ContentSerializer(Optional = true)]
	public bool UseFlippedRightForLeft;

	/// <summary>The pixel width of the animal's sprite (before in-game pixel zoom is applied).</summary>
	[ContentSerializer(Optional = true)]
	public int SpriteWidth = 16;

	/// <summary>The pixel height of the animal's sprite (before in-game pixel zoom is applied).</summary>
	[ContentSerializer(Optional = true)]
	public int SpriteHeight = 16;

	/// <summary>Whether the animal has two frames for the randomized 'unique' animation instead of one.</summary>
	/// <remarks>
	///   <para>If false, the unique sprite frames are indexes 13 (down), 14 (right), 12 (left if <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.UseFlippedRightForLeft" /> is false), and 15 (up).</para>
	///
	///   <para>If true, the unique sprite frames are indexes 16 (down), 18 (right), 22 (left), and 20 (up).</para>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public bool UseDoubleUniqueAnimationFrames;

	/// <summary>The sprite index to display when sleeping.</summary>
	[ContentSerializer(Optional = true)]
	public int SleepFrame = 12;

	/// <summary>A pixel offset to apply to emotes drawn over the farm animal.</summary>
	[ContentSerializer(Optional = true)]
	public Point EmoteOffset = Point.Zero;

	/// <summary>A pixel offset to apply to the farm animal's sprite while it's swimming.</summary>
	[ContentSerializer(Optional = true)]
	public Point SwimOffset = new Point(0, 112);

	/// <summary>The possible alternate appearances, if any. A skin is chosen at random when the animal is purchased or hatched based on the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalSkin.Weight" /> field. The default appearance (e.g. using <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture" />) is automatically an available skin with a weight of 1.</summary>
	[ContentSerializer(Optional = true)]
	public List<FarmAnimalSkin> Skins;

	/// <summary>The shadow to draw when a baby animal is swimming, or <c>null</c> to apply <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShadowWhenBaby" />.</summary>
	[ContentSerializer(Optional = true)]
	public FarmAnimalShadowData ShadowWhenBabySwims;

	/// <summary>The shadow to draw for a baby animal, or <c>null</c> to apply the default options.</summary>
	[ContentSerializer(Optional = true)]
	public FarmAnimalShadowData ShadowWhenBaby;

	/// <summary>The shadow to draw when an adult animal is swimming, or <c>null</c> to apply <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.ShadowWhenAdult" />.</summary>
	[ContentSerializer(Optional = true)]
	public FarmAnimalShadowData ShadowWhenAdultSwims;

	/// <summary>The shadow to draw for an adult animal, or <c>null</c> to apply the default options.</summary>
	[ContentSerializer(Optional = true)]
	public FarmAnimalShadowData ShadowWhenAdult;

	/// <summary>Whether animals on the farm can swim in water once they've been pet. Default false.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanSwim;

	/// <summary>Whether baby animals can follow nearby adults. This only applies for animals whose <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.House" /> field is <c>Coop</c>. Default false.</summary>
	[ContentSerializer(Optional = true)]
	public bool BabiesFollowAdults;

	/// <summary>The amount of grass eaten by this animal each day.</summary>
	[ContentSerializer(Optional = true)]
	public int GrassEatAmount = 2;

	/// <summary>An amount which affects the daily reduction in happiness if the animal wasn't pet, or didn't have a heater in winter.</summary>
	[ContentSerializer(Optional = true)]
	public int HappinessDrain;

	/// <summary>The animal sprite's tile size in the world when the player is clicking to pet them, if the animal is facing up or down. This can be a fractional value like 1.75.</summary>
	[ContentSerializer(Optional = true)]
	public Vector2 UpDownPetHitboxTileSize = new Vector2(1f, 1f);

	/// <summary>The animal sprite's tile size in the world when the player is clicking to pet them, if the animal is facing left or right. This can be a fractional value like 1.75.</summary>
	[ContentSerializer(Optional = true)]
	public Vector2 LeftRightPetHitboxTileSize = new Vector2(1f, 1f);

	/// <summary>Overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.UpDownPetHitboxTileSize" /> when the animal is a baby.</summary>
	[ContentSerializer(Optional = true)]
	public Vector2 BabyUpDownPetHitboxTileSize = new Vector2(1f, 1f);

	/// <summary>Overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.LeftRightPetHitboxTileSize" /> when the animal is a baby.</summary>
	[ContentSerializer(Optional = true)]
	public Vector2 BabyLeftRightPetHitboxTileSize = new Vector2(1f, 1f);

	/// <summary>The game stat counters to increment when the animal produces an item, if any.</summary>
	[ContentSerializer(Optional = true)]
	public List<StatIncrement> StatToIncrementOnProduce;

	/// <summary>Whether to show the farm animal in the credit scene on the summit after the player achieves perfection.</summary>
	[ContentSerializer(Optional = true)]
	public bool ShowInSummitCredits;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;

	/// <summary>Get the options to apply when drawing the animal's shadow, if any.</summary>
	/// <param name="isBaby">Whether the animal is a baby.</param>
	/// <param name="isSwimming">Whether the animal is swimming.</param>
	public FarmAnimalShadowData GetShadow(bool isBaby, bool isSwimming)
	{
		if (isBaby)
		{
			if (!isSwimming)
			{
				return ShadowWhenBaby;
			}
			return ShadowWhenBabySwims ?? ShadowWhenBaby;
		}
		if (!isSwimming)
		{
			return ShadowWhenAdult;
		}
		return ShadowWhenAdultSwims ?? ShadowWhenAdult;
	}
}
