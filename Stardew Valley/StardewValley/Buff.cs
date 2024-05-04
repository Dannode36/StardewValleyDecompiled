using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;

namespace StardewValley;

/// <summary>A set of effects to apply to a player or their stats.</summary>
public class Buff
{
	/// <summary>If the player is glowing, the per-tick rate at which the glow's opacity should shift between 0 (transparent) and 1 (opaque).</summary>
	public const float glowRate = 0.05f;

	/// <summary>A <see cref="F:StardewValley.Buff.millisecondsDuration" /> value which indicates it should last all day.</summary>
	public const int ENDLESS = -2;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.FarmingLevel" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int farming = 0;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.FishingLevel" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int fishing = 1;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.MiningLevel" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int mining = 2;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.LuckLevel" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int luck = 4;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.ForagingLevel" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int foraging = 5;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.MaxStamina" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int maxStamina = 7;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.MagneticRadius" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int magneticRadius = 8;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.Speed" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int speed = 9;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.Defense" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int defense = 10;

	/// <summary>The index for <see cref="F:StardewValley.Buffs.BuffEffects.Attack" /> when read from a raw array of buff effects, and its sprite index in <see cref="F:StardewValley.Game1.buffsIcons" />.</summary>
	public const int attack = 11;

	/// <summary>The unique ID for the 'burnt' debuff.</summary>
	public const string goblinsCurse = "12";

	/// <summary>The unique ID for the 'slimed' debuff.</summary>
	public const string slimed = "13";

	/// <summary>The unique ID for the 'jinxed' debuff.</summary>
	public const string evilEye = "14";

	/// <summary>The unique ID for the 'tipsy' debuff.</summary>
	public const string tipsy = "17";

	/// <summary>The unique ID for the 'fear' debuff.</summary>
	public const string fear = "18";

	/// <summary>The unique ID for the 'frozen' debuff.</summary>
	public const string frozen = "19";

	/// <summary>The unique ID for the 'warrior energy' buff.</summary>
	public const string warriorEnergy = "20";

	/// <summary>The unique ID for the 'Yoba's blessing' buff.</summary>
	public const string yobaBlessing = "21";

	/// <summary>The unique ID for the 'adrenaline rush' buff.</summary>
	public const string adrenalineRush = "22";

	/// <summary>The unique ID for the 'oil of garlic' buff.</summary>
	public const string avoidMonsters = "23";

	/// <summary>The unique ID for the 'full' debuff.</summary>
	public const string full = "6";

	/// <summary>The unique ID for the 'quenched' debuff.</summary>
	public const string quenched = "7";

	/// <summary>The unique ID for the spawn monsters debuff.</summary>
	public const string spawnMonsters = "24";

	/// <summary>The unique ID for the 'nauseated' debuff.</summary>
	public const string nauseous = "25";

	/// <summary>The unique ID for the 'darkness' debuff.</summary>
	public const string darkness = "26";

	/// <summary>The unique ID for the 'weakness' debuff.</summary>
	public const string weakness = "27";

	/// <summary>The unique ID for the 'squid ink ravioli' buff.</summary>
	public const string squidInkRavioli = "28";

	/// <summary>The default duration for the <see cref="F:StardewValley.Buff.full" /> buff.</summary>
	public const int fullnessLength = 180000;

	/// <summary>The default duration for the <see cref="F:StardewValley.Buff.quenched" /> buff.</summary>
	public const int quenchedLength = 60000;

	/// <summary>The remaining duration in milliseconds for this buff. If set to <see cref="F:StardewValley.Buff.ENDLESS" />, this buff will last all day.</summary>
	public int millisecondsDuration;

	/// <summary>The total duration in milliseconds for this buff, including both the elapsed and remaining duration.</summary>
	public int totalMillisecondsDuration;

	/// <summary>The effects to apply to the player's stats.</summary>
	public readonly BuffEffects effects = new BuffEffects();

	/// <summary>The unique ID for the applied buff, like <see cref="F:StardewValley.Buff.tipsy" />.</summary>
	public readonly string id;

	/// <summary>The translated name for this buff.</summary>
	public string displayName;

	/// <summary>The translated description for this buff.</summary>
	public string description;

	/// <summary>The internal name for how the buff was applied (like 'food'), if set.</summary>
	public string source;

	/// <summary>The translated label for how the buff was applied (like 'food'), if set.</summary>
	public string displaySource;

	/// <summary>The texture from which to get the buff icon.</summary>
	public Texture2D iconTexture;

	/// <summary>The buff icon's sprite index within the <see cref="F:StardewValley.Buff.iconTexture" />.</summary>
	public int iconSheetIndex;

	/// <summary>The color of the glow effect to show around the player, or <see cref="P:Microsoft.Xna.Framework.Color.White" /> to disable glowing.</summary>
	public Color glow;

	/// <summary>The number of milliseconds since the buff began flashing before removal, or 0 if it's not pending removal.</summary>
	public float displayAlphaTimer;

	/// <summary>Whether <see cref="F:StardewValley.Buff.displayAlphaTimer" /> was incremented for the current tick and hasn't been drawn yet.</summary>
	public bool alreadyUpdatedIconAlpha;

	/// <summary>The trigger actions to run when the buff is applied to the player.</summary>
	public string[] actionsOnApply;

