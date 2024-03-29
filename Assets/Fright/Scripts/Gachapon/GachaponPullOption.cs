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
	/// Defines one option that can be pulled by the gachapon puller
	public struct GachaponPullOption<TPayload>
	{
		/// The actual contents of this pull
		public TPayload payload;
		/// The random weight of pulling this option relative to other options
		public float weight;
		/// The cost to pull this option, is taken out of the pull session's budget
		public float cost;
		/// An optional classification for applying custom rules
		public int category;
		/// An optional classification for applying custom rules
		public int rarity;

		/// Convenience constuctor
		public GachaponPullOption(TPayload payload, float weight, float cost = 1.0f, int category = -1, int rarity = -1)
		{
			this.payload = payload;
			this.weight = weight;
			this.cost = cost;
			this.category = category;
			this.rarity = rarity;
		}
	}
}