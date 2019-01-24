using System;
using System.Collections.Generic;

namespace RailTimeGrabber
{
	/// <summary>
	/// The RecentStations keeps track of the most recently used station names
	/// </summary>
	class RecentStations
	{
		/// <summary>
		/// List of the most recently used station names
		/// If not already loaded get from the persistent storage
		/// </summary>
		public static List< string > Stations
		{
			get
			{
				if ( stations == null )
				{
					LoadStations();
				}

				return stations;
			}
		}

		/// <summary>
		/// Add the specified station to the list of recent stations
		/// </summary>
		/// <param name="stationName"></param>
		/// <returns>Return true if the list has changed</returns>
		public static bool AddStation( string stationName )
		{
			bool listChanged = false;

			int foundIndex = stations.IndexOf( stationName );

			// If the station is not already in the list add it to the front of the list.
			if ( foundIndex < 0 )
			{
				stations.Insert( 0, stationName );
				if ( stations.Count > MaxStations )
				{
					stations.RemoveAt( MaxStations );
				}

				listChanged = true;
			}
			// If the station is in the list, and is not at the front then remove it from the list and add to the front.
			else if ( foundIndex > 0 )
			{
				stations.RemoveAt( foundIndex );
				stations.Insert( 0, stationName );
				listChanged = true;
			}

			// If the list has changed then save it back to persistent storage
			if ( listChanged == true )
			{
				SaveStations();
			}
			return listChanged;
		}

		/// <summary>
		/// Load upto MaxStations station names from the persistent storage
		/// </summary>
		private static void LoadStations()
		{
			// Load the stations
			int numberOfRecentStations = Math.Min( PersistentStorage.GetIntItem( RecentStationsSizeName, 0 ), MaxStations );
			stations = new List< string >();

			for ( int stationIndex = 0; stationIndex < numberOfRecentStations; ++stationIndex )
			{
				stations.Add( PersistentStorage.GetStringItem( RecentStationName + stationIndex, "" ) );
			}
		}

		/// <summary>
		/// Save the entire station list to storage
		/// </summary>
		private static void SaveStations()
		{
			PersistentStorage.SetIntItem( RecentStationsSizeName, stations.Count );

			for ( int stationIndex = 0; stationIndex < stations.Count; ++stationIndex )
			{
				PersistentStorage.SetStringItem( RecentStationName + stationIndex, stations[ stationIndex ] );
			}
		}

		/// <summary>
		/// Persistent storage names
		/// </summary>
		private const string RecentStationsSizeName = "RecentStationsSize";
		private const string RecentStationName = "RecentStation";

		/// <summary>
		/// Maximum number of recent stations
		/// </summary>
		private const int MaxStations = 4;

		/// <summary>
		/// The list of most recently used station names
		/// </summary>
		private static List< string > stations = null;
	}
}