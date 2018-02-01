﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Specifies the behavior for continuation operations.
	/// </summary>
	[Flags]
	public enum AsyncContinuationOptions
	{
		/// <summary>
		/// No options. Default continuation behaviour.
		/// </summary>
		None = 0,

		/// <summary>
		/// Specifies that the continuation should be scheduled only if its antecedent has completed successfully.
		/// </summary>
		OnlyOnRanToCompletion = 1,

		/// <summary>
		/// Specifies that the continuation task should be scheduled only if its antecedent has completed with an error.
		/// </summary>
		OnlyOnFaulted = 2,

		/// <summary>
		/// Specifies that the continuation task should be scheduled only if its antecedent has been canceled.
		/// </summary>
		OnlyOnCanceled = 4,

		/// <summary>
		/// Specifies that the continuation should be scheduled only if its antecedent has not completed successfully.
		/// </summary>
		NotOnRanToCompletion = OnlyOnCanceled | OnlyOnFaulted,

		/// <summary>
		/// Specifies that the continuation should be scheduled only if its antecedent has not completed with an error.
		/// </summary>
		NotOnFaulted = OnlyOnCanceled | OnlyOnRanToCompletion,

		/// <summary>
		/// Specifies that the continuation should be scheduled only if its antecedent has not been canceled.
		/// </summary>
		NotOnCanceled = OnlyOnFaulted | OnlyOnRanToCompletion,
	}
}