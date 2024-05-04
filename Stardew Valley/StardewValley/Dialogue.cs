using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace StardewValley;

public class Dialogue
{
	public delegate bool onAnswerQuestion(int whichResponse);

	public const string dialogueHappy = "$h";

	public const string dialogueSad = "$s";

	public const string dialogueUnique = "$u";

	public const string dialogueNeutral = "$neutral";

	public const string dialogueLove = "$l";

	public const string dialogueAngry = "$a";

	public const string dialogueEnd = "$e";

	/// <summary>The character which begins a command name.</summary>
	public const char dialogueCommandPrefix = '$';

	/// <summary>A dialogue code which splits the subsequent text into a separate dialogue box shown after the player clicks.</summary>
	public const string dialogueBreak = "$b";

	/// <summary>Equivalent to <see cref="F:StardewValley.Dialogue.dialogueBreak" />, but wrapped with command delimiters so it can be added directly to dialogue text.</summary>
	public const string dialogueBreakDelimited = "#$b#";

	public const string multipleDialogueDelineator = "||";

	public const string dialogueKill = "$k";

	public const string dialogueChance = "$c";

	public const string dialogueDependingOnWorldState = "$d";

	public const string dialogueEvent = "$v";

	public const string dialogueQuickResponse = "$y";

	public const string dialoguePrerequisite = "$p";

	public const string dialogueSingle = "$1";

	/// <summary>A command which toggles between two dialogues depending on the result of a game state query.</summary>
	public const string dialogueGameStateQuery = "$query";

	/// <summary>A command which switches between gendered text based on the player gender.</summary>
	public const string dialogueGenderSwitch_startBlock = "${";

	/// <summary>The end token for a <see cref="F:StardewValley.Dialogue.dialogueGenderSwitch_startBlock" /> command.</summary>
	public const string dialogueGenderSwitch_endBlock = "}$";

	/// <summary>A command which runs an action.</summary>
	public const string dialogueRunAction = "$action";

	/// <summary>A command which begins a conversation topic.</summary>
	public const string dialogueStartConversationTopic = "$t";

	/// <summary>A command which begins a question.</summary>
	public const string dialogueQuestion = "$q";

	/// <summary>A command which starts an inquiry initiated by the player or an answer to an NPC's question.</summary>
	public const string dialogueResponse = "$r";

	/// <summary>A special character added to dialogues to signify that they are part of a broken up series of dialogues.</summary>
	public const string breakSpecialCharacter = "{";

	public const string playerNameSpecialCharacter = "@";

	public const char genderDialogueSplitCharacter = '^';

	public const char genderDialogueSplitCharacter2 = '¦';

	public const string quickResponseDelineator = "*";

	public const string randomAdjectiveSpecialCharacter = "%adj";

	public const string randomNounSpecialCharacter = "%noun";

	public const string randomPlaceSpecialCharacter = "%place";

	public const string spouseSpecialCharacter = "%spouse";

	public const string randomNameSpecialCharacter = "%name";

	public const string firstNameLettersSpecialCharacter = "%firstnameletter";

	public const string timeSpecialCharacter = "%time";

	public const string bandNameSpecialCharacter = "%band";

	public const string bookNameSpecialCharacter = "%book";

	public const string petSpecialCharacter = "%pet";

	public const string farmNameSpecialCharacter = "%farm";

	public const string favoriteThingSpecialCharacter = "%favorite";

	public const string eventForkSpecialCharacter = "%fork";

	public const string yearSpecialCharacter = "%year";

	public const string kid1specialCharacter = "%kid1";

	public const string kid2SpecialCharacter = "%kid2";

	public const string revealTasteCharacter = "%revealtaste";

	public const string seasonCharacter = "%season";

	public const string dontfacefarmer = "%noturn";

	/// <summary>A prefix added to a dialogue line to indicate it should be drawn as a small dialogue box with no portrait.</summary>
	/// <remarks>This is only applied if it's not part of another token like <c>%year</c>.</remarks>
	public const char noPortraitPrefix = '%';

	/// <summary>The tokens like <see cref="F:StardewValley.Dialogue.spouseSpecialCharacter" /> which begin with a <c>%</c> symbol.</summary>
	public static readonly string[] percentTokens = new string[18]
	{
		"%adj", "%noun", "%place", "%spouse", "%name", "%firstnameletter", "%time", "%band", "%book", "%pet",
		"%farm", "%favorite", "%fork", "%year", "%kid1", "%kid2", "%revealtaste", "%season"
	};

	private static bool nameArraysTranslated = false;

	public static string[] adjectives = new string[20]
	{
		"Purple", "Gooey", "Chalky", "Green", "Plush", "Chunky", "Gigantic", "Greasy", "Gloomy", "Practical",
		"Lanky", "Dopey", "Crusty", "Fantastic", "Rubbery", "Silly", "Courageous", "Reasonable", "Lonely", "Bitter"
	};

	public static string[] nouns = new string[23]
	{
		"Dragon", "Buffet", "Biscuit", "Robot", "Planet", "Pepper", "Tomb", "Hyena", "Lip", "Quail",
		"Cheese", "Disaster", "Raincoat", "Shoe", "Castle", "Elf", "Pump", "Chip", "Wig", "Mermaid",
		"Drumstick", "Puppet", "Submarine"
	};

	public static string[] verbs = new string[13]
	{
		"ran", "danced", "spoke", "galloped", "ate", "floated", "stood", "flowed", "smelled", "swam",
		"grilled", "cracked", "melted"
	};

	public static string[] positional = new string[13]
	{
		"atop", "near", "with", "alongside", "away from", "too close to", "dangerously close to", "far, far away from", "uncomfortably close to", "way above the",
		"miles below", "on a different planet from", "in a different century than"
	};

	public static string[] places = new string[12]
	{
		"Castle Village", "Basket Town", "Pine Mesa City", "Point Drake", "Minister Valley", "Grampleton", "Zuzu City", "a small island off the coast", "Fort Josa", "Chestervale",
		"Fern Islands", "Tanker Grove"
	};

	public static string[] colors = new string[16]
	{
		"/crimson", "/green", "/tan", "/purple", "/deep blue", "/neon pink", "/pale/yellow", "/chocolate/brown", "/sky/blue", "/bubblegum/pink",
		"/blood/red", "/bright/orange", "/aquamarine", "/silvery", "/glimmering/gold", "/rainbow"
	};

	/// <summary>The dialogues to show in their own message boxes, and/or actions to perform when selected.</summary>
	public List<DialogueLine> dialogues = new List<DialogueLine>();

	/// <summary>The <see cref="F:StardewValley.Dialogue.currentDialogueIndex" /> values for which to disable the portrait due to <see cref="F:StardewValley.Dialogue.noPortraitPrefix" />.</summary>
	public HashSet<int> indexesWithoutPortrait = new HashSet<int>();

