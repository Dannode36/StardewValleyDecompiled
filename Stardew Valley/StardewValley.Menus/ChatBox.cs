using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Logging;
using StardewValley.SaveMigrations;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus;

public class ChatBox : IClickableMenu
{
	public const int chatMessage = 0;

	public const int errorMessage = 1;

	public const int userNotificationMessage = 2;

	public const int privateMessage = 3;

	public const int defaultMaxMessages = 10;

	public const int timeToDisplayMessages = 600;

	public const int chatboxWidth = 896;

	public const int chatboxHeight = 56;

	public const int region_chatBox = 101;

	public const int region_emojiButton = 102;

	public ChatTextBox chatBox;

	public ClickableComponent chatBoxCC;

	/// <summary>A logger which copies messages to the chat box, used when entering commands through the chat.</summary>
	private readonly IGameLogger CheatCommandChatLogger;

	private List<ChatMessage> messages = new List<ChatMessage>();

	private KeyboardState oldKBState;

	private List<string> cheatHistory = new List<string>();

	private int cheatHistoryPosition = -1;

	public int maxMessages = 10;

	public static Texture2D emojiTexture;

	public ClickableTextureComponent emojiMenuIcon;

	public EmojiMenu emojiMenu;

	public bool choosingEmoji;

	private long lastReceivedPrivateMessagePlayerId;

	public ChatBox()
	{
		CheatCommandChatLogger = new CheatCommandChatLogger(this);
		Texture2D chatboxTexture = Game1.content.Load<Texture2D>("LooseSprites\\chatBox");
		chatBox = new ChatTextBox(chatboxTexture, null, Game1.smallFont, Color.White);
		chatBox.OnEnterPressed += textBoxEnter;
		chatBox.TitleText = "Chat";
		chatBoxCC = new ClickableComponent(new Rectangle(chatBox.X, chatBox.Y, chatBox.Width, chatBox.Height), "")
		{
			myID = 101
		};
		Game1.keyboardDispatcher.Subscriber = chatBox;
		emojiTexture = Game1.content.Load<Texture2D>("LooseSprites\\emojis");
		emojiMenuIcon = new ClickableTextureComponent(new Rectangle(0, 0, 40, 36), emojiTexture, new Rectangle(0, 0, 9, 9), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		emojiMenu = new EmojiMenu(this, emojiTexture, chatboxTexture);
		chatBoxCC.rightNeighborID = 102;
		updatePosition();
		chatBox.Selected = false;
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID(101);
		snapCursorToCurrentSnappedComponent();
	}

	private void updatePosition()
	{
		chatBox.Width = 896;
		chatBox.Height = 56;
		width = chatBox.Width;
		height = chatBox.Height;
		xPositionOnScreen = 0;
		yPositionOnScreen = Game1.uiViewport.Height - chatBox.Height;
		Utility.makeSafe(ref xPositionOnScreen, ref yPositionOnScreen, chatBox.Width, chatBox.Height);
		chatBox.X = xPositionOnScreen;
		chatBox.Y = yPositionOnScreen;
		chatBoxCC.bounds = new Rectangle(chatBox.X, chatBox.Y, chatBox.Width, chatBox.Height);
		emojiMenuIcon.bounds.Y = chatBox.Y + 8;
		emojiMenuIcon.bounds.X = chatBox.Width - emojiMenuIcon.bounds.Width - 8;
		if (emojiMenu != null)
		{
			emojiMenu.xPositionOnScreen = emojiMenuIcon.bounds.Center.X - 146;
			emojiMenu.yPositionOnScreen = emojiMenuIcon.bounds.Y - 248;
		}
	}

	public virtual void textBoxEnter(string text_to_send)
	{
		if (text_to_send.Length < 1)
		{
			return;
		}
		if (text_to_send[0] == '/')
		{
			string text = ArgUtility.SplitBySpaceAndGet(text_to_send, 0);
			if (text != null && text.Length > 1)
			{
				runCommand(text_to_send.Substring(1));
				return;
			}
		}
		text_to_send = Program.sdk.FilterDirtyWords(text_to_send);
		Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, text_to_send, Multiplayer.AllPlayers);
		receiveChatMessage(Game1.player.UniqueMultiplayerID, 0, LocalizedContentManager.CurrentLanguageCode, text_to_send);
	}

