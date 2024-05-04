using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Characters;

public class Horse : NPC
{
	private readonly NetGuid horseId = new NetGuid();

	private readonly NetFarmerRef netRider = new NetFarmerRef();

	public readonly NetLong ownerId = new NetLong();

	[XmlIgnore]
	public readonly NetBool mounting = new NetBool();

	[XmlIgnore]
	public readonly NetBool dismounting = new NetBool();

	private Vector2 dismountTile;

	private int ridingAnimationDirection;

	private bool roomForHorseAtDismountTile;

	[XmlElement("hat")]
	public readonly NetRef<Hat> hat = new NetRef<Hat>();

	public readonly NetMutex mutex = new NetMutex();

	[XmlIgnore]
	public Action<string> onFootstepAction;

	[XmlIgnore]
	public bool ateCarrotToday;

	private bool squeezingThroughGate;

	private int munchingCarrotTimer;

	public Guid HorseId
	{
		get
		{
			return horseId.Value;
		}
		set
		{
			horseId.Value = value;
		}
	}

	[XmlIgnore]
	public Farmer rider
	{
		get
		{
			return netRider.Value;
		}
		set
		{
			netRider.Value = value;
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public Horse()
	{
		base.willDestroyObjectsUnderfoot = false;
		base.HideShadow = true;
		drawOffset = new Vector2(-16f, 0f);
		onFootstepAction = PerformDefaultHorseFootstep;
		ChooseAppearance();
		faceDirection(3);
		base.Breather = false;
	}

	public Horse(Guid horseId, int xTile, int yTile)
		: this()
	{
		base.Name = "";
		displayName = base.Name;
		base.Position = new Vector2(xTile, yTile) * 64f;
		base.currentLocation = Game1.currentLocation;
		HorseId = horseId;
	}

	public override void reloadData()
	{
	}

	protected override string translateName()
	{
		return name.Value.Trim();
	}

	public override bool canTalk()
	{
		return false;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(horseId, "horseId").AddField(netRider.NetFields, "netRider.NetFields").AddField(mounting, "mounting")
			.AddField(dismounting, "dismounting")
			.AddField(hat, "hat")
			.AddField(mutex.NetFields, "mutex.NetFields")
			.AddField(ownerId, "ownerId");
		position.Field.AxisAlignedMovement = false;
		facingDirection.fieldChangeEvent += delegate
		{
			ClearCachedPosition();
		};
	}

	public Farmer getOwner()
	{
		if (ownerId.Value == 0L)
		{
			return null;
		}
		return Game1.getFarmerMaybeOffline(ownerId.Value);
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
	}

	/// <inheritdoc />
	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
		if (Sprite == null)
		{
			Sprite = new AnimatedSprite("Animals\\horse", 0, 32, 32);
			Sprite.textureUsesFlippedRightForLeft = true;
			Sprite.loop = true;
		}
	}

	public override void dayUpdate(int dayOfMonth)
	{
		ateCarrotToday = false;
		faceDirection(3);
	}

	public override Rectangle GetBoundingBox()
	{
		Rectangle r = base.GetBoundingBox();
		if (squeezingThroughGate && (FacingDirection == 0 || FacingDirection == 2))
		{
			r.Inflate(-36, 0);
		}
		return r;
	}

	public override bool canPassThroughActionTiles()
	{
		return false;
	}

	public void squeezeForGate()
	{
		if (!squeezingThroughGate)
		{
			squeezingThroughGate = true;
			ClearCachedPosition();
		}
		rider?.TemporaryPassableTiles.Add(GetBoundingBox());
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.currentLocation = location;
		mutex.Update(location);
		if (squeezingThroughGate)
		{
			squeezingThroughGate = false;
			ClearCachedPosition();
		}
		faceTowardFarmer = false;
		faceTowardFarmerTimer = -1;
		Sprite.loop = rider != null && !rider.hidden;
		if (rider != null && (bool)rider.hidden)
		{
			return;
		}
		if (munchingCarrotTimer > 0)
		{
			munchingCarrotTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (munchingCarrotTimer <= 0)
			{
				mutex.ReleaseLock();
			}
			base.update(time, location);
			return;
		}
		if (rider != null && rider.isAnimatingMount)
		{
			rider.showNotCarrying();
		}
		if ((bool)mounting)
		{
			if (rider == null || !rider.IsLocalPlayer)
			{
				return;
			}
			if (rider.mount != null)
			{
				mounting.Value = false;
				rider.isAnimatingMount = false;
				rider = null;
				Halt();
				farmerPassesThrough = false;
				return;
			}
			Rectangle horseBounds = GetBoundingBox();
			int anchorX = horseBounds.X + 16;
			if (rider.Position.X < (float)(anchorX - 4))
			{
				rider.position.X += 4f;
			}
			else if (rider.Position.X > (float)(anchorX + 4))
			{
				rider.position.X -= 4f;
			}
			int riderStandingY = rider.StandingPixel.Y;
			if (riderStandingY < horseBounds.Y - 4)
			{
				rider.position.Y += 4f;
			}
			else if (riderStandingY > horseBounds.Y + 4)
			{
				rider.position.Y -= 4f;
			}
			if (rider.yJumpOffset >= -8 && rider.yJumpVelocity <= 0f)
			{
				Halt();
				Sprite.loop = true;
				base.currentLocation.characters.Remove(this);
				rider.mount = this;
				rider.freezePause = -1;
				mounting.Value = false;
				rider.isAnimatingMount = false;
				rider.canMove = true;
				if (FacingDirection == 1)
				{
					rider.xOffset += 8f;
				}
			}
		}
		else if ((bool)dismounting)
		{
			if (rider == null || !rider.IsLocalPlayer)
			{
				Halt();
				return;
			}
			if (rider.isAnimatingMount)
			{
				rider.faceDirection(FacingDirection);
			}
			Vector2 targetPosition = new Vector2(dismountTile.X * 64f + 32f - (float)(rider.GetBoundingBox().Width / 2), dismountTile.Y * 64f + 4f);
			if (Math.Abs(rider.Position.X - targetPosition.X) > 4f)
			{
				if (rider.Position.X < targetPosition.X)
				{
					rider.position.X += Math.Min(4f, targetPosition.X - rider.Position.X);
				}
				else if (rider.Position.X > targetPosition.X)
				{
					rider.position.X += Math.Max(-4f, targetPosition.X - rider.Position.X);
				}
			}
			if (Math.Abs(rider.Position.Y - targetPosition.Y) > 4f)
			{
				if (rider.Position.Y < targetPosition.Y)
				{
					rider.position.Y += Math.Min(4f, targetPosition.Y - rider.Position.Y);
				}
				else if (rider.Position.Y > targetPosition.Y)
				{
					rider.position.Y += Math.Max(-4f, targetPosition.Y - rider.Position.Y);
				}
			}
			if (rider.yJumpOffset >= 0 && rider.yJumpVelocity <= 0f)
			{
				rider.position.Y += 8f;
				rider.position.X = targetPosition.X;
				int tries = 0;
				while (rider.currentLocation.isCollidingPosition(rider.GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, rider) && tries < 6)
				{
					tries++;
					rider.position.Y -= 4f;
				}
				if (tries == 6)
				{
					rider.Position = base.Position;
					dismounting.Value = false;
					rider.isAnimatingMount = false;
					rider.freezePause = -1;
					rider.canMove = true;
					return;
				}
				dismount();
			}
		}
		else if (rider == null && FacingDirection != 2 && Sprite.CurrentAnimation == null && Game1.random.NextDouble() < 0.002)
		{
			Sprite.loop = false;
			switch (FacingDirection)
			{
			case 0:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(25, Game1.random.Next(250, 750)),
					new FarmerSprite.AnimationFrame(14, 10)
				});
				break;
			case 1:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(21, 100),
					new FarmerSprite.AnimationFrame(22, 100),
					new FarmerSprite.AnimationFrame(23, 400),
					new FarmerSprite.AnimationFrame(24, 400),
					new FarmerSprite.AnimationFrame(23, 400),
					new FarmerSprite.AnimationFrame(24, 400),
					new FarmerSprite.AnimationFrame(23, 400),
					new FarmerSprite.AnimationFrame(24, 400),
					new FarmerSprite.AnimationFrame(23, 400),
					new FarmerSprite.AnimationFrame(22, 100),
					new FarmerSprite.AnimationFrame(21, 100)
				});
				break;
			case 3:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(23, 100, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: true)
				});
				break;
			}
		}
		else if (rider != null)
		{
			if (FacingDirection != rider.FacingDirection || ridingAnimationDirection != FacingDirection)
			{
				Sprite.StopAnimation();
				faceDirection(rider.FacingDirection);
			}
			bool num = (rider.movementDirections.Any() && rider.CanMove) || rider.position.Field.IsInterpolating();
			SyncPositionToRider();
			if (!num)
			{
				Sprite.StopAnimation();
				faceDirection(rider.FacingDirection);
			}
			else if (Sprite.CurrentAnimation == null)
			{
				switch (FacingDirection)
				{
				case 1:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(8, 70),
						new FarmerSprite.AnimationFrame(9, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(10, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(11, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(12, 70),
						new FarmerSprite.AnimationFrame(13, 70)
					});
					break;
				case 3:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(8, 70, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(9, 70, secondaryArm: false, flip: true, OnMountFootstep),
						new FarmerSprite.AnimationFrame(10, 70, secondaryArm: false, flip: true, OnMountFootstep),
						new FarmerSprite.AnimationFrame(11, 70, secondaryArm: false, flip: true, OnMountFootstep),
						new FarmerSprite.AnimationFrame(12, 70, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(13, 70, secondaryArm: false, flip: true)
					});
					break;
				case 0:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(15, 70),
						new FarmerSprite.AnimationFrame(16, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(17, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(18, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(19, 70),
						new FarmerSprite.AnimationFrame(20, 70)
					});
					break;
				case 2:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(1, 70),
						new FarmerSprite.AnimationFrame(2, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(3, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(4, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(5, 70),
						new FarmerSprite.AnimationFrame(6, 70)
					});
					break;
				}
				ridingAnimationDirection = FacingDirection;
			}
		}
		if (FacingDirection == 3)
		{
			drawOffset = Vector2.Zero;
		}
		else
		{
			drawOffset = new Vector2(-16f, 0f);
		}
		flip = FacingDirection == 3;
		base.update(time, location);
	}

	public virtual void OnMountFootstep(Farmer who)
	{
		if (onFootstepAction != null && rider != null)
		{
			string step_type = rider.currentLocation.doesTileHaveProperty(rider.TilePoint.X, rider.TilePoint.Y, "Type", "Back");
			onFootstepAction(step_type);
		}
	}

	public virtual void PerformDefaultHorseFootstep(string step_type)
	{
		if (rider == null)
		{
			return;
		}
		if (!(step_type == "Stone"))
		{
			if (step_type == "Wood")
			{
				if (rider.ShouldHandleAnimationSound())
				{
					rider.currentLocation.localSound("woodyStep", base.Tile);
				}
				if (rider == Game1.player)
				{
					Rumble.rumble(0.1f, 50f);
				}
			}
			else
			{
				if (rider.ShouldHandleAnimationSound())
				{
					rider.currentLocation.localSound("thudStep", base.Tile);
				}
				if (rider == Game1.player)
				{
					Rumble.rumble(0.3f, 50f);
				}
			}
		}
		else
		{
			if (rider.ShouldHandleAnimationSound())
			{
				rider.currentLocation.localSound("stoneStep", base.Tile);
			}
			if (rider == Game1.player)
			{
				Rumble.rumble(0.1f, 50f);
			}
		}
	}

	public void dismount(bool from_demolish = false)
	{
		mutex.ReleaseLock();
		rider.mount = null;
		if (base.currentLocation != null)
		{
			if (!from_demolish && TryFindStable() != null && !base.currentLocation.characters.Any((NPC c) => c is Horse horse && horse.HorseId == HorseId))
			{
				base.currentLocation.characters.Add(this);
			}
			SyncPositionToRider();
			rider.TemporaryPassableTiles.Add(new Rectangle((int)dismountTile.X * 64, (int)dismountTile.Y * 64, 64, 64));
			rider.freezePause = -1;
			dismounting.Value = false;
			rider.isAnimatingMount = false;
			rider.canMove = true;
			rider.forceCanMove();
			rider.xOffset = 0f;
			rider = null;
			Halt();
			farmerPassesThrough = false;
		}
	}

	/// <summary>Find the stable which this horse calls home, if it exists.</summary>
	public Stable TryFindStable()
	{
		Stable match = null;
		Utility.ForEachBuilding(delegate(Stable stable)
		{
			if (stable.HorseId == HorseId)
			{
				match = stable;
				return false;
			}
			return true;
		});
		return match;
	}

	public void nameHorse(string name)
	{
		if (name.Length <= 0)
		{
			return;
		}
		Game1.multiplayer.globalChatInfoMessage("HorseNamed", Game1.player.Name, name);
		Utility.ForEachVillager(delegate(NPC n)
		{
			if (n.Name == name)
			{
				name += " ";
			}
			return true;
		});
		base.Name = name;
		displayName = name;
		if (Game1.player.horseName.Value == null)
		{
			Game1.player.horseName.Value = name;
		}
		Game1.exitActiveMenu();
		Game1.playSound("newArtifact");
		if (mutex.IsLockHeld())
		{
			mutex.ReleaseLock();
		}
	}

	public override bool checkAction(Farmer who, GameLocation l)
	{
		if (who != null && !who.canMove)
		{
			return false;
		}
		if (munchingCarrotTimer > 0)
		{
			return false;
		}
		if (rider == null)
		{
			mutex.RequestLock(delegate
			{
				if (who.mount != null || rider != null || who.FarmerSprite.PauseForSingleAnimation || base.currentLocation != who.currentLocation)
				{
					mutex.ReleaseLock();
				}
				else
				{
					Stable stable = TryFindStable();
					if (stable != null)
					{
						if ((getOwner() == Game1.player || (getOwner() == null && (string.IsNullOrEmpty(Game1.player.horseName.Value) || Utility.findHorseForPlayer(Game1.player.UniqueMultiplayerID) == null))) && base.Name.Length <= 0)
						{
							stable.owner.Value = who.UniqueMultiplayerID;
							stable.updateHorseOwnership();
							Utility.ForEachBuilding(delegate(Stable curStable)
							{
								if (curStable.owner.Value == who.UniqueMultiplayerID && curStable != stable)
								{
									stable.owner.Value = 0L;
									stable.updateHorseOwnership();
								}
								return true;
							});
							if (string.IsNullOrEmpty(Game1.player.horseName.Value))
							{
								Game1.activeClickableMenu = new NamingMenu(nameHorse, Game1.content.LoadString("Strings\\Characters:NameYourHorse"), Game1.content.LoadString("Strings\\Characters:DefaultHorseName"));
								return;
							}
						}
						else
						{
							if (who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] is Hat value)
							{
								if (hat.Value != null)
								{
									Game1.createItemDebris(hat.Value, base.Position, FacingDirection);
									hat.Value = null;
								}
								else
								{
									who.Items[who.CurrentToolIndex] = null;
									hat.Value = value;
									Game1.playSound("dirtyHit");
								}
								mutex.ReleaseLock();
								return;
							}
							if (!ateCarrotToday && who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] is Object { QualifiedItemId: "(O)Carrot" })
							{
								Sprite.StopAnimation();
								Sprite.faceDirection(FacingDirection);
								Game1.playSound("eat");
								DelayedAction.playSoundAfterDelay("eat", 600);
								DelayedAction.playSoundAfterDelay("eat", 1200);
								munchingCarrotTimer = 1500;
								doEmote(20, 32);
								who.reduceActiveItemByOne();
								ateCarrotToday = true;
								return;
							}
						}
					}
					rider = who;
					rider.freezePause = 5000;
					rider.synchronizedJump(6f);
					rider.Halt();
					if (rider.Position.X < base.Position.X)
					{
						rider.faceDirection(1);
					}
					l.playSound("dwop");
					mounting.Value = true;
					rider.isAnimatingMount = true;
					rider.completelyStopAnimatingOrDoingAction();
					rider.faceGeneralDirection(Utility.PointToVector2(base.StandingPixel), 0, opposite: false, useTileCalculations: false);
				}
			});
			return true;
		}
		dismounting.Value = true;
		rider.isAnimatingMount = true;
		farmerPassesThrough = false;
		rider.TemporaryPassableTiles.Clear();
		Vector2 position = Utility.recursiveFindOpenTileForCharacter(rider, rider.currentLocation, base.Tile, 8);
		base.Position = new Vector2(position.X * 64f + 32f - (float)(GetBoundingBox().Width / 2), position.Y * 64f + 4f);
		roomForHorseAtDismountTile = !base.currentLocation.isCollidingPosition(GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, this);
		base.Position = rider.Position;
		dismounting.Value = false;
		rider.isAnimatingMount = false;
		Halt();
		if (!position.Equals(Vector2.Zero) && Vector2.Distance(position, base.Tile) < 2f)
		{
			rider.synchronizedJump(6f);
			l.playSound("dwop");
			rider.freezePause = 5000;
			rider.Halt();
			rider.xOffset = 0f;
			dismounting.Value = true;
			rider.isAnimatingMount = true;
			dismountTile = position;
		}
		else
		{
			dismount();
		}
		return true;
	}

	public void SyncPositionToRider()
	{
		if (rider != null && (!dismounting || roomForHorseAtDismountTile))
		{
			base.Position = rider.Position;
		}
	}

	public override void draw(SpriteBatch b)
	{
		flip = FacingDirection == 3;
		Sprite.UpdateSourceRect();
		base.draw(b);
		if (FacingDirection == 2 && rider != null)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(48f, -24f - rider.yOffset), new Rectangle(160, 96, 9, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f);
		}
		bool draw_hat = true;
		if (hat.Value != null)
		{
			Vector2 hatOffset = Vector2.Zero;
			switch (hat.Value.ItemId)
			{
			case "14":
				if (FacingDirection == 0)
				{
					hatOffset.X = -100f;
				}
				break;
			case "6":
				hatOffset.Y += 2f;
				if (FacingDirection == 2)
				{
					hatOffset.Y -= 1f;
				}
				break;
			case "10":
				hatOffset.Y += 3f;
				if (FacingDirection == 0)
				{
					draw_hat = false;
				}
				break;
			case "9":
			case "32":
				if (FacingDirection == 0 || FacingDirection == 2)
				{
					hatOffset.Y += 1f;
				}
				break;
			case "31":
				hatOffset.Y += 1f;
				break;
			case "39":
			case "11":
				if (FacingDirection == 3 || FacingDirection == 1)
				{
					if (flip)
					{
						hatOffset.X += 2f;
					}
					else
					{
						hatOffset.X -= 2f;
					}
				}
				break;
			case "26":
				if (FacingDirection == 3 || FacingDirection == 1)
				{
					if (flip)
					{
						hatOffset.X += 1f;
					}
					else
					{
						hatOffset.X -= 1f;
					}
				}
				break;
			case "67":
			case "56":
				if (FacingDirection == 0)
				{
					draw_hat = false;
				}
				break;
			}
			hatOffset *= 4f;
			if (shakeTimer > 0)
			{
				hatOffset += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			if (hatOffset.X <= -100f)
			{
				return;
			}
			float horse_draw_layer = (float)base.StandingPixel.Y / 10000f;
			if (rider != null)
			{
				if (FacingDirection == 2)
				{
					horse_draw_layer = (position.Y + 64f + 1f) / 10000f;
				}
				else if (FacingDirection != 0)
				{
					horse_draw_layer = (position.Y + 48f - 1f) / 10000f;
				}
			}
			if (munchingCarrotTimer > 0)
			{
				if (FacingDirection == 2)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(24f, -24f), new Rectangle(170 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 112, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, horse_draw_layer + 1E-07f);
				}
				else if (FacingDirection == 1)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(80f, -56f), new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, horse_draw_layer + 1E-07f);
				}
				else if (FacingDirection == 3)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(-16f, -56f), new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, horse_draw_layer + 1E-07f);
				}
			}
			if (!draw_hat)
			{
				return;
			}
			horse_draw_layer += 2E-07f;
			switch (Sprite.CurrentFrame)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
				hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(30f, -42f - ((rider != null) ? rider.yOffset : 0f))), 1.3333334f, 1f, horse_draw_layer, 2);
				break;
			case 7:
			case 11:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -74f), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -74f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 8:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -74f), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -74f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 9:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -70f), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -70f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 10:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -70f), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -70f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 12:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -78f)), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -78f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 13:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -78f)), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -78f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 21:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -66f)), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -66f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 22:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -54f)), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -54f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 23:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -42f)), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -42f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 24:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -42f)), 1.3333334f, 1f, horse_draw_layer, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -42f)), 1.3333334f, 1f, horse_draw_layer, 1);
				}
				break;
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
			case 25:
				hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(28f, -106f - ((rider != null) ? rider.yOffset : 0f)), 1.3333334f, 1f, horse_draw_layer, 0);
				break;
			}
		}
		else if (munchingCarrotTimer > 0)
		{
			if (FacingDirection == 2)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(24f, -24f), new Rectangle(170 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 112, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f + 1E-07f);
			}
			else if (FacingDirection == 1)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(80f, -56f), new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f + 1E-07f);
			}
			else if (FacingDirection == 3)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(-16f, -56f), new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, (base.Position.Y + 64f) / 10000f + 1E-07f);
			}
		}
	}
}
