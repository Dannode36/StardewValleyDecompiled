using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace StardewValley.Buildings;

public class JunimoHut : Building
{
	public int cropHarvestRadius = 8;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="M:StardewValley.Buildings.JunimoHut.GetOutputChest" /> instead.</summary>
	[XmlElement("output")]
	public Chest obsolete_output;

	[XmlElement("noHarvest")]
	public readonly NetBool noHarvest = new NetBool();

	[XmlElement("wasLit")]
	public readonly NetBool wasLit = new NetBool(value: false);

	private int junimoSendOutTimer;

	[XmlIgnore]
	public List<JunimoHarvester> myJunimos = new List<JunimoHarvester>();

	[XmlIgnore]
	public Point lastKnownCropLocation = Point.Zero;

	public NetInt raisinDays = new NetInt();

	[XmlElement("shouldSendOutJunimos")]
	public NetBool shouldSendOutJunimos = new NetBool(value: false);

	private Rectangle lightInteriorRect = new Rectangle(195, 0, 18, 17);

	private Rectangle bagRect = new Rectangle(208, 51, 15, 13);

	public JunimoHut(Vector2 tileLocation)
		: base("Junimo Hut", tileLocation)
	{
	}

	public JunimoHut()
		: this(Vector2.Zero)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(noHarvest, "noHarvest").AddField(wasLit, "wasLit").AddField(shouldSendOutJunimos, "shouldSendOutJunimos")
			.AddField(raisinDays, "raisinDays");
		wasLit.fieldChangeVisibleEvent += delegate
		{
			updateLightState();
		};
	}

	public override Rectangle getRectForAnimalDoor(BuildingData data)
	{
		return new Rectangle((1 + (int)tileX) * 64, ((int)tileY + 1) * 64, 64, 64);
	}

	public override Rectangle? getSourceRectForMenu()
	{
		return new Rectangle(Game1.GetSeasonIndexForLocation(GetParentLocation()) * 48, 0, 48, 64);
	}

	public Chest GetOutputChest()
	{
		return GetBuildingChest("Output");
	}

	public override void dayUpdate(int dayOfMonth)
	{
		base.dayUpdate(dayOfMonth);
		myJunimos.Clear();
		wasLit.Value = false;
		shouldSendOutJunimos.Value = true;
		if ((int)raisinDays > 0 && !Game1.IsWinter)
		{
			raisinDays.Value--;
		}
		if ((int)raisinDays == 0 && !Game1.IsWinter)
		{
			Chest output = GetOutputChest();
			if (output.Items.CountId("(O)Raisins") > 0)
			{
				raisinDays.Value += 7;
				output.Items.ReduceId("(O)Raisins", 1);
			}
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.isActive() && f.currentLocation != null && (f.currentLocation is FarmHouse || f.currentLocation.isStructure.Value))
			{
				shouldSendOutJunimos.Value = false;
			}
		}
	}

	public void sendOutJunimos()
	{
		junimoSendOutTimer = 1000;
	}

	/// <inheritdoc />
	public override void performActionOnConstruction(GameLocation location, Farmer who)
	{
		base.performActionOnConstruction(location, who);
		sendOutJunimos();
	}

	public override void resetLocalState()
	{
		base.resetLocalState();
		updateLightState();
	}

	public void updateLightState()
	{
		if (!IsInCurrentLocation())
		{
			return;
		}
		if (wasLit.Value)
		{
			if (Utility.getLightSource((int)tileX + (int)tileY * 777) == null)
			{
				Game1.currentLightSources.Add(new LightSource(4, new Vector2((int)tileX + 1, (int)tileY + 1) * 64f + new Vector2(32f, 32f), 0.5f, LightSource.LightContext.None, 0L)
				{
					Identifier = (int)tileX + (int)tileY * 777
				});
			}
			AmbientLocationSounds.addSound(new Vector2((int)tileX + 1, (int)tileY + 1), 1);
		}
		else
		{
			Utility.removeLightSource((int)tileX + (int)tileY * 777);
			AmbientLocationSounds.removeSound(new Vector2((int)tileX + 1, (int)tileY + 1));
		}
	}

	public int getUnusedJunimoNumber()
	{
		for (int i = 0; i < 3; i++)
		{
			if (i >= myJunimos.Count)
			{
				return i;
			}
			bool found = false;
			foreach (JunimoHarvester myJunimo in myJunimos)
			{
				if (myJunimo.whichJunimoFromThisHut == i)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				return i;
			}
		}
		return 2;
	}

	public override void updateWhenFarmNotCurrentLocation(GameTime time)
	{
		base.updateWhenFarmNotCurrentLocation(time);
		GameLocation location = GetParentLocation();
		Chest output = GetOutputChest();
		if (output != null && output.mutex != null)
		{
			output.mutex.Update(location);
			if (output.mutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				output.mutex.ReleaseLock();
			}
		}
		if (!Game1.IsMasterGame || junimoSendOutTimer <= 0 || !shouldSendOutJunimos.Value)
		{
			return;
		}
		junimoSendOutTimer -= time.ElapsedGameTime.Milliseconds;
		if (junimoSendOutTimer > 0 || myJunimos.Count >= 3 || location.IsWinterHere() || location.IsRainingHere() || !areThereMatureCropsWithinRadius() || (!(location.NameOrUniqueName != "Farm") && Game1.farmEvent != null))
		{
			return;
		}
		int junimoNumber = getUnusedJunimoNumber();
		bool isPrismatic = false;
		Color? gemColor = getGemColor(ref isPrismatic);
		JunimoHarvester j = new JunimoHarvester(location, new Vector2((int)tileX + 1, (int)tileY + 1) * 64f + new Vector2(0f, 32f), this, junimoNumber, gemColor);
		j.isPrismatic.Value = isPrismatic;
		location.characters.Add(j);
		myJunimos.Add(j);
		junimoSendOutTimer = 1000;
		if (Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2((int)tileX + 1, (int)tileY + 1)), 64, location))
		{
			try
			{
				location.playSound("junimoMeep1");
			}
			catch (Exception)
			{
			}
		}
	}

	public override void Update(GameTime time)
	{
		if (!shouldSendOutJunimos.Value)
		{
			shouldSendOutJunimos.Value = true;
		}
		base.Update(time);
	}

	private Color? getGemColor(ref bool isPrismatic)
	{
		List<Color> gemColors = new List<Color>();
		foreach (Item item in GetOutputChest().Items)
		{
			if (item != null && (item.Category == -12 || item.Category == -2))
			{
				Color? gemColor = TailoringMenu.GetDyeColor(item);
				if (item.QualifiedItemId == "(O)74")
				{
					isPrismatic = true;
				}
				if (gemColor.HasValue)
				{
					gemColors.Add(gemColor.Value);
				}
			}
		}
		if (gemColors.Count > 0)
		{
			return gemColors[Game1.random.Next(gemColors.Count)];
		}
		return null;
	}

	public bool areThereMatureCropsWithinRadius()
	{
		GameLocation location = GetParentLocation();
		for (int x = (int)tileX + 1 - cropHarvestRadius; x < (int)tileX + 2 + cropHarvestRadius; x++)
		{
			for (int y = (int)tileY - cropHarvestRadius + 1; y < (int)tileY + 2 + cropHarvestRadius; y++)
			{
				if (location.terrainFeatures.TryGetValue(new Vector2(x, y), out var terrainFeature))
				{
					if (location.isCropAtTile(x, y) && ((HoeDirt)terrainFeature).readyForHarvest())
					{
						lastKnownCropLocation = new Point(x, y);
						return true;
					}
					if (terrainFeature is Bush bush && (int)bush.tileSheetOffset == 1)
					{
						lastKnownCropLocation = new Point(x, y);
						return true;
					}
				}
			}
		}
		lastKnownCropLocation = Point.Zero;
		return false;
	}

	public override void performTenMinuteAction(int timeElapsed)
	{
		base.performTenMinuteAction(timeElapsed);
		GameLocation location = GetParentLocation();
		if (myJunimos.Count > 0)
		{
			for (int i = myJunimos.Count - 1; i >= 0; i--)
			{
				if (!location.characters.Contains(myJunimos[i]))
				{
					myJunimos.RemoveAt(i);
				}
				else
				{
					myJunimos[i].pokeToHarvest();
				}
			}
		}
		if (myJunimos.Count < 3 && Game1.timeOfDay < 1900)
		{
			junimoSendOutTimer = 1;
		}
		if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400)
		{
			if (!location.IsWinterHere() && Game1.random.NextDouble() < 0.2)
			{
				wasLit.Value = true;
			}
		}
		else if (Game1.timeOfDay == 2400 && !location.IsWinterHere())
		{
			wasLit.Value = false;
		}
	}

	public override bool doAction(Vector2 tileLocation, Farmer who)
	{
		if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
		{
			return false;
		}
		if (occupiesTile(tileLocation))
		{
			Chest output = GetOutputChest();
			output.mutex.RequestLock(delegate
			{
				Game1.activeClickableMenu = new ItemGrabMenu(output.Items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, output.grabItemFromInventory, null, output.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, 1, this);
			});
			return true;
		}
		return base.doAction(tileLocation, who);
	}

	public override void drawInMenu(SpriteBatch b, int x, int y)
	{
		drawShadow(b, x, y);
		b.Draw(texture.Value, new Vector2(x, y), new Rectangle(0, 0, 48, 64), color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
	}

	public override void draw(SpriteBatch b)
	{
		if (base.isMoving)
		{
			return;
		}
		if ((int)daysOfConstructionLeft > 0)
		{
			drawInConstruction(b);
			return;
		}
		drawShadow(b);
		Rectangle sourceRect = getSourceRectForMenu() ?? getSourceRect();
		b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), sourceRect, color * alpha, 0f, new Vector2(0f, texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64) / 10000f);
		if ((int)raisinDays > 0 && !Game1.IsWinter)
		{
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 12, (int)tileY * 64 + (int)tilesHigh * 64 + 20)), new Rectangle(246, 46, 10, 18), color * alpha, 0f, new Vector2(0f, 18f), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64 + 2) / 10000f);
		}
		bool containsOutput = false;
		Chest output = GetOutputChest();
		if (output != null)
		{
			foreach (Item item in output.Items)
			{
				if (item != null && item.Category != -12 && item.Category != -2)
				{
					containsOutput = true;
					break;
				}
			}
		}
		if (containsOutput)
		{
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 128 + 12, (int)tileY * 64 + (int)tilesHigh * 64 - 32)), bagRect, color * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64 + 1) / 10000f);
		}
		if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400 && wasLit.Value && !GetParentLocation().IsWinterHere())
		{
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 64, (int)tileY * 64 + (int)tilesHigh * 64 - 64)), lightInteriorRect, color * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64 + 1) / 10000f);
		}
	}
}
