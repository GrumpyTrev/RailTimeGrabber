namespace RailTimeGrabber
{
	/// <summary>
	/// Class representing the train journey details obtained from the web
	/// </summary>
	class TrainJourney
	{
		public string DepartureTime { get; set; }
		public string ArrivalTime { get; set; }
		public string Duration { get; set; }
		public string Status { get; set; }
	}
}