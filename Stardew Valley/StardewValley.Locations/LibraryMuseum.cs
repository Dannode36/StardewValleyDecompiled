using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Museum;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Triggers;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class LibraryMuseum : GameLocation
{
	public const int dwarvenGuide = 0;

	protected static int _totalArtifacts = -1;

	public const int totalNotes = 21;

	private readonly NetMutex mutex = new NetMutex();

	[XmlIgnore]
	protected Dictionary<Item, string> _itemToRewardsLookup = new Dictionary<Item, string>();

	public static int totalArtifacts
	{
		get
		{
			if (_totalArtifacts < 0)
			{
				_totalArtifacts = 0;
				foreach (string itemId in ItemRegistry.RequireTypeDefinition("(O)").GetAllIds())
				{
					if (IsItemSuitableForDonation("(O)" + itemId, checkDonatedItems: false))
					{
						_totalArtifacts++;
					}
				}
			}
			return _totalArtifacts;
		}
	}

	[XmlElement("museumPieces")]
	public NetVector2Dictionary<string, NetString> museumPieces => Game1.netWorldState.Value.MuseumPieces;

	public LibraryMuseum()
	{
	}

	public LibraryMuseum(string mapPath, string name)
		: base(mapPath, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(mutex.NetFields, "mutex.NetFields");
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
	{
		mutex.Update(this);
		base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
	}

	/// <summary>Get whether any artifacts have been donated to the museum.</summary>
	public static bool HasDonatedArtifacts()
	{
		return Game1.netWorldState.Value.MuseumPieces.Length > 0;
	}

	/// <summary>Get whether an artifact has been placed on a given museum tile.</summary>
	/// <param name="tile">The tile position to check.</param>
	public static bool HasDonatedArtifactAt(Vector2 tile)
	{
		return Game1.netWorldState.Value.MuseumPieces.ContainsKey(tile);
	}

	/// <summary>Get whether an artifact has been donated to the museum.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to check.</param>
	public static bool HasDonatedArtifact(string itemId)
	{
		if (itemId == null)
		{
			return false;
		}
		itemId = ItemRegistry.ManuallyQualifyItemId(itemId, "(O)");
		foreach (KeyValuePair<Vector2, string> pair in Game1.netWorldState.Value.MuseumPieces.Pairs)
		{
			if (itemId == "(O)" + pair.Value)
			{
				return true;
			}
		}
		return false;
	}

	public bool isItemSuitableForDonation(Item i)
	{
		return IsItemSuitableForDonation(i?.QualifiedItemId);
	}

	/// <summary>Get whether an item can be donated to the museum.</summary>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	/// <param name="checkDonatedItems">Whether to return false if the item has already been donated to the museum.</param>
	public static bool IsItemSuitableForDonation(string itemId, bool checkDonatedItems = true)
	{
		if (itemId == null)
		{
			return false;
		}
		itemId = ItemRegistry.ManuallyQualifyItemId(itemId, "(O)");
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(itemId);
		HashSet<string> tags = ItemContextTagManager.GetBaseContextTags(itemId);
		if (!itemData.HasTypeObject() || tags.Contains("not_museum_donatable"))
		{
			return false;
		}
		if (checkDonatedItems && HasDonatedArtifact(itemData.QualifiedItemId))
		{
			return false;
		}
		if (!tags.Contains("museum_donatable") && !tags.Contains("item_type_arch"))
		{
			return tags.Contains("item_type_minerals");
		}
		return true;
	}

	public bool doesFarmerHaveAnythingToDonate(Farmer who)
	{
		for (int i = 0; i < (int)who.maxItems; i++)
		{
			if (i < who.Items.Count && who.Items[i] is Object obj && isItemSuitableForDonation(obj))
			{
				return true;
			}
		}
		return false;
	}

	private Dictionary<int, Vector2> getLostBooksLocations()
	{
		Dictionary<int, Vector2> lostBooksLocations = new Dictionary<int, Vector2>();
		for (int x = 0; x < map.Layers[0].LayerWidth; x++)
		{
			for (int y = 0; y < map.Layers[0].LayerHeight; y++)
			{
				string[] action = GetTilePropertySplitBySpaces("Action", "Buildings", x, y);
				if (ArgUtility.Get(action, 0) == "Notes")
				{
					if (ArgUtility.TryGetInt(action, 1, out var noteId, out var error))
					{
						lostBooksLocations.Add(noteId, new Vector2(x, y));
					}
					else
					{
						LogTileActionError(action, x, y, error);
					}
				}
			}
		}
		return lostBooksLocations;
	}

	protected override void resetLocalState()
	{
		if (!Game1.player.eventsSeen.Contains("0") && doesFarmerHaveAnythingToDonate(Game1.player))
		{
			Game1.player.mailReceived.Add("somethingToDonate");
		}
		if (HasDonatedArtifacts())
		{
			Game1.player.mailReceived.Add("somethingWasDonated");
		}
		base.resetLocalState();
		int booksFound = Game1.netWorldState.Value.LostBooksFound;
		foreach (KeyValuePair<int, Vector2> pair in getLostBooksLocations())
		{
			int id = pair.Key;
			Vector2 tile = pair.Value;
			if (id <= booksFound && !Game1.player.mailReceived.Contains("lb_" + id))
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 447, 15, 15), new Vector2(tile.X * 64f, tile.Y * 64f - 96f - 16f), flipped: false, 0f, Color.White)
				{
					interval = 99999f,
					animationLength = 1,
					totalNumberOfLoops = 9999,
					yPeriodic = true,
					yPeriodicLoopTime = 4000f,
					yPeriodicRange = 16f,
					layerDepth = 1f,
					scale = 4f,
					id = id
				});
			}
		}
	}

	public override void cleanupBeforePlayerExit()
	{
		_itemToRewardsLookup?.Clear();
		base.cleanupBeforePlayerExit();
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		if (questionAndAnswer == null)
		{
			return false;
		}
		switch (questionAndAnswer)
		{
		case "Museum_Collect":
			OpenRewardMenu();
			break;
		case "Museum_Donate":
			OpenDonationMenu();
			break;
		case "Museum_Rearrange_Yes":
			OpenRearrangeMenu();
			break;
		}
		return base.answerDialogueAction(questionAndAnswer, questionParams);
	}

	public string getRewardItemKey(Item item)
	{
		return "museumCollectedReward" + Utility.getStandardDescriptionFromItem(item, 1, '_');
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (who.IsLocalPlayer)
		{
			string text = ArgUtility.Get(action, 0);
			if (text == "Gunther")
			{
				OpenGuntherDialogueMenu();
				return true;
			}
			if (text == "Rearrange" && !doesFarmerHaveAnythingToDonate(Game1.player))
			{
				if (HasDonatedArtifacts())
				{
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Rearrange"), createYesNoResponses(), "Museum_Rearrange");
				}
				return true;
			}
		}
		return base.performAction(action, who, tileLocation);
	}

	/// <summary>Get the reward items which can be collected by a player.</summary>
	/// <param name="player">The player collecting rewards.</param>
	public List<Item> getRewardsForPlayer(Farmer player)
	{
		_itemToRewardsLookup.Clear();
		Dictionary<string, MuseumRewards> museumRewardData = DataLoader.MuseumRewards(Game1.content);
		Dictionary<string, int> countsByTag = GetDonatedByContextTag(museumRewardData);
		List<Item> rewards = new List<Item>();
		foreach (KeyValuePair<string, MuseumRewards> pair in museumRewardData)
		{
			string id = pair.Key;
			MuseumRewards value = pair.Value;
			if (!CanCollectReward(value, id, player, countsByTag))
			{
				continue;
			}
			bool rewardAdded = false;
			if (value.RewardItemId != null)
			{
				Item item = ItemRegistry.Create(value.RewardItemId, value.RewardItemCount);
				item.IsRecipe = value.RewardItemIsRecipe;
				item.specialItem = value.RewardItemIsSpecial;
				if (AddRewardItemIfUncollected(player, rewards, item))
				{
					_itemToRewardsLookup[item] = id;
					rewardAdded = true;
				}
			}
			if (!rewardAdded)
			{
				AddNonItemRewards(value, id, player);
			}
		}
		return rewards;
	}

	/// <summary>Give the player a set of non-item donation rewards.</summary>
	/// <param name="data">The museum donation rewards to give to the player.</param>
	/// <param name="rewardId">The unique ID for <paramref name="data" />.</param>
	/// <param name="player">The player collecting rewards.</param>
	public void AddNonItemRewards(MuseumRewards data, string rewardId, Farmer player)
	{
		if (data.FlagOnCompletion)
		{
			player.mailReceived.Add(rewardId);
		}
		if (data.RewardActions == null)
		{
			return;
		}
		foreach (string action in data.RewardActions)
		{
			if (!TriggerActionManager.TryRunAction(action, out var error, out var ex))
			{
				Game1.log.Error($"Museum reward {rewardId} ignored invalid event action '{action}': {error}", ex);
			}
		}
	}

	/// <summary>Add the item to the reward list only if the item hasn't been marked as collected.</summary>
	/// <param name="player">The player collecting rewards.</param>
	/// <param name="rewards">The list of rewards to update.</param>
	/// <param name="rewardItem">The reward to add if it's uncollected.</param>
	public bool AddRewardItemIfUncollected(Farmer player, List<Item> rewards, Item rewardItem)
	{
		if (!player.mailReceived.Contains(getRewardItemKey(rewardItem)))
		{
			rewards.Add(rewardItem);
			return true;
		}
		return false;
	}

	/// <summary>Get whether the player can collect an item from the reward menu.</summary>
	/// <param name="item">The item to check.</param>
	public bool HighlightCollectableRewards(Item item)
	{
		return Game1.player.couldInventoryAcceptThisItem(item);
	}

	/// <summary>Open the artifact rearranging menu.</summary>
	public void OpenRearrangeMenu()
	{
		if (!mutex.IsLocked())
		{
			mutex.RequestLock(delegate
			{
				Game1.activeClickableMenu = new MuseumMenu(InventoryMenu.highlightNoItems)
				{
					exitFunction = mutex.ReleaseLock
				};
			});
		}
	}

	/// <summary>Open the reward collection menu.</summary>
	public void OpenRewardMenu()
	{
		Game1.activeClickableMenu = new ItemGrabMenu(getRewardsForPlayer(Game1.player), reverseGrab: false, showReceivingMenu: true, HighlightCollectableRewards, null, "Rewards", OnRewardCollected, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, allowExitWithHeldItem: true);
	}

	/// <summary>Open the artifact donation menu.</summary>
	public void OpenDonationMenu()
	{
		mutex.RequestLock(delegate
		{
			Game1.activeClickableMenu = new MuseumMenu(isItemSuitableForDonation)
			{
				exitFunction = OnDonationMenuClosed
			};
		});
	}

	/// <summary>Handle the player closing the artifact donation screen.</summary>
	public void OnDonationMenuClosed()
	{
		mutex.ReleaseLock();
		getRewardsForPlayer(Game1.player);
	}

	/// <summary>Handle the player collecting an item from the reward screen.</summary>
	/// <param name="item">The item that was collected.</param>
	/// <param name="who">The player collecting rewards.</param>
	public void OnRewardCollected(Item item, Farmer who)
	{
		if (item == null)
		{
			return;
		}
		if (item is Object && _itemToRewardsLookup.TryGetValue(item, out var rewardKey))
		{
			if (DataLoader.MuseumRewards(Game1.content).TryGetValue(rewardKey, out var rewardData))
			{
				AddNonItemRewards(rewardData, rewardKey, who);
			}
			_itemToRewardsLookup.Remove(item);
		}
		if (!who.hasOrWillReceiveMail(getRewardItemKey(item)))
		{
			who.mailReceived.Add(getRewardItemKey(item));
			if (item.QualifiedItemId.Equals("(O)499"))
			{
				who.craftingRecipes.TryAdd("Ancient Seeds", 0);
			}
		}
	}

	/// <summary>Open the dialogue menu for Gunther.</summary>
	private void OpenGuntherDialogueMenu()
	{
		if (doesFarmerHaveAnythingToDonate(Game1.player) && !mutex.IsLocked())
		{
			Response[] choice = ((getRewardsForPlayer(Game1.player).Count <= 0) ? new Response[2]
			{
				new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
				new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
			} : new Response[3]
			{
				new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
				new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
				new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
			});
			createQuestionDialogue("", choice, "Museum");
		}
		else if (getRewardsForPlayer(Game1.player).Count > 0)
		{
			createQuestionDialogue("", new Response[2]
			{
				new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
				new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
			}, "Museum");
		}
		else if (doesFarmerHaveAnythingToDonate(Game1.player) && mutex.IsLocked())
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NPC_Busy", Game1.RequireCharacter("Gunther").displayName));
		}
		else
		{
			NPC gunther = Game1.getCharacterFromName("Gunther");
			if (Game1.player.achievements.Contains(5))
			{
				Game1.DrawDialogue(new Dialogue(gunther, "Data\\ExtraDialogue:Gunther_MuseumComplete", Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_MuseumComplete"))));
			}
			else if (Game1.player.mailReceived.Contains("artifactFound"))
			{
				Game1.DrawDialogue(new Dialogue(gunther, "Data\\ExtraDialogue:Gunther_NothingToDonate", Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NothingToDonate"))));
			}
			else
			{
				Game1.DrawDialogue(gunther, "Data\\ExtraDialogue:Gunther_NoArtifactsFound");
			}
		}
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (museumPieces.TryGetValue(new Vector2(tileLocation.X, tileLocation.Y), out var itemId) || museumPieces.TryGetValue(new Vector2(tileLocation.X, tileLocation.Y - 1), out itemId))
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem("(O)" + itemId);
			Game1.drawObjectDialogue(Game1.parseText(" - " + data.DisplayName + " - " + "^" + data.Description));
			return true;
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	public bool isTileSuitableForMuseumPiece(int x, int y)
	{
		if (!HasDonatedArtifactAt(new Vector2(x, y)))
		{
			int indexOfBuildingsLayer = getTileIndexAt(new Point(x, y), "Buildings");
			if (indexOfBuildingsLayer == 1073 || indexOfBuildingsLayer == 1074 || indexOfBuildingsLayer == 1072 || indexOfBuildingsLayer == 1237 || indexOfBuildingsLayer == 1238)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Get a count of donated items by context tag.</summary>
	/// <param name="museumRewardData">The museum rewards for which to count context tags.</param>
	public Dictionary<string, int> GetDonatedByContextTag(Dictionary<string, MuseumRewards> museumRewardData)
	{
		Dictionary<string, int> counts = new Dictionary<string, int>();
		foreach (MuseumRewards value in museumRewardData.Values)
		{
			foreach (MuseumDonationRequirement targetTags in value.TargetContextTags)
			{
				counts[targetTags.Tag] = 0;
			}
		}
		string[] contextTags = counts.Keys.ToArray();
		foreach (string itemId in museumPieces.Values)
		{
			string[] array = contextTags;
			foreach (string tag in array)
			{
				if (tag == "" || ItemContextTagManager.HasBaseTag(itemId, tag))
				{
					counts[tag]++;
				}
			}
		}
		return counts;
	}

	/// <summary>Get whether a reward can be collected by a player.</summary>
	/// <param name="reward">The reward data to check.</param>
	/// <param name="rewardId">The unique ID for the <paramref name="reward" />.</param>
	/// <param name="player">The player collecting rewards.</param>
	/// <param name="countsByTag">The number of donated items matching each context tag.</param>
	public bool CanCollectReward(MuseumRewards reward, string rewardId, Farmer player, Dictionary<string, int> countsByTag)
	{
		if (reward.FlagOnCompletion && player.mailReceived.Contains(rewardId))
		{
			return false;
		}
		foreach (MuseumDonationRequirement targetTags in reward.TargetContextTags)
		{
			if (targetTags.Tag == "" && targetTags.Count == -1)
			{
				if (countsByTag[targetTags.Tag] < totalArtifacts)
				{
					return false;
				}
			}
			else if (countsByTag[targetTags.Tag] < targetTags.Count)
			{
				return false;
			}
		}
		if (reward.RewardItemId != null)
		{
			if (player.canUnderstandDwarves && ItemRegistry.QualifyItemId(reward.RewardItemId) == "(O)326")
			{
				return false;
			}
			if (reward.RewardItemIsSpecial)
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(reward.RewardItemId);
				if (((itemData.HasTypeId("(F)") || itemData.HasTypeBigCraftable()) ? player.specialBigCraftables : player.specialItems).Contains(itemData.ItemId))
				{
					return false;
				}
			}
		}
		return true;
	}

	public Microsoft.Xna.Framework.Rectangle getMuseumDonationBounds()
	{
		return new Microsoft.Xna.Framework.Rectangle(26, 5, 22, 13);
	}

	public Vector2 getFreeDonationSpot()
	{
		Microsoft.Xna.Framework.Rectangle bounds = getMuseumDonationBounds();
		for (int x = bounds.X; x <= bounds.Right; x++)
		{
			for (int y = bounds.Y; y <= bounds.Bottom; y++)
			{
				if (isTileSuitableForMuseumPiece(x, y))
				{
					return new Vector2(x, y);
				}
			}
		}
		return new Vector2(26f, 5f);
	}

	public Vector2 findMuseumPieceLocationInDirection(Vector2 startingPoint, int direction, int distanceToCheck = 8, bool ignoreExistingItems = true)
	{
		Vector2 checkTile = startingPoint;
		Vector2 offset = Vector2.Zero;
		switch (direction)
		{
		case 0:
			offset = new Vector2(0f, -1f);
			break;
		case 1:
			offset = new Vector2(1f, 0f);
			break;
		case 2:
			offset = new Vector2(0f, 1f);
			break;
		case 3:
			offset = new Vector2(-1f, 0f);
			break;
		}
		for (int j = 0; j < distanceToCheck; j++)
		{
			for (int i = 0; i < distanceToCheck; i++)
			{
				checkTile += offset;
				if (isTileSuitableForMuseumPiece((int)checkTile.X, (int)checkTile.Y) || (!ignoreExistingItems && HasDonatedArtifactAt(checkTile)))
				{
					return checkTile;
				}
			}
			checkTile = startingPoint;
			int sign = ((j % 2 != 0) ? 1 : (-1));
			switch (direction)
			{
			case 0:
			case 2:
				checkTile.X += sign * (j / 2 + 1);
				break;
			case 1:
			case 3:
				checkTile.Y += sign * (j / 2 + 1);
				break;
			}
		}
		return startingPoint;
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		foreach (TemporaryAnimatedSprite t in temporarySprites)
		{
			if (t.layerDepth >= 1f)
			{
				t.draw(b);
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		foreach (KeyValuePair<Vector2, string> v in museumPieces.Pairs)
		{
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, v.Key * 64f + new Vector2(32f, 52f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (v.Key.Y * 64f - 2f) / 10000f);
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem("(O)" + v.Value);
			b.Draw(data.GetTexture(), Game1.GlobalToLocal(Game1.viewport, v.Key * 64f), data.GetSourceRect(), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, v.Key.Y * 64f / 10000f);
		}
	}
}
