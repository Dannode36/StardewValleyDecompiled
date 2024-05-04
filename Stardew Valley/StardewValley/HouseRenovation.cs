using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.HomeRenovations;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewValley;

public class HouseRenovation : ISalable, IHaveItemTypeId
{
	public enum AnimationType
	{
		Build,
		Destroy
	}

	protected string _displayName;

	protected string _name;

	protected string _description;

	public AnimationType animationType;

	public List<List<Rectangle>> renovationBounds = new List<List<Rectangle>>();

	public string placementText = "";

	public GameLocation location;

	public bool requireClearance = true;

	public Action<HouseRenovation, int> onRenovation;

	public Func<HouseRenovation, int, bool> validate;

	/// <inheritdoc cref="F:StardewValley.GameData.HomeRenovations.HomeRenovation.Price" />
	public int Price;

	/// <inheritdoc cref="F:StardewValley.GameData.HomeRenovations.HomeRenovation.RoomId" />
	public string RoomId;

	/// <inheritdoc />
	public string TypeDefinitionId => "(Salable)";

	/// <inheritdoc />
	public string QualifiedItemId => TypeDefinitionId + "HouseRenovation";

	public string DisplayName => _displayName;

	public string Name => _name;

	public bool IsRecipe
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public int Stack
	{
		get
		{
			return 1;
		}
		set
		{
		}
	}

	public int Quality
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public bool ShouldDrawIcon()
	{
		return false;
	}

