﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal sealed class ContinueWithResult<T, U> : AsyncResult<U>, IAsyncContinuation
	{
		#region data

		private readonly IAsyncOperation _op;
		private readonly AsyncContinuationOptions _options;
		private readonly object _continuation;
		private readonly object _userState;

		#endregion

		#region interface

		internal ContinueWithResult(IAsyncOperation op, AsyncContinuationOptions options, object continuation, object userState)
			: base(AsyncOperationStatus.Running)
		{
			_op = op;
			_options = options;
			_continuation = continuation;
			_userState = userState;

			if ((options & AsyncContinuationOptions.ExecuteSynchronously) != 0)
			{
				op.AddContinuation(this, null);
			}
			else
			{
				op.AddContinuation(this);
			}
		}

		#endregion

		#region AsyncResult

		protected override float GetProgress()
		{
			return _op.Progress;
		}

		protected override void OnCancel()
		{
			_op.Cancel();
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op, bool inline)
		{
			if (AsyncContinuation.CanInvoke(op, _options))
			{
				try
				{
					var result = default(U);

					switch (_continuation)
					{
						case Action<IAsyncOperation> a:
							a.Invoke(op);
							break;

						case Action<IAsyncOperation<T>> a1:
							a1.Invoke(op as IAsyncOperation<T>);
							break;

						case Func<IAsyncOperation, U> f:
							result = f.Invoke(op);
							break;

						case Func<IAsyncOperation<T>, U> f1:
							result = f1.Invoke(op as IAsyncOperation<T>);
							break;

						case Action<IAsyncOperation, object> ao:
							ao.Invoke(op, _userState);
							break;

						case Action<IAsyncOperation<T>, object> ao1:
							ao1.Invoke(op as IAsyncOperation<T>, _userState);
							break;

						case Func<IAsyncOperation, object, U> fo:
							result = fo.Invoke(op, _userState);
							break;

						case Func<IAsyncOperation<T>, object, U> fo1:
							result = fo1.Invoke(op as IAsyncOperation<T>, _userState);
							break;

						default:
							TrySetCanceled(inline);
							return;
					}

					TrySetResult(result, inline);
				}
				catch (Exception e)
				{
					TrySetException(e, inline);
				}
			}
			else
			{
				TrySetCanceled(inline);
			}
		}

		#endregion
	}
}
