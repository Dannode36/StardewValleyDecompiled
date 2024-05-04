using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus;

public class Bundle : ClickableComponent
{
	/// <summary>The index in the raw <c>Data/Bundles</c> data for the internal name.</summary>
	public const int NameIndex = 0;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the reward data.</summary>
	public const int RewardIndex = 1;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the items needed to complete the bundle.</summary>
	public const int IngredientsIndex = 2;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the bundle color.</summary>
	public const int ColorIndex = 3;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the optional number of slots to fill.</summary>
	public const int NumberOfSlotsIndex = 4;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the optional override texture name and sprite index.</summary>
	public const int SpriteIndex = 5;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the display name.</summary>
	public const int DisplayNameIndex = 6;

	/// <summary>The number of slash-delimited fields in the raw <c>Data/Bundles</c> data.</summary>
	public const int FieldCount = 7;

	public const float shakeRate = (float)Math.PI / 200f;

	public const float shakeDecayRate = 0.0030679617f;

	public const int Color_Green = 0;

	public const int Color_Purple = 1;

	public const int Color_Orange = 2;

	public const int Color_Yellow = 3;

	public const int Color_Red = 4;

	public const int Color_Blue = 5;

	public const int Color_Teal = 6;

	public const float DefaultShakeForce = (float)Math.PI * 3f / 128f;

	public string rewardDescription;

	public List<BundleIngredientDescription> ingredients;

	public int bundleColor;

	public int numberOfIngredientSlots;

	public int bundleIndex;

	public int completionTimer;

	public bool complete;

	public bool depositsAllowed = true;

	public Texture2D bundleTextureOverride;

	public int bundleTextureIndexOverride = -1;

	public TemporaryAnimatedSprite sprite;

	private float maxShake;

	private bool shakeLeft;

	public Bundle(string name, string displayName, List<BundleIngredientDescription> ingredients, bool[] completedIngredientsList, string rewardListString = "")
		: base(new Rectangle(0, 0, 64, 64), "")
	{
		base.name = name;
		label = displayName;
		rewardDescription = rewardListString;
		numberOfIngredientSlots = ingredients.Count;
		this.ingredients = ingredients;
	}

	public Bundle(int bundleIndex, string rawBundleInfo, bool[] completedIngredientsList, Point position, string textureName, JunimoNoteMenu menu)
		: base(new Rectangle(position.X, position.Y, 64, 64), "")
	{
		if (menu != null && menu.fromGameMenu)
		{
			depositsAllowed = false;
		}
		this.bundleIndex = bundleIndex;
		string[] split = rawBundleInfo.Split('/');
		name = split[0];
		label = split[6];
		rewardDescription = split[1];
		if (!string.IsNullOrWhiteSpace(split[5]))
		{
			try
			{
				string[] parts = split[5].Split(':', 2);
				if (parts.Length == 2)
				{
					bundleTextureOverride = Game1.content.Load<Texture2D>(parts[0]);
					bundleTextureIndexOverride = int.Parse(parts[1]);
				}
				else
				{
					bundleTextureIndexOverride = int.Parse(split[5]);
				}
			}
			catch
			{
				bundleTextureOverride = null;
				bundleTextureIndexOverride = -1;
			}
		}
		string[] ingredientsSplit = ArgUtility.SplitBySpace(split[2]);
		complete = true;
		ingredients = new List<BundleIngredientDescription>();
		int tally = 0;
		for (int i = 0; i < ingredientsSplit.Length; i += 3)
		{
			ingredients.Add(new BundleIngredientDescription(ingredientsSplit[i], Convert.ToInt32(ingredientsSplit[i + 1]), Convert.ToInt32(ingredientsSplit[i + 2]), completedIngredientsList[i / 3]));
			if (!completedIngredientsList[i / 3])
			{
				complete = false;
			}
			else
			{
				tally++;
			}
		}
		bundleColor = Convert.ToInt32(split[3]);
		numberOfIngredientSlots = ArgUtility.GetInt(split, 4, ingredients.Count);
		if (tally >= numberOfIngredientSlots)
		{
			complete = true;
		}
		sprite = new TemporaryAnimatedSprite(textureName, new Rectangle(bundleColor * 256 % 512, 244 + bundleColor * 256 / 512 * 16, 16, 16), 70f, 3, 99999, new Vector2(bounds.X, bounds.Y), flicker: false, flipped: false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f)
		{
			pingPong = true
		};
		sprite.paused = true;
		sprite.sourceRect.X += sprite.sourceRect.Width;
		if (name.ToLower().Contains(Game1.currentSeason) && !complete)
		{
			shake();
		}
		if (complete)
		{
			completionAnimation(menu, playSound: false);
		}
	}

