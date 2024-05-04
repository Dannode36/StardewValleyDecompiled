using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

namespace StardewValley.Projectiles;

public abstract class Projectile : INetObject<NetFields>, IHaveModData
{
	public const int travelTimeBeforeCollisionPossible = 100;

	public const int goblinsCurseIndex = 0;

	public const int flameBallIndex = 1;

	public const int fearBolt = 2;

	public const int shadowBall = 3;

	public const int bone = 4;

	public const int throwingKnife = 5;

	public const int snowBall = 6;

	public const int shamanBolt = 7;

	public const int frostBall = 8;

	public const int frozenBolt = 9;

	public const int fireball = 10;

	public const int slash = 11;

	public const int arrowBolt = 12;

	public const int launchedSlime = 13;

	public const int magicArrow = 14;

	public const int iceOrb = 15;

	public const string projectileSheetName = "TileSheets\\Projectiles";

	public const int timePerTailUpdate = 50;

	public readonly NetInt boundingBoxWidth = new NetInt(21);

	public static Texture2D projectileSheet;

	protected float startingAlpha = 1f;

	/// <summary>The index of the sprite to draw in <see cref="F:StardewValley.Projectiles.Projectile.projectileSheetName" />. Ignored if <see cref="F:StardewValley.Projectiles.Projectile.itemId" /> is set.</summary>
	[XmlIgnore]
	public readonly NetInt currentTileSheetIndex = new NetInt();

	/// <summary>The qualified item ID for the item to draw. If set, this overrides <see cref="F:StardewValley.Projectiles.Projectile.currentTileSheetIndex" />.</summary>
	[XmlIgnore]
	public readonly NetString itemId = new NetString();

	/// <summary>The projectile's pixel position in the world.</summary>
	[XmlIgnore]
	public readonly NetPosition position = new NetPosition();

	/// <summary>The length of the tail which trails behind the main projectile.</summary>
	[XmlIgnore]
	public readonly NetInt tailLength = new NetInt();

	[XmlIgnore]
	public int tailCounter = 50;

	/// <summary>The sound to play when the projectile bounces off a wall.</summary>
	public readonly NetString bounceSound = new NetString();

	/// <summary>The number of times the projectile can bounce off walls before being destroyed.</summary>
	[XmlIgnore]
	public readonly NetInt bouncesLeft = new NetInt();

	/// <summary>The number of times the projectile can pierce through an enemy before being destroyed.</summary>
	public readonly NetInt piercesLeft = new NetInt(1);

	public int travelTime;

	protected float? _rotation;

	[XmlIgnore]
	public float hostTimeUntilAttackable = -1f;

	public readonly NetFloat startingRotation = new NetFloat();

	/// <summary>The rotation velocity.</summary>
	[XmlIgnore]
	public readonly NetFloat rotationVelocity = new NetFloat();

	public readonly NetFloat alpha = new NetFloat(1f);

	public readonly NetFloat alphaChange = new NetFloat(0f);

	/// <summary>The speed at which the projectile moves along the X axis.</summary>
	[XmlIgnore]
	public readonly NetFloat xVelocity = new NetFloat();

	/// <summary>The speed at which the projectile moves along the Y axis.</summary>
	[XmlIgnore]
	public readonly NetFloat yVelocity = new NetFloat();

	public readonly NetVector2 acceleration = new NetVector2();

	public readonly NetFloat maxVelocity = new NetFloat(-1f);

	public readonly NetColor color = new NetColor(Color.White);

	[XmlIgnore]
	public Queue<Vector2> tail = new Queue<Vector2>();

	public readonly NetInt maxTravelDistance = new NetInt(-1);

	public float travelDistance;

	public readonly NetInt projectileID = new NetInt(-1);

	public readonly NetInt uniqueID = new NetInt(-1);

	public NetFloat height = new NetFloat(0f);

	/// <summary>Whether the projectile damage monsters (true) or players (false).</summary>
	[XmlIgnore]
	public readonly NetBool damagesMonsters = new NetBool();

	[XmlIgnore]
	public readonly NetCharacterRef theOneWhoFiredMe = new NetCharacterRef();

	public readonly NetBool ignoreTravelGracePeriod = new NetBool(value: false);

	public readonly NetBool ignoreLocationCollision = new NetBool();

	public readonly NetBool ignoreObjectCollisions = new NetBool();

	public readonly NetBool ignoreMeleeAttacks = new NetBool(value: false);

