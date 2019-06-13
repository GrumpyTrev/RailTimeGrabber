using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
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

			// Create the request parameter dictionary and put in the fixed items and placeholders for the variable items
			requestParameters = new Dictionary<string, string> { { "commandName", "journeyPlannerCommand" }, { "from.searchTerm", "" },
				{ "timeOfOutwardJourney.arrivalOrDeparture", "DEPART" }, { "timeOfOutwardJourney.hour", "" }, { "timeOfOutwardJourney.minute",  "" },
				{ "timeOfOutwardJourney.monthDay", "" }, { "to.searchTerm", "" } };
		}

		/// <summary>
		/// Retrieve a set of journeys for the specified from and to stations.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public async void GetJourneys( string from, string to, DateTime requestTime, CancellationToken cancelToken )
		{
			try
			{
				// Check if a previously obtained session id cookie is still valid
				if ( ( DateTime.Now - sessionCookieTime ).TotalMinutes > SeesionCookieValidTimeInMinutes )
				{
					// Must load the search page in order to set the session id cookie
					await client.GetAsync( "http://ojp.nationalrail.co.uk/service/planjourney/search", cancelToken );

					// Save the time that this session id was obtained
					sessionCookieTime = DateTime.Now;
				}

				// Set up the variable parameters
				requestParameters[ "from.searchTerm" ] = TrainTrip.ToWebFormat( from );
				requestParameters[ "to.searchTerm" ] = TrainTrip.ToWebFormat( to );
				requestParameters[ "timeOfOutwardJourney.hour" ] = requestTime.Hour.ToString();
				requestParameters[ "timeOfOutwardJourney.minute" ] = requestTime.Minute.ToString();
				requestParameters[ "timeOfOutwardJourney.monthDay" ] = requestTime.ToString( "dd/MM/yy" );

				// Make the request
				HttpResponseMessage response = await client.PostAsync( "http://ojp.nationalrail.co.uk/service/planjourney/plan",
					new FormUrlEncodedContent( requestParameters ), cancelToken );

				// Load the result of the request into an HtmlDocument
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml( await response.Content.ReadAsStringAsync() );

				// Extract the journey, changes and date change nodes
				// The journeys are in table rows that contain cells of class 'dep'. Grab the containing row.
				// The changes associated with a journey are not in the same row as the journey so look for a cell with class 'origin'.
				// Changes are stored in the containing table. The extracted changes are stored with the last journey. 
				// Date changes between journeys are contained in table rows with class 'day-heading'.
				// If date of the first journey are in an H3 element with class 'outward top ctf-h3'. 
				HtmlNodeCollection dormNodes = doc.DocumentNode.SelectNodes( 
					"//td[@class='dep']/..|//td[@class='origin']/..|//h3[@class='outward top ctf-h3']/.|//tr[@class='day-heading']/." );
				if ( dormNodes != null )
				{
					// Assume that the journeys found are initially for the same day as the request
					DateTime responseDate = requestTime.Date;

					TrainJourneys journeys = new TrainJourneys();

					// Fill the journeys list with the results
					foreach ( HtmlNode journeyNode in dormNodes )
					{
						// Check if this is a train node
						if ( journeyNode.SelectSingleNode( "./td[@class='dep']" ) != null )
						{
							TrainJourney newJourney = new TrainJourney {
								ArrivalTime = journeyNode.SelectSingleNode( "./td[@class='arr']" ).InnerText.Substring( 0, 5 ),
								DepartureTime = journeyNode.SelectSingleNode( "./td[@class='dep']" ).InnerText.Substring( 0, 5 ),
								Duration = ReplaceWhitespace( journeyNode.SelectSingleNode( "./td[@class='dur']" ).InnerText ),
								Status = ReplaceWhitespace( journeyNode.SelectSingleNode( "./td[@class='status']" ).InnerText )
							};

							// Set the full departure timestamp from the responseDate and the departure time
							newJourney.DepartureDateTime = responseDate + TimeSpan.ParseExact( newJourney.DepartureTime, "h\\:mm", CultureInfo.InvariantCulture );

							journeys.Journeys.Add( newJourney );
						}
						else if ( journeyNode.SelectSingleNode( "./td[@class='origin']" ) != null )
						{
							// Only proceed with processing changes if a journey has been found
							if ( journeys.Journeys.Count > 0 )
							{
								// Get the change data ignoring any empty cells and whitespace
								HtmlNodeCollection cells = journeyNode.SelectNodes( "./td" );

								if ( cells != null )
								{
									// Somewhere to save the cell data
									List<string> cellContents = new List<string>();

									foreach ( HtmlNode cell in cells )
									{
										string innerText = ReplaceWhitespace( cell.InnerText );
										if ( innerText.Length > 0 )
										{
											cellContents.Add( innerText );
										}
									}

									// There should be 4 entries for a journey leg
									if ( cellContents.Count == 4 )
									{
										JourneyLeg leg = new JourneyLeg {
											DepartureTime = cellContents[ 0 ], From = cellContents[ 1 ],
											ArrivalTime = cellContents[ 2 ], To = cellContents[ 3 ]
										};

										journeys.Journeys[ journeys.Journeys.Count - 1 ].Legs.Add( leg );
									}
								}
							}
						}
						else
						{
							// If this is either the report header or a date change within the report.
							// Extraxct the date
							string headerDate = ( journeyNode.Name == "h3" ) ?
								ReplaceWhitespace( journeyNode.InnerText ).Replace( "+", " " ).Substring( 17, 10 ) :
								ReplaceWhitespace( journeyNode.InnerText );

							// Date is now of the format DDD nn MMM where nn could be one or two numeric digits
							try
							{
								responseDate = DateTime.ParseExact( headerDate, new[] { "ddd dd MMM", "ddd d MMM" }, CultureInfo.InvariantCulture, DateTimeStyles.None );
							}
							catch ( FormatException )
							{
							}
						}
					}

					// Save the journeys so they can be accessed from outside the class
					Journeys = journeys.Journeys;

					JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = true } );
				}
				else
				{
					JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = false } );
				}
			}
			catch ( HttpRequestException )
			{
				JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = false, NetworkProblem = true } );
			}
			catch ( TaskCanceledException )
			{
				// This can be caused either from a user request or a network problem
				if ( cancelToken.IsCancellationRequested == true )
				{
					JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = false, RequestCancelled = true } );
				}
				else
				{
					JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = false, NetworkProblem = true } );
				}
			}
			catch ( OperationCanceledException )
			{
				JourneysAvailableEvent?.Invoke( this, new JourneysAvailableArgs { JourneysAvailable = false, NetworkProblem = true } );
			}
		}

		/// <summary>
		/// Replace occurrences of common whitespace with empty strings
		/// </summary>
		/// <param name="targetString"></param>
		/// <returns></returns>
		private string ReplaceWhitespace( string targetString )
		{
			return targetString.Replace( "\n", "" ).Replace( "\t", "" ).Replace( "&nbsp;", " " ).Replace( "  ", "" );
		}

		/// <summary>
		/// Eventhandler used to notify when the journeys are available
		/// </summary>
		public event EventHandler<JourneysAvailableArgs> JourneysAvailableEvent;

		/// <summary>
		/// The journeys retrived from the last request
		/// </summary>
		public List<TrainJourney> Journeys { get; private set; }

		/// <summary>
		/// The arguments for the JourneysAvailableEvent.
		/// Were arguements returned or was there a problem
		/// </summary>
		public class JourneysAvailableArgs: EventArgs
		{
			public bool JourneysAvailable { get; set; } = true;
			public bool NetworkProblem { get; set; } = false;
			public bool RequestCancelled { get; set; } = false;
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

		/// <summary>
		/// Dictionary used to hold the request parameters
		/// </summary>
		private Dictionary< string, string > requestParameters = null;
	}
}