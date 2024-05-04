using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;

namespace StardewValley;

[NotImplicitNetField]
public class LightSource : INetObject<NetFields>
{
	public enum LightContext
	{
		None,
		MapLight,
		WindowLight
	}

	public const int lantern = 1;

	public const int windowLight = 2;

	public const int sconceLight = 4;

	public const int cauldronLight = 5;

	public const int indoorWindowLight = 6;

	public const int projectorLight = 7;

	public const int fishTankLight = 8;

	public const int townWinterTreeLight = 9;

	public const int pinpointLight = 10;

	public const int playerLantern = -85736;

	public readonly NetInt textureIndex = new NetInt().Interpolated(interpolate: false, wait: false);

	public Texture2D lightTexture;

	public readonly NetVector2 position = new NetVector2().Interpolated(interpolate: true, wait: true);

	public readonly NetColor color = new NetColor();

	public readonly NetFloat radius = new NetFloat();

	public readonly NetInt identifier = new NetInt();

	public readonly NetEnum<LightContext> lightContext = new NetEnum<LightContext>();

	public readonly NetLong playerID = new NetLong(0L).Interpolated(interpolate: false, wait: false);

	public readonly NetInt fadeOut = new NetInt(-1);

	public int Identifier
	{
		get
		{
			return identifier.Value;
		}
		set
		{
			identifier.Value = value;
		}
	}

	public long PlayerID
	{
		get
		{
			return playerID.Value;
		}
		set
		{
			playerID.Value = value;
		}
	}

	public NetFields NetFields { get; } = new NetFields("LightSource");


	public LightSource()
	{
		NetFields.SetOwner(this).AddField(textureIndex, "textureIndex").AddField(position, "position")
			.AddField(color, "color")
			.AddField(radius, "radius")
			.AddField(identifier, "identifier")
			.AddField(lightContext, "lightContext")
			.AddField(playerID, "playerID")
			.AddField(fadeOut, "fadeOut");
		textureIndex.fieldChangeEvent += delegate(NetInt field, int oldValue, int newValue)
		{
			loadTextureFromConstantValue(newValue);
		};
	}

	public LightSource(int textureIndex, Vector2 position, float radius, Color color, LightContext light_context = LightContext.None, long playerID = 0L)
		: this()
	{
		this.textureIndex.Value = textureIndex;
		this.position.Value = position;
		this.radius.Value = radius;
		this.color.Value = color;
		lightContext.Value = light_context;
		this.playerID.Value = playerID;
	}

	public LightSource(int textureIndex, Vector2 position, float radius, Color color, int identifier, LightContext light_context = LightContext.None, long playerID = 0L)
		: this()
	{
		this.textureIndex.Value = textureIndex;
		this.position.Value = position;
		this.radius.Value = radius;
		this.color.Value = color;
		this.identifier.Value = identifier;
		lightContext.Value = light_context;
		this.playerID.Value = playerID;
	}

	public LightSource(int textureIndex, Vector2 position, float radius, LightContext light_context = LightContext.None, long playerID = 0L)
		: this()
	{
		this.textureIndex.Value = textureIndex;
		this.position.Value = position;
		this.radius.Value = radius;
		color.Value = Color.Black;
		lightContext.Value = light_context;
		this.playerID.Value = playerID;
	}

	private void loadTextureFromConstantValue(int value)
	{
		switch (value)
		{
		case 1:
			lightTexture = Game1.lantern;
			break;
		case 2:
			lightTexture = Game1.windowLight;
			break;
		case 4:
			lightTexture = Game1.sconceLight;
			break;
		case 5:
			lightTexture = Game1.cauldronLight;
			break;
		case 6:
			lightTexture = Game1.indoorWindowLight;
			break;
		case 7:
			lightTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Lighting\\projectorLight");
			break;
		case 8:
			lightTexture = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\fishTankLight");
			break;
		case 9:
			lightTexture = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\treeLights");
			break;
		case 10:
			lightTexture = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\pinpointLight");
			break;
		case 3:
			break;
		}
	}

	/// <summary>Draw the light source to the screen if needed.</summary>
	/// <param name="spriteBatch">The sprite batch being drawn.</param>
	/// <param name="location">The location containing the light source.</param>
	/// <param name="lightMultiplier">A multiplier to apply to the light strength (e.g. for the darkness debuff).</param>
	public virtual void Draw(SpriteBatch spriteBatch, GameLocation location, float lightMultiplier)
	{
		NetInt netInt = fadeOut;
		if ((object)netInt != null && netInt.Value > 0)
		{
			if (color.Value.A <= 0)
			{
				return;
			}
			color.Value = new Color(color.R, color.G, color.B, color.A - fadeOut.Value);
		}
		if (lightContext.Value == LightContext.WindowLight && (Game1.IsRainingHere() || Game1.isTimeToTurnOffLighting(location)))
		{
			fadeOut.Value = 4;
		}
		if (PlayerID != 0L && PlayerID != Game1.player.UniqueMultiplayerID)
		{
			Farmer farmer = Game1.getFarmerMaybeOffline(PlayerID);
			if (farmer == null || (bool)farmer.hidden || (farmer.currentLocation != null && farmer.currentLocation.Name != Game1.currentLocation.Name))
			{
				return;
			}
		}
		if (Utility.isOnScreen(position.Value, (int)(radius.Value * 64f * 4f)))
		{
			Texture2D texture = lightTexture;
			int lightQuality = Game1.options.lightingQuality;
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, position.Value) / (lightQuality / 2), texture.Bounds, color.Value * lightMultiplier, 0f, new Vector2(texture.Bounds.Width / 2, texture.Bounds.Height / 2), radius.Value / (float)(lightQuality / 2), SpriteEffects.None, 0.9f);
		}
	}

	public LightSource Clone()
	{
		return new LightSource(textureIndex, position.Value, radius.Value, color.Value, identifier, lightContext.Value, playerID.Value);
	}
}
