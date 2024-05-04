using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewValley.Delegates;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Tools;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Internal;

/// <summary>Resolves item IDs like <samp>(O)128</samp> and item queries like <samp>RANDOM_ITEMS</samp> in data assets.</summary>
/// <remarks>This is an internal implementation class. Most code should use higher-level code like <see cref="M:StardewValley.Utility.TryOpenShopMenu(System.String,System.String,System.Boolean)" /> instead.</remarks>
public static class ItemQueryResolver
{
	/// <summary>The resolvers for vanilla item queries. Most code should call <c>TryResolve</c> instead of using these directly.</summary>
	public static class DefaultResolvers
	{
		/// <summary>Get every item in the game, optionally filtered by type. Format: <c>ALL_ITEMS [type]</c>.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> ALL_ITEMS(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			string onlyTypeId = null;
			bool isRandomSale = false;
			bool requirePrice = false;
			string[] args = Helpers.SplitArguments(arguments);
			int flagsIndex = 0;
			if (ArgUtility.HasIndex(args, 0) && !args[0].StartsWith('@'))
			{
				onlyTypeId = args[0];
				flagsIndex++;
			}
			for (int i = flagsIndex; i < args.Length; i++)
			{
				string arg = args[i];
				if (arg.Equals("@isRandomSale", StringComparison.OrdinalIgnoreCase))
				{
					isRandomSale = true;
					continue;
				}
				if (arg.Equals("@requirePrice", StringComparison.OrdinalIgnoreCase))
				{
					requirePrice = true;
					continue;
				}
				if (arg.StartsWith('@'))
				{
					Helpers.ErrorResult(key, arguments, logError, $"index {i} has unknown option flag '{arg}'");
					yield break;
				}
				if (onlyTypeId != null && onlyTypeId != arg)
				{
					Helpers.ErrorResult(key, arguments, logError, $"index {i} must be an option flag starting with '@'");
					yield break;
				}
				onlyTypeId = arg;
			}
			foreach (IItemDataDefinition itemDataDefinition in ItemRegistry.ItemTypes)
			{
				string typeId = itemDataDefinition.Identifier;
				if (onlyTypeId != null && typeId != onlyTypeId)
				{
					continue;
				}
				if (typeId == "(F)")
				{
					List<Furniture> furniture = new List<Furniture>();
					foreach (ParsedItemData data in itemDataDefinition.GetAllData())
					{
						if (!isRandomSale || !Helpers.ExcludeFromRandomSale(data))
						{
							Furniture item = ItemRegistry.Create<Furniture>(data.QualifiedItemId);
							if (!requirePrice || item.salePrice(ignoreProfitMargins: true) > 0)
							{
								furniture.Add(item);
							}
						}
					}
					furniture.Sort(Utility.SortAllFurnitures);
					foreach (Furniture item in furniture)
					{
						yield return new ItemQueryResult(item);
					}
					continue;
				}
				foreach (ParsedItemData data in itemDataDefinition.GetAllData())
				{
					if (!isRandomSale || !Helpers.ExcludeFromRandomSale(data))
					{
						Item item = ItemRegistry.Create(data.QualifiedItemId);
						if (!requirePrice || item.salePrice(ignoreProfitMargins: true) > 0)
						{
							yield return new ItemQueryResult(item);
						}
					}
				}
			}
		}

