﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal class CallbackCollection
	{
		#region data

		private struct CallbackData
		{
			public readonly object Callback;
			public readonly SynchronizationContext SyncContext;

			public CallbackData(object callback, SynchronizationContext syncContext)
			{
				Callback = callback;
				SyncContext = syncContext;
			}
		}

		private readonly IAsyncOperation _op;

		private CallbackData _completionCallback1;
		private CallbackData _completionCallback2;
		private CallbackData _completionCallback3;
		private CallbackData _progressCallback1;

		private List<CallbackData> _progressCallbacks;
		private List<CallbackData> _completionCallbacks;

		#endregion

		#region interface

		public CallbackCollection(IAsyncOperation op)
		{
			_op = op;
		}

		public CallbackCollection(IAsyncOperation op, object callback, SynchronizationContext syncContext)
		{
			_op = op;
			_completionCallback1 = new CallbackData(callback, syncContext);
		}

		public void AddCompletionCallback(object callback, SynchronizationContext syncContext)
		{
			var newCallback = new CallbackData(callback, syncContext);

			if (_completionCallback1.Callback == null)
			{
				_completionCallback1 = newCallback;
				return;
			}

			if (_completionCallback2.Callback == null)
			{
				_completionCallback2 = newCallback;
				return;
			}

			if (_completionCallback3.Callback == null)
			{
				_completionCallback3 = newCallback;
				return;
			}

			if (_completionCallbacks == null)
			{
				_completionCallbacks = new List<CallbackData>() { newCallback };
			}
			else
			{
				_completionCallbacks.Add(newCallback);
			}
		}

		public void AddProgressCallback(object callback, SynchronizationContext syncContext)
		{
			var newCallback = new CallbackData(callback, syncContext);

			if (_progressCallback1.Callback == null)
			{
				_progressCallback1 = newCallback;
				return;
			}

			if (_progressCallbacks == null)
			{
				_progressCallbacks = new List<CallbackData>() { newCallback };
			}
			else
			{
				_progressCallbacks.Add(newCallback);
			}
		}

		public bool Remove(object callback)
		{
			if (_completionCallback1.Callback == callback)
			{
				_completionCallback1 = default(CallbackData);
				return true;
			}

			if (_completionCallback2.Callback == callback)
			{
				_completionCallback2 = default(CallbackData);
				return true;
			}

			if (_completionCallback3.Callback == callback)
			{
				_completionCallback3 = default(CallbackData);
				return true;
			}

			if (_progressCallback1.Callback == callback)
			{
				_progressCallback1 = default(CallbackData);
				return true;
			}

			if (_completionCallbacks != null)
			{
				var count = _completionCallbacks.Count;

				for (var i = 0; i < count; ++i)
				{
					if (_completionCallbacks[i].Callback == callback)
					{
						_completionCallbacks.RemoveAt(i);
						return true;
					}
				}
			}

			if (_progressCallbacks != null)
			{
				var count = _progressCallbacks.Count;

				for (var i = 0; i < count; ++i)
				{
					if (_progressCallbacks[i].Callback == callback)
					{
						_progressCallbacks.RemoveAt(i);
						return true;
					}
				}
			}

			return false;
		}

		public void Invoke(bool invokeAsync)
		{
			if (_progressCallback1.Callback != null)
			{
				InvokeProgressCallback(_progressCallback1);
			}

			if (_progressCallbacks != null)
			{
				foreach (var item in _progressCallbacks)
				{
					InvokeProgressCallback(item);
				}
			}

			if (_completionCallback1.Callback != null)
			{
				Invoke(_completionCallback1, invokeAsync);
			}

			if (_completionCallback2.Callback != null)
			{
				Invoke(_completionCallback2, invokeAsync);
			}

			if (_completionCallback3.Callback != null)
			{
				Invoke(_completionCallback3, invokeAsync);
			}

			if (_completionCallbacks != null)
			{
				foreach (var item in _completionCallbacks)
				{
					Invoke(item, invokeAsync);
				}
			}
		}

		public void InvokeProgressCallbacks()
		{
			if (_progressCallback1.Callback != null)
			{
				InvokeProgressCallback(_progressCallback1);
			}

			if (_progressCallbacks != null)
			{
				foreach (var item in _progressCallbacks)
				{
					InvokeProgressCallback(item);
				}
			}
		}

		#endregion

		#region implementation

		private void Invoke(CallbackData callbackData, bool invokeAsync)
		{
			var syncContext = callbackData.SyncContext;

			if (invokeAsync)
			{
				if (syncContext != null)
				{
					syncContext.Post(InvokeInline, callbackData.Callback);
				}
				else
				{
					syncContext = SynchronizationContext.Current;

					if (syncContext != null)
					{
						syncContext.Post(InvokeInline, callbackData.Callback);
					}
					else
					{
						ThreadPool.QueueUserWorkItem(InvokeInline, callbackData.Callback);
					}
				}
			}
			else if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeInline(callbackData.Callback);
			}
			else
			{
				syncContext.Post(InvokeInline, callbackData.Callback);
			}
		}

		private void InvokeProgressCallback(CallbackData callbackData)
		{
			var syncContext = callbackData.SyncContext;

			if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeProgressChangedInline(callbackData.Callback);
			}
			else
			{
				syncContext.Post(InvokeProgressChangedInline, callbackData.Callback);
			}
		}

		private void InvokeInline(object callback)
		{
			Debug.Assert(callback != null);
			CallbackUtility.InvokeCompletionCallback(_op, callback);
		}

		private void InvokeProgressChangedInline(object callback)
		{
			Debug.Assert(callback != null);
			CallbackUtility.InvokeProgressCallback(_op, callback);
		}

		#endregion
	}
}