	public virtual void textBoxEnter(TextBox sender)
	{
		bool include_color_information;
		if (sender is ChatTextBox box)
		{
			if (box.finalText.Count > 0)
			{
				include_color_information = true;
				string message = box.finalText[0].message;
				if (message != null && message.StartsWith('/'))
				{
					string text = ArgUtility.SplitBySpaceAndGet(box.finalText[0].message, 0);
					if (text != null && text.Length > 1)
					{
						include_color_information = false;
					}
				}
				if (box.finalText.Count != 1)
				{
					goto IL_00c8;
				}
				if (box.finalText[0].message != null || box.finalText[0].emojiIndex != -1)
				{
					string message2 = box.finalText[0].message;
					if (message2 == null || message2.Trim().Length != 0)
					{
						goto IL_00c8;
					}
				}
			}
			goto IL_00dc;
		}
		goto IL_00e9;
		IL_00e9:
		sender.Text = "";
		clickAway();
		return;
		IL_00dc:
		box.reset();
		cheatHistoryPosition = -1;
		goto IL_00e9;
		IL_00c8:
		string textToSend = ChatMessage.makeMessagePlaintext(box.finalText, include_color_information);
		textBoxEnter(textToSend);
		goto IL_00dc;
	}

	public virtual void addInfoMessage(string message)
	{
		receiveChatMessage(0L, 2, LocalizedContentManager.CurrentLanguageCode, message);
	}

