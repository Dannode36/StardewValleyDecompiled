using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley.Pathfinding;

public class SchedulePathDescription
{
	public Stack<Point> route;

	public int time;

	public int facingDirection;

	public string endOfRouteBehavior;

	public string endOfRouteMessage;

	public string targetLocationName;

	public Point targetTile;

	public SchedulePathDescription(Stack<Point> route, int facingDirection, string endBehavior, string endMessage, string targetLocationName, Point targetTile)
	{
		endOfRouteMessage = endMessage;
		this.route = route;
		this.facingDirection = facingDirection;
		endOfRouteBehavior = endBehavior;
		this.targetLocationName = targetLocationName;
		this.targetTile = targetTile;
	}
}
