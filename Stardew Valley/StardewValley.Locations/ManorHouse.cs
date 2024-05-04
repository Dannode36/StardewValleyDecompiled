using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.SpecialOrders;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class ManorHouse : GameLocation
{
	[XmlIgnore]
	private Dictionary<string, Farmer> sendMoneyMapping = new Dictionary<string, Farmer>();

	private static readonly bool changeWalletTypeImmediately;

	public ManorHouse()
	{
	}

	public ManorHouse(string mapPath, string name)
		: base(mapPath, name)
	{
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (who.IsLocalPlayer)
		{
			switch (ArgUtility.Get(action, 0))
			{
			case "LostAndFound":
				CheckLostAndFound();
				break;
			case "MayorFridge":
				if (who.Items.ContainsId("(O)284", 10) && !who.hasOrWillReceiveMail("TH_MayorFridge") && who.hasOrWillReceiveMail("TH_Railroad"))
				{
					who.Items.ReduceId("(O)284", 10);
					Game1.player.CanMove = false;
					localSound("coin");
					Game1.player.mailReceived.Add("TH_MayorFridge");
					Game1.multipleDialogues(new string[2]
					{
						Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_ConsumeBeets"),
						Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_MrQiNote")
					});
					Game1.player.removeQuest("3");
					Game1.player.addQuest("4");
				}
				else if (who.hasOrWillReceiveMail("TH_MayorFridge"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_MrQiNote"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_Initial"));
				}
				break;
			case "DivorceBook":
				if ((bool)Game1.player.divorceTonight)
				{
					string s = null;
					if (Game1.player.hasCurrentOrPendingRoommate())
					{
						s = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_CancelQuestion_Krobus", Game1.player.getSpouse().displayName);
					}
					if (s == null)
					{
						s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_CancelQuestion");
					}
					createQuestionDialogue(s, createYesNoResponses(), "divorceCancel");
				}
				else if (Game1.player.isMarriedOrRoommates())
				{
					string s = null;
					if (Game1.player.hasCurrentOrPendingRoommate())
					{
						s = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Question_Krobus", Game1.player.getSpouse().displayName);
					}
					if (s == null)
					{
						s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question");
					}
					createQuestionDialogue(s, createYesNoResponses(), "divorce");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_NoSpouse"));
				}
				break;
			case "LedgerBook":
				readLedgerBook();
				break;
			}
		}
		return base.performAction(action, who, tileLocation);
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (Game1.eventUp && Game1.CurrentEvent.id != "prizeTicketIntro")
		{
			removeTile(4, 5, "Buildings");
			removeTile(4, 4, "Front");
			removeTile(4, 3, "Front");
			setMapTile(4, 6, 635, "Back", null);
		}
		else
		{
			setMapTile(4, 5, 109, "Buildings", "LostAndFound", 2);
			setMapTile(4, 4, 77, "Front", null, 2);
			setMapTile(4, 3, 110, "Front", null, 2);
			setMapTile(4, 6, 604, "Back", null);
		}
	}

	public void CheckLostAndFound()
	{
		string prompt = (SpecialOrder.IsSpecialOrdersBoardUnlocked() ? Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check_OrdersUnlocked") : Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check"));
		List<Response> choices = new List<Response>();
		if (Game1.player.team.returnedDonations.Count > 0 && !Game1.player.team.returnedDonationsMutex.IsLocked())
		{
			choices.Add(new Response("CheckDonations", Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_DonationItems")));
		}
		if (GetRetrievableFarmers().Count > 0)
		{
			choices.Add(new Response("RetrieveFarmhandItems", Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_FarmhandItems")));
		}
		if (choices.Count > 0)
		{
			choices.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
		}
		if (choices.Count > 0)
		{
			createQuestionDialogue(prompt, choices.ToArray(), "lostAndFound");
		}
		else
		{
			Game1.drawObjectDialogue(prompt);
		}
	}

	public List<Farmer> GetRetrievableFarmers()
	{
		List<Farmer> offline_farmers = new List<Farmer>(Game1.getAllFarmers());
		foreach (Farmer online_farmer in Game1.getOnlineFarmers())
		{
			offline_farmers.Remove(online_farmer);
		}
		for (int i = 0; i < offline_farmers.Count; i++)
		{
			Farmer farmer = offline_farmers[i];
			if (Utility.getHomeOfFarmer(farmer) is Cabin home && (farmer.isUnclaimedFarmhand || home.inventoryMutex.IsLocked()))
			{
				offline_farmers.RemoveAt(i);
				i--;
			}
		}
		return offline_farmers;
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (Game1.player.team.returnedDonations.Count > 0 && !Game1.eventUp)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2);
			Vector2 lost_and_found_indicator_position = new Vector2(4f, 4f) * 64f + new Vector2(7f, 0f) * 4f;
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(lost_and_found_indicator_position.X, lost_and_found_indicator_position.Y + yOffset)), new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
		}
	}

	private void readLedgerBook()
	{
		if (Game1.player.useSeparateWallets)
		{
			if (Game1.IsMasterGame)
			{
				List<Response> choices = new List<Response>();
				choices.Add(new Response("SendMoney", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SendMoney")));
				if ((bool)Game1.player.changeWalletTypeTonight)
				{
					choices.Add(new Response("CancelMerge", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_CancelMerge")));
				}
				else
				{
					choices.Add(new Response("MergeWallets", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_MergeWallets")));
				}
				choices.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_Leave")));
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_HostQuestion"), choices.ToArray(), "ledgerOptions");
			}
			else
			{
				ChooseRecipient();
			}
		}
		else if (!Game1.getAllFarmhands().Any())
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_Singleplayer"));
		}
		else if (Game1.IsMasterGame)
		{
			if ((bool)Game1.player.changeWalletTypeTonight)
			{
				string s = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_CancelQuestion");
				createQuestionDialogue(s, createYesNoResponses(), "cancelSeparateWallets");
			}
			else
			{
				string s = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_SeparateQuestion");
				createQuestionDialogue(s, createYesNoResponses(), "separateWallets");
			}
		}
		else
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_Client"));
		}
	}

	public void ShowOfflineFarmhandItemList()
	{
		List<Response> choices = new List<Response>();
		foreach (Farmer retrievableFarmer in GetRetrievableFarmers())
		{
			string key = retrievableFarmer.UniqueMultiplayerID.ToString() ?? "";
			string name = retrievableFarmer.Name;
			if (retrievableFarmer.Name == "")
			{
				name = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
			}
			choices.Add(new Response(key, name));
		}
		choices.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
		Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_FarmhandItemsQuestion"), choices.ToArray(), "CheckItems");
	}

	public void ChooseRecipient()
	{
		sendMoneyMapping.Clear();
		List<Response> otherFarmers = new List<Response>();
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID && !farmer.isUnclaimedFarmhand)
			{
				string key = "Transfer" + (otherFarmers.Count + 1);
				string farmerName = farmer.Name;
				if (farmer.Name == "")
				{
					farmerName = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
				}
				otherFarmers.Add(new Response(key, farmerName));
				sendMoneyMapping.Add(key, farmer);
			}
		}
		if (otherFarmers.Count == 0)
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_NoFarmhands"));
			return;
		}
		otherFarmers.Sort((Response x, Response y) => string.Compare(x.responseKey, y.responseKey));
		otherFarmers.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
		Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_TransferQuestion"), otherFarmers.ToArray(), "chooseRecipient");
	}

	private void beginSendMoney(Farmer recipient)
	{
		Game1.activeClickableMenu = new DigitEntryMenu(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_HowMuchQuestion"), delegate(int currentValue, int price, Farmer who)
		{
			sendMoney(recipient, currentValue);
		}, -1, 1, Game1.player.Money);
	}

	public void sendMoney(Farmer recipient, int amount)
	{
		Game1.playSound("smallSelect");
		Game1.player.Money -= amount;
		Game1.player.team.AddIndividualMoney(recipient, amount);
		Game1.player.stats.onMoneyGifted((uint)amount);
		if (amount == 1)
		{
			Game1.multiplayer.globalChatInfoMessage("Sent1g", Game1.player.Name, recipient.Name);
		}
		else
		{
			Game1.multiplayer.globalChatInfoMessage("SentMoney", Game1.player.Name, recipient.Name, TokenStringBuilder.NumberWithSeparators(amount));
		}
		Game1.exitActiveMenu();
	}

	public static void SeparateWallets()
	{
		if (Game1.player.useSeparateWallets || !Game1.IsMasterGame)
		{
			return;
		}
		Game1.player.changeWalletTypeTonight.Value = false;
		int totalMoney = Game1.player.Money;
		int farmerCount = 0;
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			if (!allFarmer.isUnclaimedFarmhand)
			{
				farmerCount++;
			}
		}
		int splitMoney = totalMoney / Math.Max(farmerCount, 1);
		Game1.player.team.useSeparateWallets.Value = true;
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (!farmer.isUnclaimedFarmhand)
			{
				Game1.player.team.SetIndividualMoney(farmer, splitMoney);
			}
		}
		Game1.multiplayer.globalChatInfoMessage("SeparatedWallets", Game1.player.Name, splitMoney.ToString());
	}

	public static void MergeWallets()
	{
		if (!Game1.player.useSeparateWallets || !Game1.IsMasterGame)
		{
			return;
		}
		Game1.player.changeWalletTypeTonight.Value = false;
		int totalMoney = 0;
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (!farmer.isUnclaimedFarmhand)
			{
				totalMoney += Game1.player.team.GetIndividualMoney(farmer);
			}
		}
		Game1.player.team.useSeparateWallets.Value = false;
		Game1.player.team.money.Value = totalMoney;
		Game1.multiplayer.globalChatInfoMessage("MergedWallets", Game1.player.Name);
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		string s = null;
		if (questionAndAnswer == null)
		{
			return false;
		}
		switch (questionAndAnswer)
		{
		case "divorceCancel_Yes":
			if ((bool)Game1.player.divorceTonight)
			{
				Game1.player.divorceTonight.Value = false;
				if (!Game1.player.hasRoommate())
				{
					Game1.player.addUnearnedMoney(50000);
				}
				if (Game1.player.hasCurrentOrPendingRoommate())
				{
					s = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Cancelled_Krobus", Game1.player.getSpouse().displayName);
				}
				if (s == null)
				{
					s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Cancelled");
				}
				Game1.drawObjectDialogue(s);
				if (!Game1.player.hasRoommate())
				{
					Game1.multiplayer.globalChatInfoMessage("DivorceCancel", Game1.player.Name);
				}
			}
			break;
		case "divorce_Yes":
			if (Game1.player.Money >= 50000 || Game1.player.hasCurrentOrPendingRoommate())
			{
				if (!Game1.player.hasRoommate())
				{
					Game1.player.Money -= 50000;
				}
				Game1.player.divorceTonight.Value = true;
				if (Game1.player.hasCurrentOrPendingRoommate())
				{
					s = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Filed_Krobus", Game1.player.getSpouse().displayName);
				}
				if (s == null)
				{
					s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
				}
				Game1.drawObjectDialogue(s);
				if (!Game1.player.hasRoommate())
				{
					Game1.multiplayer.globalChatInfoMessage("Divorce", Game1.player.Name);
				}
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			}
			break;
		case "separateWallets_Yes":
			if (changeWalletTypeImmediately)
			{
				SeparateWallets();
				break;
			}
			Game1.player.changeWalletTypeTonight.Value = true;
			Game1.multiplayer.globalChatInfoMessage("SeparateWallets", Game1.player.Name);
			break;
		case "cancelSeparateWallets_Yes":
			Game1.player.changeWalletTypeTonight.Value = false;
			Game1.multiplayer.globalChatInfoMessage("SeparateWalletsCancel", Game1.player.Name);
			break;
		case "mergeWallets_Yes":
			if (changeWalletTypeImmediately)
			{
				MergeWallets();
				break;
			}
			Game1.player.changeWalletTypeTonight.Value = true;
			Game1.multiplayer.globalChatInfoMessage("MergeWallets", Game1.player.Name);
			break;
		case "cancelMergeWallets_Yes":
			Game1.player.changeWalletTypeTonight.Value = false;
			Game1.multiplayer.globalChatInfoMessage("MergeWalletsCancel", Game1.player.Name);
			break;
		case "ledgerOptions_SendMoney":
			ChooseRecipient();
			break;
		case "ledgerOptions_MergeWallets":
			s = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_MergeQuestion");
			createQuestionDialogue(s, createYesNoResponses(), "mergeWallets");
			break;
		case "ledgerOptions_CancelMerge":
			s = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_CancelQuestion");
			createQuestionDialogue(s, createYesNoResponses(), "cancelMergeWallets");
			break;
		case "lostAndFound_RetrieveFarmhandItems":
			ShowOfflineFarmhandItemList();
			return true;
		case "lostAndFound_CheckDonations":
			Game1.player.team.CheckReturnedDonations();
			return true;
		}
		if (questionAndAnswer.StartsWith("CheckItems"))
		{
			if (long.TryParse(questionAndAnswer.Split('_')[1], out var id))
			{
				Farmer farmhand = Game1.getFarmerMaybeOffline(id);
				if (farmhand != null && Utility.getHomeOfFarmer(farmhand) is Cabin home && !farmhand.isActive())
				{
					home.inventoryMutex.RequestLock(home.openFarmhandInventory);
				}
			}
			return true;
		}
		if (questionAndAnswer.Contains("Transfer"))
		{
			string answer = questionAndAnswer.Split('_')[1];
			beginSendMoney(sendMoneyMapping[answer]);
			sendMoneyMapping.Clear();
		}
		return base.answerDialogueAction(questionAndAnswer, questionParams);
	}
}
