using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.SDKs.Steam;

namespace StardewValley;

public class Options
{
	public enum ItemStowingModes
	{
		Off,
		GamepadOnly,
		Both
	}

	public enum GamepadModes
	{
		Auto,
		ForceOn,
		ForceOff
	}

	public const float minZoom = 0.75f;

	public const float maxZoom = 2f;

	public const float minUIZoom = 0.75f;

	public const float maxUIZoom = 1.5f;

	public const int toggleAutoRun = 0;

	public const int musicVolume = 1;

	public const int soundVolume = 2;

	public const int toggleDialogueTypingSounds = 3;

	public const int toggleFullscreen = 4;

	public const int screenResolution = 6;

	public const int showPortraitsToggle = 7;

	public const int showMerchantPortraitsToggle = 8;

	public const int menuBG = 9;

	public const int toggleFootsteps = 10;

	public const int alwaysShowToolHitLocationToggle = 11;

	public const int hideToolHitLocationWhenInMotionToggle = 12;

	public const int windowMode = 13;

	public const int pauseWhenUnfocused = 14;

	public const int pinToolbar = 15;

	public const int toggleRumble = 16;

	public const int ambientOnly = 17;

	public const int zoom = 18;

	public const int zoomButtonsToggle = 19;

	public const int ambientVolume = 20;

	public const int footstepVolume = 21;

	public const int invertScrollDirectionToggle = 22;

	public const int snowTransparencyToggle = 23;

	public const int screenFlashToggle = 24;

	public const int toggleHardwareCursor = 26;

	public const int toggleShowPlacementTileGamepad = 27;

	public const int stowingModeSelect = 28;

	public const int toggleSnappyMenus = 29;

	public const int toggleIPConnections = 30;

	public const int serverMode = 31;

	public const int toggleFarmhandCreation = 32;

	public const int toggleShowAdvancedCraftingInformation = 34;

	public const int toggleMPReadyStatus = 35;

	public const int mapScreenshot = 36;

	public const int toggleVsync = 37;

	public const int gamepadModeSelect = 38;

	public const int uiScaleSlider = 39;

	public const int moveBuildingPermissions = 40;

	public const int slingshotModeSelect = 41;

	public const int biteChime = 42;

	public const int toggleMuteAnimalSounds = 43;

	public const int toggleUseChineseSmoothFont = 44;

	public const int dialogueFontToggle = 45;

	public const int toggleUseAlternateFont = 46;

	public const int input_actionButton = 7;

	public const int input_cancelButton = 9;

	public const int input_useToolButton = 10;

	public const int input_moveUpButton = 11;

	public const int input_moveRightButton = 12;

	public const int input_moveDownButton = 13;

	public const int input_moveLeftButton = 14;

	public const int input_menuButton = 15;

	public const int input_runButton = 16;

	public const int input_chatButton = 17;

	public const int input_journalButton = 18;

	public const int input_mapButton = 19;

	public const int input_slot1 = 20;

	public const int input_slot2 = 21;

	public const int input_slot3 = 22;

	public const int input_slot4 = 23;

	public const int input_slot5 = 24;

	public const int input_slot6 = 25;

	public const int input_slot7 = 26;

	public const int input_slot8 = 27;

	public const int input_slot9 = 28;

	public const int input_slot10 = 29;

	public const int input_slot11 = 30;

	public const int input_slot12 = 31;

	public const int input_toolbarSwap = 32;

	public const int input_emoteButton = 33;

	public const float defaultZoomLevel = 1f;

	public const int defaultLightingQuality = 8;

	public const float defaultSplitScreenZoomLevel = 1f;

	public bool autoRun;

	public bool dialogueTyping;

	public bool showPortraits;

	public bool showMerchantPortraits;

	public bool showMenuBackground;

	public bool playFootstepSounds;

	public bool alwaysShowToolHitLocation;

	public bool hideToolHitLocationWhenInMotion;

	public bool pauseWhenOutOfFocus;

	public bool pinToolbarToggle;

	public bool mouseControls;

	public bool gamepadControls;

	public bool rumble;

	public bool ambientOnlyToggle;

	public bool zoomButtons;

	public bool invertScrollDirection;

	public bool screenFlash;

	public bool showPlacementTileForGamepad;

	public bool snappyMenus;

	public bool showAdvancedCraftingInformation;

	public bool showMPEndOfNightReadyStatus;

	public bool muteAnimalSounds;

	public bool vsyncEnabled;

	public bool fullscreen;

	public bool windowedBorderlessFullscreen;

	public bool showClearBackgrounds;

	public bool useChineseSmoothFont;

	public bool useAlternateFont;

	[DontLoadDefaultSetting]
	public bool ipConnectionsEnabled;

	[DontLoadDefaultSetting]
	public bool enableServer;

	[DontLoadDefaultSetting]
	public bool enableFarmhandCreation;

	protected bool _hardwareCursor;

	public ItemStowingModes stowingMode;

	[DontLoadDefaultSetting]
	public GamepadModes gamepadMode;

	public bool useLegacySlingshotFiring;

	public float musicVolumeLevel;

	public float soundVolumeLevel;

	public float footstepVolumeLevel;

	public float ambientVolumeLevel;

	public float snowTransparency;

	public float dialogueFontScale = 1f;

	[XmlIgnore]
	public float baseZoomLevel = 1f;

	[DontLoadDefaultSetting]
	[XmlElement("zoomLevel")]
	public float singlePlayerBaseZoomLevel = 1f;

	[DontLoadDefaultSetting]
	public float localCoopBaseZoomLevel = 1f;

	[DontLoadDefaultSetting]
	[XmlElement("uiScale")]
	public float singlePlayerDesiredUIScale = -1f;

	[DontLoadDefaultSetting]
	public float localCoopDesiredUIScale = 1.5f;

	[XmlIgnore]
	public float baseUIScale = 1f;

	public int preferredResolutionX;

	public int preferredResolutionY;

