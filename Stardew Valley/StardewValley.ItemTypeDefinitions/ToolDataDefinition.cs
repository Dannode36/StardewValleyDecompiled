using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.GameData.Tools;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for tool items.</summary>
public class ToolDataDefinition : BaseItemDataDefinition
{
	/// <inheritdoc />
	public override string Identifier => "(T)";

	/// <inheritdoc />
	public override IEnumerable<string> GetAllIds()
	{
		return Game1.toolData.Keys;
	}

	/// <inheritdoc />
	public override bool Exists(string itemId)
	{
		if (itemId != null)
		{
			return Game1.toolData.ContainsKey(itemId);
		}
		return false;
	}

	/// <inheritdoc />
	public override ParsedItemData GetData(string itemId)
	{
		ToolData data = GetRawData(itemId);
		if (data == null)
		{
			return null;
		}
		return new ParsedItemData(this, itemId, (data.MenuSpriteIndex > -1) ? data.MenuSpriteIndex : data.SpriteIndex, data.Texture, itemId, TokenParser.ParseText(data.DisplayName), TokenParser.ParseText(data.Description), -99, null, data);
	}

	/// <inheritdoc />
	public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return Game1.getSquareSourceRectForNonStandardTileSheet(texture, 16, 16, spriteIndex);
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		ToolData rawData = GetRawData(data.ItemId);
		Tool tool = CreateToolInstance(data, rawData);
		if (tool == null)
		{
			return GetErrorTool(data);
		}
		tool.ItemId = data.ItemId;
		tool.SetSpriteIndex(rawData.SpriteIndex);
		if (rawData.MenuSpriteIndex > -1)
		{
			tool.IndexOfMenuItemView = rawData.MenuSpriteIndex;
		}
		tool.Name = rawData.Name;
		if (rawData.UpgradeLevel > -1)
		{
			tool.UpgradeLevel = rawData.UpgradeLevel;
		}
		if (rawData.AttachmentSlots > -1)
		{
			tool.AttachmentSlotsCount = rawData.AttachmentSlots;
		}
		if (rawData.SetProperties != null)
		{
			Type type = tool.GetType();
			foreach (KeyValuePair<string, string> pair in rawData.SetProperties)
			{
				TrySetProperty(type, tool, pair.Key, pair.Value);
			}
		}
		if (rawData.ModData != null)
		{
			foreach (KeyValuePair<string, string> pair in rawData.ModData)
			{
				tool.modData[pair.Key] = pair.Value;
			}
		}
		return tool;
	}

	/// <summary>Get the raw data fields for an item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	protected ToolData GetRawData(string itemId)
	{
		if (itemId == null || !Game1.toolData.TryGetValue(itemId, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Create an empty instance of a tool's type, if valid.</summary>
	/// <param name="itemData">The parsed item data.</param>
	/// <param name="toolData">The tool data.</param>
	/// <remarks>Note for mods: this method deliberately doesn't allow custom types that aren't part of the game code because that will cause crashes in multiplayer or when saving the game. If you patch this logic to allow a custom class type, you should be aware of the consequences and avoid permanently breaking players' saves when your mod is removed.</remarks>
	protected Tool CreateToolInstance(ParsedItemData itemData, ToolData toolData)
	{
		if (itemData != null && toolData != null)
		{
			Type type = typeof(Tool).Assembly.GetType("StardewValley.Tools." + toolData.ClassName);
			if (type != null)
			{
				Tool tool = (Tool)Activator.CreateInstance(type);
				if (tool != null)
				{
					return tool;
				}
			}
		}
		return GetErrorTool(itemData);
	}

	/// <summary>Create an Error Item tool, for use when we don't have a class to initialize.</summary>
	/// <param name="data">The item data.</param>
	protected Tool GetErrorTool(ParsedItemData data)
	{
		return new ErrorTool(data.ItemId);
	}

	/// <summary>Set a tool property.</summary>
	/// <param name="type">The tool type.</param>
	/// <param name="tool">The tool instance.</param>
	/// <param name="name">The property name.</param>
	/// <param name="rawValue">The raw property value.</param>
	protected void TrySetProperty(Type type, Tool tool, string name, string rawValue)
	{
		MemberInfo member = (MemberInfo)(((object)type.GetProperty(name)) ?? ((object)type.GetField(name)));
		string error;
		if (member == null)
		{
			Game1.log.Error($"Can't set field or property '{name}' for tool '{tool.QualifiedItemId}': the {type.FullName} class has none public with that name");
		}
		else if (!member.TrySetValueFromString(tool, rawValue, null, out error))
		{
			Game1.log.Error($"Can't set {((member is FieldInfo) ? "field" : "property")} '{name}' for tool '{tool.QualifiedItemId}': {error}.");
		}
	}
}