	public virtual void globalInfoMessage(string messageKey, params string[] args)
	{
		if (Game1.IsMultiplayer)
		{
			Game1.multiplayer.globalChatInfoMessage(messageKey, args);
		}
		else
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_" + messageKey, args));
		}
	}

	public virtual void addErrorMessage(string message)
	{
		receiveChatMessage(0L, 1, LocalizedContentManager.CurrentLanguageCode, message);
	}

	public virtual void listPlayers(bool otherPlayersOnly = false)
	{
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UserList"));
		foreach (Farmer f in Game1.getOnlineFarmers())
		{
			if (!otherPlayersOnly || f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UserListUser", formattedUserNameLong(f)));
			}
		}
	}

	public virtual void showHelp()
	{
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_Help"));
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpClear", "clear"));
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpList", "list"));
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpColor", "color"));
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpColorList", "color-list"));
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpPause", "pause"));
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpResume", "resume"));
		if (Game1.IsMultiplayer)
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpMessage", "message"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpReply", "reply"));
		}
		if (Game1.IsServer)
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpKick", "kick"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpBan", "ban"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpUnban", "unban"));
		}
	}

	protected virtual void runCommand(string command)
	{
		string[] split = ArgUtility.SplitBySpace(command);
		switch (split[0])
		{
		case "qi":
			if (Game1.player.mailReceived.Add("QiChat1"))
			{
				addMessage(Game1.content.LoadString("Strings\\UI:Chat_Qi1"), new Color(100, 50, 255));
			}
			else if (Game1.player.mailReceived.Add("QiChat2"))
			{
				addMessage(Game1.content.LoadString("Strings\\UI:Chat_Qi2"), new Color(100, 50, 255));
				addMessage(Game1.content.LoadString("Strings\\UI:Chat_Qi3"), Color.Yellow);
			}
			break;
		case "ape":
		case "concernedape":
		case "ConcernedApe":
		case "ca":
			if (Game1.player.mailReceived.Add("apeChat1"))
			{
				addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApe"), new Color(104, 214, 255));
			}
			else
			{
				addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApe2"), Color.Yellow);
			}
			break;
		case "dm":
		case "pm":
		case "message":
		case "whisper":
			sendPrivateMessage(command);
			break;
		case "reply":
		case "r":
			replyPrivateMessage(command);
			break;
		case "showmethemoney":
		case "imacheat":
		case "cheat":
		case "cheats":
		case "freegold":
		case "rosebud":
			addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApeNiceTry"), new Color(104, 214, 255));
			break;
		case "debug":
		{
			string cheatCommand;
			string error;
			if (!Program.enableCheats)
			{
				addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApeNiceTry"), new Color(104, 214, 255));
			}
			else if (!ArgUtility.TryGetRemainder(split, 1, out cheatCommand, out error))
			{
				addErrorMessage("invalid usage: requires a debug command to run");
			}
			else
			{
				cheat(cheatCommand, isDebug: true);
			}
			break;
		}
		case "logfile":
			cheat("LogFile");
			break;
		case "pause":
			if (!Game1.IsMasterGame)
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_HostOnlyCommand"));
				break;
			}
			Game1.netWorldState.Value.IsPaused = !Game1.netWorldState.Value.IsPaused;
			if (Game1.netWorldState.Value.IsPaused)
			{
				globalInfoMessage("Paused");
			}
			else
			{
				globalInfoMessage("Resumed");
			}
			break;
		case "resume":
			if (!Game1.IsMasterGame)
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_HostOnlyCommand"));
			}
			else if (Game1.netWorldState.Value.IsPaused)
			{
				Game1.netWorldState.Value.IsPaused = false;
				globalInfoMessage("Resumed");
			}
			break;
		case "printdiag":
		{
			StringBuilder sb = new StringBuilder();
			Program.AppendDiagnostics(sb);
			addInfoMessage(sb.ToString());
			Game1.log.Info(sb.ToString());
			break;
		}
		case "color":
			if (split.Length > 1)
			{
				Game1.player.defaultChatColor = split[1];
			}
			break;
		case "color-list":
			addMessage("white, red, blue, green, jade, yellowgreen, pink, purple, yellow, orange, brown, gray, cream, salmon, peach, aqua, jungle, plum", Color.White);
			break;
		case "clear":
			messages.Clear();
			break;
		case "list":
		case "users":
		case "players":
			listPlayers();
			break;
		case "help":
		case "h":
			showHelp();
			break;
		case "kick":
			if (Game1.IsMultiplayer && Game1.IsServer)
			{
				kickPlayer(command);
			}
			break;
		case "ban":
			if (Game1.IsMultiplayer && Game1.IsServer)
			{
				banPlayer(command);
			}
			break;
		case "unban":
			if (Game1.IsServer)
			{
				unbanPlayer(command);
			}
			break;
		case "unbanAll":
		case "unbanall":
			if (Game1.IsServer)
			{
				if (Game1.bannedUsers.Count == 0)
				{
					addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayersList_None"));
				}
				else
				{
					unbanAll();
				}
			}
			break;
		case "ping":
		{
			if (!Game1.IsMultiplayer)
			{
				break;
			}
			StringBuilder sb = new StringBuilder();
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> farmer in Game1.otherFarmers)
				{
					sb.Clear();
					sb.AppendFormat("Ping({0}) {1}ms ", farmer.Value.Name, (int)Game1.server.getPingToClient(farmer.Key));
					addMessage(sb.ToString(), Color.White);
				}
				break;
			}
			sb.AppendFormat("Ping: {0}ms", (int)Game1.client.GetPingToHost());
			addMessage(sb.ToString(), Color.White);
			break;
		}
		case "mapscreenshot":
			if (Game1.game1.CanTakeScreenshots())
			{
				int scale = 25;
				string screenshot_name = null;
				if (split.Length > 2 && !int.TryParse(split[2], out scale))
				{
					scale = 25;
				}
				if (split.Length > 1)
				{
					screenshot_name = split[1];
				}
				if (scale <= 10)
				{
					scale = 10;
				}
				string result = Game1.game1.takeMapScreenshot((float)scale / 100f, screenshot_name, null);
				if (result != null)
				{
					addMessage("Wrote '" + result + "'.", Color.White);
				}
				else
				{
					addMessage("Failed.", Color.Red);
				}
			}
			break;
		case "mbp":
		case "movepermission":
		case "movebuildingpermission":
			if (!Game1.IsMasterGame)
			{
				break;
			}
			if (split.Length > 1)
			{
				switch (split[1])
				{
				case "off":
					Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.Off;
					break;
				case "owned":
					Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.OwnedBuildings;
					break;
				case "on":
					Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.On;
					break;
				}
				addMessage("movebuildingpermission " + Game1.player.team.farmhandsCanMoveBuildings.Value, Color.White);
			}
			else
			{
				addMessage("off, owned, on", Color.White);
			}
			break;
		case "sleepannouncemode":
			if (!Game1.IsMasterGame)
			{
				break;
			}
			if (split.Length > 1)
			{
				switch (split[1])
				{
				case "all":
					Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.All;
					break;
				case "first":
					Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.First;
					break;
				case "off":
					Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.Off;
					break;
				}
			}
			Game1.multiplayer.globalChatInfoMessage("SleepAnnounceModeSet", TokenStringBuilder.LocalizedText($"Strings\\UI:SleepAnnounceMode_{Game1.player.team.sleepAnnounceMode.Value}"));
			break;
		case "money":
			if (Program.enableCheats)
			{
				cheat(command);
			}
			else
			{
				addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApeNiceTry"), new Color(104, 214, 255));
			}
			break;
		case "recountnuts":
			Game1.game1.RecountWalnuts();
			break;
		case "fixweapons":
			SaveMigrator_1_5.ResetForges();
			addMessage("Reset forged weapon attributes.", Color.White);
			break;
		case "e":
		case "emote":
		{
			if (!Game1.player.CanEmote())
			{
				break;
			}
			bool valid_emote = false;
			if (split.Length > 1)
			{
				string emote_type = split[1];
				emote_type = emote_type.Substring(0, Math.Min(emote_type.Length, 16));
				emote_type.Trim();
				emote_type.ToLower();
				for (int i = 0; i < Farmer.EMOTES.Length; i++)
				{
					if (emote_type == Farmer.EMOTES[i].emoteString)
					{
						valid_emote = true;
						break;
					}
				}
				if (valid_emote)
				{
					Game1.player.netDoEmote(emote_type);
				}
			}
			if (valid_emote)
			{
				break;
			}
			string emote_list = "";
			for (int i = 0; i < Farmer.EMOTES.Length; i++)
			{
				if (!Farmer.EMOTES[i].hidden)
				{
					emote_list += Farmer.EMOTES[i].emoteString;
					if (i < Farmer.EMOTES.Length - 1)
					{
						emote_list += ", ";
					}
				}
			}
			addMessage(emote_list, Color.White);
			break;
		}
		default:
			if (Program.enableCheats || Game1.isRunningMacro)
			{
				cheat(command);
			}
			break;
		}
	}

	public virtual void cheat(string command, bool isDebug = false)
	{
		string fullCommand = (isDebug ? "debug " : "") + command;
		Game1.debugOutput = null;
		addInfoMessage("/" + fullCommand);
		if (!Game1.isRunningMacro)
		{
			cheatHistory.Insert(0, "/" + fullCommand);
		}
		if (Game1.game1.parseDebugInput(command, CheatCommandChatLogger))
		{
			if (!string.IsNullOrEmpty(Game1.debugOutput))
			{
				addInfoMessage(Game1.debugOutput);
			}
		}
		else if (!string.IsNullOrEmpty(Game1.debugOutput))
		{
			addErrorMessage(Game1.debugOutput);
		}
		else
		{
			addErrorMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ChatBox.cs.10261") + " " + ArgUtility.SplitBySpaceAndGet(command, 0));
		}
	}

	private void replyPrivateMessage(string command)
	{
		if (!Game1.IsMultiplayer)
		{
			return;
		}
		if (lastReceivedPrivateMessagePlayerId == 0L)
		{
			addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerToReplyTo"));
			return;
		}
		if (!Game1.otherFarmers.TryGetValue(lastReceivedPrivateMessagePlayerId, out var lastPlayer) || !lastPlayer.isActive())
		{
			addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_CouldNotReply"));
			return;
		}
		string[] split = ArgUtility.SplitBySpace(command);
		if (split.Length <= 1)
		{
			return;
		}
		string message = "";
		for (int i = 1; i < split.Length; i++)
		{
			message += split[i];
			if (i < split.Length - 1)
			{
				message += " ";
			}
		}
		message = Program.sdk.FilterDirtyWords(message);
		Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, message, lastReceivedPrivateMessagePlayerId);
		receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, message);
	}

	private void kickPlayer(string command)
	{
		int index = 0;
		Farmer farmer = findMatchingFarmer(command, ref index, allowMatchingByUserName: true);
		if (farmer != null)
		{
			Game1.server.kick(farmer.UniqueMultiplayerID);
			return;
		}
		addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerWithThatName"));
		listPlayers(otherPlayersOnly: true);
	}

	private void banPlayer(string command)
	{
		int index = 0;
		Farmer farmer = findMatchingFarmer(command, ref index, allowMatchingByUserName: true);
		if (farmer != null)
		{
			string userId = Game1.server.ban(farmer.UniqueMultiplayerID);
			if (userId == null || !Game1.bannedUsers.TryGetValue(userId, out var userName))
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayerFailed"));
				return;
			}
			string userDisplay = ((userName != null) ? (userName + " (" + userId + ")") : userId);
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayer", userDisplay));
		}
		else
		{
			addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerWithThatName"));
			listPlayers(otherPlayersOnly: true);
		}
	}

	private void unbanAll()
	{
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UnbannedAllPlayers"));
		Game1.bannedUsers.Clear();
	}

	private void unbanPlayer(string command)
	{
		if (Game1.bannedUsers.Count == 0)
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayersList_None"));
			return;
		}
		bool listUnbannablePlayers = false;
		string[] split = ArgUtility.SplitBySpace(command);
		if (split.Length > 1)
		{
			string unbanId = split[1];
			string userId = null;
			if (Game1.bannedUsers.TryGetValue(unbanId, out var userName))
			{
				userId = unbanId;
			}
			else
			{
				foreach (KeyValuePair<string, string> bannedUser in Game1.bannedUsers)
				{
					if (bannedUser.Value == unbanId)
					{
						userId = bannedUser.Key;
						userName = bannedUser.Value;
						break;
					}
				}
			}
			if (userId != null)
			{
				string userDisplay = ((userName != null) ? (userName + " (" + userId + ")") : userId);
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UnbannedPlayer", userDisplay));
				Game1.bannedUsers.Remove(userId);
			}
			else
			{
				listUnbannablePlayers = true;
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UnbanPlayer_NotFound"));
			}
		}
		else
		{
			listUnbannablePlayers = true;
		}
		if (!listUnbannablePlayers)
		{
			return;
		}
		addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayersList"));
		foreach (KeyValuePair<string, string> bannedUser in Game1.bannedUsers)
		{
			string userDisplay = "- " + bannedUser.Key;
			if (bannedUser.Value != null)
			{
				userDisplay = "- " + bannedUser.Value + " (" + bannedUser.Key + ")";
			}
			addInfoMessage(userDisplay);
		}
	}

	private Farmer findMatchingFarmer(string command, ref int matchingIndex, bool allowMatchingByUserName = false)
	{
		string[] split = ArgUtility.SplitBySpace(command);
		Farmer matchingFarmer = null;
		foreach (Farmer farmer in Game1.otherFarmers.Values)
		{
			string[] farmerNameSplit = ArgUtility.SplitBySpace(farmer.displayName);
			bool isMatch = true;
			int i;
			for (i = 0; i < farmerNameSplit.Length; i++)
			{
				if (split.Length > i + 1)
				{
					if (split[i + 1].ToLowerInvariant() != farmerNameSplit[i].ToLowerInvariant())
					{
						isMatch = false;
						break;
					}
					continue;
				}
				isMatch = false;
				break;
			}
			if (isMatch)
			{
				matchingFarmer = farmer;
				matchingIndex = i;
				break;
			}
			if (!allowMatchingByUserName)
			{
				continue;
			}
			isMatch = true;
			string[] userNameSplit = ArgUtility.SplitBySpace(Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID));
			for (i = 0; i < userNameSplit.Length; i++)
			{
				if (split.Length > i + 1)
				{
					if (split[i + 1].ToLowerInvariant() != userNameSplit[i].ToLowerInvariant())
					{
						isMatch = false;
						break;
					}
					continue;
				}
				isMatch = false;
				break;
			}
			if (isMatch)
			{
				matchingFarmer = farmer;
				matchingIndex = i;
				break;
			}
		}
		return matchingFarmer;
	}

	private void sendPrivateMessage(string command)
	{
		if (!Game1.IsMultiplayer)
		{
			return;
		}
		string[] split = ArgUtility.SplitBySpace(command);
		int matchingIndex = 0;
		Farmer matchingFarmer = findMatchingFarmer(command, ref matchingIndex);
		if (matchingFarmer == null)
		{
			addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerWithThatName"));
			return;
		}
		string message = "";
		for (int i = matchingIndex + 1; i < split.Length; i++)
		{
			message += split[i];
			if (i < split.Length - 1)
			{
				message += " ";
			}
		}
		message = Program.sdk.FilterDirtyWords(message);
		Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, message, matchingFarmer.UniqueMultiplayerID);
		receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, message);
	}

	public bool isActive()
	{
		return chatBox.Selected;
	}

	public void activate()
	{
		chatBox.Selected = true;
		setText("");
	}

	public override void clickAway()
	{
		base.clickAway();
		if (!choosingEmoji || !emojiMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) || Game1.input.GetKeyboardState().IsKeyDown(Keys.Escape))
		{
			bool selected = chatBox.Selected;
			chatBox.Selected = false;
			choosingEmoji = false;
			setText("");
			cheatHistoryPosition = -1;
			if (selected)
			{
				Game1.oldKBState = Game1.GetKeyboardState();
			}
		}
	}

	public override bool isWithinBounds(int x, int y)
	{
		if (x - xPositionOnScreen >= width || x - xPositionOnScreen < 0 || y - yPositionOnScreen >= height || y - yPositionOnScreen < -getOldMessagesBoxHeight())
		{
			if (choosingEmoji)
			{
				return emojiMenu.isWithinBounds(x, y);
			}
			return false;
		}
		return true;
	}

	public virtual void setText(string text)
	{
		chatBox.setText(text);
	}

	public override void receiveKeyPress(Keys key)
	{
		switch (key)
		{
		case Keys.Up:
			if (cheatHistoryPosition < cheatHistory.Count - 1)
			{
				cheatHistoryPosition++;
				string cheat = cheatHistory[cheatHistoryPosition];
				chatBox.setText(cheat);
			}
			break;
		case Keys.Down:
			if (cheatHistoryPosition > 0)
			{
				cheatHistoryPosition--;
				string cheat = cheatHistory[cheatHistoryPosition];
				chatBox.setText(cheat);
			}
			break;
		}
		if (!Game1.options.doesInputListContain(Game1.options.moveUpButton, key) && !Game1.options.doesInputListContain(Game1.options.moveRightButton, key) && !Game1.options.doesInputListContain(Game1.options.moveDownButton, key) && !Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
		{
			base.receiveKeyPress(key);
		}
	}

	public override bool readyToClose()
	{
		return false;
	}

	public override void receiveGamePadButton(Buttons b)
	{
	}

	public bool isHoveringOverClickable(int x, int y)
	{
		if (emojiMenuIcon.containsPoint(x, y) || (choosingEmoji && emojiMenu.isWithinBounds(x, y)))
		{
			return true;
		}
		return false;
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!chatBox.Selected)
		{
			return;
		}
		if (emojiMenuIcon.containsPoint(x, y))
		{
			choosingEmoji = !choosingEmoji;
			Game1.playSound("shwip");
			emojiMenuIcon.scale = 4f;
			return;
		}
		if (choosingEmoji && emojiMenu.isWithinBounds(x, y))
		{
			emojiMenu.leftClick(x, y, this);
			return;
		}
		chatBox.Update();
		if (choosingEmoji)
		{
			choosingEmoji = false;
			emojiMenuIcon.scale = 4f;
		}
		if (isWithinBounds(x, y))
		{
			chatBox.Selected = true;
		}
	}

	public static string formattedUserName(Farmer farmer)
	{
		string name = farmer.Name;
		if (name == null || name.Trim() == "")
		{
			name = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
		}
		return Program.sdk.FilterDirtyWords(name);
	}

	public static string formattedUserNameLong(Farmer farmer)
	{
		string name = formattedUserName(farmer);
		return Game1.content.LoadString("Strings\\UI:Chat_PlayerName", name, Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID));
	}

	private string formatMessage(long sourceFarmer, int chatKind, string message)
	{
		string userName = Game1.content.LoadString("Strings\\UI:Chat_UnknownUserName");
		Farmer farmer;
		if (sourceFarmer == Game1.player.UniqueMultiplayerID)
		{
			farmer = Game1.player;
		}
		else if (!Game1.otherFarmers.TryGetValue(sourceFarmer, out farmer))
		{
			farmer = null;
		}
		if (farmer != null)
		{
			userName = formattedUserName(farmer);
		}
		return chatKind switch
		{
			0 => Game1.content.LoadString("Strings\\UI:Chat_ChatMessageFormat", userName, message), 
			2 => Game1.content.LoadString("Strings\\UI:Chat_UserNotificationMessageFormat", message), 
			3 => Game1.content.LoadString("Strings\\UI:Chat_PrivateMessageFormat", userName, message), 
			_ => Game1.content.LoadString("Strings\\UI:Chat_ErrorMessageFormat", message), 
		};
	}

	protected virtual Color messageColor(int chatKind)
	{
		return chatKind switch
		{
			0 => chatBox.TextColor, 
			3 => Color.DarkCyan, 
			2 => Color.Yellow, 
			_ => Color.Red, 
		};
	}

	public virtual void receiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
	{
		string text = formatMessage(sourceFarmer, chatKind, message);
		ChatMessage c = new ChatMessage();
		string s = Game1.parseText(text, chatBox.Font, chatBox.Width - 16);
		c.timeLeftToDisplay = 600;
		c.verticalSize = (int)chatBox.Font.MeasureString(s).Y + 4;
		c.color = messageColor(chatKind);
		c.language = language;
		c.parseMessageForEmoji(s);
		messages.Add(c);
		if (messages.Count > maxMessages)
		{
			messages.RemoveAt(0);
		}
		if (chatKind == 3 && sourceFarmer != Game1.player.UniqueMultiplayerID)
		{
			lastReceivedPrivateMessagePlayerId = sourceFarmer;
		}
	}

	public virtual void addMessage(string message, Color color)
	{
		ChatMessage c = new ChatMessage();
		string s = Game1.parseText(message, chatBox.Font, chatBox.Width - 8);
		c.timeLeftToDisplay = 600;
		c.verticalSize = (int)chatBox.Font.MeasureString(s).Y + 4;
		c.color = color;
		c.language = LocalizedContentManager.CurrentLanguageCode;
		c.parseMessageForEmoji(s);
		messages.Add(c);
		if (messages.Count > maxMessages)
		{
			messages.RemoveAt(0);
		}
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public override void performHoverAction(int x, int y)
	{
		emojiMenuIcon.tryHover(x, y, 1f);
		emojiMenuIcon.tryHover(x, y, 1f);
	}

	public override void update(GameTime time)
	{
		KeyboardState keyState = Game1.input.GetKeyboardState();
		Keys[] pressedKeys = keyState.GetPressedKeys();
		foreach (Keys key in pressedKeys)
		{
			if (!oldKBState.IsKeyDown(key))
			{
				receiveKeyPress(key);
			}
		}
		oldKBState = keyState;
		for (int i = 0; i < messages.Count; i++)
		{
			if (messages[i].timeLeftToDisplay > 0)
			{
				messages[i].timeLeftToDisplay--;
			}
			if (messages[i].timeLeftToDisplay < 75)
			{
				messages[i].alpha = (float)messages[i].timeLeftToDisplay / 75f;
			}
		}
		if (chatBox.Selected)
		{
			foreach (ChatMessage message in messages)
			{
				message.alpha = 1f;
			}
		}
		emojiMenuIcon.tryHover(0, 0, 1f);
	}

	public override void receiveScrollWheelAction(int direction)
	{
		if (choosingEmoji)
		{
			emojiMenu.receiveScrollWheelAction(direction);
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		updatePosition();
	}

	public static SpriteFont messageFont(LocalizedContentManager.LanguageCode language)
	{
		return Game1.content.Load<SpriteFont>("Fonts\\SmallFont", language);
	}

	public int getOldMessagesBoxHeight()
	{
		int heightSoFar = 20;
		for (int i = messages.Count - 1; i >= 0; i--)
		{
			ChatMessage message = messages[i];
			if (chatBox.Selected || message.alpha > 0.01f)
			{
				heightSoFar += message.verticalSize;
			}
		}
		return heightSoFar;
	}

	public override void draw(SpriteBatch b)
	{
		int heightSoFar = 0;
		bool drawBG = false;
		for (int i = messages.Count - 1; i >= 0; i--)
		{
			ChatMessage message = messages[i];
			if (chatBox.Selected || message.alpha > 0.01f)
			{
				heightSoFar += message.verticalSize;
				drawBG = true;
			}
		}
		if (drawBG)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), xPositionOnScreen, yPositionOnScreen - heightSoFar - 20 + ((!chatBox.Selected) ? chatBox.Height : 0), chatBox.Width, heightSoFar + 20, Color.White, 4f, drawShadow: false);
		}
		heightSoFar = 0;
		for (int i = messages.Count - 1; i >= 0; i--)
		{
			ChatMessage message = messages[i];
			heightSoFar += message.verticalSize;
			message.draw(b, xPositionOnScreen + 12, yPositionOnScreen - heightSoFar - 8 + ((!chatBox.Selected) ? chatBox.Height : 0));
		}
		if (chatBox.Selected)
		{
			chatBox.Draw(b, drawShadow: false);
			emojiMenuIcon.draw(b, Color.White, 0.99f);
			if (choosingEmoji)
			{
				emojiMenu.draw(b);
			}
			if (isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) && !Game1.options.hardwareCursor)
			{
				Game1.mouseCursor = (Game1.options.gamepadControls ? Game1.cursor_gamepad_pointer : Game1.cursor_default);
			}
		}
	}
}
