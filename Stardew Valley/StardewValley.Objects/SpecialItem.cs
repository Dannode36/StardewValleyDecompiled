using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Objects;

public class SpecialItem : Item
{
	public const int skullKey = 4;

	public const int clubCard = 2;

	public const int specialCharm = 3;

	public const int backpack = 99;

	public const int magnifyingGlass = 5;

	public const int darkTalisman = 6;

	public const int magicInk = 7;

	[XmlElement("which")]
	public readonly NetInt which = new NetInt();

	/// <summary>The backing field for <see cref="P:StardewValley.Objects.SpecialItem.displayName" />.</summary>
	[XmlIgnore]
	private string _displayName;

	/// <inheritdoc />
	public override string TypeDefinitionId { get; } = "(O)";


	/// <summary>The cached value for <see cref="P:StardewValley.Objects.SpecialItem.DisplayName" />.</summary>
	[XmlIgnore]
	private string displayName
	{
		get
		{
			if (string.IsNullOrEmpty(_displayName))
			{
				switch (which)
				{
				case 4L:
					_displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13088");
					break;
				case 2L:
					_displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13089");
					break;
				case 3L:
					_displayName = Game1.content.LoadString("Strings\\Objects:SpecialCharm");
					break;
				case 6L:
					_displayName = Game1.content.LoadString("Strings\\Objects:DarkTalisman");
					break;
				case 7L:
					_displayName = Game1.content.LoadString("Strings\\Objects:MagicInk");
					break;
				case 5L:
					_displayName = Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
					break;
				case 99L:
					if ((int)Game1.player.maxItems == 36)
					{
						_displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709");
					}
					else
					{
						_displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708");
					}
					break;
				}
			}
			return _displayName;
		}
		set
		{
			if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(_displayName))
			{
				switch (which)
				{
				case 4L:
					_displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13088");
					break;
				case 2L:
					_displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13089");
					break;
				case 3L:
					_displayName = Game1.content.LoadString("Strings\\Objects:SpecialCharm");
					break;
				case 6L:
					_displayName = Game1.content.LoadString("Strings\\Objects:DarkTalisman");
					break;
				case 5L:
					_displayName = Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
					break;
				case 7L:
					_displayName = Game1.content.LoadString("Strings\\Objects:MagicInk");
					break;
				case 99L:
					if ((int)Game1.player.maxItems == 36)
					{
						_displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709");
					}
					else
					{
						_displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708");
					}
					break;
				}
			}
			else
			{
				_displayName = value;
			}
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override string DisplayName => displayName;

	/// <inheritdoc />
	[XmlIgnore]
	public override string Name
	{
		get
		{
			if (netName.Value.Length < 1)
			{
				switch (which)
				{
				case 4L:
					return "Skull Key";
				case 2L:
					return "Club Card";
				case 6L:
					return Game1.content.LoadString("Strings\\Objects:DarkTalisman");
				case 7L:
					return Game1.content.LoadString("Strings\\Objects:MagicInk");
				case 5L:
					return Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
				case 3L:
					return Game1.content.LoadString("Strings\\Objects:SpecialCharm");
				}
			}
			return netName;
		}
		set
		{
			netName.Value = value;
		}
	}

	public SpecialItem()
	{
		which.Value = which;
		if (netName.Value == null || Name.Length < 1)
		{
			switch (which)
			{
			case 4L:
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13088");
				break;
			case 2L:
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13089");
				break;
			case 6L:
				displayName = Game1.content.LoadString("Strings\\Objects:DarkTalisman");
				break;
			case 7L:
				displayName = Game1.content.LoadString("Strings\\Objects:MagicInk");
				break;
			case 5L:
				displayName = Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
				break;
			case 3L:
				displayName = Game1.content.LoadString("Strings\\Objects:SpecialCharm");
				break;
			}
		}
	}

	public SpecialItem(int which, string name = "")
		: this()
	{
		this.which.Value = which;
		Name = name;
		if (name.Length < 1)
		{
			switch (which)
			{
			case 4:
				Name = "Skull Key";
				break;
			case 2:
				Name = "Club Card";
				break;
			case 6:
				Name = Game1.content.LoadString("Strings\\Objects:DarkTalisman");
				break;
			case 7:
				Name = Game1.content.LoadString("Strings\\Objects:MagicInk");
				break;
			case 5:
				Name = Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
				break;
			case 3:
				Name = Game1.content.LoadString("Strings\\Objects:SpecialCharm");
				break;
			}
		}
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(which, "which");
	}

	public void actionWhenReceived(Farmer who)
	{
		switch (which)
		{
		case 4L:
			who.hasSkullKey = true;
			who.addQuest("19");
			break;
		case 6L:
			who.hasDarkTalisman = true;
			break;
		case 7L:
			who.hasMagicInk = true;
			break;
		case 5L:
			who.hasMagnifyingGlass = true;
			break;
		case 3L:
			who.hasSpecialCharm = true;
			break;
		}
	}

	public TemporaryAnimatedSprite getTemporarySpriteForHoldingUp(Vector2 position)
	{
		if ((int)which == 99)
		{
			return new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(((int)Game1.player.maxItems == 36) ? 268 : 257, 1436, ((int)Game1.player.maxItems == 36) ? 11 : 9, 13), position + new Vector2(16f, 0f), flipped: false, 0f, Color.White)
			{
				scale = 4f,
				layerDepth = 1f
			};
		}
		return new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(129 + 16 * (int)which, 320, 16, 16), position, flipped: false, 0f, Color.White)
		{
			layerDepth = 1f
		};
	}

	public override string checkForSpecialItemHoldUpMeessage()
	{
		switch (which)
		{
		case 2L:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13090", displayName);
		case 4L:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13092", displayName);
		case 6L:
			return Game1.content.LoadString("Strings\\Objects:DarkTalismanDescription", displayName);
		case 7L:
			return Game1.content.LoadString("Strings\\Objects:MagicInkDescription", displayName);
		case 5L:
			return Game1.content.LoadString("Strings\\Objects:MagnifyingGlassDescription", displayName);
		case 3L:
			return Game1.content.LoadString("Strings\\Objects:SpecialCharmDescription", displayName);
		default:
			if ((int)which == 99)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13094", displayName, Game1.player.maxItems);
			}
			return base.checkForSpecialItemHoldUpMeessage();
		}
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	public override string getDescription()
	{
		return null;
	}

	public override bool isPlaceable()
	{
		return false;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		throw new NotImplementedException();
	}
}
