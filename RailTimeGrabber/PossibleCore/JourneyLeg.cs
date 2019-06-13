using System;

namespace RailTimeGrabber
{
	/// <summary>
	/// Class representing part of a journey when there are changes in the journey
	/// </summary>
	public class JourneyLeg
	{
		public string DepartureTime { get; set; }
		public string ArrivalTime { get; set; }
		public string From { get; set; }
		public string To { get; set; }
	}
}