	public readonly NetBool ignoreCharacterCollisions = new NetBool(value: false);

	public bool destroyMe;

	public readonly NetFloat startingScale = new NetFloat(1f);

	protected float? _localScale;

	public readonly NetFloat scaleGrow = new NetFloat(0f);

	public NetBool light = new NetBool();

	public bool hasLit;

	[XmlIgnore]
	public int lightID;

	protected float rotation
	{
		get
		{
			if (!_rotation.HasValue)
			{
				_rotation = startingRotation.Value;
			}
			return _rotation.Value;
		}
		set
		{
			_rotation = value;
		}
	}

	public bool IgnoreLocationCollision
	{
		get
		{
			return ignoreLocationCollision;
		}
		set
		{
			ignoreLocationCollision.Value = value;
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public ModDataDictionary modData { get; } = new ModDataDictionary();


	/// <inheritdoc />
	[XmlElement("modData")]
	public ModDataDictionary modDataForSerialization
	{
		get
		{
			return modData.GetForSerialization();
		}
		set
		{
			modData.SetFromSerialization(value);
		}
	}

	public NetFields NetFields { get; } = new NetFields("Projectile");


	[XmlIgnore]
	public virtual float localScale
	{
		get
		{
			if (!_localScale.HasValue)
			{
				_localScale = startingScale.Value;
			}
			return _localScale.Value;
		}
		set
		{
			_localScale = value;
		}
	}

	/// <summary>Construct an empty instance.</summary>
	public Projectile()
	{
		InitNetFields();
		uniqueID.Value = Game1.random.Next();
	}

	/// <summary>Initialize the collection of fields to sync in multiplayer.</summary>
	protected virtual void InitNetFields()
	{
		NetFields.SetOwner(this).AddField(currentTileSheetIndex, "currentTileSheetIndex").AddField(position.NetFields, "position.NetFields")
			.AddField(tailLength, "tailLength")
			.AddField(bouncesLeft, "bouncesLeft")
			.AddField(bounceSound, "bounceSound")
			.AddField(rotationVelocity, "rotationVelocity")
			.AddField(startingRotation, "startingRotation")
			.AddField(xVelocity, "xVelocity")
			.AddField(yVelocity, "yVelocity")
			.AddField(damagesMonsters, "damagesMonsters")
			.AddField(theOneWhoFiredMe.NetFields, "theOneWhoFiredMe.NetFields")
			.AddField(ignoreLocationCollision, "ignoreLocationCollision")
			.AddField(maxTravelDistance, "maxTravelDistance")
			.AddField(ignoreTravelGracePeriod, "ignoreTravelGracePeriod")
			.AddField(ignoreMeleeAttacks, "ignoreMeleeAttacks")
			.AddField(height, "height")
			.AddField(startingScale, "startingScale")
			.AddField(scaleGrow, "scaleGrow")
			.AddField(color, "color")
			.AddField(light, "light")
			.AddField(itemId, "itemId")
			.AddField(projectileID, "projectileID")
			.AddField(ignoreObjectCollisions, "ignoreObjectCollisions")
			.AddField(acceleration, "acceleration")
			.AddField(maxVelocity, "maxVelocity")
			.AddField(alpha, "alpha")
			.AddField(alphaChange, "alphaChange")
			.AddField(boundingBoxWidth, "boundingBoxWidth")
			.AddField(ignoreCharacterCollisions, "ignoreCharacterCollisions")
			.AddField(uniqueID, "uniqueID")
			.AddField(modData, "modData");
	}

	/// <summary>Handle the projectile hitting an obstacle.</summary>
	/// <param name="location">The location containing the projectile.</param>
	/// <param name="target">The target player or monster that was hit, if applicable.</param>
	/// <param name="terrainFeature">The terrain feature that was hit, if applicable.</param>
	private void behaviorOnCollision(GameLocation location, Character target, TerrainFeature terrainFeature)
	{
		bool successfulHit = true;
		if (!(target is Farmer player))
		{
			if (target is NPC npc)
			{
				if (!npc.IsInvisible)
				{
					behaviorOnCollisionWithMonster(npc, location);
				}
				else
				{
					successfulHit = false;
				}
			}
			else if (terrainFeature != null)
			{
				behaviorOnCollisionWithTerrainFeature(terrainFeature, terrainFeature.Tile, location);
			}
			else
			{
				behaviorOnCollisionWithOther(location);
			}
		}
		else
		{
			behaviorOnCollisionWithPlayer(location, player);
		}
		if (successfulHit && piercesLeft.Value <= 0 && hasLit && Utility.getLightSource(lightID) != null)
		{
			Utility.getLightSource(lightID).fadeOut.Value = 3;
		}
	}

	public abstract void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player);

	public abstract void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location);

