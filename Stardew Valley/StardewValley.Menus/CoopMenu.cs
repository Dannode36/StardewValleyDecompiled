using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Network;
using StardewValley.SDKs;

namespace StardewValley.Menus;

public class CoopMenu : LoadGameMenu
{
	public enum Tab
	{
		JOIN_TAB,
		HOST_TAB
	}

	protected abstract class CoopMenuSlot : MenuSlot
	{
		protected new CoopMenu menu;

		public CoopMenuSlot(CoopMenu menu)
			: base(menu)
		{
			this.menu = menu;
		}
	}

	protected abstract class LabeledSlot : CoopMenuSlot
	{
		private string message;

		public LabeledSlot(CoopMenu menu, string message)
			: base(menu)
		{
			this.message = message;
		}

		public abstract override void Activate();

		public override void Draw(SpriteBatch b, int i)
		{
			int strWidth = SpriteText.getWidthOfString(message);
			int strHeight = SpriteText.getHeightOfString(message);
			Rectangle bounds = menu.slotButtons[i].bounds;
			int x = bounds.X + (bounds.Width - strWidth) / 2;
			int y = bounds.Y + (bounds.Height - strHeight) / 2;
			SpriteText.drawString(b, message, x, y);
		}
	}

	protected class LanSlot : LabeledSlot
	{
		public LanSlot(CoopMenu menu)
			: base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_JoinLANGame"))
		{
		}

		public override void Activate()
		{
			menu.enterIPPressed();
		}
	}

	protected class InviteCodeSlot : LabeledSlot
	{
		public InviteCodeSlot(CoopMenu menu)
			: base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_EnterInviteCode"))
		{
		}

		public override void Activate()
		{
			menu.enterInviteCodePressed();
		}
	}

	protected class HostNewFarmSlot : LabeledSlot
	{
		public HostNewFarmSlot(CoopMenu menu)
			: base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_HostNewFarm"))
		{
			ActivateDelay = 2150;
		}

		public override void Activate()
		{
			Game1.resetPlayer();
			TitleMenu.subMenu = new CharacterCustomization(CharacterCustomization.Source.HostNewFarm);
			Game1.changeMusicTrack("CloudCountry");
		}
	}

	protected class TooManyFarmsSlot : LabeledSlot
	{
		public TooManyFarmsSlot(CoopMenu menu)
			: base(menu, Game1.content.LoadString("Strings\\UI:TooManyFarmsMenu_TooManyFarms"))
		{
		}

		public override void Activate()
		{
		}
	}

	protected class HostFileSlot : SaveFileSlot
	{
		protected new CoopMenu menu;

		public HostFileSlot(CoopMenu menu, Farmer farmer)
			: base(menu, farmer, null)
		{
			this.menu = menu;
		}

		public override void Activate()
		{
			Game1.multiplayerMode = 2;
			base.Activate();
		}

		protected override void drawSlotSaveNumber(SpriteBatch b, int i)
		{
		}

		protected override string slotName()
		{
			return Game1.content.LoadString("Strings\\UI:CoopMenu_HostFile", Farmer.Name, Farmer.farmName.Value);
		}

		protected override string slotSubName()
		{
			return Farmer.Name;
		}

		protected override Vector2 portraitOffset()
		{
			return base.portraitOffset() - new Vector2(32f, 0f);
		}
	}

	protected class FriendFarmData
	{
		public object Lobby;

		public string OwnerName;

		public string FarmName;

		public int FarmType;

		public ModFarmType ModFarmType;

		public WorldDate Date;

		public bool PreviouslyJoined;

		public string ProtocolVersion;
	}

	protected class FriendFarmSlot : CoopMenuSlot
	{
		public FriendFarmData Farm;

		public FriendFarmSlot(CoopMenu menu, FriendFarmData farm)
			: base(menu)
		{
			Farm = farm;
		}

		public bool MatchAddress(object Lobby)
		{
			return object.Equals(Farm.Lobby, Lobby);
		}

		public void Update(FriendFarmData newData)
		{
			Farm = newData;
		}

