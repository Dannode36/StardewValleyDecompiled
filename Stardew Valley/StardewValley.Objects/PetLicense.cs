using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Pets;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Objects;

public class PetLicense : Object
{
	/// <summary>The delimiter between the pet ID and breed ID in the <see cref="P:StardewValley.Object.Name" /> field.</summary>
	public const char Delimiter = '|';

	public PetLicense()
		: base("PetLicense", 1)
	{
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		if (drawShadow && !bigCraftable.Value && base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)SeedSpot")
		{
			spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
		}
		ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		float drawnScale = scaleSize;
		if (bigCraftable.Value && drawnScale > 0.2f)
		{
			drawnScale /= 2f;
		}
		string[] split = Name.Split('|');
		if (Game1.petData.TryGetValue(split[0], out var petData))
		{
			PetBreed breed = petData.GetBreedById(split[1]);
			if (breed != null)
			{
				Rectangle sourceRect = breed.IconSourceRect;
				spriteBatch.Draw(Game1.content.Load<Texture2D>(breed.IconTexture), location + new Vector2(32f, 32f), sourceRect, color * transparency, 0f, new Vector2(sourceRect.Width / 2, sourceRect.Height / 2), 4f * drawnScale, SpriteEffects.None, layerDepth);
			}
		}
		DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public override bool actionWhenPurchased(string shopId)
	{
		Game1.exitActiveMenu();
		string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
		Game1.activeClickableMenu = new NamingMenu(namePet, title, Dialogue.randomName());
		Game1.playSound("purchaseClick");
		return true;
	}

	private void namePet(string name)
	{
		string[] split = Name.Split('|');
		FarmHouse home = Utility.getHomeOfFarmer(Game1.player);
		Point petTile = new Point(3, 7);
		if (home.upgradeLevel == 1)
		{
			petTile = new Point(9, 7);
		}
		else if (home.upgradeLevel >= 2)
		{
			petTile = new Point(27, 26);
		}
		Pet p = new Pet(petTile.X, petTile.Y, split[1], split[0]);
		p.currentLocation = home;
		home.characters.Add(p);
		p.warpToFarmHouse(Game1.player);
		p.Name = name;
		p.displayName = p.name;
		foreach (Building building in Game1.getFarm().buildings)
		{
			if (building is PetBowl bowl && !bowl.HasPet())
			{
				bowl.AssignPet(p);
				break;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			allFarmer.autoGenerateActiveDialogueEvent("gotPet");
		}
		Game1.exitActiveMenu();
		if (Game1.currentLocation.getCharacterFromName("Marnie") != null)
		{
			Game1.DrawDialogue(Game1.currentLocation.getCharacterFromName("Marnie"), "Strings\\1_6_Strings:AdoptedPet_Marnie", name);
		}
		else
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:AdoptedPet", name));
		}
	}
}
