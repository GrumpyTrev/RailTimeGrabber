namespace RailTimeGrabber
{
	/// <summary>
	/// A pair of two stations representing a train trip
	/// </summary>
	class TrainTrip
	{
		/// <summary>
		/// The starting station name
		/// </summary>
		public string From { get; set; }

		/// <summary>
		/// The starting station code
		/// </summary>
		public string FromCode { get; set; }

		/// <summary>
		/// The destination station name
		/// </summary>
		public string To { get; set; }

		/// <summary>
		/// The destination station code
		/// </summary>
		public string ToCode { get; set; }

		/// <summary>
		/// Convert a station name to the format required in a web request
		/// </summary>
		/// <param name="stationName"></param>
		/// <returns></returns>
		public static string ToWebFormat( string stationName )
		{
			return stationName.Replace( ' ', '+' );
		}
	}
}