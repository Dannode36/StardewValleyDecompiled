using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Fences;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;

namespace StardewValley;

public class Fence : Object
{
	public const int debrisPieces = 4;

	public static int fencePieceWidth = 16;

	public static int fencePieceHeight = 32;

	public const int gateClosedPosition = 0;

	public const int gateOpenedPosition = 88;

	public const int sourceRectForSoloGate = 17;

	public const int globalHealthMultiplier = 2;

	public const int N = 1000;

	public const int E = 100;

	public const int S = 500;

	public const int W = 10;

	/// <summary>The unqualified item ID for a wood fence.</summary>
	public const string woodFenceId = "322";

	/// <summary>The unqualified item ID for a stone fence.</summary>
	public const string stoneFenceId = "323";

	/// <summary>The unqualified item ID for an iron fence.</summary>
	public const string ironFenceId = "324";

	/// <summary>The unqualified item ID for a hardwood fence.</summary>
	public const string hardwoodFenceId = "298";

	/// <summary>The unqualified item ID for a fence gate.</summary>
	public const string gateId = "325";

	[XmlIgnore]
	public Lazy<Texture2D> fenceTexture;

	public static Dictionary<int, int> fenceDrawGuide;

	[XmlElement("health")]
	public new readonly NetFloat health = new NetFloat();

	[XmlElement("maxHealth")]
	public readonly NetFloat maxHealth = new NetFloat();

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Item.ItemId" /> instead.</summary>
	[XmlElement("whichType")]
	public int? obsolete_whichType;

	[XmlElement("gatePosition")]
	public readonly NetInt gatePosition = new NetInt();

	public int gateMotion;

	[XmlElement("isGate")]
	public readonly NetBool isGate = new NetBool();

	[XmlIgnore]
	public readonly NetBool repairQueued = new NetBool();

	protected static Dictionary<string, FenceData> _FenceLookup;

