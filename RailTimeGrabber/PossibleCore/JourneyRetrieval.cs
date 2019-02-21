using System;

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
			trainJourneyRequest.GetJourneys( trip.From, trip.To, DateTime.Now + TimeSpan.FromMinutes( 1 ) );
		}

		/// <summary>
		/// Update the journeys for the current trip. Don't clear the results first in case there is a network problem
		/// </summary>
		public void UpdateJourneys()
		{
			// Make the request for one minute from now
			currentRequest = RequestType.Update;
			trainJourneyRequest.GetJourneys( trip.From, trip.To, DateTime.Now + TimeSpan.FromMinutes( 1 ) );
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
		/// The interface used to report the reults of a request
		/// </summary>
		public IJourneyResponse JourneyResponse { get; set; } = null;

		/// <summary>
		/// The JourneyRequest has completed its web request.
		/// If there was a network problem then leave the current set of journeys as they are.
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
				if ( ( currentRequest == RequestType.NewRequest ) || ( currentRequest == RequestType.Update ) )
				{
					// Clear the existing journeys and replace with the new ones
					retrievedJourneys.Journeys.Clear();
					retrievedJourneys.Journeys.AddRange( trainJourneyRequest.Journeys );

					// Report back
					JourneyResponse?.JourneysAvailable( retrievedJourneys );

					// Request finished
					JourneyResponse?.JourneyRequestComplete( false, false );
				}
			}
			else if ( args.NetworkProblem == true )
			{
				JourneyResponse?.JourneyRequestComplete( true, false );
			}
			else
			{
				ClearJourneys();
				JourneyResponse?.JourneyRequestComplete( false, true );
			}

			currentRequest = RequestType.Idle;
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
	}
}