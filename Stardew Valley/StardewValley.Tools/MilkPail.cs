using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Tools;

public class MilkPail : Tool
{
	[XmlIgnore]
	private readonly NetEvent0 finishEvent = new NetEvent0();

	/// <summary>The farm animal the milk pail is being used on, if any.</summary>
	[XmlIgnore]
	public FarmAnimal animal;

	public MilkPail()
		: base("Milk Pail", -1, 6, 6, stackable: false)
	{
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new MilkPail();
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(finishEvent, "finishEvent");
		finishEvent.onEvent += doFinish;
	}

	public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
	{
		x = (int)who.GetToolLocation().X;
		y = (int)who.GetToolLocation().Y;
		animal = Utility.GetBestHarvestableFarmAnimal(toolRect: new Rectangle(x - 32, y - 32, 64, 64), animals: location.animals.Values, tool: this);
		if (animal?.currentProduce.Value != null && animal.isAdult() && animal.CanGetProduceWithTool(this) && who.couldInventoryAcceptThisItem(animal.currentProduce, 1))
		{
			animal.doEmote(20);
			animal.friendshipTowardFarmer.Value = Math.Min(1000, (int)animal.friendshipTowardFarmer + 5);
			who.playNearbySoundLocal("Milking");
			animal.pauseTimer = 1500;
		}
		else if (animal?.currentProduce.Value != null && animal.isAdult())
		{
			if (who == Game1.player)
			{
				if (!animal.CanGetProduceWithTool(this))
				{
					string harvestTool = animal.GetAnimalData()?.HarvestTool;
					if (harvestTool != null)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14167", harvestTool));
					}
				}
				else if (!who.couldInventoryAcceptThisItem(animal.currentProduce, (!animal.hasEatenAnimalCracker.Value) ? 1 : 2))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
				}
			}
		}
		else if (who == Game1.player)
		{
			DelayedAction.playSoundAfterDelay("fishingRodBend", 300);
			DelayedAction.playSoundAfterDelay("fishingRodBend", 1200);
			string toSay = null;
			if (animal != null)
			{
				toSay = (animal.CanGetProduceWithTool(this) ? (animal.isBaby() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14176", animal.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14177", animal.displayName)) : Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14175", animal.displayName));
			}
			if (toSay != null)
			{
				DelayedAction.showDialogueAfterDelay(toSay, 1000);
			}
		}
		who.Halt();
		int g = who.FarmerSprite.CurrentFrame;
		who.FarmerSprite.animateOnce(287 + who.FacingDirection, 50f, 4);
		who.FarmerSprite.oldFrame = g;
		who.UsingTool = true;
		who.CanMove = false;
		return true;
	}

	public override void tickUpdate(GameTime time, Farmer who)
	{
		lastUser = who;
		base.tickUpdate(time, who);
		finishEvent.Poll();
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		base.DoFunction(location, x, y, power, who);
		who.Stamina -= 4f;
		base.CurrentParentTileIndex = 6;
		base.IndexOfMenuItemView = 6;
		if (animal?.currentProduce.Value != null && animal.isAdult() && animal.CanGetProduceWithTool(this))
		{
			Object produce = ItemRegistry.Create<Object>("(O)" + animal.currentProduce.Value);
			produce.CanBeSetDown = false;
			produce.Quality = animal.produceQuality.Value;
			if (animal.hasEatenAnimalCracker.Value)
			{
				produce.Stack = 2;
			}
			if (who.addItemToInventoryBool(produce))
			{
				Utility.RecordAnimalProduce(animal, animal.currentProduce);
				Game1.playSound("coin");
				animal.currentProduce.Value = null;
				animal.ReloadTextureIfNeeded();
				who.gainExperience(0, 5);
			}
		}
		finish();
	}

	private void finish()
	{
		finishEvent.Fire();
	}

	private void doFinish()
	{
		animal = null;
		lastUser.CanMove = true;
		lastUser.completelyStopAnimatingOrDoingAction();
		lastUser.UsingTool = false;
		lastUser.canReleaseTool = true;
	}
}
