using System;
using System.Collections.Generic;
using System.Threading;

namespace RailTimeGrabber
{
	/// <summary>
	/// The NextDepartureRetrieval class is responsible for retrieving a next journey for a specified trip.
	/// The JourneyRequest class is used to perform the actual web requests required to obtain a set of journeys for 
	/// a specified time.
	/// </summary>
	class NextDepartureRetrieval
	{
		public NextDepartureRetrieval()
		{
			trainJourneyRequest.JourneysAvailableEvent += JourneysAvailable;
		}

		/// <summary>
		/// Get a set of journeys for the specified trip
		/// </summary>
		/// <param name="requiredTrip"></param>
		public void GetJourneys( TrainTrip requiredTrip )
		{
			// Make the request for one minute from now
			MakeRequestAfterSpecifiedTime( DateTime.Now, requiredTrip );
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
		/// The JourneyRequest has completed its web request. Extract the departure time for the first journey and update the saved
		/// departure time if it has changed.
		/// If there was a network problem or the request was cancelled then leave the saved departure time as it is.
		/// If no journeys were found then leave  the saved departure time as it is.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void JourneysAvailable( object sender, JourneyRequest.JourneysAvailableArgs args )
		{
			if ( args.JourneysAvailable == true )
			{
				// Extract the next departure time from the returned list and report any changes
				ReportNextDepartureChanges.ReportTimeChanges( MarkJourneyDateChanges( trainJourneyRequest.Journeys, requestDate ) );
			}
			else if ( args.NetworkProblem == true )
			{
				// If there has been a network problem then mark the stored departure time as suspect
				ReportNextDepartureChanges.ReportSuspectStateChanges( true );
			}
			else if ( args.RequestCancelled == true )
			{
				// If the request has been cancelled don't update the stored value last retrieved
			}
			else
			{
				// No journeys have been retieved then mark the next departure value as suspect
				ReportNextDepartureChanges.ReportSuspectStateChanges( true );
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
		private void MakeRequestAfterSpecifiedTime( DateTime specifiedTime, TrainTrip requiredTrip )
		{
			// Make the request for one minute from specifiedTime
			DateTime requestTime = specifiedTime + TimeSpan.FromMinutes( 1 );

			// Record the day of the request
			requestDate = requestTime.Date;

			// Pass a cancellation token in case this request needs to be cancelled
			tokenSource = new CancellationTokenSource();

			trainJourneyRequest.GetJourneys( requiredTrip.From, requiredTrip.To, requestTime, tokenSource.Token );
		}

		/// <summary>
		/// JourneyRequest instance used to access the web for journey details
		/// </summary>
		private JourneyRequest trainJourneyRequest = new JourneyRequest();

		/// <summary>
		/// The original date for this request
		/// </summary>
		private DateTime requestDate = DateTime.MinValue;

		/// <summary>
		/// CancellationTokenSource used to cancel the current journey request
		/// </summary>
		private CancellationTokenSource tokenSource = null;
	}
}