using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buffs;
using StardewValley.Extensions;

namespace StardewValley.Menus;

public class BuffsDisplay : IClickableMenu
{
	/// <summary>The buff attributes shown for buffs which don't have their own icon or description.</summary>
	/// <remarks>For example, a food buff which adds +2 fishing and +1 luck will show two buff icons using this data. A buff which has its own icon but no description will show a single icon with a combined description based on this data.</remarks>
	public static readonly List<BuffAttributeDisplay> displayAttributes = new List<BuffAttributeDisplay>
	{
		new BuffAttributeDisplay(0, (BuffEffects buff) => buff.FarmingLevel, "Strings\\StringsFromCSFiles:Buff.cs.480"),
		new BuffAttributeDisplay(1, (BuffEffects buff) => buff.FishingLevel, "Strings\\StringsFromCSFiles:Buff.cs.483"),
		new BuffAttributeDisplay(2, (BuffEffects buff) => buff.MiningLevel, "Strings\\StringsFromCSFiles:Buff.cs.486"),
		new BuffAttributeDisplay(4, (BuffEffects buff) => buff.LuckLevel, "Strings\\StringsFromCSFiles:Buff.cs.489"),
		new BuffAttributeDisplay(5, (BuffEffects buff) => buff.ForagingLevel, "Strings\\StringsFromCSFiles:Buff.cs.492"),
		new BuffAttributeDisplay(16, (BuffEffects buff) => buff.MaxStamina, "Strings\\StringsFromCSFiles:Buff.cs.495"),
		new BuffAttributeDisplay(11, (BuffEffects buff) => buff.Attack, "Strings\\StringsFromCSFiles:Buff.cs.504"),
		new BuffAttributeDisplay(8, (BuffEffects buff) => buff.MagneticRadius, "Strings\\StringsFromCSFiles:Buff.cs.498"),
		new BuffAttributeDisplay(10, (BuffEffects buff) => buff.Defense, "Strings\\StringsFromCSFiles:Buff.cs.501"),
		new BuffAttributeDisplay(9, (BuffEffects buff) => buff.Speed, "Strings\\StringsFromCSFiles:Buff.cs.507")
	};

	private readonly Dictionary<ClickableTextureComponent, Buff> buffs = new Dictionary<ClickableTextureComponent, Buff>();

	/// <summary>The buff IDs added or renewed since the last icon render.</summary>
	public readonly HashSet<string> updatedIDs = new HashSet<string>();

	public bool dirty;

	public string hoverText = "";

	public BuffsDisplay()
	{
		updatePosition();
	}

	private void updatePosition()
	{
		Rectangle tsarea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
		int w = 288;
		int h = 64;
		int x = tsarea.Right - 300 - width;
		int y = tsarea.Top + 8;
		if (x != xPositionOnScreen || y != yPositionOnScreen || w != width || h != height)
		{
			xPositionOnScreen = x;
			yPositionOnScreen = y;
			width = w;
			height = h;
			resetIcons();
		}
	}

	public override bool isWithinBounds(int x, int y)
	{
		foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
		{
			if (buff.Key.containsPoint(x, y))
			{
				return true;
			}
		}
		return false;
	}

	public int getNumBuffs()
	{
		if (buffs == null)
		{
			return 0;
		}
		return buffs.Count;
	}

	public override void performHoverAction(int x, int y)
	{
		hoverText = "";
		foreach (KeyValuePair<ClickableTextureComponent, Buff> c in buffs)
		{
			if (c.Key.containsPoint(x, y))
			{
				hoverText = c.Key.hoverText + ((c.Value.millisecondsDuration != -2) ? (Environment.NewLine + c.Value.getTimeLeft()) : "");
				string format = hoverText;
				object[] buffDescriptionTextReplacement = getBuffDescriptionTextReplacement(c.Value.id);
				hoverText = string.Format(format, buffDescriptionTextReplacement);
				c.Key.scale = Math.Min(c.Key.baseScale + 0.1f, c.Key.scale + 0.02f);
				break;
			}
		}
	}

	public string[] getBuffDescriptionTextReplacement(string buffName)
	{
		if (buffName == "statue_of_blessings_3")
		{
			return new string[1] { Game1.player.stats.Get("blessingOfWaters").ToString() };
		}
		return LegacyShims.EmptyArray<string>();
	}

	public void arrangeTheseComponentsInThisRectangle(int rectangleX, int rectangleY, int rectangleWidthInComponentWidthUnits, int componentWidth, int componentHeight, int buffer, bool rightToLeft)
	{
		int x = 0;
		int y = 0;
		foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
		{
			ClickableTextureComponent c = buff.Key;
			if (rightToLeft)
			{
				c.bounds = new Rectangle(rectangleX + rectangleWidthInComponentWidthUnits * componentWidth - (x + 1) * (componentWidth + buffer), rectangleY + y * (componentHeight + buffer), componentWidth, componentHeight);
			}
			else
			{
				c.bounds = new Rectangle(rectangleX + x * (componentWidth + buffer), rectangleY + y * (componentHeight + buffer), componentWidth, componentHeight);
			}
			x++;
			if (x > rectangleWidthInComponentWidthUnits)
			{
				y++;
				x = 0;
			}
		}
	}

