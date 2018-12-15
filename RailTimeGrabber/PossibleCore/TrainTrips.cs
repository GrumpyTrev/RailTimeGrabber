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
	/// A collection of train trips and the currently selected trip
	/// </summary>
	class TrainTrips
	{
		public List<TrainTrip> Trips { get; private set; } = new List<TrainTrip>();

		/// <summary>
		/// Load the collection of train trips
		/// </summary>
		public void LoadTrips()
		{
			// Load the trips
			int numberOfTrips = PersistentStorage.GetIntItem( TrainTripsSizeName, 0 );

			for ( int tripIndex = 0; tripIndex < numberOfTrips; ++tripIndex )
			{
				Trips.Add( new TrainTrip { From = PersistentStorage.GetStringItem( TrainTripFromName + tripIndex, "" ),
										   To = PersistentStorage.GetStringItem( TrainTripToName + tripIndex, "" ) } );
			}

			// Get the current trip
			selectedTrip = PersistentStorage.GetIntItem( TrainTripSelectedName, 0 );
		}

		/// <summary>
		/// Add a new trip to the collection and store.
		/// </summary>
		/// <param name="trip"></param>
		public void AddTrip( TrainTrip trip )
		{
			// Store the trip
			PersistentStorage.SetStringItem( TrainTripFromName + Trips.Count, trip.From );
			PersistentStorage.SetStringItem( TrainTripToName + Trips.Count, trip.To );

			// Add to the list
			Trips.Add( trip );

			// Update the count
			PersistentStorage.SetIntItem( TrainTripsSizeName, Trips.Count );
		}

		/// <summary>
		/// The currently selected trip
		/// </summary>
		public int Selected
		{
			get
			{
				return selectedTrip;
			}

			set
			{
				if ( ( value >= 0 ) && ( value < Trips.Count ) )
				{
					selectedTrip = value;
					PersistentStorage.SetIntItem( TrainTripSelectedName, selectedTrip );
				}
			}
		}

		public TrainTrip SelectedTrip
		{
			get
			{
				return Trips[ selectedTrip ];
			}
		}

		private const string TrainTripsSizeName = "TrainTripsSize";
		private const string TrainTripFromName = "TrainTripFrom";
		private const string TrainTripToName = "TrainTripTo";
		private const string TrainTripSelectedName = "TrainTripSelected";

		private int selectedTrip = -1;
	}
}