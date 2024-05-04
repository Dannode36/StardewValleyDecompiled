using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StardewValley.ConsoleAsync;

public class AsyncOperationManager
{
	private static AsyncOperationManager _instance;

	private List<IAsyncOperation> _pendingOps;

	private List<IAsyncOperation> _tempOps;

	private List<IAsyncOperation> _doneOps;

	public static AsyncOperationManager Use => _instance;

	public static void Init()
	{
		_instance = new AsyncOperationManager();
	}

	private AsyncOperationManager()
	{
		_pendingOps = new List<IAsyncOperation>();
		_tempOps = new List<IAsyncOperation>();
		_doneOps = new List<IAsyncOperation>();
	}

	public void AddPending(Task task, Action<GenericResult> doneAction)
	{
		GenericOp op = new GenericOp();
		op.DoneCallback = OnDone;
		op.Task = task;
		if (task.Status > TaskStatus.Created)
		{
			op.TaskStarted = true;
		}
		AddPending(op);
		void OnDone()
		{
			GenericResult res = default(GenericResult);
			res.Ex = op.Task.Exception;
			if (res.Ex != null)
			{
				res.Ex = res.Ex.GetBaseException();
			}
			res.Failed = res.Ex != null;
			res.Success = res.Ex == null;
			doneAction(res);
		}
	}

	public void AddPending(Action workAction, Action<GenericResult> doneAction)
	{
		GenericOp op = new GenericOp();
		op.DoneCallback = OnDone;
		Task task = new Task(workAction);
		op.Task = task;
		AddPending(op);
		void OnDone()
		{
			GenericResult res = default(GenericResult);
			res.Ex = op.Task.Exception;
			if (res.Ex != null)
			{
				res.Ex = res.Ex.GetBaseException();
			}
			res.Failed = res.Ex != null;
			res.Success = res.Ex == null;
			doneAction(res);
		}
	}

	public void AddPending(IAsyncOperation op)
	{
		lock (_pendingOps)
		{
			_pendingOps.Add(op);
		}
	}

	public void Update()
	{
		lock (_pendingOps)
		{
			_doneOps.Clear();
			_tempOps.Clear();
			_tempOps.AddRange(_pendingOps);
			_pendingOps.Clear();
			bool working = false;
			for (int i = 0; i < _tempOps.Count; i++)
			{
				IAsyncOperation op = _tempOps[i];
				if (working)
				{
					_pendingOps.Add(op);
					continue;
				}
				working = true;
				if (!op.Started)
				{
					op.Begin();
					_pendingOps.Add(op);
				}
				else if (op.Done)
				{
					_doneOps.Add(op);
				}
				else
				{
					_pendingOps.Add(op);
				}
			}
			_tempOps.Clear();
		}
		for (int i = 0; i < _doneOps.Count; i++)
		{
			_doneOps[i].Conclude();
		}
		_doneOps.Clear();
	}
}
