using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

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
		/// The destination station name
		/// </summary>
		public string To { get; set; }

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