using System;
using Galaxy.Api;
using StardewValley.Menus;
using StardewValley.SDKs.GogGalaxy.Listeners;
using Steamworks;

namespace StardewValley.SDKs.Steam;

public class SteamHelper : SDKHelper
{
	private Callback<GameOverlayActivated_t> gameOverlayActivated;

	private CallResult<EncryptedAppTicketResponse_t> encryptedAppTicketResponse;

	private Callback<GamepadTextInputDismissed_t> gamepadTextInputDismissed;

	private GalaxyAuthListener galaxyAuthListener;

	private GalaxyOperationalStateChangeListener galaxyStateChangeListener;

	private GalaxySpecificUserDataListener galaxySpecificUserDataListener;

	public bool active;

	private SDKNetHelper networking;

	private TextBox _keyboardTextBox;

	protected bool _runningOnSteamDeck;

	public SDKNetHelper Networking => networking;

	public bool ConnectionFinished { get; private set; }

	public int ConnectionProgress { get; private set; }

	public bool GalaxyConnected { get; private set; }

	public string Name { get; } = "Steam";


	public bool HasOverlay => false;

	public bool IsJapaneseRegionRelease => false;

	public bool IsEnterButtonAssignmentFlipped => false;

	public void EarlyInitialize()
	{
	}

	public virtual bool IsRunningOnSteamDeck()
	{
		return _runningOnSteamDeck;
	}