	[DontLoadDefaultSetting]
	public ServerPrivacy serverPrivacy = ServerPrivacy.FriendsOnly;

	public InputButton[] actionButton = new InputButton[2]
	{
		new InputButton(Keys.X),
		new InputButton(mouseLeft: false)
	};

	public InputButton[] cancelButton = new InputButton[1]
	{
		new InputButton(Keys.V)
	};

	public InputButton[] useToolButton = new InputButton[2]
	{
		new InputButton(Keys.C),
		new InputButton(mouseLeft: true)
	};

	public InputButton[] moveUpButton = new InputButton[1]
	{
		new InputButton(Keys.W)
	};

	public InputButton[] moveRightButton = new InputButton[1]
	{
		new InputButton(Keys.D)
	};

	public InputButton[] moveDownButton = new InputButton[1]
	{
		new InputButton(Keys.S)
	};

	public InputButton[] moveLeftButton = new InputButton[1]
	{
		new InputButton(Keys.A)
	};

	public InputButton[] menuButton = new InputButton[2]
	{
		new InputButton(Keys.E),
		new InputButton(Keys.Escape)
	};

	public InputButton[] runButton = new InputButton[1]
	{
		new InputButton(Keys.LeftShift)
	};

	public InputButton[] tmpKeyToReplace = new InputButton[1]
	{
		new InputButton(Keys.None)
	};

	public InputButton[] chatButton = new InputButton[2]
	{
		new InputButton(Keys.T),
		new InputButton(Keys.OemQuestion)
	};

	public InputButton[] mapButton = new InputButton[1]
	{
		new InputButton(Keys.M)
	};

	public InputButton[] journalButton = new InputButton[1]
	{
		new InputButton(Keys.F)
	};

	public InputButton[] inventorySlot1 = new InputButton[1]
	{
		new InputButton(Keys.D1)
	};

	public InputButton[] inventorySlot2 = new InputButton[1]
	{
		new InputButton(Keys.D2)
	};

	public InputButton[] inventorySlot3 = new InputButton[1]
	{
		new InputButton(Keys.D3)
	};

	public InputButton[] inventorySlot4 = new InputButton[1]
	{
		new InputButton(Keys.D4)
	};

	public InputButton[] inventorySlot5 = new InputButton[1]
	{
		new InputButton(Keys.D5)
	};

	public InputButton[] inventorySlot6 = new InputButton[1]
	{
		new InputButton(Keys.D6)
	};

	public InputButton[] inventorySlot7 = new InputButton[1]
	{
		new InputButton(Keys.D7)
	};

	public InputButton[] inventorySlot8 = new InputButton[1]
	{
		new InputButton(Keys.D8)
	};

	public InputButton[] inventorySlot9 = new InputButton[1]
	{
		new InputButton(Keys.D9)
	};

	public InputButton[] inventorySlot10 = new InputButton[1]
	{
		new InputButton(Keys.D0)
	};

	public InputButton[] inventorySlot11 = new InputButton[1]
	{
		new InputButton(Keys.OemMinus)
	};

	public InputButton[] inventorySlot12 = new InputButton[1]
	{
		new InputButton(Keys.OemPlus)
	};

	public InputButton[] toolbarSwap = new InputButton[1]
	{
		new InputButton(Keys.Tab)
	};

	public InputButton[] emoteButton = new InputButton[1]
	{
		new InputButton(Keys.Y)
	};

	[XmlIgnore]
	public bool optionsDirty;

	[XmlIgnore]
	private XmlSerializer defaultSettingsSerializer = new XmlSerializer(typeof(Options));

	private int appliedLightingQuality = -1;

