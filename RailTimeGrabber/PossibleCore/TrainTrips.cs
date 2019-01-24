using System;
using System.Collections.Generic;

namespace RailTimeGrabber
{
	/// <summary>
	/// A collection of train trips and the currently selected trip
	/// </summary>
	class TrainTrips
	{
		public static List<TrainTrip> Trips
		{
			get
			{
				if ( trips == null )
				{
					LoadTrips();
				}

				return trips;
			}

		}

		/// <summary>
		/// Load the collection of train trips
		/// </summary>
		private static void LoadTrips()
		{
			// Load the trips
			int numberOfTrips = PersistentStorage.GetIntItem( TrainTripsSizeName, 0 );
			trips = new List<TrainTrip>();

			for ( int tripIndex = 0; tripIndex < numberOfTrips; ++tripIndex )
			{
				trips.Add( new TrainTrip { From = PersistentStorage.GetStringItem( TrainTripFromName + tripIndex, "" ),
										   To = PersistentStorage.GetStringItem( TrainTripToName + tripIndex, "" ) } );
			}

			// Get the current trip
			selectedTrip = PersistentStorage.GetIntItem( TrainTripSelectedName, 0 );

			// Make sure that the selected trip is valid
			if ( selectedTrip >= trips.Count )
			{
				selectedTrip = trips.Count - 1;
				PersistentStorage.SetIntItem( TrainTripSelectedName, selectedTrip );
			}
		}

		/// <summary>
		/// Add a new trip to the collection and store.
		/// </summary>
		/// <param name="trip"></param>
		public static void AddTrip( TrainTrip trip )
		{
			// Store the trip
			PersistentStorage.SetStringItem( TrainTripFromName + Trips.Count, trip.From );
			PersistentStorage.SetStringItem( TrainTripToName + Trips.Count, trip.To );

			// Add to the list
			Trips.Add( trip );

			// Update the count
			PersistentStorage.SetIntItem( TrainTripsSizeName, Trips.Count );
		}

		public static void DeleteTrip( int index )
		{
			// Delete the item at the index and save the entire list back to storage
			Trips.RemoveAt( index );
			SaveTrips();
		}

		/// <summary>
		/// The currently selected trip index
		/// </summary>
		public static int Selected
		{
			get
			{
				return selectedTrip;
			}

			set
			{
				if ( value < Trips.Count )
				{
					selectedTrip = value;
					PersistentStorage.SetIntItem( TrainTripSelectedName, selectedTrip );
				}
			}
		}

		/// <summary>
		/// The selected trip
		/// </summary>
		public static TrainTrip SelectedTrip
		{
			get
			{
				return Trips[ selectedTrip ];
			}
		}

		public static bool IsDuplicateTrip( string fromStation, string toStation )
		{
			return ( Array.FindAll( Trips.ToArray(), trip => ( trip.From == fromStation ) && ( trip.To == toStation ) ).Length > 0 );
		}

		/// <summary>
		/// Save the entire trip list to storage
		/// </summary>
		private static void SaveTrips()
		{
			PersistentStorage.SetIntItem( TrainTripsSizeName, trips.Count );

			for ( int tripIndex = 0; tripIndex < trips.Count; ++tripIndex )
			{
				PersistentStorage.SetStringItem( TrainTripFromName + tripIndex, trips[ tripIndex ].From );
				PersistentStorage.SetStringItem( TrainTripToName + tripIndex, trips[ tripIndex ].To );
			}
		}

		/// <summary>
		/// Persistent storage names
		/// </summary>
		private const string TrainTripsSizeName = "TrainTripsSize";
		private const string TrainTripFromName = "TrainTripFrom";
		private const string TrainTripToName = "TrainTripTo";
		private const string TrainTripSelectedName = "TrainTripSelected";

		/// <summary>
		/// The currently selected trip
		/// </summary>
		private static int selectedTrip = -1;

		/// <summary>
		/// The list of trips
		/// </summary>
		private static List<TrainTrip> trips = null;
	}
}