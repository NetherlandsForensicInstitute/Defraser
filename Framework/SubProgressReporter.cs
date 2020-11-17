/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Netherlands Forensic Institute nor the names 
 *    of its contributors may be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The <see cref="SubProgressReporter"/> reports progress of a sub-task to an
	/// overall progress reporter.
	/// </summary>
	public sealed class SubProgressReporter : IProgressReporter
	{
		private readonly IProgressReporter _progressReporter;
		private readonly int _startProgressPercentage;
		private readonly int _progressPercentageRange;
		private int _currentProgressPercentage;

		#region Properties
		public bool CancellationPending
		{
			get { return _progressReporter.CancellationPending; }
		}
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="SubProgressReporter"/>.
		/// </summary>
		/// <remarks>
		/// The progress reported to the overall progress reporter ranges from
		/// <paramref name="startProgressPercentage"/> for <c>0</c> percent of this
		/// <see cref="SubProgressReporter"/> to <paramref name="endProgressPercentage"/>
		/// for <c>100</c> percent of this <see cref="SubProgressReporter"/>.
		/// </remarks>
		/// <param name="progressReporter">The overall progress reporter</param>
		/// <param name="startProgressPercentage">The start progress percentage in <paramref name="progressReporter"/></param>
		/// <param name="endProgressPercentage">The end progress percentage in <paramref name="progressReporter"/></param>
		public SubProgressReporter(IProgressReporter progressReporter, int startProgressPercentage, int endProgressPercentage)
		{
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();
			PreConditions.Argument("startProgressPercentage").Value(startProgressPercentage).IsNotNegative();
			PreConditions.Argument("endProgressPercentage").Value(endProgressPercentage).InRange(startProgressPercentage, 100);

			_progressReporter = progressReporter;
			_startProgressPercentage = startProgressPercentage;
			_progressPercentageRange = (endProgressPercentage - startProgressPercentage);
			_currentProgressPercentage = -1;
		}

		/// <summary>
		/// Creates a new <see cref="SubProgressReporter"/> for a sub-task starting
		/// at <paramref name="offset"/>.with given <paramref name="length"/> as part
		/// of a (larger) task of <paramref name="total"/> length.
		/// </summary>
		/// <param name="progressReporter">The overall progress reporter</param>
		/// <param name="offset">The start of the sub-task</param>
		/// <param name="length">The length (duration) of the sub-task</param>
		/// <param name="total">The total length (duration) of the (larger) task</param>
		public SubProgressReporter(IProgressReporter progressReporter, long offset, long length, long total)
		{
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();
			PreConditions.Argument("offset").Value(offset).InRange(0L, total);
			PreConditions.Argument("length").Value(length).InRange(0L, (total - offset));
			PreConditions.Argument("total").Value(total).IsNotNegative();

			_progressReporter = progressReporter;
			_startProgressPercentage = (int)((100L * offset) / total);
			_progressPercentageRange = ((int)((100L * (offset + length)) / total) - _startProgressPercentage);
			_currentProgressPercentage = -1;
		}

		public void ReportProgress(int percentProgress)
		{
			int overallProgressPercentage = GetOverallProgressPercentage(percentProgress);
			if (overallProgressPercentage != _currentProgressPercentage)
			{
				_currentProgressPercentage = overallProgressPercentage;
				_progressReporter.ReportProgress(overallProgressPercentage);
			}
		}

		public void ReportProgress(int percentProgress, object userState)
		{
			int overallProgressPercentage = GetOverallProgressPercentage(percentProgress);
			_currentProgressPercentage = -1;
			_progressReporter.ReportProgress(overallProgressPercentage, userState);
		}

		private int GetOverallProgressPercentage(int percentProgress)
		{
			PreConditions.Argument("percentProgress").Value(percentProgress).InRange(0, 100);

			return _startProgressPercentage + ((_progressPercentageRange * percentProgress) / 100);
		}
	}
}
