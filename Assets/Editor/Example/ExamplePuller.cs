using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Fright.Gachapon;
using UnityEngine;
using UnityEditor;

public class ExamplePuller : GachaponPuller<string>
{
	public ExamplePuller()
	{
		var catPool = new GachaponPool<string>()
		{
			category = 0,
			pullOptions = new GachaponPullOption<string>[]
			{
				new GachaponPullOption<string>("Pickles", 5.0f),
				new GachaponPullOption<string>("Pepper", 5.0f),
				new GachaponPullOption<string>("Taz", 40.0f),
				new GachaponPullOption<string>("Kaltse", 50.0f),
			}
		};

		var dogPool = new GachaponPool<string>()
		{
			category = 1,
			pullOptions = new GachaponPullOption<string>[]
			{
				new GachaponPullOption<string>("Diesel", 5.0f),
				new GachaponPullOption<string>("Sophie", 5.0f),
				new GachaponPullOption<string>("Ellie", 40.0f),
				new GachaponPullOption<string>("Brittany", 50.0f),
			}
		};

		var petInitData = new GachaponInitModel<string>()
		{
			id = "pets",
			pools = new List<(string gachaponPoolID, float weight)>()
			{
				("cats", 40.0f),
				("dogs", 60.0f),
			},
		};

		pools.Add("cats", catPool);
		pools.Add("dogs", dogPool);

		initModels.Add("pets", petInitData);
	}

	[MenuItem("Fright/Pick some pets")]
	public static void PickSomePets()
	{
		const int PULLS = 10000;
		ExamplePuller puller = new ExamplePuller();
		Dictionary<string, int> timesPulled = new Dictionary<string, int>();

		for(int i = 0; i < PULLS; ++i)
		{
			foreach(var pet in puller.Pull("pets", 1.0f))
			{
				timesPulled.TryGetValue(pet, out int count);
				timesPulled[pet] = count + 1;
			}
		}

		var result = new System.Text.StringBuilder();
		result.Append("Pet Results\n=======================\n");

		foreach(var pair in (timesPulled).OrderByDescending(entry => entry.Value))
		{
			result.AppendLine($"    {pair.Value}\t {(float)pair.Value * 100.0f / (float)PULLS}%  {pair.Key}");
		}

		Debug.Log(result);
	}
}