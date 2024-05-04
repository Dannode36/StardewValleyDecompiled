using System;
using System.Collections.Generic;

namespace StardewValley.Extensions;

/// <summary>Provides utility extension methods on framework types.</summary>
public static class RandomExtensions
{
	/// <summary>Randomly choose one of the given options.</summary>
	/// <typeparam name="T">The option type.</typeparam>
	/// <param name="random">The random instance with which to check.</param>
	/// <param name="optionA">The first option, which has a 50% chance of being selected.</param>
	/// <param name="optionB">The second option, which has a 50% chance of being selected.</param>
	public static T Choose<T>(this Random random, T optionA, T optionB)
	{
		if (!(random.NextDouble() < 0.5))
		{
			return optionB;
		}
		return optionA;
	}

	/// <summary>Randomly choose one of the given options.</summary>
	/// <typeparam name="T">The option type.</typeparam>
	/// <param name="random">The random instance with which to check.</param>
	/// <param name="optionA">The first option, which has a 33.3% chance of being selected.</param>
	/// <param name="optionB">The second option, which has a 33.3% chance of being selected.</param>
	/// <param name="optionC">The third option, which has a 33.3% chance of being selected.</param>
	public static T Choose<T>(this Random random, T optionA, T optionB, T optionC)
	{
		return random.Next(3) switch
		{
			0 => optionA, 
			1 => optionB, 
			_ => optionC, 
		};
	}

	/// <summary>Randomly choose one of the given options.</summary>
	/// <typeparam name="T">The option type.</typeparam>
	/// <param name="random">The random instance with which to check.</param>
	/// <param name="optionA">The first option, which has a 25% chance of being selected.</param>
	/// <param name="optionB">The second option, which has a 25% chance of being selected.</param>
	/// <param name="optionC">The third option, which has a 25% chance of being selected.</param>
	/// <param name="optionD">The fourth option, which has a 25% chance of being selected.</param>
	public static T Choose<T>(this Random random, T optionA, T optionB, T optionC, T optionD)
	{
		return random.Next(4) switch
		{
			0 => optionA, 
			1 => optionB, 
			2 => optionC, 
			_ => optionD, 
		};
	}

	/// <summary>Randomly choose one of the given options.</summary>
	/// <typeparam name="T">The option type.</typeparam>
	/// <param name="random">The random instance with which to check.</param>
	/// <param name="options">The options to choose from, which each have an equal chance of being selected.</param>
	/// <returns>Returns a random option, or the default value for <typeparamref name="T" /> if there are none.</returns>
	/// <remarks>This chooses one of the input parameters. To choose from an existing list, see <see cref="M:StardewValley.Extensions.RandomExtensions.ChooseFrom``1(System.Random,System.Collections.Generic.IList{``0})" /> instead.</remarks>
	public static T Choose<T>(this Random random, params T[] options)
	{
		if (options == null || options.Length == 0)
		{
			return default(T);
		}
		return options[random.Next(options.Length)];
	}

	/// <summary>Randomly choose an option from a list.</summary>
	/// <typeparam name="T">The option type.</typeparam>
	/// <param name="random">The random instance with which to check.</param>
	/// <param name="options">The options to choose from, which each have an equal chance of being selected.</param>
	/// <returns>Returns a random option, or the default value for <typeparamref name="T" /> if there are none.</returns>
	public static T ChooseFrom<T>(this Random random, IList<T> options)
	{
		if (options == null || options.Count <= 0)
		{
			return default(T);
		}
		return options[random.Next(options.Count)];
	}

	/// <summary>Get a random boolean value (i.e. a 50% chance).</summary>
	/// <param name="random">The random instance with which to check.</param>
	public static bool NextBool(this Random random)
	{
		return random.NextDouble() < 0.5;
	}

	/// <summary>Get a random boolean value with a weighted chance.</summary>
	/// <param name="random">The random instance with which to check.</param>
	/// <param name="chance">The probability of returning true, as a value between 0 (never) and 1 (always).</param>
	public static bool NextBool(this Random random, double chance)
	{
		if (!(chance >= 1.0))
		{
			return random.NextDouble() < chance;
		}
		return true;
	}

	/// <inheritdoc cref="M:StardewValley.Extensions.RandomExtensions.NextBool(System.Random,System.Double)" />
	public static bool NextBool(this Random random, float chance)
	{
		if (!(chance >= 1f))
		{
			return random.NextDouble() < (double)chance;
		}
		return true;
	}
}