	/// <summary>The responses which the player can choose from, if any.</summary>
	private List<NPCDialogueResponse> playerResponses;

	private List<string> quickResponses;

	private bool isLastDialogueInteractive;

	private bool quickResponse;

	public bool isCurrentStringContinuedOnNextScreen;

	private bool finishedLastDialogue;

	public bool showPortrait;

	public bool removeOnNextMove;

	public bool dontFaceFarmer;

	public string temporaryDialogueKey;

	public int currentDialogueIndex;

	/// <summary>The backing field for <see cref="P:StardewValley.Dialogue.CurrentEmotion" />.</summary>
	/// <remarks>Most code shouldn't use this directly.</remarks>
	private string currentEmotion;

	public NPC speaker;

	public onAnswerQuestion answerQuestionBehavior;

	public Texture2D overridePortrait;

	public Action onFinish;

	/// <summary>The translation key from which the <see cref="F:StardewValley.Dialogue.dialogues" /> were taken, if known, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>. This is informational only, and has no effect on the dialogue text. The displayed text may not match the translation text exactly (e.g. due to token substitutions or dialogue parsing).</summary>
	public readonly string TranslationKey;

	/// <summary>The portrait command for the current dialogue, usually matching a constant like <see cref="F:StardewValley.Dialogue.dialogueHappy" /> or a numeric index like <c>$1</c>.</summary>
	public string CurrentEmotion
	{
		get
		{
			return currentEmotion ?? "$neutral";
		}
		set
		{
			currentEmotion = value;
		}
	}

	/// <summary>Whether the <see cref="P:StardewValley.Dialogue.CurrentEmotion" /> was set explicitly (e.g. via a dialogue command like <see cref="F:StardewValley.Dialogue.dialogueNeutral" />), instead of being the default value.</summary>
	public bool CurrentEmotionSetExplicitly => currentEmotion != null;

	public Farmer farmer
	{
		get
		{
			if (Game1.CurrentEvent != null)
			{
				return Game1.CurrentEvent.farmer;
			}
			return Game1.player;
		}
	}

