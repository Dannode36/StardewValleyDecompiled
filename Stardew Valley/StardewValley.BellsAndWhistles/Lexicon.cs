using System;
using System.Linq;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;

namespace StardewValley.BellsAndWhistles;

public class Lexicon
{
	/// <summary>
	///
	/// A noun to represent some kind of "bad" object. Kind of has connotations of it being disgusting or cheap. preface with "that" or "such"
	///
	/// </summary>
	/// <returns></returns>
	public static string getRandomNegativeItemSlanderNoun()
	{
		Random random = Utility.CreateDaySaveRandom();
		string[] choices = Game1.content.LoadString("Strings\\Lexicon:RandomNegativeItemNoun").Split('#');
		return random.Choose(choices);
	}

	public static string getProperArticleForWord(string word)
	{
		if (LocalizedContentManager.CurrentLanguageCode != 0)
		{
			return "";
		}
		if (word != null && word.Length > 0)
		{
			switch (word.ToLower()[0])
			{
			case 'a':
			case 'e':
			case 'i':
			case 'o':
			case 'u':
				return "an";
			}
		}
		return "a";
	}

	public static string capitalize(string text)
	{
		if (text == null || text == "" || LocalizedContentManager.CurrentLanguageCode != 0)
		{
			return text;
		}
		int positionOfFirstCapitalizableCharacter = 0;
		for (int i = 0; i < text.Length; i++)
		{
			if (char.IsLetter(text[i]))
			{
				positionOfFirstCapitalizableCharacter = i;
				break;
			}
		}
		if (positionOfFirstCapitalizableCharacter == 0)
		{
			return text[0].ToString().ToUpper() + text.Substring(1);
		}
		return text.Substring(0, positionOfFirstCapitalizableCharacter) + text[positionOfFirstCapitalizableCharacter].ToString().ToUpper() + text.Substring(positionOfFirstCapitalizableCharacter + 1);
	}

	public static string makePlural(string word, bool ignore = false)
	{
		if (ignore || LocalizedContentManager.CurrentLanguageCode != 0 || word == null)
		{
			return word;
		}
		switch (word)
		{
		case "Dragon Tooth":
			return "Dragon Teeth";
		case "Rice Pudding":
			return "bowls of Rice Pudding";
		case "Algae Soup":
			return "bowls of Algae Soup";
		case "Coal":
			return "lumps of Coal";
		case "Salt":
			return "pieces of Salt";
		case "Jelly":
			return "Jellies";
		case "Wheat":
			return "bushels of Wheat";
		case "Ginger":
			return "pieces of Ginger";
		case "Garlic":
			return "bulbs of Garlic";
		case "Bok Choy":
		case "Broken Glasses":
		case "Bream":
		case "Carp":
		case "Chub":
		case "Clay":
		case "Crab Cakes":
		case "Cranberries":
		case "Dried Cranberries":
		case "Dried Sunflowers":
		case "Driftwood":
		case "Fossilized Ribs":
		case "Ghostfish":
		case "Glass Shards":
		case "Glazed Yams":
		case "Green Canes":
		case "Hashbrowns":
		case "Hops":
		case "Largemouth Bass":
		case "Mixed Seeds":
		case "Pancakes":
		case "Pepper Poppers":
		case "Pickles":
		case "Red Canes":
		case "Roasted Hazelnuts":
		case "Sandfish":
		case "Smallmouth Bass":
		case "Star Shards":
		case "Tea Leaves":
		case "Warp Totem: Mountains":
		case "Weeds":
			return word;
		default:
			switch (word.Last())
			{
			case 'y':
				return word.Substring(0, word.Length - 1) + "ies";
			case 's':
				if (!word.EndsWith(" Seeds") && !word.EndsWith(" Shorts") && !word.EndsWith(" Bass") && !word.EndsWith(" Flowers") && !word.EndsWith(" Peach"))
				{
					return word + "es";
				}
				return word;
			case 'x':
			case 'z':
				return word + "es";
			default:
				if (word.Length > 2)
				{
					string ending = word.Substring(word.Length - 2);
					if (ending == "sh" || ending == "ch")
					{
						return word + "es";
					}
				}
				return word + "s";
			}
		}
	}

	/// <summary>In English only, prepend an article like 'a' or 'an' to a word.</summary>
	/// <param name="word">The word for which to prepend an article.</param>
	public static string prependArticle(string word)
	{
		if (LocalizedContentManager.CurrentLanguageCode != 0)
		{
			return word;
		}
		return getProperArticleForWord(word) + " " + word;
	}

	/// <summary>In English only, prepend an article like 'a' or 'an' to a word as a <see cref="T:StardewValley.TokenizableStrings.TokenParser">tokenizable string</see>.</summary>
	/// <param name="word">The tokenizable string which returns the word for which to prepend an article.</param>
	public static string prependTokenizedArticle(string word)
	{
		if (LocalizedContentManager.CurrentLanguageCode != 0)
		{
			return word;
		}
		return TokenStringBuilder.ArticleFor(word) + " " + word;
	}