		public override void Activate()
		{
			menu.setMenu(new FarmhandMenu(Program.sdk.Networking.CreateClient(Farm.Lobby)));
		}

		protected virtual string slotName()
		{
			string messageKey = (Farm.PreviouslyJoined ? "Strings\\UI:CoopMenu_RevisitFriendFarm" : "Strings\\UI:CoopMenu_JoinFriendFarm");
			return Game1.content.LoadString(messageKey, Farm.FarmName);
		}

		protected virtual void drawSlotName(SpriteBatch b, int i)
		{
			SpriteText.drawString(b, slotName(), menu.slotButtons[i].bounds.X + 128 + 36, menu.slotButtons[i].bounds.Y + 36);
		}

		protected virtual void drawSlotDate(SpriteBatch b, int i)
		{
			Utility.drawTextWithShadow(b, Farm.Date.Localize(), Game1.dialogueFont, new Vector2(menu.slotButtons[i].bounds.X + 128 + 32, menu.slotButtons[i].bounds.Y + 64 + 40), Game1.textColor);
		}

		protected virtual void drawSlotFarm(SpriteBatch b, int i)
		{
			int drawn_farm_type = Farm.FarmType;
			if (drawn_farm_type == 7)
			{
				drawn_farm_type = 0;
			}
			Rectangle sourceRect = new Rectangle(22 * (drawn_farm_type % 5), 324 + 21 * (drawn_farm_type / 5), 22, 20);
			Texture2D texture = Game1.mouseCursors;
			Rectangle space = new Rectangle(menu.slotButtons[i].bounds.X, menu.slotButtons[i].bounds.Y, 160, menu.slotButtons[i].bounds.Height);
			Rectangle destRect = new Rectangle(space.X + (space.Width - sourceRect.Width * 4) / 2, space.Y + (space.Height - sourceRect.Height * 4) / 2, sourceRect.Width * 4, sourceRect.Height * 4);
			if (Farm.ModFarmType?.IconTexture != null)
			{
				texture = Game1.content.Load<Texture2D>(Farm.ModFarmType.IconTexture);
				b.Draw(texture, destRect, null, Color.White);
			}
			else
			{
				b.Draw(texture, destRect, sourceRect, Color.White);
			}
		}

		protected virtual void drawSlotOwnerName(SpriteBatch b, int i)
		{
			float scale = 1f;
			float x_pos_offset = 128f;
			float y_pos_offset = 44f;
			Utility.drawTextWithShadow(b, Farm.OwnerName, Game1.dialogueFont, new Vector2((float)(menu.slotButtons[i].bounds.X + menu.width) - x_pos_offset - Game1.dialogueFont.MeasureString(Farm.OwnerName).X * scale, (float)menu.slotButtons[i].bounds.Y + y_pos_offset), Game1.textColor, scale);
		}

		public override void Draw(SpriteBatch b, int i)
		{
			drawSlotName(b, i);
			drawSlotDate(b, i);
			drawSlotFarm(b, i);
			drawSlotOwnerName(b, i);
		}
	}

	public class LobbyUpdateCallback : LobbyUpdateListener
	{
		private Action<object> callback;

		public LobbyUpdateCallback(Action<object> callback)
		{
			this.callback = callback;
		}

		public void OnLobbyUpdate(object lobby)
		{
			callback?.Invoke(lobby);
		}
	}

	public const int region_refresh = 810;

	public const int region_joinTab = 811;

	public const int region_hostTab = 812;

	public const int region_tabs = 1000;

	protected List<MenuSlot> hostSlots = new List<MenuSlot>();

	public ClickableComponent refreshButton;

	public ClickableComponent joinTab;

	public ClickableComponent hostTab;

	private LobbyUpdateListener lobbyUpdateListener;

	public Tab currentTab;

	private bool smallScreenFormat;

	private bool isSetUp;

	private int updateCounter;

	private string Filter;

	private float _refreshDelay = -1f;

	public bool tooManyFarms;

	public static string lastEnteredInviteCode;

	private StringBuilder _stringBuilder = new StringBuilder();