	private static void TranslateArraysOfStrings()
	{
		colors = new string[16]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.795"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.796"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.797"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.798"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.799"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.800"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.801"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.802"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.803"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.804"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.805"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.806"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.807"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.808"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.809"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.810")
		};
		adjectives = new string[20]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.679"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.680"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.681"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.682"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.683"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.684"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.685"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.686"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.687"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.688"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.689"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.690"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.691"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.692"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.693"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.694"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.695"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.696"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.697"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.698")
		};
		nouns = new string[23]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.699"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.700"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.701"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.702"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.703"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.704"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.705"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.706"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.707"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.708"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.709"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.710"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.711"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.712"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.713"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.714"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.715"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.716"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.717"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.718"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.719"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.720"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.721")
		};
		verbs = new string[13]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.722"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.723"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.724"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.725"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.726"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.727"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.728"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.729"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.730"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.731"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.732"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.733"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.734")
		};
		positional = new string[13]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.735"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.736"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.737"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.738"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.739"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.740"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.741"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.742"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.743"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.744"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.745"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.746"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.747")
		};
		places = new string[12]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.748"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.749"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.750"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.751"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.752"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.753"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.754"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.755"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.756"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.757"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.758"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.759")
		};
		nameArraysTranslated = true;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which the <paramref name="dialogueText" /> was taken, if known, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>. This is informational only, and has no effect on the dialogue text.</param>
	/// <param name="dialogueText">The literal dialogue text to display.</param>
	/// <remarks>This constructor allows setting literal text. To use a translation as-is, see the other constructor.</remarks>
	public Dialogue(NPC speaker, string translationKey, string dialogueText)
	{
		if (!nameArraysTranslated)
		{
			TranslateArraysOfStrings();
		}
		this.speaker = speaker;
		TranslationKey = translationKey;
		try
		{
			parseDialogueString(dialogueText, translationKey);
			checkForSpecialDialogueAttributes();
		}
		catch (Exception ex)
		{
			Game1.log.Error($"Failed parsing dialogue string for NPC {speaker?.Name} (key: {translationKey}, text: {dialogueText}).", ex);
			parseDialogueString("...", null);
			checkForSpecialDialogueAttributes();
		}
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="isGendered">Whether the <paramref name="translationKey" /> matches a gendered translation.</param>
	/// <remarks>This matches the most common convention, i.e. a translation with no format placeholders. For more advanced cases, see <c>FromTranslation</c> or the constructor which takes a <c>dialogueText</c> parameter.</remarks>
	public Dialogue(NPC speaker, string translationKey, bool isGendered = false)
		: this(speaker, translationKey, isGendered ? Game1.LoadStringByGender(speaker.Gender, translationKey) : Game1.content.LoadString(translationKey))
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="other">The data to copy.</param>
	public Dialogue(Dialogue other)
	{
		foreach (DialogueLine line in other.dialogues)
		{
			dialogues.Add(new DialogueLine(line.Text, line.SideEffects));
		}
		indexesWithoutPortrait = new HashSet<int>(other.indexesWithoutPortrait);
		if (other.playerResponses != null)
		{
			playerResponses = new List<NPCDialogueResponse>();
			foreach (NPCDialogueResponse response in other.playerResponses)
			{
				playerResponses.Add(new NPCDialogueResponse(response));
			}
		}
		if (other.quickResponses != null)
		{
			quickResponses = new List<string>(other.quickResponses);
		}
		isLastDialogueInteractive = other.isLastDialogueInteractive;
		quickResponse = other.quickResponse;
		isCurrentStringContinuedOnNextScreen = other.isCurrentStringContinuedOnNextScreen;
		finishedLastDialogue = other.finishedLastDialogue;
		showPortrait = other.showPortrait;
		removeOnNextMove = other.removeOnNextMove;
		dontFaceFarmer = other.dontFaceFarmer;
		temporaryDialogueKey = other.temporaryDialogueKey;
		currentDialogueIndex = other.currentDialogueIndex;
		currentEmotion = other.currentEmotion;
		speaker = other.speaker;
		answerQuestionBehavior = other.answerQuestionBehavior;
		overridePortrait = other.overridePortrait;
		onFinish = other.onFinish;
		TranslationKey = other.TranslationKey;
	}

	/// <summary>Get a dialogue instance if the given translation key exists.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	public static Dialogue TryGetDialogue(NPC speaker, string translationKey)
	{
		string text = Game1.content.LoadStringReturnNullIfNotFound(translationKey);
		if (text == null)
		{
			return null;
		}
		return new Dialogue(speaker, translationKey, text);
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey)
	{
		return new Dialogue(speaker, translationKey);
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="sub1">The value with which to replace the <c>{0}</c> placeholder in the loaded text.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey, object sub1)
	{
		return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, sub1));
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="sub1">The value with which to replace the <c>{0}</c> placeholder in the loaded text.</param>
	/// <param name="sub2">The value with which to replace the <c>{1}</c> placeholder in the loaded text.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey, object sub1, object sub2)
	{
		return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, sub1, sub2));
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="sub1">The value with which to replace the <c>{0}</c> placeholder in the loaded text.</param>
	/// <param name="sub2">The value with which to replace the <c>{1}</c> placeholder in the loaded text.</param>
	/// <param name="sub3">The value with which to replace the <c>{2}</c> placeholder in the loaded text.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey, object sub1, object sub2, object sub3)
	{
		return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, sub1, sub2, sub3));
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="substitutions">The values with which to replace placeholders like <c>{0}</c> in the loaded text.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey, params object[] substitutions)
	{
		return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, substitutions));
	}

	public static string getRandomVerb()
	{
		if (!nameArraysTranslated)
		{
			TranslateArraysOfStrings();
		}
		return Game1.random.Choose(verbs);
	}

	public static string getRandomAdjective()
	{
		if (!nameArraysTranslated)
		{
			TranslateArraysOfStrings();
		}
		return Game1.random.Choose(adjectives);
	}

	public static string getRandomNoun()
	{
		if (!nameArraysTranslated)
		{
			TranslateArraysOfStrings();
		}
		return Game1.random.Choose(nouns);
	}

	public static string getRandomPositional()
	{
		if (!nameArraysTranslated)
		{
			TranslateArraysOfStrings();
		}
		return Game1.random.Choose(positional);
	}

	public int getPortraitIndex()
	{
		if (speaker != null && Game1.isGreenRain && speaker.Name.Equals("Demetrius") && Game1.year == 1)
		{
			return 7;
		}
		switch (CurrentEmotion)
		{
		case "$neutral":
			return 0;
		case "$h":
			return 1;
		case "$s":
			return 2;
		case "$u":
			return 3;
		case "$l":
			return 4;
		case "$a":
			return 5;
		default:
		{
			if (!int.TryParse(CurrentEmotion.Substring(1), out var index))
			{
				return 0;
			}
			return index;
		}
		}
	}

	/// <summary>Parse raw dialogue text.</summary>
	/// <param name="masterString">The raw dialogue text to parse.</param>
	/// <param name="translationKey">The translation key from which the dialogue was loaded, if known.</param>
	protected virtual void parseDialogueString(string masterString, string translationKey)
	{
		masterString = TokenParser.ParseText(masterString ?? "...");
		string[] multipleWeeklyDialogueSplit = masterString.Split("||");
		if (multipleWeeklyDialogueSplit.Length > 1)
		{
			masterString = multipleWeeklyDialogueSplit[Game1.stats.DaysPlayed / 7 % multipleWeeklyDialogueSplit.Length];
		}
		playerResponses?.Clear();
		string[] masterDialogueSplit = masterString.Split('#');
		for (int i = 0; i < masterDialogueSplit.Length; i++)
		{
			string curDialogue = masterDialogueSplit[i];
			if (curDialogue.Length < 2)
			{
				continue;
			}
			curDialogue = (masterDialogueSplit[i] = checkForSpecialCharacters(curDialogue));
			bool handledCommand = false;
			if (curDialogue.StartsWith('$'))
			{
				string[] parts = ArgUtility.SplitBySpace(curDialogue, 2);
				string commandToken = parts[0];
				string commandArgs = ArgUtility.Get(parts, 1);
				handledCommand = true;
				switch (commandToken)
				{
				case "$b":
					if (dialogues.Count > 0)
					{
						dialogues[dialogues.Count - 1].Text += "{";
					}
					break;
				case "$1":
				{
					string messageId = ArgUtility.SplitBySpaceAndGet(commandArgs, 0);
					if (messageId != null)
					{
						if (farmer.mailReceived.Contains(messageId))
						{
							i += 3;
							if (i < masterDialogueSplit.Length)
							{
								masterDialogueSplit[i] = checkForSpecialCharacters(masterDialogueSplit[i]);
								dialogues.Add(new DialogueLine(masterDialogueSplit[i]));
							}
						}
						else
						{
							masterDialogueSplit[i + 1] = checkForSpecialCharacters(masterDialogueSplit[i + 1]);
							dialogues.Add(new DialogueLine(messageId + "}" + masterDialogueSplit[i + 1]));
							i = 99999;
						}
						break;
					}
					goto default;
				}
				case "$c":
				{
					string rawChance = ArgUtility.SplitBySpaceAndGet(commandArgs, 0);
					if (rawChance != null)
					{
						double chance = Convert.ToDouble(rawChance);
						if (!Game1.random.NextBool(chance))
						{
							i++;
							break;
						}
						dialogues.Add(new DialogueLine(masterDialogueSplit[i + 1]));
						i += 3;
						break;
					}
					goto default;
				}
				case "$action":
					dialogues.Add(new DialogueLine("", delegate
					{
						if (!TriggerActionManager.TryRunAction(commandArgs, out var error, out var exception))
						{
							error = $"Failed to parse {"$action"} token for {translationKey ?? speaker?.Name ?? ("\"" + masterString + "\"")}: {error}.";
							if (exception == null)
							{
								Game1.log.Warn(error);
							}
							else
							{
								Game1.log.Error(error, exception);
							}
						}
					}));
					break;
				case "$t":
					dialogues.Add(new DialogueLine("", delegate
					{
						string[] array2 = ArgUtility.SplitBySpace(commandArgs);
						if (!ArgUtility.TryGet(array2, 0, out var value, out var error2, allowBlank: false) || !ArgUtility.TryGetOptionalInt(array2, 1, out var value2, out error2, 4))
						{
							Game1.log.Warn($"Failed to parse {"$t"} token for {translationKey ?? speaker?.Name ?? ("\"" + masterString + "\"")}: {error2}.");
						}
						else
						{
							Game1.player.activeDialogueEvents.TryAdd(value, value2);
						}
					}));
					break;
				case "$q":
				{
					if (dialogues.Count > 0)
					{
						dialogues[dialogues.Count - 1].Text += "{";
					}
					string[] questionSplit = ArgUtility.SplitBySpace(commandArgs);
					string[] answerIDs = questionSplit[0].Split('/');
					bool alreadySeenAnswer = false;
					for (int j = 0; j < answerIDs.Length; j++)
					{
						if (farmer.DialogueQuestionsAnswered.Contains(answerIDs[j]))
						{
							alreadySeenAnswer = true;
							break;
						}
					}
					if (alreadySeenAnswer && answerIDs[0] != "-1")
					{
						if (!questionSplit[1].Equals("null"))
						{
							masterDialogueSplit = masterDialogueSplit.Take(i).Concat(speaker.Dialogue[questionSplit[1]].Split('#')).ToArray();
							i--;
						}
					}
					else
					{
						isLastDialogueInteractive = true;
					}
					break;
				}
				case "$r":
				{
					string[] responseSplit = ArgUtility.SplitBySpace(commandArgs);
					if (playerResponses == null)
					{
						playerResponses = new List<NPCDialogueResponse>();
					}
					isLastDialogueInteractive = true;
					playerResponses.Add(new NPCDialogueResponse(responseSplit[0], Convert.ToInt32(responseSplit[1]), responseSplit[2], masterDialogueSplit[i + 1]));
					i++;
					break;
				}
				case "$query":
				{
					string queryString = commandArgs;
					string[] dialogueOptions = ArgUtility.Get(masterString.Split('#', 2), 1)?.Split('|') ?? LegacyShims.EmptyArray<string>();
					masterDialogueSplit = (GameStateQuery.CheckConditions(queryString) ? dialogueOptions[0].Split('#') : ArgUtility.Get(dialogueOptions, 1, dialogueOptions[0]).Split('#'));
					i--;
					break;
				}
				case "$p":
				{
					string[] prerequisiteSplit = ArgUtility.SplitBySpace(commandArgs);
					string[] prerequisiteDialogueSplit = masterDialogueSplit[i + 1].Split('|');
					bool choseOne = false;
					for (int j = 0; j < prerequisiteSplit.Length; j++)
					{
						if (farmer.DialogueQuestionsAnswered.Contains(prerequisiteSplit[j]))
						{
							choseOne = true;
							break;
						}
					}
					if (choseOne)
					{
						masterDialogueSplit = prerequisiteDialogueSplit[0].Split('#');
						i = -1;
					}
					else
					{
						masterDialogueSplit[i + 1] = masterDialogueSplit[i + 1].Split('|').Last();
					}
					break;
				}
				case "$d":
				{
					string[] array = ArgUtility.SplitBySpace(commandArgs);
					string prerequisiteDialogue = masterString.Substring(masterString.IndexOf('#') + 1);
					bool worldStateConfirmed = false;
					switch (array[0].ToLower())
					{
					case "joja":
						worldStateConfirmed = Game1.isLocationAccessible("JojaMart");
						break;
					case "cc":
					case "communitycenter":
						worldStateConfirmed = Game1.isLocationAccessible("CommunityCenter");
						break;
					case "bus":
						worldStateConfirmed = Game1.MasterPlayer.mailReceived.Contains("ccVault");
						break;
					case "kent":
						worldStateConfirmed = Game1.year >= 2;
						break;
					}
					char toLookFor = (prerequisiteDialogue.Contains('|') ? '|' : '#');
					masterDialogueSplit = ((!worldStateConfirmed) ? prerequisiteDialogue.Split(toLookFor)[1].Split('#') : prerequisiteDialogue.Split(toLookFor)[0].Split('#'));
					i--;
					break;
				}
				case "$y":
				{
					quickResponse = true;
					isLastDialogueInteractive = true;
					if (quickResponses == null)
					{
						quickResponses = new List<string>();
					}
					if (playerResponses == null)
					{
						playerResponses = new List<NPCDialogueResponse>();
					}
					string raw = curDialogue.Substring(curDialogue.IndexOf('\'') + 1);
					raw = raw.Substring(0, raw.Length - 1);
					string[] rawSplit = raw.Split('_');
					dialogues.Add(new DialogueLine(rawSplit[0]));
					for (int j = 1; j < rawSplit.Length; j += 2)
					{
						playerResponses.Add(new NPCDialogueResponse(null, -1, "quickResponse" + j, Game1.parseText(rawSplit[j])));
						quickResponses.Add(rawSplit[j + 1].Replace("*", "#$b#"));
					}
					break;
				}
				default:
					handledCommand = false;
					break;
				case "$e":
				case "$k":
					break;
				}
			}
			if (!handledCommand)
			{
				curDialogue = applyGenderSwitch(curDialogue);
				dialogues.Add(new DialogueLine(curDialogue));
			}
		}
	}

	public virtual void prepareDialogueForDisplay()
	{
		if (dialogues.Count > 0 && speaker != null && (bool)speaker.shouldWearIslandAttire && Game1.player.friendshipData.TryGetValue(speaker.Name, out var friendship) && friendship.IsDivorced() && CurrentEmotion == "$u")
		{
			CurrentEmotion = "$neutral";
		}
	}

	/// <summary>Parse dialogue commands and tokens in the current dialogue (i.e. the <see cref="F:StardewValley.Dialogue.currentDialogueIndex" /> entry in <see cref="F:StardewValley.Dialogue.dialogues" />).</summary>
	public virtual void prepareCurrentDialogueForDisplay()
	{
		applyAndSkipPlainSideEffects();
		if (dialogues.Count == 0)
		{
			return;
		}
		string currentDialogue = dialogues[currentDialogueIndex].Text;
		currentDialogue = Utility.ParseGiftReveals(currentDialogue);
		showPortrait = true;
		if (currentDialogue.StartsWith("$v"))
		{
			string[] split = ArgUtility.SplitBySpace(currentDialogue);
			string eventId = split[1];
			bool checkPrecondition = true;
			bool checkSeen = true;
			if (split.Length > 2 && split[2] == "false")
			{
				checkPrecondition = false;
			}
			if (split.Length > 3 && split[3] == "false")
			{
				checkSeen = false;
			}
			if (Game1.PlayEvent(eventId, checkPrecondition, checkSeen))
			{
				dialogues.Clear();
				exitCurrentDialogue();
				return;
			}
			exitCurrentDialogue();
			if (!isDialogueFinished())
			{
				prepareCurrentDialogueForDisplay();
			}
			return;
		}
		if (currentDialogue.Contains('}'))
		{
			farmer.mailReceived.Add(currentDialogue.Split('}')[0]);
			currentDialogue = currentDialogue.Substring(currentDialogue.IndexOf("}") + 1);
			currentDialogue = currentDialogue.Replace("$k", "");
		}
		if (currentDialogue.Contains("$k"))
		{
			currentDialogue = currentDialogue.Replace("$k", "");
			dialogues.RemoveRange(currentDialogueIndex + 1, dialogues.Count - 1 - currentDialogueIndex);
			if (currentDialogue.Length < 2)
			{
				finishedLastDialogue = true;
			}
		}
		if (currentDialogue.StartsWith('%'))
		{
			bool isToken = false;
			string[] array = percentTokens;
			foreach (string token in array)
			{
				if (currentDialogue.StartsWith(token))
				{
					isToken = true;
					break;
				}
			}
			if (!isToken)
			{
				indexesWithoutPortrait.Add(currentDialogueIndex);
				showPortrait = false;
				currentDialogue = currentDialogue.Substring(1);
			}
		}
		else if (indexesWithoutPortrait.Contains(currentDialogueIndex))
		{
			showPortrait = false;
		}
		currentDialogue = ReplacePlayerEnteredStrings(currentDialogue);
		if (currentDialogue.Contains('['))
		{
			int open_index = -1;
			do
			{
				open_index = currentDialogue.IndexOf('[', Math.Max(open_index, 0));
				if (open_index < 0)
				{
					continue;
				}
				int close_index = currentDialogue.IndexOf(']', open_index);
				if (close_index < 0)
				{
					break;
				}
				string[] split = ArgUtility.SplitBySpace(currentDialogue.Substring(open_index + 1, close_index - open_index - 1));
				bool fail = false;
				string[] array = split;
				for (int i = 0; i < array.Length; i++)
				{
					if (ItemRegistry.GetData(array[i]) == null)
					{
						fail = true;
						break;
					}
				}
				if (fail)
				{
					open_index++;
					continue;
				}
				Item item = ItemRegistry.Create(Game1.random.Choose(split));
				if (item != null)
				{
					if (farmer.addItemToInventoryBool(item, makeActiveObject: true))
					{
						farmer.showCarrying();
					}
					else
					{
						farmer.addItemByMenuIfNecessary(item, null, forceQueue: true);
					}
				}
				currentDialogue = currentDialogue.Remove(open_index, close_index - open_index + 1);
			}
			while (open_index >= 0 && open_index < currentDialogue.Length);
		}
		currentDialogue = currentDialogue.Replace("%time", Game1.getTimeOfDayString(Game1.timeOfDay));
		bool? flag = speaker?.SpeaksDwarvish();
		if (flag.HasValue && flag.GetValueOrDefault() && !farmer.canUnderstandDwarves)
		{
			currentDialogue = convertToDwarvish(currentDialogue);
		}
		dialogues[currentDialogueIndex].Text = currentDialogue;
	}

	public virtual string getCurrentDialogue()
	{
		if (currentDialogueIndex >= dialogues.Count || finishedLastDialogue)
		{
			return "";
		}
		if (dialogues.Count <= 0)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.792");
		}
		return dialogues[currentDialogueIndex].Text;
	}

	public bool isItemGrabDialogue()
	{
		if (dialogues.Count > 0)
		{
			return dialogues[currentDialogueIndex].Text.Contains('[');
		}
		return false;
	}

	/// <summary>Whether we're currently displaying the last entry in <see cref="F:StardewValley.Dialogue.dialogues" /> which has text to display.</summary>
	public bool isOnFinalDialogue()
	{
		for (int i = currentDialogueIndex + 1; i < dialogues.Count; i++)
		{
			if (dialogues[i].HasText)
			{
				return false;
			}
		}
		return true;
	}

	public bool isDialogueFinished()
	{
		return finishedLastDialogue;
	}

	public string ReplacePlayerEnteredStrings(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		string farmer_name = Utility.FilterUserName(farmer.Name);
		str = str.Replace("@", farmer_name);
		if (str.Contains('%'))
		{
			str = str.Replace("%firstnameletter", farmer_name.Substring(0, Math.Max(0, farmer_name.Length / 2)));
			if (str.Contains("%spouse"))
			{
				if (farmer.spouse != null)
				{
					string spouseName = NPC.GetDisplayName(farmer.spouse);
					str = str.Replace("%spouse", spouseName);
				}
				else
				{
					long? spouseId = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
					if (spouseId.HasValue)
					{
						Farmer spouse = Game1.getFarmerMaybeOffline(spouseId.Value);
						str = str.Replace("%spouse", spouse.Name);
					}
				}
			}
			string farmName = Utility.FilterUserName(farmer.farmName);
			str = str.Replace("%farm", farmName);
			string favoriteThing = Utility.FilterUserName(farmer.favoriteThing);
			str = str.Replace("%favorite", favoriteThing);
			int kids = farmer.getNumberOfChildren();
			str = str.Replace("%kid1", (kids > 0) ? farmer.getChildren()[0].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.793"));
			str = str.Replace("%kid2", (kids > 1) ? farmer.getChildren()[1].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.794"));
			str = str.Replace("%pet", farmer.getPetDisplayName());
		}
		return str;
	}

	public string checkForSpecialCharacters(string str)
	{
		str = applyGenderSwitch(str, altTokenOnly: true);
		if (str.Contains('%'))
		{
			str = str.Replace("%adj", Game1.random.Choose(adjectives).ToLower());
			if (str.Contains("%noun"))
			{
				str = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(nouns)) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(nouns))) : (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(nouns).ToLower()) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(nouns).ToLower())));
			}
			str = str.Replace("%place", Game1.random.Choose(places));
			str = str.Replace("%name", randomName());
			str = str.Replace("%band", Game1.samBandName);
			if (str.Contains("%book"))
			{
				str = str.Replace("%book", Game1.elliottBookName);
			}
			str = str.Replace("%year", Game1.year.ToString() ?? "");
			str = str.Replace("%season", Game1.CurrentSeasonDisplayName);
			if (str.Contains("%fork"))
			{
				str = str.Replace("%fork", "");
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.specialEventVariable1 = true;
				}
			}
		}
		return str;
	}

	/// <summary>Get the gender-appropriate dialogue from a dialogue string which may contain a gender-switch token.</summary>
	/// <param name="str">The dialogue string to parse.</param>
	/// <param name="altTokenOnly">Only apply the <see cref="F:StardewValley.Dialogue.genderDialogueSplitCharacter2" /> token, and ignore <see cref="F:StardewValley.Dialogue.genderDialogueSplitCharacter" />.</param>
	public string applyGenderSwitch(string str, bool altTokenOnly = false)
	{
		return applyGenderSwitch(farmer.Gender, str, altTokenOnly);
	}

	/// <summary>Get the gender-appropriate dialogue from a dialogue string which may contain gender-switch tokens.</summary>
	/// <param name="gender">The gender for which to apply tokens.</param>
	/// <param name="str">The dialogue string to parse.</param>
	/// <param name="altTokenOnly">Only apply the <see cref="F:StardewValley.Dialogue.genderDialogueSplitCharacter2" /> token, and ignore <see cref="F:StardewValley.Dialogue.genderDialogueSplitCharacter" />.</param>
	public static string applyGenderSwitch(Gender gender, string str, bool altTokenOnly = false)
	{
		str = applyGenderSwitchBlocks(gender, str);
		int splitIndex = ((!altTokenOnly) ? str.IndexOf('^') : (-1));
		if (splitIndex == -1)
		{
			splitIndex = str.IndexOf('¦');
		}
		if (splitIndex != -1)
		{
			str = ((gender == Gender.Male) ? str.Substring(0, splitIndex) : str.Substring(splitIndex + 1));
		}
		return str;
	}

	/// <summary>Replace gender-switch blocks like <c>${male^female}$</c> or <c>${male¦female}$</c> in the input string with the gender-appropriate text.</summary>
	/// <param name="gender">The gender for which to apply tokens.</param>
	/// <param name="str">The dialogue string to parse.</param>
	/// <remarks>This should only be called directly in cases where <see cref="M:StardewValley.Dialogue.applyGenderSwitch(StardewValley.Gender,System.String,System.Boolean)" /> isn't applied, since that includes gender-switch blocks.</remarks>
	public static string applyGenderSwitchBlocks(Gender gender, string str)
	{
		int startIndex = 0;
		while (true)
		{
			int index = str.IndexOf("${", startIndex, StringComparison.Ordinal);
			if (index == -1)
			{
				return str;
			}
			int endIndex = str.IndexOf("}$", index, StringComparison.Ordinal);
			if (endIndex == -1)
			{
				break;
			}
			string originalSubstr = str.Substring(index + 2, endIndex - index - 2);
			string[] parts = (originalSubstr.Contains('¦') ? originalSubstr.Split('¦') : originalSubstr.Split('^'));
			string newSubstr = gender switch
			{
				Gender.Male => parts[0], 
				Gender.Female => ArgUtility.Get(parts, 1, parts[0]), 
				_ => ArgUtility.Get(parts, 2, parts[0]), 
			};
			str = str.Substring(0, index) + newSubstr + str.Substring(endIndex + "}$".Length);
			startIndex = index + newSubstr.Length;
		}
		return str;
	}

	/// <summary>If the next dialogue(s) in <see cref="F:StardewValley.Dialogue.dialogues" /> have side-effects without text, apply them and set <see cref="F:StardewValley.Dialogue.currentDialogueIndex" /> to the next dialogue which has text.</summary>
	public void applyAndSkipPlainSideEffects()
	{
		while (currentDialogueIndex < dialogues.Count)
		{
			DialogueLine entry = dialogues[currentDialogueIndex];
			if (!entry.HasText)
			{
				entry.SideEffects?.Invoke();
				currentDialogueIndex++;
				continue;
			}
			break;
		}
	}

	public static string randomName()
	{
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.ja:
		{
			string[] names = new string[38]
			{
				"ローゼン", "ミルド", "ココ", "ナミ", "こころ", "サルコ", "ハンゾー", "クッキー", "ココナツ", "せん",
				"ハル", "ラン", "オサム", "ヨシ", "ソラ", "ホシ", "まこと", "マサ", "ナナ", "リオ",
				"リン", "フジ", "うどん", "ミント", "さくら", "ボンボン", "レオ", "モリ", "コーヒー", "ミルク",
				"マロン", "クルミ", "サムライ", "カミ", "ゴロ", "マル", "チビ", "ユキダマ"
			};
			return new Random().Choose(names);
		}
		case LocalizedContentManager.LanguageCode.zh:
		{
			string[] names = new string[183]
			{
				"雨果", "蛋挞", "小百合", "毛毛", "小雨", "小溪", "精灵", "安琪儿", "小糕", "玫瑰",
				"小黄", "晓雨", "阿江", "铃铛", "马琪", "果粒", "郁金香", "小黑", "雨露", "小江",
				"灵力", "萝拉", "豆豆", "小莲", "斑点", "小雾", "阿川", "丽丹", "玛雅", "阿豆",
				"花花", "琉璃", "滴答", "阿山", "丹麦", "梅西", "橙子", "花儿", "晓璃", "小夕",
				"山大", "咪咪", "卡米", "红豆", "花朵", "洋洋", "太阳", "小岩", "汪汪", "玛利亚",
				"小菜", "花瓣", "阳阳", "小夏", "石头", "阿狗", "邱洁", "苹果", "梨花", "小希",
				"天天", "浪子", "阿猫", "艾薇儿", "雪梨", "桃花", "阿喜", "云朵", "风儿", "狮子",
				"绮丽", "雪莉", "樱花", "小喜", "朵朵", "田田", "小红", "宝娜", "梅子", "小樱",
				"嘻嘻", "云儿", "小草", "小黄", "纳香", "阿梅", "茶花", "哈哈", "芸儿", "东东",
				"小羽", "哈豆", "桃子", "茶叶", "双双", "沫沫", "楠楠", "小爱", "麦当娜", "杏仁",
				"椰子", "小王", "泡泡", "小林", "小灰", "马格", "鱼蛋", "小叶", "小李", "晨晨",
				"小琳", "小慧", "布鲁", "晓梅", "绿叶", "甜豆", "小雪", "晓林", "康康", "安妮",
				"樱桃", "香板", "甜甜", "雪花", "虹儿", "美美", "葡萄", "薇儿", "金豆", "雪玲",
				"瑶瑶", "龙眼", "丁香", "晓云", "雪豆", "琪琪", "麦子", "糖果", "雪丽", "小艺",
				"小麦", "小圆", "雨佳", "小火", "麦茶", "圆圆", "春儿", "火灵", "板子", "黑点",
				"冬冬", "火花", "米粒", "喇叭", "晓秋", "跟屁虫", "米果", "欢欢", "爱心", "松子",
				"丫头", "双子", "豆芽", "小子", "彤彤", "棉花糖", "阿贵", "仙儿", "冰淇淋", "小彬",
				"贤儿", "冰棒", "仔仔", "格子", "水果", "悠悠", "莹莹", "巧克力", "梦洁", "汤圆",
				"静香", "茄子", "珍珠"
			};
			return new Random().Choose(names);
		}
		case LocalizedContentManager.LanguageCode.ru:
		{
			string[] names = new string[50]
			{
				"Августина", "Альф", "Анфиса", "Ариша", "Афоня", "Баламут", "Балкан", "Бандит", "Бланка", "Бобик",
				"Боня", "Борька", "Буренка", "Бусинка", "Вася", "Гаврюша", "Глаша", "Гоша", "Дуня", "Дуся",
				"Зорька", "Ивонна", "Игнат", "Кеша", "Клара", "Кузя", "Лада", "Максимус", "Маня", "Марта",
				"Маруся", "Моня", "Мотя", "Мурзик", "Мурка", "Нафаня", "Ника", "Нюша", "Проша", "Пятнушка",
				"Сеня", "Сивка", "Тихон", "Тоша", "Фунтик", "Шайтан", "Юнона", "Юпитер", "Ягодка", "Яшка"
			};
			return new Random().Choose(names);
		}
		default:
		{
			int nameLength = Game1.random.Next(3, 6);
			string[] startingConsonants = new string[24]
			{
				"B", "Br", "J", "F", "S", "M", "C", "Ch", "L", "P",
				"K", "W", "G", "Z", "Tr", "T", "Gr", "Fr", "Pr", "N",
				"Sn", "R", "Sh", "St"
			};
			string[] consonants = new string[12]
			{
				"ll", "tch", "l", "m", "n", "p", "r", "s", "t", "c",
				"rt", "ts"
			};
			string[] vowels = new string[5] { "a", "e", "i", "o", "u" };
			string[] consonantEndings = new string[5] { "ie", "o", "a", "ers", "ley" };
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
			dictionary["a"] = new string[6] { "nie", "bell", "bo", "boo", "bella", "s" };
			dictionary["e"] = new string[4] { "ll", "llo", "", "o" };
			dictionary["i"] = new string[18]
			{
				"ck", "e", "bo", "ba", "lo", "la", "to", "ta", "no", "na",
				"ni", "a", "o", "zor", "que", "ca", "co", "mi"
			};
			dictionary["o"] = new string[12]
			{
				"nie", "ze", "dy", "da", "o", "ver", "la", "lo", "s", "ny",
				"mo", "ra"
			};
			dictionary["u"] = new string[4] { "rt", "mo", "", "s" };
			Dictionary<string, string[]> endings = dictionary;
			dictionary = new Dictionary<string, string[]>();
			dictionary["a"] = new string[12]
			{
				"nny", "sper", "trina", "bo", "-bell", "boo", "lbert", "sko", "sh", "ck",
				"ishe", "rk"
			};
			dictionary["e"] = new string[9] { "lla", "llo", "rnard", "cardo", "ffe", "ppo", "ppa", "tch", "x" };
			dictionary["i"] = new string[18]
			{
				"llard", "lly", "lbo", "cky", "card", "ne", "nnie", "lbert", "nono", "nano",
				"nana", "ana", "nsy", "msy", "skers", "rdo", "rda", "sh"
			};
			dictionary["o"] = new string[17]
			{
				"nie", "zzy", "do", "na", "la", "la", "ver", "ng", "ngus", "ny",
				"-mo", "llo", "ze", "ra", "ma", "cco", "z"
			};
			dictionary["u"] = new string[11]
			{
				"ssie", "bbie", "ffy", "bba", "rt", "s", "mby", "mbo", "mbus", "ngus",
				"cky"
			};
			Dictionary<string, string[]> endingsForShortNames = dictionary;
			string name = startingConsonants[Game1.random.Next(startingConsonants.Length - 1)];
			for (int i = 1; i < nameLength - 1; i++)
			{
				name = ((i % 2 != 0) ? (name + Game1.random.Choose(vowels)) : (name + Game1.random.Choose(consonants)));
				if (name.Length >= nameLength)
				{
					break;
				}
			}
			string lastLetter = name[name.Length - 1].ToString();
			if (Game1.random.NextBool() && !vowels.Contains(lastLetter))
			{
				name += Game1.random.Choose(consonantEndings);
			}
			else if (vowels.Contains(lastLetter))
			{
				if (Game1.random.NextDouble() < 0.8)
				{
					name = ((name.Length > 3) ? (name + Game1.random.ChooseFrom(endings[lastLetter])) : (name + Game1.random.ChooseFrom(endingsForShortNames[lastLetter])));
				}
			}
			else
			{
				name += Game1.random.Choose(vowels);
			}
			for (int i = name.Length - 1; i > 2; i--)
			{
				if (vowels.Contains(name[i].ToString()) && vowels.Contains(name[i - 2].ToString()))
				{
					switch (name[i - 1])
					{
					case 'c':
						name = name.Substring(0, i) + "k" + name.Substring(i);
						i--;
						break;
					case 'r':
						name = name.Substring(0, i - 1) + "k" + name.Substring(i);
						i--;
						break;
					case 'l':
						name = name.Substring(0, i - 1) + "n" + name.Substring(i);
						i--;
						break;
					}
				}
			}
			if (name.Length <= 3 && Game1.random.NextDouble() < 0.1)
			{
				name = (Game1.random.NextBool() ? (name + name) : (name + "-" + name));
			}
			if (name.Length <= 2 && name.Last() == 'e')
			{
				name += Game1.random.Choose('m', 'p', 'b');
			}
			string lowerName = name.ToLower();
			if (lowerName.Contains("sex") || lowerName.Contains("taboo") || lowerName.Contains("fuck") || lowerName.Contains("rape") || lowerName.Contains("cock") || lowerName.Contains("willy") || lowerName.Contains("cum") || lowerName.Contains("goock") || lowerName.Contains("trann") || lowerName.Contains("gook") || lowerName.Contains("bitch") || lowerName.Contains("shit") || lowerName.Contains("pusie") || lowerName.Contains("kike") || lowerName.Contains("nigg") || lowerName.Contains("puss") || lowerName.Contains("puta") || lowerName.Equals("boner"))
			{
				name = Game1.random.Choose("Bobo", "Wumbus");
			}
			switch (lowerName)
			{
			case "packi":
			case "packie":
				return "Packina";
			case "trananie":
			case "trani":
			case "tranie":
				return "Tranello";
			default:
				return name;
			}
		}
		}
	}

	public virtual string exitCurrentDialogue()
	{
		if (isOnFinalDialogue())
		{
			currentDialogueIndex++;
			applyAndSkipPlainSideEffects();
			onFinish?.Invoke();
		}
		bool num = isCurrentStringContinuedOnNextScreen;
		if (currentDialogueIndex < dialogues.Count - 1)
		{
			currentDialogueIndex++;
			checkForSpecialDialogueAttributes();
		}
		else
		{
			finishedLastDialogue = true;
		}
		if (num)
		{
			return getCurrentDialogue();
		}
		return null;
	}

	private void checkForSpecialDialogueAttributes()
	{
		dontFaceFarmer = false;
		if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Text.Contains("{"))
		{
			dialogues[currentDialogueIndex].Text = dialogues[currentDialogueIndex].Text.Replace("{", "");
			isCurrentStringContinuedOnNextScreen = true;
		}
		else
		{
			isCurrentStringContinuedOnNextScreen = false;
		}
		if (dialogues[currentDialogueIndex].Text.Contains("%noturn"))
		{
			dialogues[currentDialogueIndex].Text = dialogues[currentDialogueIndex].Text.Replace("%noturn", "");
			dontFaceFarmer = true;
		}
		checkEmotions();
	}

	private void checkEmotions()
	{
		CurrentEmotion = null;
		if (dialogues.Count == 0)
		{
			return;
		}
		string dialogue = dialogues[currentDialogueIndex].Text;
		int emoteIndex = dialogue.IndexOf('$');
		if (emoteIndex == -1 || dialogues.Count <= 0)
		{
			return;
		}
		if (dialogue.Contains("$h"))
		{
			CurrentEmotion = "$h";
			dialogues[currentDialogueIndex].Text = dialogue.Replace("$h", "");
			return;
		}
		if (dialogue.Contains("$s"))
		{
			CurrentEmotion = "$s";
			dialogues[currentDialogueIndex].Text = dialogue.Replace("$s", "");
			return;
		}
		if (dialogue.Contains("$u"))
		{
			CurrentEmotion = "$u";
			dialogues[currentDialogueIndex].Text = dialogue.Replace("$u", "");
			return;
		}
		if (dialogue.Contains("$l"))
		{
			CurrentEmotion = "$l";
			dialogues[currentDialogueIndex].Text = dialogue.Replace("$l", "");
			return;
		}
		if (dialogue.Contains("$a"))
		{
			CurrentEmotion = "$a";
			dialogues[currentDialogueIndex].Text = dialogue.Replace("$a", "");
			return;
		}
		int digits = 0;
		for (int i = emoteIndex + 1; i < dialogue.Length && char.IsDigit(dialogue[i]); i++)
		{
			digits++;
		}
		if (digits > 0)
		{
			string emote = (CurrentEmotion = dialogue.Substring(emoteIndex, digits + 1));
			dialogues[currentDialogueIndex].Text = dialogue.Replace(emote, "");
		}
	}

	public List<NPCDialogueResponse> getNPCResponseOptions()
	{
		return playerResponses;
	}

	public Response[] getResponseOptions()
	{
		return playerResponses.Cast<Response>().ToArray();
	}

	public bool isCurrentDialogueAQuestion()
	{
		if (isLastDialogueInteractive)
		{
			return currentDialogueIndex == dialogues.Count - 1;
		}
		return false;
	}

	public virtual bool chooseResponse(Response response)
	{
		for (int i = 0; i < playerResponses.Count; i++)
		{
			if (playerResponses[i].responseKey == null || response.responseKey == null || !playerResponses[i].responseKey.Equals(response.responseKey))
			{
				continue;
			}
			if (answerQuestionBehavior != null)
			{
				if (answerQuestionBehavior(i))
				{
					Game1.currentSpeaker = null;
				}
				isLastDialogueInteractive = false;
				finishedLastDialogue = true;
				answerQuestionBehavior = null;
				return true;
			}
			if (quickResponse)
			{
				isLastDialogueInteractive = false;
				finishedLastDialogue = true;
				isCurrentStringContinuedOnNextScreen = true;
				speaker.setNewDialogue(new Dialogue(speaker, null, quickResponses[i]));
				Game1.drawDialogue(speaker);
				speaker.faceTowardFarmerForPeriod(4000, 3, faceAway: false, farmer);
				return true;
			}
			if (Game1.isFestival())
			{
				Game1.currentLocation.currentEvent.answerDialogueQuestion(speaker, playerResponses[i].responseKey);
				isLastDialogueInteractive = false;
				finishedLastDialogue = true;
				return false;
			}
			farmer.changeFriendship(playerResponses[i].friendshipChange, speaker);
			if (playerResponses[i].id != null)
			{
				farmer.addSeenResponse(playerResponses[i].id);
			}
			if (playerResponses[i].extraArgument != null)
			{
				try
				{
					performDialogueResponseExtraArgument(farmer, playerResponses[i].extraArgument);
				}
				catch (Exception)
				{
				}
			}
			isLastDialogueInteractive = false;
			finishedLastDialogue = false;
			parseDialogueString(speaker.Dialogue[playerResponses[i].responseKey], speaker.LoadedDialogueKey + ":" + playerResponses[i].responseKey);
			isCurrentStringContinuedOnNextScreen = true;
			return false;
		}
		return false;
	}

	public void performDialogueResponseExtraArgument(Farmer farmer, string argument)
	{
		string[] split = argument.Split("_");
		if (split[0].ToLower() == "friend")
		{
			farmer.changeFriendship(Convert.ToInt32(split[2]), Game1.getCharacterFromName(split[1]));
		}
	}

	/// <summary>Convert the current dialogue text into Dwarvish, as spoken by Dwarf when the player doesn't have the Dwarvish Translation Guide.</summary>
	public void convertToDwarvish()
	{
		for (int i = 0; i < dialogues.Count; i++)
		{
			dialogues[i].Text = convertToDwarvish(dialogues[i].Text);
		}
	}

	/// <summary>Convert dialogue text into Dwarvish, as spoken by Dwarf when the player doesn't have the Dwarvish Translation Guide.</summary>
	/// <param name="str">The text to translate.</param>
	public static string convertToDwarvish(string str)
	{
		if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
		{
			string charset1 = "bcdfghjklmnpqrstvwxyz";
			string charset2 = "bcd fghj klmn pqrst vwxy z";
			StringBuilder result = new StringBuilder();
			bool nextCapital = true;
			foreach (char cjk in str)
			{
				int code = cjk;
				if ((19968 <= code && code <= 40959) || (12352 <= code && code <= 12543) || cjk == '々' || (44032 <= code && code <= 55215))
				{
					char char1 = charset1[code % charset1.Length];
					if (nextCapital)
					{
						char1 = char.ToUpper(char1);
						nextCapital = false;
					}
					result.Append(char1);
					char char2 = charset2[(code >> 1) % charset2.Length];
					result.Append(char2);
				}
				else
				{
					result.Append(cjk);
					if (cjk != ' ')
					{
						nextCapital = true;
					}
				}
			}
			return result.ToString();
		}
		StringBuilder translated = new StringBuilder();
		for (int i = 0; i < str.Length; i++)
		{
			switch (str[i])
			{
			case 'a':
				translated.Append('o');
				continue;
			case 'e':
				translated.Append('u');
				continue;
			case 'i':
				translated.Append("e");
				continue;
			case 'o':
				translated.Append('a');
				continue;
			case 'u':
				translated.Append("i");
				continue;
			case 'y':
				translated.Append("ol");
				continue;
			case 'z':
				translated.Append('b');
				continue;
			case 'A':
				translated.Append('O');
				continue;
			case 'E':
				translated.Append('U');
				continue;
			case 'I':
				translated.Append("E");
				continue;
			case 'O':
				translated.Append('A');
				continue;
			case 'U':
				translated.Append("I");
				continue;
			case 'Y':
				translated.Append("Ol");
				continue;
			case 'Z':
				translated.Append('B');
				continue;
			case '1':
				translated.Append('M');
				continue;
			case '5':
				translated.Append('X');
				continue;
			case '9':
				translated.Append('V');
				continue;
			case '0':
				translated.Append('Q');
				continue;
			case 'g':
				translated.Append('l');
				continue;
			case 'c':
				translated.Append('t');
				continue;
			case 't':
				translated.Append('n');
				continue;
			case 'd':
				translated.Append('p');
				continue;
			case ' ':
			case '!':
			case '"':
			case '\'':
			case ',':
			case '.':
			case '?':
			case 'h':
			case 'm':
			case 's':
				translated.Append(str[i]);
				continue;
			case '\n':
			case 'n':
			case 'p':
				continue;
			}
			if (char.IsLetterOrDigit(str[i]))
			{
				translated.Append((char)(str[i] + 2));
			}
		}
		return translated.ToString().Replace("nhu", "doo");
	}
}
