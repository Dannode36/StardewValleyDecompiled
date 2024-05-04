using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles;

public class PlayerStatusList : INetObject<NetFields>
{
	public enum SortMode
	{
		None,
		NumberSort,
		NumberSortDescending,
		AlphaSort,
		AlphaSortDescending
	}

	public enum DisplayMode
	{
		Text,
		LocalizedText,
		Icons
	}

	public enum VerticalAlignment
	{
		Top,
		Bottom
	}

	public enum HorizontalAlignment
	{
		Left,
		Right
	}

	protected readonly NetLongDictionary<string, NetString> _statusList = new NetLongDictionary<string, NetString>
	{
		InterpolationWait = false
	};

	protected readonly Dictionary<long, string> _formattedStatusList = new Dictionary<long, string>();

	protected readonly Dictionary<string, Texture2D> _iconSprites = new Dictionary<string, Texture2D>();

	protected readonly List<Farmer> _sortedFarmers = new List<Farmer>();

	public int iconAnimationFrames = 1;

	public int largestSpriteWidth;

	public int largestSpriteHeight;

	public SortMode sortMode;

	public DisplayMode displayMode;

	protected Dictionary<string, KeyValuePair<string, Rectangle>> _iconDefinitions = new Dictionary<string, KeyValuePair<string, Rectangle>>();

	public NetFields NetFields { get; } = new NetFields("PlayerStatusList");


	public PlayerStatusList()
	{
		InitNetFields();
	}

	public void InitNetFields()
	{
		NetFields.SetOwner(this).AddField(_statusList, "_statusList");
		_statusList.OnValueRemoved += delegate
		{
			_OnValueChanged();
		};
		_statusList.OnValueAdded += delegate
		{
			_OnValueChanged();
		};
		_statusList.OnConflictResolve += delegate
		{
			_OnValueChanged();
		};
		_statusList.OnValueTargetUpdated += delegate(long key, string value, string targetValue)
		{
			if (_statusList.FieldDict.TryGetValue(key, out var value2))
			{
				value2.CancelInterpolation();
			}
			_OnValueChanged();
		};
	}

	public void AddSpriteDefinition(string key, string file, int x, int y, int width, int height)
	{
		if (!_iconSprites.TryGetValue(file, out var iconSprite) || iconSprite.IsDisposed)
		{
			_iconSprites[file] = Game1.content.Load<Texture2D>(file);
		}
		_iconDefinitions[key] = new KeyValuePair<string, Rectangle>(file, new Rectangle(x, y, width, height));
		if (width > largestSpriteWidth)
		{
			largestSpriteWidth = width;
		}
		if (height > largestSpriteHeight)
		{
			largestSpriteHeight = height;
		}
	}

	public void UpdateState(string newState)
	{
		if (!_statusList.TryGetValue(Game1.player.UniqueMultiplayerID, out var oldState) || oldState != newState)
		{
			_statusList[Game1.player.UniqueMultiplayerID] = newState;
		}
	}

	public void WithdrawState()
	{
		_statusList.Remove(Game1.player.UniqueMultiplayerID);
	}

	protected void _OnValueChanged()
	{
		foreach (long id in _statusList.Keys)
		{
			_formattedStatusList[id] = GetStatusText(id);
		}
		_ResortList();
	}

