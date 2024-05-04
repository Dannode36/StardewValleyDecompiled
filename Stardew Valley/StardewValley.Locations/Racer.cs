using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Locations;

public class Racer : INetObject<NetFields>
{
	public NetBool moving = new NetBool();

	public Vector2? lastPosition;

	public NetPosition position = new NetPosition();

	public NetInt direction = new NetInt();

	public float horizontalPosition = -1f;

	public int currentTrackIndex = -1;

	public Vector2 segmentStart = Vector2.Zero;

	public Vector2 segmentEnd = Vector2.Zero;

	public NetVector2 jumpSegmentStart = new NetVector2();

	public NetVector2 jumpSegmentEnd = new NetVector2();

	public NetBool jumping = new NetBool();

	public NetBool tripping = new NetBool();

	public NetBool drawAboveMap = new NetBool();

	public float moveSpeed = 3f;

	public float minMoveSpeed = 3f;

	public float maxMoveSpeed = 6f;

	public float height;

	public float tripTimer;

	public NetInt racerIndex = new NetInt();

	protected Texture2D _texture;

	public bool frame;

	public float nextFrameSwap;

	public float burstDuration;

	public float nextBurst;

	public float extraLuck;

	public float gravity;

	public int _tripLeaps;

	public float progress;

	public NetInt sabotages = new NetInt(0);

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("DesertFestival.Racer");


	public Racer()
	{
		InitNetFields();
		direction.Value = 3;
		_texture = Game1.content.Load<Texture2D>("LooseSprites\\DesertRacers");
	}

	public Racer(int index)
		: this()
	{
		racerIndex.Value = index;
		ResetMoveSpeed();
	}

	public virtual void ResetMoveSpeed()
	{
		minMoveSpeed = 1.5f;
		maxMoveSpeed = 4f;
		extraLuck = Utility.RandomFloat(-0.25f, 0.25f);
		if ((int)racerIndex == 3)
		{
			minMoveSpeed = 0.5f;
			maxMoveSpeed = 3.5f;
		}
		SpeedBurst();
	}

	private void InitNetFields()
	{
		NetFields.SetOwner(this).AddField(racerIndex, "racerIndex").AddField(position.NetFields, "position.NetFields")
			.AddField(direction, "direction")
			.AddField(jumpSegmentStart, "jumpSegmentStart")
			.AddField(jumpSegmentEnd, "jumpSegmentEnd")
			.AddField(jumping, "jumping")
			.AddField(drawAboveMap, "drawAboveMap")
			.AddField(tripping, "tripping")
			.AddField(sabotages, "sabotages")
			.AddField(moving, "moving");
		jumpSegmentStart.Interpolated(interpolate: false, wait: false);
		jumpSegmentEnd.Interpolated(interpolate: false, wait: false);
	}

	public virtual void UpdateRaceProgress(DesertFestival location)
	{
		if (currentTrackIndex < 0)
		{
			progress = location.raceTrack.Length;
			return;
		}
		Vector2 segment = segmentEnd - segmentStart;
		float segment_length = segment.Length();
		segment.Normalize();
		Vector2 current_offset = position.Value - segmentStart;
		float position_in_segment = Vector2.Dot(segment, current_offset);
		if (segment_length > 0f)
		{
			segment_length = position_in_segment / segment_length;
		}
		progress = (float)currentTrackIndex + segment_length;
	}

