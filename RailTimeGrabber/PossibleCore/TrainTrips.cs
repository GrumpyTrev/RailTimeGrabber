using System;
using System.Collections.Generic;

namespace RailTimeGrabber
{
	/// <summary>
	/// A collection of train trips and the currently selected trip
	/// </summary>
	class TrainTrips
	{
		/// <summary>
		/// The train trips collection. Read from storage if not already read.
		/// </summary>
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
		/// Return a formatted list of the trips to be displayed
		/// </summary>
		/// <returns></returns>
		public static List<string> TripStrings()
		{
			List<string> tripStrings = new List<string>();
			foreach ( TrainTrip trip in Trips )
			{
				tripStrings.Add( string.Format( "{0} to {1}", trip.From, trip.To ) );
			}

			return tripStrings;
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
				TrainTrip tripToAdd = new TrainTrip {
					From = PersistentStorage.GetStringItem( TrainTripFromName + tripIndex, "" ),
					To = PersistentStorage.GetStringItem( TrainTripToName + tripIndex, "" ),
					FromCode = PersistentStorage.GetStringItem( TrainTripFromCodeName + tripIndex, "" ),
					ToCode = PersistentStorage.GetStringItem( TrainTripToCodeName + tripIndex, "" )
				};
				trips.Add( tripToAdd );
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
			PersistentStorage.SetStringItem( TrainTripFromCodeName + Trips.Count, trip.FromCode );
			PersistentStorage.SetStringItem( TrainTripToCodeName + Trips.Count, trip.ToCode );

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
				if ( trips == null )
				{
					LoadTrips();
				}

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

		/// <summary>
		/// Check if the set of trips already includes the from and to pairing 
		/// </summary>
		/// <param name="fromStation"></param>
		/// <param name="toStation"></param>
		/// <returns></returns>
		public static bool IsDuplicateTrip( string fromStation, string toStation )
		{
			return ( Array.FindAll( Trips.ToArray(), trip => ( trip.From == fromStation ) && ( trip.To == toStation ) ).Length > 0 );
		}

		/// <summary>
		/// Clear the loaded trips so that the next reference will reload them
		/// </summary>
		public static void Reset()
		{
			trips = null;
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
				PersistentStorage.SetStringItem( TrainTripFromCodeName + Trips.Count, trips[ tripIndex ].FromCode );
				PersistentStorage.SetStringItem( TrainTripToCodeName + Trips.Count, trips[ tripIndex ].ToCode );
			}
		}

		/// <summary>
		/// Persistent storage names
		/// </summary>
		private const string TrainTripsSizeName = "TrainTripsSize";
		private const string TrainTripFromName = "TrainTripFrom";
		private const string TrainTripToName = "TrainTripTo";
		private const string TrainTripFromCodeName = "TrainTripFromCode";
		private const string TrainTripToCodeName = "TrainTripToCode";
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