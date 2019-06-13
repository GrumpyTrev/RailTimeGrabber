using System;
using System.Collections.Generic;
using System.Threading;

namespace RailTimeGrabber
{
	/// <summary>
	/// The JourneyRetrieval class is responsible for retrieving a set of journeys for a specified trip.
	/// Once an initial set of journeys have been obtained, the journeys can be refreshed and more journeys obtained
	/// upon request.
	/// The JourneyRequest class is used to perform the actual web requests required to obtain a set of journeys for 
	/// a specified time.
	/// </summary>
	class JourneyRetrieval
	{
		public JourneyRetrieval()
		{
			trainJourneyRequest.JourneysAvailableEvent += JourneysAvailable;
		}

		/// <summary>
		/// Get a set of journeys for the specified trip
		/// </summary>
		/// <param name="requiredTrip"></param>
		public void GetJourneys( TrainTrip requiredTrip )
		{
			// Save the trip
			trip = requiredTrip;

			// As this is a new request clear the old results
			ClearJourneys();

			// Make the request for one minute from now
			currentRequest = RequestType.NewRequest;
			MakeRequestAfterSpecifiedTime( DateTime.Now );
		}

		/// <summary>
		/// Update the journeys for the current trip. Don't clear the results first in case there is a network problem
		/// </summary>
		public void UpdateJourneys()
		{
			// Depending on the number of journeys previously obtained this request may involve multiple requests.
			// Keep track of the number of entries already obtained and the target
			updateCount = 0;
			updateTarget = retrievedJourneys.Journeys.Count;

			// Make the request for one minute from now
			currentRequest = RequestType.Update;
			MakeRequestAfterSpecifiedTime( DateTime.Now );
		}

		/// <summary>
		/// Get the next set of journeys for the current trip.
		/// Use the departure time of the last journey already obtained as the basis for new request.
		/// If this time is in the past then treat this request as a standard new request
		/// </summary>
		public void MoreJourneys()
		{
			if ( retrievedJourneys.Journeys.Count > 0 )
			{
				DateTime lastDepartureTime = retrievedJourneys.Journeys[ retrievedJourneys.Journeys.Count - 1 ].DepartureDateTime; 

				if ( lastDepartureTime < DateTime.Now )
				{
					// The journeys already obtained are too old.
					// Treat this as a new request
					GetJourneys( trip );
				}
				else
				{
					// Make the request for one minute from the last time
					currentRequest = RequestType.FetchMore;
					MakeRequestAfterSpecifiedTime( lastDepartureTime );
				}
			}
			else
			{
				// Called incorrectly, no existing journeys. Treat as a new request
				GetJourneys( trip );
			}
		}

		/// <summary>
		/// Clear the currently held journeys
		/// </summary>
		public void ClearJourneys()
		{
			retrievedJourneys.Journeys.Clear();

			// Report back
			JourneyResponse?.JourneysCleared();
		}

		/// <summary>
		/// Cancel the current request if there is one
		/// </summary>
		public void CancelRequest()
		{
			if ( tokenSource != null )
			{
				tokenSource.Cancel();
			}
		}

		/// <summary>
		/// The interface used to report the results of a request
		/// </summary>
		public IJourneyResponse JourneyResponse { get; set; } = null;