		/// <summary>Get the dish of the day sold at the Saloon, if any. Format: <c>DISH_OF_THE_DAY</c> (no arguments).</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> DISH_OF_THE_DAY(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			if (Game1.dishOfTheDay == null)
			{
				return LegacyShims.EmptyArray<ItemQueryResult>();
			}
			return new ItemQueryResult[1]
			{
				new ItemQueryResult(Game1.dishOfTheDay.getOne())
				{
					OverrideShopAvailableStock = Game1.dishOfTheDay.Stack,
					SyncStacksWith = Game1.dishOfTheDay
				}
			};
		}

		/// <summary>Get a flavored item for a given type and ingredient (like Wine + Blueberry = Blueberry Wine). Format: <c>FLAVORED_ITEM &lt;type&gt; &lt;ingredient item ID&gt; [ingredient preserved ID]</c>.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> FLAVORED_ITEM(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			int quality = 0;
			bool isWildHoney = false;
			string[] splitArgs = Helpers.SplitArguments(arguments);
			if (!Utility.TryParseEnum<Object.PreserveType>(splitArgs[0], out var type))
			{
				return Helpers.ErrorResult(key, arguments, logError, "invalid flavored item type (must be one of " + string.Join(", ", Enum.GetNames(typeof(Object.PreserveType))) + ")");
			}
			string ingredientId = ArgUtility.Get(splitArgs, 1);
			if (type == Object.PreserveType.Honey && ingredientId == "-1")
			{
				isWildHoney = true;
				ingredientId = null;
			}
			else
			{
				ingredientId = ItemRegistry.QualifyItemId(ingredientId);
				if (ingredientId == null)
				{
					return Helpers.ErrorResult(key, arguments, logError, "must specify a valid flavor ingredient ID");
				}
			}
			string ingredientPreservedId = ArgUtility.Get(splitArgs, 2);
			if (ingredientPreservedId == "0")
			{
				ingredientPreservedId = null;
			}
			ArgUtility.TryGetOptionalInt(splitArgs, 2, out quality, out var _);
			ObjectDataDefinition objectData = ItemRegistry.GetObjectTypeDefinition();
			Object ingredient = null;
			if (!isWildHoney)
			{
				try
				{
					ingredient = ((type == Object.PreserveType.AgedRoe && ingredientId == "(O)812" && ingredientPreservedId != null) ? objectData.CreateFlavoredItem(Object.PreserveType.Roe, ItemRegistry.Create<Object>(ingredientPreservedId)) : (ItemRegistry.Create(ingredientId) as Object));
				}
				catch (Exception ex)
				{
					return Helpers.ErrorResult(key, arguments, logError, ex.Message);
				}
				if (ingredient != null)
				{
					ingredient.Quality = quality;
				}
			}
			Object flavoredItem = objectData.CreateFlavoredItem(type, ingredient);
			if (flavoredItem == null)
			{
				return Helpers.ErrorResult(key, arguments, logError, $"unsupported flavor type '{type}'.");
			}
			return new ItemQueryResult[1]
			{
				new ItemQueryResult(flavoredItem)
			};
		}

		/// <summary>Get the items lost when the player collapsed in the mines, which can be recovered from Marlon's shop. Format: <c>ITEMS_LOST_ON_DEATH</c> (no arguments).</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> ITEMS_LOST_ON_DEATH(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			List<ItemQueryResult> items = new List<ItemQueryResult>();
			foreach (Item item in Game1.player.itemsLostLastDeath)
			{
				if (item != null)
				{
					item.isLostItem = true;
					items.Add(new ItemQueryResult(item)
					{
						OverrideStackSize = item.Stack,
						OverrideBasePrice = ((Game1.player.stats.Get("Book_Marlon") != 0) ? ((int)((float)Utility.getSellToStorePriceOfItem(item) * 0.5f)) : Utility.getSellToStorePriceOfItem(item))
					});
				}
			}
			return items;
		}

		/// <summary>Get items the player has recently sold to a given shop. Format: <c>ITEMS_SOLD_BY_PLAYER &lt;shop location ID&gt;</c>.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> ITEMS_SOLD_BY_PLAYER(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			if (string.IsNullOrWhiteSpace(arguments))
			{
				Helpers.ErrorResult(key, arguments, logError, "must specify a location ID");
				yield break;
			}
			GameLocation rawShop = Game1.getLocationFromName(arguments);
			if (rawShop == null)
			{
				Helpers.ErrorResult(key, arguments, logError, "the specified location ID didn't match any location");
				yield break;
			}
			if (!(rawShop is ShopLocation shopLocation))
			{
				Helpers.ErrorResult(key, arguments, logError, "the specified location ID matched a location which isn't a ShopLocation instance");
				yield break;
			}
			foreach (Item i in shopLocation.itemsFromPlayerToSell)
			{
				if (i.Stack > 0)
				{
					int price = ((i is Object obj) ? obj.sellToStorePrice(-1L) : i.salePrice());
					yield return new ItemQueryResult(i.getOne())
					{
						OverrideBasePrice = price,
						OverrideShopAvailableStock = i.Stack,
						SyncStacksWith = i
					};
				}
			}
		}

		/// <summary>Get a fish which can be caught in a location based on its <c>Data/Locations</c> entry. Format: <c>LOCATION_FISH &lt;location name&gt; &lt;bobber x&gt; &lt;bobber y&gt; &lt;water depth&gt;</c>.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> LOCATION_FISH(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			string[] splitArgs = Helpers.SplitArguments(arguments);
			if (splitArgs.Length != 4)
			{
				return Helpers.ErrorResult(key, arguments, logError, "expected four arguments in the form <location name> <bobber x> <bobber y> <depth>");
			}
			string locationName = splitArgs[0];
			string rawX = splitArgs[1];
			string rawY = splitArgs[2];
			string rawDepth = splitArgs[3];
			if (!int.TryParse(rawX, out var x) || !int.TryParse(rawY, out var y))
			{
				return Helpers.ErrorResult(key, arguments, logError, $"can't parse '{rawX} {rawY}' as numeric 'x y' values");
			}
			if (!int.TryParse(rawDepth, out var depth))
			{
				return Helpers.ErrorResult(key, arguments, logError, "can't parse '" + rawDepth + "' as a numeric depth value");
			}
			Item fish = GameLocation.GetFishFromLocationData(locationName, new Vector2(x, y), depth, context?.Player ?? Game1.player, isTutorialCatch: false, isInherited: true);
			if (fish == null)
			{
				return LegacyShims.EmptyArray<ItemQueryResult>();
			}
			return new ItemQueryResult[1]
			{
				new ItemQueryResult(fish)
			};
		}

		/// <summary>Get a lost book (if they haven't all been found), else the given item query (if provided), else nothing. Format: <c>LOST_BOOK_OR_ITEM [alternate item query]</c>.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> LOST_BOOK_OR_ITEM(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			if (Game1.netWorldState.Value.LostBooksFound < 21)
			{
				return new ItemQueryResult[1]
				{
					new ItemQueryResult(ItemRegistry.Create("(O)102"))
				};
			}
			if (string.IsNullOrWhiteSpace(arguments))
			{
				return LegacyShims.EmptyArray<ItemQueryResult>();
			}
			return TryResolve(arguments, new ItemQueryContext(context));
		}

		/// <summary>Get the rewards that can currently be collected from Gil in the Adventurer's Guild. Format: <c>MONSTER_SLAYER_REWARDS</c> (no arguments).</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> MONSTER_SLAYER_REWARDS(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			KeyValuePair<string, MonsterSlayerQuestData>[] monsterSlayerQuestData = (from p in DataLoader.MonsterSlayerQuests(Game1.content)
				where AdventureGuild.HasCollectedReward(context.Player, p.Key)
				select p).ToArray();
			HashSet<string> questIds = new HashSet<string>();
			KeyValuePair<string, MonsterSlayerQuestData>[] array = monsterSlayerQuestData;
			for (int j = 0; j < array.Length; j++)
			{
				KeyValuePair<string, MonsterSlayerQuestData> pair = array[j];
				string id = pair.Key;
				MonsterSlayerQuestData questData = pair.Value;
				if (!questIds.Contains(id) && questData.RewardItemId != null && questData.RewardItemPrice != -1 && ItemContextTagManager.HasBaseTag(questData.RewardItemId, "item_type_ring"))
				{
					Item i = ItemRegistry.Create(questData.RewardItemId);
					yield return new ItemQueryResult(i)
					{
						OverrideBasePrice = questData.RewardItemPrice,
						OverrideShopAvailableStock = int.MaxValue
					};
					questIds.Add(id);
				}
			}
			array = monsterSlayerQuestData;
			for (int j = 0; j < array.Length; j++)
			{
				KeyValuePair<string, MonsterSlayerQuestData> pair = array[j];
				string id = pair.Key;
				MonsterSlayerQuestData questData = pair.Value;
				if (!questIds.Contains(id) && questData.RewardItemId != null && questData.RewardItemPrice != -1 && !(ItemRegistry.ResolveMetadata(questData.RewardItemId)?.GetTypeDefinition()?.Identifier != "(H)"))
				{
					Item i = ItemRegistry.Create(questData.RewardItemId);
					yield return new ItemQueryResult(i)
					{
						OverrideBasePrice = questData.RewardItemPrice,
						OverrideShopAvailableStock = int.MaxValue
					};
					questIds.Add(id);
				}
			}
			array = monsterSlayerQuestData;
			for (int j = 0; j < array.Length; j++)
			{
				KeyValuePair<string, MonsterSlayerQuestData> pair = array[j];
				string id = pair.Key;
				MonsterSlayerQuestData questData = pair.Value;
				if (!questIds.Contains(id) && questData.RewardItemId != null && questData.RewardItemPrice != -1 && !(ItemRegistry.ResolveMetadata(questData.RewardItemId)?.GetTypeDefinition()?.Identifier != "(W)"))
				{
					Item i = ItemRegistry.Create(questData.RewardItemId);
					yield return new ItemQueryResult(i)
					{
						OverrideBasePrice = questData.RewardItemPrice,
						OverrideShopAvailableStock = int.MaxValue
					};
					questIds.Add(id);
				}
			}
			array = monsterSlayerQuestData;
			for (int j = 0; j < array.Length; j++)
			{
				KeyValuePair<string, MonsterSlayerQuestData> pair = array[j];
				string id = pair.Key;
				MonsterSlayerQuestData questData = pair.Value;
				if (!questIds.Contains(id) && questData.RewardItemId != null && questData.RewardItemPrice != -1)
				{
					Item i = ItemRegistry.Create(questData.RewardItemId);
					yield return new ItemQueryResult(i)
					{
						OverrideBasePrice = questData.RewardItemPrice,
						OverrideShopAvailableStock = int.MaxValue
					};
					questIds.Add(id);
				}
			}
		}

		/// <summary>Get the movie concessions to show for an invited NPC. Format <c>MOVIE_CONCESSIONS_FOR_GUEST [npcName]</c>.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> MOVIE_CONCESSIONS_FOR_GUEST(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			string npcName = ArgUtility.SplitBySpaceAndGet(arguments, 0);
			List<MovieConcession> concessions = ((npcName != null) ? MovieTheater.GetConcessionsForGuest(npcName) : MovieTheater.GetConcessionsForGuest());
			foreach (MovieConcession concession in concessions)
			{
				yield return new ItemQueryResult(concession);
			}
		}

		/// <summary>Get the first artifact in <c>Data/Objects</c> which lists the current location as a spawn location and whose chance matches. Format <c>RANDOM_ARTIFACT_FOR_DIG_SPOT</c> (no arguments).</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> RANDOM_ARTIFACT_FOR_DIG_SPOT(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			Random random = context.Random ?? Game1.random;
			Farmer player = context.Player;
			string locationName = context.Location.Name;
			Hoe obj = player.CurrentTool as Hoe;
			int chanceMultiplier = ((obj == null || !obj.hasEnchantmentOfType<ArchaeologistEnchantment>()) ? 1 : 2);
			foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (!(data.ObjectType != "Arch"))
				{
					Dictionary<string, float> dropChances = (data.RawData as ObjectData)?.ArtifactSpotChances;
					if (dropChances != null && dropChances.TryGetValue(locationName, out var chance) && random.NextBool((float)chanceMultiplier * chance))
					{
						return new ItemQueryResult[1]
						{
							new ItemQueryResult(ItemRegistry.Create(data.QualifiedItemId))
						};
					}
				}
			}
			return LegacyShims.EmptyArray<ItemQueryResult>();
		}

		/// <summary>Get a random seasonal vanilla item which can be found by searching garbage cans, breaking containers in the mines, etc. Format: <c>RANDOM_BASE_SEASON_ITEM</c> (no arguments).</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> RANDOM_BASE_SEASON_ITEM(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			GameLocation location = context.Location;
			Item item = ItemRegistry.Create(Utility.getRandomItemFromSeason(random: context.Random ?? Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode(key + arguments)), season: location.GetSeason(), forQuest: false));
			return new ItemQueryResult[1]
			{
				new ItemQueryResult(item)
			};
		}

		/// <summary>Get random items for a given type, optionally within a numeric ID range. Format: <c>RANDOM_ITEMS &lt;item data definition ID&gt; [min numeric id] [max numeric id]</c>.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> RANDOM_ITEMS(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			int minId = int.MinValue;
			int maxId = int.MaxValue;
			bool isRandomSale = false;
			bool requirePrice = false;
			string[] args = Helpers.SplitArguments(arguments);
			if (!ArgUtility.TryGet(args, 0, out var typeId, out var error, allowBlank: false))
			{
				Helpers.ErrorResult(key, arguments, logError, error);
				yield break;
			}
			int flagsIndex = 1;
			if (ArgUtility.HasIndex(args, 1) && int.TryParse(args[1], out var parsedId))
			{
				minId = parsedId;
				flagsIndex++;
				if (ArgUtility.HasIndex(args, 2) && int.TryParse(args[2], out parsedId))
				{
					maxId = parsedId;
					flagsIndex++;
				}
			}
			for (int i = flagsIndex; i < args.Length; i++)
			{
				string arg = args[i];
				if (arg.Equals("@isRandomSale", StringComparison.OrdinalIgnoreCase))
				{
					isRandomSale = true;
					continue;
				}
				if (arg.Equals("@requirePrice", StringComparison.OrdinalIgnoreCase))
				{
					requirePrice = true;
					continue;
				}
				if (arg.StartsWith('@'))
				{
					Helpers.ErrorResult(key, arguments, logError, $"index {i} has unknown flag argument '{arg}'");
				}
				else if (i == 1 || i == 2)
				{
					Helpers.ErrorResult(key, arguments, logError, $"index {i} must a numeric {((i == 1) ? "min" : "max")} ID, or an option flag starting with '@'.");
				}
				else
				{
					Helpers.ErrorResult(key, arguments, logError, $"index {i} must be an option flag starting with '@'.");
				}
				yield break;
			}
			IItemDataDefinition typeDef = ItemRegistry.GetTypeDefinition(typeId);
			if (typeDef == null)
			{
				Helpers.ErrorResult(key, arguments, logError, "there's no item data definition with ID '" + typeId + "'");
				yield break;
			}
			bool hasRange = minId != int.MinValue || maxId != int.MaxValue;
			Random random = context.Random ?? Game1.random;
			foreach (ParsedItemData data in from p in typeDef.GetAllData()
				orderby random.Next()
				select p)
			{
				if ((!isRandomSale || !Helpers.ExcludeFromRandomSale(data)) && (!hasRange || (int.TryParse(data.ItemId, out var index) && index >= minId && index <= maxId)))
				{
					Item item = ItemRegistry.Create(data.QualifiedItemId);
					if (!requirePrice || item.salePrice(ignoreProfitMargins: true) > 0)
					{
						yield return new ItemQueryResult(item);
					}
				}
			}
		}

		/// <summary>Get a secret note (if the player unlocked them and hasn't found them all), else the given item query (if provided), else nothing. Format: <c>SECRET_NOTE_OR_ITEM [alternate item query]</c>.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> SECRET_NOTE_OR_ITEM(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			GameLocation location = context.Location;
			Farmer player = context.Player;
			if (location != null && location.HasUnlockedAreaSecretNotes(player))
			{
				Object secretNote = location.tryToCreateUnseenSecretNote(player);
				if (secretNote != null)
				{
					return new ItemQueryResult[1]
					{
						new ItemQueryResult(secretNote)
					};
				}
			}
			if (string.IsNullOrWhiteSpace(arguments))
			{
				return LegacyShims.EmptyArray<ItemQueryResult>();
			}
			return TryResolve(arguments, new ItemQueryContext(context));
		}

		/// <summary>Get a special 'key to the town' shop item. This returns an <see cref="T:StardewValley.ISalable" /> instance which may be ignored or invalid outside shops. Format: <c>SHOP_TOWN_KEY</c> (no arguments).</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> SHOP_TOWN_KEY(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			ISalable townKey = new PurchaseableKeyItem(Game1.content.LoadString("Strings\\StringsFromCSFiles:KeyToTheTown"), Game1.content.LoadString("Strings\\StringsFromCSFiles:KeyToTheTown_desc"), 912, delegate(Farmer farmer)
			{
				farmer.HasTownKey = true;
			});
			return new ItemQueryResult[1]
			{
				new ItemQueryResult(townKey)
				{
					OverrideShopAvailableStock = 1
				}
			};
		}

		/// <summary>Get the tool upgrades listed in <c>Data/Shops</c> for the given tool ID (or all tool upgrades if <c>[tool ID]</c> is omitted). Format: <c>TOOL_UPGRADES [tool ID]</c>.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ResolveItemQueryDelegate" />
		public static IEnumerable<ItemQueryResult> TOOL_UPGRADES(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			string onlyItemId = null;
			if (!string.IsNullOrWhiteSpace(arguments))
			{
				ParsedItemData data = ItemRegistry.GetDataOrErrorItem(arguments);
				if (data.HasTypeId("(T)"))
				{
					return Helpers.ErrorResult(key, arguments, logError, "can't filter for ID '" + arguments + "' because that isn't a tool item ID");
				}
				onlyItemId = data.ItemId;
			}
			List<ItemQueryResult> stock = new List<ItemQueryResult>();
			foreach (KeyValuePair<string, ToolData> pair in Game1.toolData)
			{
				string itemId = pair.Key;
				ToolData entry = pair.Value;
				if (onlyItemId == null || !(itemId != onlyItemId))
				{
					ToolUpgradeData upgrade = ShopBuilder.GetToolUpgradeData(entry, Game1.player);
					if (upgrade != null)
					{
						Item tool = ItemRegistry.Create("(T)" + itemId);
						int price = ((upgrade.Price > -1) ? upgrade.Price : Math.Max(0, tool.salePrice()));
						stock.Add(new ItemQueryResult(tool)
						{
							OverrideBasePrice = price,
							OverrideShopAvailableStock = 1,
							OverrideTradeItemId = upgrade.TradeItemId,
							OverrideTradeItemAmount = upgrade.TradeItemAmount
						});
					}
				}
			}
			return stock;
		}

		public static IEnumerable<ItemQueryResult> PET_ADOPTION(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
		{
			List<ItemQueryResult> stock = new List<ItemQueryResult>();
			foreach (KeyValuePair<string, PetData> pair in Game1.petData)
			{
				foreach (PetBreed breed in pair.Value.Breeds)
				{
					if (breed.CanBeAdoptedFromMarnie)
					{
						stock.Add(new ItemQueryResult(new PetLicense
						{
							Name = pair.Key + "|" + breed.Id
						})
						{
							OverrideBasePrice = breed.AdoptionPrice
						});
					}
				}
			}
			return stock;
		}
	}

	/// <summary>The helper methods which simplify implementing custom item queries.</summary>
	public static class Helpers
	{
		/// <summary>Split an argument list into individual arguments.</summary>
		/// <param name="arguments">The arguments to split.</param>
		public static string[] SplitArguments(string arguments)
		{
			if (arguments.Length <= 0)
			{
				return LegacyShims.EmptyArray<string>();
			}
			return ArgUtility.SplitBySpace(arguments);
		}

		/// <summary>Log an error for an invalid query, and return an empty list of items.</summary>
		/// <param name="key">The query key specified in the item ID.</param>
		/// <param name="arguments">Any text specified in the item ID after the <paramref name="key" />.</param>
		/// <param name="logError">Log an error message to the console, given the item query and error message.</param>
		/// <param name="message">A human-readable message indicating why the query is invalid.</param>
		public static ItemQueryResult[] ErrorResult(string key, string arguments, Action<string, string> logError, string message)
		{
			logError?.Invoke((key + " " + arguments).Trim(), message);
			return LegacyShims.EmptyArray<ItemQueryResult>();
		}

		/// <summary>Get whether to exclude this item from shops when selecting random items to sell, including catalogues.</summary>
		/// <param name="data">The parsed item data.</param>
		public static bool ExcludeFromRandomSale(ParsedItemData data)
		{
			if (data.ExcludeFromRandomSale)
			{
				return true;
			}
			string itemTypeId = data.GetItemTypeId();
			if (!(itemTypeId == "(WP)"))
			{
				if (itemTypeId == "(FL)" && Utility.isFlooringOffLimitsForSale(data.ItemId))
				{
					return true;
				}
			}
			else if (Utility.isWallpaperOffLimitsForSale(data.ItemId))
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>The item query keys that can be used instead of an item ID in list data fields like <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> or <see cref="P:StardewValley.GameData.ISpawnItemData.RandomItemId" /> fields, and the methods which create the items for them.</summary>
	public static Dictionary<string, ResolveItemQueryDelegate> ItemResolvers { get; }

	/// <summary>Register the default item queries, defined as <see cref="T:StardewValley.Internal.ItemQueryResolver.DefaultResolvers" /> methods.</summary>
	static ItemQueryResolver()
	{
		ItemResolvers = new Dictionary<string, ResolveItemQueryDelegate>(StringComparer.OrdinalIgnoreCase);
		MethodInfo[] methods = typeof(DefaultResolvers).GetMethods(BindingFlags.Static | BindingFlags.Public);
		foreach (MethodInfo method in methods)
		{
			ResolveItemQueryDelegate queryDelegate = (ResolveItemQueryDelegate)Delegate.CreateDelegate(typeof(ResolveItemQueryDelegate), method);
			Register(method.Name, queryDelegate);
		}
	}

	/// <summary>Register an item query resolver.</summary>
	/// <param name="queryKey">The item query key, like <c>ALL_ITEMS</c>. This should only contain alphanumeric, underscore, and dot characters. For custom queries, this should be prefixed with your mod ID like <c>Example.ModId_QueryName</c>.</param>
	/// <param name="queryDelegate">The resolver which returns the items produced by the item query.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="queryKey" /> is null or whitespace-only.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="queryDelegate" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="queryKey" /> is already registered.</exception>
	public static void Register(string queryKey, ResolveItemQueryDelegate queryDelegate)
	{
		if (string.IsNullOrWhiteSpace(queryKey))
		{
			throw new ArgumentException("The query key can't be null or empty.", "queryKey");
		}
		if (ItemResolvers.ContainsKey(queryKey))
		{
			throw new InvalidOperationException("The query key '" + queryKey + "' is already registered.");
		}
		ItemResolvers[queryKey.Trim()] = queryDelegate ?? throw new ArgumentNullException("queryDelegate");
	}

	/// <summary>Get the items matching an item ID or query.</summary>
	/// <param name="query">The item ID or query to match.</param>
	/// <param name="context">The contextual info for item queries, or <c>null</c> to use the global context.</param>
	/// <param name="filter">The filter to apply to the search results.</param>
	/// <param name="perItemCondition">A game state query which indicates whether an item produced from the other fields should be returned. Defaults to always true.</param>
	/// <param name="maxItems">The maximum number of item stacks to produce, or <c>null</c> to include all stacks produced by the <paramref name="query" />.</param>
	/// <param name="avoidRepeat">Whether to avoid adding duplicate items.</param>
	/// <param name="avoidItemIds">The qualified item IDs which shouldn't be returned.</param>
	/// <param name="logError">Log an error message to the console, given the item query and error message.</param>
	public static ItemQueryResult[] TryResolve(string query, ItemQueryContext context, ItemQuerySearchMode filter = ItemQuerySearchMode.All, string perItemCondition = null, int? maxItems = null, bool avoidRepeat = false, HashSet<string> avoidItemIds = null, Action<string, string> logError = null)
	{
		if (string.IsNullOrWhiteSpace(query))
		{
			return Helpers.ErrorResult(query, "", logError, "must specify an item ID or query");
		}
		string queryKey = query;
		string arguments = null;
		int splitIndex = query.IndexOf(' ');
		if (splitIndex > -1)
		{
			queryKey = query.Substring(0, splitIndex);
			arguments = query.Substring(splitIndex + 1);
		}
		if (context == null)
		{
			context = new ItemQueryContext();
		}
		context.QueryString = query;
		if (context.ParentContext != null)
		{
			List<string> path = new List<string>();
			for (ItemQueryContext cur = context; cur != null; cur = cur.ParentContext)
			{
				bool num = path.Contains(cur.QueryString);
				path.Add(cur.QueryString);
				if (num)
				{
					logError?.Invoke(query, "detected circular reference in item queries: " + string.Join(" -> ", path));
					return LegacyShims.EmptyArray<ItemQueryResult>();
				}
			}
		}
		IEnumerable<ItemQueryResult> results;
		if (ItemResolvers.TryGetValue(queryKey, out var resolver))
		{
			results = resolver(queryKey, arguments ?? string.Empty, context, avoidRepeat, avoidItemIds, logError ?? new Action<string, string>(LogNothing));
			if (results is ItemQueryResult[] rawArray && rawArray.Length == 0)
			{
				return rawArray;
			}
			HashSet<string> duplicates = (avoidRepeat ? new HashSet<string>() : null);
			if (!avoidRepeat)
			{
				HashSet<string> hashSet = avoidItemIds;
				if ((hashSet == null || hashSet.Count <= 0) && GameStateQuery.IsImmutablyFalse(perItemCondition))
				{
					goto IL_0174;
				}
			}
			results = results.Where(delegate(ItemQueryResult result)
			{
				HashSet<string> hashSet3 = avoidItemIds;
				if (hashSet3 == null || !hashSet3.Contains(result.Item.QualifiedItemId))
				{
					HashSet<string> hashSet4 = duplicates;
					if (hashSet4 == null || hashSet4.Add(result.Item.QualifiedItemId))
					{
						return GameStateQuery.CheckConditions(perItemCondition, null, null, result.Item as Item);
					}
				}
				return false;
			});
			goto IL_0174;
		}
		Item instance = ItemRegistry.Create(query);
		if (instance != null)
		{
			HashSet<string> hashSet2 = avoidItemIds;
			if (hashSet2 == null || !hashSet2.Contains(instance.QualifiedItemId))
			{
				return new ItemQueryResult[1]
				{
					new ItemQueryResult(instance)
				};
			}
		}
		return LegacyShims.EmptyArray<ItemQueryResult>();
		IL_0174:
		switch (filter)
		{
		case ItemQuerySearchMode.AllOfTypeItem:
			results = results.Where((ItemQueryResult result) => result.Item is Item);
			break;
		case ItemQuerySearchMode.FirstOfTypeItem:
		{
			ItemQueryResult result = results.FirstOrDefault((ItemQueryResult p) => p.Item is Item);
			results = ((result == null) ? LegacyShims.EmptyArray<ItemQueryResult>() : new ItemQueryResult[1] { result });
			break;
		}
		case ItemQuerySearchMode.RandomOfTypeItem:
		{
			ItemQueryResult result = Game1.random.ChooseFrom(results.Where((ItemQueryResult p) => p.Item is Item).ToArray());
			results = ((result == null) ? LegacyShims.EmptyArray<ItemQueryResult>() : new ItemQueryResult[1] { result });
			break;
		}
		}
		if (maxItems.HasValue)
		{
			results = results.Take(maxItems.Value);
		}
		return (results as ItemQueryResult[]) ?? results.ToArray();
	}

	/// <summary>Get the items matching spawn data from a content asset.</summary>
	/// <param name="data">The spawn data to match.</param>
	/// <param name="context">The contextual info for item queries, or <c>null</c> to use the global context.</param>
	/// <param name="filter">The filter to apply to the search results.</param>
	/// <param name="avoidRepeat">Whether to avoid adding duplicate items.</param>
	/// <param name="avoidItemIds">The qualified item IDs which shouldn't be returned.</param>
	/// <param name="formatItemId">Format the raw item ID before it's resolved. Note that this is applied after <paramref name="avoidRepeat" /> and <paramref name="avoidItemIds" /> are checked.</param>
	/// <param name="inputItem">The input item (e.g. machine input) for which to check queries, or <c>null</c> if not applicable.</param>
	/// <param name="logError">Log an error message to the console, given the item query and error message.</param>
	public static IList<ItemQueryResult> TryResolve(ISpawnItemData data, ItemQueryContext context, ItemQuerySearchMode filter = ItemQuerySearchMode.All, bool avoidRepeat = false, HashSet<string> avoidItemIds = null, Func<string, string> formatItemId = null, Action<string, string> logError = null, Item inputItem = null)
	{
		Random random = context?.Random ?? Game1.random;
		string itemId = data.ItemId;
		List<string> randomItemId = data.RandomItemId;
		if (randomItemId != null && randomItemId.Any())
		{
			if (avoidItemIds != null)
			{
				if (!Utility.TryGetRandomExcept(data.RandomItemId, avoidItemIds, random, out itemId))
				{
					return LegacyShims.EmptyArray<ItemQueryResult>();
				}
			}
			else
			{
				itemId = random.ChooseFrom(data.RandomItemId);
			}
		}
		if (formatItemId != null)
		{
			itemId = formatItemId(itemId);
		}
		ItemQueryResult[] results = TryResolve(itemId, context, filter, data.PerItemCondition, data.MaxItems, avoidRepeat, avoidItemIds, logError);
		ItemQueryResult[] array = results;
		foreach (ItemQueryResult obj in array)
		{
			obj.Item = ApplyItemFields(obj.Item, data, context, inputItem);
		}
		return results;
	}

	/// <summary>Get a random item matching an item ID or query.</summary>
	/// <param name="query">The item ID or query to match.</param>
	/// <param name="context">The contextual info for item queries, or <c>null</c> to use the global context.</param>
	/// <param name="avoidRepeat">Whether to avoid adding duplicate items.</param>
	/// <param name="avoidItemIds">The qualified item IDs which shouldn't be returned.</param>
	/// <param name="logError">Log an error message to the console, given the item query and error message.</param>
	public static Item TryResolveRandomItem(string query, ItemQueryContext context, bool avoidRepeat = false, HashSet<string> avoidItemIds = null, Action<string, string> logError = null)
	{
		return TryResolve(query, context, ItemQuerySearchMode.RandomOfTypeItem, null, null, avoidRepeat, avoidItemIds, logError).FirstOrDefault()?.Item as Item;
	}

	/// <summary>Get the items matching spawn data from a content asset.</summary>
	/// <param name="data">The spawn data to match.</param>
	/// <param name="context">The contextual info for item queries, or <c>null</c> to use the global context.</param>
	/// <param name="avoidRepeat">Whether to avoid adding duplicate items.</param>
	/// <param name="avoidItemIds">The qualified item IDs which shouldn't be returned.</param>
	/// <param name="formatItemId">Format the selected item ID before it's resolved.</param>
	/// <param name="inputItem">The input item (e.g. machine input) for which to check queries, or <c>null</c> if not applicable.</param>
	/// <param name="logError">Log an error message to the console, given the item query and error message.</param>
	public static Item TryResolveRandomItem(ISpawnItemData data, ItemQueryContext context, bool avoidRepeat = false, HashSet<string> avoidItemIds = null, Func<string, string> formatItemId = null, Item inputItem = null, Action<string, string> logError = null)
	{
		return TryResolve(data, context, ItemQuerySearchMode.RandomOfTypeItem, avoidRepeat, avoidItemIds, formatItemId, logError, inputItem).FirstOrDefault()?.Item as Item;
	}

	/// <summary>Apply data fields to an item instance.</summary>
	/// <param name="item">The item to modify.</param>
	/// <param name="data">The spawn data to apply.</param>
	/// <param name="context">The contextual info for item queries, or <c>null</c> to use the global context.</param>
	/// <remarks>This is applied automatically by methods which take an <see cref="T:StardewValley.GameData.ISpawnItemData" />, so it only needs to be called directly when creating an item from an item query string directly.</remarks>
	/// <param name="inputItem">The input item (e.g. machine input) for which to check queries, or <c>null</c> if not applicable.</param>
	/// <returns>Returns the modified item. This is usually the input <paramref name="item" />, but may be a new item instance in some cases.</returns>
	public static ISalable ApplyItemFields(ISalable item, ISpawnItemData data, ItemQueryContext context, Item inputItem = null)
	{
		return ApplyItemFields(item, data.MinStack, data.MaxStack, data.ToolUpgradeLevel, data.ObjectInternalName, data.ObjectDisplayName, data.Quality, data.IsRecipe, data.StackModifiers, data.StackModifierMode, data.QualityModifiers, data.QualityModifierMode, data.ModData, context, inputItem);
	}

	/// <summary>Apply data fields to an item instance.</summary>
	/// <param name="item">The item to modify.</param>
	/// <param name="minStackSize">The minimum stack size for the item to create, or <c>-1</c> to keep it as-is.</param>
	/// <param name="maxStackSize">The maximum stack size for the item to create, or <c>-1</c> to match <paramref name="minStackSize" />.</param>
	/// <param name="toolUpgradeLevel">For tools only, the tool upgrade level to set, or <c>-1</c> to keep it as-is.</param>
	/// <param name="objectInternalName">For objects only, the internal name to use (or <c>null</c> for the item's name in data). This should usually be null.</param>
	/// <param name="objectDisplayName">For objects only, a tokenizable string for the display name to use (or <c>null</c> for the item's default display name). See remarks on <see cref="F:StardewValley.Object.displayNameFormat" />.</param>
	/// <param name="quality">The object quality to set, or <c>-1</c> to keep it as-is.</param>
	/// <param name="isRecipe">Whether to mark the item as a recipe that can be learned by the player, instead of an instance that can be picked up.</param>
	/// <param name="stackSizeModifiers">The modifiers to apply to the item's stack size.</param>
	/// <param name="stackSizeModifierMode">How multiple <paramref name="stackSizeModifiers" /> should be combined.</param>
	/// <param name="qualityModifiers">The modifiers to apply to the item's quality.</param>
	/// <param name="qualityModifierMode">How multiple <paramref name="qualityModifiers" /> should be combined.</param>
	/// <param name="modData">Custom metadata to add to the created item's <c>modData</c> field for mod use.</param>
	/// <param name="context">The contextual info for item queries, or <c>null</c> to use the global context.</param>
	/// <param name="inputItem">The input item (e.g. machine input) for which to check queries, or <c>null</c> if not applicable.</param>
	/// <returns>Returns the modified item. This is usually the input <paramref name="item" />, but may be a new item instance in some cases.</returns>
	/// <remarks>This is applied automatically by methods which take an <see cref="T:StardewValley.GameData.ISpawnItemData" />, so it only needs to be called directly when creating an item from an item query string directly.</remarks>
	public static ISalable ApplyItemFields(ISalable item, int minStackSize, int maxStackSize, int toolUpgradeLevel, string objectInternalName, string objectDisplayName, int quality, bool isRecipe, List<QuantityModifier> stackSizeModifiers, QuantityModifier.QuantityModifierMode stackSizeModifierMode, List<QuantityModifier> qualityModifiers, QuantityModifier.QuantityModifierMode qualityModifierMode, Dictionary<string, string> modData, ItemQueryContext context, Item inputItem = null)
	{
		if (item == null)
		{
			return null;
		}
		Ring ring = item as Ring;
		if (ring != null && isRecipe)
		{
			item = new Object(ring.ItemId, ring.Stack, isRecipe: true);
		}
		int stackSize = 1;
		if (!isRecipe)
		{
			if (minStackSize == -1 && maxStackSize == -1)
			{
				stackSize = item.Stack;
			}
			else if (maxStackSize > 1)
			{
				minStackSize = Math.Max(minStackSize, 1);
				maxStackSize = Math.Max(maxStackSize, minStackSize);
				stackSize = (context?.Random ?? Game1.random).Next(minStackSize, maxStackSize + 1);
			}
			else if (minStackSize > 1)
			{
				stackSize = minStackSize;
			}
			stackSize = (int)Utility.ApplyQuantityModifiers(stackSize, stackSizeModifiers, stackSizeModifierMode, context?.Location, context?.Player, item as Item, inputItem, context?.Random);
		}
		quality = ((quality >= 0) ? quality : item.Quality);
		quality = (int)Utility.ApplyQuantityModifiers(quality, qualityModifiers, qualityModifierMode, context?.Location, context?.Player, item as Item, inputItem, context?.Random);
		if (isRecipe)
		{
			item.IsRecipe = true;
		}
		if (stackSize > -1 && stackSize != item.Stack)
		{
			item.Stack = stackSize;
			item.FixStackSize();
		}
		if (quality >= 0 && quality != item.Quality)
		{
			item.Quality = quality;
			item.FixQuality();
		}
		if (modData != null && modData.Count > 0)
		{
			(item as Item)?.modData.CopyFrom(modData);
		}
		if (!(item is Object obj))
		{
			if (item is Tool tool && toolUpgradeLevel > -1 && toolUpgradeLevel != tool.UpgradeLevel)
			{
				tool.UpgradeLevel = toolUpgradeLevel;
			}
		}
		else
		{
			if (objectInternalName != null)
			{
				obj.Name = objectInternalName;
			}
			if (objectDisplayName != null)
			{
				obj.displayNameFormat = objectDisplayName;
			}
		}
		return item;
	}

	/// <summary>A default implementation for <c>logError</c> parameters which logs nothing.</summary>
	/// <param name="query">The item query which failed.</param>
	/// <param name="error">The error indicating why it failed.</param>
	private static void LogNothing(string query, string error)
	{
	}
}