	public abstract void behaviorOnCollisionWithOther(GameLocation location);

	public abstract void behaviorOnCollisionWithMonster(NPC n, GameLocation location);

	public virtual bool update(GameTime time, GameLocation location)
	{
		if (Game1.isTimePaused)
		{
			return false;
		}
		if (Game1.IsMasterGame && hostTimeUntilAttackable > 0f)
		{
			hostTimeUntilAttackable -= (float)time.ElapsedGameTime.TotalSeconds;
			if (hostTimeUntilAttackable <= 0f)
			{
				ignoreMeleeAttacks.Value = false;
				hostTimeUntilAttackable = -1f;
			}
		}
		if ((bool)light)
		{
			if (!hasLit)
			{
				hasLit = true;
				lightID = Game1.random.Next(int.MinValue, int.MaxValue);
				if (location.Equals(Game1.currentLocation))
				{
					Game1.currentLightSources.Add(new LightSource(4, position.Value + new Vector2(32f, 32f), 1f, new Color(Utility.getOppositeColor(color.Value).ToVector4() * alpha.Value), lightID, LightSource.LightContext.None, 0L));
				}
			}
			else
			{
				LightSource l = Utility.getLightSource(lightID);
				if (l != null)
				{
					l.color.A = (byte)(255f * alpha.Value);
				}
				Utility.repositionLightSource(lightID, position.Value + new Vector2(32f, 32f));
			}
		}
		alpha.Value += alphaChange.Value;
		alpha.Value = Utility.Clamp(alpha.Value, 0f, 1f);
		rotation += rotationVelocity.Value;
		travelTime += time.ElapsedGameTime.Milliseconds;
		if (scaleGrow.Value != 0f)
		{
			localScale += scaleGrow.Value;
		}
		Vector2 old_position = position.Value;
		updatePosition(time);
		updateTail(time);
		travelDistance += (old_position - position.Value).Length();
		if (maxTravelDistance.Value >= 0)
		{
			if (travelDistance > (float)((int)maxTravelDistance - 128))
			{
				alpha.Value = ((float)(int)maxTravelDistance - travelDistance) / 128f;
			}
			if (travelDistance >= (float)(int)maxTravelDistance)
			{
				if (hasLit)
				{
					Utility.removeLightSource(lightID);
				}
				return true;
			}
		}
		if ((travelTime > 100 || ignoreTravelGracePeriod.Value) && isColliding(location, out var target, out var terrainFeature) && ShouldApplyCollisionLocally(location))
		{
			if ((int)bouncesLeft <= 0 || target != null)
			{
				behaviorOnCollision(location, target, terrainFeature);
				return piercesLeft.Value <= 0;
			}
			bouncesLeft.Value--;
			bool[] array = Utility.horizontalOrVerticalCollisionDirections(getBoundingBox(), theOneWhoFiredMe.Get(location), projectile: true);
			if (array[0])
			{
				xVelocity.Value = 0f - xVelocity.Value;
			}
			if (array[1])
			{
				yVelocity.Value = 0f - yVelocity.Value;
			}
			if (!string.IsNullOrEmpty(bounceSound.Value))
			{
				location?.playSound(bounceSound.Value);
			}
		}
		return false;
	}

	/// <summary>Get whether this projectile's <see cref="M:StardewValley.Projectiles.Projectile.behaviorOnCollision(StardewValley.GameLocation,StardewValley.Character,StardewValley.TerrainFeatures.TerrainFeature)" /> should be called for the local player.</summary>
	/// <param name="location">The location containing the projectile.</param>
	protected virtual bool ShouldApplyCollisionLocally(GameLocation location)
	{
		if (theOneWhoFiredMe.Get(location) is Farmer firedBy && firedBy != Game1.player)
		{
			if (Game1.IsMasterGame)
			{
				return firedBy.currentLocation != location;
			}
			return false;
		}
		return true;
	}

	protected virtual void updateTail(GameTime time)
	{
		tailCounter -= time.ElapsedGameTime.Milliseconds;
		if (tailCounter <= 0)
		{
			tailCounter = 50;
			tail.Enqueue(position.Value);
			if (tail.Count > (int)tailLength)
			{
				tail.Dequeue();
			}
		}
	}

