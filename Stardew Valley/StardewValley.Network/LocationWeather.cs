using System;
using Netcode;
using StardewValley.GameData.LocationContexts;

namespace StardewValley.Network;

public class LocationWeather : INetObject<NetFields>
{
	public readonly NetString weatherForTomorrow = new NetString();

	public readonly NetString weather = new NetString();

	public readonly NetBool isRaining = new NetBool();

	public readonly NetBool isSnowing = new NetBool();

	public readonly NetBool isLightning = new NetBool();

	public readonly NetBool isDebrisWeather = new NetBool();

	public readonly NetBool isGreenRain = new NetBool();

	public readonly NetInt monthlyNonRainyDayCount = new NetInt();

	public NetFields NetFields { get; } = new NetFields("LocationWeather");


	public string WeatherForTomorrow
	{
		get
		{
			return weatherForTomorrow.Value;
		}
		set
		{
			weatherForTomorrow.Value = value;
		}
	}

	public string Weather
	{
		get
		{
			return weather.Value;
		}
		set
		{
			weather.Value = value;
		}
	}

	public bool IsRaining
	{
		get
		{
			return isRaining.Value;
		}
		set
		{
			isRaining.Value = value;
		}
	}

	public bool IsSnowing
	{
		get
		{
			return isSnowing.Value;
		}
		set
		{
			isSnowing.Value = value;
		}
	}

	public bool IsLightning
	{
		get
		{
			return isLightning.Value;
		}
		set
		{
			isLightning.Value = value;
		}
	}

	public bool IsDebrisWeather
	{
		get
		{
			return isDebrisWeather.Value;
		}
		set
		{
			isDebrisWeather.Value = value;
		}
	}

	public bool IsGreenRain
	{
		get
		{
			return isGreenRain.Value;
		}
		set
		{
			isGreenRain.Value = value;
			if (value)
			{
				IsRaining = true;
			}
		}
	}

	public LocationWeather()
	{
		NetFields.SetOwner(this).AddField(weatherForTomorrow, "weatherForTomorrow").AddField(weather, "weather")
			.AddField(isRaining, "isRaining")
			.AddField(isSnowing, "isSnowing")
			.AddField(isLightning, "isLightning")
			.AddField(isDebrisWeather, "isDebrisWeather")
			.AddField(isGreenRain, "isGreenRain")
			.AddField(monthlyNonRainyDayCount, "monthlyNonRainyDayCount");
	}

	public void InitializeDayWeather()
	{
		Weather = WeatherForTomorrow;
		IsRaining = false;
		IsSnowing = false;
		IsLightning = false;
		IsDebrisWeather = false;
		IsGreenRain = false;
	}

	public void UpdateDailyWeather(string locationContextId, LocationContextData data, Random random)
	{
		InitializeDayWeather();
		switch (WeatherForTomorrow)
		{
		case "Rain":
			IsRaining = true;
			break;
		case "GreenRain":
			IsGreenRain = true;
			break;
		case "Storm":
			IsRaining = true;
			IsLightning = true;
			break;
		case "Wind":
			IsDebrisWeather = true;
			break;
		case "Snow":
			IsSnowing = true;
			break;
		}
		WeatherForTomorrow = "Sun";
		WorldDate tomorrow = new WorldDate(Game1.Date);
		tomorrow.TotalDays++;
		if (Utility.isFestivalDay(tomorrow.DayOfMonth, tomorrow.Season, locationContextId))
		{
			WeatherForTomorrow = "Festival";
			return;
		}
		if (Utility.TryGetPassiveFestivalDataForDay(tomorrow.DayOfMonth, tomorrow.Season, locationContextId, out var _, out var _))
		{
			WeatherForTomorrow = "Sun";
			return;
		}
		foreach (WeatherCondition weatherCondition in data.WeatherConditions)
		{
			if (GameStateQuery.CheckConditions(weatherCondition.Condition, null, null, null, null, random))
			{
				WeatherForTomorrow = weatherCondition.Weather;
				break;
			}
		}
	}

	public void CopyFrom(LocationWeather other)
	{
		Weather = other.Weather;
		IsRaining = other.IsRaining;
		IsSnowing = other.IsSnowing;
		IsLightning = other.IsLightning;
		IsDebrisWeather = other.IsDebrisWeather;
		IsGreenRain = other.IsGreenRain;
		WeatherForTomorrow = other.WeatherForTomorrow;
		monthlyNonRainyDayCount.Value = other.monthlyNonRainyDayCount.Value;
		if (Weather == null)
		{
			if (IsLightning)
			{
				Weather = "Storm";
			}
			else if (IsRaining)
			{
				Weather = "Rain";
			}
			else if (IsSnowing)
			{
				Weather = "Snow";
			}
			else if (IsDebrisWeather)
			{
				Weather = "Wind";
			}
			else
			{
				Weather = "Sun";
			}
		}
	}
}