	public virtual void Update(DesertFestival location)
	{
		if (Game1.IsMasterGame)
		{
			bool has_moved = false;
			if (location.currentRaceState.Value == DesertFestival.RaceState.StartingLine && currentTrackIndex < 0)
			{
				if (horizontalPosition < 0f)
				{
					int index = location.netRacers.IndexOf(this);
					horizontalPosition = (float)index / (float)(location.racerCount - 1);
				}
				currentTrackIndex = 0;
				Vector3 track_position = location.GetTrackPosition(currentTrackIndex, horizontalPosition);
				segmentStart = position.Value;
				segmentEnd = new Vector2(track_position.X, track_position.Y);
			}
			float frame_travel = maxMoveSpeed;
			if (location.currentRaceState.Value == DesertFestival.RaceState.Go)
			{
				if (location.finishedRacers.Count <= 0)
				{
					if (burstDuration > 0f)
					{
						moveSpeed = maxMoveSpeed;
						burstDuration -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
						if (burstDuration <= 0f)
						{
							burstDuration = 0f;
							nextBurst = Utility.RandomFloat(0.75f, 1.5f);
							if (Game1.random.NextDouble() + (double)extraLuck < 0.25)
							{
								nextBurst *= 0.5f;
							}
							if ((int)racerIndex == 3)
							{
								nextBurst *= 0.25f;
							}
							float last_place = location.raceTrack.Length;
							foreach (Racer racer in location.netRacers)
							{
								last_place = Math.Min(last_place, racer.progress);
							}
							if (progress > last_place && Game1.random.NextDouble() < (double)Math.Min(0.05f + (float)(int)sabotages * 0.2f, 0.5f))
							{
								tripping.Value = true;
								tripTimer = Utility.RandomFloat(1.5f, 2f);
							}
						}
					}
					else if (nextBurst > 0f)
					{
						moveSpeed = Utility.MoveTowards(moveSpeed, minMoveSpeed, 0.5f);
						nextBurst -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
						if (nextBurst <= 0f)
						{
							SpeedBurst();
							nextBurst = 0f;
						}
					}
					frame_travel = moveSpeed;
				}
				if (tripTimer > 0f)
				{
					tripTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
					if (tripTimer < 0f)
					{
						tripTimer = 0f;
						tripping.Value = false;
					}
				}
			}
			if ((bool)jumping)
			{
				frame_travel = ((!((segmentEnd - segmentStart).Length() / 64f > 3f)) ? 3f : 6f);
			}
			else if (tripping.Value)
			{
				frame_travel = 0.25f;
			}
			if (segmentStart == segmentEnd && position.Value == segmentEnd && currentTrackIndex < 0)
			{
				frame_travel = 0f;
			}
			while (frame_travel > 0f)
			{
				float moved_amount = Math.Min((segmentEnd - position.Value).Length(), frame_travel);
				frame_travel -= moved_amount;
				Vector2 delta = segmentEnd - position.Value;
				if (delta.X != 0f || delta.Y != 0f)
				{
					delta.Normalize();
					position.Value += delta * moved_amount;
					has_moved = true;
					if (Math.Abs(delta.Y) > Math.Abs(delta.X))
					{
						if (delta.Y < 0f)
						{
							direction.Value = 0;
						}
						else
						{
							direction.Value = 2;
						}
					}
					else if (delta.X < 0f)
					{
						direction.Value = 3;
					}
					else
					{
						direction.Value = 1;
					}
				}
				if (!((position.Value - segmentEnd).Length() < 0.01f))
				{
					continue;
				}
				position.Value = segmentEnd;
				if (location.currentRaceState.Value == DesertFestival.RaceState.Go && currentTrackIndex >= 0)
				{
					Vector3 track_position = location.GetTrackPosition(currentTrackIndex, horizontalPosition);
					if (track_position.Z > 0f)
					{
						tripping.Value = false;
						tripTimer = 0f;
						jumping.Value = true;
					}
					else
					{
						jumping.Value = false;
					}
					if (track_position.Z == 2f)
					{
						drawAboveMap.Value = true;
					}
					else if (track_position.Z == 3f)
					{
						drawAboveMap.Value = false;
					}
					currentTrackIndex++;
					if (currentTrackIndex >= location.raceTrack.Length)
					{
						currentTrackIndex = -2;
						segmentStart = segmentEnd;
						segmentEnd = new Vector2(44.5f, 37.5f - (float)location.finishedRacers.Count) * 64f;
						horizontalPosition = (float)(location.racerCount - 1 - location.finishedRacers.Count) / (float)(location.racerCount - 1);
						location.finishedRacers.Add(racerIndex);
						if (location.finishedRacers.Count == 1)
						{
							location.announceRaceEvent.Fire("Race_Finish");
							location.OnRaceWon(racerIndex);
						}
					}
					else
					{
						track_position = location.GetTrackPosition(currentTrackIndex, horizontalPosition);
						segmentStart = segmentEnd;
						segmentEnd = new Vector2(track_position.X, track_position.Y);
					}
					if (jumping.Value)
					{
						jumpSegmentStart.Value = segmentStart;
						jumpSegmentEnd.Value = segmentEnd;
					}
				}
				else
				{
					frame_travel = 0f;
					segmentStart = segmentEnd;
					if (location.currentRaceState.Value >= DesertFestival.RaceState.StartingLine && location.currentRaceState.Value < DesertFestival.RaceState.Go)
					{
						direction.Value = 0;
					}
					else
					{
						direction.Value = 3;
					}
				}
			}
			moving.Value = has_moved;
		}
		if (!lastPosition.HasValue)
		{
			lastPosition = position.Value;
		}
		float distance_traveled = (lastPosition.Value - position.Value).Length();
		nextFrameSwap -= distance_traveled;
		while (nextFrameSwap <= 0f)
		{
			frame = !frame;
			nextFrameSwap += 8f;
		}
		lastPosition = position.Value;
		if (!jumping.Value)
		{
			if (moving.Value)
			{
				if ((bool)tripping && height == 0f)
				{
					if (_tripLeaps == 0)
					{
						gravity = 1f;
					}
					else
					{
						gravity = Utility.RandomFloat(0.5f, 0.75f);
					}
					_tripLeaps++;
				}
				else if ((int)racerIndex == 2 && height == 0f)
				{
					gravity = Utility.RandomFloat(0.25f, 0.5f);
				}
			}
			if (height != 0f || gravity != 0f)
			{
				height += gravity;
				gravity -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds * 2f;
				if (gravity == 0f)
				{
					gravity = -0.0001f;
				}
				if (height <= 0f)
				{
					gravity = 0f;
					height = 0f;
				}
			}
		}
		if (!tripping.Value)
		{
			_tripLeaps = 0;
		}
		if (jumping.Value)
		{
			Vector2 segment = jumpSegmentEnd.Value - jumpSegmentStart.Value;
			float segment_length = segment.Length();
			segment.Normalize();
			Vector2 current_offset = position.Value - jumpSegmentStart.Value;
			float position_in_segment = Vector2.Dot(segment, current_offset);
			if (segment_length > 0f)
			{
				height = (float)Math.Sin((double)Utility.Clamp(position_in_segment / segment_length, 0f, 1f) * Math.PI) * 48f;
			}
		}
		else if (gravity == 0f)
		{
			height = 0f;
		}
	}