	public void Initialize()
	{
		try
		{
			active = SteamAPI.Init();
			Game1.log.Verbose("Steam logged on: " + SteamUser.BLoggedOn());
			if (active)
			{
				_runningOnSteamDeck = SteamUtils.IsSteamRunningOnSteamDeck();
				Game1.log.Verbose("Initializing GalaxySDK");
				encryptedAppTicketResponse = CallResult<EncryptedAppTicketResponse_t>.Create(onEncryptedAppTicketResponse);
				Game1.log.Verbose("Requesting Steam app ticket");
				SteamAPICall_t handle = SteamUser.RequestEncryptedAppTicket(LegacyShims.EmptyArray<byte>(), 0);
				encryptedAppTicketResponse.Set(handle);
				ConnectionProgress++;
				SteamNetworkingUtils.InitRelayNetworkAccess();
			}
		}
		catch (Exception e)
		{
			Game1.log.Error("Error connecting to Steam.", e);
			active = false;
			ConnectionFinished = true;
		}
		if (active)
		{
			try
			{
				GalaxyInstance.Init(new InitParams("48767653913349277", "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a", "."));
				galaxyAuthListener = new GalaxyAuthListener(onGalaxyAuthSuccess, onGalaxyAuthFailure, onGalaxyAuthLost);
				galaxyStateChangeListener = new GalaxyOperationalStateChangeListener(onGalaxyStateChange);
			}
			catch (Exception e)
			{
				Game1.log.Error("Error initializing the Galaxy API.", e);
			}
			gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(onGameOverlayActivated);
			gamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnKeyboardDismissed);
		}
	}

	public void CancelKeyboard()
	{
		_keyboardTextBox = null;
	}

	public void ShowKeyboard(TextBox text_box)
	{
		_keyboardTextBox = text_box;
		SteamUtils.ShowGamepadTextInput(text_box.PasswordBox ? EGamepadTextInputMode.k_EGamepadTextInputModePassword : EGamepadTextInputMode.k_EGamepadTextInputModeNormal, EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, "", (text_box.textLimit < 0) ? 100u : ((uint)text_box.textLimit), text_box.Text);
	}

	public void OnKeyboardDismissed(GamepadTextInputDismissed_t callback)
	{
		if (_keyboardTextBox == null)
		{
			return;
		}
		if (!callback.m_bSubmitted)
		{
			_keyboardTextBox = null;
			return;
		}
		uint length = SteamUtils.GetEnteredGamepadTextLength();
		if (!SteamUtils.GetEnteredGamepadTextInput(out var entered_text, length))
		{
			_keyboardTextBox = null;
			return;
		}
		_keyboardTextBox.RecieveTextInput(entered_text);
		_keyboardTextBox = null;
	}

	private void onSetGalaxyProfileName(GalaxyID userID)
	{
		try
		{
			if (userID != GalaxyInstance.User().GetGalaxyID())
			{
				return;
			}
		}
		catch (Exception)
		{
			return;
		}
		Game1.log.Verbose("Successfully set GOG Galaxy profile name.");
		galaxySpecificUserDataListener?.Dispose();
		galaxySpecificUserDataListener = null;
	}

	private void onGalaxyStateChange(uint operationalState)
	{
		if (networking != null)
		{
			return;
		}
		if ((operationalState & (true ? 1u : 0u)) != 0)
		{
			Game1.log.Verbose("Galaxy signed in");
			ConnectionProgress++;
		}
		if ((operationalState & 2) == 0)
		{
			return;
		}
		Game1.log.Verbose("Galaxy logged on");
		networking = new SteamNetHelper();
		ConnectionProgress++;
		ConnectionFinished = true;
		GalaxyConnected = true;
		try
		{
			galaxySpecificUserDataListener = new GalaxySpecificUserDataListener(onSetGalaxyProfileName);
			GalaxyInstance.User().SetUserData("StardewDisplayName", SteamFriends.GetPersonaName());
		}
		catch (Exception ex)
		{
			Game1.log.Error("Failed to set GOG Galaxy profile name.", ex);
		}
	}

	private void onGalaxyAuthSuccess()
	{
		Game1.log.Verbose("Galaxy auth success");
		ConnectionProgress++;
	}

	private void onGalaxyAuthFailure(IAuthListener.FailureReason reason)
	{
		if (networking == null)
		{
			networking = new SteamNetHelper();
		}
		Game1.log.Error("Galaxy auth failure: " + reason);
		ConnectionFinished = true;
		GalaxyConnected = false;
	}

	private void onGalaxyAuthLost()
	{
		if (networking == null)
		{
			networking = new SteamNetHelper();
		}
		Game1.log.Error("Galaxy auth lost");
		ConnectionFinished = true;
		GalaxyConnected = false;
	}

	private void onEncryptedAppTicketResponse(EncryptedAppTicketResponse_t response, bool ioFailure)
	{
		if (response.m_eResult == EResult.k_EResultOK)
		{
			byte[] ticket = new byte[1024];
			SteamUser.GetEncryptedAppTicket(ticket, 1024, out var ticketSize);
			ConnectionProgress++;
			Game1.log.Verbose("Signing into GalaxySDK");
			try
			{
				GalaxyInstance.User().SignInSteam(ticket, ticketSize, SteamFriends.GetPersonaName());
				return;
			}
			catch (Exception e)
			{
				Game1.log.Error("Galaxy SignInSteam failed with an exception:", e);
				return;
			}
		}
		Game1.log.Error("Failed to retrieve encrypted app ticket: " + response.m_eResult.ToString() + ", " + ioFailure);
		ConnectionFinished = true;
	}

	private void onGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		if (active)
		{
			if (pCallback.m_bActive != 0)
			{
				Game1.paused = !Game1.IsMultiplayer;
			}
			else
			{
				Game1.paused = false;
			}
		}
	}

	public void GetAchievement(string achieve)
	{
		if (active && SteamAPI.IsSteamRunning())
		{
			if (achieve.Equals("0"))
			{
				achieve = "a0";
			}
			try
			{
				SteamUserStats.SetAchievement(achieve);
				SteamUserStats.StoreStats();
			}
			catch (Exception)
			{
			}
		}
	}

	public void ResetAchievements()
	{
		if (active && SteamAPI.IsSteamRunning())
		{
			try
			{
				SteamUserStats.ResetAllStats(bAchievementsToo: true);
			}
			catch (Exception)
			{
			}
		}
	}

	public void Update()
	{
		if (active)
		{
			SteamAPI.RunCallbacks();
			try
			{
				GalaxyInstance.ProcessData();
			}
			catch (Exception)
			{
			}
		}
		Game1.game1.IsMouseVisible = Game1.paused || Game1.options.hardwareCursor;
	}

	public void Shutdown()
	{
		SteamAPI.Shutdown();
	}

	public void DebugInfo()
	{
		if (SteamAPI.IsSteamRunning())
		{
			Game1.debugOutput = (SteamUser.BLoggedOn() ? "steam is running, user logged on" : "steam is running");
			return;
		}
		Game1.debugOutput = "steam is not running";
		SteamAPI.Init();
	}

	public string FilterDirtyWords(string words)
	{
		return words;
	}
}