	protected virtual void resetIcons()
	{
		buffs.Clear();
		if (Game1.player == null)
		{
			return;
		}
		IDictionary<string, float> prevIconScales = new Dictionary<string, float>();
		foreach (KeyValuePair<ClickableTextureComponent, Buff> entry in buffs)
		{
			prevIconScales[entry.Value.id] = entry.Key.scale;
		}
		foreach (Buff buff in GetSortedBuffs())
		{
			if (!buff.visible)
			{
				continue;
			}
			bool isUpdated = updatedIDs.Contains(buff.id);
			foreach (ClickableTextureComponent icon in getClickableComponents(buff))
			{
				float scale;
				if (isUpdated)
				{
					icon.scale = icon.baseScale + 0.2f;
				}
				else if (prevIconScales.TryGetValue(buff.id, out scale))
				{
					icon.scale = Math.Max(icon.baseScale, scale);
				}
				buffs.Add(icon, buff);
			}
		}
		updatedIDs.Clear();
		arrangeTheseComponentsInThisRectangle(xPositionOnScreen, yPositionOnScreen, width / 64, 64, 64, 8, rightToLeft: true);
	}

	public new void update(GameTime time)
	{
		if (dirty)
		{
			resetIcons();
			dirty = false;
		}
		if (!Game1.wasMouseVisibleThisFrame)
		{
			hoverText = "";
		}
		foreach (KeyValuePair<ClickableTextureComponent, Buff> pair in buffs)
		{
			ClickableTextureComponent icon = pair.Key;
			Buff buff = pair.Value;
			icon.scale = Math.Max(icon.baseScale, icon.scale - 0.01f);
			if (!buff.alreadyUpdatedIconAlpha && (float)buff.millisecondsDuration < Math.Min(10000f, (float)buff.totalMillisecondsDuration / 10f) && buff.millisecondsDuration != -2)
			{
				buff.displayAlphaTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / (((float)buff.millisecondsDuration < Math.Min(2000f, (float)buff.totalMillisecondsDuration / 20f)) ? 1f : 2f);
				buff.alreadyUpdatedIconAlpha = true;
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		updatePosition();
		foreach (KeyValuePair<ClickableTextureComponent, Buff> pair in buffs)
		{
			pair.Key.draw(b, Color.White * ((pair.Value.displayAlphaTimer > 0f) ? ((float)(Math.Cos(pair.Value.displayAlphaTimer / 100f) + 3.0) / 4f) : 1f), 0.8f);
			pair.Value.alreadyUpdatedIconAlpha = false;
		}
		if (hoverText.Length != 0 && isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
		{
			performHoverAction(Game1.getOldMouseX(), Game1.getOldMouseY());
			IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
		}
	}

	public IEnumerable<Buff> GetSortedBuffs()
	{
		return from p in Game1.player.buffs.AppliedBuffs.Values
			orderby p.id == "food" descending, p.id == "drink" descending
			select p;
	}

	protected virtual string getDescription(Buff buff)
	{
		StringBuilder s = new StringBuilder();
		string displayName = buff.displayName;
		if (displayName != null && displayName.Length > 1)
		{
			s.AppendLine(buff.displayName);
			s.AppendLine("[line]");
		}
		string description2 = buff.description;
		if (description2 != null && description2.Length > 1)
		{
			s.AppendLine(buff.description);
		}
		foreach (BuffAttributeDisplay attribute in displayAttributes)
		{
			string description = getDescription(buff, attribute, withSource: false);
			if (description != null)
			{
				s.AppendLine(description);
			}
		}
		string source = getSourceLine(buff);
		if (source != null)
		{
			s.AppendLine(source);
		}
		return s.ToString().TrimEnd();
	}

	protected virtual string getDescription(Buff buff, BuffAttributeDisplay attribute, bool withSource)
	{
		float value = attribute.Value(buff);
		if (value == 0f)
		{
			return null;
		}
		string description = attribute.Description(value);
		if (withSource)
		{
			string source = getSourceLine(buff);
			if (source != null)
			{
				description = description + "\n" + source;
			}
		}
		return description;
	}

	protected virtual string getSourceLine(Buff buff)
	{
		string source = buff.displaySource ?? buff.source;
		if (string.IsNullOrWhiteSpace(source))
		{
			return null;
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.508") + source;
	}

	public virtual IEnumerable<ClickableTextureComponent> getClickableComponents(Buff buff)
	{
		if (!buff.visible)
		{
			yield break;
		}
		if (buff.iconTexture != null)
		{
			Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(buff.iconTexture, buff.iconSheetIndex, 16, 16);
			yield return new ClickableTextureComponent("", Rectangle.Empty, null, getDescription(buff), buff.iconTexture, sourceRect, 4f);
			yield break;
		}
		foreach (BuffAttributeDisplay attribute in displayAttributes)
		{
			string description = getDescription(buff, attribute, withSource: true);
			if (description != null)
			{
				Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(attribute.Texture(), attribute.SpriteIndex, 16, 16);
				yield return new ClickableTextureComponent("", Rectangle.Empty, null, description, attribute.Texture(), sourceRect, 4f);
			}
		}
	}
}
