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
		}

		/// <summary>
		/// Retrieve a set of journeys for the specified from and to stations.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public async void GetJourneys( string from, string to )
		{
			// Need to use a non-default HttpClientHandler to hold the cookies
			using ( HttpClientHandler clientHandler = new HttpClientHandler() )
			{
				// Cookies are required by the website to hold a session id
				clientHandler.CookieContainer = new CookieContainer();

				// Use a HttpClient for the requests
				using ( HttpClient client = new HttpClient( clientHandler ) )
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

							// Save the journeys so they can be accessed from outside the class
							Journeys = journeys.Journeys.ToArray();

							JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = true } );
						}
					}
					catch ( HttpRequestException )
					{
						JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = false } );
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
	}
}