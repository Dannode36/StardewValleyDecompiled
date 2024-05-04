using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Characters;

/// <summary>The content data for an NPC.</summary>
public class CharacterData
{
	/// <summary>A tokenizable string for the NPC's display name.</summary>
	public string DisplayName;

	/// <summary>The season when the NPC was born.</summary>
	[ContentSerializer(Optional = true)]
	public Season? BirthSeason;

	/// <summary>The day when the NPC was born.</summary>
	[ContentSerializer(Optional = true)]
	public int BirthDay;

	/// <summary>The region of the world in which the NPC lives (one of <c>Desert</c>, <c>Town</c>, or <c>Other</c>).</summary>
	/// <remarks>For example, only <c>Town</c> NPCs are counted for the introductions quest, can be selected as a secret santa for the Feast of the Winter Star, or get a friendship boost from the Luau.</remarks>
	[ContentSerializer(Optional = true)]
	public string HomeRegion = "Other";

	/// <summary>The language spoken by the NPC.</summary>
	[ContentSerializer(Optional = true)]
	public NpcLanguage Language;

	/// <summary>The character's gender identity.</summary>
	[ContentSerializer(Optional = true)]
	public Gender Gender = Gender.Undefined;

	/// <summary>The general age of the NPC.</summary>
	/// <remarks>This affects generated dialogue lines (e.g. a child might say "stupid" and an adult might say "depressing"), generic dialogue (e.g. a child might respond to dumpster diving with "Eww... What are you doing?" and a teen would say "Um... Why are you digging in the trash?"), and the gift they choose as Feast of the Winter Star gift-giver. Children are also excluded from item delivery quests.</remarks>
	[ContentSerializer(Optional = true)]
	public NpcAge Age;

	/// <summary>A measure of the character's general politeness.</summary>
	/// <remarks>This affects some generic dialogue lines.</remarks>
	[ContentSerializer(Optional = true)]
	public NpcManner Manner;

	/// <summary>A measure of the character's comfort with social situations.</summary>
	/// <remarks>This affects some generic dialogue lines.</remarks>
	[ContentSerializer(Optional = true)]
	public NpcSocialAnxiety SocialAnxiety = NpcSocialAnxiety.Neutral;

	/// <summary>A measure of the character's overall optimism.</summary>
	[ContentSerializer(Optional = true)]
	public NpcOptimism Optimism = NpcOptimism.Neutral;

	/// <summary>Whether the NPC has dark skin, which affects the chance of children with the player having dark skin too.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsDarkSkinned;

	/// <summary>Whether players can date and marry this NPC.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanBeRomanced;

	/// <summary>Unused.</summary>
	[ContentSerializer(Optional = true)]
	public string LoveInterest;

	/// <summary>How the NPC's birthday is shown on the calendar.</summary>
	[ContentSerializer(Optional = true)]
	public CalendarBehavior Calendar;

	/// <summary>How the NPC is shown on the social tab.</summary>
	[ContentSerializer(Optional = true)]
	public SocialTabBehavior SocialTab;

	/// <summary>A game state query which indicates whether to enable social features (like birthdays, gift giving, friendship, and an entry in the social tab). Defaults to true (except for monsters, horses, pets, and Junimos).</summary>
	[ContentSerializer(Optional = true)]
	public string CanSocialize;

	/// <summary>Whether players can give gifts to this NPC. Default true.</summary>
	/// <remarks>The NPC must also be social per <see cref="F:StardewValley.GameData.Characters.CharacterData.CanSocialize" /> and have an entry in <c>Data/NPCGiftTastes</c> to receive gifts, regardless of this value.</remarks>
	[ContentSerializer(Optional = true)]
	public bool CanReceiveGifts = true;

	/// <summary>Whether this NPC can show a speech bubble greeting nearby players or NPCs, and or be greeted by other NPCs. Default true.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanGreetNearbyCharacters = true;

