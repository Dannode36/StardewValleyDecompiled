namespace StardewValley.TokenizableStrings;

/// <summary>Creates tokenized strings in the format recognized by <see cref="T:StardewValley.TokenizableStrings.TokenParser" />.</summary>
public static class TokenStringBuilder
{
	/// <summary>Build a tokenized string which wraps the input in [EscapedText] if it contains spaces.</summary>
	/// <param name="value">The value to escape.</param>
	/// <param name="skipIfNotNeeded">Whether to keep the input as-is if it likely doesn't need to be escaped.</param>
	public static string EscapedText(string value, bool skipIfNotNeeded = true)
	{
		if (!skipIfNotNeeded || (value.IndexOfAny(TokenParser.HeuristicCharactersForEscapableStrings) != -1 && !value.StartsWith("[EscapedText")))
		{
			value = "[EscapedText " + value + "]";
		}
		return value;
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.AchievementName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="achievementId">The achievement ID.</param>
	public static string AchievementName(int achievementId)
	{
		return BuildTokenWithArgumentString("AchievementName", achievementId.ToString());
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.ArticleFor(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="tokenizedString">The translation key containing the asset name and entry key, like <c>Strings\Lexicon:Pronoun_Female</c>.</param>
	public static string ArticleFor(string tokenizedString)
	{
		return BuildTokenWithArgumentString("ArticleFor", tokenizedString);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.ItemName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="itemId">The qualified item ID.</param>
	/// <param name="fallbackText">The text to display if the item doesn't exist; defaults to "Error Item (id)".</param>
	public static string ItemName(string itemId, string fallbackText = null)
	{
		if (fallbackText == null)
		{
			return BuildTokenWithArgumentString("ItemName", itemId);
		}
		return BuildTokenWithArgumentString("ItemName", itemId, fallbackText);
	}

	/// <summary>Build a <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.LocalizedText(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="translationKey">The translation key containing the asset name and entry key, like <c>Strings\Lexicon:Pronoun_Female</c>.</param>
	public static string LocalizedText(string translationKey)
	{
		return BuildTokenWithArgumentString("LocalizedText", translationKey);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.MonsterName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="monsterId">The monster ID.</param>
	/// <param name="fallbackText">The text to display if a localized name isn't found in data; defaults to the monster ID.</param>
	public static string MonsterName(string monsterId, string fallbackText = null)
	{
		if (fallbackText == null)
		{
			return BuildTokenWithArgumentString("MonsterName", monsterId);
		}
		return BuildTokenWithArgumentString("MonsterName", monsterId, fallbackText);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.MovieName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="movieId">The movie ID.</param>
	public static string MovieName(string movieId)
	{
		return BuildTokenWithArgumentString("MovieName", movieId);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.NumberWithSeparators(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="number">The number to format.</param>
	public static string NumberWithSeparators(int number)
	{
		return BuildTokenWithArgumentString("NumberWithSeparators", number.ToString());
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.SpecialOrderName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="orderId">The special order ID.</param>
	public static string SpecialOrderName(string orderId)
	{
		return BuildTokenWithArgumentString("SpecialOrderName", orderId);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.ToolName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="itemId">The qualified tool ID.</param>
	/// <param name="upgradeLevel">The tool upgrade level.</param>
	public static string ToolName(string itemId, int upgradeLevel)
	{
		return BuildTokenWithArgumentString("ToolName", itemId, upgradeLevel.ToString());
	}

	/// <summary>Build a tokenized string in the form <c>[token [EscapedText argument]]</c>.</summary>
	/// <param name="tokenName">The literal token name, like <c>LocalizedText</c>.</param>
	/// <param name="argument">The tokenized string passed as an argument to the token.</param>
	public static string BuildTokenWithArgumentString(string tokenName, string argument)
	{
		return "[" + tokenName + " " + EscapedText(argument) + "]";
	}

	/// <summary>Build a tokenized string in the form <c>[token [EscapedText argument]]</c>.</summary>
	/// <param name="tokenName">The literal token name, like <c>LocalizedText</c>.</param>
	/// <param name="arg1">The tokenized string passed as the first argument to the token.</param>
	/// <param name="arg2">The tokenized string passed as the second argument to the token.</param>
	public static string BuildTokenWithArgumentString(string tokenName, string arg1, string arg2)
	{
		return "[" + tokenName + " " + EscapedText(arg1) + " " + EscapedText(arg2) + "]";
	}
}
