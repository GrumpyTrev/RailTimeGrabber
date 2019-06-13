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
	/// The NextDepartureExtractor class extracts the next departure time from a set of journeys retrived from the web 
	/// </summary>
	class NextDepartureExtractor
	{
		/// <summary>
		/// Traverse the list of journeys until one is found with a departure time that is later than the current time
		/// </summary>
		/// <param name="journeys"></param>
		public static DateTime ExtractDepartureTime( List<TrainJourney> journeys )
		{
			bool found = false;
			DateTime departureTime = DateTime.MinValue;

			IEnumerator<TrainJourney> enumerator = journeys.GetEnumerator();

			while ( ( found == false ) && ( enumerator.MoveNext() == true ) )
			{
				if ( enumerator.Current.DepartureDateTime > DateTime.Now )
				{
					departureTime = enumerator.Current.DepartureDateTime;
					found = true;
				}
			}

			return departureTime;
		}
	}
}