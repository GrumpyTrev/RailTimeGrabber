using System;
using Android.Widget;
using Java.Lang;

namespace RailTimeGrabber
{
	/// <summary>
	/// Filter providing a filtered subset of the station list
	/// </summary>
	public class StationFilter: Filter
	{
		/// <summary>
		/// The StationAdapter used to display the station list
		/// </summary>
		public StationAdapter Adapter { get; set; }

		/// <summary>
		/// Called in a worker thread to filter the set of stations according to the input characters
		/// </summary>
		/// <param name="constraint"></param>
		/// <returns></returns>
		protected override FilterResults PerformFiltering( ICharSequence constraint )
		{
			FilterResults results = new FilterResults();
			string searchString = "";

			// Use the input string if supplied
			if ( constraint != null )
			{
				searchString = constraint.ToString().ToLower();
			}

			// If no input then display the most recently used stations
			if ( searchString.Length == 0 )
			{
				// Get this from perisitent storage
				results.Values = RecentStations.Stations.ToArray();
				results.Count = RecentStations.Stations.Count;
			}
			else if ( StationStorage.StationNames != null )
			{
				// Match the station names against the input characters
				string[] filteredStations = Array.FindAll( StationStorage.StationNames, c => c.ToLower().StartsWith( searchString ) );
				results.Values = filteredStations;
				results.Count = filteredStations.Length;
			}

			return results;
		}

		/// <summary>
		/// Called in the UI thread to display the filtered results
		/// </summary>
		/// <param name="constraint"></param>
		/// <param name="results"></param>
		protected override void PublishResults( ICharSequence constraint, FilterResults results )
		{
			Adapter.Clear();

			foreach ( string station in ( string[] )results.Values )
			{
				Adapter.Add( station );
			}
		}
	}
}