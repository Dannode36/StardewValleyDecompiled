namespace StardewValley.Hashing;

/// <summary>Combines hash codes in a deterministic way that's consistent between both sessions and players.</summary>
/// <remarks>This avoids <see cref="M:System.String.GetHashCode" /> and <c>HashCode.Combine</c> which are non-deterministic across sessions or players. That's preferable for actual hashing, but it prevents us from using it as deterministic random seeds.</remarks>
public interface IHashUtility
{
	/// <summary>Get a deterministic hash code for a string.</summary>
	/// <param name="value">The string value to hash.</param>
	int GetDeterministicHashCode(string value);

	/// <summary>Get a deterministic hash code for a set of values.</summary>
	/// <param name="values">The values to hash.</param>
	int GetDeterministicHashCode(params int[] values);
}