	public override List<MenuSlot> MenuSlots
	{
		get
		{
			return currentTab switch
			{
				Tab.JOIN_TAB => menuSlots, 
				Tab.HOST_TAB => hostSlots, 
				_ => null, 
			};
		}
		set
		{
			switch (currentTab)
			{
			case Tab.JOIN_TAB:
				menuSlots = value;
				break;
			case Tab.HOST_TAB:
				hostSlots = value;
				break;
			}
		}
	}

	public CoopMenu(bool tooManyFarms, Tab initialTab = Tab.JOIN_TAB, string filter = null)
	{
		this.tooManyFarms = tooManyFarms;
		currentTab = initialTab;
		Filter = filter;
	}

	public override bool readyToClose()
	{
		if (isSetUp)
		{
			return base.readyToClose();
		}
		return true;
	}

	protected override bool hasDeleteButtons()
	{
		return false;
	}

	/// <inheritdoc />
	protected override void startListPopulation(string filter)
	{
	}

	protected virtual void connectionFinished()
	{
		string label = Game1.content.LoadString("Strings\\UI:CoopMenu_Refresh");
		int width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
		Vector2 pos = new Vector2(backButton.bounds.Right - width, backButton.bounds.Y - 128);
		refreshButton = new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, 96), "", label)
		{
			myID = 810,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = 81114
		};
		_refreshDelay = 8f;
		smallScreenFormat = Game1.graphics.GraphicsDevice.Viewport.Height < 1080;
		label = Game1.content.LoadString("Strings\\UI:CoopMenu_Join");
		width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
		pos = (smallScreenFormat ? new Vector2(xPositionOnScreen, yPositionOnScreen) : new Vector2(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen - 96));
		joinTab = new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, smallScreenFormat ? 72 : 64), "", label)
		{
			myID = 811,
			downNeighborID = -99998,
			rightNeighborID = 812,
			region = 1000
		};
		label = Game1.content.LoadString("Strings\\UI:CoopMenu_Host");
		width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
		pos = (smallScreenFormat ? new Vector2(joinTab.bounds.Right + ((!smallScreenFormat) ? 4 : 0), yPositionOnScreen) : new Vector2(joinTab.bounds.Right + 4, yPositionOnScreen - 64));
		hostTab = new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, smallScreenFormat ? 72 : 64), "", label)
		{
			myID = 812,
			downNeighborID = -99998,
			leftNeighborID = 811,
			rightNeighborID = 800,
			region = 1000
		};
		backButton.upNeighborID = 810;
		if (tooManyFarms)
		{
			hostSlots.Add(new TooManyFarmsSlot(this));
		}
		else
		{
			hostSlots.Add(new HostNewFarmSlot(this));
		}
		menuSlots.Add(new LanSlot(this));
		if (Program.sdk.Networking != null && Program.sdk.Networking.SupportsInviteCodes())
		{
			menuSlots.Add(new InviteCodeSlot(this));
		}
		SetTab(currentTab, play_sound: false);
		isSetUp = true;
		Game1.mouseCursor = 0;
		base.startListPopulation(Filter);
		populateClickableComponentList();
	}

	public override void receiveGamePadButton(Buttons b)
	{
		base.receiveGamePadButton(b);
		if (IsDoingTask())
		{
			return;
		}
		switch (b)
		{
		case Buttons.LeftTrigger:
		{
			ClickableComponent clickableComponent2 = joinTab;
			if (clickableComponent2 != null && clickableComponent2.visible)
			{
				SetTab(Tab.JOIN_TAB);
				setCurrentlySnappedComponentTo(joinTab.myID);
				snapCursorToCurrentSnappedComponent();
			}
			break;
		}
		case Buttons.RightTrigger:
		{
			ClickableComponent clickableComponent = hostTab;
			if (clickableComponent != null && clickableComponent.visible)
			{
				SetTab(Tab.HOST_TAB);
				setCurrentlySnappedComponentTo(hostTab.myID);
				snapCursorToCurrentSnappedComponent();
			}
			break;
		}
		}
	}

	public override void UpdateButtons()
	{
		base.UpdateButtons();
		foreach (ClickableComponent c in slotButtons)
		{
			if (c.myID == 0)
			{
				if (currentItemIndex == 0)
				{
					c.upNeighborID = 811;
				}
				else
				{
					c.upNeighborID = -7777;
				}
			}
		}
	}

	public override void update(GameTime time)
	{
		float elapsed = (float)time.ElapsedGameTime.TotalSeconds;
		updateCounter++;
		if (!isSetUp)
		{
			if (Program.sdk.ConnectionFinished)
			{
				connectionFinished();
			}
			else
			{
				Game1.mouseCursor = 1;
			}
			return;
		}
		if (refreshButton != null && refreshButton.visible && _refreshDelay > 0f)
		{
			_refreshDelay -= elapsed;
		}
		base.update(time);
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		if (joinTab != null && hostTab != null && backButton != null && refreshButton != null)
		{
			smallScreenFormat = Game1.graphics.GraphicsDevice.Viewport.Height < 1080;
			string label = Game1.content.LoadString("Strings\\UI:CoopMenu_Join");
			Vector2 pos = (smallScreenFormat ? new Vector2(xPositionOnScreen, yPositionOnScreen) : new Vector2(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen - 96));
			joinTab.bounds.X = (int)pos.X;
			joinTab.bounds.Y = (int)pos.Y;
			label = Game1.content.LoadString("Strings\\UI:CoopMenu_Host");
			pos = (smallScreenFormat ? new Vector2(joinTab.bounds.Right + ((!smallScreenFormat) ? 4 : 0), yPositionOnScreen) : new Vector2(joinTab.bounds.Right + 4, yPositionOnScreen - 64));
			hostTab.bounds.X = (int)pos.X;
			hostTab.bounds.Y = (int)pos.Y;
			label = Game1.content.LoadString("Strings\\UI:CoopMenu_Refresh");
			int width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
			pos = new Vector2(backButton.bounds.Right - width, backButton.bounds.Y - 128);
			refreshButton.bounds.X = (int)pos.X;
			refreshButton.bounds.Y = (int)pos.Y;
		}
	}

	protected override void saveFileScanComplete()
	{
		if (Program.sdk.Networking != null)
		{
			lobbyUpdateListener = new LobbyUpdateCallback(onLobbyUpdate);
			Program.sdk.Networking.AddLobbyUpdateListener(lobbyUpdateListener);
			Program.sdk.Networking.RequestFriendLobbyData();
		}
	}

	protected virtual FriendFarmData readLobbyFarmData(object lobby)
	{
		FriendFarmData farm = new FriendFarmData
		{
			Lobby = lobby,
			Date = new WorldDate()
		};
		farm.OwnerName = Program.sdk.Networking.GetLobbyOwnerName(lobby);
		farm.FarmName = Program.sdk.Networking.GetLobbyData(lobby, "farmName");
		string farmType = Program.sdk.Networking.GetLobbyData(lobby, "farmType");
		string mod_farm_type = Program.sdk.Networking.GetLobbyData(lobby, "modFarmType");
		string lobbyData = Program.sdk.Networking.GetLobbyData(lobby, "date");
		int farmType_i = Convert.ToInt32(farmType);
		int farmDate_day = Convert.ToInt32(lobbyData);
		farm.FarmType = farmType_i;
		farm.ModFarmType = null;
		if (!string.IsNullOrEmpty(mod_farm_type))
		{
			List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
			if (farm_types != null)
			{
				foreach (ModFarmType farm_type in farm_types)
				{
					if (farm_type.Id == mod_farm_type)
					{
						farm.ModFarmType = farm_type;
						break;
					}
				}
			}
		}
		farm.Date.TotalDays = farmDate_day;
		farm.ProtocolVersion = Program.sdk.Networking.GetLobbyData(lobby, "protocolVersion");
		farm.FarmName = Program.sdk.FilterDirtyWords(farm.FarmName);
		farm.OwnerName = Program.sdk.FilterDirtyWords(farm.OwnerName);
		return farm;
	}

	protected virtual bool checkFriendFarmCompatibility(FriendFarmData farm)
	{
		if (farm.FarmType < 0 || farm.FarmType > 7)
		{
			return false;
		}
		if (farm.ProtocolVersion != Multiplayer.protocolVersion)
		{
			return false;
		}
		return true;
	}

	protected virtual void onLobbyUpdate(object lobby)
	{
		try
		{
			string protocolVersion = Program.sdk.Networking.GetLobbyData(lobby, "protocolVersion");
			if (protocolVersion != Multiplayer.protocolVersion)
			{
				return;
			}
			Game1.log.Verbose("Receiving friend lobby data...\nOwner: " + Program.sdk.Networking.GetLobbyOwnerName(lobby) + "\nfarmName = " + Program.sdk.Networking.GetLobbyData(lobby, "farmName") + "\nfarmType = " + Program.sdk.Networking.GetLobbyData(lobby, "farmType") + "\ndate = " + Program.sdk.Networking.GetLobbyData(lobby, "date") + "\nprotocolVersion = " + protocolVersion + "\nfarmhands = " + Program.sdk.Networking.GetLobbyData(lobby, "farmhands") + "\nnewFarmhands = " + Program.sdk.Networking.GetLobbyData(lobby, "newFarmhands"));
			FriendFarmData farm = readLobbyFarmData(lobby);
			if (!checkFriendFarmCompatibility(farm) || (farm.FarmType == 7 && farm.ModFarmType == null))
			{
				return;
			}
			string selfID = Program.sdk.Networking.GetUserID();
			string farmhands = Program.sdk.Networking.GetLobbyData(lobby, "farmhands");
			bool newFarmhands = Convert.ToBoolean(Program.sdk.Networking.GetLobbyData(lobby, "newFarmhands"));
			if (farmhands == "" && !newFarmhands)
			{
				return;
			}
			string[] farmUsers = farmhands.Split(',');
			if (!farmUsers.Contains(selfID) && !newFarmhands)
			{
				return;
			}
			farm.PreviouslyJoined = farmUsers.Contains(selfID);
			if (menuSlots == null)
			{
				return;
			}
			foreach (MenuSlot menuSlot in menuSlots)
			{
				if (menuSlot is FriendFarmSlot farmSlot && farmSlot.MatchAddress(lobby))
				{
					farmSlot.Update(farm);
					return;
				}
			}
			menuSlots.Add(new FriendFarmSlot(this, farm));
			UpdateButtons();
			populateClickableComponentList();
		}
		catch (FormatException)
		{
		}
		catch (OverflowException)
		{
		}
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (a.region == 1000 && (direction == 2 || direction == 0) && b.region == 1000)
		{
			return false;
		}
		if (a.myID == 810 && direction == 0 && b.region != 900)
		{
			return false;
		}
		if (a.myID == 810 && direction == 1 && b.myID == 81114)
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	protected override void addSaveFiles(List<Farmer> files)
	{
		hostSlots.AddRange(files.Where((Farmer file) => file.slotCanHost).Select((Func<Farmer, MenuSlot>)((Farmer file) => new HostFileSlot(this, file))));
		UpdateButtons();
	}

	protected virtual void setMenu(IClickableMenu menu)
	{
		if (Game1.activeClickableMenu is TitleMenu)
		{
			TitleMenu.subMenu = menu;
		}
		else
		{
			Game1.activeClickableMenu = menu;
		}
	}

	private void enterIPPressed()
	{
		string last_entered_ip = "";
		try
		{
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false, applyLanguage: false);
			last_entered_ip = startupPreferences.lastEnteredIP;
		}
		catch (Exception)
		{
		}
		string title = Game1.content.LoadString("Strings\\UI:CoopMenu_EnterIP");
		setMenu(new TitleTextInputMenu(title, delegate(string address)
		{
			try
			{
				StartupPreferences startupPreferences2 = new StartupPreferences();
				startupPreferences2.loadPreferences(async: false, applyLanguage: false);
				startupPreferences2.lastEnteredIP = address;
				startupPreferences2.savePreferences(async: false);
			}
			catch (Exception)
			{
			}
			if (address == "")
			{
				address = "localhost";
			}
			setMenu(new FarmhandMenu(Game1.multiplayer.InitClient(new LidgrenClient(address))));
		}, last_entered_ip, "join_menu"));
	}

	private void enterInviteCodePressed()
	{
		if (Program.sdk.Networking == null || !Program.sdk.Networking.SupportsInviteCodes())
		{
			return;
		}
		string title = Game1.content.LoadString("Strings\\UI:CoopMenu_EnterInviteCode");
		setMenu(new TitleTextInputMenu(title, delegate(string code)
		{
			lastEnteredInviteCode = code;
			object lobbyFromInviteCode = Program.sdk.Networking.GetLobbyFromInviteCode(code);
			if (lobbyFromInviteCode != null)
			{
				Client client = Program.sdk.Networking.CreateClient(lobbyFromInviteCode);
				setMenu(new FarmhandMenu(client));
			}
		}, lastEnteredInviteCode, "join_menu"));
	}

	private bool tabClick(int x, int y)
	{
		if (joinTab.visible && joinTab.containsPoint(x, y))
		{
			SetTab(Tab.JOIN_TAB);
			return true;
		}
		if (hostTab.visible && hostTab.containsPoint(x, y))
		{
			SetTab(Tab.HOST_TAB);
			return true;
		}
		return false;
	}

	public virtual void SetTab(Tab new_tab, bool play_sound = true)
	{
		if (currentTab == new_tab)
		{
			return;
		}
		currentTab = new_tab;
		if (!smallScreenFormat && isSetUp)
		{
			if (currentTab == Tab.HOST_TAB)
			{
				hostTab.bounds.Y = yPositionOnScreen - 96;
				joinTab.bounds.Y = yPositionOnScreen - 64;
			}
			else
			{
				hostTab.bounds.Y = yPositionOnScreen - 64;
				joinTab.bounds.Y = yPositionOnScreen - 96;
			}
		}
		if (play_sound)
		{
			Game1.playSound("smallSelect");
		}
		if (isSetUp)
		{
			UpdateButtons();
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!isSetUp)
		{
			return;
		}
		if (refreshButton.visible && refreshButton.containsPoint(x, y))
		{
			if (_refreshDelay < 0f)
			{
				Game1.playSound("bigDeSelect");
				setMenu(new CoopMenu(tooManyFarms));
			}
		}
		else if (!smallScreenFormat || !tabClick(x, y))
		{
			base.receiveLeftClick(x, y, playSound);
			if (!smallScreenFormat && !loading)
			{
				tabClick(x, y);
			}
		}
	}

	public override void performHoverAction(int x, int y)
	{
		if (isSetUp)
		{
			if (refreshButton.visible && refreshButton.containsPoint(x, y))
			{
				refreshButton.scale = 1f;
			}
			else
			{
				refreshButton.scale = 0f;
			}
			if (smallScreenFormat && (hostTab.containsPoint(x, y) || joinTab.containsPoint(x, y)))
			{
				base.performHoverAction(-100, -100);
			}
			else
			{
				base.performHoverAction(x, y);
			}
		}
	}

	protected override string getStatusText()
	{
		return null;
	}

	private void drawTabs(SpriteBatch b)
	{
		if (isSetUp)
		{
			Color selectColor = (smallScreenFormat ? Color.Orange : new Color(255, 255, 150));
			Color hoverColor = Color.Yellow;
			Color selectShadow = (smallScreenFormat ? Color.DarkOrange : Game1.textShadowDarkerColor);
			Color hoverShadow = Color.DarkGoldenrod;
			if (joinTab.visible)
			{
				bool colorSelect = currentTab == Tab.JOIN_TAB;
				bool colorHover = currentTab != 0 && joinTab.containsPoint(Game1.getMouseX(), Game1.getMouseY());
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), joinTab.bounds.X, joinTab.bounds.Y, joinTab.bounds.Width, joinTab.bounds.Height + ((!smallScreenFormat) ? 64 : 0), colorSelect ? selectColor : (colorHover ? hoverColor : Color.White), 1f, drawShadow: false);
				Utility.drawTextWithColoredShadow(b, joinTab.label, Game1.dialogueFont, new Vector2(joinTab.bounds.Center.X, joinTab.bounds.Y + 40) - Game1.dialogueFont.MeasureString(joinTab.label) / 2f, Game1.textColor, colorHover ? hoverShadow : (colorSelect ? selectShadow : Game1.textShadowDarkerColor), 1.01f);
			}
			if (hostTab.visible)
			{
				bool colorSelect = currentTab == Tab.HOST_TAB;
				bool colorHover = currentTab != Tab.HOST_TAB && hostTab.containsPoint(Game1.getMouseX(), Game1.getMouseY());
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), hostTab.bounds.X, hostTab.bounds.Y, hostTab.bounds.Width, hostTab.bounds.Height + ((!smallScreenFormat) ? 64 : 0), colorSelect ? selectColor : (colorHover ? hoverColor : Color.White), 1f, drawShadow: false);
				Utility.drawTextWithColoredShadow(b, hostTab.label, Game1.dialogueFont, new Vector2(hostTab.bounds.Center.X, hostTab.bounds.Y + 40) - Game1.dialogueFont.MeasureString(hostTab.label) / 2f, Game1.textColor, colorHover ? hoverShadow : (colorSelect ? selectShadow : Game1.textShadowDarkerColor), 1.01f);
			}
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.snapToDefaultClickableComponent();
		if (currentlySnappedComponent == null)
		{
			currentlySnappedComponent = getComponentWithID(811);
			snapCursorToCurrentSnappedComponent();
		}
	}

	protected override void drawBefore(SpriteBatch b)
	{
		base.drawBefore(b);
		if (isSetUp && !smallScreenFormat)
		{
			drawTabs(b);
		}
	}

	protected override void drawExtra(SpriteBatch b)
	{
		base.drawExtra(b);
		if (!isSetUp)
		{
			return;
		}
		if (refreshButton.visible)
		{
			Color color = ((refreshButton.scale > 0f) ? Color.Wheat : Color.White);
			if (_refreshDelay > 0f)
			{
				color = Color.Gray;
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), refreshButton.bounds.X, refreshButton.bounds.Y, refreshButton.bounds.Width, refreshButton.bounds.Height, color, 4f);
			Utility.drawTextWithShadow(b, refreshButton.label, Game1.dialogueFont, new Vector2(refreshButton.bounds.Center.X, refreshButton.bounds.Center.Y + 4) - Game1.dialogueFont.MeasureString(refreshButton.label) / 2f, Game1.textColor, 1f, -1f, -1, -1, 0f);
		}
		if (smallScreenFormat)
		{
			drawTabs(b);
		}
	}

	protected override void drawStatusText(SpriteBatch b)
	{
		if (getStatusText() != null)
		{
			base.drawStatusText(b);
		}
		else if (!isSetUp)
		{
			int maxEllipsis = 1 + Program.sdk.ConnectionProgress;
			int ellipsisCount = updateCounter / 5 % maxEllipsis;
			string basicText = Game1.content.LoadString("Strings\\UI:CoopMenu_ConnectingOnlineServices");
			_stringBuilder.Clear();
			_stringBuilder.Append(basicText);
			for (int i = 0; i < ellipsisCount; i++)
			{
				_stringBuilder.Append(".");
			}
			string currentText = _stringBuilder.ToString();
			for (int i = ellipsisCount; i < maxEllipsis; i++)
			{
				_stringBuilder.Append(".");
			}
			int maxWidth = SpriteText.getWidthOfString(_stringBuilder.ToString());
			SpriteText.drawString(b, currentText, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X - maxWidth / 2, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (lobbyUpdateListener != null && Program.sdk.Networking != null)
		{
			Program.sdk.Networking.RemoveLobbyUpdateListener(lobbyUpdateListener);
		}
		lobbyUpdateListener = null;
		base.Dispose(disposing);
	}
}
