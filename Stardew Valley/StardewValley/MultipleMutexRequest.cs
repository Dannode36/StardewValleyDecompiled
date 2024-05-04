using System;
using System.Collections.Generic;
using StardewValley.Network;

namespace StardewValley;

public class MultipleMutexRequest
{
	protected int _reportedCount;

	protected List<NetMutex> _acquiredLocks;

	protected List<NetMutex> _mutexList;

	protected Action<MultipleMutexRequest> _onSuccess;

	protected Action<MultipleMutexRequest> _onFailure;

	public MultipleMutexRequest(List<NetMutex> mutexes, Action<MultipleMutexRequest> success_callback = null, Action<MultipleMutexRequest> failure_callback = null)
	{
		_onSuccess = success_callback;
		_onFailure = failure_callback;
		_acquiredLocks = new List<NetMutex>();
		_mutexList = new List<NetMutex>(mutexes);
		_RequestMutexes();
	}

	public MultipleMutexRequest(NetMutex[] mutexes, Action<MultipleMutexRequest> success_callback = null, Action<MultipleMutexRequest> failure_callback = null)
	{
		_onSuccess = success_callback;
		_onFailure = failure_callback;
		_acquiredLocks = new List<NetMutex>();
		_mutexList = new List<NetMutex>(mutexes);
		_RequestMutexes();
	}

	protected void _RequestMutexes()
	{
		if (_mutexList == null)
		{
			_onFailure?.Invoke(this);
			return;
		}
		if (_mutexList.Count == 0)
		{
			_onSuccess?.Invoke(this);
			return;
		}
		for (int i = 0; i < _mutexList.Count; i++)
		{
			if (_mutexList[i].IsLocked())
			{
				_onFailure?.Invoke(this);
				return;
			}
		}
		for (int i = 0; i < _mutexList.Count; i++)
		{
			NetMutex mutex = _mutexList[i];
			mutex.RequestLock(delegate
			{
				_OnLockAcquired(mutex);
			}, delegate
			{
				_OnLockFailed(mutex);
			});
		}
	}

	protected void _OnLockAcquired(NetMutex mutex)
	{
		_reportedCount++;
		_acquiredLocks.Add(mutex);
		if (_reportedCount >= _mutexList.Count)
		{
			_Finalize();
		}
	}

	protected void _OnLockFailed(NetMutex mutex)
	{
		_reportedCount++;
		if (_reportedCount >= _mutexList.Count)
		{
			_Finalize();
		}
	}

	protected void _Finalize()
	{
		if (_acquiredLocks.Count < _mutexList.Count)
		{
			ReleaseLocks();
			_onFailure(this);
		}
		else
		{
			_onSuccess(this);
		}
	}

	public void ReleaseLocks()
	{
		for (int i = 0; i < _acquiredLocks.Count; i++)
		{
			_acquiredLocks[i].ReleaseLock();
		}
		_acquiredLocks.Clear();
	}
}
