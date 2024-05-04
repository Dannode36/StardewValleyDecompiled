using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.GameData.Buildings;
using StardewValley.Tools;

namespace StardewValley.Buildings;

public class PetBowl : Building
{
	/// <summary>Whether the pet bowl is full.</summary>
	[XmlElement("watered")]
	public readonly NetBool watered = new NetBool();

	private int nameTimer;

	private string nameTimerMessage;

	/// <summary>The pet to which this bowl belongs, if any.</summary>
	/// <remarks>When a pet is assigned, this matches <see cref="F:StardewValley.Characters.Pet.petId" />.</remarks>
	[XmlElement("petGuid")]
	public readonly NetGuid petId = new NetGuid();

	public PetBowl(Vector2 tileLocation)
		: base("Pet Bowl", tileLocation)
	{
	}

	public PetBowl()
		: this(Vector2.Zero)
	{
	}

	/// <summary>Assign a pet to this pet bowl.</summary>
	/// <param name="pet">The pet to assign.</param>
	public virtual void AssignPet(Pet pet)
	{
		petId.Value = pet.petId.Value;
		pet.homeLocationName.Value = parentLocationName.Value;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(watered, "watered").AddField(petId, "petId");
	}

	public virtual Point GetPetSpot()
	{
		return new Point(tileX, (int)tileY + 1);
	}

	public override bool doAction(Vector2 tileLocation, Farmer who)
	{
		if (!isTilePassable(tileLocation))
		{
			_ = petId.Value;
			Pet p = Utility.findPet(petId.Value);
			if (p != null)
			{
				nameTimer = 3500;
				nameTimerMessage = Game1.content.LoadString("Strings\\1_6_Strings:PetBowlName", p.displayName);
			}
		}
		return base.doAction(tileLocation, who);
	}

	public override void Update(GameTime time)
	{
		if (nameTimer > 0)
		{
			nameTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
		base.Update(time);
	}

	public override void performToolAction(Tool t, int tileX, int tileY)
	{
		if (t is WateringCan)
		{
			string value = null;
			if (doesTileHaveProperty(tileX, tileY, "PetBowl", "Buildings", ref value))
			{
				watered.Value = true;
			}
		}
		base.performToolAction(t, tileX, tileY);
	}

	/// <summary>Get whether any pet has been assigned to this pet bowl.</summary>
	public bool HasPet()
	{
		return petId.Value != Guid.Empty;
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (base.isMoving || isUnderConstruction())
		{
			return;
		}
		if (watered.Value)
		{
			BuildingData data = GetData();
			float sortY = ((int)tileY + (int)tilesHigh) * 64;
			if (data != null)
			{
				sortY -= data.SortTileOffset * 64f;
			}
			sortY += 1.5f;
			sortY /= 10000f;
			Vector2 drawPosition = new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64);
			Vector2 drawOffset = Vector2.Zero;
			if (data != null)
			{
				drawOffset = data.DrawOffset * 4f;
			}
			Rectangle sourceRect = getSourceRect();
			sourceRect.X += sourceRect.Width;
			b.Draw(origin: new Vector2(0f, sourceRect.Height), texture: texture.Value, position: Game1.GlobalToLocal(Game1.viewport, drawPosition + drawOffset), sourceRectangle: sourceRect, color: color * alpha, rotation: 0f, scale: 4f, effects: SpriteEffects.None, layerDepth: sortY);
		}
		if (nameTimer > 0)
		{
			BuildingData data = GetData();
			float sortY = ((int)tileY + (int)tilesHigh) * 64;
			if (data != null)
			{
				sortY -= data.SortTileOffset * 64f;
			}
			sortY += 1.5f;
			sortY /= 10000f;
			SpriteText.drawSmallTextBubble(b, nameTimerMessage, Game1.GlobalToLocal(new Vector2(((float)(int)tileX + 1.5f) * 64f, (int)tileY * 64 - 32)), -1, sortY + 1E-06f);
		}
	}
}
