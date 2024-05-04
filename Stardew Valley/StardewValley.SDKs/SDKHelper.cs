namespace StardewValley.SDKs;

public interface SDKHelper
{
	/// <summary>
	/// This property needs to be initialized to the correct value before Initialize(), so probably within EarlyInitialize().
	/// </summary>
	bool IsEnterButtonAssignmentFlipped { get; }

	/// <summary>
	/// This property needs to be initialized to the correct value before Initialize(), so probably within EarlyInitialize().
	/// </summary>
	bool IsJapaneseRegionRelease { get; }

	string Name { get; }

	SDKNetHelper Networking { get; }

	bool ConnectionFinished { get; }

	int ConnectionProgress { get; }

	bool HasOverlay { get; }

	void EarlyInitialize();

	void Initialize();

	void Update();

	void Shutdown();

	void DebugInfo();

	void GetAchievement(string achieve);

	void ResetAchievements();

	string FilterDirtyWords(string words);
}