	/// <summary>Whether this NPC can comment on items that a player sold to a shop which then resold it to them, or <c>null</c> to allow it if their <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" /> is <c>Town</c>.</summary>
	/// <remarks>The NPC must also be social per <see cref="F:StardewValley.GameData.Characters.CharacterData.CanSocialize" /> to allow it, regardless of this value.</remarks>
	[ContentSerializer(Optional = true)]
	public bool? CanCommentOnPurchasedShopItems;

	/// <summary>A game state query which indicates whether the NPC can visit Ginger Island once the resort is unlocked.</summary>
	[ContentSerializer(Optional = true)]
	public string CanVisitIsland;

	/// <summary>Whether to include this NPC in the introductions quest, or <c>null</c> to include them if their <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" /> is <c>Town</c>.</summary>
	[ContentSerializer(Optional = true)]
	public bool? IntroductionsQuest;

	/// <summary>A game state query which indicates whether this NPC can give item delivery quests, or <c>null</c> to allow it if their <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" /> is <c>Town</c>.</summary>
	/// <remarks>The NPC must also be social per <see cref="F:StardewValley.GameData.Characters.CharacterData.CanSocialize" /> to be included, regardless of this value.</remarks>
	[ContentSerializer(Optional = true)]
	public string ItemDeliveryQuests;

	/// <summary>Whether to include this NPC when checking whether the player has max friendships with every NPC for the perfection score.</summary>
	/// <remarks>The NPC must also be social per <see cref="F:StardewValley.GameData.Characters.CharacterData.CanSocialize" /> to be counted, regardless of this value.</remarks>
	[ContentSerializer(Optional = true)]
	public bool PerfectionScore = true;

	/// <summary>How the NPC appears in the end-game perfection slide show.</summary>
	[ContentSerializer(Optional = true)]
	public EndSlideShowBehavior EndSlideShow = EndSlideShowBehavior.MainGroup;

	/// <summary>A game state query which indicates whether the player will need to adopt children with this spouse, instead of either the player or NPC giving birth. If null, defaults to true for same-gender and false for opposite-gender spouses.</summary>
	[ContentSerializer(Optional = true)]
	public string SpouseAdopts;

	/// <summary>A game state query which indicates whether the spouse will ask to have children. Defaults to true.</summary>
	[ContentSerializer(Optional = true)]
	public string SpouseWantsChildren;

	/// <summary>A game state query which indicates whether the spouse will get jealous when the player gifts items to another NPC of the same gender when it's not their birthday. Defaults to true.</summary>
	[ContentSerializer(Optional = true)]
	public string SpouseGiftJealousy;

	/// <summary>The friendship change when <see cref="F:StardewValley.GameData.Characters.CharacterData.SpouseGiftJealousy" /> applies.</summary>
	[ContentSerializer(Optional = true)]
	public int SpouseGiftJealousyFriendshipChange = -30;

	/// <summary>The NPC's spouse room in the farmhouse when the player marries them.</summary>
	[ContentSerializer(Optional = true)]
	public CharacterSpouseRoomData SpouseRoom;

	/// <summary>The NPC's patio area on the farm when the player marries them, if any.</summary>
	[ContentSerializer(Optional = true)]
	public CharacterSpousePatioData SpousePatio;

	/// <summary>The floor IDs which the NPC might randomly apply to the farmhouse when married, or an empty list to choose from the vanilla floors.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> SpouseFloors = new List<string>();

	/// <summary>The wallpaper IDs which the NPC might randomly apply to the farmhouse when married, or an empty list to choose from the vanilla wallpapers.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> SpouseWallpapers = new List<string>();

	/// <summary>The friendship point change if this NPC sees a player rummaging through trash.</summary>
	[ContentSerializer(Optional = true)]
	public int DumpsterDiveFriendshipEffect = -25;

	/// <summary>The emote ID to show above the NPC's head when they see a player rummaging through trash.</summary>
	[ContentSerializer(Optional = true)]
	public int? DumpsterDiveEmote;