	protected FenceData _data;

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(health, "health").AddField(maxHealth, "maxHealth").AddField(gatePosition, "gatePosition")
			.AddField(isGate, "isGate")
			.AddField(repairQueued, "repairQueued");
		itemId.fieldChangeVisibleEvent += delegate
		{
			OnIdChanged();
		};
		isGate.fieldChangeVisibleEvent += delegate
		{
			OnIdChanged();
		};
	}

	public Fence(Vector2 tileLocation, string itemId, bool isGate)
		: base(itemId, 1)
	{
		if (fenceDrawGuide == null)
		{
			populateFenceDrawGuide();
		}
		base.Type = "Crafting";
		this.isGate.Value = isGate;
		TileLocation = tileLocation;
		canBeSetDown.Value = true;
		canBeGrabbed.Value = true;
		price.Value = 1;
		ResetHealth((float)Game1.random.Next(-100, 101) / 100f);
		if (isGate)
		{
			health.Value *= 2f;
		}
		OnIdChanged();
	}

	public Fence()
		: this(Vector2.Zero, "322", isGate: false)
	{
	}

	public virtual void ResetHealth(float amount_adjustment)
	{
		float base_health = GetData()?.Health ?? 100;
		if ((bool)isGate)
		{
			amount_adjustment = 0f;
		}
		health.Value = base_health + amount_adjustment;
		health.Value *= 2f;
		maxHealth.Value = health.Value;
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		switch (obsolete_whichType ?? 1)
		{
		case 2:
			base.ItemId = "323";
			break;
		case 3:
			base.ItemId = "324";
			break;
		case 4:
			base.ItemId = "325";
			break;
		case 5:
			base.ItemId = "298";
			break;
		default:
			base.ItemId = "322";
			break;
		}
		obsolete_whichType = null;
	}

	/// <summary>Reset the fence data and texture when the item ID changes (e.g. when the save is being loaded).</summary>
	protected virtual void OnIdChanged()
	{
		if (fenceTexture == null || fenceTexture.IsValueCreated)
		{
			fenceTexture = new Lazy<Texture2D>(loadFenceTexture);
		}
		_data = null;
	}

	public virtual void repair()
	{
		ResetHealth((float)Game1.random.Next(-100, 101) / 100f);
	}

	public static void populateFenceDrawGuide()
	{
		fenceDrawGuide = new Dictionary<int, int>();
		fenceDrawGuide.Add(0, 5);
		fenceDrawGuide.Add(10, 9);
		fenceDrawGuide.Add(100, 10);
		fenceDrawGuide.Add(1000, 3);
		fenceDrawGuide.Add(500, 5);
		fenceDrawGuide.Add(1010, 8);
		fenceDrawGuide.Add(1100, 6);
		fenceDrawGuide.Add(1500, 3);
		fenceDrawGuide.Add(600, 0);
		fenceDrawGuide.Add(510, 2);
		fenceDrawGuide.Add(110, 7);
		fenceDrawGuide.Add(1600, 0);
		fenceDrawGuide.Add(1610, 4);
		fenceDrawGuide.Add(1510, 2);
		fenceDrawGuide.Add(1110, 7);
		fenceDrawGuide.Add(610, 4);
	}

	public virtual void PerformRepairIfNecessary()
	{
		if (Game1.IsMasterGame && repairQueued.Value)
		{
			ResetHealth(GetRepairHealthAdjustment());
			repairQueued.Value = false;
		}
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		PerformRepairIfNecessary();
		int gatePosition = this.gatePosition.Get();
		gatePosition += gateMotion;
		if (gatePosition == 88)
		{
			int drawSum = getDrawSum();
			if (drawSum != 110 && drawSum != 1500 && drawSum != 1000 && drawSum != 500 && drawSum != 100 && drawSum != 10)
			{
				toggleGate(Game1.player, open: false);
			}
		}
		this.gatePosition.Set(gatePosition);
		if (gatePosition >= 88 || gatePosition <= 0)
		{
			gateMotion = 0;
		}
		heldObject.Get()?.updateWhenCurrentLocation(time);
	}

	public static Dictionary<string, FenceData> GetFenceLookup()
	{
		if (_FenceLookup == null)
		{
			_LoadFenceData();
		}
		return _FenceLookup;
	}

	/// <summary>Get the fence's data from <c>Data/Fences</c>, if found.</summary>
	public FenceData GetData()
	{
		if (_data == null)
		{
			TryGetData(base.ItemId, out _data);
		}
		return _data;
	}

	/// <summary>Try to get a fence's data from <c>Data/Fences</c>.</summary>
	/// <param name="itemId">The fence's unqualified item ID (i.e. the key in <c>Data/Fences</c>).</param>
	/// <param name="data">The fence data, if found.</param>
	/// <returns>Returns whether the fence data was found.</returns>
	public static bool TryGetData(string itemId, out FenceData data)
	{
		if (itemId == null)
		{
			data = null;
			return false;
		}
		return GetFenceLookup().TryGetValue(itemId, out data);
	}

	protected static void _LoadFenceData()
	{
		_FenceLookup = DataLoader.Fences(Game1.content);
	}

	public int getDrawSum()
	{
		GameLocation location = Location;
		if (location == null)
		{
			return 0;
		}
		int drawSum = 0;
		Vector2 surroundingLocations = tileLocation.Value;
		surroundingLocations.X += 1f;
		if (location.objects.TryGetValue(surroundingLocations, out var rightObj) && rightObj is Fence rightFence && rightFence.countsForDrawing(base.ItemId))
		{
			drawSum += 100;
		}
		surroundingLocations.X -= 2f;
		if (location.objects.TryGetValue(surroundingLocations, out var leftObj) && leftObj is Fence leftFence && leftFence.countsForDrawing(base.ItemId))
		{
			drawSum += 10;
		}
		surroundingLocations.X += 1f;
		surroundingLocations.Y += 1f;
		if (location.objects.TryGetValue(surroundingLocations, out var downObj) && downObj is Fence downFence && downFence.countsForDrawing(base.ItemId))
		{
			drawSum += 500;
		}
		surroundingLocations.Y -= 2f;
		if (location.objects.TryGetValue(surroundingLocations, out var upObj) && upObj is Fence upFence && upFence.countsForDrawing(base.ItemId))
		{
			drawSum += 1000;
		}
		return drawSum;
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		GameLocation location = Location;
		if (location == null)
		{
			return false;
		}
		if (!justCheckingForActivity && who != null)
		{
			Point playerTile = who.TilePoint;
			if (location.objects.ContainsKey(new Vector2(playerTile.X, playerTile.Y - 1)) && location.objects.ContainsKey(new Vector2(playerTile.X, playerTile.Y + 1)) && location.objects.ContainsKey(new Vector2(playerTile.X + 1, playerTile.Y)) && location.objects.ContainsKey(new Vector2(playerTile.X - 1, playerTile.Y)) && !location.objects[new Vector2(playerTile.X, playerTile.Y - 1)].isPassable() && !location.objects[new Vector2(playerTile.X, playerTile.Y - 1)].isPassable() && !location.objects[new Vector2(playerTile.X - 1, playerTile.Y)].isPassable() && !location.objects[new Vector2(playerTile.X + 1, playerTile.Y)].isPassable())
			{
				performToolAction(null);
			}
		}
		if (health.Value <= 1f)
		{
			return false;
		}
		if ((bool)isGate)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if ((bool)isGate)
			{
				toggleGate(who, (int)gatePosition == 0);
			}
			return true;
		}
		if (justCheckingForActivity)
		{
			return false;
		}
		foreach (Vector2 v in Utility.getAdjacentTileLocations(tileLocation.Value))
		{
			if (location.objects.TryGetValue(v, out var obj) && obj is Fence fence && (bool)fence.isGate)
			{
				fence.checkForAction(who);
				return true;
			}
		}
		return health.Value <= 0f;
	}

	public virtual void toggleGate(bool open, bool is_toggling_counterpart = false, Farmer who = null)
	{
		if (health.Value <= 1f)
		{
			return;
		}
		GameLocation location = Location;
		if (location == null)
		{
			return;
		}
		int drawSum = getDrawSum();
		if (drawSum == 110 || drawSum == 1500 || drawSum == 1000 || drawSum == 500 || drawSum == 100 || drawSum == 10)
		{
			who?.TemporaryPassableTiles.Add(new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64));
			if (open)
			{
				gatePosition.Value = 88;
			}
			else
			{
				gatePosition.Value = 0;
			}
			if (!is_toggling_counterpart)
			{
				location?.playSound("doorClose");
			}
		}
		else
		{
			who?.TemporaryPassableTiles.Add(new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64));
			gatePosition.Value = 0;
		}
		if (is_toggling_counterpart)
		{
			return;
		}
		switch (drawSum)
		{
		case 100:
		{
			Vector2 neighborTile = tileLocation.Value + new Vector2(-1f, 0f);
			if (location.objects.TryGetValue(neighborTile, out var neighbor) && neighbor is Fence fence && (bool)fence.isGate && fence.getDrawSum() == 10)
			{
				fence.toggleGate((int)gatePosition != 0, is_toggling_counterpart: true, who);
			}
			break;
		}
		case 10:
		{
			Vector2 neighborTile = tileLocation.Value + new Vector2(1f, 0f);
			if (location.objects.TryGetValue(neighborTile, out var neighbor) && neighbor is Fence fence && (bool)fence.isGate && fence.getDrawSum() == 100)
			{
				fence.toggleGate((int)gatePosition != 0, is_toggling_counterpart: true, who);
			}
			break;
		}
		case 1000:
		{
			Vector2 neighborTile = tileLocation.Value + new Vector2(0f, 1f);
			if (location.objects.TryGetValue(neighborTile, out var neighbor) && neighbor is Fence fence && (bool)fence.isGate && fence.getDrawSum() == 500)
			{
				fence.toggleGate((int)gatePosition != 0, is_toggling_counterpart: true, who);
			}
			break;
		}
		case 500:
		{
			Vector2 neighborTile = tileLocation.Value + new Vector2(0f, -1f);
			if (location.objects.TryGetValue(neighborTile, out var neighbor) && neighbor is Fence fence && (bool)fence.isGate && fence.getDrawSum() == 1000)
			{
				fence.toggleGate((int)gatePosition != 0, is_toggling_counterpart: true, who);
			}
			break;
		}
		}
	}

	public void toggleGate(Farmer who, bool open, bool is_toggling_counterpart = false)
	{
		toggleGate(open, is_toggling_counterpart, who);
	}

	public override void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
	{
		location.debris.Add(new Debris(base.ItemId, origin, destination));
	}

	public override bool performToolAction(Tool t)
	{
		GameLocation location = Location;
		if (heldObject.Value != null && t != null && !(t is MeleeWeapon) && t.isHeavyHitter())
		{
			Object value = heldObject.Value;
			heldObject.Value.performRemoveAction();
			heldObject.Value = null;
			Game1.createItemDebris(value.getOne(), TileLocation * 64f, -1);
			playNearbySoundAll("axchop");
		}
		else if (isGate.Value && (t is Axe || t is Pickaxe))
		{
			playNearbySoundAll("axchop");
			Game1.createObjectDebris("(O)325", (int)tileLocation.X, (int)tileLocation.Y, Game1.player.UniqueMultiplayerID, location);
			location.objects.Remove(tileLocation.Value);
			Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, 6, resource: false);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
		}
		else if (!isGate.Value && IsValidRemovalTool(t))
		{
			FenceData data = GetData();
			string sound = data?.RemovalSound ?? data?.PlacementSound ?? "hammer";
			int removalDebrisType = data?.RemovalDebrisType ?? 14;
			playNearbySoundAll(sound);
			location.objects.Remove(tileLocation.Value);
			for (int i = 0; i < 4; i++)
			{
				location.temporarySprites.Add(new CosmeticDebris(fenceTexture.Value, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), (float)Game1.random.Next(-5, 5) / 100f, (float)Game1.random.Next(-64, 64) / 30f, (float)Game1.random.Next(-800, -100) / 100f, (int)((tileLocation.Y + 1f) * 64f), new Rectangle(32 + Game1.random.Next(2) * 16 / 2, 96 + Game1.random.Next(2) * 16 / 2, 8, 8), Color.White, Game1.soundBank.GetCue("shiny4"), null, 0, 200));
			}
			Game1.createRadialDebris(location, removalDebrisType, (int)tileLocation.X, (int)tileLocation.Y, 6, resource: false);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f));
			if (maxHealth.Value - health.Value < 0.5f)
			{
				location.debris.Add(new Debris(new Object(base.ItemId, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
		}
		return false;
	}

	/// <summary>Get whether a tool can be used to break this fence.</summary>
	/// <param name="tool">The tool instance to check.</param>
	public virtual bool IsValidRemovalTool(Tool tool)
	{
		if (tool == null)
		{
			return !isGate.Value;
		}
		FenceData data = GetData();
		List<string> removalToolIds = data?.RemovalToolIds;
		List<string> removalToolTypes = data?.RemovalToolTypes;
		bool allowAnyTool = true;
		if (removalToolIds != null && removalToolIds.Count > 0)
		{
			allowAnyTool = false;
			string toolName = tool.BaseName;
			foreach (string requiredName in removalToolIds)
			{
				if (toolName == requiredName)
				{
					return true;
				}
			}
		}
		if (removalToolTypes != null && removalToolTypes.Count > 0)
		{
			allowAnyTool = false;
			string toolType = tool.GetType().FullName;
			foreach (string requiredType in removalToolTypes)
			{
				if (toolType == requiredType)
				{
					return true;
				}
			}
		}
		return allowAnyTool;
	}

	/// <inheritdoc />
	public override bool minutesElapsed(int minutes)
	{
		if (!Game1.IsMasterGame)
		{
			return false;
		}
		PerformRepairIfNecessary();
		if (!Game1.IsBuildingConstructed("Gold Clock") || Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
		{
			health.Value -= (float)minutes / 1440f;
			if (health.Value <= -1f && (Game1.timeOfDay <= 610 || Game1.timeOfDay > 1800))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntry()
	{
		base.actionOnPlayerEntry();
		if (heldObject.Value != null)
		{
			heldObject.Value.TileLocation = tileLocation.Value;
			heldObject.Value.Location = Location;
			heldObject.Value.actionOnPlayerEntry();
			heldObject.Value.isOn.Value = true;
			heldObject.Value.initializeLightSource(tileLocation.Value);
		}
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		GameLocation location = Location;
		if (location == null)
		{
			return false;
		}
		if (dropInItem.HasTypeObject() && dropInItem.ItemId == "325")
		{
			if (probe)
			{
				return false;
			}
			if (!isGate)
			{
				int drawSum = getDrawSum();
				if (drawSum == 1500 || drawSum == 110 || drawSum == 1000 || drawSum == 10 || drawSum == 100 || drawSum == 500)
				{
					Vector2 neighbor = default(Vector2);
					switch (drawSum)
					{
					case 10:
						neighbor = tileLocation.Value + new Vector2(1f, 0f);
						if (location.objects.ContainsKey(neighbor) && location.objects[neighbor] is Fence && (bool)((Fence)location.objects[neighbor]).isGate)
						{
							int neighbor_sum = ((Fence)location.objects[neighbor]).getDrawSum();
							if (neighbor_sum != 100 && neighbor_sum != 110)
							{
								return false;
							}
						}
						break;
					case 100:
						neighbor = tileLocation.Value + new Vector2(-1f, 0f);
						if (location.objects.ContainsKey(neighbor) && location.objects[neighbor] is Fence && (bool)((Fence)location.objects[neighbor]).isGate)
						{
							int neighbor_sum = ((Fence)location.objects[neighbor]).getDrawSum();
							if (neighbor_sum != 10 && neighbor_sum != 110)
							{
								return false;
							}
						}
						break;
					case 1000:
						neighbor = tileLocation.Value + new Vector2(0f, 1f);
						if (location.objects.ContainsKey(neighbor) && location.objects[neighbor] is Fence && (bool)((Fence)location.objects[neighbor]).isGate)
						{
							int neighbor_sum = ((Fence)location.objects[neighbor]).getDrawSum();
							if (neighbor_sum != 500 && neighbor_sum != 1500)
							{
								return false;
							}
						}
						break;
					case 500:
						neighbor = tileLocation.Value + new Vector2(0f, -1f);
						if (location.objects.ContainsKey(neighbor) && location.objects[neighbor] is Fence && (bool)((Fence)location.objects[neighbor]).isGate)
						{
							int neighbor_sum = ((Fence)location.objects[neighbor]).getDrawSum();
							if (neighbor_sum != 1000 && neighbor_sum != 1500)
							{
								return false;
							}
						}
						break;
					}
					foreach (Vector2 adjacent_tile in new List<Vector2>
					{
						tileLocation.Value + new Vector2(1f, 0f),
						tileLocation.Value + new Vector2(-1f, 0f),
						tileLocation.Value + new Vector2(0f, -1f),
						tileLocation.Value + new Vector2(0f, 1f)
					})
					{
						if (!(adjacent_tile == neighbor) && location.objects.TryGetValue(adjacent_tile, out var adjacent) && adjacent is Fence fence && (bool)fence.isGate && fence.Type == base.Type)
						{
							return false;
						}
					}
					if (heldObject.Value != null)
					{
						Object value = heldObject.Value;
						heldObject.Value.performRemoveAction();
						heldObject.Value = null;
						Game1.createItemDebris(value.getOne(), TileLocation * 64f, -1);
					}
					isGate.Value = true;
					if (TryGetData("325", out var gateData))
					{
						location.playSound(gateData.PlacementSound);
					}
					return true;
				}
			}
		}
		else if (dropInItem.QualifiedItemId == "(O)93" && heldObject.Value == null && !isGate)
		{
			if (!probe)
			{
				heldObject.Value = new Torch();
				location.playSound("axe");
				heldObject.Value.Location = Location;
				heldObject.Value.initializeLightSource(tileLocation.Value);
			}
			return true;
		}
		if (health.Value <= 1f && !repairQueued.Value && CanRepairWithThisItem(dropInItem))
		{
			if (!probe)
			{
				string repair_sound = GetRepairSound();
				if (!string.IsNullOrEmpty(repair_sound))
				{
					location.playSound(repair_sound);
				}
				repairQueued.Value = true;
			}
			return true;
		}
		return base.performObjectDropInAction(dropInItem, probe, who, returnFalseIfItemConsumed);
	}

	public virtual float GetRepairHealthAdjustment()
	{
		FenceData data = GetData();
		if (data == null)
		{
			return 0f;
		}
		return Utility.RandomFloat(data.RepairHealthAdjustmentMinimum, data.RepairHealthAdjustmentMaximum);
	}

	public virtual string GetRepairSound()
	{
		return GetData()?.PlacementSound ?? "";
	}

	public virtual bool CanRepairWithThisItem(Item item)
	{
		if (health.Value > 1f)
		{
			return false;
		}
		if (item == null)
		{
			return false;
		}
		return item.QualifiedItemId == base.QualifiedItemId;
	}

	/// <inheritdoc />
	public override bool performDropDownAction(Farmer who)
	{
		return false;
	}

	public virtual Texture2D loadFenceTexture()
	{
		if (base.ItemId == "325")
		{
			isGate.Value = true;
		}
		FenceData data = GetData();
		if (data == null)
		{
			return ItemRegistry.RequireTypeDefinition(TypeDefinitionId).GetErrorTexture();
		}
		return Game1.content.Load<Texture2D>(data.Texture);
	}

	public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
	{
		spriteBatch.Draw(fenceTexture.Value, objectPosition - new Vector2(0f, 64f), new Rectangle(5 * fencePieceWidth % fenceTexture.Value.Bounds.Width, 5 * fencePieceWidth / fenceTexture.Value.Bounds.Width * fencePieceHeight, fencePieceWidth, fencePieceHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)(f.StandingPixel.Y + 1) / 10000f);
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scale, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		location.Y -= 64f * scale;
		int drawSum = getDrawSum();
		int sourceRectPosition = fenceDrawGuide[drawSum];
		if ((bool)isGate)
		{
			switch (drawSum)
			{
			case 110:
				spriteBatch.Draw(fenceTexture.Value, location + new Vector2(6f, 6f), new Rectangle(0, 512, 88, 24), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				return;
			case 1500:
				spriteBatch.Draw(fenceTexture.Value, location + new Vector2(6f, 6f), new Rectangle(112, 512, 16, 64), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				return;
			}
		}
		spriteBatch.Draw(fenceTexture.Value, location + new Vector2(32f, 32f) * scale, Game1.getArbitrarySourceRect(fenceTexture.Value, 64, 128, sourceRectPosition), color * transparency, 0f, new Vector2(32f, 32f) * scale, scale, SpriteEffects.None, layerDepth);
	}

	public bool countsForDrawing(string otherItemId)
	{
		if ((health.Value > 1f || repairQueued.Value) && !isGate)
		{
			if (!(otherItemId == base.ItemId))
			{
				return otherItemId == "325";
			}
			return true;
		}
		return false;
	}

	public override bool isPassable()
	{
		if ((bool)isGate)
		{
			return (int)gatePosition >= 88;
		}
		return false;
	}

	public override void draw(SpriteBatch b, int x, int y, float alpha = 1f)
	{
		int sourceRectPosition = 1;
		FenceData data = GetData();
		if (data == null)
		{
			IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition(TypeDefinitionId);
			b.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), itemType.GetErrorSourceRect(), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f);
			return;
		}
		if (health.Value > 1f || repairQueued.Value)
		{
			int drawSum = getDrawSum();
			sourceRectPosition = fenceDrawGuide[drawSum];
			if ((bool)isGate)
			{
				Vector2 offset = new Vector2(0f, 0f);
				switch (drawSum)
				{
				case 10:
					b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 - 16, y * 64 - 128)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 192, 24, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
					return;
				case 100:
					b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 - 16, y * 64 - 128)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 240, 24, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
					return;
				case 1000:
					b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 288, 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
					return;
				case 500:
					b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 320, 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
					return;
				case 110:
					b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 - 16, y * 64 - 64)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 128, 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
					return;
				case 1500:
					b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle(((int)gatePosition == 88) ? 16 : 0, 160, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
					b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 + 44)), new Rectangle(((int)gatePosition == 88) ? 16 : 0, 176, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
					return;
				}
				sourceRectPosition = 17;
			}
			else if (heldObject.Value != null)
			{
				Vector2 offset = Vector2.Zero;
				offset += data.HeldObjectDrawOffset;
				switch (drawSum)
				{
				case 10:
					offset.X = data.RightEndHeldObjectDrawX;
					break;
				case 100:
					offset.X = data.LeftEndHeldObjectDrawX;
					break;
				}
				offset *= 4f;
				heldObject.Value.draw(b, x * 64 + (int)offset.X, y * 64 + (int)offset.Y, (float)(y * 64 + 64) / 10000f, 1f);
			}
		}
		b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64)), new Rectangle(sourceRectPosition * fencePieceWidth % fenceTexture.Value.Bounds.Width, sourceRectPosition * fencePieceWidth / fenceTexture.Value.Bounds.Width * fencePieceHeight, fencePieceWidth, fencePieceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32) / 10000f);
	}
}
