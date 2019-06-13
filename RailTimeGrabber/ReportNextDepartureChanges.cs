using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;

namespace RailTimeGrabber
{
	/// <summary>
	/// Report any next departure time or status changes in response to the last set of received results
	/// </summary>
	class ReportNextDepartureChanges
	{
		/// <summary>
		/// Extract the next departure time from the results and report it if the time has changed
		/// </summary>
		/// <param name="journeys"></param>
		public static void ReportTimeChanges( List<TrainJourney> journeys )
		{
			DateTime newDepartureTime = NextDepartureExtractor.ExtractDepartureTime( journeys );

			// If no departure time was found then make the current time as suspect
			if ( newDepartureTime == DateTime.MinValue )
			{
				if ( NextDeparture.TimeSuspect == false )
				{
					// Update the suspect flag but leave the departure time as it was
					NextDeparture.TimeSuspect = true;

					Application.Context.SendBroadcast( new Intent( AppWidget.NextDepartureSuspectChange ) );
				}
			}
			else
			{
				// If either the departure time has changed, or the time was suspect (and is not now) then report the change
				if ( NextDeparture.Differs( newDepartureTime, false ) == true )
				{
					// Update both time and suspect flags
					NextDeparture.TimeSuspect = false;
					NextDeparture.DepartureTime = newDepartureTime;

					Application.Context.SendBroadcast( new Intent( AppWidget.NextDepartureTimeChange ) );
				}
			}

			// The update is no longer in progress, so report this
			Application.Context.SendBroadcast( new Intent( AppWidget.UpdateFinished ) );
		}

		/// <summary>
		/// Store this change and report it
		/// </summary>
		/// <param name="suspect"></param>
		public static void ReportSuspectStateChanges( bool suspect )
		{
			// Check it really has changed
			if ( NextDeparture.TimeSuspect != suspect )
			{
				NextDeparture.TimeSuspect = suspect;

				Application.Context.SendBroadcast( new Intent( AppWidget.NextDepartureSuspectChange ) );
			}

			// The update is no longer in progress, so report this
			Application.Context.SendBroadcast( new Intent( AppWidget.UpdateFinished ) );
		}
	}
}