	/// <summary>The NPC's closest friends and family, where the key is the NPC name and the value is an optional tokenizable string for the name to use in dialogue text (like 'mom').</summary>
	/// <remarks>This affects generic dialogue for revealing likes and dislikes to family members, and may affect <c>inlaw_{NPC}</c> dialogues. This isn't necessarily comprehensive.</remarks>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> FriendsAndFamily = new Dictionary<string, string>();

	/// <summary>Whether the NPC can be asked to dance at the Flower Dance festival. This can be true (can be asked even if not romanceable), false (can never ask), or null (true if romanceable).</summary>
	[ContentSerializer(Optional = true)]
	public bool? FlowerDanceCanDance;

	/// <summary>At the Winter Star festival, the possible gifts this NPC can give to players.</summary>
	/// <remarks>If this doesn't return a match, a generic gift is selected based on <see cref="F:StardewValley.GameData.Characters.CharacterData.Age" />.</remarks>
	[ContentSerializer(Optional = true)]
	public List<GenericSpawnItemDataWithCondition> WinterStarGifts = new List<GenericSpawnItemDataWithCondition>();

	/// <summary>A game state query which indicates whether this NPC can give and receive gifts at the Feast of the Winter Star, or <c>null</c> to allow it if their <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" /> is <c>Town</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string WinterStarParticipant;

	/// <summary>A game state query which indicates whether the NPC should be added to the world, checked when loading a save and when ending each day. This only affects whether the NPC is added when missing; returning false won't remove an NPC that's already been added.</summary>
	[ContentSerializer(Optional = true)]
	public string UnlockConditions;

	/// <summary>Whether to add this NPC to the world automatically when they're missing and the <see cref="F:StardewValley.GameData.Characters.CharacterData.UnlockConditions" /> match.</summary>
	[ContentSerializer(Optional = true)]
	public bool SpawnIfMissing = true;

	/// <summary>The possible locations for the NPC's default map. The first matching entry is used.</summary>
	[ContentSerializer(Optional = true)]
	public List<CharacterHomeData> Home;

	/// <summary>The <strong>last segment</strong> of the NPC's portrait and sprite asset names when not set via <see cref="F:StardewValley.GameData.Characters.CharacterData.Appearance" />. For example, set to <c>"Abigail"</c> to use <c>Portraits/Abigail</c> and <c>Characters/Abigail</c> respectively. Defaults to the internal NPC name.</summary>
	[ContentSerializer(Optional = true)]
	public string TextureName;

	/// <summary>The sprite and portrait texture to use, if set.</summary>
	/// <remarks>
	///   <para>The appearances are sorted by <see cref="F:StardewValley.GameData.Characters.CharacterAppearanceData.Precedence" />, then filtered to those whose fields match. If multiple matching appearances have the highest precedence, one entry is randomly chosen based on their relative weight. This randomization is stable per day, so the NPC always makes the same choice until the next day.</para>
	///
	///   <para>If a portrait/sprite can't be loaded (or no appearances match), the NPC will use the default asset based on <see cref="F:StardewValley.GameData.Characters.CharacterData.TextureName" />.</para>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public List<CharacterAppearanceData> Appearance = new List<CharacterAppearanceData>();

	/// <summary>The pixel area in the character's sprite texture to show as their mug shot in contexts like the calendar or social menu, or <c>null</c> for the first sprite in the spritesheet.</summary>
	/// <remarks>This should be approximately 16x24 pixels for best results.</remarks>
	[ContentSerializer(Optional = true)]
	public Rectangle? MugShotSourceRect;

	/// <summary>The pixel size of the individual sprites in their world sprite spritesheet.</summary>
	[ContentSerializer(Optional = true)]
	public Point Size = new Point(16, 32);

	/// <summary>Whether the chest on the NPC's world sprite puffs in and out as they breathe.</summary>
	[ContentSerializer(Optional = true)]
	public bool Breather = true;

	/// <summary>The pixel area within the spritesheet which expands and contracts to simulate breathing, relative to the top-left corner of the source rectangle for their current sprite, or <c>null</c> to calculate it automatically.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? BreathChestRect;

