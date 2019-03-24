using Android.App;
using Android.OS;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Widget;
using System;
using System.Collections.Generic;
using Android.Views;
using Android.Content;
using Android.Runtime;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace RailTimeGrabber
{
	[Activity( Label = "@string/app_name", MainLauncher = true )]
	public class MainActivity : AppCompatActivity, IJourneyResponse
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
			spinnerAdapter = new TripAdapter( this, Resource.Layout.spinner_item, TrainTrips.TripStrings(), tripSpinner );
			spinnerAdapter.SetDropDownViewResource( Android.Resource.Layout.SimpleSpinnerDropDownItem );

			// Link the spinner with the trip data and display the selected trip
			tripSpinner.Adapter = spinnerAdapter;
			tripSpinner.SetSelection( TrainTrips.Selected );

			// Put an empty set of results into an adapter and assign the adapter to the list view. 
			journeyList = FindViewById<ListView>( Resource.Id.journeysView );
			journeyAdapter = new TrainJourneyWrapper( this, new TrainJourney[ 0 ] );

			journeyList.Adapter = journeyAdapter;

			// Link into the adapters More Journeys button
			journeyAdapter.MoreJourneysEvent += MoreJourneysRequest;

			// Link this activity with responses from the JourneyRetrieval instance
			trainJourneyRetrieval.JourneyResponse = this;

			// Trap clicking on the manual update field
			updateText = FindViewById<TextView>( Resource.Id.updateText );
			updateText.Click += PerformManualUpdate;

			// Create a ResultsTimer to handle updating the results 'age' message
			updateTimer = new ResultsTimer();
			updateTimer.TextChangedEvent += UpdateTextChanged;
			updateTimer.ResetTimer();

			// Get the train journeys for the current trip (if there is one )
			if ( TrainTrips.Selected >= 0 )
			{
				// Keep track of the selected trip details so that any changes can be validated
				selectedTrainTrip = TrainTrips.SelectedTrip;

				PrepareForRequest( true );
				
				// This is an synch call. It will load the results into the TrainJourneyWrapper when available
				trainJourneyRetrieval.GetJourneys( TrainTrips.SelectedTrip );
			}

			// Trap the spinner selection after the initial request
			tripSpinner.ItemSelected += TripItemSelected;

			// Trap the spinner being opened
			tripSpinner.ClickedEvent += TripSpinnerClicked;

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
				// Cancel any request currently in progress and start the Add Trip activity
				trainJourneyRetrieval.CancelRequest();

				StartActivityForResult( new Intent( this, typeof( AddTripActivity ) ), 0 );
			}

			return base.OnOptionsItemSelected( item );
		}

		/// <summary>
		/// Called when the set of journeys have been cleared, either at the start of a request or if no journeys have been found
		/// </summary>
		public void JourneysCleared()
		{
			journeyAdapter.Items = new TrainJourney[ 0 ];
			journeyAdapter.NotifyDataSetChanged();
		}

		/// <summary>
		/// Called when either a new set of journeys or an extended set of journeys have been retrieved
		/// </summary>
		/// <param name="journeys"></param>
		public void JourneysAvailable( TrainJourneys journeys )
		{
			// Display them
			journeyAdapter.Items = journeys.Journeys.ToArray();
			journeyAdapter.NotifyDataSetChanged();

			// Update the manual update text
			updateTimer.JustUpdated();
		}

		/// <summary>
		///  Called when the request has completed
		/// </summary>
		/// <param name="networkProblem"></param>
		/// <param name="noJourneysFound"></param>
		public void JourneyRequestComplete( bool networkProblem, bool noJourneysFound )
		{
			if ( networkProblem == true )
			{
				Toast.MakeText( this, "Problem accessing network. Check network settings.", ToastLength.Long ).Show();
			}
			else if ( noJourneysFound == true )
			{
				Toast.MakeText( this, "No journeys found.", ToastLength.Long ).Show();
			}

			PrepareForRequest( false );
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
				// Refresh the spinner adapter
				spinnerAdapter.Clear();
				spinnerAdapter.AddAll( TrainTrips.TripStrings() );

				// Assume that the user wants to display results for the added trip, so select it.
				tripSpinner.SetSelection( TrainTrips.Trips.Count - 1 );
			}
		}

		/// <summary>
		/// Called when the More Journeys buttons has been clicked.
		/// Pass on this request to the JourneyRetrieval class
		/// </summary>
		private void MoreJourneysRequest()
		{
			PrepareForRequest( true );

			// Get more journeys for the trip
			trainJourneyRetrieval.MoreJourneys();
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

			// If the selected trip has not changed, i.e. this has been called due to a trip deletion, then don't bother
			// getting the journeys again.
			if ( ( selectedTrainTrip == null ) || ( selectedTrainTrip.From != TrainTrips.SelectedTrip.From ) || ( selectedTrainTrip.To != TrainTrips.SelectedTrip.To ) )
			{
				selectedTrainTrip = TrainTrips.SelectedTrip;

				PrepareForRequest( true );

				// Get the journeys for the new trip
				trainJourneyRetrieval.GetJourneys( TrainTrips.SelectedTrip );
			}
		}

		/// <summary>
		/// Called when the trip spinner has been opened.
		/// Cancel any ongoing request
		/// </summary>
		private void TripSpinnerClicked()
		{
			trainJourneyRetrieval.CancelRequest();
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
						spinnerAdapter.ReloadSpinner( TrainTrips.TripStrings() );
					}
					// If the position of the trip to delete is less than the currently selected trip the delete it and reduce the
					// index of the selected trip
					else if ( e.TripPosition < TrainTrips.Selected )
					{
						// Delete the trip
						TrainTrips.DeleteTrip( e.TripPosition );

						// Refresh the spinner adapter
						spinnerAdapter.ReloadSpinner( TrainTrips.TripStrings() );

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
						spinnerAdapter.ReloadSpinner( TrainTrips.TripStrings() );

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
								trainJourneyRetrieval.ClearJourneys();

								// Reset the update time
								updateTimer.ResetTimer();
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
		/// Called when the update text is clicked
		/// Perform an update
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PerformManualUpdate( object sender, EventArgs e )
		{
			PrepareForRequest( true );

			// This is an synch call. It will load the results into the TrainJourneyWrapper when available
			trainJourneyRetrieval.UpdateJourneys();
		}

		/// <summary>
		/// Update the text showing the age of the results
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void UpdateTextChanged( object sender, ResultsTimer.TextChangedEventArgs args )
		{
			RunOnUiThread( () => {
				updateText.Text = args.Text;
			} );
		}

		/// <summary>
		/// Carry out common UI work prior to sending off or receiving a journeys request
		/// </summary>
		private void PrepareForRequest( bool requestInProgress )
		{
			// Show or hide the progress bar
			loadingProgress.Visibility = ( requestInProgress == true ) ? ViewStates.Visible : ViewStates.Invisible;

			// Disable or enable the update text and the results list view
			updateText.Enabled = !requestInProgress;
			journeyList.Enabled = !requestInProgress;

			// Disable the button as well
			if ( journeyAdapter.MoreButton != null )
			{
				journeyAdapter.MoreButton.Enabled = !requestInProgress;
			}
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
		/// JourneyRetrieval instance used to get journeys for teh selected trip
		/// </summary>
		private JourneyRetrieval trainJourneyRetrieval = new JourneyRetrieval();

		/// <summary>
		/// The currently selected train trip
		/// </summary>
		private TrainTrip selectedTrainTrip = null;

		/// <summary>
		/// The manual update text field
		/// </summary>
		private TextView updateText = null;

		/// <summary>
		/// The list view displaying the results of the web request
		/// </summary>
		private ListView journeyList = null;

		/// <summary>
		/// ResultsTimer instance used to handle updating the results 'age' text
		/// </summary>
		private ResultsTimer updateTimer = null;
	}
}