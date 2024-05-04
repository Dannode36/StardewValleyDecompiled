using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using StardewValley.Extensions;
using StardewValley.GameData.Bundles;

namespace StardewValley;

public class BundleGenerator
{
	public List<RandomBundleData> randomBundleData;

	public Dictionary<string, string> bundleData;

	public Random random;

	public Dictionary<string, string> Generate(List<RandomBundleData> bundle_data, Random rng)
	{
		random = rng;
		randomBundleData = bundle_data;
		bundleData = new Dictionary<string, string>(DataLoader.Bundles(Game1.content));
		foreach (RandomBundleData area_data in randomBundleData)
		{
			List<int> index_lookups = new List<int>();
			string[] array = ArgUtility.SplitBySpace(area_data.Keys);
			Dictionary<int, BundleData> selected_bundles = new Dictionary<int, BundleData>();
			string[] array2 = array;
			foreach (string index_string in array2)
			{
				index_lookups.Add(int.Parse(index_string));
			}
			BundleSetData bundle_set = random.ChooseFrom(area_data.BundleSets);
			if (bundle_set != null)
			{
				foreach (BundleData bundle in bundle_set.Bundles)
				{
					selected_bundles[bundle.Index] = bundle;
				}
			}
			List<BundleData> random_bundle_pool = new List<BundleData>();
			foreach (BundleData bundle in area_data.Bundles)
			{
				random_bundle_pool.Add(bundle);
			}
			for (int i = 0; i < index_lookups.Count; i++)
			{
				if (selected_bundles.ContainsKey(i))
				{
					continue;
				}
				List<BundleData> index_bundles = new List<BundleData>();
				foreach (BundleData bundle in random_bundle_pool)
				{
					if (bundle.Index == i)
					{
						index_bundles.Add(bundle);
					}
				}
				if (index_bundles.Count > 0)
				{
					BundleData selected_bundle = random.ChooseFrom(index_bundles);
					random_bundle_pool.Remove(selected_bundle);
					selected_bundles[i] = selected_bundle;
					continue;
				}
				foreach (BundleData bundle in random_bundle_pool)
				{
					if (bundle.Index == -1)
					{
						index_bundles.Add(bundle);
					}
				}
				if (index_bundles.Count > 0)
				{
					BundleData selected_bundle = random.ChooseFrom(index_bundles);
					random_bundle_pool.Remove(selected_bundle);
					selected_bundles[i] = selected_bundle;
				}
			}
			foreach (int key in selected_bundles.Keys)
			{
				BundleData data = selected_bundles[key];
				StringBuilder string_data = new StringBuilder();
				string_data.Append(data.Name);
				string_data.Append("/");
				string reward_string = data.Reward;
				if (reward_string.Length > 0)
				{
					try
					{
						if (char.IsDigit(reward_string[0]))
						{
							string[] reward_split = ArgUtility.SplitBySpace(reward_string);
							int count = int.Parse(reward_split[0]);
							Item reward = Utility.fuzzyItemSearch(string.Join(" ", reward_split, 1, reward_split.Length - 1), count);
							if (reward != null)
							{
								reward_string = Utility.getStandardDescriptionFromItem(reward, reward.Stack);
							}
						}
					}
					catch (Exception e)
					{
						Game1.log.Error("ERROR: Malformed reward string in bundle: " + reward_string, e);
						reward_string = data.Reward;
					}
				}
				string_data.Append(reward_string);
				string_data.Append("/");
				int color = 0;
				switch (data.Color)
				{
				case "Red":
					color = 4;
					break;
				case "Blue":
					color = 5;
					break;
				case "Green":
					color = 0;
					break;
				case "Orange":
					color = 2;
					break;
				case "Purple":
					color = 1;
					break;
				case "Teal":
					color = 6;
					break;
				case "Yellow":
					color = 3;
					break;
				}
				ParseItemList(string_data, data.Items, data.Pick, data.RequiredItems, color);
				string_data.Append("/");
				string_data.Append(data.Sprite);
				string_data.Append('/');
				string_data.Append(data.Name);
				bundleData[area_data.AreaName + "/" + index_lookups[key]] = string_data.ToString();
			}
		}
		return bundleData;
	}

	public string ParseRandomTags(string data)
	{
		int open_index;
		do
		{
			open_index = data.LastIndexOf('[');
			if (open_index >= 0)
			{
				int close_index = data.IndexOf(']', open_index);
				if (close_index == -1)
				{
					return data;
				}
				string inner = data.Substring(open_index + 1, close_index - open_index - 1);
				string value = random.ChooseFrom(inner.Split('|'));
				data = data.Remove(open_index, close_index - open_index + 1);
				data = data.Insert(open_index, value);
			}
		}
		while (open_index >= 0);
		return data;
	}

	public Item ParseItemString(string item_string)
	{
		string[] parts = ArgUtility.SplitBySpace(item_string);
		int index = 0;
		int count = int.Parse(parts[index]);
		index++;
		int quality = 0;
		switch (parts[index])
		{
		case "NQ":
			quality = 0;
			index++;
			break;
		case "SQ":
			quality = 1;
			index++;
			break;
		case "GQ":
			quality = 2;
			index++;
			break;
		case "IQ":
			quality = 3;
			index++;
			break;
		}
		string item_name = string.Join(" ", parts, index, parts.Length - index);
		if (char.IsDigit(item_name[0]))
		{
			Item item = ItemRegistry.Create("(O)" + item_name, count);
			item.Quality = quality;
			return item;
		}
		Item found_item = null;
		if (item_name.ToLowerInvariant().EndsWith("category"))
		{
			try
			{
				FieldInfo field = typeof(Object).GetField(item_name);
				if (field != null)
				{
					found_item = new Object(((int)field.GetValue(null)).ToString(), 1);
				}
			}
			catch (Exception)
			{
			}
		}
		if (found_item == null)
		{
			found_item = Utility.fuzzyItemSearch(item_name);
			found_item.Quality = quality;
		}
		if (found_item == null)
		{
			throw new Exception("Invalid item name '" + item_name + "' encountered while generating a bundle.");
		}
		found_item.Stack = count;
		return found_item;
	}

	public void ParseItemList(StringBuilder builder, string item_list, int pick_count, int required_items, int color)
	{
		item_list = ParseRandomTags(item_list);
		string[] items = item_list.Split(',');
		List<string> item_strings = new List<string>();
		for (int i = 0; i < items.Length; i++)
		{
			Item item = ParseItemString(items[i]);
			item_strings.Add(item.ItemId + " " + item.Stack + " " + item.Quality);
		}
		if (pick_count < 0)
		{
			pick_count = item_strings.Count;
		}
		if (required_items < 0)
		{
			required_items = pick_count;
		}
		while (item_strings.Count > pick_count)
		{
			int index_to_remove = random.Next(item_strings.Count);
			item_strings.RemoveAt(index_to_remove);
		}
		for (int i = 0; i < item_strings.Count; i++)
		{
			builder.Append(item_strings[i]);
			if (i < item_strings.Count - 1)
			{
				builder.Append(" ");
			}
		}
		builder.Append("/");
		builder.Append(color);
		builder.Append("/");
		builder.Append(required_items);
	}
}
