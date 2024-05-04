using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Movies;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Triggers;
using StardewValley.Util;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley;

/// <summary>The debug commands that can be executed through the console.</summary>
public static class DebugCommands
{
	/// <summary>The low-level handlers for vanilla debug commands. Most code should call <see cref="M:StardewValley.DebugCommands.TryHandle(System.String[],StardewValley.Logging.IGameLogger)" /> instead, which adds error-handling.</summary>
	public static class DefaultHandlers
	{
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GrowWildTrees(string[] command, IGameLogger log)
		{
			TerrainFeature[] array = Game1.currentLocation.terrainFeatures.Values.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is Tree tree)
				{
					tree.growthStage.Value = 4;
					tree.fertilized.Value = true;
					tree.dayUpdate();
					tree.fertilized.Value = false;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void EventTestSpecific(string[] command, IGameLogger log)
		{
			Game1.eventTest = new EventTest(command);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void EventTest(string[] command, IGameLogger log)
		{
			Game1.eventTest = new EventTest((command.Length > 1) ? command[1] : "", (command.Length > 2) ? Convert.ToInt32(command[2]) : 0);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GetAllQuests(string[] command, IGameLogger log)
		{
			foreach (KeyValuePair<string, string> v in DataLoader.Quests(Game1.content))
			{
				Game1.player.addQuest(v.Key);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Movie(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptional(command, 1, out var movieId, out var error, null, allowBlank: false) || !ArgUtility.TryGetOptional(command, 2, out var invitedNpcName, out error, null, allowBlank: false))
			{
				log.Error(error);
				return;
			}
			if (movieId != null && !MovieTheater.TryGetMovieData(movieId, out var _))
			{
				log.Error("No movie found with ID '" + movieId + "'.");
				return;
			}
			if (invitedNpcName != null)
			{
				NPC npc = Utility.fuzzyCharacterSearch(invitedNpcName);
				if (npc != null)
				{
					MovieTheater.Invite(Game1.player, npc);
				}
				else
				{
					log.Error("No NPC found matching '" + invitedNpcName + "'.");
				}
			}
			if (movieId != null)
			{
				MovieTheater.forceMovieId = movieId;
			}
			LocationRequest locationRequest = Game1.getLocationRequest("MovieTheater");
			locationRequest.OnWarp += delegate
			{
				((MovieTheater)Game1.currentLocation).performAction("Theater_Doors", Game1.player, Location.Origin);
			};
			Game1.warpFarmer(locationRequest, 10, 10, 0);
		}

		/// <summary>Print the movie schedule for a specified year.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MovieSchedule(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var year, out var error, Game1.year))
			{
				log.Error(error);
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(20, 1, stringBuilder);
			handler.AppendLiteral("Movie schedule for ");
			handler.AppendFormatted((year == Game1.year) ? $"this year (year {year})" : $"year {year}");
			handler.AppendLiteral(":");
			StringBuilder schedule = stringBuilder.AppendLine(ref handler).AppendLine();
			Season[] array = new Season[4]
			{
				StardewValley.Season.Spring,
				StardewValley.Season.Summer,
				StardewValley.Season.Fall,
				StardewValley.Season.Winter
			};
			foreach (Season season in array)
			{
				List<Tuple<MovieData, int>> movies = new List<Tuple<MovieData, int>>();
				string lastMovieId = null;
				for (int day = 1; day <= 28; day++)
				{
					MovieData movie = MovieTheater.GetMovieForDate(new WorldDate(year, season, day));
					if (movie.Id != lastMovieId)
					{
						movies.Add(Tuple.Create(movie, day));
						lastMovieId = movie.Id;
					}
				}
				for (int i = 0; i < movies.Count; i++)
				{
					MovieData item = movies[i].Item1;
					int startDay = movies[i].Item2;
					int endDay = ((movies.Count > i + 1) ? (movies[i + 1].Item2 - 1) : 28);
					string title = TokenParser.ParseText(item.Title);
					schedule.Append(season).Append(' ').Append(startDay);
					if (endDay != startDay)
					{
						schedule.Append("-").Append(endDay);
					}
					schedule.Append(": ").AppendLine(title);
				}
			}
			log.Info(schedule.ToString());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Shop(string[] command, IGameLogger log)
		{
			string rawShopId = ArgUtility.Get(command, 1);
			string ownerName = ArgUtility.Get(command, 2);
			if (string.IsNullOrWhiteSpace(rawShopId))
			{
				log.Error("You must specify a shop ID to open.");
				return;
			}
			string shopId = Utility.fuzzySearch(rawShopId, DataLoader.Shops(Game1.content).Keys.ToArray());
			if (shopId == null)
			{
				log.Error("Couldn't find any shop in Data/Shops matching ID '" + rawShopId + "'.");
			}
			else if ((ownerName != null) ? Utility.TryOpenShopMenu(shopId, ownerName) : Utility.TryOpenShopMenu(shopId, Game1.player.currentLocation, null, null, forceOpen: true))
			{
				log.Info("Opened shop with ID '" + shopId + "'.");
			}
			else
			{
				log.Error("Failed to open shop with ID '" + shopId + "'. Is the data in Data/Shops valid?");
			}
		}

		/// <summary>Export a summary of every shop's current inventory.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ExportShops(string[] command, IGameLogger log)
		{
			StringBuilder report = new StringBuilder();
			string[] openShopArgs = new string[2] { "Shop", null };
			foreach (string shopId in DataLoader.Shops(Game1.content).Keys)
			{
				report.AppendLine(shopId);
				report.AppendLine("".PadRight(Math.Max(50, shopId.Length), '-'));
				try
				{
					openShopArgs[1] = shopId;
					Shop(openShopArgs, log);
				}
				catch (Exception ex)
				{
					StringBuilder stringBuilder = report.Append("    ");
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder);
					handler.AppendLiteral("Failed to open shop '");
					handler.AppendFormatted(shopId);
					handler.AppendLiteral("'.");
					stringBuilder2.AppendLine(ref handler);
					report.AppendLine("    " + string.Join("\n    ", ex.ToString().Split('\n')));
					continue;
				}
				if (Game1.activeClickableMenu is ShopMenu shop)
				{
					switch (shop.currency)
					{
					case 0:
						report.AppendLine("    Currency: gold");
						break;
					case 1:
						report.AppendLine("    Currency: star tokens");
						break;
					case 2:
						report.AppendLine("    Currency: Qi coins");
						break;
					case 4:
						report.AppendLine("    Currency: Qi gems");
						break;
					default:
					{
						StringBuilder stringBuilder = report;
						StringBuilder stringBuilder3 = stringBuilder;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(20, 2, stringBuilder);
						handler.AppendFormatted("    ");
						handler.AppendLiteral("Currency: unknown (");
						handler.AppendFormatted(shop.currency);
						handler.AppendLiteral(")");
						stringBuilder3.AppendLine(ref handler);
						break;
					}
					}
					report.AppendLine();
					var summary = shop.itemPriceAndStock.Select(delegate(KeyValuePair<ISalable, ItemStockInformation> entry)
					{
						ISalable key = entry.Key;
						ItemStockInformation value = entry.Value;
						return new
						{
							Id = key.QualifiedItemId,
							Name = key.DisplayName,
							Price = value.Price,
							Trade = ((value.TradeItem != null) ? (value.TradeItem + " x" + (value.TradeItemCount ?? 1)) : null),
							StockLimit = ((value.Stock != int.MaxValue && value.LimitedStockMode != LimitedStockMode.None) ? $"{value.LimitedStockMode} {value.Stock}" : null)
						};
					}).ToArray();
					int idWidth = "id".Length;
					int nameWidth = "name".Length;
					int priceWidth = "price".Length;
					int tradeWidth = "trade".Length;
					int stockWidth = "stock limit".Length;
					var array = summary;
					foreach (var entry in array)
					{
						idWidth = Math.Max(idWidth, entry.Id.Length);
						nameWidth = Math.Max(nameWidth, entry.Name.Length);
						priceWidth = Math.Max(priceWidth, entry.Price.ToString().Length);
						if (entry.Trade != null)
						{
							tradeWidth = Math.Max(tradeWidth, entry.Trade.Length);
						}
						if (entry.StockLimit != null)
						{
							tradeWidth = Math.Max(tradeWidth, entry.StockLimit.Length);
						}
					}
					report.Append("    ").Append("id".PadRight(idWidth)).Append(" | ")
						.Append("name".PadRight(nameWidth))
						.Append(" | ")
						.Append("price".PadRight(priceWidth))
						.Append(" | ")
						.Append("trade".PadRight(tradeWidth))
						.AppendLine(" | stock limit");
					report.Append("    ").Append("".PadRight(idWidth, '-')).Append(" | ")
						.Append("".PadRight(nameWidth, '-'))
						.Append(" | ")
						.Append("".PadRight(priceWidth, '-'))
						.Append(" | ")
						.Append("".PadRight(tradeWidth, '-'))
						.Append(" | ")
						.AppendLine("".PadRight(stockWidth, '-'));
					array = summary;
					foreach (var entry in array)
					{
						report.Append("    ").Append(entry.Id.PadRight(idWidth)).Append(" | ")
							.Append(entry.Name.PadRight(nameWidth))
							.Append(" | ")
							.Append(entry.Price.ToString().PadRight(priceWidth))
							.Append(" | ")
							.Append((entry.Trade ?? "").PadRight(tradeWidth))
							.Append(" | ")
							.AppendLine(entry.StockLimit);
					}
				}
				else
				{
					StringBuilder stringBuilder = report.Append("    ");
					StringBuilder stringBuilder4 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(60, 1, stringBuilder);
					handler.AppendLiteral("Failed to open shop '");
					handler.AppendFormatted(shopId);
					handler.AppendLiteral("': shop menu unexpected failed to open.");
					stringBuilder4.AppendLine(ref handler);
				}
				report.AppendLine();
				report.AppendLine();
			}
			string exportFilePath = Path.Combine(Program.GetLocalAppDataFolder("Exports"), $"{DateTime.Now:yyyy-MM-dd} shop export.txt");
			File.WriteAllText(exportFilePath, report.ToString());
			log.Info("Exported shop data to " + exportFilePath + ".");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Dating(string[] command, IGameLogger log)
		{
			Game1.player.friendshipData[command[1]].Status = FriendshipStatus.Dating;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearActiveDialogueEvents(string[] command, IGameLogger log)
		{
			Game1.player.activeDialogueEvents.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Buff(string[] command, IGameLogger log)
		{
			Game1.player.applyBuff(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearBuffs(string[] command, IGameLogger log)
		{
			Game1.player.ClearBuffs();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void PauseTime(string[] command, IGameLogger log)
		{
			Game1.isTimePaused = !Game1.isTimePaused;
			if (Game1.isTimePaused)
			{
				Game1.playSound("bigSelect");
			}
			else
			{
				Game1.playSound("bigDeSelect");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fbf" })]
		public static void FrameByFrame(string[] command, IGameLogger log)
		{
			Game1.frameByFrame = !Game1.frameByFrame;
			if (Game1.frameByFrame)
			{
				Game1.playSound("bigSelect");
			}
			else
			{
				Game1.playSound("bigDeSelect");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fbp", "fill", "fillbp" })]
		public static void FillBackpack(string[] command, IGameLogger log)
		{
			for (int i = 0; i < Game1.player.Items.Count; i++)
			{
				if (Game1.player.Items[i] != null)
				{
					continue;
				}
				ItemMetadata metadata = null;
				while (metadata == null)
				{
					metadata = ItemRegistry.ResolveMetadata(Game1.random.Next(1000).ToString());
					ParsedItemData data = metadata?.GetParsedData();
					if (data == null || data.Category == -999 || data.ObjectType == "Crafting" || data.ObjectType == "Seeds")
					{
						metadata = null;
					}
				}
				Game1.player.Items[i] = metadata.CreateItem();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Bobber(string[] command, IGameLogger log)
		{
			Game1.player.bobberStyle.Value = Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sl" })]
		public static void ShiftToolbarLeft(string[] command, IGameLogger log)
		{
			Game1.player.shiftToolbar(right: false);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sr" })]
		public static void ShiftToolbarRight(string[] command, IGameLogger log)
		{
			Game1.player.shiftToolbar(right: true);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CharacterInfo(string[] command, IGameLogger log)
		{
			Game1.showGlobalMessage(Game1.currentLocation.characters.Count + " characters on this map");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DoesItemExist(string[] command, IGameLogger log)
		{
			Game1.showGlobalMessage(Utility.doesItemExistAnywhere(command[1]) ? "Yes" : "No");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpecialItem(string[] command, IGameLogger log)
		{
			Game1.player.specialItems.Add(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AnimalInfo(string[] command, IGameLogger log)
		{
			int animalCount = 0;
			int locationCount = 0;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				int length = location.animals.Length;
				if (length > 0)
				{
					animalCount += length;
					locationCount++;
				}
				return true;
			});
			Game1.showGlobalMessage($"{animalCount} animals in {locationCount} locations");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearChildren(string[] command, IGameLogger log)
		{
			Game1.player.getRidOfChildren();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CreateSplash(string[] command, IGameLogger log)
		{
			Point offset = default(Point);
			switch (Game1.player.FacingDirection)
			{
			case 3:
				offset.X = -4;
				break;
			case 1:
				offset.X = 4;
				break;
			case 0:
				offset.Y = 4;
				break;
			case 2:
				offset.Y = -4;
				break;
			}
			Game1.player.currentLocation.fishSplashPoint.Set(new Point(Game1.player.TilePoint.X + offset.X, Game1.player.TilePoint.Y + offset.Y));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Pregnant(string[] command, IGameLogger log)
		{
			WorldDate birthingDate = Game1.Date;
			birthingDate.TotalDays++;
			Game1.player.GetSpouseFriendship().NextBirthingDate = birthingDate;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpreadSeeds(string[] command, IGameLogger log)
		{
			GameLocation location = Game1.currentLocation;
			if (location == null)
			{
				return;
			}
			foreach (KeyValuePair<Vector2, TerrainFeature> t in location.terrainFeatures.Pairs)
			{
				if (t.Value is HoeDirt dirt)
				{
					dirt.crop = new Crop(command[1], (int)t.Key.X, (int)t.Key.Y, location);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpreadDirt(string[] command, IGameLogger log)
		{
			GameLocation location = Game1.currentLocation;
			if (location == null)
			{
				return;
			}
			for (int x = 0; x < location.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < location.map.Layers[0].LayerHeight; y++)
				{
					if (location.doesTileHaveProperty(x, y, "Diggable", "Back") != null && location.CanItemBePlacedHere(new Vector2(x, y), itemIsPassable: true, CollisionMask.All, CollisionMask.None))
					{
						location.terrainFeatures.Add(new Vector2(x, y), new HoeDirt());
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveFurniture(string[] command, IGameLogger log)
		{
			Game1.currentLocation.furniture.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MakeEx(string[] command, IGameLogger log)
		{
			Game1.player.friendshipData[command[1]].RoommateMarriage = false;
			Game1.player.friendshipData[command[1]].Status = FriendshipStatus.Divorced;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DarkTalisman(string[] command, IGameLogger log)
		{
			GameLocation gameLocation = Game1.RequireLocation("Railroad");
			GameLocation witchHut = Game1.RequireLocation("WitchHut");
			Game1.player.hasDarkTalisman = true;
			gameLocation.setMapTile(54, 35, 287, "Buildings", "", 1);
			gameLocation.setMapTile(54, 34, 262, "Front", "", 1);
			witchHut.setMapTile(4, 11, 114, "Buildings", "", 1);
			witchHut.setTileProperty(4, 11, "Buildings", "Action", "MagicInk");
			Game1.player.hasMagicInk = false;
			Game1.player.mailReceived.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ConventionMode(string[] command, IGameLogger log)
		{
			Game1.conventionMode = !Game1.conventionMode;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FarmMap(string[] command, IGameLogger log)
		{
			for (int i = 0; i < Game1.locations.Count; i++)
			{
				if (Game1.locations[i] is Farm || Game1.locations[i] is FarmHouse)
				{
					Game1.locations.RemoveAt(i);
				}
			}
			Game1.whichFarm = Convert.ToInt32(command[1]);
			Game1.locations.Add(new Farm("Maps\\" + Farm.getMapNameFromTypeInt(Game1.whichFarm), "Farm"));
			Game1.locations.Add(new FarmHouse("Maps\\FarmHouse", "FarmHouse"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearMuseum(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<LibraryMuseum>("ArchaeologyHouse").museumPieces.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Clone(string[] command, IGameLogger log)
		{
			Game1.currentLocation.characters.Add(Utility.fuzzyCharacterSearch(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "zl" })]
		public static void ZoomLevel(string[] command, IGameLogger log)
		{
			Game1.options.desiredBaseZoomLevel = (float)Convert.ToInt32(command[1]) / 100f;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "us" })]
		public static void UiScale(string[] command, IGameLogger log)
		{
			Game1.options.desiredUIScale = (float)Convert.ToInt32(command[1]) / 100f;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DeleteArch(string[] command, IGameLogger log)
		{
			Game1.player.archaeologyFound.Clear();
			Game1.player.fishCaught.Clear();
			Game1.player.mineralsFound.Clear();
			Game1.player.mailReceived.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Save(string[] command, IGameLogger log)
		{
			Game1.saveOnNewDay = !Game1.saveOnNewDay;
			if (Game1.saveOnNewDay)
			{
				Game1.playSound("bigSelect");
			}
			else
			{
				Game1.playSound("bigDeSelect");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "removeLargeTf" })]
		public static void RemoveLargeTerrainFeature(string[] command, IGameLogger log)
		{
			Game1.currentLocation.largeTerrainFeatures.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Test(string[] command, IGameLogger log)
		{
			Game1.currentMinigame = new Test();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FenceDecay(string[] command, IGameLogger log)
		{
			foreach (Object value in Game1.currentLocation.objects.Values)
			{
				if (value is Fence fence)
				{
					fence.health.Value -= Convert.ToInt32(command[1]);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sb" })]
		public static void ShowTextAboveHead(string[] command, IGameLogger log)
		{
			Utility.fuzzyCharacterSearch(command[1]).showTextAboveHead(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3206"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Gamepad(string[] command, IGameLogger log)
		{
			Game1.options.gamepadControls = !Game1.options.gamepadControls;
			Game1.options.mouseControls = !Game1.options.gamepadControls;
			Game1.showGlobalMessage(Game1.options.gamepadControls ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3209") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3210"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Slimecraft(string[] command, IGameLogger log)
		{
			Game1.player.craftingRecipes.Add("Slime Incubator", 0);
			Game1.player.craftingRecipes.Add("Slime Egg-Press", 0);
			Game1.playSound("crystal", 0);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "kms" })]
		public static void KillMonsterStat(string[] command, IGameLogger log)
		{
			string monster = command[1];
			int kills = Convert.ToInt32(command[2]);
			Game1.stats.specificMonstersKilled[monster] = kills;
			log.Info(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", monster, kills));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FixAnimals(string[] command, IGameLogger log)
		{
			bool fixedAny = false;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				int num = 0;
				foreach (Building building in location.buildings)
				{
					if (building.GetIndoors() is AnimalHouse animalHouse)
					{
						foreach (FarmAnimal current in animalHouse.animals.Values)
						{
							foreach (Building current2 in location.buildings)
							{
								if (current2.GetIndoors() is AnimalHouse animalHouse2 && animalHouse2.animalsThatLiveHere.Contains(current.myID.Value) && !current2.Equals(current.home))
								{
									for (int num2 = animalHouse2.animalsThatLiveHere.Count - 1; num2 >= 0; num2--)
									{
										if (animalHouse2.animalsThatLiveHere[num2] == current.myID.Value)
										{
											animalHouse2.animalsThatLiveHere.RemoveAt(num2);
											Game1.playSound("crystal", 0);
											num++;
										}
									}
								}
							}
						}
						for (int num3 = animalHouse.animalsThatLiveHere.Count - 1; num3 >= 0; num3--)
						{
							if (Utility.getAnimal(animalHouse.animalsThatLiveHere[num3]) == null)
							{
								animalHouse.animalsThatLiveHere.RemoveAt(num3);
								Game1.playSound("crystal", 0);
								num++;
							}
						}
					}
				}
				if (num > 0)
				{
					log.Info($"Fixed {num} animals in the '{location.NameOrUniqueName}' location.");
					fixedAny = true;
				}
				return true;
			}, includeInteriors: false);
			if (!fixedAny)
			{
				log.Info("No animal issues found.");
			}
			Utility.fixAllAnimals();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DisplaceAnimals(string[] command, IGameLogger log)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location.animals.Length == 0 && location.buildings.Count == 0)
				{
					return true;
				}
				Utility.fixAllAnimals();
				foreach (Building building in location.buildings)
				{
					if (building.GetIndoors() is AnimalHouse animalHouse)
					{
						foreach (FarmAnimal current in animalHouse.animals.Values)
						{
							current.home = null;
							current.Position = Utility.recursiveFindOpenTileForCharacter(current, location, new Vector2(40f, 40f), 200) * 64f;
							location.animals.TryAdd(current.myID.Value, current);
						}
						animalHouse.animals.Clear();
						animalHouse.animalsThatLiveHere.Clear();
					}
				}
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sdkInfo" })]
		public static void SteamInfo(string[] command, IGameLogger log)
		{
			Program.sdk.DebugInfo();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Achieve(string[] command, IGameLogger log)
		{
			Program.sdk.GetAchievement(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ResetAchievements(string[] command, IGameLogger log)
		{
			Program.sdk.ResetAchievements();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Divorce(string[] command, IGameLogger log)
		{
			Game1.player.divorceTonight.Value = true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BefriendAnimals(string[] command, IGameLogger log)
		{
			foreach (FarmAnimal value in Game1.currentLocation.animals.Values)
			{
				value.friendshipTowardFarmer.Value = ((command.Length > 1) ? Convert.ToInt32(command[1]) : 1000);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void PetToFarm(string[] command, IGameLogger log)
		{
			Game1.RequireCharacter<Pet>(Game1.player.getPetName(), mustBeVillager: false).setAtFarmPosition();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BefriendPets(string[] command, IGameLogger log)
		{
			foreach (NPC allCharacter in Utility.getAllCharacters())
			{
				if (allCharacter is Pet pet)
				{
					pet.friendshipTowardFarmer.Value = 1000;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Version(string[] command, IGameLogger log)
		{
			log.Info(typeof(Game1).Assembly.GetName().Version?.ToString() ?? "");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ns" })]
		public static void NoSave(string[] command, IGameLogger log)
		{
			Game1.saveOnNewDay = !Game1.saveOnNewDay;
			if (!Game1.saveOnNewDay)
			{
				Game1.playSound("bigDeSelect");
			}
			else
			{
				Game1.playSound("bigSelect");
			}
			log.Info("Saving is now " + (Game1.saveOnNewDay ? "enabled" : "disabled"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "rfh" })]
		public static void ReadyForHarvest(string[] command, IGameLogger log)
		{
			Game1.currentLocation.objects[new Vector2(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]))].minutesUntilReady.Value = 1;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BeachBridge(string[] command, IGameLogger log)
		{
			Beach beach = Game1.RequireLocation<Beach>("Beach");
			beach.bridgeFixed.Value = !beach.bridgeFixed;
			if (!beach.bridgeFixed)
			{
				beach.setMapTile(58, 13, 284, "Buildings", null, 1);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		/// <remarks>See also <see cref="M:StardewValley.DebugCommands.DefaultHandlers.DaysPlayed(System.String[],StardewValley.Logging.IGameLogger)" />.</remarks>
		public static void Dp(string[] command, IGameLogger log)
		{
			Game1.stats.DaysPlayed = (uint)Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fo" })]
		public static void FrameOffset(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var frame, out var error) || !ArgUtility.TryGetInt(command, 2, out var offsetX, out error) || !ArgUtility.TryGetInt(command, 3, out var offsetY, out error))
			{
				log.Error("Failed parsing " + command[0] + " command: " + error);
				return;
			}
			FarmerRenderer.featureXOffsetPerFrame[frame] = (short)offsetX;
			FarmerRenderer.featureYOffsetPerFrame[frame] = (short)offsetY;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Horse(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var tileX, out var error, Game1.player.TilePoint.X) || !ArgUtility.TryGetOptionalInt(command, 1, out var tileY, out error, Game1.player.TilePoint.Y))
			{
				log.Error(error);
			}
			else
			{
				Game1.currentLocation.characters.Add(new Horse(GuidHelper.NewGuid(), tileX, tileY));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Owl(string[] command, IGameLogger log)
		{
			Game1.currentLocation.addOwl();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Pole(string[] command, IGameLogger log)
		{
			Item fishingRod = ArgUtility.GetInt(command, 1) switch
			{
				1 => ItemRegistry.Create("(T)TrainingRod"), 
				2 => ItemRegistry.Create("(T)FiberglassRod"), 
				3 => ItemRegistry.Create("(T)IridiumRod"), 
				_ => ItemRegistry.Create("(T)BambooRod"), 
			};
			Game1.player.addItemToInventoryBool(fishingRod);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveQuest(string[] command, IGameLogger log)
		{
			Game1.player.removeQuest(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CompleteQuest(string[] command, IGameLogger log)
		{
			Game1.player.completeQuest(command[1]);
		}

		/// <summary>Set the current player's preferred pet type and breed. This doesn't change any existing pets; see <see cref="M:StardewValley.DebugCommands.DefaultHandlers.ChangePet(System.String[],StardewValley.Logging.IGameLogger)" /> for that.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetPreferredPet(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var typeId, out var error, allowBlank: false) || !ArgUtility.TryGetOptional(command, 2, out var breedId, out error, null, allowBlank: false))
			{
				log.Error(error);
				return;
			}
			if (!Pet.TryGetData(typeId, out var data))
			{
				log.Error($"Can't set the player's preferred pet type to '{typeId}': no such pet type found. Expected one of ['{string.Join("', '", Game1.petData.Keys)}'].");
				return;
			}
			if (breedId != null && data.Breeds.All((PetBreed p) => p.Id != breedId))
			{
				log.Error($"Can't set the player's preferred pet breed to '{breedId}': no such breed found. Expected one of ['{string.Join("', '", data.Breeds.Select((PetBreed p) => p.Id))}'].");
				return;
			}
			bool changed = false;
			if (Game1.player.whichPetType != typeId)
			{
				log.Info($"Changed preferred pet type from '{Game1.player.whichPetType}' to '{typeId}'.");
				Game1.player.whichPetType = typeId;
				changed = true;
				if (breedId == null)
				{
					breedId = data.Breeds.FirstOrDefault()?.Id;
				}
			}
			if (breedId != null && Game1.player.whichPetBreed != breedId)
			{
				log.Info($"Changed preferred pet breed from '{Game1.player.whichPetBreed}' to '{breedId}'.");
				Game1.player.whichPetBreed = breedId;
				changed = true;
			}
			if (!changed)
			{
				log.Info("The player's pet type and breed already match those values.");
			}
		}

		/// <summary>Change the pet type and/or breed for a specific pet. This doesn't change the player's preferred pet type/breed; see <see cref="M:StardewValley.DebugCommands.DefaultHandlers.SetPreferredPet(System.String[],StardewValley.Logging.IGameLogger)" /> for that.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ChangePet(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var petName, out var error, allowBlank: false) || !ArgUtility.TryGet(command, 2, out var typeId, out error, allowBlank: false) || !ArgUtility.TryGetOptional(command, 3, out var breedId, out error, null, allowBlank: false))
			{
				log.Error(error);
				return;
			}
			if (!Pet.TryGetData(typeId, out var data))
			{
				log.Error($"Can't set the pet type to '{typeId}': no such pet type found. Expected one of ['{string.Join("', '", Game1.petData.Keys)}'].");
				return;
			}
			if (breedId != null && data.Breeds.All((PetBreed p) => p.Id != breedId))
			{
				log.Error($"Can't set the pet breed to '{breedId}': no such breed found. Expected one of ['{string.Join("', '", data.Breeds.Select((PetBreed p) => p.Id))}'].");
				return;
			}
			Pet pet = Game1.getCharacterFromName<Pet>(petName, mustBeVillager: false);
			if (pet == null)
			{
				log.Error("No pet found with name '" + petName + "'.");
				return;
			}
			bool changed = false;
			if (pet.petType.Value != typeId)
			{
				log.Info($"Changed {pet.Name}'s type from '{pet.petType.Value}' to '{typeId}'.");
				pet.petType.Value = typeId;
				changed = true;
				if (breedId == null)
				{
					breedId = data.Breeds.FirstOrDefault()?.Id;
				}
			}
			if (breedId != null && pet.whichBreed.Value != breedId)
			{
				log.Info($"Changed {pet.Name}'s breed from '{pet.whichBreed.Value}' to '{breedId}'.");
				pet.whichBreed.Value = breedId;
				changed = true;
			}
			if (!changed)
			{
				log.Info(pet.Name + "'s type and breed already match those values.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearCharacters(string[] command, IGameLogger log)
		{
			Game1.currentLocation.characters.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Cat(string[] command, IGameLogger log)
		{
			Game1.currentLocation.characters.Add(new Pet(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]), (command.Length > 3) ? command[3] : "0", "Cat"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Dog(string[] command, IGameLogger log)
		{
			Game1.currentLocation.characters.Add(new Pet(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]), (command.Length > 3) ? command[3] : "0", "Dog"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Quest(string[] command, IGameLogger log)
		{
			Game1.player.addQuest(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DeliveryQuest(string[] command, IGameLogger log)
		{
			Game1.player.questLog.Add(new ItemDeliveryQuest());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CollectQuest(string[] command, IGameLogger log)
		{
			Game1.player.questLog.Add(new ResourceCollectionQuest());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SlayQuest(string[] command, IGameLogger log)
		{
			Game1.player.questLog.Add(new SlayMonsterQuest());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Quests(string[] command, IGameLogger log)
		{
			foreach (string id in DataLoader.Quests(Game1.content).Keys)
			{
				if (!Game1.player.hasQuest(id))
				{
					Game1.player.addQuest(id);
				}
			}
			Game1.player.questLog.Add(new ItemDeliveryQuest());
			Game1.player.questLog.Add(new SlayMonsterQuest());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearQuests(string[] command, IGameLogger log)
		{
			Game1.player.questLog.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fb" })]
		public static void FillBin(string[] command, IGameLogger log)
		{
			IInventory shippingBin = Game1.getFarm().getShippingBin(Game1.player);
			shippingBin.Add(ItemRegistry.Create("(O)24"));
			shippingBin.Add(ItemRegistry.Create("(O)82"));
			shippingBin.Add(ItemRegistry.Create("(O)136"));
			shippingBin.Add(ItemRegistry.Create("(O)16"));
			shippingBin.Add(ItemRegistry.Create("(O)388"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Gold(string[] command, IGameLogger log)
		{
			Game1.player.Money += 1000000;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearFarm(string[] command, IGameLogger log)
		{
			Farm farm = Game1.getFarm();
			Layer layer = farm.map.Layers[0];
			farm.removeObjectsAndSpawned(0, 0, layer.LayerWidth, layer.LayerHeight);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetupFarm(string[] command, IGameLogger log)
		{
			Farm farm = Game1.getFarm();
			Layer layer = farm.map.Layers[0];
			farm.buildings.Clear();
			farm.AddDefaultBuildings();
			farm.removeObjectsAndSpawned(0, 0, layer.LayerWidth, 16 + ((command.Length > 1) ? 32 : 0));
			farm.removeObjectsAndSpawned(56, 17, 16, 18);
			for (int x = 58; x < 70; x++)
			{
				for (int y = 19; y < 29; y++)
				{
					farm.terrainFeatures.Add(new Vector2(x, y), new HoeDirt());
				}
			}
			if (farm.buildStructure("Coop", new Vector2(52f, 11f), Game1.player, out var coop))
			{
				coop.daysOfConstructionLeft.Value = 0;
			}
			if (farm.buildStructure("Silo", new Vector2(36f, 9f), Game1.player, out var silo))
			{
				silo.daysOfConstructionLeft.Value = 0;
			}
			if (farm.buildStructure("Barn", new Vector2(42f, 10f), Game1.player, out var barn))
			{
				barn.daysOfConstructionLeft.Value = 0;
			}
			for (int i = 0; i < Game1.player.Items.Count; i++)
			{
				if (Game1.player.Items[i] is Tool tool)
				{
					string newId = null;
					switch (tool.QualifiedItemId)
					{
					case "(T)Axe":
					case "(T)CopperAxe":
					case "(T)SteelAxe":
					case "(T)GoldAxe":
						newId = "(T)IridiumAxe";
						break;
					case "(T)Hoe":
					case "(T)CopperHoe":
					case "(T)SteelHoe":
					case "(T)GoldHoe":
						newId = "(T)IridiumHoe";
						break;
					case "(T)Pickaxe":
					case "(T)CopperPickaxe":
					case "(T)SteelPickaxe":
					case "(T)GoldPickaxe":
						newId = "(T)IridiumPickaxe";
						break;
					case "(T)WateringCan":
					case "(T)CopperWateringCan":
					case "(T)SteelWateringCan":
					case "(T)GoldWateringCan":
						newId = "(T)IridiumWateringCan";
						break;
					}
					if (newId != null)
					{
						Tool newTool = ItemRegistry.Create<Tool>(newId);
						newTool.UpgradeFrom(newTool);
						Game1.player.Items[i] = newTool;
					}
				}
			}
			Game1.player.Money += 20000;
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(T)Shears"));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(T)MilkPail"));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)472", 999));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)473", 999));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)322", 999));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)388", 999));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)390", 999));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveBuildings(string[] command, IGameLogger log)
		{
			Game1.currentLocation.buildings.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Build(string[] command, IGameLogger log)
		{
			if (Game1.currentLocation.buildStructure(command[1], (command.Length > 3) ? new Vector2(Convert.ToInt32(command[2]), Convert.ToInt32(command[3])) : new Vector2(Game1.player.TilePoint.X + 1, Game1.player.TilePoint.Y), Game1.player, out var constructed))
			{
				constructed.daysOfConstructionLeft.Value = 0;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ForceBuild(string[] command, IGameLogger log)
		{
			if (Game1.currentLocation.buildStructure(command[1], (command.Length > 3) ? new Vector2(Convert.ToInt32(command[2]), Convert.ToInt32(command[3])) : new Vector2(Game1.player.TilePoint.X + 1, Game1.player.TilePoint.Y), Game1.player, out var constructed, magicalConstruction: false, skipSafetyChecks: true))
			{
				constructed.daysOfConstructionLeft.Value = 0;
			}
		}

		[OtherNames(new string[] { "fab" })]
		public static void FinishAllBuilds(string[] command, IGameLogger log)
		{
			if (!Game1.IsMasterGame)
			{
				log.Error("Only the host can use this command.");
				return;
			}
			int count = 0;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (Building current in location.buildings)
				{
					if ((int)current.daysOfConstructionLeft > 0 || (int)current.daysUntilUpgrade > 0)
					{
						current.FinishConstruction();
						int num = count + 1;
						count = num;
					}
				}
				return true;
			});
			log.Info($"Finished constructing {count} building(s).");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "bc" })]
		public static void BuildCoop(string[] command, IGameLogger log)
		{
			if (Game1.currentLocation.buildStructure("Coop", new Vector2(Convert.ToInt32(command[1]), Convert.ToInt32(command[2])), Game1.player, out var constructed))
			{
				constructed.daysOfConstructionLeft.Value = 0;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LocalInfo(string[] command, IGameLogger log)
		{
			int grass = 0;
			int trees = 0;
			int other = 0;
			foreach (TerrainFeature t in Game1.currentLocation.terrainFeatures.Values)
			{
				if (!(t is Grass))
				{
					if (t is Tree)
					{
						trees++;
					}
					else
					{
						other++;
					}
				}
				else
				{
					grass++;
				}
			}
			string summary = "Grass:" + grass + ",  " + "Trees:" + trees + ",  " + "Other Terrain Features:" + other + ",  " + "Objects: " + Game1.currentLocation.objects.Length + ",  " + "temporarySprites: " + Game1.currentLocation.temporarySprites.Count + ",  ";
			log.Info(summary);
			Game1.drawObjectDialogue(summary);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "al" })]
		public static void AmbientLight(string[] command, IGameLogger log)
		{
			Game1.ambientLight = new Color(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]), Convert.ToInt32(command[3]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ResetMines(string[] command, IGameLogger log)
		{
			MineShaft.permanentMineChanges.Clear();
			Game1.playSound("jingle1");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "db" })]
		public static void SpeakTo(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new DialogueBox(Utility.fuzzyCharacterSearch((command.Length > 1) ? command[1] : "Pierre").CurrentDialogue.Peek());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SkullKey(string[] command, IGameLogger log)
		{
			Game1.player.hasSkullKey = true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void TownKey(string[] command, IGameLogger log)
		{
			Game1.player.HasTownKey = true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Specials(string[] command, IGameLogger log)
		{
			Game1.player.hasRustyKey = true;
			Game1.player.hasSkullKey = true;
			Game1.player.hasSpecialCharm = true;
			Game1.player.hasDarkTalisman = true;
			Game1.player.hasMagicInk = true;
			Game1.player.hasClubCard = true;
			Game1.player.canUnderstandDwarves = true;
			Game1.player.hasMagnifyingGlass = true;
			Game1.player.eventsSeen.Add("2120303");
			Game1.player.eventsSeen.Add("3910979");
			Game1.player.HasTownKey = true;
			Game1.player.stats.Set("trinketSlots", 1);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SkullGear(string[] command, IGameLogger log)
		{
			int addSlots = 36 - Game1.player.MaxItems;
			if (addSlots > 0)
			{
				Game1.player.increaseBackpackSize(addSlots);
			}
			Game1.player.hasSkullKey = true;
			Game1.player.Equip(ItemRegistry.Create<Ring>("(O)527"), Game1.player.leftRing);
			Game1.player.Equip(ItemRegistry.Create<Ring>("(O)523"), Game1.player.rightRing);
			Game1.player.Equip(ItemRegistry.Create<Boots>("(B)514"), Game1.player.boots);
			Game1.player.clearBackpack();
			Game1.player.addItemToInventory(ItemRegistry.Create("(T)IridiumPickaxe"));
			Game1.player.addItemToInventory(ItemRegistry.Create("(W)4"));
			Game1.player.addItemToInventory(ItemRegistry.Create("(O)226", 20));
			Game1.player.addItemToInventory(ItemRegistry.Create("(O)288", 20));
			Game1.player.professions.Add(24);
			Game1.player.maxHealth = 75;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearSpecials(string[] command, IGameLogger log)
		{
			Game1.player.hasRustyKey = false;
			Game1.player.hasSkullKey = false;
			Game1.player.hasSpecialCharm = false;
			Game1.player.hasDarkTalisman = false;
			Game1.player.hasMagicInk = false;
			Game1.player.hasClubCard = false;
			Game1.player.canUnderstandDwarves = false;
			Game1.player.hasMagnifyingGlass = false;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Tv(string[] command, IGameLogger log)
		{
			string itemId = Game1.random.Choose("(F)1466", "(F)1468");
			Game1.player.addItemToInventoryBool(ItemRegistry.Create(itemId));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sn" })]
		public static void SecretNote(string[] command, IGameLogger log)
		{
			Game1.player.hasMagnifyingGlass = true;
			if (command.Length > 1)
			{
				int whichNote = Convert.ToInt32(command[1]);
				Object note = ItemRegistry.Create<Object>("(O)79");
				note.name = note.name + " #" + whichNote;
				Game1.player.addItemToInventory(note);
			}
			else
			{
				Game1.player.addItemToInventory(Game1.currentLocation.tryToCreateUnseenSecretNote(Game1.player));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Child2(string[] command, IGameLogger log)
		{
			if (Game1.player.getChildrenCount() > 1)
			{
				Game1.player.getChildren()[1].Age++;
				Game1.player.getChildren()[1].reloadSprite();
			}
			else
			{
				Game1.RequireLocation<FarmHouse>("FarmHouse").characters.Add(new Child("Baby2", Game1.random.NextBool(), Game1.random.NextBool(), Game1.player));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "kid" })]
		public static void Child(string[] command, IGameLogger log)
		{
			if (Game1.player.getChildren().Count > 0)
			{
				Game1.player.getChildren()[0].Age++;
				Game1.player.getChildren()[0].reloadSprite();
			}
			else
			{
				Game1.RequireLocation<FarmHouse>("FarmHouse").characters.Add(new Child("Baby", Game1.random.NextBool(), Game1.random.NextBool(), Game1.player));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void KillAll(string[] command, IGameLogger log)
		{
			string safeCharacter = command[1];
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (!location.Equals(Game1.currentLocation))
				{
					location.characters.Clear();
				}
				else
				{
					for (int num = location.characters.Count - 1; num >= 0; num--)
					{
						if (location.characters[num].Name != safeCharacter)
						{
							location.characters.RemoveAt(num);
						}
					}
				}
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ResetWorldState(string[] command, IGameLogger log)
		{
			Game1.worldStateIDs.Clear();
			Game1.netWorldState.Value = new NetWorldState();
			Game1.game1.parseDebugInput("DeleteArch", log);
			Game1.player.mailReceived.Clear();
			Game1.player.eventsSeen.Clear();
			Game1.eventsSeenSinceLastLocationChange.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void KillAllHorses(string[] command, IGameLogger log)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				for (int num = location.characters.Count - 1; num >= 0; num--)
				{
					if (location.characters[num] is Horse)
					{
						location.characters.RemoveAt(num);
						Game1.playSound("drumkit0");
					}
				}
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DatePlayer(string[] command, IGameLogger log)
		{
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer != Game1.player && (bool)farmer.isCustomized)
				{
					Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmer.UniqueMultiplayerID).Status = FriendshipStatus.Dating;
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void EngagePlayer(string[] command, IGameLogger log)
		{
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer != Game1.player && (bool)farmer.isCustomized)
				{
					Friendship friendship = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmer.UniqueMultiplayerID);
					friendship.Status = FriendshipStatus.Engaged;
					friendship.WeddingDate = Game1.Date;
					friendship.WeddingDate.TotalDays++;
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MarryPlayer(string[] command, IGameLogger log)
		{
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (farmer != Game1.player && (bool)farmer.isCustomized)
				{
					Friendship friendship = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmer.UniqueMultiplayerID);
					friendship.Status = FriendshipStatus.Married;
					friendship.WeddingDate = Game1.Date;
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Marry(string[] command, IGameLogger log)
		{
			NPC spouse = Utility.fuzzyCharacterSearch(command[1]);
			if (spouse == null)
			{
				log.Error("No character found matching '" + command[1] + "'.");
				return;
			}
			if (!Game1.player.friendshipData.TryGetValue(spouse.Name, out var friendship))
			{
				friendship = (Game1.player.friendshipData[spouse.Name] = new Friendship());
			}
			Game1.player.changeFriendship(2500, spouse);
			Game1.player.spouse = spouse.Name;
			friendship.WeddingDate = new WorldDate(Game1.Date);
			friendship.Status = FriendshipStatus.Married;
			Game1.prepareSpouseForWedding(Game1.player);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Engaged(string[] command, IGameLogger log)
		{
			NPC spouse = Utility.fuzzyCharacterSearch(command[1]);
			if (spouse == null)
			{
				log.Error("No character found matching '" + command[1] + "'.");
				return;
			}
			if (!Game1.player.friendshipData.TryGetValue(spouse.Name, out var friendship))
			{
				friendship = (Game1.player.friendshipData[spouse.Name] = new Friendship());
			}
			Game1.player.changeFriendship(2500, spouse);
			Game1.player.spouse = spouse.Name;
			friendship.Status = FriendshipStatus.Engaged;
			WorldDate weddingDate = Game1.Date;
			weddingDate.TotalDays++;
			friendship.WeddingDate = weddingDate;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearLightGlows(string[] command, IGameLogger log)
		{
			Game1.currentLocation.lightGlows.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wp" })]
		public static void Wallpaper(string[] command, IGameLogger log)
		{
			if (command.Length > 1)
			{
				Game1.player.addItemToInventoryBool(new Wallpaper(Convert.ToInt32(command[1])));
				return;
			}
			bool floor = Game1.random.NextBool();
			Game1.player.addItemToInventoryBool(new Wallpaper(floor ? Game1.random.Next(40) : Game1.random.Next(112), floor));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearFurniture(string[] command, IGameLogger log)
		{
			Game1.currentLocation.furniture.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ff" })]
		public static void Furniture(string[] command, IGameLogger log)
		{
			if (command.Length < 2)
			{
				Item furniture = null;
				while (furniture == null)
				{
					try
					{
						furniture = ItemRegistry.Create("(F)" + Game1.random.Next(1613));
					}
					catch
					{
					}
				}
				Game1.player.addItemToInventoryBool(furniture);
			}
			else
			{
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(F)" + command[1]));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpawnCoopsAndBarns(string[] command, IGameLogger log)
		{
			if (!(Game1.currentLocation is Farm farm))
			{
				return;
			}
			int num = Convert.ToInt32(command[1]);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < 20; j++)
				{
					bool coop = Game1.random.NextBool();
					if (farm.buildStructure(coop ? "Deluxe Coop" : "Deluxe Barn", farm.getRandomTile(), Game1.player, out var building))
					{
						building.daysOfConstructionLeft.Value = 0;
						building.doAction(Utility.PointToVector2(building.animalDoor.Value) + new Vector2((int)building.tileX, (int)building.tileY), Game1.player);
						for (int k = 0; k < 16; k++)
						{
							Utility.addAnimalToFarm(new FarmAnimal(coop ? "White Chicken" : "Cow", Game1.random.Next(int.MaxValue), Game1.player.UniqueMultiplayerID));
						}
						break;
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetupFishPondFarm(string[] command, IGameLogger log)
		{
			int population = ((command.Length > 1) ? Convert.ToInt32(command[1]) : 10);
			Game1.game1.parseDebugInput("ClearFarm", log);
			for (int x = 4; x < 77; x += 6)
			{
				for (int y = 9; y < 60; y += 6)
				{
					Game1.game1.parseDebugInput($"{"Build"} \"Fish Pond\" {x} {y}", log);
				}
			}
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building is FishPond fishPond)
				{
					int fish = Game1.random.Next(128, 159);
					if (Game1.random.NextDouble() < 0.15)
					{
						fish = Game1.random.Next(698, 724);
					}
					if (Game1.random.NextDouble() < 0.05)
					{
						fish = Game1.random.Next(796, 801);
					}
					ParsedItemData data = ItemRegistry.GetData(fish.ToString());
					if (data != null && data.Category == -4)
					{
						fishPond.fishType.Value = fish.ToString();
					}
					else
					{
						fishPond.fishType.Value = Game1.random.Choose("393", "397");
					}
					fishPond.maxOccupants.Value = 10;
					fishPond.currentOccupants.Value = population;
					fishPond.GetFishObject();
				}
			}
			Game1.game1.parseDebugInput("DayUpdate 1", log);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Grass(string[] command, IGameLogger log)
		{
			GameLocation location = Game1.currentLocation;
			if (location == null)
			{
				return;
			}
			for (int x = 0; x < location.Map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < location.Map.Layers[0].LayerHeight; y++)
				{
					if (location.CanItemBePlacedHere(new Vector2(x, y), itemIsPassable: true, CollisionMask.All, CollisionMask.None))
					{
						location.terrainFeatures.Add(new Vector2(x, y), new Grass(1, 4));
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetupBigFarm(string[] command, IGameLogger log)
		{
			Farm farm = Game1.getFarm();
			Game1.game1.parseDebugInput("ClearFarm", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Coop\" 4 9", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Coop\" 10 9", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Coop\" 36 11", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Barn\" 16 9", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Barn\" 3 16", log);
			Game1.game1.parseDebugInput("Build Mill 30 20", log);
			Game1.game1.parseDebugInput("Build Stable 46 10", log);
			Game1.game1.parseDebugInput("Build Silo 54 14", log);
			Game1.game1.parseDebugInput("Build \"Junimo Hut\" 48 52", log);
			Game1.game1.parseDebugInput("Build \"Junimo Hut\" 55 52", log);
			Game1.game1.parseDebugInput("Build \"Junimo Hut\" 59 52", log);
			Game1.game1.parseDebugInput("Build \"Junimo Hut\" 65 52", log);
			foreach (Building building in farm.buildings)
			{
				if (!(building.GetIndoors() is AnimalHouse animalHouse))
				{
					continue;
				}
				BuildingData buildingData = building.GetData();
				string[] validAnimalKeys = (from p in Game1.farmAnimalData
					where p.Value.House != null && buildingData.ValidOccupantTypes.Contains(p.Value.House)
					select p.Key).ToArray();
				for (int i = 0; i < animalHouse.animalLimit.Value; i++)
				{
					if (animalHouse.isFull())
					{
						break;
					}
					FarmAnimal animal = new FarmAnimal(Game1.random.ChooseFrom(validAnimalKeys), Game1.random.Next(int.MaxValue), Game1.player.UniqueMultiplayerID);
					if (Game1.random.NextBool())
					{
						animal.growFully();
					}
					animalHouse.adoptAnimal(animal);
				}
			}
			foreach (Building building in farm.buildings)
			{
				building.doAction(Utility.PointToVector2(building.animalDoor.Value) + new Vector2((int)building.tileX, (int)building.tileY), Game1.player);
			}
			for (int x = 11; x < 23; x++)
			{
				for (int y = 14; y < 25; y++)
				{
					farm.terrainFeatures.Add(new Vector2(x, y), new Grass(1, 4));
				}
			}
			for (int x = 3; x < 23; x++)
			{
				for (int y = 57; y < 61; y++)
				{
					farm.terrainFeatures.Add(new Vector2(x, y), new Grass(1, 4));
				}
			}
			for (int y = 17; y < 25; y++)
			{
				farm.terrainFeatures.Add(new Vector2(64f, y), new Flooring("6"));
			}
			for (int x = 35; x < 64; x++)
			{
				farm.terrainFeatures.Add(new Vector2(x, 24f), new Flooring("6"));
			}
			for (int x = 38; x < 76; x++)
			{
				for (int y = 18; y < 52; y++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x, y), itemIsPassable: true, CollisionMask.All, CollisionMask.None))
					{
						HoeDirt dirt = new HoeDirt();
						farm.terrainFeatures.Add(new Vector2(x, y), dirt);
						dirt.plant((472 + Game1.random.Next(5)).ToString(), Game1.player, isFertilizer: false);
					}
				}
			}
			Game1.game1.parseDebugInput("GrowCrops 8", log);
			Vector2[] obj = new Vector2[18]
			{
				new Vector2(8f, 25f),
				new Vector2(11f, 25f),
				new Vector2(14f, 25f),
				new Vector2(17f, 25f),
				new Vector2(20f, 25f),
				new Vector2(23f, 25f),
				new Vector2(8f, 28f),
				new Vector2(11f, 28f),
				new Vector2(14f, 28f),
				new Vector2(17f, 28f),
				new Vector2(20f, 28f),
				new Vector2(23f, 28f),
				new Vector2(8f, 31f),
				new Vector2(11f, 31f),
				new Vector2(14f, 31f),
				new Vector2(17f, 31f),
				new Vector2(20f, 31f),
				new Vector2(23f, 31f)
			};
			NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = farm.terrainFeatures;
			Vector2[] array = obj;
			foreach (Vector2 tile in array)
			{
				terrainFeatures.Add(tile, new FruitTree((628 + Game1.random.Next(2)).ToString(), 4));
			}
			for (int x = 3; x < 15; x++)
			{
				for (int y = 36; y < 45; y++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x, y)))
					{
						Object keg = ItemRegistry.Create<Object>("(BC)12");
						farm.objects.Add(new Vector2(x, y), keg);
						keg.performObjectDropInAction(ItemRegistry.Create<Object>("(O)454"), probe: false, Game1.player);
					}
				}
			}
			for (int x = 16; x < 26; x++)
			{
				for (int y = 36; y < 45; y++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x, y)))
					{
						farm.objects.Add(new Vector2(x, y), ItemRegistry.Create<Object>("(BC)13"));
					}
				}
			}
			for (int x = 3; x < 15; x++)
			{
				for (int y = 47; y < 57; y++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x, y)))
					{
						farm.objects.Add(new Vector2(x, y), ItemRegistry.Create<Object>("(BC)16"));
					}
				}
			}
			for (int x = 16; x < 26; x++)
			{
				for (int y = 47; y < 57; y++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x, y)))
					{
						farm.objects.Add(new Vector2(x, y), ItemRegistry.Create<Object>("(BC)15"));
					}
				}
			}
			for (int x = 28; x < 38; x++)
			{
				for (int y = 26; y < 46; y++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x, y)))
					{
						new Torch().placementAction(farm, x * 64, y * 64, null);
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "hu", "house" })]
		public static void HouseUpgrade(string[] command, IGameLogger log)
		{
			Utility.getHomeOfFarmer(Game1.player).moveObjectsForHouseUpgrade(Convert.ToInt32(command[1]));
			Utility.getHomeOfFarmer(Game1.player).setMapForUpgradeLevel(Convert.ToInt32(command[1]));
			Game1.player.HouseUpgradeLevel = Convert.ToInt32(command[1]);
			Game1.addNewFarmBuildingMaps();
			Utility.getHomeOfFarmer(Game1.player).ReadWallpaperAndFloorTileData();
			Utility.getHomeOfFarmer(Game1.player).RefreshFloorObjectNeighbors();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "thu", "thishouse" })]
		public static void ThisHouseUpgrade(string[] command, IGameLogger log)
		{
			FarmHouse house = (Game1.currentLocation?.getBuildingAt(Game1.player.Tile + new Vector2(0f, -1f))?.GetIndoors() as FarmHouse) ?? (Game1.currentLocation as FarmHouse);
			if (house != null)
			{
				house.moveObjectsForHouseUpgrade(Convert.ToInt32(command[1]));
				house.setMapForUpgradeLevel(Convert.ToInt32(command[1]));
				house.upgradeLevel = Convert.ToInt32(command[1]);
				Game1.addNewFarmBuildingMaps();
				house.ReadWallpaperAndFloorTileData();
				house.RefreshFloorObjectNeighbors();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ci" })]
		public static void Clear(string[] command, IGameLogger log)
		{
			Game1.player.clearBackpack();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "w" })]
		public static void Wall(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<FarmHouse>("FarmHouse").SetWallpaper(command[1], null);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Floor(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<FarmHouse>("FarmHouse").SetFloor(command[1], null);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Sprinkle(string[] command, IGameLogger log)
		{
			Utility.addSprinklesToLocation(Game1.currentLocation, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, 7, 7, 2000, 100, Color.White);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearMail(string[] command, IGameLogger log)
		{
			Game1.player.mailReceived.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BroadcastMailbox(string[] command, IGameLogger log)
		{
			Game1.addMail(command[1], noLetter: false, sendToEveryone: true);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "mft" })]
		public static void MailForTomorrow(string[] command, IGameLogger log)
		{
			Game1.addMailForTomorrow(command[1], command.Length > 2);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AllMail(string[] command, IGameLogger log)
		{
			foreach (string key in DataLoader.Mail(Game1.content).Keys)
			{
				Game1.addMailForTomorrow(key);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AllMailRead(string[] command, IGameLogger log)
		{
			foreach (string key in DataLoader.Mail(Game1.content).Keys)
			{
				Game1.player.mailReceived.Add(key);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ShowMail(string[] command, IGameLogger log)
		{
			if (command.Length < 2)
			{
				log.Error("Not enough parameters, expecting: showMail <mailTitle>");
				return;
			}
			string mailTitle = command[1];
			if (!DataLoader.Mail(Game1.content).TryGetValue(mailTitle, out var mail))
			{
				mail = "";
			}
			Game1.activeClickableMenu = new LetterViewerMenu(mail, mailTitle);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "where" })]
		public static void WhereIs(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false))
			{
				log.Error(error);
				return;
			}
			List<string> lines = new List<string>();
			if (Game1.CurrentEvent != null)
			{
				foreach (NPC npc in Game1.CurrentEvent.actors)
				{
					if (Utility.fuzzyCompare(npcName, npc.Name).HasValue)
					{
						lines.Add($"{npc.Name} is in this event at ({npc.TilePoint.X}, {npc.TilePoint.Y})");
					}
				}
			}
			Utility.ForEachCharacter(delegate(NPC character)
			{
				if (Utility.fuzzyCompare(npcName, character.Name).HasValue)
				{
					lines.Add($"'{character.Name}'{(character.EventActor ? " (event actor)" : "")} is at {character.currentLocation.NameOrUniqueName} ({character.TilePoint.X}, {character.TilePoint.Y})");
				}
				return true;
			}, includeEventActors: true);
			if (lines.Any())
			{
				log.Info(string.Join("\n", lines));
			}
			else
			{
				log.Error("No NPC found matching '" + command[1] + "'.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "pm" })]
		public static void PanMode(string[] command, IGameLogger log)
		{
			if (command.Length < 2)
			{
				if (!Game1.panMode)
				{
					Game1.panMode = true;
					Game1.viewportFreeze = true;
					Game1.debugMode = true;
					Game1.game1.panFacingDirectionWait = false;
					Game1.game1.panModeString = "";
					log.Info("Screen pan mode enabled.");
				}
				else
				{
					Game1.panMode = false;
					Game1.viewportFreeze = false;
					Game1.game1.panModeString = "";
					Game1.debugMode = false;
					Game1.game1.panFacingDirectionWait = false;
					Game1.inputSimulator = null;
					log.Info("Screen pan mode disabled.");
				}
			}
			else if (Game1.panMode)
			{
				int time;
				string error;
				if (command[1] == "clear")
				{
					Game1.game1.panModeString = "";
					Game1.game1.panFacingDirectionWait = false;
				}
				else if (ArgUtility.TryGetInt(command, 1, out time, out error))
				{
					if (!Game1.game1.panFacingDirectionWait)
					{
						Game1 game = Game1.game1;
						game.panModeString = game.panModeString + ((Game1.game1.panModeString.Length > 0) ? "/" : "") + time + " ";
						log.Info(Game1.game1.panModeString + Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3191"));
					}
				}
				else
				{
					log.Error("Invalid usage. The first argument must be omitted (to toggle pan mode), 'clear', or a numeric time.");
				}
			}
			else
			{
				log.Error("Screen pan mode isn't enabled. You can enable it by using this command without arguments.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "is" })]
		public static void InputSim(string[] command, IGameLogger log)
		{
			Game1.inputSimulator = null;
			if (command.Length < 2)
			{
				log.Error("Invalid arguments, call as: inputSim <simType>");
				return;
			}
			string text = command[1].ToLower();
			if (!(text == "spamtool"))
			{
				if (text == "spamlr")
				{
					Game1.inputSimulator = new LeftRightClickSpamInputSimulator();
				}
				else
				{
					log.Error("No input simulator found for " + command[1]);
				}
			}
			else
			{
				Game1.inputSimulator = new ToolSpamInputSimulator();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Hurry(string[] command, IGameLogger log)
		{
			Utility.fuzzyCharacterSearch(command[1]).warpToPathControllerDestination();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MorePollen(string[] command, IGameLogger log)
		{
			for (int i = 0; i < Convert.ToInt32(command[1]); i++)
			{
				Game1.debrisWeather.Add(new WeatherDebris(new Vector2(Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Width), Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Height)), 0, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FillWithObject(string[] command, IGameLogger log)
		{
			string id = command[1];
			bool bigCraftable = command.Length > 2 && Convert.ToBoolean(command[2]);
			for (int y = 0; y < Game1.currentLocation.map.Layers[0].LayerHeight; y++)
			{
				for (int x = 0; x < Game1.currentLocation.map.Layers[0].LayerWidth; x++)
				{
					Vector2 loc = new Vector2(x, y);
					if (Game1.currentLocation.CanItemBePlacedHere(loc))
					{
						string typeId = (bigCraftable ? "(BC)" : "(O)");
						Game1.currentLocation.setObject(loc, ItemRegistry.Create<Object>(typeId + id));
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpawnWeeds(string[] command, IGameLogger log)
		{
			for (int i = 0; i < Convert.ToInt32(command[1]); i++)
			{
				Game1.currentLocation.spawnWeedsAndStones(1);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BusDriveBack(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<BusStop>("BusStop").busDriveBack();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BusDriveOff(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<BusStop>("BusStop").busDriveOff();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CompleteJoja(string[] command, IGameLogger log)
		{
			Game1.player.mailReceived.Add("ccCraftsRoom");
			Game1.player.mailReceived.Add("ccVault");
			Game1.player.mailReceived.Add("ccFishTank");
			Game1.player.mailReceived.Add("ccBoilerRoom");
			Game1.player.mailReceived.Add("ccPantry");
			Game1.player.mailReceived.Add("jojaCraftsRoom");
			Game1.player.mailReceived.Add("jojaVault");
			Game1.player.mailReceived.Add("jojaFishTank");
			Game1.player.mailReceived.Add("jojaBoilerRoom");
			Game1.player.mailReceived.Add("jojaPantry");
			Game1.player.mailReceived.Add("JojaMember");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CompleteCc(string[] command, IGameLogger log)
		{
			Game1.player.mailReceived.Add("ccCraftsRoom");
			Game1.player.mailReceived.Add("ccVault");
			Game1.player.mailReceived.Add("ccFishTank");
			Game1.player.mailReceived.Add("ccBoilerRoom");
			Game1.player.mailReceived.Add("ccPantry");
			Game1.player.mailReceived.Add("ccBulletin");
			Game1.player.mailReceived.Add("ccBoilerRoom");
			Game1.player.mailReceived.Add("ccPantry");
			Game1.player.mailReceived.Add("ccBulletin");
			CommunityCenter ccc = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
			for (int i = 0; i < ccc.areasComplete.Count; i++)
			{
				ccc.markAreaAsComplete(i);
				ccc.areasComplete[i] = true;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Break(string[] command, IGameLogger log)
		{
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void WhereOre(string[] command, IGameLogger log)
		{
			log.Info(Convert.ToString(Game1.currentLocation.orePanPoint.Value));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AllBundles(string[] command, IGameLogger log)
		{
			foreach (KeyValuePair<int, NetArray<bool, NetBool>> b in Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundles.FieldDict)
			{
				for (int j = 0; j < b.Value.Count; j++)
				{
					b.Value[j] = true;
				}
			}
			Game1.playSound("crystal", 0);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void JunimoGoodbye(string[] command, IGameLogger log)
		{
			if (!(Game1.currentLocation is CommunityCenter communityCenter))
			{
				log.Error("The JunimoGoodbye command must be run while inside the community center.");
			}
			else
			{
				communityCenter.junimoGoodbyeDance();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Bundle(string[] command, IGameLogger log)
		{
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
			int key = Convert.ToInt32(command[1]);
			foreach (KeyValuePair<int, NetArray<bool, NetBool>> b in communityCenter.bundles.FieldDict)
			{
				if (b.Key == key)
				{
					for (int j = 0; j < b.Value.Count; j++)
					{
						b.Value[j] = true;
					}
				}
			}
			Game1.playSound("crystal", 0);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "lu" })]
		public static void Lookup(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var search, out var error))
			{
				log.Error(error);
				return;
			}
			foreach (ParsedItemData item in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (string.Equals(item.InternalName, search, StringComparison.OrdinalIgnoreCase))
				{
					log.Info(item.InternalName + " " + item.ItemId);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CcLoadCutscene(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").restoreAreaCutscene(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CcLoad(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").loadArea(Convert.ToInt32(command[1]));
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").markAreaAsComplete(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Plaque(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").addStarToPlaque();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void JunimoStar(string[] command, IGameLogger log)
		{
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
			Junimo junimo = communityCenter.characters.OfType<Junimo>().FirstOrDefault();
			if (junimo == null)
			{
				log.Error("No Junimo found in the community center.");
			}
			else
			{
				junimo.returnToJunimoHutToFetchStar(communityCenter);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "j", "aj" })]
		public static void AddJunimo(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").addCharacter(new Junimo(new Vector2(Convert.ToInt32(command[1]), Convert.ToInt32(command[2])) * 64f, Convert.ToInt32(command[3])));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ResetJunimoNotes(string[] command, IGameLogger log)
		{
			foreach (NetArray<bool, NetBool> b in Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundles.FieldDict.Values)
			{
				for (int i = 0; i < b.Count; i++)
				{
					b[i] = false;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "jn" })]
		public static void JunimoNote(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").addJunimoNote(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void WaterColor(string[] command, IGameLogger log)
		{
			Game1.currentLocation.waterColor.Value = new Color(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]), Convert.ToInt32(command[3])) * 0.5f;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FestivalScore(string[] command, IGameLogger log)
		{
			Game1.player.festivalScore += Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AddOtherFarmer(string[] command, IGameLogger log)
		{
			Farmer f = new Farmer(new FarmerSprite("Characters\\Farmer\\farmer_base"), new Vector2(Game1.player.Position.X - 64f, Game1.player.Position.Y), 2, Dialogue.randomName(), null, isMale: true);
			f.changeShirt(Game1.random.Next(1000, 1040).ToString());
			f.changePantsColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
			f.changeHairStyle(Game1.random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
			if (Game1.random.NextBool())
			{
				f.changeHat(Game1.random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
			}
			else
			{
				Game1.player.changeHat(-1);
			}
			f.changeHairColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
			f.changeSkinColor(Game1.random.Next(16));
			f.currentLocation = Game1.currentLocation;
			Game1.otherFarmers.Add(Game1.random.Next(), f);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void PlayMusic(string[] command, IGameLogger log)
		{
			Game1.changeMusicTrack(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Jump(string[] command, IGameLogger log)
		{
			float jumpV = 8f;
			if (command.Length > 2)
			{
				jumpV = (float)Convert.ToDouble(command[2]);
			}
			if (command[1].Equals("farmer"))
			{
				Game1.player.jump(jumpV);
			}
			else
			{
				Utility.fuzzyCharacterSearch(command[1]).jump(jumpV);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Toss(string[] command, IGameLogger log)
		{
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(738, 2700f, 1, 0, Game1.player.Tile * 64f, flicker: false, flipped: false)
			{
				rotationChange = (float)Math.PI / 32f,
				motion = new Vector2(0f, -6f),
				acceleration = new Vector2(0f, 0.08f)
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Rain(string[] command, IGameLogger log)
		{
			string contextId = Game1.player.currentLocation.GetLocationContextId();
			LocationWeather weather = Game1.netWorldState.Value.GetWeatherForLocation(contextId);
			weather.IsRaining = !weather.IsRaining;
			weather.IsDebrisWeather = false;
			if (contextId == "Default")
			{
				Game1.isRaining = weather.IsRaining;
				Game1.isDebrisWeather = false;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GreenRain(string[] command, IGameLogger log)
		{
			string contextId = Game1.player.currentLocation.GetLocationContextId();
			LocationWeather weather = Game1.netWorldState.Value.GetWeatherForLocation(contextId);
			weather.IsGreenRain = !weather.IsGreenRain;
			weather.IsDebrisWeather = false;
			if (contextId == "Default")
			{
				Game1.isRaining = weather.IsRaining;
				Game1.isGreenRain = weather.IsGreenRain;
				Game1.isDebrisWeather = false;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sf" })]
		public static void SetFrame(string[] command, IGameLogger log)
		{
			Game1.player.FarmerSprite.PauseForSingleAnimation = true;
			Game1.player.FarmerSprite.setCurrentSingleAnimation(Convert.ToInt32(command[1]));
		}

		/// <summary>Immediately end the current event.</summary>
		[OtherNames(new string[] { "ee" })]
		public static void EndEvent(string[] command, IGameLogger log)
		{
			Event @event = Game1.CurrentEvent;
			if (@event == null)
			{
				log.Warn("Can't end an event because there's none playing.");
				return;
			}
			if (@event.id == "1590166")
			{
				Game1.player.mailReceived.Add("rejectedPet");
			}
			@event.skipped = true;
			@event.skipEvent();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Language(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new LanguageSelectionMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "rte" })]
		public static void RunTestEvent(string[] command, IGameLogger log)
		{
			Game1.runTestEvent();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "qb" })]
		public static void QiBoard(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new SpecialOrdersBoard("Qi");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void OrdersBoard(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new SpecialOrdersBoard();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ReturnedDonations(string[] command, IGameLogger log)
		{
			Game1.player.team.CheckReturnedDonations();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "cso" })]
		public static void CompleteSpecialOrders(string[] command, IGameLogger log)
		{
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				foreach (OrderObjective objective in specialOrder.objectives)
				{
					objective.SetCount(objective.maxCount.Value);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpecialOrder(string[] command, IGameLogger log)
		{
			Game1.player.team.AddSpecialOrder(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BoatJourney(string[] command, IGameLogger log)
		{
			Game1.currentMinigame = new BoatJourney();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Minigame(string[] command, IGameLogger log)
		{
			switch (command[1])
			{
			case "cowboy":
				Game1.updateViewportForScreenSizeChange(fullscreenChange: false, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
				Game1.currentMinigame = new AbigailGame();
				break;
			case "blastoff":
				Game1.currentMinigame = new RobotBlastoff();
				break;
			case "minecart":
				Game1.currentMinigame = new MineCart(0, 3);
				break;
			case "grandpa":
				Game1.currentMinigame = new GrandpaStory();
				break;
			case "marucomet":
				Game1.currentMinigame = new MaruComet();
				break;
			case "haleyCows":
				Game1.currentMinigame = new HaleyCowPictures();
				break;
			case "plane":
				Game1.currentMinigame = new PlaneFlyBy();
				break;
			case "slots":
				Game1.currentMinigame = new Slots();
				break;
			case "target":
				Game1.currentMinigame = new TargetGame();
				break;
			case "fishing":
				Game1.currentMinigame = new FishingGame();
				break;
			case "intro":
				Game1.currentMinigame = new Intro();
				break;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Event(string[] command, IGameLogger log)
		{
			if (command.Length <= 3)
			{
				Game1.player.eventsSeen.Clear();
			}
			GameLocation location = Utility.fuzzyLocationSearch(command[1]);
			if (location == null)
			{
				log.Error("No location with name " + command[1]);
				return;
			}
			string locationName = location.Name;
			if (locationName == "Pool")
			{
				locationName = "BathHouse_Pool";
			}
			string assetName = "Data\\Events\\" + locationName;
			KeyValuePair<string, string> entry = Game1.content.Load<Dictionary<string, string>>(assetName).ElementAt(Convert.ToInt32(command[2]));
			if (entry.Key.Contains('/'))
			{
				LocationRequest locationRequest = Game1.getLocationRequest(locationName);
				locationRequest.OnLoad += delegate
				{
					Game1.currentLocation.currentEvent = new Event(entry.Value, assetName, StardewValley.Event.SplitPreconditions(entry.Key)[0]);
				};
				Game1.warpFarmer(locationRequest, 8, 8, Game1.player.FacingDirection);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ebi" })]
		public static void EventById(string[] command, IGameLogger log)
		{
			if (command.Length < 1)
			{
				log.Error("Event ID not specified");
				return;
			}
			string eventId = command[1];
			Game1.player.eventsSeen.Remove(eventId);
			Game1.eventsSeenSinceLastLocationChange.Remove(eventId);
			if (Game1.PlayEvent(eventId, checkPreconditions: false, checkSeen: false))
			{
				log.Info("Starting event " + eventId);
			}
			else
			{
				log.Error("Event '" + eventId + "' not found.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sfe" })]
		public static void SetFarmEvent(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var eventName, out var error, allowBlank: false))
			{
				log.Error(error);
				return;
			}
			Dictionary<string, Func<FarmEvent>> farmEvents = new Dictionary<string, Func<FarmEvent>>(StringComparer.OrdinalIgnoreCase)
			{
				["dogs"] = () => new SoundInTheNightEvent(2),
				["earthquake"] = () => new SoundInTheNightEvent(4),
				["fairy"] = () => new FairyEvent(),
				["meteorite"] = () => new SoundInTheNightEvent(1),
				["owl"] = () => new SoundInTheNightEvent(3),
				["racoon"] = () => new SoundInTheNightEvent(5),
				["ufo"] = () => new SoundInTheNightEvent(0),
				["witch"] = () => new WitchEvent()
			};
			if (farmEvents.TryGetValue(eventName, out var getEvent))
			{
				Game1.farmEventOverride = getEvent();
				log.Info("Set farm event to '" + eventName + "'! The event will play if no other nightly event plays normally.");
			}
			else
			{
				log.Error("Unknown event type; expected one of '" + string.Join("', '", farmEvents.Keys) + "'.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void TestWedding(string[] command, IGameLogger log)
		{
			Event weddingEvent = Utility.getWeddingEvent(Game1.player);
			LocationRequest locationRequest = Game1.getLocationRequest("Town");
			locationRequest.OnLoad += delegate
			{
				Game1.currentLocation.currentEvent = weddingEvent;
			};
			int x = 8;
			int y = 8;
			Utility.getDefaultWarpLocation(locationRequest.Name, ref x, ref y);
			Game1.warpFarmer(locationRequest, x, y, Game1.player.FacingDirection);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Festival(string[] command, IGameLogger log)
		{
			Dictionary<string, string> festivalData = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + command[1]);
			if (festivalData != null)
			{
				string season = new string(command[1].Where(char.IsLetter).ToArray());
				int day = Convert.ToInt32(new string(command[1].Where(char.IsDigit).ToArray()));
				Game1.game1.parseDebugInput("Season " + season, log);
				Game1.game1.parseDebugInput($"{"Day"} {day}", log);
				string[] array = festivalData["conditions"].Split('/');
				int startTime = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(array[1], 0));
				Game1.game1.parseDebugInput($"{"Time"} {startTime}", log);
				string where = array[0];
				Game1.game1.parseDebugInput("Warp " + where + " 1 1", log);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ps" })]
		public static void PlaySound(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var soundId, out var error) || !ArgUtility.TryGetOptionalInt(command, 2, out var pitch, out error, -1))
			{
				log.Error("Failed parsing " + command[0] + " command: " + error);
			}
			else if (pitch > -1)
			{
				Game1.playSound(soundId, pitch);
			}
			else
			{
				Game1.playSound(soundId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LogSounds(string[] command, IGameLogger log)
		{
			Game1.sounds.LogSounds = !Game1.sounds.LogSounds;
			log.Info((Game1.sounds.LogSounds ? "Enabled" : "Disabled") + " sound logging.");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Crafting(string[] command, IGameLogger log)
		{
			foreach (string s in CraftingRecipe.craftingRecipes.Keys)
			{
				Game1.player.craftingRecipes.TryAdd(s, 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Cooking(string[] command, IGameLogger log)
		{
			foreach (string s in CraftingRecipe.cookingRecipes.Keys)
			{
				Game1.player.cookingRecipes.TryAdd(s, 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Experience(string[] command, IGameLogger log)
		{
			int which = 0;
			if (command[1].Length > 1)
			{
				switch (command[1].ToLower())
				{
				case "all":
					Game1.player.gainExperience(0, Convert.ToInt32(command[2]));
					Game1.player.gainExperience(1, Convert.ToInt32(command[2]));
					Game1.player.gainExperience(3, Convert.ToInt32(command[2]));
					Game1.player.gainExperience(2, Convert.ToInt32(command[2]));
					Game1.player.gainExperience(4, Convert.ToInt32(command[2]));
					return;
				case "farming":
					which = 0;
					break;
				case "fishing":
					which = 1;
					break;
				case "mining":
					which = 3;
					break;
				case "foraging":
					which = 2;
					break;
				case "combat":
					which = 4;
					break;
				}
			}
			else
			{
				which = Convert.ToInt32(command[1]);
			}
			Game1.player.gainExperience(which, Convert.ToInt32(command[2]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ShowExperience(string[] command, IGameLogger log)
		{
			log.Info(Game1.player.experiencePoints[Convert.ToInt32(command[1])].ToString());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Profession(string[] command, IGameLogger log)
		{
			Game1.player.professions.Add(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearFishCaught(string[] command, IGameLogger log)
		{
			Game1.player.fishCaught.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "caughtFish" })]
		public static void FishCaught(string[] command, IGameLogger log)
		{
			Game1.stats.FishCaught = (uint)Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "r" })]
		public static void ResetForPlayerEntry(string[] command, IGameLogger log)
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLocation.resetForPlayerEntry();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Fish(string[] command, IGameLogger log)
		{
			List<string> tackleIds = (Game1.player.CurrentTool as FishingRod).GetTackleQualifiedItemIDs();
			Game1.activeClickableMenu = new BobberBar(command[1], 0.5f, treasure: true, tackleIds, null, isBossFish: false);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GrowAnimals(string[] command, IGameLogger log)
		{
			foreach (FarmAnimal value in Game1.currentLocation.animals.Values)
			{
				value.growFully();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void PauseAnimals(string[] command, IGameLogger log)
		{
			foreach (FarmAnimal value in Game1.currentLocation.Animals.Values)
			{
				value.pauseTimer = int.MaxValue;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void UnpauseAnimals(string[] command, IGameLogger log)
		{
			foreach (FarmAnimal value in Game1.currentLocation.Animals.Values)
			{
				value.pauseTimer = 0;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "removetf" })]
		public static void RemoveTerrainFeatures(string[] command, IGameLogger log)
		{
			Game1.currentLocation.terrainFeatures.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MushroomTrees(string[] command, IGameLogger log)
		{
			foreach (TerrainFeature value in Game1.currentLocation.terrainFeatures.Values)
			{
				if (value is Tree tree)
				{
					tree.treeType.Value = "7";
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void TrashCan(string[] command, IGameLogger log)
		{
			Game1.player.trashCanLevel = Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FruitTrees(string[] command, IGameLogger log)
		{
			foreach (KeyValuePair<Vector2, TerrainFeature> pair in Game1.currentLocation.terrainFeatures.Pairs)
			{
				if (pair.Value is FruitTree tree)
				{
					tree.daysUntilMature.Value -= 27;
					tree.dayUpdate();
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Train(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<Railroad>("Railroad").setTrainComing(7500);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DebrisWeather(string[] command, IGameLogger log)
		{
			string contextId = Game1.player.currentLocation.GetLocationContextId();
			LocationWeather weather = Game1.netWorldState.Value.GetWeatherForLocation(contextId);
			weather.IsDebrisWeather = !weather.IsDebrisWeather;
			if (contextId == "Default")
			{
				Game1.isDebrisWeather = weather.isDebrisWeather;
			}
			Game1.debrisWeather.Clear();
			if (weather.IsDebrisWeather)
			{
				Game1.populateDebrisWeatherArray();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Speed(string[] command, IGameLogger log)
		{
			if (command.Length < 2)
			{
				log.Error("Missing parameters. Run as: 'speed <value> (minutes=30)'");
				return;
			}
			BuffEffects effects = new BuffEffects();
			effects.Speed.Value = Convert.ToInt32(command[1]);
			Game1.player.applyBuff(new Buff("debug_speed", "Debug Speed", "Debug Speed", ArgUtility.GetInt(command, 2, 30) * Game1.realMilliSecondsPerGameMinute, null, 0, effects, false));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DayUpdate(string[] command, IGameLogger log)
		{
			Game1.currentLocation.DayUpdate(Game1.dayOfMonth);
			if (command.Length > 1)
			{
				for (int i = 0; i < Convert.ToInt32(command[1]) - 1; i++)
				{
					Game1.currentLocation.DayUpdate(Game1.dayOfMonth);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FarmerDayUpdate(string[] command, IGameLogger log)
		{
			Game1.player.dayupdate(Game1.timeOfDay);
			if (command.Length > 1)
			{
				for (int i = 0; i < Convert.ToInt32(command[1]) - 1; i++)
				{
					Game1.player.dayupdate(Game1.timeOfDay);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MuseumLoot(string[] command, IGameLogger log)
		{
			foreach (ParsedItemData allDatum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				string id = allDatum.ItemId;
				string type = allDatum.ObjectType;
				if ((type == "Arch" || type == "Minerals") && !Game1.player.mineralsFound.ContainsKey(id) && !Game1.player.archaeologyFound.ContainsKey(id))
				{
					if (type == "Arch")
					{
						Game1.player.foundArtifact(id, 1);
					}
					else
					{
						Game1.player.addItemToInventoryBool(new Object(id, 1));
					}
				}
				if (Game1.player.freeSpotsInInventory() == 0)
				{
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NewMuseumLoot(string[] command, IGameLogger log)
		{
			foreach (ParsedItemData allDatum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				string itemId = allDatum.QualifiedItemId;
				if (LibraryMuseum.IsItemSuitableForDonation(itemId) && !LibraryMuseum.HasDonatedArtifact(itemId))
				{
					Game1.player.addItemToInventoryBool(ItemRegistry.Create(itemId));
				}
				if (Game1.player.freeSpotsInInventory() == 0)
				{
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CreateDebris(string[] command, IGameLogger log)
		{
			if (command.Length < 2)
			{
				log.Error("Invalid parameters; call like: createDebris <itemId>");
			}
			else
			{
				Game1.createObjectDebris(command[1], Game1.player.TilePoint.X, Game1.player.TilePoint.Y);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveDebris(string[] command, IGameLogger log)
		{
			Game1.currentLocation.debris.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveDirt(string[] command, IGameLogger log)
		{
			Game1.currentLocation.terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> pair) => pair.Value is HoeDirt);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DyeAll(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.DyePots);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DyeShirt(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new CharacterCustomization(Game1.player.shirtItem.Value);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DyePants(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new CharacterCustomization(Game1.player.pantsItem.Value);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "cmenu", "customize" })]
		public static void CustomizeMenu(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.NewGame);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CopyOutfit(string[] command, IGameLogger log)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<Item><OutfitParts>");
			if (Game1.player.hat.Value != null)
			{
				sb.Append("<Item><ItemId>" + Game1.player.hat.Value.QualifiedItemId + "</ItemId></Item>");
			}
			if (Game1.player.pantsItem.Value != null)
			{
				sb.Append("<Item><ItemId>" + Game1.player.pantsItem.Value.QualifiedItemId + "</ItemId><Color>" + Game1.player.pantsItem.Value.clothesColor.Value.R + " " + Game1.player.pantsItem.Value.clothesColor.Value.G + " " + Game1.player.pantsItem.Value.clothesColor.Value.B + "</Color></Item>");
			}
			if (Game1.player.shirtItem.Value != null)
			{
				sb.Append("<Item><ItemId>" + Game1.player.shirtItem.Value.QualifiedItemId + "</ItemId><Color>" + Game1.player.shirtItem.Value.clothesColor.Value.R + " " + Game1.player.shirtItem.Value.clothesColor.Value.G + " " + Game1.player.shirtItem.Value.clothesColor.Value.B + "</Color></Item>");
			}
			sb.Append("</OutfitParts></Item>");
			string text = sb.ToString();
			DesktopClipboard.SetText(text);
			Game1.debugOutput = text;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SkinColor(string[] command, IGameLogger log)
		{
			Game1.player.changeSkinColor(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Hat(string[] command, IGameLogger log)
		{
			Game1.player.changeHat(Convert.ToInt32(command[1]));
			Game1.playSound("coin");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Pants(string[] command, IGameLogger log)
		{
			Game1.player.changePantsColor(new Color(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]), Convert.ToInt32(command[3])));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void HairStyle(string[] command, IGameLogger log)
		{
			Game1.player.changeHairStyle(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void HairColor(string[] command, IGameLogger log)
		{
			Game1.player.changeHairColor(new Color(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]), Convert.ToInt32(command[3])));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Shirt(string[] command, IGameLogger log)
		{
			Game1.player.changeShirt(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "m", "mv" })]
		public static void MusicVolume(string[] command, IGameLogger log)
		{
			Game1.musicPlayerVolume = (float)Convert.ToDouble(command[1]);
			Game1.options.musicVolumeLevel = (float)Convert.ToDouble(command[1]);
			Game1.musicCategory.SetVolume(Game1.options.musicVolumeLevel);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveObjects(string[] command, IGameLogger log)
		{
			Game1.currentLocation.objects.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveLights(string[] command, IGameLogger log)
		{
			Game1.currentLightSources.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "i" })]
		public static void Item(string[] command, IGameLogger log)
		{
			string itemId = ArgUtility.Get(command, 1, "(O)0");
			int amount = ArgUtility.GetInt(command, 2, 1);
			int quality = ArgUtility.GetInt(command, 3);
			Item item = ItemRegistry.Create(itemId, amount, quality);
			Game1.playSound("coin");
			Game1.player.addItemToInventoryBool(item);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "iq" })]
		public static void ItemQuery(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var query, out var error))
			{
				log.Error(error);
				return;
			}
			ItemQueryResult[] result = ItemQueryResolver.TryResolve(query, null, ItemQuerySearchMode.All, null, null, avoidRepeat: false, null, delegate(string _, string queryError)
			{
				log.Error("Failed parsing that query: " + queryError);
			});
			if (result.Length == 0)
			{
				log.Info("That query did not match any items.");
				return;
			}
			ShopMenu shop = new ShopMenu("DebugItemQuery", new Dictionary<ISalable, ItemStockInformation>());
			ItemQueryResult[] array = result;
			foreach (ItemQueryResult entry in array)
			{
				shop.AddForSale(entry.Item, new ItemStockInformation(0, int.MaxValue));
			}
			Game1.activeClickableMenu = shop;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "gq" })]
		public static void GameQuery(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var query, out var error))
			{
				log.Error(error);
				return;
			}
			var rows = (from rawQuery in GameStateQuery.SplitRaw(query)
				select new
				{
					Query = rawQuery,
					Result = GameStateQuery.CheckConditions(rawQuery)
				}).ToArray();
			int queryLength = Math.Max("Query".Length, rows.Max(p => p.Query.Length));
			StringBuilder summary = new StringBuilder().AppendLine().Append("   ").Append("Query".PadRight(queryLength, ' '))
				.AppendLine(" | Result")
				.Append("   ")
				.Append("".PadRight(queryLength, '-'))
				.AppendLine(" | ------");
			bool result = true;
			var array = rows;
			foreach (var row in array)
			{
				result = result && row.Result;
				summary.Append("   ").Append(row.Query.PadRight(queryLength, ' ')).Append(" | ")
					.AppendLine(row.Result.ToString().ToLower());
			}
			summary.AppendLine().Append("Overall result: ").Append(result.ToString().ToLower())
				.AppendLine(".");
			log.Info(summary.ToString());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Tokens(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var input, out var error))
			{
				log.Error(error);
				return;
			}
			string result = TokenParser.ParseText(input);
			log.Info("Result: \"" + result + "\".");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DyeMenu(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new DyeMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Tailor(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new TailoringMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Forge(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new ForgeMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ListTags(string[] command, IGameLogger log)
		{
			if (Game1.player.CurrentItem == null)
			{
				return;
			}
			string out_string = "Tags on " + Game1.player.CurrentItem.DisplayName + ": ";
			foreach (string tag in Game1.player.CurrentItem.GetContextTags())
			{
				out_string = out_string + tag + " ";
			}
			log.Info(out_string.Trim());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void QualifiedId(string[] command, IGameLogger log)
		{
			if (Game1.player.CurrentItem != null)
			{
				string result = "Qualified ID of " + Game1.player.CurrentItem.DisplayName + ": " + Game1.player.CurrentItem.QualifiedItemId;
				log.Info(result.Trim());
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Dye(string[] command, IGameLogger log)
		{
			Color target = Color.White;
			switch (command[2].ToLower().Trim())
			{
			case "black":
				target = Color.Black;
				break;
			case "red":
				target = new Color(220, 0, 0);
				break;
			case "blue":
				target = new Color(0, 100, 220);
				break;
			case "yellow":
				target = new Color(255, 230, 0);
				break;
			case "white":
				target = Color.White;
				break;
			case "green":
				target = new Color(10, 143, 0);
				break;
			}
			float dye_strength = 1f;
			if (command.Length > 2)
			{
				dye_strength = float.Parse(command[3]);
			}
			string text = command[1].ToLower().Trim();
			if (!(text == "shirt"))
			{
				if (text == "pants" && Game1.player.pantsItem.Value != null)
				{
					Game1.player.pantsItem.Value.Dye(target, dye_strength);
				}
			}
			else if (Game1.player.shirtItem.Value != null)
			{
				Game1.player.shirtItem.Value.Dye(target, dye_strength);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GetIndex(string[] command, IGameLogger log)
		{
			Item item = Utility.fuzzyItemSearch(command[1]);
			if (item != null)
			{
				log.Info(item.DisplayName + "'s qualified ID is " + item.QualifiedItemId);
			}
			else
			{
				log.Error("No item found with name " + command[1]);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "f", "fin" })]
		public static void FuzzyItemNamed(string[] command, IGameLogger log)
		{
			int quality = 0;
			int stack_count = 1;
			if (command.Length > 2)
			{
				int.TryParse(command[2], out stack_count);
			}
			if (command.Length > 3)
			{
				int.TryParse(command[3], out quality);
			}
			Item item = Utility.fuzzyItemSearch(command[1], stack_count);
			if (item != null)
			{
				item.quality.Value = quality;
				if (item is MeleeWeapon weapon)
				{
					MeleeWeapon.attemptAddRandomInnateEnchantment(weapon, null);
				}
				Game1.player.addItemToInventory(item);
				Game1.playSound("coin");
				string type_name = item.GetType().ToString();
				if (type_name.Contains('.'))
				{
					type_name = type_name.Substring(type_name.LastIndexOf('.') + 1);
					if (item is Object obj && (bool)obj.bigCraftable)
					{
						type_name = "Big Craftable";
					}
				}
				log.Info($"Added {item.DisplayName} ({type_name})");
			}
			else
			{
				log.Error("No item found with name " + command[1]);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "in" })]
		public static void ItemNamed(string[] command, IGameLogger log)
		{
			foreach (ParsedItemData allDatum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				string id = allDatum.ItemId;
				if (string.Equals(allDatum.InternalName, command[1], StringComparison.OrdinalIgnoreCase))
				{
					Game1.player.addItemToInventory(new Object(id, (command.Length < 3) ? 1 : Convert.ToInt32(command[2]), isRecipe: false, -1, (command.Length >= 4) ? Convert.ToInt32(command[3]) : 0));
					Game1.playSound("coin");
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Achievement(string[] command, IGameLogger log)
		{
			Game1.getAchievement(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Heal(string[] command, IGameLogger log)
		{
			Game1.player.health = Game1.player.maxHealth;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Die(string[] command, IGameLogger log)
		{
			Game1.player.health = 0;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Energize(string[] command, IGameLogger log)
		{
			Game1.player.Stamina = Game1.player.MaxStamina;
			if (command.Length > 1)
			{
				Game1.player.Stamina = Convert.ToInt32(command[1]);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Exhaust(string[] command, IGameLogger log)
		{
			Game1.player.Stamina = -15f;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Warp(string[] command, IGameLogger log)
		{
			GameLocation location = Utility.fuzzyLocationSearch(command[1]);
			if (location != null)
			{
				int x = 0;
				int y = 0;
				if (command.Length >= 4)
				{
					x = Convert.ToInt32(command[2]);
					y = Convert.ToInt32(command[3]);
				}
				else
				{
					Utility.getDefaultWarpLocation(location.Name, ref x, ref y);
				}
				Game1.warpFarmer(new LocationRequest(location.NameOrUniqueName, location.uniqueName.Value != null, location), x, y, 2);
				log.Info($"Warping Game1.player to {location.NameOrUniqueName} at {x}, {y}");
			}
			else
			{
				log.Error("No location with name " + command[1]);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wh" })]
		public static void WarpHome(string[] command, IGameLogger log)
		{
			Game1.warpHome();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Money(string[] command, IGameLogger log)
		{
			Game1.player.Money = Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CatchAllFish(string[] command, IGameLogger log)
		{
			foreach (ParsedItemData itemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (itemData.ObjectType == "Fish")
				{
					Game1.player.caughtFish(itemData.ItemId, 9);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ActivateCalicoStatue(string[] command, IGameLogger log)
		{
			Game1.mine.calicoStatueSpot.Value = new Point(8, 8);
			Game1.mine.calicoStatueActivated(new NetPoint(new Point(8, 8)), Point.Zero, new Point(8, 8));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Perfection(string[] command, IGameLogger log)
		{
			Game1.game1.parseDebugInput("CompleteCc", log);
			Game1.game1.parseDebugInput("Specials", log);
			Game1.game1.parseDebugInput("FriendAll", log);
			Game1.game1.parseDebugInput("Cooking", log);
			Game1.game1.parseDebugInput("Crafting", log);
			foreach (string key in Game1.player.craftingRecipes.Keys)
			{
				Game1.player.craftingRecipes[key] = 1;
			}
			foreach (ParsedItemData item in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				string id = item.ItemId;
				if (item.ObjectType == "Fish")
				{
					Game1.player.fishCaught.Add(item.QualifiedItemId, new int[3]);
				}
				if (Object.isPotentialBasicShipped(id, item.Category, item.ObjectType))
				{
					Game1.player.basicShipped.Add(id, 1);
				}
				Game1.player.recipesCooked.Add(id, 1);
			}
			Game1.game1.parseDebugInput("Walnut 130", log);
			Game1.player.mailReceived.Add("CF_Fair");
			Game1.player.mailReceived.Add("CF_Fish");
			Game1.player.mailReceived.Add("CF_Sewer");
			Game1.player.mailReceived.Add("CF_Mines");
			Game1.player.mailReceived.Add("CF_Spouse");
			Game1.player.mailReceived.Add("CF_Statue");
			Game1.player.mailReceived.Add("museumComplete");
			Game1.player.miningLevel.Value = 10;
			Game1.player.fishingLevel.Value = 10;
			Game1.player.foragingLevel.Value = 10;
			Game1.player.combatLevel.Value = 10;
			Game1.player.farmingLevel.Value = 10;
			Farm farm = Game1.getFarm();
			farm.buildStructure("Water Obelisk", new Vector2(0f, 0f), Game1.player, out var constructed, magicalConstruction: true, skipSafetyChecks: true);
			farm.buildStructure("Earth Obelisk", new Vector2(4f, 0f), Game1.player, out constructed, magicalConstruction: true, skipSafetyChecks: true);
			farm.buildStructure("Desert Obelisk", new Vector2(8f, 0f), Game1.player, out constructed, magicalConstruction: true, skipSafetyChecks: true);
			farm.buildStructure("Island Obelisk", new Vector2(12f, 0f), Game1.player, out constructed, magicalConstruction: true, skipSafetyChecks: true);
			farm.buildStructure("Gold Clock", new Vector2(16f, 0f), Game1.player, out constructed, magicalConstruction: true, skipSafetyChecks: true);
			foreach (KeyValuePair<string, string> v in DataLoader.Monsters(Game1.content))
			{
				for (int i = 0; i < 500; i++)
				{
					Game1.stats.monsterKilled(v.Key);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Walnut(string[] command, IGameLogger log)
		{
			Game1.netWorldState.Value.GoldenWalnuts += Convert.ToInt32(command[1]);
			Game1.netWorldState.Value.GoldenWalnutsFound += Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Gem(string[] command, IGameLogger log)
		{
			Game1.player.QiGems += Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "removeNpc" })]
		public static void KillNpc(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var name, out var error, allowBlank: false))
			{
				log.Error(error);
				return;
			}
			bool anyFound = false;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				location.characters.RemoveWhere(delegate(NPC npc)
				{
					if (npc.Name == name)
					{
						log.Info("Removed " + npc.Name + " from " + location.NameOrUniqueName);
						anyFound = true;
						return true;
					}
					return false;
				});
				return true;
			});
			if (!anyFound)
			{
				log.Error("Couldn't find " + name + " in any locations.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		/// <remarks>See also <see cref="M:StardewValley.DebugCommands.DefaultHandlers.Dp(System.String[],StardewValley.Logging.IGameLogger)" />.</remarks>
		[OtherNames(new string[] { "dap" })]
		public static void DaysPlayed(string[] command, IGameLogger log)
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3332", (int)Game1.stats.DaysPlayed));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FriendAll(string[] command, IGameLogger log)
		{
			if (Game1.year == 1)
			{
				Game1.AddCharacterIfNecessary("Kent", bypassConditions: true);
				Game1.AddCharacterIfNecessary("Leo", bypassConditions: true);
			}
			Utility.ForEachVillager(delegate(NPC n)
			{
				if (!n.CanSocialize && n.Name != "Sandy" && n.Name == "Krobus")
				{
					return true;
				}
				if (n.Name == "Marlon")
				{
					return true;
				}
				if (!Game1.player.friendshipData.ContainsKey(n.Name))
				{
					Game1.player.friendshipData.Add(n.Name, new Friendship());
				}
				Game1.player.changeFriendship((command.Length > 1) ? Convert.ToInt32(command[1]) : 2500, n);
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "friend" })]
		public static void Friendship(string[] command, IGameLogger log)
		{
			NPC npc = Utility.fuzzyCharacterSearch(command[1]);
			if (npc == null)
			{
				log.Error("No character found matching '" + command[1] + "'.");
				return;
			}
			if (!Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship))
			{
				friendship = (Game1.player.friendshipData[npc.Name] = new Friendship());
			}
			friendship.Points = Convert.ToInt32(command[2]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GetStat(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var statName, out var error, allowBlank: false))
			{
				log.Error(error);
				return;
			}
			uint value = Game1.stats.Get(statName);
			log.Info($"The '{statName}' stat is set to {value}.");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetStat(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var statName, out var error, allowBlank: false) || !ArgUtility.TryGetInt(command, 2, out var newValue, out error))
			{
				log.Error(error);
				return;
			}
			Game1.stats.Set(statName, newValue);
			log.Info($"Set '{statName}' stat to {Game1.stats.Get(statName)}.");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "eventSeen" })]
		public static void SeenEvent(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var eventId, out var error, allowBlank: false) || !ArgUtility.TryGetOptionalBool(command, 2, out var seen, out error, defaultValue: true))
			{
				log.Error(error);
				return;
			}
			Game1.player.eventsSeen.Toggle(eventId, seen);
			if (!seen)
			{
				Game1.eventsSeenSinceLastLocationChange.Remove(eventId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SeenMail(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var mailId, out var error, allowBlank: false) || !ArgUtility.TryGetOptionalBool(command, 2, out var seen, out error, defaultValue: true))
			{
				log.Error(error);
			}
			else
			{
				Game1.player.mailReceived.Toggle(mailId, seen);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CookingRecipe(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var recipeName, out var error))
			{
				log.Error(error);
			}
			else
			{
				Game1.player.cookingRecipes.Add(recipeName.Trim(), 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "craftingRecipe" })]
		public static void AddCraftingRecipe(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var recipeName, out var error))
			{
				log.Error(error);
			}
			else
			{
				Game1.player.craftingRecipes.Add(recipeName.Trim(), 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void UpgradeHouse(string[] command, IGameLogger log)
		{
			Game1.player.HouseUpgradeLevel = Math.Min(3, Game1.player.HouseUpgradeLevel + 1);
			Game1.addNewFarmBuildingMaps();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void StopRafting(string[] command, IGameLogger log)
		{
			Game1.player.isRafting = false;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Time(string[] command, IGameLogger log)
		{
			Game1.timeOfDay = Convert.ToInt32(command[1]);
			Game1.outdoorLight = Color.White;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AddMinute(string[] command, IGameLogger log)
		{
			Game1.addMinute();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AddHour(string[] command, IGameLogger log)
		{
			Game1.addHour();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Water(string[] command, IGameLogger log)
		{
			foreach (TerrainFeature value in Game1.currentLocation.terrainFeatures.Values)
			{
				if (value is HoeDirt dirt)
				{
					dirt.state.Value = 1;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GrowCrops(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var days, out var error))
			{
				log.Error(error);
				return;
			}
			foreach (KeyValuePair<Vector2, TerrainFeature> pair in Game1.currentLocation.terrainFeatures.Pairs)
			{
				if (!(pair.Value is HoeDirt { crop: not null } dirt))
				{
					continue;
				}
				for (int i = 0; i < days; i++)
				{
					dirt.crop.newDay(1);
					if (dirt.crop == null)
					{
						break;
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "c", "cm" })]
		public static void CanMove(string[] command, IGameLogger log)
		{
			Game1.player.isEating = false;
			Game1.player.CanMove = true;
			Game1.player.UsingTool = false;
			Game1.player.usingSlingshot = false;
			Game1.player.FarmerSprite.PauseForSingleAnimation = false;
			if (Game1.player.CurrentTool is FishingRod fishingRod)
			{
				fishingRod.isFishing = false;
			}
			Game1.player.mount?.dismount();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Backpack(string[] command, IGameLogger log)
		{
			Game1.player.increaseBackpackSize(Math.Min(36 - Game1.player.Items.Count, Convert.ToInt32(command[1])));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Question(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var questionId, out var error, allowBlank: false) || !ArgUtility.TryGetOptionalBool(command, 2, out var seen, out error, defaultValue: true))
			{
				log.Error(error);
			}
			else
			{
				Game1.player.dialogueQuestionsAnswered.Toggle(questionId, seen);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Year(string[] command, IGameLogger log)
		{
			Game1.year = Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Day(string[] command, IGameLogger log)
		{
			Game1.stats.DaysPlayed = (uint)(Game1.seasonIndex * 28 + Convert.ToInt32(command[1]) + (Game1.year - 1) * 4 * 28);
			Game1.dayOfMonth = Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Season(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetEnum<Season>(command, 1, out var season, out var error))
			{
				log.Error("Invalid usage: " + error);
				return;
			}
			Game1.season = season;
			Game1.setGraphicsForSeason();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "dialogue" })]
		public static void AddDialogue(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var search, out var error, allowBlank: false) || !ArgUtility.TryGetRemainder(command, 2, out var dialogueText, out error))
			{
				log.Error("Invalid usage: " + error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(search);
			if (npc == null)
			{
				log.Error("No NPC found matching search '" + search + "'.");
			}
			else
			{
				Game1.DrawDialogue(new Dialogue(npc, null, dialogueText));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Speech(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var search, out var error, allowBlank: false) || !ArgUtility.TryGetRemainder(command, 2, out var dialogueText, out error))
			{
				log.Error("Invalid usage: " + error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(search);
			if (npc == null)
			{
				log.Error("No NPC found matching search '" + search + "'.");
			}
			else
			{
				Game1.DrawDialogue(new Dialogue(npc, null, dialogueText));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LoadDialogue(string[] command, IGameLogger log)
		{
			NPC npc = Utility.fuzzyCharacterSearch(command[1]);
			string translationKey = command[2];
			string text = Game1.content.LoadString(translationKey).Replace("{", "<").Replace("}", ">");
			npc.CurrentDialogue.Push(new Dialogue(npc, translationKey, text));
			Game1.drawDialogue(npc);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Wedding(string[] command, IGameLogger log)
		{
			Game1.player.spouse = command[1];
			Game1.weddingsToday.Add(Game1.player.UniqueMultiplayerID);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GameMode(string[] command, IGameLogger log)
		{
			Game1.setGameMode(Convert.ToByte(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Volcano(string[] command, IGameLogger log)
		{
			Game1.warpFarmer(VolcanoDungeon.GetLevelName(Convert.ToInt32(command[1])), 0, 1, 2);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MineLevel(string[] command, IGameLogger log)
		{
			Game1.enterMine(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MineInfo(string[] command, IGameLogger log)
		{
			log.Info($"MineShaft.lowestLevelReached = {MineShaft.lowestLevelReached}\nplayer.deepestMineLevel = {Game1.player.deepestMineLevel}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Viewport(string[] command, IGameLogger log)
		{
			Game1.viewport.X = Convert.ToInt32(command[1]) * 64;
			Game1.viewport.Y = Convert.ToInt32(command[2]) * 64;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MakeInedible(string[] command, IGameLogger log)
		{
			if (Game1.player.ActiveObject != null)
			{
				Game1.player.ActiveObject.edibility.Value = -300;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "watm" })]
		public static void WarpAnimalToMe(string[] command, IGameLogger log)
		{
			FarmAnimal target_character = Utility.fuzzyAnimalSearch(command[1]);
			if (target_character != null)
			{
				log.Info("Warping " + target_character.displayName);
				target_character.currentLocation.Animals.Remove(target_character.myID.Value);
				Game1.currentLocation.Animals.Add(target_character.myID.Value, target_character);
				target_character.Position = Game1.player.Position;
				target_character.controller = null;
			}
			else
			{
				log.Info("Couldn't find character named " + command[1]);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wctm" })]
		public static void WarpCharacterToMe(string[] command, IGameLogger log)
		{
			NPC target_character = Utility.fuzzyCharacterSearch(command[1], must_be_villager: false);
			if (target_character != null)
			{
				log.Info("Warping " + target_character.displayName);
				Game1.warpCharacter(target_character, Game1.currentLocation.Name, new Vector2(Game1.player.TilePoint.X, Game1.player.TilePoint.Y));
				target_character.controller = null;
				target_character.Halt();
			}
			else
			{
				log.Error("Couldn't find character named " + command[1]);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wc" })]
		public static void WarpCharacter(string[] command, IGameLogger log)
		{
			NPC target_character = Utility.fuzzyCharacterSearch(command[1], must_be_villager: false);
			if (target_character == null)
			{
				return;
			}
			if (command.Length < 4)
			{
				log.Error("Missing parameters, run as: 'wc <npcName> <x> <y> [facingDirection=1]'");
				return;
			}
			int facingDirection = 2;
			if (command.Length >= 5)
			{
				facingDirection = Convert.ToInt32(command[4]);
			}
			Game1.warpCharacter(target_character, Game1.currentLocation.Name, new Vector2(Convert.ToInt32(command[2]), Convert.ToInt32(command[3])));
			target_character.faceDirection(facingDirection);
			target_character.controller = null;
			target_character.Halt();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wtp" })]
		public static void WarpToPlayer(string[] command, IGameLogger log)
		{
			if (command.Length < 2)
			{
				log.Error("Missing parameters, run as: 'wtp <playerName>'");
				return;
			}
			string cleanedName = command[1].ToLower();
			Farmer otherFarmer = null;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (farmer.displayName.ToLower() == cleanedName)
				{
					otherFarmer = farmer;
					break;
				}
			}
			if (otherFarmer == null)
			{
				log.Error("Could not find other farmer " + command[1]);
				return;
			}
			Game1.game1.parseDebugInput($"{"Warp"} {otherFarmer.currentLocation.NameOrUniqueName} {otherFarmer.TilePoint.X} {otherFarmer.TilePoint.Y}", log);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wtc" })]
		public static void WarpToCharacter(string[] command, IGameLogger log)
		{
			if (command.Length < 2)
			{
				log.Error("Missing parameters, run as: 'wtc <npcName>'");
				return;
			}
			NPC target_character = Utility.fuzzyCharacterSearch(command[1]);
			if (target_character == null)
			{
				log.Error("Could not find valid character " + command[1]);
				return;
			}
			Game1.game1.parseDebugInput($"{"Warp"} {Utility.getGameLocationOfCharacter(target_character).Name} {target_character.TilePoint.X} {target_character.TilePoint.Y}", log);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wct" })]
		public static void WarpCharacterTo(string[] command, IGameLogger log)
		{
			NPC target_character = Utility.fuzzyCharacterSearch(command[1]);
			if (target_character == null)
			{
				return;
			}
			if (command.Length < 5)
			{
				log.Error("Missing parameters, run as: 'wct <npcName> <locationName> <x> <y> [facingDirection=1]'");
				return;
			}
			int facingDirection = 2;
			if (command.Length >= 6)
			{
				facingDirection = Convert.ToInt32(command[4]);
			}
			Game1.warpCharacter(target_character, command[2], new Vector2(Convert.ToInt32(command[3]), Convert.ToInt32(command[4])));
			target_character.faceDirection(facingDirection);
			target_character.controller = null;
			target_character.Halt();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ws" })]
		public static void WarpShop(string[] command, IGameLogger log)
		{
			if (command.Length < 2)
			{
				log.Error("Missing argument. Run as: 'warpshop <npcname>'");
				return;
			}
			switch (command[1].ToLower())
			{
			case "pierre":
				Game1.game1.parseDebugInput("Warp SeedShop 4 19", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Pierre SeedShop 4 17", log);
				break;
			case "robin":
				Game1.game1.parseDebugInput("Warp ScienceHouse 8 20", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Robin ScienceHouse 8 18", log);
				break;
			case "krobus":
				Game1.game1.parseDebugInput("Warp Sewer 31 19", log);
				break;
			case "sandy":
				Game1.game1.parseDebugInput("Warp SandyHouse 2 7", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Sandy SandyHouse 2 5", log);
				break;
			case "marnie":
				Game1.game1.parseDebugInput("Warp AnimalShop 12 16", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Marnie AnimalShop 12 14", log);
				break;
			case "clint":
				Game1.game1.parseDebugInput("Warp Blacksmith 3 15", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Clint Blacksmith 3 13", log);
				break;
			case "gus":
				Game1.game1.parseDebugInput("Warp Saloon 10 20", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Gus Saloon 10 18", log);
				break;
			case "willy":
				Game1.game1.parseDebugInput("Warp FishShop 6 6", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Willy FishShop 6 4", log);
				break;
			case "pam":
				Game1.game1.parseDebugInput("Warp BusStop 7 12", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Pam BusStop 11 10", log);
				break;
			case "dwarf":
				Game1.game1.parseDebugInput("Warp Mine 43 7", log);
				break;
			case "wizard":
				Game1.player.eventsSeen.Add("418172");
				Game1.player.hasMagicInk = true;
				Game1.game1.parseDebugInput("Warp WizardHouse 2 14", log);
				break;
			default:
				log.Error("That npc doesn't have a shop or it isn't handled by this command");
				break;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FacePlayer(string[] command, IGameLogger log)
		{
			Utility.fuzzyCharacterSearch(command[1]).faceTowardFarmer = true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Refuel(string[] command, IGameLogger log)
		{
			if (Game1.player.getToolFromName("Lantern") != null)
			{
				((Lantern)Game1.player.getToolFromName("Lantern")).fuelLeft = 100;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Lantern(string[] command, IGameLogger log)
		{
			Game1.player.Items.Add(ItemRegistry.Create("(T)Lantern"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GrowGrass(string[] command, IGameLogger log)
		{
			Game1.currentLocation.spawnWeeds(weedsOnly: false);
			Game1.currentLocation.growWeedGrass(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AddAllCrafting(string[] command, IGameLogger log)
		{
			foreach (string s in CraftingRecipe.craftingRecipes.Keys)
			{
				Game1.player.craftingRecipes.Add(s, 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Animal(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var animalName, out var error))
			{
				log.Error(error);
			}
			else
			{
				Utility.addAnimalToFarm(new FarmAnimal(animalName.Trim(), Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MoveBuilding(string[] command, IGameLogger log)
		{
			GameLocation location = Game1.currentLocation;
			if (location != null)
			{
				Building building = location.getBuildingAt(new Vector2(Convert.ToInt32(command[1]), Convert.ToInt32(command[2])));
				if (building != null)
				{
					building.tileX.Value = Convert.ToInt32(command[3]);
					building.tileY.Value = Convert.ToInt32(command[4]);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Fishing(string[] command, IGameLogger log)
		{
			Game1.player.fishingLevel.Value = Convert.ToInt32(command[1]);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fd", "face" })]
		public static void FaceDirection(string[] command, IGameLogger log)
		{
			if (command[1].Equals("farmer"))
			{
				Game1.player.Halt();
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.player.faceDirection(Convert.ToInt32(command[2]));
			}
			else
			{
				Utility.fuzzyCharacterSearch(command[1]).faceDirection(Convert.ToInt32(command[2]));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Note(string[] command, IGameLogger log)
		{
			if (!Game1.player.archaeologyFound.TryGetValue("102", out var data))
			{
				data = (Game1.player.archaeologyFound["102"] = new int[2]);
			}
			data[0] = 18;
			Game1.netWorldState.Value.LostBooksFound = 18;
			Game1.currentLocation.readNote(Convert.ToInt32(command[1]));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetHost(string[] command, IGameLogger log)
		{
			Game1.multiplayer.StartServer();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetJoin(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new FarmhandMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LevelUp(string[] command, IGameLogger log)
		{
			if (command.Length > 3)
			{
				Game1.activeClickableMenu = new LevelUpMenu(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]));
			}
			else
			{
				Game1.activeClickableMenu = new LevelUpMenu(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Darts(string[] command, IGameLogger log)
		{
			Game1.currentMinigame = new Darts();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MineGame(string[] command, IGameLogger log)
		{
			int game_mode = 3;
			if (command.Length >= 2 && command[1] == "infinite")
			{
				game_mode = 2;
			}
			Game1.currentMinigame = new MineCart(0, game_mode);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Crane(string[] command, IGameLogger log)
		{
			Game1.currentMinigame = new CraneGame();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "trlt" })]
		public static void TailorRecipeListTool(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new TailorRecipeListTool();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "apt" })]
		public static void AnimationPreviewTool(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new AnimationPreviewTool();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CreateDino(string[] command, IGameLogger log)
		{
			Game1.currentLocation.characters.Add(new DinoMonster(Game1.player.position.Value + new Vector2(100f, 0f)));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Action(string[] command, IGameLogger log)
		{
			string error;
			Exception ex;
			if (!ArgUtility.TryGetRemainder(command, 1, out var action, out var _))
			{
				log.Error("invalid usage: requires an action to perform");
			}
			else if (TriggerActionManager.TryRunAction(action, out error, out ex))
			{
				log.Info("Applied action '" + action + "'.");
			}
			else
			{
				log.Error("Couldn't apply action '" + action + "': " + error, ex);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BroadcastMail(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var mailId, out var error))
			{
				log.Error(error);
			}
			else
			{
				Game1.addMailForTomorrow(mailId, noLetter: false, sendToEveryone: true);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Phone(string[] command, IGameLogger log)
		{
			Game1.game1.ShowTelephoneMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Renovate(string[] command, IGameLogger log)
		{
			HouseRenovation.ShowRenovationMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Crib(string[] command, IGameLogger log)
		{
			if (Game1.getLocationFromName(Game1.player.homeLocation.Value) is FarmHouse house)
			{
				int style = Convert.ToInt32(command[1]);
				house.cribStyle.Value = style;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void TestNut(string[] command, IGameLogger log)
		{
			Game1.createItemDebris(ItemRegistry.Create("(O)73"), Vector2.Zero, 2);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ShuffleBundles(string[] command, IGameLogger log)
		{
			Game1.GenerateBundles(Game1.BundleType.Remixed, use_seed: false);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Split(string[] command, IGameLogger log)
		{
			if (command.Length >= 2)
			{
				int player_index = int.Parse(command[1]);
				GameRunner.instance.AddGameInstance((PlayerIndex)player_index);
			}
			else
			{
				Game1.game1.ShowLocalCoopJoinMenu();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "bsm" })]
		public static void SkinBuilding(string[] command, IGameLogger log)
		{
			Building building = Game1.currentLocation?.getBuildingAt(Game1.player.Tile + new Vector2(0f, -1f));
			if (building != null)
			{
				if (building.CanBeReskinned())
				{
					Game1.activeClickableMenu = new BuildingSkinMenu(building);
				}
				else
				{
					log.Error("The '" + building.buildingType.Value + "' building in front of the player can't be skinned.");
				}
			}
			else
			{
				log.Error("No building found in front of player.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "bpm" })]
		public static void PaintBuilding(string[] command, IGameLogger log)
		{
			Building building = Game1.currentLocation?.getBuildingAt(Game1.player.Tile + new Vector2(0f, -1f));
			if (building != null)
			{
				if (building.CanBePainted())
				{
					Game1.activeClickableMenu = new BuildingPaintMenu(building);
					return;
				}
				log.Error("The '" + building.buildingType.Value + "' building in front of the player can't be painted. Defaulting to main farmhouse.");
			}
			Building farmhouse = Game1.getFarm().GetMainFarmHouse();
			if (farmhouse == null)
			{
				log.Error("The main farmhouse wasn't found.");
			}
			else if (!farmhouse.CanBePainted())
			{
				log.Error("The main farmhouse can't be painted.");
			}
			else
			{
				Game1.activeClickableMenu = new BuildingPaintMenu(farmhouse);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "md" })]
		public static void MineDifficulty(string[] command, IGameLogger log)
		{
			if (command.Length > 1)
			{
				Game1.netWorldState.Value.MinesDifficulty = Convert.ToInt32(command[1]);
			}
			log.Info($"Mine difficulty: {Game1.netWorldState.Value.MinesDifficulty}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "scd" })]
		public static void SkullCaveDifficulty(string[] command, IGameLogger log)
		{
			if (command.Length > 1)
			{
				Game1.netWorldState.Value.SkullCavesDifficulty = Convert.ToInt32(command[1]);
			}
			log.Info($"Skull Cave difficulty: {Game1.netWorldState.Value.SkullCavesDifficulty}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "tls" })]
		public static void ToggleLightingScale(string[] command, IGameLogger log)
		{
			Game1.game1.useUnscaledLighting = !Game1.game1.useUnscaledLighting;
			log.Info($"Toggled Lighting Scale: useUnscaledLighting: {Game1.game1.useUnscaledLighting}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FixWeapons(string[] command, IGameLogger log)
		{
			SaveMigrator_1_5.ResetForges();
			log.Info("Reset forged weapon attributes.");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "pdb" })]
		public static void PrintGemBirds(string[] command, IGameLogger log)
		{
			log.Info($"Gem birds: North {IslandGemBird.GetBirdTypeForLocation("IslandNorth")} South {IslandGemBird.GetBirdTypeForLocation("IslandSouth")} East {IslandGemBird.GetBirdTypeForLocation("IslandEast")} West {IslandGemBird.GetBirdTypeForLocation("IslandWest")}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ppp" })]
		public static void PrintPlayerPos(string[] command, IGameLogger log)
		{
			log.Info($"Player tile position is {Game1.player.Tile} (World position: {Game1.player.Position})");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ShowPlurals(string[] command, IGameLogger log)
		{
			List<string> item_names = new List<string>();
			foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				item_names.Add(data.InternalName);
			}
			foreach (ParsedItemData data in ItemRegistry.RequireTypeDefinition("(BC)").GetAllData())
			{
				item_names.Add(data.InternalName);
			}
			item_names.Sort();
			foreach (string item_name in item_names)
			{
				log.Info(Lexicon.makePlural(item_name));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void HoldItem(string[] command, IGameLogger log)
		{
			Game1.player.holdUpItemThenMessage(Game1.player.CurrentItem, showMessage: false);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "rm" })]
		public static void RunMacro(string[] command, IGameLogger log)
		{
			string macro_file = ArgUtility.GetRemainder(command, 1, "macro.txt");
			if (Game1.isRunningMacro)
			{
				log.Error("You cannot run a macro from within a macro.");
				return;
			}
			Game1.isRunningMacro = true;
			try
			{
				StreamReader file = new StreamReader(macro_file);
				string line;
				while ((line = file.ReadLine()) != null)
				{
					Game1.chatBox.textBoxEnter(line);
				}
				log.Info("Executed macro file " + macro_file);
				file.Close();
			}
			catch (Exception e)
			{
				log.Error("Error running macro file " + macro_file + ".", e);
			}
			Game1.isRunningMacro = false;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void InviteMovie(string[] command, IGameLogger log)
		{
			if (command.Length < 2)
			{
				log.Error("invalid usage: expected NPC name.");
				return;
			}
			NPC invited_npc = Utility.fuzzyCharacterSearch(command[1]);
			if (invited_npc == null)
			{
				log.Error("Invalid NPC");
			}
			else
			{
				MovieTheater.Invite(Game1.player, invited_npc);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Monster(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var typeName, out var error, allowBlank: false) || !ArgUtility.TryGetPoint(command, 2, out var tile, out error) || !ArgUtility.TryGetOptionalRemainder(command, 4, out var monsterNameOrNumber))
			{
				log.Error("You must specify a search term.");
				return;
			}
			string fullTypeName = "StardewValley.Monsters." + typeName;
			Type monsterType = Type.GetType(fullTypeName);
			if ((object)monsterType == null)
			{
				log.Error("There's no monster with type '" + fullTypeName + "'.");
				return;
			}
			Vector2 pos = new Vector2(tile.X * 64, tile.Y * 64);
			int numberArg;
			object[] args = (string.IsNullOrWhiteSpace(monsterNameOrNumber) ? new object[1] { pos } : ((!int.TryParse(monsterNameOrNumber, out numberArg)) ? new object[2] { pos, monsterNameOrNumber } : new object[2] { pos, numberArg }));
			Monster mon = Activator.CreateInstance(monsterType, args) as Monster;
			Game1.currentLocation.characters.Add(mon);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "shaft" })]
		public static void Ladder(string[] command, IGameLogger log)
		{
			if (command.Length > 1)
			{
				Game1.mine.createLadderDown(Convert.ToInt32(command[1]), Convert.ToInt32(command[2]), command[0] == "shaft");
			}
			else
			{
				Game1.mine.createLadderDown(Game1.player.TilePoint.X, Game1.player.TilePoint.Y + 1, command[0] == "shaft");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetLog(string[] command, IGameLogger log)
		{
			Game1.multiplayer.logging.IsLogging = !Game1.multiplayer.logging.IsLogging;
			log.Info("Turned " + (Game1.multiplayer.logging.IsLogging ? "on" : "off") + " network write logging");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetClear(string[] command, IGameLogger log)
		{
			Game1.multiplayer.logging.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetDump(string[] command, IGameLogger log)
		{
			log.Info("Wrote log to " + Game1.multiplayer.logging.Dump());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LogBandwidth(string[] command, IGameLogger log)
		{
			if (Game1.IsServer)
			{
				Game1.server.LogBandwidth = !Game1.server.LogBandwidth;
				log.Info("Turned " + (Game1.server.LogBandwidth ? "on" : "off") + " server bandwidth logging");
			}
			else if (Game1.IsClient)
			{
				Game1.client.LogBandwidth = !Game1.client.LogBandwidth;
				log.Info("Turned " + (Game1.client.LogBandwidth ? "on" : "off") + " client bandwidth logging");
			}
			else
			{
				log.Error("Cannot toggle bandwidth logging in non-multiplayer games");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ChangeWallet(string[] command, IGameLogger log)
		{
			if (Game1.IsMasterGame)
			{
				Game1.player.changeWalletTypeTonight.Value = true;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SeparateWallets(string[] command, IGameLogger log)
		{
			if (Game1.IsMasterGame)
			{
				ManorHouse.SeparateWallets();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MergeWallets(string[] command, IGameLogger log)
		{
			if (Game1.IsMasterGame)
			{
				ManorHouse.MergeWallets();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "nd", "newDay" })]
		public static void Sleep(string[] command, IGameLogger log)
		{
			Game1.player.isInBed.Value = true;
			Game1.player.sleptInTemporaryBed.Value = true;
			Game1.currentLocation.answerDialogueAction("Sleep_Yes", null);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "gm", "inv" })]
		public static void Invincible(string[] command, IGameLogger log)
		{
			if (Game1.player.temporarilyInvincible)
			{
				Game1.player.temporaryInvincibilityTimer = 0;
				Game1.playSound("bigDeSelect");
			}
			else
			{
				Game1.player.temporarilyInvincible = true;
				Game1.player.temporaryInvincibilityTimer = -1000000000;
				Game1.playSound("bigSelect");
			}
		}

		/// <summary>Toggle whether multiplayer sync fields should run detailed validation to detect possible bugs. See remarks on <see cref="F:Netcode.NetFields.ShouldValidateNetFields" />.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ValidateNetFields(string[] command, IGameLogger log)
		{
			NetFields.ShouldValidateNetFields = !NetFields.ShouldValidateNetFields;
			log.Info(NetFields.ShouldValidateNetFields ? "Enabled net field validation, which may impact performance. This only affects new net fields created after it's enabled." : "Disabled net field validation.");
		}

		/// <summary>Filter the saves shown in the current load or co-op menu based on a search term.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "flm" })]
		public static void FilterLoadMenu(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var filter, out var _))
			{
				log.Error("You must specify a search term.");
				return;
			}
			if (Game1.activeClickableMenu is TitleMenu)
			{
				IClickableMenu subMenu = TitleMenu.subMenu;
				if (subMenu is CoopMenu coopMenu)
				{
					TitleMenu.subMenu = new CoopMenu(coopMenu.tooManyFarms, coopMenu.currentTab, filter);
					return;
				}
				if (!(subMenu is FarmhandMenu) && subMenu is LoadGameMenu)
				{
					TitleMenu.subMenu = new LoadGameMenu(filter);
					return;
				}
			}
			log.Error("The FilterLoadMenu debug command must be run while the list of saved games is open.");
		}

		/// <summary>Toggle the <see cref="F:StardewValley.Menus.MapPage.EnableDebugLines" /> option.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void WorldMapLines(string[] command, IGameLogger log)
		{
			MapPage.WorldMapDebugLineType types;
			if (command.Length > 1)
			{
				if (!Utility.TryParseEnum<MapPage.WorldMapDebugLineType>(string.Join(", ", command.Skip(1)), out types))
				{
					log.Error($"Unknown type '{string.Join(" ", command.Skip(1))}', expected space-delimited list of {string.Join(", ", Enum.GetNames(typeof(MapPage.WorldMapDebugLineType)))}.");
					return;
				}
			}
			else
			{
				types = ((MapPage.EnableDebugLines == MapPage.WorldMapDebugLineType.None) ? MapPage.WorldMapDebugLineType.All : MapPage.WorldMapDebugLineType.None);
			}
			MapPage.EnableDebugLines = types;
			if (types == MapPage.WorldMapDebugLineType.None)
			{
				log.Info("World map debug lines disabled.");
				return;
			}
			log.Info($"World map debug lines enabled for types {types}.");
		}

		/// <summary>List debug commands in the game.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Search(string[] command, IGameLogger log)
		{
			string search = ArgUtility.Get(command, 1);
			ILookup<string, string> aliasesByName = Aliases.ToLookup((KeyValuePair<string, string> p) => p.Value, (KeyValuePair<string, string> p) => p.Key);
			List<string> commands = new List<string>();
			foreach (string name in Handlers.Keys.OrderBy<string, string>((string p) => p, StringComparer.OrdinalIgnoreCase))
			{
				string[] aliases = aliasesByName[name].ToArray();
				commands.Add((aliases.Length != 0) ? (name + " (" + string.Join(", ", aliases.OrderBy<string, string>((string p) => p, StringComparer.OrdinalIgnoreCase)) + ")") : name);
			}
			if (search != null)
			{
				commands.RemoveAll((string line) => !Utility.fuzzyCompare(search, line).HasValue);
			}
			if (commands.Count == 0)
			{
				log.Info("No debug commands found matching '" + search + "'.");
				return;
			}
			log.Info(((search != null) ? $"Found {commands.Count} debug commands matching search term '{search}':\n" : $"{commands.Count} debug commands registered:\n") + "  - " + string.Join("\n  - ", commands) + ((search == null) ? "\n\nTip: you can search debug commands like 'debug Search searchTermHere'." : ""));
		}

		/// <summary>Add artifact spots in every available spot in a 9x9 grid around the player.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ArtifactSpots(string[] commands, IGameLogger log)
		{
			GameLocation location = Game1.player.currentLocation;
			Vector2 playerTile = Game1.player.Tile;
			if (location == null)
			{
				log.Info("You must be in a location to use this command.");
				return;
			}
			int spawned = 0;
			Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(playerTile);
			foreach (Vector2 tile in surroundingTileLocationsArray)
			{
				if (location.terrainFeatures.TryGetValue(tile, out var feature) && feature is HoeDirt { crop: null })
				{
					location.terrainFeatures.Remove(tile);
				}
				if (location.isTilePassable(tile) && !location.IsTileOccupiedBy(tile, ~(CollisionMask.Characters | CollisionMask.Farmers | CollisionMask.TerrainFeatures)))
				{
					location.objects.Add(tile, ItemRegistry.Create<Object>("(O)590"));
					spawned++;
				}
			}
			if (spawned == 0)
			{
				log.Info("No unoccupied tiles found around the player.");
				return;
			}
			log.Info($"Spawned {spawned} artifact spots around the player.");
		}

		/// <summary>Enable or disable writing messages to the debug log file.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LogFile(string[] command, IGameLogger log)
		{
			if (Game1.log is DefaultLogger logger)
			{
				Game1.log = new DefaultLogger(logger.ShouldWriteToConsole, !logger.ShouldWriteToLogFile);
				log.Info((logger.ShouldWriteToLogFile ? "Disabled" : "Enabled") + " the game log file at " + Program.GetDebugLogPath() + ".");
			}
			else if (Game1.log?.GetType().FullName?.StartsWith("StardewModdingAPI.") ?? false)
			{
				log.Error("The debug log can't be enabled when SMAPI is installed. SMAPI already includes log messages in its own log file.");
			}
			else
			{
				log.Error("The debug log can't be enabled: the game logger has been replaced with unknown implementation '" + Game1.log?.GetType()?.FullName + "'.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ToggleCheats(string[] command, IGameLogger log)
		{
			Program.enableCheats = !Program.enableCheats;
			log.Info((Program.enableCheats ? "Enabled" : "Disabled") + " in-game cheats.");
		}
	}

	/// <summary>The supported tokens and their resolvers.</summary>
	private static readonly Dictionary<string, DebugCommandHandlerDelegate> Handlers;

	/// <summary>Alternate names for debug commands (e.g. shorthand or acronyms).</summary>
	private static readonly Dictionary<string, string> Aliases;

	/// <summary>Register the default debug commands, defined as <see cref="T:StardewValley.DebugCommands.DefaultHandlers" /> methods.</summary>
	static DebugCommands()
	{
		Handlers = new Dictionary<string, DebugCommandHandlerDelegate>(StringComparer.OrdinalIgnoreCase);
		Aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		MethodInfo[] methods = typeof(DefaultHandlers).GetMethods(BindingFlags.Static | BindingFlags.Public);
		MethodInfo[] array = methods;
		foreach (MethodInfo method in array)
		{
			try
			{
				Handlers[method.Name] = (DebugCommandHandlerDelegate)Delegate.CreateDelegate(typeof(DebugCommandHandlerDelegate), method);
			}
			catch (Exception ex)
			{
				Game1.log.Error("Failed to initialize debug command " + method.Name + ".", ex);
			}
		}
		array = methods;
		foreach (MethodInfo method in array)
		{
			OtherNamesAttribute attribute = method.GetCustomAttribute<OtherNamesAttribute>();
			if (attribute == null)
			{
				continue;
			}
			string[] aliases = attribute.Aliases;
			foreach (string alias in aliases)
			{
				if (Handlers.ContainsKey(alias))
				{
					Game1.log.Error($"Can't register alias '{alias}' for debug command '{method.Name}', because there's a command with that name.");
				}
				if (Aliases.TryGetValue(alias, out var conflictingName))
				{
					Game1.log.Error($"Can't register alias '{alias}' for debug command '{method.Name}', because that's already an alias for '{conflictingName}'.");
				}
				Aliases[alias] = method.Name;
			}
		}
	}

	/// <summary>Try to handle a debug command.</summary>
	/// <param name="command">The full debug command split by spaces, including the command name and arguments.</param>
	/// <param name="log">The log to which to write command output, or <c>null</c> to use <see cref="F:StardewValley.Game1.log" />.</param>
	/// <returns>Returns whether the command was found and executed, regardless of whether the command logic succeeded.</returns>
	public static bool TryHandle(string[] command, IGameLogger log = null)
	{
		if (log == null)
		{
			log = Game1.log;
		}
		string commandName = ArgUtility.Get(command, 0);
		if (string.IsNullOrWhiteSpace(commandName))
		{
			log.Error("Can't parse an empty command.");
			return false;
		}
		if (Aliases.TryGetValue(commandName, out var aliasTarget))
		{
			commandName = aliasTarget;
		}
		if (!Handlers.TryGetValue(commandName, out var handler))
		{
			log.Error("Unknown debug command '" + commandName + "'.");
			return false;
		}
		try
		{
			handler(command, log);
			return true;
		}
		catch (Exception ex)
		{
			log.Error("Error running debug command '" + string.Join(" ", command) + "'.", ex);
			return false;
		}
	}
}
