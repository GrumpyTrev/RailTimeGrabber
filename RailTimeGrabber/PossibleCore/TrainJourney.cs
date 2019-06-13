using System;
using System.Collections.Generic;

namespace RailTimeGrabber
{
	/// <summary>
	/// Class representing the train journey details obtained from the web
	/// </summary>
	public class TrainJourney
	{
		public DateTime DepartureDateTime { get; set; }
		public string DepartureTime { get; set; }
		public string ArrivalTime { get; set; }
		public string Duration { get; set; }
		public string Status { get; set; }
		public bool DateChange { get; set; } = false;

		public List<JourneyLeg> Legs { get; set; } = new List<JourneyLeg>();
	}
}