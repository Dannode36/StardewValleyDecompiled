using System.Threading.Tasks;

namespace StardewValley.ConsoleAsync;

public abstract class AsyncTaskOperation : IAsyncOperation
{
	public Task Task;

	public bool TaskStarted;

	bool IAsyncOperation.Started => TaskStarted;

	public abstract bool Done { get; }

	void IAsyncOperation.Begin()
	{
		DebugTools.Assert(!TaskStarted, "AsyncTaskOperation.Begin called but TaskStarted already is true!");
		TaskStarted = true;
		Task.Start();
	}

	public abstract void Conclude();
}
