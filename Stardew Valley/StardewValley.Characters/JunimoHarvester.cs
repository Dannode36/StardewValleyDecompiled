using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;

namespace StardewValley.Characters;

public class JunimoHarvester : NPC
{
	protected float alpha = 1f;

	protected float alphaChange;

	protected Vector2 motion = Vector2.Zero;

	protected new Rectangle nextPosition;

	protected readonly NetColor color = new NetColor();

	protected bool destroy;

	protected Item lastItemHarvested;

	public int whichJunimoFromThisHut;

	protected int harvestTimer;

	public readonly NetBool isPrismatic = new NetBool(value: false);

	protected readonly NetGuid netHome = new NetGuid();

	protected readonly NetEvent1Field<int, NetInt> netAnimationEvent = new NetEvent1Field<int, NetInt>();

	public Guid HomeId
	{
		get
		{
			return netHome.Value;
		}
		set
		{
			netHome.Value = value;
		}
	}

	[XmlIgnore]
	public JunimoHut home
	{
		get
		{
			if (!base.currentLocation.buildings.TryGetValue(netHome.Value, out var building))
			{
				return null;
			}
			return building as JunimoHut;
		}
		set
		{
			netHome.Value = base.currentLocation.buildings.GuidOf(value);
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public JunimoHarvester()
	{
	}

	public JunimoHarvester(GameLocation location, Vector2 position, JunimoHut hut, int whichJunimoNumberFromThisHut, Color? c)
		: base(new AnimatedSprite("Characters\\Junimo", 0, 16, 16), position, 2, "Junimo")
	{
		base.currentLocation = location;
		home = hut;
		whichJunimoFromThisHut = whichJunimoNumberFromThisHut;
		if (!c.HasValue)
		{
			pickColor();
		}
		else
		{
			color.Value = c.Value;
		}
		nextPosition = GetBoundingBox();
		base.Breather = false;
		base.speed = 3;
		forceUpdateTimer = 9999;
		collidesWithOtherCharacters.Value = true;
		ignoreMovementAnimation = true;
		farmerPassesThrough = true;
		base.Scale = 0.75f;
		base.willDestroyObjectsUnderfoot = false;
		Vector2 tileToPathfindTo = Vector2.Zero;
		switch (whichJunimoNumberFromThisHut)
		{
		case 0:
			tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((int)hut.tileX + 1, (int)hut.tileY + (int)hut.tilesHigh + 1), 30);
			break;
		case 1:
			tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((int)hut.tileX - 1, (int)hut.tileY), 30);
			break;
		case 2:
			tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((int)hut.tileX + (int)hut.tilesWide, (int)hut.tileY), 30);
			break;
		}
		if (tileToPathfindTo != Vector2.Zero)
		{
			controller = new PathFindController(this, base.currentLocation, Utility.Vector2ToPoint(tileToPathfindTo), -1, reachFirstDestinationFromHut, 100);
		}
		if ((controller == null || controller.pathToEndPoint == null) && Game1.IsMasterGame)
		{
			pathfindToRandomSpotAroundHut();
			if (controller?.pathToEndPoint == null)
			{
				destroy = true;
			}
		}
		collidesWithOtherCharacters.Value = false;
	}

	protected virtual void pickColor()
	{
		JunimoHut hut = home;
		if (hut == null)
		{
			color.Value = Color.White;
			return;
		}
		Random r = Utility.CreateRandom((int)hut.tileX, (double)(int)hut.tileY * 777.0, whichJunimoFromThisHut);
		if (r.NextBool(0.25))
		{
			if (r.NextBool(0.01))
			{
				color.Value = Color.White;
				return;
			}
			switch (r.Next(8))
			{
			case 0:
				color.Value = Color.Red;
				break;
			case 1:
				color.Value = Color.Goldenrod;
				break;
			case 2:
				color.Value = Color.Yellow;
				break;
			case 3:
				color.Value = Color.Lime;
				break;
			case 4:
				color.Value = new Color(0, 255, 180);
				break;
			case 5:
				color.Value = new Color(0, 100, 255);
				break;
			case 6:
				color.Value = Color.MediumPurple;
				break;
			default:
				color.Value = Color.Salmon;
				break;
			}
		}
		else
		{
			switch (r.Next(8))
			{
			case 0:
				color.Value = Color.LimeGreen;
				break;
			case 1:
				color.Value = Color.Orange;
				break;
			case 2:
				color.Value = Color.LightGreen;
				break;
			case 3:
				color.Value = Color.Tan;
				break;
			case 4:
				color.Value = Color.GreenYellow;
				break;
			case 5:
				color.Value = Color.LawnGreen;
				break;
			case 6:
				color.Value = Color.PaleGreen;
				break;
			default:
				color.Value = Color.Turquoise;
				break;
			}
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(color, "color").AddField(netHome.NetFields, "netHome.NetFields").AddField(netAnimationEvent, "netAnimationEvent")
			.AddField(isPrismatic, "isPrismatic");
		netAnimationEvent.onEvent += doAnimationEvent;
	}

	/// <inheritdoc />
	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
		if (Sprite == null)
		{
			Sprite = new AnimatedSprite(content ?? Game1.content, "Characters\\Junimo");
		}
	}

	protected virtual void doAnimationEvent(int animId)
	{
		switch (animId)
		{
		case 0:
			Sprite.CurrentAnimation = null;
			break;
		case 2:
			Sprite.currentFrame = 0;
			break;
		case 3:
			Sprite.currentFrame = 1;
			break;
		case 4:
			Sprite.currentFrame = 2;
			break;
		case 5:
			Sprite.currentFrame = 44;
			break;
		case 6:
			Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(12, 200),
				new FarmerSprite.AnimationFrame(13, 200),
				new FarmerSprite.AnimationFrame(14, 200),
				new FarmerSprite.AnimationFrame(15, 200)
			});
			break;
		case 7:
			Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(44, 200),
				new FarmerSprite.AnimationFrame(45, 200),
				new FarmerSprite.AnimationFrame(46, 200),
				new FarmerSprite.AnimationFrame(47, 200)
			});
			break;
		case 8:
			Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(28, 100),
				new FarmerSprite.AnimationFrame(29, 100),
				new FarmerSprite.AnimationFrame(30, 100),
				new FarmerSprite.AnimationFrame(31, 100)
			});
			break;
		case 1:
			break;
		}
	}

	public virtual void reachFirstDestinationFromHut(Character c, GameLocation l)
	{
		tryToHarvestHere();
	}

	public virtual void tryToHarvestHere()
	{
		if (base.currentLocation != null)
		{
			if (isHarvestable())
			{
				harvestTimer = 2000;
			}
			else
			{
				pokeToHarvest();
			}
		}
	}

	public virtual void pokeToHarvest()
	{
		JunimoHut hut = home;
		if (hut != null)
		{
			if (!hut.isTilePassable(base.Tile) && Game1.IsMasterGame)
			{
				destroy = true;
			}
			else if (harvestTimer <= 0 && Game1.random.NextDouble() < 0.7)
			{
				pathfindToNewCrop();
			}
		}
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		return true;
	}

	public void setMoving(int xSpeed, int ySpeed)
	{
		motion.X = xSpeed;
		motion.Y = ySpeed;
	}

	public void setMoving(Vector2 motion)
	{
		this.motion = motion;
	}

	public override void Halt()
	{
		base.Halt();
		motion = Vector2.Zero;
	}

	public override bool canTalk()
	{
		return false;
	}

	public void junimoReachedHut(Character c, GameLocation l)
	{
		controller = null;
		motion.X = 0f;
		motion.Y = -1f;
		destroy = true;
	}

	public virtual bool foundCropEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
	{
		if (location.terrainFeatures.TryGetValue(new Vector2(currentNode.x, currentNode.y), out var terrainFeature))
		{
			if (location.isCropAtTile(currentNode.x, currentNode.y) && (terrainFeature as HoeDirt).readyForHarvest())
			{
				return true;
			}
			if (terrainFeature is Bush bush && (int)bush.tileSheetOffset == 1)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void pathfindToNewCrop()
	{
		JunimoHut hut = home;
		if (hut == null)
		{
			return;
		}
		if (Game1.timeOfDay > 1900)
		{
			if (controller == null)
			{
				returnToJunimoHut(base.currentLocation);
			}
			return;
		}
		if (Game1.random.NextDouble() < 0.035 || (bool)hut.noHarvest)
		{
			pathfindToRandomSpotAroundHut();
			return;
		}
		controller = new PathFindController(this, base.currentLocation, foundCropEndFunction, -1, reachFirstDestinationFromHut, 100, Point.Zero);
		Point? endpoint = controller.pathToEndPoint?.Last();
		if (!endpoint.HasValue || Math.Abs(endpoint.Value.X - ((int)hut.tileX + 1)) > hut.cropHarvestRadius || Math.Abs(endpoint.Value.Y - ((int)hut.tileY + 1)) > hut.cropHarvestRadius)
		{
			if (Game1.random.NextBool() && !hut.lastKnownCropLocation.Equals(Point.Zero))
			{
				controller = new PathFindController(this, base.currentLocation, hut.lastKnownCropLocation, -1, reachFirstDestinationFromHut, 100);
			}
			else if (Game1.random.NextDouble() < 0.25)
			{
				netAnimationEvent.Fire(0);
				returnToJunimoHut(base.currentLocation);
			}
			else
			{
				pathfindToRandomSpotAroundHut();
			}
		}
		else
		{
			netAnimationEvent.Fire(0);
		}
	}

	public virtual void returnToJunimoHut(GameLocation location)
	{
		if (Utility.isOnScreen(Utility.Vector2ToPoint(position.Value / 64f), 64, base.currentLocation))
		{
			jump();
		}
		collidesWithOtherCharacters.Value = false;
		if (Game1.IsMasterGame)
		{
			JunimoHut hut = home;
			if (hut == null)
			{
				return;
			}
			controller = new PathFindController(this, location, new Point((int)hut.tileX + 1, (int)hut.tileY + 1), 0, junimoReachedHut);
			if (controller.pathToEndPoint == null || controller.pathToEndPoint.Count == 0 || location.isCollidingPosition(nextPosition, Game1.viewport, isFarmer: false, 0, glider: false, this))
			{
				destroy = true;
			}
		}
		if (Utility.isOnScreen(Utility.Vector2ToPoint(position.Value / 64f), 64, base.currentLocation))
		{
			location.playSound("junimoMeep1");
		}
	}

	public override void faceDirection(int direction)
	{
	}

	protected override void updateSlaveAnimation(GameTime time)
	{
	}

	protected virtual bool isHarvestable()
	{
		if (base.currentLocation.terrainFeatures.TryGetValue(base.Tile, out var terrainFeature))
		{
			if (terrainFeature is HoeDirt dirt)
			{
				return dirt.readyForHarvest();
			}
			if (terrainFeature is Bush bush)
			{
				return (int)bush.tileSheetOffset == 1;
			}
		}
		return false;
	}

	public override void update(GameTime time, GameLocation location)
	{
		netAnimationEvent.Poll();
		base.update(time, location);
		if (isPrismatic.Value)
		{
			color.Value = Utility.GetPrismaticColor(whichJunimoFromThisHut);
		}
		forceUpdateTimer = 99999;
		if (EventActor)
		{
			return;
		}
		if (destroy)
		{
			alphaChange = -0.05f;
		}
		alpha += alphaChange;
		if (alpha > 1f)
		{
			alpha = 1f;
		}
		else if (alpha < 0f)
		{
			alpha = 0f;
			if (destroy && Game1.IsMasterGame)
			{
				location.characters.Remove(this);
				home?.myJunimos.Remove(this);
			}
		}
		if (Game1.IsMasterGame)
		{
			if (harvestTimer > 0)
			{
				int oldTimer = harvestTimer;
				harvestTimer -= time.ElapsedGameTime.Milliseconds;
				if (harvestTimer > 1800)
				{
					netAnimationEvent.Fire(2);
				}
				else if (harvestTimer > 1600)
				{
					netAnimationEvent.Fire(3);
				}
				else if (harvestTimer > 1000)
				{
					netAnimationEvent.Fire(4);
					shake(50);
				}
				else if (oldTimer >= 1000 && harvestTimer < 1000)
				{
					netAnimationEvent.Fire(2);
					JunimoHut hut = home;
					if (base.currentLocation != null && hut != null && !hut.noHarvest && isHarvestable())
					{
						netAnimationEvent.Fire(5);
						lastItemHarvested = null;
						TerrainFeature terrainFeature = base.currentLocation.terrainFeatures[base.Tile];
						if (!(terrainFeature is Bush bush))
						{
							if (terrainFeature is HoeDirt dirt && dirt.crop.harvest(base.TilePoint.X, base.TilePoint.Y, dirt, this))
							{
								dirt.destroyCrop(base.currentLocation.farmers.Any());
							}
						}
						else if ((int)bush.tileSheetOffset == 1)
						{
							tryToAddItemToHut(ItemRegistry.Create("(O)815"));
							bush.tileSheetOffset.Value = 0;
							bush.setUpSourceRect();
							if (Utility.isOnScreen(base.TilePoint, 64, base.currentLocation))
							{
								bush.performUseAction(base.Tile);
							}
							if (Utility.isOnScreen(base.TilePoint, 64, base.currentLocation))
							{
								DelayedAction.playSoundAfterDelay("coin", 260, base.currentLocation);
							}
						}
						if (lastItemHarvested != null)
						{
							bool gotDouble = false;
							if ((int)home.raisinDays > 0 && Game1.random.NextDouble() < 0.2)
							{
								gotDouble = true;
								Item i = lastItemHarvested.getOne();
								i.Quality = lastItemHarvested.Quality;
								tryToAddItemToHut(i);
							}
							if (base.currentLocation.farmers.Any())
							{
								ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(lastItemHarvested.QualifiedItemId);
								float mainDrawLayer = (float)base.StandingPixel.Y / 10000f + 0.01f;
								if (gotDouble)
								{
									for (int i = 0; i < 2; i++)
									{
										Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(), 1000f, 1, 0, base.Position + new Vector2(0f, -40f), flicker: false, flipped: false, mainDrawLayer, 0.02f, Color.White, 4f, -0.01f, 0f, 0f)
										{
											motion = new Vector2((float)((i != 0) ? 1 : (-1)) * 0.5f, -0.25f),
											delayBeforeAnimationStart = 200
										});
										if (lastItemHarvested is ColoredObject coloredObj2)
										{
											Rectangle colored_source_rect = ItemRegistry.GetDataOrErrorItem(lastItemHarvested.QualifiedItemId).GetSourceRect(1);
											Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(itemData.TextureName, colored_source_rect, 1000f, 1, 0, base.Position + new Vector2(0f, -40f), flicker: false, flipped: false, mainDrawLayer + 0.005f, 0.02f, coloredObj2.color.Value, 4f, -0.01f, 0f, 0f)
											{
												motion = new Vector2((float)((i != 0) ? 1 : (-1)) * 0.5f, -0.25f),
												delayBeforeAnimationStart = 200
											});
										}
									}
								}
								else
								{
									Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(), 1000f, 1, 0, base.Position + new Vector2(0f, -40f), flicker: false, flipped: false, mainDrawLayer, 0.02f, Color.White, 4f, -0.01f, 0f, 0f)
									{
										motion = new Vector2(0.08f, -0.25f)
									});
									if (lastItemHarvested is ColoredObject coloredObj)
									{
										Rectangle colored_source_rect = ItemRegistry.GetDataOrErrorItem(lastItemHarvested.QualifiedItemId).GetSourceRect(1);
										Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(itemData.TextureName, colored_source_rect, 1000f, 1, 0, base.Position + new Vector2(0f, -40f), flicker: false, flipped: false, mainDrawLayer + 0.005f, 0.02f, coloredObj.color.Value, 4f, -0.01f, 0f, 0f)
										{
											motion = new Vector2(0.08f, -0.25f)
										});
									}
								}
							}
						}
					}
				}
				else if (harvestTimer <= 0)
				{
					pokeToHarvest();
				}
			}
			else if (alpha > 0f && controller == null)
			{
				if ((addedSpeed > 0f || base.speed > 3 || isCharging) && Game1.IsMasterGame)
				{
					destroy = true;
				}
				nextPosition = GetBoundingBox();
				nextPosition.X += (int)motion.X;
				bool sparkle = false;
				if (!location.isCollidingPosition(nextPosition, Game1.viewport, this))
				{
					position.X += (int)motion.X;
					sparkle = true;
				}
				nextPosition.X -= (int)motion.X;
				nextPosition.Y += (int)motion.Y;
				if (!location.isCollidingPosition(nextPosition, Game1.viewport, this))
				{
					position.Y += (int)motion.Y;
					sparkle = true;
				}
				if (!motion.Equals(Vector2.Zero) && sparkle && Game1.random.NextDouble() < 0.005)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(Game1.random.Choose(10, 11), base.Position, color.Value)
					{
						motion = motion / 4f,
						alphaFade = 0.01f,
						layerDepth = 0.8f,
						scale = 0.75f,
						alpha = 0.75f
					});
				}
				if (Game1.random.NextDouble() < 0.002)
				{
					switch (Game1.random.Next(6))
					{
					case 0:
						netAnimationEvent.Fire(6);
						break;
					case 1:
						netAnimationEvent.Fire(7);
						break;
					case 2:
						netAnimationEvent.Fire(0);
						break;
					case 3:
						jumpWithoutSound();
						yJumpVelocity /= 2f;
						netAnimationEvent.Fire(0);
						break;
					case 4:
					{
						JunimoHut hut = home;
						if (hut != null && !hut.noHarvest)
						{
							pathfindToNewCrop();
						}
						break;
					}
					case 5:
						netAnimationEvent.Fire(8);
						break;
					}
				}
			}
		}
		bool moveRight = base.moveRight;
		bool moveLeft = base.moveLeft;
		bool moveUp = base.moveUp;
		bool moveDown = base.moveDown;
		if (Game1.IsMasterGame)
		{
			if (controller == null && motion.Equals(Vector2.Zero))
			{
				return;
			}
			moveRight |= Math.Abs(motion.X) > Math.Abs(motion.Y) && motion.X > 0f;
			moveLeft |= Math.Abs(motion.X) > Math.Abs(motion.Y) && motion.X < 0f;
			moveUp |= Math.Abs(motion.Y) > Math.Abs(motion.X) && motion.Y < 0f;
			moveDown |= Math.Abs(motion.Y) > Math.Abs(motion.X) && motion.Y > 0f;
		}
		else
		{
			moveLeft = IsRemoteMoving() && FacingDirection == 3;
			moveRight = IsRemoteMoving() && FacingDirection == 1;
			moveUp = IsRemoteMoving() && FacingDirection == 0;
			moveDown = IsRemoteMoving() && FacingDirection == 2;
			if (!moveRight && !moveLeft && !moveUp && !moveDown)
			{
				return;
			}
		}
		Sprite.CurrentAnimation = null;
		if (moveRight)
		{
			flip = false;
			if (Sprite.Animate(time, 16, 8, 50f))
			{
				Sprite.currentFrame = 16;
			}
		}
		else if (moveLeft)
		{
			if (Sprite.Animate(time, 16, 8, 50f))
			{
				Sprite.currentFrame = 16;
			}
			flip = true;
		}
		else if (moveUp)
		{
			if (Sprite.Animate(time, 32, 8, 50f))
			{
				Sprite.currentFrame = 32;
			}
		}
		else if (moveDown)
		{
			Sprite.Animate(time, 0, 8, 50f);
		}
	}

	public virtual void pathfindToRandomSpotAroundHut()
	{
		JunimoHut hut = home;
		if (hut != null)
		{
			controller = new PathFindController(endPoint: Utility.Vector2ToPoint(new Vector2((int)hut.tileX + 1 + Game1.random.Next(-hut.cropHarvestRadius, hut.cropHarvestRadius + 1), (int)hut.tileY + 1 + Game1.random.Next(-hut.cropHarvestRadius, hut.cropHarvestRadius + 1))), c: this, location: base.currentLocation, finalFacingDirection: -1, endBehaviorFunction: reachFirstDestinationFromHut, limit: 100);
		}
	}

	public virtual void tryToAddItemToHut(Item i)
	{
		lastItemHarvested = i;
		Item result = home?.GetOutputChest().addItem(i);
		if (result != null)
		{
			for (int j = 0; j < result.Stack; j++)
			{
				Game1.createObjectDebris(i.QualifiedItemId, base.TilePoint.X, base.TilePoint.Y, -1, i.Quality, 1f, base.currentLocation);
			}
		}
	}

	public override void draw(SpriteBatch b, float alpha = 1f)
	{
		if (this.alpha > 0f)
		{
			float mainDrawLayer = (float)(base.StandingPixel.Y + 2) / 10000f;
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Sprite.SpriteWidth * 4 / 2, (float)Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow(Sprite.SpriteHeight / 16, 2.0) + (float)yJumpOffset - 8f) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, color.Value * this.alpha, rotation, new Vector2(Sprite.SpriteWidth * 4 / 2, (float)(Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : mainDrawLayer));
			if (!swimming)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2((float)(Sprite.SpriteWidth * 4) / 2f, 44f)), Game1.shadowTexture.Bounds, color.Value * this.alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (4f + (float)yJumpOffset / 40f) * scale.Value, SpriteEffects.None, Math.Max(0f, mainDrawLayer) - 1E-06f);
			}
		}
	}
}
