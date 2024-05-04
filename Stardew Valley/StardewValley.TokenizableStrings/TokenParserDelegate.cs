using System;

namespace StardewValley.TokenizableStrings;

/// <summary>Provides the output for a token within a text parsed by <see cref="T:StardewValley.TokenizableStrings.TokenParser" />.</summary>
/// <param name="query">The full token string split by spaces, including the token name.</param>
/// <param name="replacement">The output string with which to replace the token within the text being parsed.</param>
/// <param name="random">The RNG to use for randomization.</param>
/// <param name="player">The player to use for any player-related checks.</param>
/// <returns>Returns whether the text was handled.</returns>
public delegate bool TokenParserDelegate(string[] query, out string replacement, Random random, Farmer player);