	protected void _ResortList()
	{
		_sortedFarmers.Clear();
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			_sortedFarmers.Add(farmer);
		}
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (Game1.IsMasterGame && !_sortedFarmers.Contains(farmer) && _statusList.ContainsKey(farmer.UniqueMultiplayerID))
			{
				_statusList.Remove(farmer.UniqueMultiplayerID);
			}
			if (!_statusList.ContainsKey(farmer.UniqueMultiplayerID))
			{
				_sortedFarmers.Remove(farmer);
			}
		}
		switch (sortMode)
		{
		case SortMode.AlphaSort:
		case SortMode.AlphaSortDescending:
			_sortedFarmers.Sort((Farmer a, Farmer b) => GetStatusText(a.UniqueMultiplayerID).CompareTo(GetStatusText(b.UniqueMultiplayerID)));
			if (sortMode == SortMode.AlphaSortDescending)
			{
				_sortedFarmers.Reverse();
			}
			break;
		case SortMode.NumberSort:
		case SortMode.NumberSortDescending:
			_sortedFarmers.Sort((Farmer a, Farmer b) => GetStatusInt(a.UniqueMultiplayerID).CompareTo(GetStatusInt(b.UniqueMultiplayerID)));
			if (sortMode == SortMode.NumberSortDescending)
			{
				_sortedFarmers.Reverse();
			}
			break;
		}
	}

	/// <summary>Try to get the status text for a player.</summary>
	/// <param name="id">The unique multiplayer ID for the player whose status to get.</param>
	/// <param name="statusText">The status text if found, else <c>null</c>.</param>
	/// <returns>Whether the status was found.</returns>
	public bool TryGetStatusText(long id, out string statusText)
	{
		if (_statusList.TryGetValue(id, out statusText))
		{
			if (displayMode == DisplayMode.LocalizedText)
			{
				statusText = Game1.content.LoadString(statusText);
			}
			return true;
		}
		statusText = null;
		return false;
	}

	/// <summary>Get the string representation of a player's status.</summary>
	/// <param name="id">The unique multiplayer ID for the player whose status to get.</param>
	/// <param name="fallback">The value to return if no status is found for the player.</param>
	/// <returns>The string representation of the player's status, or <paramref name="fallback" /> if not found.</returns>
	public string GetStatusText(long id, string fallback = "")
	{
		if (!TryGetStatusText(id, out var statusText))
		{
			return fallback;
		}
		return statusText;
	}

	/// <summary>Get the integer representation of a player's status (e.g. number of eggs found at the Egg Festival).</summary>
	/// <param name="id">The unique multiplayer ID for the player whose status to get.</param>
	/// <param name="fallback">The value to return if no status is found for the player.</param>
	/// <returns>The integer representation of the player's status, or <paramref name="fallback" /> if not found.</returns>
	public int GetStatusInt(long id, int fallback = 0)
	{
		if (!TryGetStatusText(id, out var statusText) || !int.TryParse(statusText, out var status))
		{
			return fallback;
		}
		return status;
	}

	public void Draw(SpriteBatch b, Vector2 draw_position, float draw_scale = 4f, float draw_layer = 0.45f, HorizontalAlignment horizontal_origin = HorizontalAlignment.Left, VerticalAlignment vertical_origin = VerticalAlignment.Top)
	{
		float y_offset_per_entry = 12f;
		if (displayMode == DisplayMode.Icons && (float)largestSpriteHeight > y_offset_per_entry)
		{
			y_offset_per_entry = largestSpriteHeight;
		}
		if (horizontal_origin == HorizontalAlignment.Right)
		{
			float longest_string = 0f;
			if (displayMode == DisplayMode.Icons)
			{
				draw_position.X -= (float)largestSpriteWidth * draw_scale;
			}
			else
			{
				foreach (Farmer farmer in _sortedFarmers)
				{
					if (_formattedStatusList.TryGetValue(farmer.UniqueMultiplayerID, out var state))
					{
						float string_length = Game1.dialogueFont.MeasureString(state).X;
						if (longest_string < string_length)
						{
							longest_string = string_length;
						}
					}
				}
				draw_position.X -= (longest_string + 16f) * draw_scale;
			}
		}
		if (vertical_origin == VerticalAlignment.Bottom)
		{
			draw_position.Y -= y_offset_per_entry * (float)_statusList.Length * draw_scale;
		}
		foreach (Farmer farmer in _sortedFarmers)
		{
			float sort_direction = ((!Game1.isUsingBackToFrontSorting) ? 1 : (-1));
			if (_formattedStatusList.TryGetValue(farmer.UniqueMultiplayerID, out var state))
			{
				Vector2 draw_offset = Vector2.Zero;
				farmer.FarmerRenderer.drawMiniPortrat(b, draw_position, draw_layer, draw_scale * 0.75f, 2, farmer);
				if (displayMode == DisplayMode.Icons && _iconDefinitions.TryGetValue(state, out var spriteDefinition))
				{
					draw_offset.X += 12f * draw_scale;
					Rectangle currentSrcRect = spriteDefinition.Value;
					currentSrcRect.Y = (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % (double)(iconAnimationFrames * 100) / 100.0) * 16;
					b.Draw(_iconSprites[spriteDefinition.Key], draw_position + draw_offset, currentSrcRect, Color.White, 0f, Vector2.Zero, draw_scale, SpriteEffects.None, draw_layer - 0.0001f * sort_direction);
				}
				else
				{
					draw_offset.X += 16f * draw_scale;
					draw_offset.Y += 2f * draw_scale;
					string drawn_string = state;
					b.DrawString(Game1.dialogueFont, drawn_string, draw_position + draw_offset + Vector2.One * draw_scale, Color.Black, 0f, Vector2.Zero, draw_scale / 4f, SpriteEffects.None, draw_layer - 0.0001f * sort_direction);
					b.DrawString(Game1.dialogueFont, drawn_string, draw_position + draw_offset, Color.White, 0f, Vector2.Zero, draw_scale / 4f, SpriteEffects.None, draw_layer);
				}
				draw_position.Y += y_offset_per_entry * draw_scale;
			}
		}
	}
}
