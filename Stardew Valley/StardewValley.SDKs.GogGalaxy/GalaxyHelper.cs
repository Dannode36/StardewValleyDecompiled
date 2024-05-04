using System;
using Galaxy.Api;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy;

public class GalaxyHelper : SDKHelper
{
	public const string ClientID = "48767653913349277";

	public const string ClientSecret = "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a";

	/// <summary>The key we use to store the user's custom display name on the Galaxy API.</summary>
	public const string DisplayNameDataKey = "StardewDisplayName";

	public bool active;

	private GalaxyAuthListener authListener;

	private GalaxyOperationalStateChangeListener stateChangeListener;

	private GalaxyNetHelper networking;

	public string Name { get; } = "Galaxy";


	public bool ConnectionFinished { get; private set; }

	public int ConnectionProgress { get; private set; }

	public SDKNetHelper Networking => networking;

	public bool HasOverlay => false;

	public bool IsJapaneseRegionRelease => false;

	public bool IsEnterButtonAssignmentFlipped => false;

	public void EarlyInitialize()
	{
	}

	public void Initialize()
	{
		try
		{
			GalaxyInstance.Init(new InitParams("48767653913349277", "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a"));
			authListener = new GalaxyAuthListener(onGalaxyAuthSuccess, onGalaxyAuthFailure, onGalaxyAuthLost);
			stateChangeListener = new GalaxyOperationalStateChangeListener(onGalaxyStateChange);
			GalaxyInstance.User().SignInGalaxy(requireOnline: true);
			active = true;
			ConnectionProgress++;
		}
		catch (Exception e)
		{
			Game1.log.Error("Error initializing GalaxyHelper.", e);
			ConnectionFinished = true;
		}
	}

	private void onGalaxyStateChange(uint operationalState)
	{
		if (networking == null)
		{
			if ((operationalState & (true ? 1u : 0u)) != 0)
			{
				Game1.log.Verbose("Galaxy signed in");
				ConnectionProgress++;
			}
			if ((operationalState & 2u) != 0)
			{
				Game1.log.Verbose("Galaxy logged on");
				networking = new GalaxyNetHelper();
				ConnectionProgress++;
				ConnectionFinished = true;
			}
		}
	}

	private void onGalaxyAuthSuccess()
	{
		Game1.log.Verbose("Galaxy auth success");
		ConnectionProgress++;
	}

	private void onGalaxyAuthFailure(IAuthListener.FailureReason reason)
	{
		Game1.log.Error("Galaxy auth failure: " + reason);
		ConnectionFinished = true;
	}

	private void onGalaxyAuthLost()
	{
		Game1.log.Error("Galaxy auth lost");
		ConnectionFinished = true;
	}

	public void GetAchievement(string achieve)
	{
	}

	public void ResetAchievements()
	{
		if (active)
		{
			GalaxyInstance.Stats().ResetStatsAndAchievements();
		}
	}

	public void Update()
	{
		if (active)
		{
			GalaxyInstance.ProcessData();
		}
	}

	public void Shutdown()
	{
	}

	public void DebugInfo()
	{
	}

	public string FilterDirtyWords(string words)
	{
		return words;
	}
}
