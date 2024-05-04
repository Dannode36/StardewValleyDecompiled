using StardewValley.Logging;

namespace StardewValley.Delegates;

/// <summary>Handles a debug command.</summary>
/// <param name="command">The full debug command split by spaces, including the command name.</param>
/// <param name="log">The log to which to write debug command output.</param>
public delegate void DebugCommandHandlerDelegate(string[] command, IGameLogger log);
