namespace StardewValley.Delegates;

/// <summary>A <see cref="T:StardewValley.GameStateQuery" /> query resolver.</summary>
/// <param name="query">The game state query split by space, including the query key.</param>
/// <param name="context">The game state query context.</param>
/// <returns>Returns whether the query matches.</returns>
public delegate bool GameStateQueryDelegate(string[] query, GameStateQueryContext context);