	/// <summary>Whether the buff is visible in the UI. This should normally be true.</summary>
	public bool visible = true;

	/// <summary>Construct an instance.</summary>
	/// <param name="id"><inheritdoc cref="F:StardewValley.Buff.id" path="/summary" /></param>
	/// <param name="source"><inheritdoc cref="F:StardewValley.Buff.source" path="/summary" /></param>
	/// <param name="displaySource"><inheritdoc cref="F:StardewValley.Buff.displaySource" path="/summary" /></param>
	/// <param name="duration"><inheritdoc cref="F:StardewValley.Buff.millisecondsDuration" path="/summary" /></param>
	/// <param name="iconTexture"><inheritdoc cref="F:StardewValley.Buff.iconTexture" path="/summary" /></param>
	/// <param name="iconSheetIndex"><inheritdoc cref="F:StardewValley.Buff.iconSheetIndex" path="/summary" /></param>
	/// <param name="effects"><inheritdoc cref="F:StardewValley.Buff.effects" path="/summary" /></param>
	/// <param name="isDebuff">Whether this buff counts as a debuff, so its duration should be halved when wearing a Sturdy Ring.</param>
	/// <param name="displayName"><inheritdoc cref="F:StardewValley.Buff.displayName" path="/summary" /></param>
	/// <param name="description"><inheritdoc cref="F:StardewValley.Buff.description" path="/summary" /></param>
	public Buff(string id, string source = null, string displaySource = null, int duration = -1, Texture2D iconTexture = null, int iconSheetIndex = -1, BuffEffects effects = null, bool? isDebuff = false, string displayName = null, string description = null)
	{
		this.id = id;
		this.source = source;
		this.displaySource = displaySource;
		bool defaultIsDebuff = false;
		if (id != null && DataLoader.Buffs(Game1.content).TryGetValue(id, out var data))
		{
			this.displayName = TokenParser.ParseText(data.DisplayName);
			this.description = TokenParser.ParseText(data.Description);
			glow = Utility.StringToColor(data.GlowColor) ?? glow;
			millisecondsDuration = ((data.MaxDuration > 0 && data.MaxDuration > data.Duration) ? Game1.random.Next(data.Duration, data.MaxDuration + 1) : data.Duration);
			this.iconTexture = ((data.IconTexture == "TileSheets\\BuffsIcons") ? Game1.buffsIcons : Game1.content.Load<Texture2D>(data.IconTexture));
			this.iconSheetIndex = data.IconSpriteIndex;
			this.effects.Add(data.Effects);
			actionsOnApply = data.ActionsOnApply?.ToArray();
			defaultIsDebuff = data.IsDebuff;
		}
		if (duration != -1)
		{
			millisecondsDuration = duration;
		}
		if (iconTexture != null)
		{
			this.iconTexture = iconTexture;
		}
		if (iconSheetIndex != -1)
		{
			this.iconSheetIndex = iconSheetIndex;
		}
		if (displayName != null)
		{
			this.displayName = displayName;
		}
		if (description != null)
		{
			this.description = description;
		}
		if (!isDebuff.HasValue)
		{
			isDebuff = defaultIsDebuff;
		}
		if (isDebuff.Value && Game1.player.isWearingRing("525") && millisecondsDuration != -2)
		{
			millisecondsDuration /= 2;
		}
		totalMillisecondsDuration = millisecondsDuration;
		if (effects != null)
		{
			this.effects.Add(effects);
		}
	}

	/// <summary>Whether this buff changes any of the player's stats when applied.</summary>
	public bool HasAnyEffects()
	{
		return effects.HasAnyValue();
	}

	/// <summary>Get a translated label showing the number of seconds remaining before the buff expires.</summary>
	public string getTimeLeft()
	{
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.476") + millisecondsDuration / 60000 + ":" + millisecondsDuration % 60000 / 10000 + millisecondsDuration % 60000 % 10000 / 1000;
	}

	/// <summary>Update the buff state on game tick.</summary>
	/// <param name="time">The elapsed game time.</param>
	public virtual bool update(GameTime time)
	{
		if (millisecondsDuration == -2 || !Game1.shouldTimePass())
		{
			return false;
		}
		int old = millisecondsDuration;
		millisecondsDuration -= time.ElapsedGameTime.Milliseconds;
		if (id == "13" && old % 500 < millisecondsDuration % 500 && old < 3000)
		{
			Game1.multiplayer.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite(44, Game1.player.getStandingPosition() + new Vector2(-40 + Game1.random.Next(-8, 12), Game1.random.Next(-32, -16)), Color.Green * 0.5f, 8, Game1.random.NextBool(), 70f)
			{
				scale = 1f
			});
		}
		if (millisecondsDuration <= 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>Apply any logic needed when the buff is applied to a player.</summary>
	public virtual void OnAdded()
	{
		if (id == "19")
		{
			Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(118, 227, 16, 13), Game1.player.getStandingPosition() + new Vector2(-32f, -21f), flipped: false, 0f, Color.White)
			{
				layerDepth = (float)(Game1.player.StandingPixel.Y + 1) / 10000f,
				animationLength = 1,
				interval = 2000f,
				scale = 4f
			});
		}
	}

	/// <summary>Apply any logic needed when the buff is removed from a player.</summary>
	public virtual void OnRemoved()
	{
	}
}