	public virtual void SpeedBurst()
	{
		burstDuration = Utility.RandomFloat(0.25f, 1f);
		if (Game1.random.NextDouble() + (double)extraLuck < 0.25)
		{
			burstDuration *= 2f;
		}
		if ((int)racerIndex == 3)
		{
			burstDuration *= 0.25f;
		}
		moveSpeed = maxMoveSpeed;
	}

	public virtual void Draw(SpriteBatch sb)
	{
		float sort_y = (position.Y + (float)(int)racerIndex * 0.1f) / 10000f;
		float height_fade = Utility.Clamp(1f - height / 12f, 0f, 1f);
		sb.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position.Value), null, Color.White * 0.75f * height_fade, 0f, new Vector2(Game1.shadowTexture.Width / 2, Game1.shadowTexture.Height / 2), new Vector2(3f, 3f), SpriteEffects.None, sort_y / 10000f - 1E-07f);
		SpriteEffects effect = SpriteEffects.None;
		Rectangle source_rect = new Rectangle(0, 0, 16, 16);
		source_rect.Y = (int)racerIndex * 16;
		if ((int)direction == 0)
		{
			source_rect.X = 0;
		}
		if ((int)direction == 2)
		{
			source_rect.X = 64;
		}
		if ((int)direction == 3)
		{
			source_rect.X = 32;
			effect = SpriteEffects.FlipHorizontally;
		}
		if ((int)direction == 1)
		{
			source_rect.X = 32;
		}
		if (frame)
		{
			source_rect.X += 16;
		}
		Vector2 offset = Vector2.Zero;
		if (tripping.Value)
		{
			source_rect.X = 96;
			offset.X += (float)Game1.random.Next(-1, 2) * 0.5f;
			offset.Y += (float)Game1.random.Next(-1, 2) * 0.5f;
		}
		sb.Draw(_texture, Game1.GlobalToLocal(position.Value + new Vector2(offset.X, 0f - height + offset.Y) * 4f), source_rect, Color.White, 0f, new Vector2(8f, 14f), 4f, effect, sort_y);
	}
}
