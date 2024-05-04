using System;
using System.Collections.Generic;
using System.IO;

namespace StardewValley;

public class NetLogger
{
	private Dictionary<string, NetLogRecord> loggedWrites = new Dictionary<string, NetLogRecord>();

	private DateTime timeLastStarted;

	private double priorMillis;

	private bool isLogging;

	public bool IsLogging
	{
		get
		{
			return isLogging;
		}
		set
		{
			if (value != isLogging)
			{
				isLogging = value;
				if (isLogging)
				{
					timeLastStarted = DateTime.UtcNow;
				}
				else
				{
					priorMillis += (DateTime.UtcNow - timeLastStarted).TotalMilliseconds;
				}
			}
		}
	}

	public double LogDuration
	{
		get
		{
			if (isLogging)
			{
				return priorMillis + (DateTime.UtcNow - timeLastStarted).TotalMilliseconds;
			}
			return priorMillis;
		}
	}

	public void LogWrite(string path, long length)
	{
		if (IsLogging)
		{
			loggedWrites.TryGetValue(path, out var record);
			record.Path = path;
			record.Count++;
			record.Bytes += length;
			loggedWrites[path] = record;
		}
	}

	public void Clear()
	{
		loggedWrites.Clear();
		priorMillis = 0.0;
		timeLastStarted = DateTime.UtcNow;
	}

	public string Dump()
	{
		string path = Path.Combine(Program.GetLocalAppDataFolder("Profiling"), DateTime.UtcNow.Ticks + ".csv");
		using StreamWriter writer = File.CreateText(path);
		double duration = LogDuration / 1000.0;
		writer.WriteLine("Profile Duration: {0:F2}", duration);
		writer.WriteLine("Stack,Deltas,Bytes,Deltas/s,Bytes/s,Bytes/Delta");
		foreach (NetLogRecord record in loggedWrites.Values)
		{
			writer.WriteLine("{0:F2},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2}", record.Path, record.Count, record.Bytes, (double)record.Count / duration, (double)record.Bytes / duration, (double)record.Bytes / (double)record.Count);
		}
		return path;
	}
}
