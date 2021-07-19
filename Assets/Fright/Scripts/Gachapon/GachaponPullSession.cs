//
// MIT License
// 
// Copyright (c) 2021 Brandon Dahn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fright.Gachapon
{
	/// Defines a session used to pull one or more times from the gachapon puller.
	/// A session lives for the lifetime of the pull request, and can be used by the gachapon rules.
	public class GachaponPullSession<TPayload> : IDisposable
	{
		/// The object performing the pulling operation
		public GachaponPuller<TPayload> puller;
		/// The pools that can be pulled from
		public List<PoolEntry> pools = new List<PoolEntry>();
		/// The number of times an option has been pulled and added to the result list.
		/// NOT the same as the results' count. Session rules might add or remove from the result list.
		public int pulls = 0;
		/// The amount of budget remaining, options cannot be pulled unless their cost is less than or equal to the remaining budget
		public float budgetRemaining = 0.0f;
		/// The results that will be returned by the gachapon puller
		public List<GachaponPullOption<TPayload>> results = new List<GachaponPullOption<TPayload>>();
		/// The rules that are being applied to this gachapon pull session
		/// Pool rules can be used to change the weights of pulling from any given pool
		public List<IPoolRule<TPayload>> poolRules = new List<IPoolRule<TPayload>>();
		/// The rules that are being applied to this gachapon pull session
		/// Pull option rules can modify any aspect of a pull option, such as it's weight, payload, or classifications
		public List<IPullOptionRule<TPayload>> pullOptionRules = new List<IPullOptionRule<TPayload>>();
		/// The rules that are being applied to this gachapon pull session
		/// Session rules can modify any aspect of a gachapon session, but usually modifies the results
		public List<ISessionRule<TPayload>> sessionRules = new List<ISessionRule<TPayload>>();

		public IEnumerable<TPayload> GetPayloadResults()
		{
			foreach(var result in results)
			{
				yield return result.payload;
			}
		}

		/// Returns the session's pool entries after accounting for any modification from rules
		public IEnumerable<PoolEntry> GetModifiedPoolEntries()
		{
			foreach(var poolEntry in pools)
			{
				var pool = poolEntry.pool;
				float randomWeight = poolEntry.weight;

				//Let each rule have a chance to modify the weight
				foreach(var poolRule in poolRules)
				{
					poolRule.ModifyPoolWeight(this, pool, ref randomWeight);
				}

				//Return the pool if its weight isn't zero or negative
				if (randomWeight > 0.0f)
				{
					yield return new PoolEntry()
					{
						pool = pool,
						weight = randomWeight,
					};
				}
			}
		}

		/// Returns the pull options from the provided pool after accounting for any modifications from the rules
		public IEnumerable<GachaponPullOption<TPayload>> GetModifiedPullOptions(GachaponPool<TPayload> pool)
		{
			for(int i = 0; i < pool.pullOptions.Length; ++i)
			{
				var modifiedOption = pool.pullOptions[i];

				//Let each rule have a chance to modify the option
				foreach(var pullRule in pullOptionRules)
					pullRule.ModifyPullOption(this, ref modifiedOption);

				//Return the option if its weight isn't zero or negative
				if (modifiedOption.weight > 0.0f)
				{
					yield return modifiedOption;
				}
			}
		}

		/// Cleans up any references or resources used by the pull session
		public virtual void Dispose()
		{
			pools = null;
			results = null;
			poolRules = null;
			pullOptionRules = null;
			sessionRules = null;
			puller = null;
		}

		#region Embedded Types
		public struct PoolEntry
		{
			public GachaponPool<TPayload> pool;
			public float weight;
		}
		#endregion
	}
}