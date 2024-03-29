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
using UnityEngine;

namespace Fright.Gachapon
{
	/// Performs essential operations required to pull from a gachapon
	public class GachaponPuller<TPayload>
	{
		public Dictionary<string, GachaponInitModel<TPayload>> initModels = new Dictionary<string, GachaponInitModel<TPayload>>();
		public Dictionary<string, GachaponPool<TPayload>> pools = new Dictionary<string, GachaponPool<TPayload>>();
		public List<IGachaponRule<TPayload>> defaultGachaponRules = new List<IGachaponRule<TPayload>>();

		/// Pulls as many items from the gachapon session with the given initialize model
		public virtual IEnumerable<TPayload> Pull(string initModelID, float budget, params IGachaponRule<TPayload>[] extraRules)
		{
			if (initModels.TryGetValue(initModelID, out GachaponInitModel<TPayload> model))
			{
				foreach(var result in Pull(budget, GetPoolEntriesFromInitModel(model), extraRules))
				{
					yield return result;
				}
			}
			else
			{
				Debug.LogError($"[GachaponPuller] Unknown gachapon init model \"{initModelID}\"");
				yield break;
			}
		}

		/// Pulls as many items as possible with the give budget, pools, and rules
		public virtual IEnumerable<TPayload> Pull(
			float budget,
			IEnumerable<GachaponPullSession<TPayload>.PoolEntry> pools,
			params IGachaponRule<TPayload>[] extraRules)
		{
			var session = CreatePullSession(budget, pools, extraRules);

			//Do the actual pulling here
			PullUntilDone(session);

			//Notify the session rules that the session is about to close
			foreach(var sessionRule in session.sessionRules)
				sessionRule.OnSessionClosing(session);

			//Return the result
			return session.GetPayloadResults();
		}

		/// Pulls until there is no budget left, or there are no longer any options
		protected internal virtual void PullUntilDone(GachaponPullSession<TPayload> session)
		{
			bool isStillPulling = true;

			while (isStillPulling && session.budgetRemaining > 0.0f)
			{
				isStillPulling = PullOnce(session);
			}
		}

		/// Performs a single pull, return true if it was successful
		protected internal virtual bool PullOnce(GachaponPullSession<TPayload> session)
		{
			GachaponPullOption<TPayload> pulledResult = default;

			//Pick one of the pools
			(var poolEntry, bool didPickPool) = PickRandom(session.GetModifiedPoolEntries(), entry => entry.weight);

			if (didPickPool && poolEntry.pool != null)
			{
				//Pick one of the options
				(var option, bool didPickOption) = PickRandom(session.GetModifiedPullOptions(poolEntry.pool), option => option.weight);

				if (didPickOption)
				{
					pulledResult = option;
				}
			}

			//Check if successful
			if (!pulledResult.Equals(default))
			{
				//Update the session
				session.budgetRemaining -= pulledResult.cost;
				session.results.Add(pulledResult);
				
				//Notify the session rules that a pull was completed
				foreach(var sessionRule in session.sessionRules)
					sessionRule.OnPullCompleted(session);
			}

			//Return the result
			return !pulledResult.Equals(default);
		}

		/// Given an enumerable list of weighted object, picks and returns one of those objects
		protected internal virtual (T result, bool wasSuccessful) PickRandom<T>(IEnumerable<T> options, Func<T, float> getWeightFunction)
		{
			float sumWeight = 0.0f;

			//Sum all the weights together
			foreach(var option in options)
			{
				float weight = getWeightFunction(option);

				if (weight > 0.0f)
				{
					sumWeight += weight;
				}
			}

			//Check if there were any options to pick
			if (sumWeight <= 0.0f)
			{
				return (default, false);
			}
			
			//Randomly pick an item
			float rand = UnityEngine.Random.Range(0.0f, sumWeight);
			
			//Find the randomly picked item and return it
			foreach(var option in options)
			{
				float weight = getWeightFunction(option);

				if (weight > 0.0f)
				{
					rand -= weight;

					if (rand <= 0.0f)
					{
						return (option, true);
					}
				}
			}
			
			throw new Exception("Function ended without returning a result. getWeightFunction should return the same value for the same option");
		}

		protected internal virtual IEnumerable<GachaponPullSession<TPayload>.PoolEntry> GetPoolEntriesFromInitModel(GachaponInitModel<TPayload> initModel)
		{
			foreach(var tuple in initModel.pools)
			{
				if (pools.TryGetValue(tuple.gachaponPoolID, out GachaponPool<TPayload> pool))
				{
					yield return new GachaponPullSession<TPayload>.PoolEntry()
					{
						pool = pool,
						weight = tuple.weight,
					};
				}
				else
				{
					Debug.LogError($"[GachaponPuller] Unknown pool \"{tuple.gachaponPoolID}\"");
				}
			}
		}

		protected internal virtual GachaponPullSession<TPayload> CreatePullSession(
			float initialBudget,
			IEnumerable<GachaponPullSession<TPayload>.PoolEntry> pools,
			IEnumerable<IGachaponRule<TPayload>> rules)
		{
			//Construct the pull session
			var result = new GachaponPullSession<TPayload>();
			result.puller = this;
			result.budgetRemaining = initialBudget;
			result.pools.AddRange(pools);
			
			//Add the default and custom rules
			foreach(var rule in defaultGachaponRules)
				AddRuleToSession(result, rule);
			foreach(var rule in rules)
				AddRuleToSession(result, rule);

			//Notify the session rules that a session was created
			foreach(var sessionRule in result.sessionRules)
				sessionRule.OnSessionCreated(result);

			//Return the result
			return result;
		}

		protected internal virtual void AddRuleToSession(GachaponPullSession<TPayload> session, IGachaponRule<TPayload> rule)
		{
			if (rule is IPoolRule<TPayload> poolRule)
				session.poolRules.Add(poolRule);
			if (rule is IPullOptionRule<TPayload> pullOptionRule)
				session.pullOptionRules.Add(pullOptionRule);
			if (rule is ISessionRule<TPayload> sessionRule)
				session.sessionRules.Add(sessionRule);
		}
	}
}