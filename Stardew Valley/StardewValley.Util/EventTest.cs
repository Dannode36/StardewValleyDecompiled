using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Menus;

namespace StardewValley.Util;

public class EventTest
{
	private int currentEventIndex;

	private int currentLocationIndex;

	private int aButtonTimer;

	private List<string> specificEventsToDo = new List<string>();

	private bool doingSpecifics;

	public EventTest(string startingLocationName = "", int startingEventIndex = 0)
	{
		currentLocationIndex = 0;
		if (startingLocationName.Length > 0)
		{
			for (int i = 0; i < Game1.locations.Count; i++)
			{
				if (Game1.locations[i].Name.Equals(startingLocationName))
				{
					currentLocationIndex = i;
					break;
				}
			}
		}
		currentEventIndex = startingEventIndex;
	}

	public EventTest(string[] whichEvents)
	{
		for (int i = 1; i < whichEvents.Length; i += 2)
		{
			specificEventsToDo.Add(whichEvents[i] + " " + whichEvents[i + 1]);
		}
		doingSpecifics = true;
		currentLocationIndex = -1;
	}

	public void update()
	{
		if (!Game1.eventUp && !Game1.fadeToBlack)
		{
			if (currentLocationIndex >= Game1.locations.Count)
			{
				return;
			}
			if (doingSpecifics && currentLocationIndex == -1)
			{
				if (specificEventsToDo.Count == 0)
				{
					return;
				}
				for (int i = 0; i < Game1.locations.Count; i++)
				{
					string lastEvent = specificEventsToDo.Last();
					string[] lastEventParts = ArgUtility.SplitBySpace(lastEvent);
					if (!Game1.locations[i].Name.Equals(lastEventParts[0]))
					{
						continue;
					}
					currentLocationIndex = i;
					int j = -1;
					foreach (KeyValuePair<string, string> pair in Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.locations[i].Name))
					{
						j++;
						if (int.TryParse(pair.Key.Split('/')[0], out var result) && result == Convert.ToInt32(lastEventParts[1]))
						{
							currentEventIndex = j;
							break;
						}
					}
					specificEventsToDo.Remove(lastEvent);
					break;
				}
			}
			GameLocation l = Game1.locations[currentLocationIndex];
			if (l.currentEvent != null)
			{
				return;
			}
			string locationName = l.name;
			if (locationName == "Pool")
			{
				locationName = "BathHouse_Pool";
			}
			bool exists = true;
			Dictionary<string, string> data = null;
			try
			{
				data = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName);
			}
			catch (Exception)
			{
				exists = false;
			}
			if (exists && currentEventIndex < data.Count)
			{
				KeyValuePair<string, string> entry = data.ElementAt(currentEventIndex);
				string key = entry.Key;
				string script = entry.Value;
				if (key.Contains('/') && !script.Equals("null"))
				{
					if (Game1.currentLocation.Name.Equals(locationName))
					{
						Game1.eventUp = true;
						Game1.currentLocation.currentEvent = new Event(script);
					}
					else
					{
						LocationRequest locationRequest = Game1.getLocationRequest(locationName);
						locationRequest.OnLoad += delegate
						{
							Game1.currentLocation.currentEvent = new Event(script);
						};
						Game1.warpFarmer(locationRequest, 8, 8, Game1.player.FacingDirection);
					}
				}
			}
			currentEventIndex++;
			if (!exists || currentEventIndex >= data.Count)
			{
				currentEventIndex = 0;
				currentLocationIndex++;
			}
			if (doingSpecifics)
			{
				currentLocationIndex = -1;
			}
			return;
		}
		aButtonTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
		if (aButtonTimer < 0)
		{
			aButtonTimer = 100;
			if (Game1.activeClickableMenu is DialogueBox dialogueBox)
			{
				dialogueBox.performHoverAction(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 64 - Game1.random.Next(300));
				dialogueBox.receiveLeftClick(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 64 - Game1.random.Next(300));
			}
		}
	}
}