	public Item getReward()
	{
		return Utility.getItemFromStandardTextDescription(rewardDescription, Game1.player);
	}

	public void shake(float force = (float)Math.PI * 3f / 128f)
	{
		if (sprite.paused)
		{
			maxShake = force;
		}
	}

	public void shake(int extraInfo)
	{
		maxShake = (float)Math.PI * 3f / 128f;
		if (extraInfo == 1)
		{
			Game1.playSound("leafrustle");
			TemporaryAnimatedSprite tempSprite = new TemporaryAnimatedSprite(50, sprite.position, getColorFromColorIndex(bundleColor))
			{
				motion = new Vector2(-1f, 0.5f),
				acceleration = new Vector2(0f, 0.02f)
			};
			tempSprite.sourceRect.Y++;
			tempSprite.sourceRect.Height--;
			JunimoNoteMenu.tempSprites.Add(tempSprite);
			tempSprite = new TemporaryAnimatedSprite(50, sprite.position, getColorFromColorIndex(bundleColor))
			{
				motion = new Vector2(1f, 0.5f),
				acceleration = new Vector2(0f, 0.02f),
				flipped = true,
				delayBeforeAnimationStart = 50
			};
			tempSprite.sourceRect.Y++;
			tempSprite.sourceRect.Height--;
			JunimoNoteMenu.tempSprites.Add(tempSprite);
		}
	}

	public void tryHoverAction(int x, int y)
	{
		if (bounds.Contains(x, y) && !complete)
		{
			sprite.paused = false;
			JunimoNoteMenu.hoverText = Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", label);
		}
		else if (!complete)
		{
			sprite.reset();
			sprite.sourceRect.X += sprite.sourceRect.Width;
			sprite.paused = true;
		}
	}

	public bool IsValidItemForThisIngredientDescription(Item item, BundleIngredientDescription ingredient)
	{
		if (item == null || ingredient.completed || ingredient.quality > item.Quality)
		{
			return false;
		}
		if (ingredient.preservesId != null)
		{
			if (ItemQueryResolver.TryResolve("FLAVORED_ITEM " + ingredient.id + " " + ingredient.preservesId, new ItemQueryContext(Game1.currentLocation, Game1.player, Game1.random)).FirstOrDefault()?.Item is Object obj && item.QualifiedItemId == obj.QualifiedItemId && obj.preservedParentSheetIndex.Contains(ingredient.preservesId))
			{
				return true;
			}
			return false;
		}
		if (ingredient.category.HasValue)
		{
			if (item.QualifiedItemId == "(O)107" && ingredient.category == -5)
			{
				return true;
			}
			return item.Category == ingredient.category;
		}
		return ItemRegistry.HasItemId(item, ingredient.id);
	}

