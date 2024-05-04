namespace StardewValley.ConsoleAsync;

public interface IAsyncOperation
{
	bool Started { get; }

	bool Done { get; }

	void Begin();

	void Conclude();
}
