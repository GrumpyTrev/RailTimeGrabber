﻿using Android.App;
using Android.OS;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Widget;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Android.Views;
using Android.Content;
using Android.Runtime;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace RailTimeGrabber
{
	[Activity( Label = "@string/app_name", MainLauncher = true )]
	public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.activity_main );

			// Initialise the action bar 
			SetSupportActionBar( FindViewById<Toolbar>( Resource.Id.toolbar ) );

			// Create a StorageMechanism instance
			PersistentStorage.StorageMechanism = new StorageMechanism( this );

			// Hide the progress bar 
			loadingProgress = FindViewById<ProgressBar>( Resource.Id.loadProgress );
			loadingProgress.Visibility = ViewStates.Invisible;

			// Fill the spinner control with all the available trips and set the selected trip
			tripSpinner = FindViewById<TripSpinner>( Resource.Id.spinner );

			// Create adapter to supply these strings. Use a custom layout for the selected item but the standard layout for the dropdown
			spinnerAdapter = new TripAdapter( this, Resource.Layout.spinner_item, TripStrings(), tripSpinner );
			spinnerAdapter.SetDropDownViewResource( Android.Resource.Layout.SimpleSpinnerDropDownItem );

			// Link the spinner with the trip data and display the selected trip
			tripSpinner.Adapter = spinnerAdapter;
			tripSpinner.SetSelection( TrainTrips.Selected );

			// Put an empty set of results into an adapter and assign the adapter to the list view. 
			journeyAdapter = new TrainJourneyWrapper( this, new TrainJourney[ 0 ] );
			( ( ListView )FindViewById<ListView>( Resource.Id.listView1 ) ).Adapter = journeyAdapter;

			// Create a journey request obect to perform the web access and link into its event
			trainJourneyRequest = new JourneyRequest();
			trainJourneyRequest.JourneysAvailableEvent += JourneysAvailable;

			// Get the train journeys for the current trip (if there is one )
			if ( TrainTrips.Selected >= 0 )
			{
				loadingProgress.Visibility = ViewStates.Visible;

				// This is an synch call. It will load the results into the TrainJourneyWrapper when available
				trainJourneyRequest.GetJourneys( TrainTrips.SelectedTrip.From, TrainTrips.SelectedTrip.To );
			}

			// Trap the spinner selection after the initial request
			tripSpinner.ItemSelected += TripItemSelected;

			// Trap the trip list long click
			spinnerAdapter.LongClickEvent += TripLongClick;
		}

		/// <summary>
		/// Called during creation to allow a toolbar menu to be created
		/// </summary>
		/// <param name="menu"></param>
		/// <returns></returns>
		public override bool OnCreateOptionsMenu( IMenu menu )
		{
			MenuInflater.Inflate( Resource.Menu.toolbar_menu, menu );
			return base.OnCreateOptionsMenu( menu );
		}

		/// <summary>
		/// Called when a menu item has been selected
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			if ( item.ItemId == Resource.Id.menu_new )
			{
				StartActivityForResult( new Intent( this, typeof( AddTripActivity ) ), 0 );
			}

			return base.OnOptionsItemSelected( item );
		}

		/// <summary>
		/// Called when another activity stated by this activity exits
		/// Because only the AddTripActivity is the only activity started there is no need to check the request code.
		/// </summary>
		/// <param name="requestCode"></param>
		/// <param name="resultCode"></param>
		/// <param name="data"></param>
		protected override void OnActivityResult( int requestCode, [GeneratedEnum] Result resultCode, Intent data )
		{
			if ( resultCode == Result.Ok )
			{
				// Refresh the spinner adpater in case a trip was added
				spinnerAdapter.Clear();
				spinnerAdapter.AddAll( TripStrings() );
			}
		}

		/// <summary>
		/// Called when a new trip has been selected.
		/// Get the journeys for this trip
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void TripItemSelected( object sender, AdapterView.ItemSelectedEventArgs args )
		{
			// Update the selection
			TrainTrips.Selected = args.Position;

			// Clear the currently displayed data as this may take a while
			journeyAdapter.Items = new TrainJourney[ 0 ];
			journeyAdapter.NotifyDataSetChanged();

			loadingProgress.Visibility = ViewStates.Visible;

			// Get the journeys for the new trip
			// This is an synch call. It will load the results into the TrainJourneyWrapper when available
			trainJourneyRequest.GetJourneys( TrainTrips.SelectedTrip.From, TrainTrips.SelectedTrip.To );
		}

		/// <summary>
		/// Called when a trip item has been long clicked.
		/// Confirm deletion and then delete the item.
		/// This may involve changing the currently selected trip
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TripLongClick( object sender, TripAdapter.LongClickEventArgs e )
		{
			TrainTrip tripToDelete = TrainTrips.Trips[ e.TripPosition ];

			// Display a confirmation dialogue
			new AlertDialog.Builder( this )
				.SetTitle( "Confirm trip deletion" )
				.SetMessage( string.Format( "Do you realy want to delete the '{0} to {1}' trip?", tripToDelete.From, tripToDelete.To ) )
				.SetPositiveButton( "Yes", ( senderAlert, args ) => {

					// If the position of the trip to delete is greater than the currently selected trip then just delete it
					if ( e.TripPosition > TrainTrips.Selected )
					{
						// Delete the trip
						TrainTrips.DeleteTrip( e.TripPosition );

						// Refresh the spinner adapter
						spinnerAdapter.Clear();
						spinnerAdapter.AddAll( TripStrings() );
					}
					// If the position of the trip to delete is less than the currently selected trip the delete it and reduce the
					// index of the selected trip
					else if ( e.TripPosition < TrainTrips.Selected )
					{
						// Delete the trip
						TrainTrips.DeleteTrip( e.TripPosition );

						// Refresh the spinner adapter
						spinnerAdapter.Clear();
						spinnerAdapter.AddAll( TripStrings() );

						// Select the previous trip
						tripSpinner.SetSelection( TrainTrips.Selected - 1 );
					}
					// If the selected trip is being deleted then either keep the selected index the same and refresh it, or if
					// the end of the list has been reached then reduce the selected trip index. If there are no items left then set the index to -1
					// and make sure that the results are cleared
					else
					{
						// Delete the trip
						TrainTrips.DeleteTrip( e.TripPosition );

						// Refresh the spinner adapter
						spinnerAdapter.Clear();
						spinnerAdapter.AddAll( TripStrings() );

						if ( TrainTrips.Selected >= TrainTrips.Trips.Count )
						{
							if ( TrainTrips.Selected > 0 )
							{
								// Select the previous trip
								tripSpinner.SetSelection( TrainTrips.Selected - 1 );
							}
							else
							{
								// Clear the selection. This will not cause the selected event to be called so clear the journey details explicitly
								tripSpinner.SetSelection( -1 );

								TrainTrips.Selected = -1;

								// Clear the currently displayed data
								journeyAdapter.Items = new TrainJourney[ 0 ];
								journeyAdapter.NotifyDataSetChanged();
							}
						}
						else
						{
							// Need to simulate a selection changed event
							TripItemSelected( null, new AdapterView.ItemSelectedEventArgs( null, null, TrainTrips.Selected, 0 ) );
						}
					}
				} )
				.SetNegativeButton( "No", ( EventHandler<DialogClickEventArgs> )null )
				.Create()
				.Show();
		}

		/// <summary>
		/// Return a formatted list of the trips to be displayed in the dropdown spinner
		/// </summary>
		/// <returns></returns>
		private List<string> TripStrings()
		{
			List<string> tripStrings = new List<string>();
			foreach ( TrainTrip trip in TrainTrips.Trips )
			{
				tripStrings.Add( string.Format( "{0} to {1}", trip.From, trip.To ) );
			}

			return tripStrings;
		}

		/// <summary>
		/// Called when the JourneyRequest object has retrieved some journeys to display
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void JourneysAvailable( object sender, JourneyRequest.JourneysAvailableArgs args )
		{
			if ( args.JourneysAvailable == true )
			{
				// Display them
				journeyAdapter.Items = trainJourneyRequest.Journeys;
				journeyAdapter.NotifyDataSetChanged();
			}
			else
			{
				Toast.MakeText( this, "Problem accessing network. Check network settings.", ToastLength.Long ).Show();
			}

			loadingProgress.Visibility = ViewStates.Invisible;
		}

		/// <summary>
		/// Adapter used to provide data to the list view showing the journeys
		/// </summary>
		private TrainJourneyWrapper journeyAdapter = null;

		/// <summary>
		/// Adapter used to hold the trips for the spinner
		/// </summary>
		private TripAdapter spinnerAdapter = null;

		/// <summary>
		/// Spinner control used to hold the trip list
		/// </summary>
		private TripSpinner tripSpinner = null;

		/// <summary>
		/// The progress bar to show when content is being obtained from the internet
		/// </summary>
		private ProgressBar loadingProgress = null;

		/// <summary>
		/// JourneyRequest instance used to access the web for journey details
		/// </summary>
		private JourneyRequest trainJourneyRequest = null;
	}
}