namespace StardewValley.Objects;

public class RainbowHairTrinketEffect : TrinketEffect
{
	public RainbowHairTrinketEffect(Trinket trinket)
		: base(trinket)
	{
	}

	public override void Apply(Farmer farmer)
	{
		farmer.prismaticHair.Value = true;
	}

	public override void Unapply(Farmer farmer)
	{
		farmer.prismaticHair.Value = false;
	}
}