	/// <summary>The pixel offset to apply to the NPC's <see cref="F:StardewValley.GameData.Characters.CharacterData.BreathChestPosition" /> when drawn over the NPC, or <c>null</c> for the default offset.</summary>
	[ContentSerializer(Optional = true)]
	public Point? BreathChestPosition;

	/// <summary>The shadow to draw, or <c>null</c> to apply the default options.</summary>
	[ContentSerializer(Optional = true)]
	public CharacterShadowData Shadow;

	/// <summary>A pixel offset to apply to the character's default emote position.</summary>
	[ContentSerializer(Optional = true)]
	public Point EmoteOffset = Point.Zero;

	/// <summary>The portrait indexes which should shake when displayed.</summary>
	[ContentSerializer(Optional = true)]
	public List<int> ShakePortraits = new List<int>();

	/// <summary>The sprite index within the <see cref="F:StardewValley.GameData.Characters.CharacterData.TextureName" /> to use when kissing a player.</summary>
	[ContentSerializer(Optional = true)]
	public int KissSpriteIndex = 28;

	/// <summary>Whether the character is facing right (true) or left (false) in their <see cref="F:StardewValley.GameData.Characters.CharacterData.KissSpriteIndex" />. The sprite will be flipped as needed to face the player.</summary>
	[ContentSerializer(Optional = true)]
	public bool KissSpriteFacingRight = true;

	/// <summary>For the hidden gift log emote, the cue ID for the sound played when clicking the sprite. Defaults to <c>drumkit6</c>.</summary>
	/// <remarks>The hidden gift log emote happens when clicking on a character's sprite in the profile menu after earning enough hearts.</remarks>
	[ContentSerializer(Optional = true)]
	public string HiddenProfileEmoteSound;

	/// <summary>For the hidden gift log emote, how long the animation plays measured in milliseconds. Defaults to 4000 (4 seconds).</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound" />.</remarks>
	[ContentSerializer(Optional = true)]
	public int HiddenProfileEmoteDuration = -1;

	/// <summary>For the hidden gift log emote, the index within the NPC's world sprite spritesheet at which the animation starts. If omitted for a vanilla NPC, the game plays a default animation specific to that NPC; if omitted for a custom NPC, the game just shows them walking while facing down.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound" />.</remarks>
	[ContentSerializer(Optional = true)]
	public int HiddenProfileEmoteStartFrame = -1;

	/// <summary>For the hidden gift log emote, the number of frames in the animation. The first frame corresponds to <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteStartFrame" />, and each subsequent frame will use the next sprite in the spritesheet. This has no effect if <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteStartFrame" /> isn't set.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound" />.</remarks>
	[ContentSerializer(Optional = true)]
	public int HiddenProfileEmoteFrameCount = 1;

	/// <summary>For the hidden gift log emote, how long each animation frame is shown on-screen before switching to the next one, measured in milliseconds. This has no effect if <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteStartFrame" /> isn't set.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.HiddenProfileEmoteSound" />.</remarks>
	[ContentSerializer(Optional = true)]
	public float HiddenProfileEmoteFrameDuration = 200f;

	/// <summary>The former NPC names which may appear in save data.</summary>
	/// <remarks>If a NPC in save data has a name which (a) matches one of these values and (b) doesn't match the name of a loaded NPC, its data will be loaded into this NPC instead. If that happens, this will also update other references like friendship and spouse data.</remarks>
	[ContentSerializer(Optional = true)]
	public List<string> FormerCharacterNames = new List<string>();

	/// <summary>The NPC's index in the <c>Maps/characterSheet</c> tilesheet, if applicable. This is used for placing vanilla NPCs in festivals from the map; custom NPCs should use the <c>{layer}_additionalCharacters</c> field in the festival data instead.</summary>
	[ContentSerializer(Optional = true)]
	public int FestivalVanillaActorIndex = -1;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