	public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
	}

	public string getDescription()
	{
		return _description;
	}

	public int maximumStackSize()
	{
		return 1;
	}

	public int addToStack(Item stack)
	{
		return 0;
	}

	/// <inheritdoc />
	public int sellToStorePrice(long specificPlayerID = -1L)
	{
		return -1;
	}

	/// <inheritdoc />
	public int salePrice(bool ignoreProfitMargins = false)
	{
		if (Price <= 0)
		{
			return 0;
		}
		return Price;
	}

	/// <inheritdoc />
	public bool appliesProfitMargins()
	{
		return false;
	}

	/// <inheritdoc />
	public bool actionWhenPurchased(string shopId)
	{
		return false;
	}

	public bool canStackWith(ISalable other)
	{
		return false;
	}

	public bool CanBuyItem(Farmer farmer)
	{
		return true;
	}

	public bool IsInfiniteStock()
	{
		return true;
	}

	public ISalable GetSalableInstance()
	{
		return this;
	}

	/// <inheritdoc />
	public void FixStackSize()
	{
	}

	/// <inheritdoc />
	public void FixQuality()
	{
	}

	/// <inheritdoc />
	public string GetItemTypeId()
	{
		return TypeDefinitionId;
	}

	public static void ShowRenovationMenu()
	{
		Game1.activeClickableMenu = new ShopMenu("HouseRenovations", GetAvailableRenovations(), 0, null, OnPurchaseRenovation)
		{
			purchaseSound = null
		};
	}

	public static List<ISalable> GetAvailableRenovations()
	{
		FarmHouse farmhouse = Game1.RequireLocation<FarmHouse>(Game1.player.homeLocation);
		List<ISalable> available_renovations = new List<ISalable>();
		Dictionary<string, HomeRenovation> data = DataLoader.HomeRenovations(Game1.content);
		foreach (string key in data.Keys)
		{
			HomeRenovation renovation_data = data[key];
			bool valid = true;
			foreach (RenovationValue requirement_data in renovation_data.Requirements)
			{
				if (requirement_data.Type == "Value")
				{
					string requirement_value = requirement_data.Value;
					bool match = true;
					if (requirement_value.Length > 0 && requirement_value[0] == '!')
					{
						requirement_value = requirement_value.Substring(1);
						match = false;
					}
					int value = int.Parse(requirement_value);
					try
					{
						NetInt field = (NetInt)farmhouse.GetType().GetField(requirement_data.Key).GetValue(farmhouse);
						if (field == null)
						{
							valid = false;
							break;
						}
						if (field.Value == value != match)
						{
							valid = false;
							break;
						}
					}
					catch (Exception)
					{
						valid = false;
						break;
					}
				}
				else if (requirement_data.Type == "Mail" && Game1.player.hasOrWillReceiveMail(requirement_data.Key) != (requirement_data.Value == "1"))
				{
					valid = false;
					break;
				}
			}
			if (!valid)
			{
				continue;
			}
			HouseRenovation renovation = new HouseRenovation
			{
				location = farmhouse,
				_name = key
			};
			string[] split = Game1.content.LoadString(renovation_data.TextStrings).Split('/');
			try
			{
				renovation._displayName = split[0];
				renovation._description = split[1];
				renovation.placementText = split[2];
			}
			catch (Exception)
			{
				renovation._displayName = "?";
				renovation._description = "?";
				renovation.placementText = "?";
			}
			if (renovation_data.CheckForObstructions)
			{
				renovation.validate = (Func<HouseRenovation, int, bool>)Delegate.Combine(renovation.validate, new Func<HouseRenovation, int, bool>(EnsureNoObstructions));
			}
			if (renovation_data.AnimationType == "destroy")
			{
				renovation.animationType = AnimationType.Destroy;
			}
			else
			{
				renovation.animationType = AnimationType.Build;
			}
			renovation.Price = renovation_data.Price;
			renovation.RoomId = ((!string.IsNullOrEmpty(renovation_data.RoomId)) ? renovation_data.RoomId : key);
			if (!string.IsNullOrEmpty(renovation_data.SpecialRect))
			{
				if (renovation_data.SpecialRect == "crib")
				{
					Rectangle? crib_bounds = farmhouse.GetCribBounds();
					if (!farmhouse.CanModifyCrib() || !crib_bounds.HasValue)
					{
						continue;
					}
					renovation.AddRenovationBound(crib_bounds.Value);
				}
			}
			else
			{
				foreach (RectGroup rectGroup in renovation_data.RectGroups)
				{
					List<Rectangle> rectangles = new List<Rectangle>();
					foreach (Rect rect in rectGroup.Rects)
					{
						Rectangle rectangle = default(Rectangle);
						rectangle.X = rect.X;
						rectangle.Y = rect.Y;
						rectangle.Width = rect.Width;
						rectangle.Height = rect.Height;
						rectangles.Add(rectangle);
					}
					renovation.AddRenovationBound(rectangles);
				}
			}
			foreach (RenovationValue renovateAction in renovation_data.RenovateActions)
			{
				RenovationValue action_data = renovateAction;
				if (action_data.Type == "Value")
				{
					try
					{
						NetInt field = (NetInt)farmhouse.GetType().GetField(action_data.Key).GetValue(farmhouse);
						if (field == null)
						{
							valid = false;
							break;
						}
						renovation.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(renovation.onRenovation, new Action<HouseRenovation, int>(OnRenovation));
						void OnRenovation(HouseRenovation selectedRenovation, int index)
						{
							if (action_data.Value == "selected")
							{
								field.Value = index;
							}
							else
							{
								int value = int.Parse(action_data.Value);
								field.Value = value;
							}
						}
					}
					catch (Exception)
					{
						valid = false;
						break;
					}
				}
				else if (action_data.Type == "Mail")
				{
					renovation.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(renovation.onRenovation, new Action<HouseRenovation, int>(OnRenovation));
				}
				void OnRenovation(HouseRenovation selectedRenovation, int index)
				{
					if (action_data.Value == "0")
					{
						Game1.player.mailReceived.Remove(action_data.Key);
					}
					else
					{
						Game1.player.mailReceived.Add(action_data.Key);
					}
				}
			}
			if (valid)
			{
				renovation.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(renovation.onRenovation, (Action<HouseRenovation, int>)delegate
				{
					farmhouse.UpdateForRenovation();
				});
				available_renovations.Add(renovation);
			}
		}
		return available_renovations;
	}

	public static bool EnsureNoObstructions(HouseRenovation renovation, int selected_index)
	{
		if (renovation.location != null)
		{
			foreach (Rectangle rectangle in renovation.renovationBounds[selected_index])
			{
				foreach (Vector2 tile in rectangle.GetVectors())
				{
					if (renovation.location.isTileOccupiedByFarmer(tile) != null)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"));
						return false;
					}
					if (renovation.location.IsTileOccupiedBy(tile))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"));
						return false;
					}
				}
				Rectangle world_box = new Rectangle(rectangle.X * 64, rectangle.Y * 64, rectangle.Width * 64, rectangle.Height * 64);
				if (!(renovation.location is DecoratableLocation decoratable_location))
				{
					continue;
				}
				foreach (Furniture item in decoratable_location.furniture)
				{
					if (item.GetBoundingBox().Intersects(world_box))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"));
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	public static void BuildCrib(HouseRenovation renovation, int selected_index)
	{
		if (renovation.location is FarmHouse farm_house)
		{
			farm_house.cribStyle.Value = 1;
		}
	}

	public static void RemoveCrib(HouseRenovation renovation, int selected_index)
	{
		if (renovation.location is FarmHouse farm_house)
		{
			farm_house.cribStyle.Value = 0;
		}
	}

	public static void OpenBedroom(HouseRenovation renovation, int selected_index)
	{
		if (renovation.location is FarmHouse farm_house)
		{
			Game1.player.mailReceived.Add("renovation_bedroom_open");
			farm_house.UpdateForRenovation();
		}
	}

	public static void CloseBedroom(HouseRenovation renovation, int selected_index)
	{
		if (renovation.location is FarmHouse farm_house)
		{
			Game1.player.mailReceived.Remove("renovation_bedroom_open");
			farm_house.UpdateForRenovation();
		}
	}

	public static void OpenSouthernRoom(HouseRenovation renovation, int selected_index)
	{
		if (renovation.location is FarmHouse farm_house)
		{
			Game1.player.mailReceived.Add("renovation_southern_open");
			farm_house.UpdateForRenovation();
		}
	}

	public static void CloseSouthernRoom(HouseRenovation renovation, int selected_index)
	{
		if (renovation.location is FarmHouse farm_house)
		{
			Game1.player.mailReceived.Remove("renovation_southern_open");
			farm_house.UpdateForRenovation();
		}
	}

	public static void OpenCornernRoom(HouseRenovation renovation, int selected_index)
	{
		if (renovation.location is FarmHouse farm_house)
		{
			Game1.player.mailReceived.Add("renovation_corner_open");
			farm_house.UpdateForRenovation();
		}
	}

	public static void CloseCornerRoom(HouseRenovation renovation, int selected_index)
	{
		if (renovation.location is FarmHouse farm_house)
		{
			Game1.player.mailReceived.Remove("renovation_corner_open");
			farm_house.UpdateForRenovation();
		}
	}

	public static bool OnPurchaseRenovation(ISalable salable, Farmer who, int amount)
	{
		if (salable is HouseRenovation renovation)
		{
			who._money += salable.salePrice();
			Game1.activeClickableMenu = new RenovateMenu(renovation);
			return true;
		}
		return false;
	}

	public virtual void AddRenovationBound(Rectangle bound)
	{
		List<Rectangle> bounds = new List<Rectangle>();
		bounds.Add(bound);
		renovationBounds.Add(bounds);
	}

	public virtual void AddRenovationBound(List<Rectangle> bounds)
	{
		renovationBounds.Add(bounds);
	}
}
