using Android.App;
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

			// Get the train journeys for the current trip (if there is one )
			if ( TrainTrips.Selected >= 0 )
			{
				// This is an synch call. It will load the results into the TrainJourneyWrapper when available
				GetTrainJourneys( TrainTrips.SelectedTrip.From, TrainTrips.SelectedTrip.To );
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

			// Get the journeys for the new trip
			// This is an synch call. It will load the results into the TrainJourneyWrapper when available
			GetTrainJourneys( TrainTrips.SelectedTrip.From, TrainTrips.SelectedTrip.To );
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
		/// Get the next few train journeys for the specified trip
		/// </summary>
		/// <returns></returns>
		private async void GetTrainJourneys( string from, string to )
		{
			// Need to use a non-default HttpClientHandler to hold the cookies
			using ( HttpClientHandler handler = new HttpClientHandler() )
			{
				// Cookies are required by the website to hold a session id
				handler.CookieContainer = new CookieContainer();

				// Use a HttpClient for the requests
				using ( HttpClient client = new HttpClient( handler ) )
				{
					try
					{
						// Must load the search page in order to set the session id cookie
						HttpResponseMessage response = await client.GetAsync( "http://ojp.nationalrail.co.uk/service/planjourney/search" );

						// Set up the request for one minute from now
						DateTime requestTime = DateTime.Now + TimeSpan.FromMinutes( 1 );

						// Load the result of the request into an HtmlDocument
						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml( await MakeRequest( client, TrainTrip.ToWebFormat( from ), TrainTrip.ToWebFormat( to ), requestTime.Hour, requestTime.Minute ) );

						// Any journeys
						HtmlNodeCollection dormNodes = doc.DocumentNode.SelectNodes( "//td[@class='dep']/.." );
						if ( dormNodes != null )
						{
							TrainJourneys journeys = new TrainJourneys();

							// Fill the journeys list with the results
							foreach ( HtmlNode journeyNode in dormNodes )
							{
								string departureTime = journeyNode.SelectSingleNode( "./td[@class='dep']" ).InnerText.Substring( 0, 5 );
								string arrivalTime = journeyNode.SelectSingleNode( "./td[@class='arr']" ).InnerText.Substring( 0, 5 );
								string duration = journeyNode.SelectSingleNode( "./td[@class='dur']" ).InnerText.Replace( "\n", "" ).Replace( "\t", "" );
								string status = journeyNode.SelectSingleNode( "./td[@class='status']" ).InnerText.Replace( "\n", "" ).Replace( "\t", "" );

								journeys.Journeys.Add( new TrainJourney {
									ArrivalTime = arrivalTime, DepartureTime = departureTime, Duration = duration,
									Status = status
								} );
							}

							// Display them
							journeyAdapter.Items = journeys.Journeys.ToArray();
							journeyAdapter.NotifyDataSetChanged();
						}
					}
					catch ( HttpRequestException )
					{
						Toast.MakeText( this, "Problem accessing network. Check network settings.", ToastLength.Long ).Show();
					}
				}
			}
		}

		/// <summary>
		/// Make a request for trains between the specified stations
		/// </summary>
		/// <param name="client"></param>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="hours"></param>
		/// <param name="minutes"></param>
		/// <returns></returns>
		private async Task<string> MakeRequest( HttpClient client, string from, string to, int hours, int minutes )
		{
			HttpResponseMessage response = await client.PostAsync( "http://ojp.nationalrail.co.uk/service/planjourney/plan",
				new FormUrlEncodedContent( new Dictionary<string, string> { { "commandName", "journeyPlannerCommand" }, { "from.searchTerm", from },
					{ "timeOfOutwardJourney.arrivalOrDeparture", "DEPART" }, { "timeOfOutwardJourney.hour", hours.ToString() },
					{ "timeOfOutwardJourney.minute",  minutes.ToString() }, { "timeOfOutwardJourney.monthDay", "Today" }, { "to.searchTerm", to } } ) );

			return await response.Content.ReadAsStringAsync();
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
	}
}