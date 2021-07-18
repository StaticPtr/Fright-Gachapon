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
using System.Collections;
using System.Collections.Generic;

namespace Fright.Gachapon
{
	/// Defines a session used to pull one or more times from the gachapon puller.
	/// A session lives for the lifetime of the pull request, and can be used by the gachapon rules.
	public class GachaponPullSession<TPayload>
	{
		/// The object performing the pulling operation
		public GachaponPuller<TPayload> puller;
		/// The number of times an option has been pulled and added to the result list.
		/// NOT the same as the results' count. Session rules might add or remove from the result list.
		public int pulls = 0;
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
	}
}