	public int GetBundleIngredientDescriptionIndexForItem(Item item)
	{
		for (int i = 0; i < ingredients.Count; i++)
		{
			if (IsValidItemForThisIngredientDescription(item, ingredients[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public bool canAcceptThisItem(Item item, ClickableTextureComponent slot)
	{
		return canAcceptThisItem(item, slot, ignore_stack_count: false);
	}

	public bool canAcceptThisItem(Item item, ClickableTextureComponent slot, bool ignore_stack_count = false)
	{
		if (!depositsAllowed)
		{
			return false;
		}
		for (int i = 0; i < ingredients.Count; i++)
		{
			if (IsValidItemForThisIngredientDescription(item, ingredients[i]) && (ignore_stack_count || ingredients[i].stack <= item.Stack) && (slot == null || slot.item == null))
			{
				return true;
			}
		}
		return false;
	}

	public Item tryToDepositThisItem(Item item, ClickableTextureComponent slot, string noteTextureName, JunimoNoteMenu parentMenu)
	{
		if (!depositsAllowed)
		{
			if (Game1.player.hasCompletedCommunityCenter())
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtAJM"));
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtCC"));
			}
			return item;
		}
		CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
		for (int i = 0; i < ingredients.Count; i++)
		{
			BundleIngredientDescription ingredient = ingredients[i];
			if (IsValidItemForThisIngredientDescription(item, ingredient) && slot.item == null)
			{
				item.Stack -= ingredient.stack;
				ingredient = (ingredients[i] = new BundleIngredientDescription(ingredient, completed: true));
				ingredientDepositAnimation(slot, noteTextureName);
				string id = JunimoNoteMenu.GetRepresentativeItemId(ingredient);
				if (ingredient.preservesId != null)
				{
					slot.item = Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack);
				}
				else
				{
					slot.item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality);
				}
				Game1.playSound("newArtifact");
				slot.sourceRect.X = 512;
				slot.sourceRect.Y = 244;
				if (parentMenu.onIngredientDeposit != null)
				{
					parentMenu.onIngredientDeposit(i);
					break;
				}
				communityCenter.bundles.FieldDict[bundleIndex][i] = true;
				Game1.multiplayer.globalChatInfoMessage("BundleDonate", Game1.player.displayName, TokenStringBuilder.ItemName(slot.item.QualifiedItemId));
				break;
			}
		}
		if (item.Stack > 0)
		{
			return item;
		}
		return null;
	}

	public void ingredientDepositAnimation(ClickableTextureComponent slot, string noteTextureName, bool skipAnimation = false)
	{
		TemporaryAnimatedSprite t = new TemporaryAnimatedSprite(noteTextureName, new Rectangle(530, 244, 18, 18), 50f, 6, 1, new Vector2(slot.bounds.X, slot.bounds.Y), flicker: false, flipped: false, 0.88f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			holdLastFrame = true,
			endSound = "cowboy_monsterhit"
		};
		if (skipAnimation)
		{
			t.sourceRect.Offset(t.sourceRect.Width * 5, 0);
			t.sourceRectStartingPos = new Vector2(t.sourceRect.X, t.sourceRect.Y);
			t.animationLength = 1;
		}
		JunimoNoteMenu.tempSprites.Add(t);
	}

	public bool canBeClicked()
	{
		return !complete;
	}

	public void completionAnimation(JunimoNoteMenu menu, bool playSound = true, int delay = 0)
	{
		if (delay <= 0)
		{
			completionAnimation(playSound);
		}
		else
		{
			completionTimer = delay;
		}
	}

	private void completionAnimation(bool playSound = true)
	{
		if (Game1.activeClickableMenu is JunimoNoteMenu junimoNoteMenu)
		{
			junimoNoteMenu.takeDownBundleSpecificPage();
		}
		sprite.pingPong = false;
		sprite.paused = false;
		sprite.sourceRect.X = (int)sprite.sourceRectStartingPos.X;
		sprite.sourceRect.X += sprite.sourceRect.Width;
		sprite.animationLength = 15;
		sprite.interval = 50f;
		sprite.totalNumberOfLoops = 0;
		sprite.holdLastFrame = true;
		sprite.endFunction = shake;
		sprite.extraInfoForEndBehavior = 1;
		if (complete)
		{
			sprite.sourceRect.X += sprite.sourceRect.Width * 14;
			sprite.sourceRectStartingPos = new Vector2(sprite.sourceRect.X, sprite.sourceRect.Y);
			sprite.currentParentTileIndex = 14;
			sprite.interval = 0f;
			sprite.animationLength = 1;
			sprite.extraInfoForEndBehavior = 0;
		}
		else
		{
			if (playSound)
			{
				Game1.playSound("dwop");
			}
			bounds.Inflate(64, 64);
			JunimoNoteMenu.tempSprites.AddRange(Utility.sparkleWithinArea(bounds, 8, getColorFromColorIndex(bundleColor) * 0.5f));
			bounds.Inflate(-64, -64);
		}
		complete = true;
	}

	public void update(GameTime time)
	{
		sprite.update(time);
		if (completionTimer > 0 && JunimoNoteMenu.screenSwipe == null)
		{
			completionTimer -= time.ElapsedGameTime.Milliseconds;
			if (completionTimer <= 0)
			{
				completionAnimation();
			}
		}
		if (Game1.random.NextDouble() < 0.005 && (complete || name.ToLower().Contains(Game1.currentSeason)))
		{
			shake();
		}
		if (maxShake > 0f)
		{
			if (shakeLeft)
			{
				sprite.rotation -= (float)Math.PI / 200f;
				if (sprite.rotation <= 0f - maxShake)
				{
					shakeLeft = false;
				}
			}
			else
			{
				sprite.rotation += (float)Math.PI / 200f;
				if (sprite.rotation >= maxShake)
				{
					shakeLeft = true;
				}
			}
		}
		if (maxShake > 0f)
		{
			maxShake = Math.Max(0f, maxShake - 0.0007669904f);
		}
	}

	public void draw(SpriteBatch b)
	{
		sprite.draw(b, localPosition: true);
	}

	public static Color getColorFromColorIndex(int color)
	{
		return color switch
		{
			5 => Color.LightBlue, 
			0 => Color.Lime, 
			2 => Color.Orange, 
			1 => Color.DeepPink, 
			4 => Color.Red, 
			6 => Color.Cyan, 
			3 => Color.Orange, 
			_ => Color.Lime, 
		};
	}
}