		/// <summary>
		/// The JourneyRequest has completed its web request.
		/// If there was a network problem or the request was cancelled then leave the current set of journeys as they are.
		/// If some journeys were found and this was a new request or an update then replace the current request with the new results.
		/// If this was a request for more journeys then add the returned journeys to the existing set.
		/// If no journeys were found then clear the existing results.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void JourneysAvailable( object sender, JourneyRequest.JourneysAvailableArgs args )
		{
			if ( args.JourneysAvailable == true )
			{
				// Some new journeys have been received. If this is a new request then any existing entries can now be cleared.
				// If this is an update request, and it is the first reply received for the update then also clear the existing entries
				if ( ( currentRequest == RequestType.NewRequest ) || ( ( currentRequest == RequestType.Update ) && ( updateCount == 0 ) ) )
				{
					retrievedJourneys.Journeys.Clear();
				}

				// Add the new entries to the existing journeys
				// First of all check that the first entry of the new entries is not the same as the last entry of the old entries
				if ( ( ( retrievedJourneys.Journeys.Count > 0 ) && ( trainJourneyRequest.Journeys.Count > 0 ) ) && 
					( retrievedJourneys.Journeys[ retrievedJourneys.Journeys.Count - 1 ].DepartureDateTime == trainJourneyRequest.Journeys[ 0 ].DepartureDateTime ) )
				{
					// Remove the first entry
					trainJourneyRequest.Journeys.RemoveAt( 0 );
				}

				retrievedJourneys.Journeys.AddRange( MarkJourneyDateChanges( trainJourneyRequest.Journeys, requestDate ) );

				// If this is an update request check if sufficient journeys have been obtained
				if ( currentRequest == RequestType.Update )
				{
					// Report the results back just to let the user know that some results have been obtained
					JourneyResponse?.JourneysAvailable( retrievedJourneys );

					updateCount = retrievedJourneys.Journeys.Count;
					if ( updateCount >= updateTarget )
					{
						// Request finished
						JourneyResponse?.JourneyRequestComplete( false, false );
						currentRequest = RequestType.Idle;
					}
					else
					{
						// Make the request for one minute from the last time
						MakeRequestAfterSpecifiedTime( retrievedJourneys.Journeys[ retrievedJourneys.Journeys.Count - 1 ].DepartureDateTime );
					}
				}
				else
				{
					// Report back
					JourneyResponse?.JourneysAvailable( retrievedJourneys );

					// Extract the next departure time from the returned list and report any changes
					ReportNextDepartureChanges.ReportTimeChanges( retrievedJourneys.Journeys );

					// Request finished
					JourneyResponse?.JourneyRequestComplete( false, false );
					currentRequest = RequestType.Idle;
				}
			}
			else if ( args.NetworkProblem == true )
			{
				// Update the next departure time status
				ReportNextDepartureChanges.ReportSuspectStateChanges( true );
				
				JourneyResponse?.JourneyRequestComplete( true, false );
				currentRequest = RequestType.Idle;
			}
			else if ( args.RequestCancelled == true )
			{
				JourneyResponse?.JourneyRequestComplete( false, false );
				currentRequest = RequestType.Idle;
			}
			else
			{
				ClearJourneys();

				// Update the next departure time status
				ReportNextDepartureChanges.ReportSuspectStateChanges( true );

				JourneyResponse?.JourneyRequestComplete( false, true );
				currentRequest = RequestType.Idle;
			}
		}

		/// <summary>
		/// Mark the places in the retrived journeys where there is a date change from the original request date 
		/// </summary>
		/// <param name="journeys"></param>
		private List<TrainJourney> MarkJourneyDateChanges( List<TrainJourney> journeys, DateTime baseDate )
		{
			DateTime runningDate = baseDate;

			foreach ( TrainJourney journey in journeys )
			{
				if ( journey.DepartureDateTime.Date > runningDate )
				{
					journey.DateChange = true;
					runningDate = journey.DepartureDateTime.Date;
				}
			}

			return journeys;
		}

		/// <summary>
		/// Make a request for journeys for the current trip one minute after the specified time
		/// </summary>
		private void MakeRequestAfterSpecifiedTime( DateTime specifiedTime )
		{
			// Make the request for one minute from specifiedTime
			DateTime requestTime = specifiedTime + TimeSpan.FromMinutes( 1 );

			// Record the day of the request
			requestDate = requestTime.Date;

			// Pass a cancellation token in case this request needs to be cancelled
			tokenSource = new CancellationTokenSource();

			trainJourneyRequest.GetJourneys( trip.From, trip.To, requestTime, tokenSource.Token );
		}

		/// <summary>
		/// The possible type of request currently being processed
		/// </summary>
		private enum RequestType { NewRequest, Update, FetchMore, Idle };

		/// <summary>
		/// THe request currently being processed
		/// </summary>
		private RequestType currentRequest = RequestType.Idle;

		/// <summary>
		/// JourneyRequest instance used to access the web for journey details
		/// </summary>
		private JourneyRequest trainJourneyRequest = new JourneyRequest();

		/// <summary>
		/// The journeys obtained for the current trip
		/// </summary>
		private TrainJourneys retrievedJourneys = new TrainJourneys();

		/// <summary>
		/// The trip for which the journeys have been obtained
		/// </summary>
		private TrainTrip trip = null;

		/// <summary>
		/// The original date for this request
		/// </summary>
		private DateTime requestDate = DateTime.MinValue;

		/// <summary>
		/// The number of entries obtained in the current update request
		/// </summary>
		private int updateCount =  0;

		/// <summary>
		/// The target number of entries required for an update
		/// </summary>
		private int updateTarget = 0;

		/// <summary>
		/// CancellationTokenSource used to cancel the current journey request
		/// </summary>
		private CancellationTokenSource tokenSource = null;
	}
}