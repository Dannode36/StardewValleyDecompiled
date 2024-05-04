using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class FishShop : ShopLocation
{
	public FishShop()
	{
	}

	public FishShop(string map, string name)
		: base(map, name)
	{
	}

	public override Dialogue getPurchasedItemDialogueForNPC(Object i, NPC n)
	{
		Dialogue response = null;
		string[] split = Game1.content.LoadString("Strings\\Lexicon:GenericPlayerTerm").Split('^');
		string genderName = split[0];
		if (split.Length > 1 && !Game1.player.IsMale)
		{
			genderName = split[1];
		}
		string whatToCallPlayer = ((Game1.random.NextDouble() < (double)(Game1.player.getFriendshipLevelForNPC(n.Name) / 1250)) ? Game1.player.Name : genderName);
		if (n.Age != 0)
		{
			whatToCallPlayer = Game1.player.Name;
		}
		string particle = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? Lexicon.getProperArticleForWord(i.name) : "");
		if ((i.Category == -4 || i.Category == -75 || i.Category == -79) && Game1.random.NextBool())
		{
			particle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SeedShop.cs.9701");
		}
		int whichDialogue = Game1.random.Next(5);
		if (n.Manners == 2)
		{
			whichDialogue = 2;
		}
		switch (whichDialogue)
		{
		case 0:
		case 4:
			response = ((!(Game1.random.NextDouble() < (double)(int)i.quality * 0.5 + 0.2)) ? Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_1_QualityLow_Willy", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomNegativeFoodAdjective(n)) : Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_1_QualityHigh_Willy", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomDeliciousAdjective(n)));
			break;
		case 1:
			response = (((int)i.quality != 0) ? ((!n.Name.Equals("Jodi")) ? Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Willy", whatToCallPlayer, particle, i.DisplayName) : Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Jodi_Willy", whatToCallPlayer, particle, i.DisplayName)) : Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityLow_Willy", whatToCallPlayer, particle, i.DisplayName));
			break;
		case 2:
			response = ((n.Manners != 2) ? Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_NonRude_Willy", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2) : (((int)i.quality >= 2) ? Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_QualityHigh_Rude_Willy", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomSlightlyPositiveAdjectiveForEdibleNoun(n)) : Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_QualityLow_Rude_Willy", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomNegativeFoodAdjective(n), Lexicon.getRandomNegativeItemSlanderNoun())));
			break;
		case 3:
			response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_4_Willy", whatToCallPlayer, particle, i.DisplayName);
			break;
		}
		if (n.Name == "Willy")
		{
			string key = (((int)i.quality == 0) ? "Data\\ExtraDialogue:PurchasedItem_Pierre_QualityLow_Willy" : "Data\\ExtraDialogue:PurchasedItem_Pierre_QualityHigh_Willy");
			response = Dialogue.FromTranslation(n, key, whatToCallPlayer, particle, i.DisplayName);
		}
		return response ?? new Dialogue(n, null, "...");
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (ArgUtility.Get(action, 0) == "WarpBoatTunnel")
		{
			if (Game1.player.mailReceived.Contains("willyBackRoomInvitation"))
			{
				Game1.warpFarmer("BoatTunnel", 6, 12, flip: false);
				playSound("doorClose");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
			}
		}
		return base.performAction(action, who, tileLocation);
	}
}
