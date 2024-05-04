using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley.Events;

/// <summary>Generates the event that plays when watching a movie at the <see cref="T:StardewValley.Locations.MovieTheater" />.</summary>
public class MovieTheaterScreeningEvent
{
	public int currentResponse;

	public List<List<Character>> playerAndGuestAudienceGroups;

	public Dictionary<int, Character> _responseOrder = new Dictionary<int, Character>();

	protected Dictionary<Character, Character> _whiteListDependencyLookup;

	protected Dictionary<Character, string> _characterResponses;

	public MovieData movieData;

	protected List<Farmer> _farmers;

	protected Dictionary<Character, MovieConcession> _concessionsData;

	public Event getMovieEvent(string movieId, List<List<Character>> player_and_guest_audience_groups, List<List<Character>> npcOnlyAudienceGroups, Dictionary<Character, MovieConcession> concessions_data = null)
	{
		_concessionsData = concessions_data;
		_responseOrder = new Dictionary<int, Character>();
		_whiteListDependencyLookup = new Dictionary<Character, Character>();
		_characterResponses = new Dictionary<Character, string>();
		movieData = MovieTheater.GetMovieDataById()[movieId];
		playerAndGuestAudienceGroups = player_and_guest_audience_groups;
		currentResponse = 0;
		StringBuilder sb = new StringBuilder();
		Random theaterRandom = Utility.CreateDaySaveRandom();
		sb.Append("movieScreenAmbience/-2000 -2000/");
		string playerCharacterEventName = "farmer" + Utility.getFarmerNumberFromFarmer(Game1.player);
		string playerCharacterGuestName = "";
		bool hasPlayerGuest = false;
		foreach (List<Character> list in playerAndGuestAudienceGroups)
		{
			if (!list.Contains(Game1.player))
			{
				continue;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i] is Farmer))
				{
					playerCharacterGuestName = list[i].name;
					hasPlayerGuest = true;
					break;
				}
			}
		}
		_farmers = new List<Farmer>();
		foreach (List<Character> playerAndGuestAudienceGroup in playerAndGuestAudienceGroups)
		{
			foreach (Character item in playerAndGuestAudienceGroup)
			{
				if (item is Farmer player && !_farmers.Contains(player))
				{
					_farmers.Add(player);
				}
			}
		}
		List<Character> allAudience = playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).ToList();
		if (allAudience.Count <= 12)
		{
			allAudience.AddRange(npcOnlyAudienceGroups.SelectMany((List<Character> x) => x).ToList());
		}
		bool first = true;
		foreach (Character c in allAudience)
		{
			if (c != null)
			{
				if (!first)
				{
					sb.Append(" ");
				}
				if (c is Farmer f)
				{
					sb.Append("farmer" + Utility.getFarmerNumberFromFarmer(f));
				}
				else
				{
					sb.Append(c.name);
				}
				sb.Append(" -1000 -1000 0");
				first = false;
			}
		}
		sb.Append("/changeToTemporaryMap MovieTheaterScreen false/specificTemporarySprite movieTheater_setup/ambientLight 0 0 0/");
		string[] backRow = new string[8];
		string[] midRow = new string[6];
		string[] frontRow = new string[4];
		playerAndGuestAudienceGroups = playerAndGuestAudienceGroups.OrderBy((List<Character> x) => theaterRandom.Next()).ToList();
		int startingSeat = theaterRandom.Next(8 - Math.Min(playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).Count(), 8) + 1);
		int whichGroup = 0;
		if (playerAndGuestAudienceGroups.Count > 0)
		{
			for (int i = 0; i < 8; i++)
			{
				int seat = (i + startingSeat) % 8;
				if (playerAndGuestAudienceGroups[whichGroup].Count == 2 && (seat == 3 || seat == 7))
				{
					i++;
					seat++;
					seat %= 8;
				}
				for (int j = 0; j < playerAndGuestAudienceGroups[whichGroup].Count && seat + j < backRow.Length; j++)
				{
					backRow[seat + j] = ((playerAndGuestAudienceGroups[whichGroup][j] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(playerAndGuestAudienceGroups[whichGroup][j] as Farmer)) : ((string)playerAndGuestAudienceGroups[whichGroup][j].name));
					if (j > 0)
					{
						i++;
					}
				}
				whichGroup++;
				if (whichGroup >= playerAndGuestAudienceGroups.Count)
				{
					break;
				}
			}
		}
		else
		{
			Game1.log.Warn("The movie audience somehow has no players. This is likely a bug.");
		}
		bool usedMidRow = false;
		if (whichGroup < playerAndGuestAudienceGroups.Count)
		{
			startingSeat = 0;
			for (int i = 0; i < 4; i++)
			{
				int seat = (i + startingSeat) % 4;
				for (int j = 0; j < playerAndGuestAudienceGroups[whichGroup].Count && seat + j < frontRow.Length; j++)
				{
					frontRow[seat + j] = ((playerAndGuestAudienceGroups[whichGroup][j] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(playerAndGuestAudienceGroups[whichGroup][j] as Farmer)) : ((string)playerAndGuestAudienceGroups[whichGroup][j].name));
					if (j > 0)
					{
						i++;
					}
				}
				whichGroup++;
				if (whichGroup >= playerAndGuestAudienceGroups.Count)
				{
					break;
				}
			}
			if (whichGroup < playerAndGuestAudienceGroups.Count)
			{
				usedMidRow = true;
				startingSeat = 0;
				for (int i = 0; i < 6; i++)
				{
					int seat = (i + startingSeat) % 6;
					if (playerAndGuestAudienceGroups[whichGroup].Count == 2 && seat == 2)
					{
						i++;
						seat++;
						seat %= 8;
					}
					for (int j = 0; j < playerAndGuestAudienceGroups[whichGroup].Count && seat + j < midRow.Length; j++)
					{
						midRow[seat + j] = ((playerAndGuestAudienceGroups[whichGroup][j] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(playerAndGuestAudienceGroups[whichGroup][j] as Farmer)) : ((string)playerAndGuestAudienceGroups[whichGroup][j].name));
						if (j > 0)
						{
							i++;
						}
					}
					whichGroup++;
					if (whichGroup >= playerAndGuestAudienceGroups.Count)
					{
						break;
					}
				}
			}
		}
		if (!usedMidRow)
		{
			for (int j = 0; j < npcOnlyAudienceGroups.Count; j++)
			{
				int seat = theaterRandom.Next(3 - npcOnlyAudienceGroups[j].Count + 1) + j * 3;
				for (int i = 0; i < npcOnlyAudienceGroups[j].Count; i++)
				{
					midRow[seat + i] = npcOnlyAudienceGroups[j][i].name;
				}
			}
		}
		int soFar = 0;
		int sittingTogetherCount = 0;
		for (int i = 0; i < backRow.Length; i++)
		{
			if (backRow[i] == null || !(backRow[i] != "") || !(backRow[i] != playerCharacterEventName) || !(backRow[i] != playerCharacterGuestName))
			{
				continue;
			}
			soFar++;
			if (soFar < 2)
			{
				continue;
			}
			sittingTogetherCount++;
			Point seat = getBackRowSeatTileFromIndex(i);
			sb.Append("warp ").Append(backRow[i]).Append(" ")
				.Append(seat.X)
				.Append(" ")
				.Append(seat.Y)
				.Append("/positionOffset ")
				.Append(backRow[i])
				.Append(" 0 -10/");
			if (sittingTogetherCount == 2)
			{
				sittingTogetherCount = 0;
				if (theaterRandom.NextBool() && backRow[i] != playerCharacterGuestName && backRow[i - 1] != playerCharacterGuestName && backRow[i - 1] != null)
				{
					sb.Append("faceDirection " + backRow[i] + " 3 true/");
					sb.Append("faceDirection " + backRow[i - 1] + " 1 true/");
				}
			}
		}
		soFar = 0;
		sittingTogetherCount = 0;
		for (int i = 0; i < midRow.Length; i++)
		{
			if (midRow[i] == null || !(midRow[i] != "") || !(midRow[i] != playerCharacterEventName) || !(midRow[i] != playerCharacterGuestName))
			{
				continue;
			}
			soFar++;
			if (soFar < 2)
			{
				continue;
			}
			sittingTogetherCount++;
			Point seat = getMidRowSeatTileFromIndex(i);
			sb.Append("warp ").Append(midRow[i]).Append(" ")
				.Append(seat.X)
				.Append(" ")
				.Append(seat.Y)
				.Append("/positionOffset ")
				.Append(midRow[i])
				.Append(" 0 -10/");
			if (sittingTogetherCount == 2)
			{
				sittingTogetherCount = 0;
				if (i != 3 && theaterRandom.NextBool() && midRow[i - 1] != null)
				{
					sb.Append("faceDirection " + midRow[i] + " 3 true/");
					sb.Append("faceDirection " + midRow[i - 1] + " 1 true/");
				}
			}
		}
		soFar = 0;
		sittingTogetherCount = 0;
		for (int i = 0; i < frontRow.Length; i++)
		{
			if (frontRow[i] == null || !(frontRow[i] != "") || !(frontRow[i] != playerCharacterEventName) || !(frontRow[i] != playerCharacterGuestName))
			{
				continue;
			}
			soFar++;
			if (soFar < 2)
			{
				continue;
			}
			sittingTogetherCount++;
			Point seat = getFrontRowSeatTileFromIndex(i);
			sb.Append("warp ").Append(frontRow[i]).Append(" ")
				.Append(seat.X)
				.Append(" ")
				.Append(seat.Y)
				.Append("/positionOffset ")
				.Append(frontRow[i])
				.Append(" 0 -10/");
			if (sittingTogetherCount == 2)
			{
				sittingTogetherCount = 0;
				if (theaterRandom.NextBool() && frontRow[i - 1] != null)
				{
					sb.Append("faceDirection " + frontRow[i] + " 3 true/");
					sb.Append("faceDirection " + frontRow[i - 1] + " 1 true/");
				}
			}
		}
		Point warpPoint = new Point(1, 15);
		soFar = 0;
		for (int i = 0; i < backRow.Length; i++)
		{
			if (backRow[i] != null && backRow[i] != "" && backRow[i] != playerCharacterEventName && backRow[i] != playerCharacterGuestName)
			{
				Point seat = getBackRowSeatTileFromIndex(i);
				if (soFar == 1)
				{
					sb.Append("warp ").Append(backRow[i]).Append(" ")
						.Append(seat.X - 1)
						.Append(" 10")
						.Append("/advancedMove ")
						.Append(backRow[i])
						.Append(" false 1 " + 200 + " 1 0 4 1000/")
						.Append("positionOffset ")
						.Append(backRow[i])
						.Append(" 0 -10/");
				}
				else
				{
					sb.Append("warp ").Append(backRow[i]).Append(" 1 12")
						.Append("/advancedMove ")
						.Append(backRow[i])
						.Append(" false 1 200 ")
						.Append("0 -2 ")
						.Append(seat.X - 1)
						.Append(" 0 4 1000/")
						.Append("positionOffset ")
						.Append(backRow[i])
						.Append(" 0 -10/");
				}
				soFar++;
			}
			if (soFar >= 2)
			{
				break;
			}
		}
		soFar = 0;
		for (int i = 0; i < midRow.Length; i++)
		{
			if (midRow[i] != null && midRow[i] != "" && midRow[i] != playerCharacterEventName && midRow[i] != playerCharacterGuestName)
			{
				Point seat = getMidRowSeatTileFromIndex(i);
				if (soFar == 1)
				{
					sb.Append("warp ").Append(midRow[i]).Append(" ")
						.Append(seat.X - 1)
						.Append(" 8")
						.Append("/advancedMove ")
						.Append(midRow[i])
						.Append(" false 1 " + 400 + " 1 0 4 1000/");
				}
				else
				{
					sb.Append("warp ").Append(midRow[i]).Append(" 2 9")
						.Append("/advancedMove ")
						.Append(midRow[i])
						.Append(" false 1 300 ")
						.Append("0 -1 ")
						.Append(seat.X - 2)
						.Append(" 0 4 1000/");
				}
				soFar++;
			}
			if (soFar >= 2)
			{
				break;
			}
		}
		soFar = 0;
		for (int i = 0; i < frontRow.Length; i++)
		{
			if (frontRow[i] != null && frontRow[i] != "" && frontRow[i] != playerCharacterEventName && frontRow[i] != playerCharacterGuestName)
			{
				Point seat = getFrontRowSeatTileFromIndex(i);
				if (soFar == 1)
				{
					sb.Append("warp ").Append(frontRow[i]).Append(" ")
						.Append(seat.X - 1)
						.Append(" 6")
						.Append("/advancedMove ")
						.Append(frontRow[i])
						.Append(" false 1 " + 400 + " 1 0 4 1000/");
				}
				else
				{
					sb.Append("warp ").Append(frontRow[i]).Append(" 3 7")
						.Append("/advancedMove ")
						.Append(frontRow[i])
						.Append(" false 1 300 ")
						.Append("0 -1 ")
						.Append(seat.X - 3)
						.Append(" 0 4 1000/");
				}
				soFar++;
			}
			if (soFar >= 2)
			{
				break;
			}
		}
		sb.Append("viewport 6 8 true/pause 500/");
		for (int i = 0; i < backRow.Length; i++)
		{
			if (backRow[i] != null && backRow[i] != "")
			{
				Point seat = getBackRowSeatTileFromIndex(i);
				if (backRow[i] == playerCharacterEventName || backRow[i] == playerCharacterGuestName)
				{
					sb.Append("warp ").Append(backRow[i]).Append(" ")
						.Append(warpPoint.X)
						.Append(" ")
						.Append(warpPoint.Y)
						.Append("/advancedMove ")
						.Append(backRow[i])
						.Append(" false 0 -5 ")
						.Append(seat.X - warpPoint.X)
						.Append(" 0 4 1000/")
						.Append("pause ")
						.Append(1000)
						.Append("/");
				}
			}
		}
		for (int i = 0; i < midRow.Length; i++)
		{
			if (midRow[i] != null && midRow[i] != "")
			{
				Point seat = getMidRowSeatTileFromIndex(i);
				if (midRow[i] == playerCharacterEventName || midRow[i] == playerCharacterGuestName)
				{
					sb.Append("warp ").Append(midRow[i]).Append(" ")
						.Append(warpPoint.X)
						.Append(" ")
						.Append(warpPoint.Y)
						.Append("/advancedMove ")
						.Append(midRow[i])
						.Append(" false 0 -7 ")
						.Append(seat.X - warpPoint.X)
						.Append(" 0 4 1000/")
						.Append("pause ")
						.Append(1000)
						.Append("/");
				}
			}
		}
		for (int i = 0; i < frontRow.Length; i++)
		{
			if (frontRow[i] != null && frontRow[i] != "")
			{
				Point seat = getFrontRowSeatTileFromIndex(i);
				if (frontRow[i] == playerCharacterEventName || frontRow[i] == playerCharacterGuestName)
				{
					sb.Append("warp ").Append(frontRow[i]).Append(" ")
						.Append(warpPoint.X)
						.Append(" ")
						.Append(warpPoint.Y)
						.Append("/advancedMove ")
						.Append(frontRow[i])
						.Append(" false 0 -7 1 0 0 -1 1 0 0 -1 ")
						.Append(seat.X - 3)
						.Append(" 0 4 1000/")
						.Append("pause ")
						.Append(1000)
						.Append("/");
				}
			}
		}
		sb.Append("pause 3000");
		if (hasPlayerGuest)
		{
			sb.Append("/proceedPosition ").Append(playerCharacterGuestName);
		}
		sb.Append("/pause 1000");
		if (!hasPlayerGuest)
		{
			sb.Append("/proceedPosition farmer");
		}
		sb.Append("/waitForAllStationary/pause 100");
		foreach (Character c in allAudience)
		{
			string actorName = getEventName(c);
			if (actorName != playerCharacterEventName && actorName != playerCharacterGuestName)
			{
				if (c is Farmer)
				{
					sb.Append("/faceDirection ").Append(actorName).Append(" 0 true/positionOffset ")
						.Append(actorName)
						.Append(" 0 42 true");
				}
				else
				{
					sb.Append("/faceDirection ").Append(actorName).Append(" 0 true/positionOffset ")
						.Append(actorName)
						.Append(" 0 12 true");
				}
				if (theaterRandom.NextDouble() < 0.2)
				{
					sb.Append("/pause 100");
				}
			}
		}
		sb.Append("/positionOffset ").Append(playerCharacterEventName).Append(" 0 32");
		if (hasPlayerGuest)
		{
			sb.Append("/positionOffset ").Append(playerCharacterGuestName).Append(" 0 8");
		}
		sb.Append("/ambientLight 210 210 120 true/pause 500/viewport move 0 -1 4000/pause 5000");
		List<Character> responding_characters = new List<Character>();
		foreach (List<Character> playerAndGuestAudienceGroup2 in playerAndGuestAudienceGroups)
		{
			foreach (Character character in playerAndGuestAudienceGroup2)
			{
				if (!(character is Farmer) && !responding_characters.Contains(character))
				{
					responding_characters.Add(character);
				}
			}
		}
		for (int i = 0; i < responding_characters.Count; i++)
		{
			int index = theaterRandom.Next(responding_characters.Count);
			Character character = responding_characters[i];
			responding_characters[i] = responding_characters[index];
			responding_characters[index] = character;
		}
		int current_response_index = 0;
		foreach (MovieScene scene in movieData.Scenes)
		{
			if (scene.ResponsePoint == null)
			{
				continue;
			}
			bool found_reaction = false;
			for (int i = 0; i < responding_characters.Count; i++)
			{
				MovieCharacterReaction reaction = MovieTheater.GetReactionsForCharacter(responding_characters[i] as NPC);
				if (reaction == null)
				{
					continue;
				}
				foreach (MovieReaction movie_reaction in reaction.Reactions)
				{
					if (!movie_reaction.ShouldApplyToMovie(movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(responding_characters[i] as NPC)) || movie_reaction.SpecialResponses?.DuringMovie == null || (!(movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == scene.ResponsePoint) && movie_reaction.Whitelist.Count <= 0))
					{
						continue;
					}
					if (!_whiteListDependencyLookup.ContainsKey(responding_characters[i]))
					{
						_responseOrder[current_response_index] = responding_characters[i];
						if (movie_reaction.Whitelist != null)
						{
							for (int j = 0; j < movie_reaction.Whitelist.Count; j++)
							{
								Character white_list_character = Game1.getCharacterFromName(movie_reaction.Whitelist[j]);
								if (white_list_character == null)
								{
									continue;
								}
								_whiteListDependencyLookup[white_list_character] = responding_characters[i];
								foreach (int key in _responseOrder.Keys)
								{
									if (_responseOrder[key] == white_list_character)
									{
										_responseOrder.Remove(key);
									}
								}
							}
						}
					}
					responding_characters.RemoveAt(i);
					i--;
					found_reaction = true;
					break;
				}
				if (found_reaction)
				{
					break;
				}
			}
			if (!found_reaction)
			{
				for (int i = 0; i < responding_characters.Count; i++)
				{
					MovieCharacterReaction reaction = MovieTheater.GetReactionsForCharacter(responding_characters[i] as NPC);
					if (reaction == null)
					{
						continue;
					}
					foreach (MovieReaction movie_reaction in reaction.Reactions)
					{
						if (!movie_reaction.ShouldApplyToMovie(movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(responding_characters[i] as NPC)) || movie_reaction.SpecialResponses?.DuringMovie == null || !(movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == current_response_index.ToString()))
						{
							continue;
						}
						if (!_whiteListDependencyLookup.ContainsKey(responding_characters[i]))
						{
							_responseOrder[current_response_index] = responding_characters[i];
							if (movie_reaction.Whitelist != null)
							{
								for (int j = 0; j < movie_reaction.Whitelist.Count; j++)
								{
									Character white_list_character = Game1.getCharacterFromName(movie_reaction.Whitelist[j]);
									if (white_list_character == null)
									{
										continue;
									}
									_whiteListDependencyLookup[white_list_character] = responding_characters[i];
									foreach (int key in _responseOrder.Keys)
									{
										if (_responseOrder[key] == white_list_character)
										{
											_responseOrder.Remove(key);
										}
									}
								}
							}
						}
						responding_characters.RemoveAt(i);
						i--;
						found_reaction = true;
						break;
					}
					if (found_reaction)
					{
						break;
					}
				}
			}
			current_response_index++;
		}
		current_response_index = 0;
		for (int i = 0; i < responding_characters.Count; i++)
		{
			if (!_whiteListDependencyLookup.ContainsKey(responding_characters[i]))
			{
				for (; _responseOrder.ContainsKey(current_response_index); current_response_index++)
				{
				}
				_responseOrder[current_response_index] = responding_characters[i];
				current_response_index++;
			}
		}
		responding_characters = null;
		foreach (MovieScene scene in movieData.Scenes)
		{
			_ParseScene(sb, scene);
		}
		while (currentResponse < _responseOrder.Count)
		{
			_ParseResponse(sb);
		}
		sb.Append("/stopMusic");
		sb.Append("/fade/viewport -1000 -1000");
		sb.Append("/pause 500/message \"" + Game1.content.LoadString("Strings\\Locations:Theater_MovieEnd") + "\"/pause 500");
		sb.Append("/requestMovieEnd");
		return new Event(sb.ToString(), null, "MovieTheaterScreening");
	}

	protected void _ParseScene(StringBuilder sb, MovieScene scene)
	{
		if (!string.IsNullOrWhiteSpace(scene.Sound))
		{
			sb.Append("/playSound " + scene.Sound);
		}
		if (!string.IsNullOrWhiteSpace(scene.Music))
		{
			sb.Append("/playMusic " + scene.Music);
		}
		if (scene.MessageDelay > 0)
		{
			sb.Append("/pause " + scene.MessageDelay);
		}
		if (scene.Image >= 0)
		{
			sb.Append("/specificTemporarySprite movieTheater_screen " + movieData.Id + " " + scene.Image + " " + scene.Shake);
			if (movieData.Texture != null)
			{
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 1, sb);
				handler.AppendLiteral(" \"");
				handler.AppendFormatted(ArgUtility.EscapeQuotes(movieData.Texture));
				handler.AppendLiteral("\"");
				sb.Append(ref handler);
			}
		}
		if (!string.IsNullOrWhiteSpace(scene.Script))
		{
			sb.Append(TokenParser.ParseText(scene.Script));
		}
		if (!string.IsNullOrWhiteSpace(scene.Text))
		{
			sb.Append("/message \"" + TokenParser.ParseText(scene.Text) + "\"");
		}
		if (scene.ResponsePoint != null)
		{
			_ParseResponse(sb, scene);
		}
	}

	protected void _ParseResponse(StringBuilder sb, MovieScene scene = null)
	{
		if (_responseOrder.TryGetValue(currentResponse, out var responding_character))
		{
			sb.Append("/pause 500");
			bool hadUniqueScript = false;
			if (!_whiteListDependencyLookup.ContainsKey(responding_character))
			{
				MovieCharacterReaction reaction = MovieTheater.GetReactionsForCharacter(responding_character as NPC);
				if (reaction != null)
				{
					foreach (MovieReaction movie_reaction in reaction.Reactions)
					{
						if (movie_reaction.ShouldApplyToMovie(movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(responding_character as NPC)) && movie_reaction.SpecialResponses?.DuringMovie != null && (string.IsNullOrEmpty(movie_reaction.SpecialResponses.DuringMovie.ResponsePoint) || (scene != null && movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == scene.ResponsePoint) || movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == currentResponse.ToString() || movie_reaction.Whitelist.Count > 0))
						{
							string script = TokenParser.ParseText(movie_reaction.SpecialResponses.DuringMovie.Script);
							string text = TokenParser.ParseText(movie_reaction.SpecialResponses.DuringMovie.Text);
							if (!string.IsNullOrWhiteSpace(script))
							{
								sb.Append(script);
								hadUniqueScript = true;
							}
							if (!string.IsNullOrWhiteSpace(text))
							{
								sb.Append(string.Concat("/speak ", responding_character.name, " \"", text, "\""));
							}
							break;
						}
					}
				}
			}
			_ParseCharacterResponse(sb, responding_character, hadUniqueScript);
			foreach (Character key in _whiteListDependencyLookup.Keys)
			{
				if (_whiteListDependencyLookup[key] == responding_character)
				{
					_ParseCharacterResponse(sb, key);
				}
			}
		}
		currentResponse++;
	}

	protected void _ParseCharacterResponse(StringBuilder sb, Character responding_character, bool ignoreScript = false)
	{
		string response = MovieTheater.GetResponseForMovie(responding_character as NPC);
		if (_whiteListDependencyLookup.TryGetValue(responding_character, out var requestingCharacter))
		{
			response = MovieTheater.GetResponseForMovie(requestingCharacter as NPC);
		}
		switch (response)
		{
		case "love":
			sb.Append("/friendship " + responding_character.Name + " " + 200);
			if (!ignoreScript)
			{
				sb.Append(string.Concat("/playSound reward/emote ", responding_character.name, " ", 20.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveMovie", responding_character.displayName), "\""));
			}
			break;
		case "like":
			sb.Append("/friendship " + responding_character.Name + " " + 100);
			if (!ignoreScript)
			{
				sb.Append(string.Concat("/playSound give_gift/emote ", responding_character.name, " ", 56.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeMovie", responding_character.displayName), "\""));
			}
			break;
		case "dislike":
			sb.Append("/friendship " + responding_character.Name + " " + 0);
			if (!ignoreScript)
			{
				sb.Append(string.Concat("/playSound newArtifact/emote ", responding_character.name, " ", 24.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeMovie", responding_character.displayName), "\""));
			}
			break;
		}
		if (_concessionsData != null && _concessionsData.TryGetValue(responding_character, out var concession))
		{
			string concession_response = MovieTheater.GetConcessionTasteForCharacter(responding_character, concession);
			string gender_tag = "";
			if (NPC.TryGetData(responding_character.name, out var npcData))
			{
				switch (npcData.Gender)
				{
				case Gender.Female:
					gender_tag = "_Female";
					break;
				case Gender.Male:
					gender_tag = "_Male";
					break;
				}
			}
			string sound = "eat";
			if (concession.Tags != null && concession.Tags.Contains("Drink"))
			{
				sound = "gulp";
			}
			switch (concession_response)
			{
			case "love":
				sb.Append("/friendship " + responding_character.Name + " " + 50);
				sb.Append("/tossConcession " + responding_character.Name + " " + concession.Id + "/pause 1000");
				sb.Append("/playSound " + sound + "/shake " + responding_character.Name + " 500/pause 1000");
				sb.Append(string.Concat("/playSound reward/emote ", responding_character.name, " ", 20.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveConcession" + gender_tag, responding_character.displayName, concession.DisplayName), "\""));
				break;
			case "like":
				sb.Append("/friendship " + responding_character.Name + " " + 25);
				sb.Append("/tossConcession " + responding_character.Name + " " + concession.Id + "/pause 1000");
				sb.Append("/playSound " + sound + "/shake " + responding_character.Name + " 500/pause 1000");
				sb.Append(string.Concat("/playSound give_gift/emote ", responding_character.name, " ", 56.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeConcession" + gender_tag, responding_character.displayName, concession.DisplayName), "\""));
				break;
			case "dislike":
				sb.Append("/friendship " + responding_character.Name + " " + 0);
				sb.Append("/playSound croak/pause 1000");
				sb.Append(string.Concat("/playSound newArtifact/emote ", responding_character.name, " ", 40.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeConcession" + gender_tag, responding_character.displayName, concession.DisplayName), "\""));
				break;
			}
		}
		_characterResponses[responding_character] = response;
	}

	public Dictionary<Character, string> GetCharacterResponses()
	{
		return _characterResponses;
	}

	private static string getEventName(Character c)
	{
		if (c is Farmer player)
		{
			return "farmer" + Utility.getFarmerNumberFromFarmer(player);
		}
		return c.name;
	}

	private Point getBackRowSeatTileFromIndex(int index)
	{
		return index switch
		{
			0 => new Point(2, 10), 
			1 => new Point(3, 10), 
			2 => new Point(4, 10), 
			3 => new Point(5, 10), 
			4 => new Point(8, 10), 
			5 => new Point(9, 10), 
			6 => new Point(10, 10), 
			7 => new Point(11, 10), 
			_ => new Point(4, 12), 
		};
	}

	private Point getMidRowSeatTileFromIndex(int index)
	{
		return index switch
		{
			0 => new Point(3, 8), 
			1 => new Point(4, 8), 
			2 => new Point(5, 8), 
			3 => new Point(8, 8), 
			4 => new Point(9, 8), 
			5 => new Point(10, 8), 
			_ => new Point(4, 12), 
		};
	}

	private Point getFrontRowSeatTileFromIndex(int index)
	{
		return index switch
		{
			0 => new Point(4, 6), 
			1 => new Point(5, 6), 
			2 => new Point(8, 6), 
			3 => new Point(9, 6), 
			_ => new Point(4, 12), 
		};
	}
}