	/// <summary>Get whether the projectile is colliding with a wall or target.</summary>
	/// <param name="location">The location containing the projectile.</param>
	/// <param name="target">The target that was hit, if applicable.</param>
	/// <param name="terrainFeature">The terrain feature that was hit, if applicable.</param>
	public virtual bool isColliding(GameLocation location, out Character target, out TerrainFeature terrainFeature)
	{
		target = null;
		terrainFeature = null;
		Rectangle boundingBox = getBoundingBox();
		if (!ignoreCharacterCollisions)
		{
			if (damagesMonsters.Value)
			{
				Character npc = location.doesPositionCollideWithCharacter(boundingBox);
				if (npc != null)
				{
					if (npc is NPC && (npc as NPC).IsInvisible)
					{
						return false;
					}
					target = npc;
					return true;
				}
			}
			else if (Game1.player.currentLocation == location && Game1.player.GetBoundingBox().Intersects(boundingBox))
			{
				target = Game1.player;
				return true;
			}
		}
		foreach (Vector2 tile in Utility.getListOfTileLocationsForBordersOfNonTileRectangle(boundingBox))
		{
			if (location.terrainFeatures.TryGetValue(tile, out var feature) && !feature.isPassable())
			{
				terrainFeature = feature;
				return true;
			}
		}
		if (!location.isTileOnMap(position.Value / 64f) || (!ignoreLocationCollision && location.isCollidingPosition(boundingBox, Game1.viewport, isFarmer: false, 0, glider: true, theOneWhoFiredMe.Get(location), pathfinding: false, projectile: true)))
		{
			return true;
		}
		return false;
	}

	public abstract void updatePosition(GameTime time);

	public virtual Rectangle getBoundingBox()
	{
		Vector2 pos = position.Value;
		int damageSize = (int)boundingBoxWidth + (damagesMonsters ? 8 : 0);
		float current_scale = localScale;
		damageSize = (int)((float)damageSize * current_scale);
		return new Rectangle((int)pos.X + 32 - damageSize / 2, (int)pos.Y + 32 - damageSize / 2, damageSize, damageSize);
	}

	public virtual void draw(SpriteBatch b)
	{
		float current_scale = 4f * localScale;
		Texture2D texture = GetTexture();
		Rectangle sourceRect = GetSourceRect();
		Vector2 pixelPosition = position.Value;
		b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(0f, 0f - height.Value) + new Vector2(32f, 32f)), sourceRect, color.Value * alpha.Value, rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (pixelPosition.Y + 96f) / 10000f);
		if (height.Value > 0f)
		{
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(32f, 32f)), Game1.shadowTexture.Bounds, Color.White * alpha.Value * 0.75f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 2f, SpriteEffects.None, (pixelPosition.Y - 1f) / 10000f);
		}
		float tailAlpha = alpha.Value;
		for (int i = tail.Count - 1; i >= 0; i--)
		{
			b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp((i == tail.Count - 1) ? pixelPosition : tail.ElementAt(i + 1), tail.ElementAt(i), (float)tailCounter / 50f) + new Vector2(0f, 0f - height.Value) + new Vector2(32f, 32f)), sourceRect, color.Value * tailAlpha, rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (pixelPosition.Y - (float)(tail.Count - i) + 96f) / 10000f);
			tailAlpha -= 1f / (float)tail.Count;
			current_scale = 0.8f * (float)(4 - 4 / (i + 4));
		}
	}

	/// <summary>Get the texture to draw for the projectile.</summary>
	public Texture2D GetTexture()
	{
		if (itemId.Value == null)
		{
			return projectileSheet;
		}
		return ItemRegistry.GetDataOrErrorItem(itemId.Value).GetTexture();
	}

	/// <summary>Get the source rectangle to draw for the projectile.</summary>
	public Rectangle GetSourceRect()
	{
		if (itemId.Value == null)
		{
			return Game1.getSourceRectForStandardTileSheet(projectileSheet, currentTileSheetIndex, 16, 16);
		}
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId.Value);
		switch (itemId.Value)
		{
		case "(O)388":
		case "(O)390":
		case "(O)378":
		case "(O)380":
		case "(O)384":
		case "(O)382":
		case "(O)386":
			return data.GetSourceRect(1);
		default:
			return data.GetSourceRect();
		}
	}
}