	public bool hardwareCursor
	{
		get
		{
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				return false;
			}
			return _hardwareCursor;
		}
		set
		{
			_hardwareCursor = value;
		}
	}

	public int lightingQuality => 8;

	[XmlIgnore]
	public float zoomLevel
	{
		get
		{
			if (Game1.game1.takingMapScreenshot)
			{
				return baseZoomLevel;
			}
			return baseZoomLevel * Game1.game1.zoomModifier;
		}
	}

	[XmlIgnore]
	public float desiredBaseZoomLevel
	{
		get
		{
			if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
			{
				return localCoopBaseZoomLevel;
			}
			return singlePlayerBaseZoomLevel;
		}
		set
		{
			if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
			{
				localCoopBaseZoomLevel = value;
			}
			else
			{
				singlePlayerBaseZoomLevel = value;
			}
		}
	}

	[XmlIgnore]
	public float desiredUIScale
	{
		get
		{
			if (Game1.gameMode != 3)
			{
				return 1f;
			}
			if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
			{
				return localCoopDesiredUIScale;
			}
			return singlePlayerDesiredUIScale;
		}
		set
		{
			if (Game1.gameMode == 3)
			{
				if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
				{
					localCoopDesiredUIScale = value;
				}
				else
				{
					singlePlayerDesiredUIScale = value;
				}
			}
		}
	}

	[XmlIgnore]
	public float uiScale => baseUIScale * Game1.game1.zoomModifier;

	public bool allowStowing
	{
		get
		{
			switch (stowingMode)
			{
			case ItemStowingModes.Off:
				return false;
			case ItemStowingModes.GamepadOnly:
				if (gamepadControls)
				{
					if (Program.sdk is SteamHelper steamHelper && steamHelper.IsRunningOnSteamDeck() && Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
					{
						return false;
					}
					return true;
				}
				return false;
			default:
				return true;
			}
		}
	}

	public bool SnappyMenus
	{
		get
		{
			if (snappyMenus && gamepadControls && Game1.input.GetMouseState().LeftButton != ButtonState.Pressed)
			{
				return Game1.input.GetMouseState().RightButton != ButtonState.Pressed;
			}
			return false;
		}
	}

	public Options()
	{
		setToDefaults();
	}

	/// <summary>Get the absolute file path for the <c>default_options</c> file.</summary>
	public string GetFilePathForDefaultOptions()
	{
		return Path.Combine(Program.GetAppDataFolder(), "default_options");
	}

	public virtual void LoadDefaultOptions()
	{
		if (!Game1.game1.IsMainInstance)
		{
			return;
		}
		Options default_options = null;
		string filePath = GetFilePathForDefaultOptions();
		try
		{
			using FileStream stream = File.Open(filePath, FileMode.Open);
			default_options = defaultSettingsSerializer.Deserialize(stream) as Options;
		}
		catch (Exception)
		{
		}
		if (default_options == null)
		{
			return;
		}
		Type type = typeof(Options);
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo field in fields)
		{
			if (field.GetCustomAttribute<DontLoadDefaultSetting>() == null && field.GetCustomAttribute<XmlIgnoreAttribute>() == null)
			{
				field.SetValue(this, field.GetValue(default_options));
			}
		}
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo property_info in properties)
		{
			if (property_info.GetCustomAttribute<DontLoadDefaultSetting>() == null && property_info.GetCustomAttribute<XmlIgnoreAttribute>() == null && property_info.GetSetMethod() != null && property_info.GetGetMethod() != null)
			{
				property_info.SetValue(this, property_info.GetValue(default_options, null), null);
			}
		}
	}

	public virtual void SaveDefaultOptions()
	{
		optionsDirty = false;
		if (!Game1.game1.IsMainInstance)
		{
			return;
		}
		string filePath = GetFilePathForDefaultOptions();
		XmlWriterSettings settings = new XmlWriterSettings();
		try
		{
			using FileStream stream = File.Open(filePath, FileMode.Create);
			using XmlWriter writer = XmlWriter.Create(stream, settings);
			writer.WriteStartDocument();
			defaultSettingsSerializer.Serialize(writer, Game1.options);
			writer.WriteEndDocument();
			writer.Flush();
		}
		catch (Exception)
		{
		}
	}

	public void platformClampValues()
	{
	}

	public Keys getFirstKeyboardKeyFromInputButtonList(InputButton[] inputButton)
	{
		for (int i = 0; i < inputButton.Length; i++)
		{
			if (inputButton[i].key != 0)
			{
				return inputButton[i].key;
			}
		}
		return Keys.None;
	}

	public void reApplySetOptions()
	{
		platformClampValues();
		if (lightingQuality != appliedLightingQuality)
		{
			Program.gamePtr.refreshWindowSettings();
			appliedLightingQuality = lightingQuality;
		}
		Program.gamePtr.IsMouseVisible = hardwareCursor;
	}

	public void setToDefaults()
	{
		playFootstepSounds = true;
		showMenuBackground = false;
		showClearBackgrounds = false;
		showMerchantPortraits = true;
		showPortraits = true;
		autoRun = true;
		alwaysShowToolHitLocation = false;
		hideToolHitLocationWhenInMotion = true;
		dialogueTyping = true;
		rumble = true;
		fullscreen = false;
		pinToolbarToggle = false;
		baseZoomLevel = 1f;
		localCoopBaseZoomLevel = 1f;
		if (Game1.options == this)
		{
			Game1.forceSnapOnNextViewportUpdate = true;
		}
		zoomButtons = false;
		pauseWhenOutOfFocus = true;
		screenFlash = true;
		snowTransparency = 1f;
		invertScrollDirection = false;
		ambientOnlyToggle = false;
		showAdvancedCraftingInformation = false;
		stowingMode = ItemStowingModes.Off;
		useLegacySlingshotFiring = false;
		gamepadMode = GamepadModes.Auto;
		windowedBorderlessFullscreen = true;
		showPlacementTileForGamepad = true;
		hardwareCursor = false;
		musicVolumeLevel = 0.75f;
		ambientVolumeLevel = 0.75f;
		footstepVolumeLevel = 0.9f;
		soundVolumeLevel = 1f;
		dialogueFontScale = 1f;
		DisplayMode displayMode = Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes.Last();
		preferredResolutionX = displayMode.Width;
		preferredResolutionY = displayMode.Height;
		vsyncEnabled = true;
		GameRunner.instance.OnWindowSizeChange(null, null);
		snappyMenus = true;
		ipConnectionsEnabled = true;
		enableServer = true;
		serverPrivacy = ServerPrivacy.FriendsOnly;
		enableFarmhandCreation = true;
		showMPEndOfNightReadyStatus = false;
		muteAnimalSounds = false;
		useChineseSmoothFont = false;
		useAlternateFont = false;
	}

	public void setControlsToDefault()
	{
		actionButton = new InputButton[2]
		{
			new InputButton(Keys.X),
			new InputButton(mouseLeft: false)
		};
		cancelButton = new InputButton[1]
		{
			new InputButton(Keys.V)
		};
		useToolButton = new InputButton[2]
		{
			new InputButton(Keys.C),
			new InputButton(mouseLeft: true)
		};
		moveUpButton = new InputButton[1]
		{
			new InputButton(Keys.W)
		};
		moveRightButton = new InputButton[1]
		{
			new InputButton(Keys.D)
		};
		moveDownButton = new InputButton[1]
		{
			new InputButton(Keys.S)
		};
		moveLeftButton = new InputButton[1]
		{
			new InputButton(Keys.A)
		};
		menuButton = new InputButton[2]
		{
			new InputButton(Keys.E),
			new InputButton(Keys.Escape)
		};
		runButton = new InputButton[1]
		{
			new InputButton(Keys.LeftShift)
		};
		tmpKeyToReplace = new InputButton[1]
		{
			new InputButton(Keys.None)
		};
		chatButton = new InputButton[2]
		{
			new InputButton(Keys.T),
			new InputButton(Keys.OemQuestion)
		};
		mapButton = new InputButton[1]
		{
			new InputButton(Keys.M)
		};
		journalButton = new InputButton[1]
		{
			new InputButton(Keys.F)
		};
		inventorySlot1 = new InputButton[1]
		{
			new InputButton(Keys.D1)
		};
		inventorySlot2 = new InputButton[1]
		{
			new InputButton(Keys.D2)
		};
		inventorySlot3 = new InputButton[1]
		{
			new InputButton(Keys.D3)
		};
		inventorySlot4 = new InputButton[1]
		{
			new InputButton(Keys.D4)
		};
		inventorySlot5 = new InputButton[1]
		{
			new InputButton(Keys.D5)
		};
		inventorySlot6 = new InputButton[1]
		{
			new InputButton(Keys.D6)
		};
		inventorySlot7 = new InputButton[1]
		{
			new InputButton(Keys.D7)
		};
		inventorySlot8 = new InputButton[1]
		{
			new InputButton(Keys.D8)
		};
		inventorySlot9 = new InputButton[1]
		{
			new InputButton(Keys.D9)
		};
		inventorySlot10 = new InputButton[1]
		{
			new InputButton(Keys.D0)
		};
		inventorySlot11 = new InputButton[1]
		{
			new InputButton(Keys.OemMinus)
		};
		inventorySlot12 = new InputButton[1]
		{
			new InputButton(Keys.OemPlus)
		};
		emoteButton = new InputButton[1]
		{
			new InputButton(Keys.Y)
		};
		toolbarSwap = new InputButton[1]
		{
			new InputButton(Keys.Tab)
		};
	}

	public string getNameOfOptionFromIndex(int index)
	{
		return index switch
		{
			0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4556"), 
			1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4557"), 
			2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4558"), 
			3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4559"), 
			4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4560"), 
			5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4561"), 
			6 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4562"), 
			_ => "", 
		};
	}

	public void changeCheckBoxOption(int which, bool value)
	{
		switch (which)
		{
		case 0:
			autoRun = value;
			Game1.player.setRunning(autoRun);
			break;
		case 3:
			dialogueTyping = value;
			break;
		case 7:
			showPortraits = value;
			break;
		case 8:
			showMerchantPortraits = value;
			break;
		case 9:
			showMenuBackground = value;
			break;
		case 10:
			playFootstepSounds = value;
			break;
		case 11:
			alwaysShowToolHitLocation = value;
			break;
		case 12:
			hideToolHitLocationWhenInMotion = value;
			break;
		case 14:
			pauseWhenOutOfFocus = value;
			break;
		case 15:
			pinToolbarToggle = value;
			break;
		case 16:
			rumble = value;
			break;
		case 17:
			ambientOnlyToggle = value;
			break;
		case 19:
			zoomButtons = value;
			break;
		case 22:
			invertScrollDirection = value;
			break;
		case 24:
			screenFlash = value;
			break;
		case 26:
			hardwareCursor = value;
			Program.gamePtr.IsMouseVisible = hardwareCursor;
			break;
		case 27:
			showPlacementTileForGamepad = value;
			break;
		case 37:
			vsyncEnabled = value;
			GameRunner.instance.OnWindowSizeChange(null, null);
			break;
		case 29:
			snappyMenus = value;
			break;
		case 30:
			ipConnectionsEnabled = value;
			break;
		case 32:
			enableFarmhandCreation = value;
			Game1.server?.updateLobbyData();
			break;
		case 34:
			showAdvancedCraftingInformation = value;
			break;
		case 35:
			showMPEndOfNightReadyStatus = value;
			break;
		case 43:
			muteAnimalSounds = value;
			break;
		case 44:
			useChineseSmoothFont = value;
			loadChineseFonts();
			break;
		case 46:
			useAlternateFont = value;
			break;
		}
		optionsDirty = true;
	}

	public void changeSliderOption(int which, int value)
	{
		switch (which)
		{
		case 45:
			dialogueFontScale = (float)value / 100f + 1f;
			break;
		case 1:
			musicVolumeLevel = (float)value / 100f;
			Game1.musicCategory.SetVolume(musicVolumeLevel);
			Game1.musicPlayerVolume = musicVolumeLevel;
			break;
		case 2:
			soundVolumeLevel = (float)value / 100f;
			Game1.soundCategory.SetVolume(soundVolumeLevel);
			break;
		case 20:
			ambientVolumeLevel = (float)value / 100f;
			Game1.ambientCategory.SetVolume(ambientVolumeLevel);
			Game1.ambientPlayerVolume = ambientVolumeLevel;
			break;
		case 21:
			footstepVolumeLevel = (float)value / 100f;
			Game1.footstepCategory.SetVolume(footstepVolumeLevel);
			break;
		case 23:
			snowTransparency = (float)value / 100f;
			break;
		case 39:
		{
			int zoomlvl = (int)(desiredUIScale * 100f);
			int newValue = (int)((float)value * 100f);
			if (newValue >= zoomlvl + 10 || newValue >= 100)
			{
				zoomlvl += 10;
				zoomlvl = Math.Min(100, zoomlvl);
			}
			else if (newValue <= zoomlvl - 10 || newValue <= 50)
			{
				zoomlvl -= 10;
				zoomlvl = Math.Max(50, zoomlvl);
			}
			desiredUIScale = (float)zoomlvl / 100f;
			break;
		}
		case 18:
		{
			int zoomlvl = (int)(desiredBaseZoomLevel * 100f);
			int oldZoom = zoomlvl;
			int newValue = (int)((float)value * 100f);
			if (newValue >= zoomlvl + 10 || newValue >= 100)
			{
				zoomlvl += 10;
				zoomlvl = Math.Min(100, zoomlvl);
			}
			else if (newValue <= zoomlvl - 10 || newValue <= 50)
			{
				zoomlvl -= 10;
				zoomlvl = Math.Max(50, zoomlvl);
			}
			if (zoomlvl != oldZoom)
			{
				desiredBaseZoomLevel = (float)zoomlvl / 100f;
				Game1.forceSnapOnNextViewportUpdate = true;
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4563") + zoomLevel);
			}
			break;
		}
		}
		optionsDirty = true;
	}

	public void loadChineseFonts()
	{
		if (useChineseSmoothFont)
		{
			Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\Chinese_round\\SmallFont");
			Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\Chinese_round\\SpriteFont1");
			SpriteText.LoadFontData(LocalizedContentManager.LanguageCode.zh);
		}
		else
		{
			Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\SmallFont");
			Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\SpriteFont1");
			SpriteText.LoadFontData(LocalizedContentManager.LanguageCode.zh);
		}
	}

	public void setBackgroundMode(string setting)
	{
		switch (setting)
		{
		case "Standard":
			showMenuBackground = false;
			showClearBackgrounds = false;
			break;
		case "Graphical":
			showMenuBackground = true;
			break;
		case "None":
			showClearBackgrounds = true;
			showMenuBackground = false;
			break;
		}
	}

	public void setStowingMode(string setting)
	{
		switch (setting)
		{
		case "off":
			stowingMode = ItemStowingModes.Off;
			break;
		case "gamepad":
			stowingMode = ItemStowingModes.GamepadOnly;
			break;
		case "both":
			stowingMode = ItemStowingModes.Both;
			break;
		}
	}

	public void setSlingshotMode(string setting)
	{
		if (setting == "legacy")
		{
			useLegacySlingshotFiring = true;
		}
		else
		{
			useLegacySlingshotFiring = false;
		}
	}

	public void setBiteChime(string setting)
	{
		try
		{
			Game1.player.biteChime.Value = int.Parse(setting);
		}
		catch (Exception)
		{
			Game1.player.biteChime.Value = -1;
		}
	}

	public void setGamepadMode(string setting)
	{
		switch (setting)
		{
		case "auto":
			gamepadMode = GamepadModes.Auto;
			break;
		case "force_on":
			gamepadMode = GamepadModes.ForceOn;
			break;
		case "force_off":
			gamepadMode = GamepadModes.ForceOff;
			break;
		}
		try
		{
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false, applyLanguage: false);
			startupPreferences.gamepadMode = gamepadMode;
			startupPreferences.savePreferences(async: false);
		}
		catch (Exception)
		{
		}
	}

	public void setMoveBuildingPermissions(string setting)
	{
		switch (setting)
		{
		case "off":
			Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.Off;
			break;
		case "on":
			Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.On;
			break;
		case "owned":
			Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.OwnedBuildings;
			break;
		}
	}

	public void setServerMode(string setting)
	{
		switch (setting)
		{
		case "offline":
			enableServer = false;
			Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ServerOfflineMode);
			return;
		case "friends":
			serverPrivacy = ServerPrivacy.FriendsOnly;
			break;
		case "invite":
			serverPrivacy = ServerPrivacy.InviteOnly;
			break;
		}
		if (Game1.server == null && Game1.client == null)
		{
			enableServer = true;
			Game1.multiplayer.StartServer();
		}
		else if (Game1.server != null)
		{
			enableServer = true;
			Game1.server.setPrivacy(serverPrivacy);
		}
	}

	public void setWindowedOption(string setting)
	{
		switch (setting)
		{
		case "Windowed":
			setWindowedOption(1);
			break;
		case "Fullscreen":
			setWindowedOption(2);
			break;
		case "Windowed Borderless":
			setWindowedOption(0);
			break;
		}
	}

	public void setWindowedOption(int setting)
	{
		windowedBorderlessFullscreen = isCurrentlyWindowedBorderless();
		fullscreen = !windowedBorderlessFullscreen && Game1.graphics.IsFullScreen;
		int whichMode = -1;
		switch (setting)
		{
		case 1:
			if (Game1.graphics.IsFullScreen && !windowedBorderlessFullscreen)
			{
				fullscreen = false;
				Game1.toggleNonBorderlessWindowedFullscreen();
				windowedBorderlessFullscreen = false;
			}
			else if (windowedBorderlessFullscreen)
			{
				fullscreen = false;
				windowedBorderlessFullscreen = false;
				Game1.toggleFullscreen();
			}
			whichMode = 1;
			break;
		case 2:
			if (windowedBorderlessFullscreen)
			{
				fullscreen = true;
				windowedBorderlessFullscreen = false;
				Game1.toggleFullscreen();
			}
			else if (!Game1.graphics.IsFullScreen)
			{
				fullscreen = true;
				windowedBorderlessFullscreen = false;
				Game1.toggleNonBorderlessWindowedFullscreen();
				hardwareCursor = false;
				Program.gamePtr.IsMouseVisible = false;
			}
			whichMode = 2;
			break;
		case 0:
			if (!windowedBorderlessFullscreen)
			{
				windowedBorderlessFullscreen = true;
				Game1.toggleFullscreen();
				fullscreen = false;
			}
			whichMode = 0;
			break;
		}
		try
		{
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false, applyLanguage: false);
			startupPreferences.windowMode = whichMode;
			startupPreferences.fullscreenResolutionX = preferredResolutionX;
			startupPreferences.fullscreenResolutionY = preferredResolutionY;
			startupPreferences.displayIndex = GameRunner.instance.Window.GetDisplayIndex();
			startupPreferences.savePreferences(async: false);
		}
		catch (Exception)
		{
		}
	}

	public void changeDropDownOption(int which, string value)
	{
		switch (which)
		{
		case 9:
			setBackgroundMode(value);
			break;
		case 39:
		{
			int newZoom = Convert.ToInt32(value.Replace("%", ""));
			desiredUIScale = (float)newZoom / 100f;
			break;
		}
		case 18:
		{
			int newZoom = Convert.ToInt32(value.Replace("%", ""));
			desiredBaseZoomLevel = (float)newZoom / 100f;
			Game1.forceSnapOnNextViewportUpdate = true;
			if (Game1.debrisWeather != null)
			{
				Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
			}
			Game1.randomizeRainPositions();
			break;
		}
		case 6:
		{
			string[] array = ArgUtility.SplitBySpace(value);
			int width = Convert.ToInt32(array[0]);
			int height = Convert.ToInt32(array[2]);
			preferredResolutionX = width;
			preferredResolutionY = height;
			Game1.graphics.PreferredBackBufferWidth = width;
			Game1.graphics.PreferredBackBufferHeight = height;
			if (!isCurrentlyWindowed())
			{
				try
				{
					StartupPreferences startupPreferences = new StartupPreferences();
					startupPreferences.loadPreferences(async: false, applyLanguage: false);
					startupPreferences.fullscreenResolutionX = preferredResolutionX;
					startupPreferences.fullscreenResolutionY = preferredResolutionY;
					startupPreferences.savePreferences(async: false);
				}
				catch (Exception)
				{
				}
			}
			Game1.graphics.ApplyChanges();
			GameRunner.instance.OnWindowSizeChange(null, null);
			break;
		}
		case 13:
			setWindowedOption(value);
			break;
		case 31:
			setServerMode(value);
			break;
		case 28:
			setStowingMode(value);
			break;
		case 38:
			setGamepadMode(value);
			break;
		case 40:
			setMoveBuildingPermissions(value);
			break;
		case 41:
			setSlingshotMode(value);
			break;
		case 42:
			setBiteChime(value);
			Game1.player.PlayFishBiteChime();
			break;
		}
		optionsDirty = true;
	}

	public bool isKeyInUse(Keys key)
	{
		foreach (InputButton allUsedInputButton in getAllUsedInputButtons())
		{
			if (allUsedInputButton.key == key)
			{
				return true;
			}
		}
		return false;
	}

	public List<InputButton> getAllUsedInputButtons()
	{
		List<InputButton> list = new List<InputButton>();
		list.AddRange(useToolButton);
		list.AddRange(actionButton);
		list.AddRange(moveUpButton);
		list.AddRange(moveRightButton);
		list.AddRange(moveDownButton);
		list.AddRange(moveLeftButton);
		list.AddRange(runButton);
		list.AddRange(menuButton);
		list.AddRange(journalButton);
		list.AddRange(mapButton);
		list.AddRange(chatButton);
		list.AddRange(inventorySlot1);
		list.AddRange(inventorySlot2);
		list.AddRange(inventorySlot3);
		list.AddRange(inventorySlot4);
		list.AddRange(inventorySlot5);
		list.AddRange(inventorySlot6);
		list.AddRange(inventorySlot7);
		list.AddRange(inventorySlot8);
		list.AddRange(inventorySlot9);
		list.AddRange(inventorySlot10);
		list.AddRange(inventorySlot11);
		list.AddRange(inventorySlot12);
		list.AddRange(toolbarSwap);
		list.AddRange(emoteButton);
		return list;
	}

	public void setCheckBoxToProperValue(OptionsCheckbox checkbox)
	{
		switch (checkbox.whichOption)
		{
		case 0:
			checkbox.isChecked = autoRun;
			break;
		case 3:
			checkbox.isChecked = dialogueTyping;
			break;
		case 4:
			fullscreen = Game1.graphics.IsFullScreen || windowedBorderlessFullscreen;
			checkbox.isChecked = fullscreen;
			break;
		case 5:
			checkbox.isChecked = windowedBorderlessFullscreen;
			checkbox.greyedOut = !fullscreen;
			break;
		case 7:
			checkbox.isChecked = showPortraits;
			break;
		case 8:
			checkbox.isChecked = showMerchantPortraits;
			break;
		case 9:
			checkbox.isChecked = showMenuBackground;
			break;
		case 10:
			checkbox.isChecked = playFootstepSounds;
			break;
		case 11:
			checkbox.isChecked = alwaysShowToolHitLocation;
			break;
		case 12:
			checkbox.isChecked = hideToolHitLocationWhenInMotion;
			break;
		case 14:
			checkbox.isChecked = pauseWhenOutOfFocus;
			break;
		case 15:
			checkbox.isChecked = pinToolbarToggle;
			break;
		case 16:
			checkbox.isChecked = rumble;
			checkbox.greyedOut = !gamepadControls;
			break;
		case 17:
			checkbox.isChecked = ambientOnlyToggle;
			break;
		case 19:
			checkbox.isChecked = zoomButtons;
			break;
		case 22:
			checkbox.isChecked = invertScrollDirection;
			break;
		case 24:
			checkbox.isChecked = screenFlash;
			break;
		case 26:
			checkbox.isChecked = _hardwareCursor;
			checkbox.greyedOut = fullscreen;
			break;
		case 27:
			checkbox.isChecked = showPlacementTileForGamepad;
			checkbox.greyedOut = !gamepadControls;
			break;
		case 29:
			checkbox.isChecked = snappyMenus;
			break;
		case 30:
			checkbox.isChecked = ipConnectionsEnabled;
			break;
		case 32:
			checkbox.isChecked = enableFarmhandCreation;
			break;
		case 34:
			checkbox.isChecked = showAdvancedCraftingInformation;
			break;
		case 35:
			checkbox.isChecked = showMPEndOfNightReadyStatus;
			break;
		case 37:
			checkbox.isChecked = vsyncEnabled;
			break;
		case 43:
			checkbox.isChecked = muteAnimalSounds;
			break;
		case 44:
			checkbox.isChecked = useChineseSmoothFont;
			break;
		case 46:
			checkbox.isChecked = useAlternateFont;
			break;
		case 1:
		case 2:
		case 6:
		case 13:
		case 18:
		case 20:
		case 21:
		case 23:
		case 25:
		case 28:
		case 31:
		case 33:
		case 36:
		case 38:
		case 39:
		case 40:
		case 41:
		case 42:
		case 45:
			break;
		}
	}

	public void setPlusMinusToProperValue(OptionsPlusMinus plusMinus)
	{
		switch (plusMinus.whichOption)
		{
		case 39:
		{
			string currentZoom = Math.Round(desiredUIScale * 100f) + "%";
			for (int i = 0; i < plusMinus.options.Count; i++)
			{
				if (plusMinus.options[i].Equals(currentZoom))
				{
					plusMinus.selected = i;
					break;
				}
			}
			break;
		}
		case 18:
		{
			string currentZoom = Math.Round(desiredBaseZoomLevel * 100f) + "%";
			for (int i = 0; i < plusMinus.options.Count; i++)
			{
				if (plusMinus.options[i].Equals(currentZoom))
				{
					plusMinus.selected = i;
					break;
				}
			}
			break;
		}
		}
	}

	public void setSliderToProperValue(OptionsSlider slider)
	{
		switch (slider.whichOption)
		{
		case 45:
			slider.value = (int)((dialogueFontScale - 1f) * 100f);
			break;
		case 1:
			slider.value = (int)(musicVolumeLevel * 100f);
			break;
		case 2:
			slider.value = (int)(soundVolumeLevel * 100f);
			break;
		case 20:
			slider.value = (int)(ambientVolumeLevel * 100f);
			break;
		case 21:
			slider.value = (int)(footstepVolumeLevel * 100f);
			break;
		case 23:
			slider.value = (int)(snowTransparency * 100f);
			break;
		case 18:
			slider.value = (int)(desiredBaseZoomLevel * 100f);
			break;
		case 39:
			slider.value = (int)(desiredUIScale * 100f);
			break;
		}
	}

	public bool doesInputListContain(InputButton[] list, Keys key)
	{
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i].key == key)
			{
				return true;
			}
		}
		return false;
	}

	public void changeInputListenerValue(int whichListener, Keys key)
	{
		switch (whichListener)
		{
		case 7:
			actionButton[0] = new InputButton(key);
			break;
		case 17:
			chatButton[0] = new InputButton(key);
			break;
		case 15:
			menuButton[0] = new InputButton(key);
			break;
		case 13:
			moveDownButton[0] = new InputButton(key);
			break;
		case 14:
			moveLeftButton[0] = new InputButton(key);
			break;
		case 12:
			moveRightButton[0] = new InputButton(key);
			break;
		case 11:
			moveUpButton[0] = new InputButton(key);
			break;
		case 16:
			runButton[0] = new InputButton(key);
			break;
		case 10:
			useToolButton[0] = new InputButton(key);
			break;
		case 18:
			journalButton[0] = new InputButton(key);
			break;
		case 19:
			mapButton[0] = new InputButton(key);
			break;
		case 20:
			inventorySlot1[0] = new InputButton(key);
			break;
		case 21:
			inventorySlot2[0] = new InputButton(key);
			break;
		case 22:
			inventorySlot3[0] = new InputButton(key);
			break;
		case 23:
			inventorySlot4[0] = new InputButton(key);
			break;
		case 24:
			inventorySlot5[0] = new InputButton(key);
			break;
		case 25:
			inventorySlot6[0] = new InputButton(key);
			break;
		case 26:
			inventorySlot7[0] = new InputButton(key);
			break;
		case 27:
			inventorySlot8[0] = new InputButton(key);
			break;
		case 28:
			inventorySlot9[0] = new InputButton(key);
			break;
		case 29:
			inventorySlot10[0] = new InputButton(key);
			break;
		case 30:
			inventorySlot11[0] = new InputButton(key);
			break;
		case 31:
			inventorySlot12[0] = new InputButton(key);
			break;
		case 32:
			toolbarSwap[0] = new InputButton(key);
			break;
		case 33:
			emoteButton[0] = new InputButton(key);
			break;
		}
		optionsDirty = true;
	}

	public void setInputListenerToProperValue(OptionsInputListener inputListener)
	{
		inputListener.buttonNames.Clear();
		switch (inputListener.whichOption)
		{
		case 7:
		{
			InputButton[] array = actionButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 17:
		{
			InputButton[] array = chatButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 15:
		{
			InputButton[] array = menuButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 13:
		{
			InputButton[] array = moveDownButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 14:
		{
			InputButton[] array = moveLeftButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 12:
		{
			InputButton[] array = moveRightButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 11:
		{
			InputButton[] array = moveUpButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 16:
		{
			InputButton[] array = runButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 10:
		{
			InputButton[] array = useToolButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 32:
		{
			InputButton[] array = toolbarSwap;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 18:
		{
			InputButton[] array = journalButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 19:
		{
			InputButton[] array = mapButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 20:
		{
			InputButton[] array = inventorySlot1;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 21:
		{
			InputButton[] array = inventorySlot2;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 22:
		{
			InputButton[] array = inventorySlot3;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 23:
		{
			InputButton[] array = inventorySlot4;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 24:
		{
			InputButton[] array = inventorySlot5;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 25:
		{
			InputButton[] array = inventorySlot6;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 26:
		{
			InputButton[] array = inventorySlot7;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 27:
		{
			InputButton[] array = inventorySlot8;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 28:
		{
			InputButton[] array = inventorySlot9;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 29:
		{
			InputButton[] array = inventorySlot10;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 30:
		{
			InputButton[] array = inventorySlot11;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 31:
		{
			InputButton[] array = inventorySlot12;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 33:
		{
			InputButton[] array = emoteButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 8:
		case 9:
			break;
		}
	}

	public void setDropDownToProperValue(OptionsDropDown dropDown)
	{
		switch (dropDown.whichOption)
		{
		case 9:
			dropDown.dropDownOptions.Add("Standard");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\1_6_Strings:options_menubg_0"));
			dropDown.dropDownOptions.Add("Graphical");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\1_6_Strings:options_menubg_1"));
			dropDown.dropDownOptions.Add("None");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\1_6_Strings:options_menubg_2"));
			if (showMenuBackground)
			{
				dropDown.selectedOption = 1;
			}
			else if (!showClearBackgrounds)
			{
				dropDown.selectedOption = 0;
			}
			else
			{
				dropDown.selectedOption = 2;
			}
			break;
		case 6:
		{
			try
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(async: false, applyLanguage: false);
				if (startupPreferences.fullscreenResolutionX != 0)
				{
					preferredResolutionX = startupPreferences.fullscreenResolutionX;
					preferredResolutionY = startupPreferences.fullscreenResolutionY;
				}
			}
			catch (Exception)
			{
			}
			int i = 0;
			foreach (DisplayMode v in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
			{
				if (v.Width >= 1280)
				{
					dropDown.dropDownOptions.Add(v.Width + " x " + v.Height);
					dropDown.dropDownDisplayOptions.Add(v.Width + " x " + v.Height);
					if (v.Width == preferredResolutionX && v.Height == preferredResolutionY)
					{
						dropDown.selectedOption = i;
					}
					i++;
				}
			}
			dropDown.greyedOut = !fullscreen || windowedBorderlessFullscreen;
			break;
		}
		case 13:
			windowedBorderlessFullscreen = isCurrentlyWindowedBorderless();
			fullscreen = Game1.graphics.IsFullScreen && !windowedBorderlessFullscreen;
			dropDown.dropDownOptions.Add("Windowed");
			if (!windowedBorderlessFullscreen)
			{
				dropDown.dropDownOptions.Add("Fullscreen");
			}
			if (!fullscreen)
			{
				dropDown.dropDownOptions.Add("Windowed Borderless");
			}
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4564"));
			if (!windowedBorderlessFullscreen)
			{
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4560"));
			}
			if (!fullscreen)
			{
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4561"));
			}
			if (Game1.graphics.IsFullScreen || windowedBorderlessFullscreen)
			{
				dropDown.selectedOption = 1;
			}
			else
			{
				dropDown.selectedOption = 0;
			}
			break;
		case 28:
			dropDown.dropDownOptions.Add("off");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_Off"));
			dropDown.dropDownOptions.Add("gamepad");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_GamepadOnly"));
			dropDown.dropDownOptions.Add("both");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_On"));
			switch (stowingMode)
			{
			case ItemStowingModes.Off:
				dropDown.selectedOption = 0;
				break;
			case ItemStowingModes.GamepadOnly:
				dropDown.selectedOption = 1;
				break;
			case ItemStowingModes.Both:
				dropDown.selectedOption = 2;
				break;
			}
			break;
		case 38:
			try
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(async: false, applyLanguage: false);
				gamepadMode = startupPreferences.gamepadMode;
			}
			catch (Exception)
			{
			}
			dropDown.dropDownOptions.Add("auto");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_Auto"));
			dropDown.dropDownOptions.Add("force_on");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_ForceOn"));
			dropDown.dropDownOptions.Add("force_off");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_ForceOff"));
			switch (gamepadMode)
			{
			case GamepadModes.Auto:
				dropDown.selectedOption = 0;
				break;
			case GamepadModes.ForceOn:
				dropDown.selectedOption = 1;
				break;
			case GamepadModes.ForceOff:
				dropDown.selectedOption = 2;
				break;
			}
			break;
		case 41:
			dropDown.dropDownOptions.Add("hold");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode_Hold"));
			dropDown.dropDownOptions.Add("legacy");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode_Pull"));
			if (useLegacySlingshotFiring)
			{
				dropDown.selectedOption = 1;
			}
			else
			{
				dropDown.selectedOption = 0;
			}
			break;
		case 42:
		{
			dropDown.dropDownOptions.Add("-1");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:BiteChime_Default"));
			for (int j = 0; j <= 3; j++)
			{
				dropDown.dropDownOptions.Add(j.ToString());
				dropDown.dropDownDisplayOptions.Add((j + 1).ToString());
			}
			dropDown.selectedOption = Game1.player.biteChime.Value + 1;
			break;
		}
		case 31:
			dropDown.dropDownOptions.Add("offline");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_Offline"));
			if (Program.sdk.Networking != null)
			{
				dropDown.dropDownOptions.Add("friends");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_FriendsOnly"));
				dropDown.dropDownOptions.Add("invite");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_InviteOnly"));
			}
			else
			{
				dropDown.dropDownOptions.Add("online");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_Online"));
			}
			if (Game1.server == null)
			{
				dropDown.selectedOption = 0;
			}
			else if (Program.sdk.Networking != null)
			{
				switch (serverPrivacy)
				{
				case ServerPrivacy.FriendsOnly:
					dropDown.selectedOption = 1;
					break;
				case ServerPrivacy.InviteOnly:
					dropDown.selectedOption = 2;
					break;
				}
			}
			else
			{
				dropDown.selectedOption = 1;
			}
			Game1.log.Verbose("setDropDownToProperValue( serverMode, " + dropDown.dropDownOptions[dropDown.selectedOption] + " ) called.");
			break;
		case 40:
			dropDown.dropDownOptions.Add("on");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_On"));
			dropDown.dropDownOptions.Add("owned");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_Owned"));
			dropDown.dropDownOptions.Add("off");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_Off"));
			switch (Game1.player.team.farmhandsCanMoveBuildings.Value)
			{
			case FarmerTeam.RemoteBuildingPermissions.On:
				dropDown.selectedOption = 0;
				break;
			case FarmerTeam.RemoteBuildingPermissions.OwnedBuildings:
				dropDown.selectedOption = 1;
				break;
			case FarmerTeam.RemoteBuildingPermissions.Off:
				dropDown.selectedOption = 2;
				break;
			}
			break;
		}
	}

	public bool isCurrentlyWindowedBorderless()
	{
		if (Game1.graphics.IsFullScreen)
		{
			return !Game1.graphics.HardwareModeSwitch;
		}
		return false;
	}

	public bool isCurrentlyFullscreen()
	{
		if (Game1.graphics.IsFullScreen)
		{
			return Game1.graphics.HardwareModeSwitch;
		}
		return false;
	}

	public bool isCurrentlyWindowed()
	{
		if (!isCurrentlyWindowedBorderless())
		{
			return !isCurrentlyFullscreen();
		}
		return false;
	}
}
