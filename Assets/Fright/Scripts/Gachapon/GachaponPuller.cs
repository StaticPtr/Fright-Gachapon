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
	/// Performs essential operations required to pull from a gachapon
	public class GachaponPuller<TPayload>
	{
		public Dictionary<string, GachaponInitModel<TPayload>> initModels = new Dictionary<string, GachaponInitModel<TPayload>>();
		public List<IGachaponRule<TPayload>> defaultGachaponRules = new List<IGachaponRule<TPayload>>();

		public virtual IEnumerable<TPayload> Pull(string initModelID, float budget, params IGachaponRule<TPayload>[] extraRules) => throw new NotImplementedException();

		public virtual IEnumerable<TPayload> Pull(
			float budget,
			IEnumerable<GachaponPullSession<TPayload>.PoolEntry> pools,
			params IGachaponRule<TPayload>[] extraRules)
		{
			using var session = CreatePullSession(budget, pools, extraRules);

			//Do the actual pulling here
			throw new NotImplementedException();

			//Notify the session rules that the session is about to close
			foreach(var sessionRule in session.sessionRules)
				sessionRule.OnSessionClosing(session);

			//Return the result
			return session.GetPayloadResults();
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