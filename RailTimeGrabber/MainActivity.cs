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

namespace RailTimeGrabber
{
	[Activity( Label = "@string/app_name", MainLauncher = true )]
	public class MainActivity : AppCompatActivity
    {
        async protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.activity_main );

			// Initialise the action bar 
			SetSupportActionBar( FindViewById<Toolbar>( Resource.Id.toolbar ) );

			// Create a StorageMechanism instance
			PersistentStorage.StorageMechanism = new StorageMechanism( this );

			// Get the set of trips from storage
			trainTripsCollection.LoadTrips();

			// Form a list of all the trips as formatted strings for the spinner
			List<string> tripStrings = new List<string>();
			foreach ( TrainTrip trip in trainTripsCollection.Trips )
			{
				tripStrings.Add( string.Format( "{0} to {1}", trip.From, trip.To ) );
			}

			// Add to the spinner and display the selected trip
			// Fill the spinner control with all the available trips and set the selected trip
			TripSpinner tripSpinner = FindViewById<TripSpinner>( Resource.Id.spinner );

			// Create adapter to supply these strings. Use a custom layout for the selected item but the standard layout for the dropdown
			TripAdapter spinnerAdapter = new TripAdapter( this, Resource.Layout.spinner_item, tripStrings, tripSpinner );
			spinnerAdapter.SetDropDownViewResource( Android.Resource.Layout.SimpleSpinnerDropDownItem );

			// Link the spinner with the trip data and display the selected trip
			tripSpinner.Adapter = spinnerAdapter;
			tripSpinner.SetSelection( trainTripsCollection.Selected );

			// Get the train journeys for the current trip
			TrainJourneys journeys = await GetTrainJourneys( trainTripsCollection.SelectedTrip.From, trainTripsCollection.SelectedTrip.To );

			// Put results into an adapter and assign the adapter to the list view. 
			// Put the adapter in a class variable so that it can be refreshed with new data
			journeyAdapter = new TrainJourneyWrapper( this, journeys.Journeys.ToArray() );
			( ( ListView )FindViewById<ListView>( Resource.Id.listView1 ) ).Adapter = journeyAdapter;

			// Trap the spinner selection after the initial request
			tripSpinner.ItemSelected += TripItemSelected;
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
				StartActivity( new Intent( this, typeof( AddTripActivity ) ) );
			}
			//			Toast.MakeText( this, "Action selected: " + item.TitleFormatted, ToastLength.Short ).Show();
			return base.OnOptionsItemSelected( item );
		}

		/// <summary>
		/// Called when a new trip has been selected.
		/// Get the journeys for this trip
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		async private void TripItemSelected( object sender, AdapterView.ItemSelectedEventArgs args )
		{
			// Update the selection
			trainTripsCollection.Selected = args.Position;

			// Clear the currently displayed data as this may take a while
			journeyAdapter.Items = new TrainJourney[ 0 ];
			journeyAdapter.NotifyDataSetChanged();

			// Get the journeys for the new trip
			TrainJourneys journeys = await GetTrainJourneys( trainTripsCollection.SelectedTrip.From, trainTripsCollection.SelectedTrip.To );

			// Display them
			journeyAdapter.Items = journeys.Journeys.ToArray();
			journeyAdapter.NotifyDataSetChanged();
		}

		/// <summary>
		/// Get the next few train journeys for the specified trip
		/// </summary>
		/// <returns></returns>
		async Task<TrainJourneys> GetTrainJourneys( string from, string to )
		{
			TrainJourneys returnedJourneys = new TrainJourneys();

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
						await response.Content.ReadAsStringAsync();

						// Set up the request for one minute from now
						DateTime requestTime = DateTime.Now + TimeSpan.FromMinutes( 1 );

						// Load the result of the request into an HtmlDocument
						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml( await MakeRequest( client, TrainTrip.ToWebFormat( from ), TrainTrip.ToWebFormat( to ), requestTime.Hour, requestTime.Minute ) );

						// Any journeys
						HtmlNodeCollection dormNodes = doc.DocumentNode.SelectNodes( "//td[@class='dep']/.." );
						if ( dormNodes != null )
						{
							// Fill the journeys list with the results
							foreach ( HtmlNode journeyNode in dormNodes )
							{
								string departureTime = journeyNode.SelectSingleNode( "./td[@class='dep']" ).InnerText.Substring( 0, 5 );
								string arrivalTime = journeyNode.SelectSingleNode( "./td[@class='arr']" ).InnerText.Substring( 0, 5 );
								string duration = journeyNode.SelectSingleNode( "./td[@class='dur']" ).InnerText.Replace( "\n", "" ).Replace( "\t", "" );
								string status = journeyNode.SelectSingleNode( "./td[@class='status']" ).InnerText.Replace( "\n", "" ).Replace( "\t", "" );

								returnedJourneys.Journeys.Add( new TrainJourney {
									ArrivalTime = arrivalTime, DepartureTime = departureTime, Duration = duration,
									Status = status
								} );
							}
						}
					}
					catch ( HttpRequestException requestException )
					{
						Toast.MakeText( this, "Problem accessing network. Check network settings.", ToastLength.Long ).Show();
					}
				}
			}

			return returnedJourneys;
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
		static async Task<string> MakeRequest( HttpClient client, string from, string to, int hours, int minutes )
		{
			HttpResponseMessage response = await client.PostAsync( "http://ojp.nationalrail.co.uk/service/planjourney/plan",
				new FormUrlEncodedContent( new Dictionary<string, string> { { "commandName", "journeyPlannerCommand" }, { "from.searchTerm", from },
					{ "timeOfOutwardJourney.arrivalOrDeparture", "DEPART" }, { "timeOfOutwardJourney.hour", hours.ToString() },
					{ "timeOfOutwardJourney.minute",  minutes.ToString() }, { "timeOfOutwardJourney.monthDay", "Today" }, { "to.searchTerm", to } } ) );

			return await response.Content.ReadAsStringAsync();
		}

		/// <summary>
		/// Adapter used to provide data to the list view showing the journeys
		/// </summary>
		private TrainJourneyWrapper journeyAdapter = null;

		/// <summary>
		/// The set of train trips configured into the system
		/// </summary>
		private TrainTrips trainTripsCollection = new TrainTrips();
	}
}