	/// <summary>
	///
	/// Adjectives like "wonderful" "amazing" "excellent", prefaced with "are"  "is"  "was" "will be" "usually is", etc.
	/// these wouldn't really make sense for an object, more for a person,place, or event
	/// </summary>
	/// <returns></returns>
	public static string getRandomPositiveAdjectiveForEventOrPerson(NPC n = null)
	{
		Random r = Utility.CreateDaySaveRandom();
		string[] choices = ((n != null && n.Age != 0) ? Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_Child").Split('#') : (n?.Gender switch
		{
			Gender.Male => Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_AdultMale").Split('#'), 
			Gender.Female => Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_AdultFemale").Split('#'), 
			_ => Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_PlaceOrEvent").Split('#'), 
		}));
		return r.Choose(choices);
	}

	/// <summary>
	///
	/// An adjective to represent something tasty, like "delicious", "tasty", "wonderful", "satisfying"
	///
	/// </summary>
	/// <returns></returns>
	public static string getRandomDeliciousAdjective(NPC n = null)
	{
		Random random = Utility.CreateDaySaveRandom();
		string[] choices = ((n == null || n.Age != 2) ? Game1.content.LoadString("Strings\\Lexicon:RandomDeliciousAdjective").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomDeliciousAdjective_Child").Split('#'));
		return random.Choose(choices);
	}

	/// <summary>
	///
	/// Adjective to describe something that is not tasty. "gross", "disgusting", "nasty"
	/// </summary>
	/// <returns></returns>
	public static string getRandomNegativeFoodAdjective(NPC n = null)
	{
		Random random = Utility.CreateDaySaveRandom();
		string[] choices = ((n != null && n.Age == 2) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective_Child").Split('#') : ((n == null || n.Manners != 1) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective_Polite").Split('#')));
		return random.Choose(choices);
	}

	/// <summary>
	///
	/// Adjectives like "decent" "good"
	/// </summary>
	/// <returns></returns>
	public static string getRandomSlightlyPositiveAdjectiveForEdibleNoun(NPC n = null)
	{
		Random random = Utility.CreateDaySaveRandom();
		string[] choices = Game1.content.LoadString("Strings\\Lexicon:RandomSlightlyPositiveFoodAdjective").Split('#');
		return random.Choose(choices);
	}

	/// <summary>Get a generic term for a child of a given gender (i.e. "boy" or "girl").</summary>
	/// <param name="isMale">Whether the child is male.</param>
	public static string getGenderedChildTerm(bool isMale)
	{
		return Game1.content.LoadString(isMale ? "Strings\\Lexicon:ChildTerm_Male" : "Strings\\Lexicon:ChildTerm_Female");
	}

	/// <summary>Get a generic term for a child of a given gender (i.e. "boy" or "girl"), as a <see cref="T:StardewValley.TokenizableStrings.TokenParser">tokenizable string</see>.</summary>
	/// <param name="isMale">Whether the child is male.</param>
	public static string getTokenizedGenderedChildTerm(bool isMale)
	{
		return TokenStringBuilder.LocalizedText(isMale ? "Strings\\Lexicon:ChildTerm_Male" : "Strings\\Lexicon:ChildTerm_Female");
	}

	/// <summary>Get a gendered pronoun (i.e. "him" or "her").</summary>
	/// <param name="isMale">Whether to get a male pronoun.</param>
	public static string getPronoun(bool isMale)
	{
		return Game1.content.LoadString(isMale ? "Strings\\Lexicon:Pronoun_Male" : "Strings\\Lexicon:Pronoun_Female");
	}

	/// <summary>Get a gendered pronoun (i.e. "him" or "her"), as a <see cref="T:StardewValley.TokenizableStrings.TokenParser">tokenizable string</see>.</summary>
	/// <param name="isMale">Whether to get a male pronoun.</param>
	public static string getTokenizedPronoun(bool isMale)
	{
		return TokenStringBuilder.LocalizedText(isMale ? "Strings\\Lexicon:Pronoun_Male" : "Strings\\Lexicon:Pronoun_Female");
	}

	/// <summary>Get a possessive gendered pronoun (i.e. "his" or "her").</summary>
	/// <param name="isMale">Whether to get a male pronoun.</param>
	public static string getPossessivePronoun(bool isMale)
	{
		return Game1.content.LoadString(isMale ? "Strings\\Lexicon:Possessive_Pronoun_Male" : "Strings\\Lexicon:Possessive_Pronoun_Female");
	}

	/// <summary>Get a possessive gendered pronoun (i.e. "his" or "her"), as a <see cref="T:StardewValley.TokenizableStrings.TokenParser">tokenizable string</see>.</summary>
	/// <param name="isMale">Whether to get a male pronoun.</param>
	public static string getTokenizedPossessivePronoun(bool isMale)
	{
		return TokenStringBuilder.LocalizedText(isMale ? "Strings\\Lexicon:Possessive_Pronoun_Male" : "Strings\\Lexicon:Possessive_Pronoun_Female");
	}
}
