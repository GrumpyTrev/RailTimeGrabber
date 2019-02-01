using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RailTimeGrabber
{
	/// <summary>
	/// Encapsulates the retrieval of train journey information from the National Rail web site
	/// </summary>
	class JourneyRequest
	{
		public JourneyRequest()
		{
			// Create a client using a handler with a cookie container and a timeout
			client = new HttpClient( new HttpClientHandler { CookieContainer = new CookieContainer() } );
			client.Timeout = TimeSpan.FromSeconds( ClientRequestTimeout );
		}

		/// <summary>
		/// Retrieve a set of journeys for the specified from and to stations.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public async void GetJourneys( string from, string to )
		{
			try
			{
				// Check if a previously obtained session id cookie is still valid
				if ( ( DateTime.Now - sessionCookieTime ).TotalMinutes > SeesionCookieValidTimeInMinutes )
				{
					// Must load the search page in order to set the session id cookie
					await client.GetAsync( "http://ojp.nationalrail.co.uk/service/planjourney/search" );

					// Save the time that this session id was obtained
					sessionCookieTime = DateTime.Now;
				}

				// Set up the request for one minute from now
				DateTime requestTime = DateTime.Now + TimeSpan.FromMinutes( 1 );

				// Get the journeys
				HttpResponseMessage response = await client.PostAsync( "http://ojp.nationalrail.co.uk/service/planjourney/plan",
					new FormUrlEncodedContent( new Dictionary<string, string> { { "commandName", "journeyPlannerCommand" },
						{ "from.searchTerm", TrainTrip.ToWebFormat( from ) }, { "timeOfOutwardJourney.arrivalOrDeparture", "DEPART" },
						{ "timeOfOutwardJourney.hour", requestTime.Hour.ToString() }, { "timeOfOutwardJourney.minute",  requestTime.Minute.ToString() },
						{ "timeOfOutwardJourney.monthDay", "Today" }, { "to.searchTerm", TrainTrip.ToWebFormat( to ) } } ) );

				// Load the result of the request into an HtmlDocument
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml( await response.Content.ReadAsStringAsync() );

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

					// Save the journeys so they can be accessed from outside the class
					Journeys = journeys.Journeys.ToArray();

					JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = true } );
				}
			}
			catch ( HttpRequestException )
			{
				JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = false } );
			}
			catch ( TaskCanceledException )
			{
				JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = false } );
			}
			catch ( OperationCanceledException )
			{
				JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = false } );
			}
		}

		/// <summary>
		/// Eventhandler used to notify when the journeys are available
		/// </summary>
		public event EventHandler<JourneysAvailableArgs> JourneysAvailableEvent;

		/// <summary>
		/// The journeys retrived from the last request
		/// </summary>
		public TrainJourney[] Journeys { get; private set; }

		/// <summary>
		/// The arguments for the JourneysAvailableEvent.
		/// Were arguements returned or was there a problem
		/// </summary>
		public class JourneysAvailableArgs: EventArgs
		{
			public bool JourneysAvailable { get; set; }
		}

		/// <summary>
		/// The client used for all requests
		/// </summary>
		private HttpClient client = null;

		/// <summary>
		/// The time that the session cookie was obtained.
		/// It is valid for 10 minutes
		/// </summary>
		private DateTime sessionCookieTime = DateTime.MinValue;

		/// <summary>
		/// Time difference used to check for session id cookie validity
		/// </summary>
		private const double SeesionCookieValidTimeInMinutes = 9.5;

		/// <summary>
		/// Timeout for client requests
		/// </summary>
		private const int ClientRequestTimeout = 20;
	}
}