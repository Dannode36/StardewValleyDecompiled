using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

[InstanceStatics]
public class Phone : Object
{
	/// <summary>The methods which handle incoming phone calls.</summary>
	public static List<IPhoneHandler> PhoneHandlers = new List<IPhoneHandler>
	{
		new DefaultPhoneHandler()
	};

	/// <summary>While the phone is ringing, how long each ring sound should last in milliseconds.</summary>
	public const int RING_DURATION = 600;

	/// <summary>While the phone is ringing, the delay between each ring sound in milliseconds.</summary>
	public const int RING_CYCLE_TIME = 1800;

	public static Random r;

	protected static bool _phoneSoundPlaying = false;

	public static int ringingTimer;

	public static string whichPhoneCall = null;

	public static long lastRunTick = -1L;

	public static long lastMinutesElapsedTick = -1L;

	public static int intervalsToRing = 0;

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	public Phone()
	{
	}

	public Phone(Vector2 position)
		: base(position, "214")
	{
		Name = "Telephone";
		type.Value = "Crafting";
		bigCraftable.Value = true;
		canBeSetDown.Value = true;
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		string callId = whichPhoneCall;
		StopRinging();
		if (callId == null)
		{
			Game1.game1.ShowTelephoneMenu();
		}
		else if (!HandleIncomingCall(callId))
		{
			HangUp();
		}
		return true;
	}

	/// <summary>Handle an incoming phone call when the player interacts with the phone, if applicable.</summary>
	/// <param name="callId">The unique ID for the incoming call.</param>
	/// <remarks>For custom calls, add a new handler to <see cref="F:StardewValley.Objects.Phone.PhoneHandlers" /> instead.</remarks>
	public static bool HandleIncomingCall(string callId)
	{
		Action showDialogue = GetIncomingCallAction(callId);
		if (showDialogue == null)
		{
			return false;
		}
		Game1.playSound("openBox");
		Game1.player.freezePause = 500;
		DelayedAction.functionAfterDelay(showDialogue, 500);
		if (!Game1.player.callsReceived.TryGetValue(callId, out var count))
		{
			count = 0;
		}
		Game1.player.callsReceived[callId] = count + 1;
		return true;
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		if (Location != Game1.currentLocation)
		{
			return;
		}
		if (Game1.ticks != lastRunTick)
		{
			if (Game1.eventUp)
			{
				return;
			}
			lastRunTick = Game1.ticks;
			if (whichPhoneCall != null && Game1.shouldTimePass())
			{
				if (ringingTimer == 0)
				{
					Game1.playSound("phone");
					_phoneSoundPlaying = true;
				}
				ringingTimer += (int)time.ElapsedGameTime.TotalMilliseconds;
				if (ringingTimer >= 1800)
				{
					ringingTimer = 0;
					_phoneSoundPlaying = false;
				}
			}
		}
		base.updateWhenCurrentLocation(time);
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
		_phoneSoundPlaying = false;
		ringingTimer = 0;
		whichPhoneCall = null;
		intervalsToRing = 0;
	}

	/// <inheritdoc />
	public override bool minutesElapsed(int minutes)
	{
		if (!Game1.IsMasterGame)
		{
			return false;
		}
		if (lastMinutesElapsedTick != Game1.ticks)
		{
			lastMinutesElapsedTick = Game1.ticks;
			if (intervalsToRing == 0)
			{
				if (r == null)
				{
					r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
				}
				foreach (IPhoneHandler phoneHandler in PhoneHandlers)
				{
					string callId = phoneHandler.CheckForIncomingCall(r);
					if (callId != null)
					{
						intervalsToRing = 3;
						Game1.player.team.ringPhoneEvent.Fire(callId);
						break;
					}
				}
			}
			else
			{
				intervalsToRing--;
				if (intervalsToRing <= 0)
				{
					Game1.player.team.ringPhoneEvent.Fire(null);
				}
			}
		}
		return base.minutesElapsed(minutes);
	}

	/// <summary>Get whether the phone is currently ringing.</summary>
	public static bool IsRinging()
	{
		return _phoneSoundPlaying;
	}

	/// <summary>Start ringing the phone for an incoming call.</summary>
	/// <param name="callId">The unique ID for the incoming call.</param>
	public static void Ring(string callId)
	{
		if (string.IsNullOrWhiteSpace(callId))
		{
			StopRinging();
		}
		else if (GetIncomingCallAction(callId) != null)
		{
			whichPhoneCall = callId;
			ringingTimer = 0;
			_phoneSoundPlaying = false;
		}
	}

	/// <summary>Stop ringing the phone and discard the incoming call, if any.</summary>
	public static void StopRinging()
	{
		whichPhoneCall = null;
		ringingTimer = 0;
		intervalsToRing = 0;
		if (IsRinging())
		{
			Game1.soundBank.GetCue("phone").Stop(AudioStopOptions.Immediate);
			_phoneSoundPlaying = false;
		}
	}

	/// <summary>Hang up the phone.</summary>
	public static void HangUp()
	{
		StopRinging();
		Game1.currentLocation.playSound("openBox");
	}

	/// <summary>Get the action to call when the player answers the phone, if the call ID is valid.</summary>
	/// <param name="callId">The unique ID for the incoming call.</param>
	public static Action GetIncomingCallAction(string callId)
	{
		foreach (IPhoneHandler phoneHandler in PhoneHandlers)
		{
			if (phoneHandler.TryHandleIncomingCall(callId, out var showDialogue))
			{
				return showDialogue;
			}
		}
		return null;
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		if (!isTemporarilyInvisible)
		{
			base.draw(spriteBatch, x, y, alpha);
			bool ringing = ringingTimer > 0 && ringingTimer < 600;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle destination = new Rectangle((int)position.X + ((ringing || shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)position.Y + ((ringing || shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), 64, 128);
			float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f;
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), destination, itemData.GetSourceRect(1, base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
		}
	}
}
