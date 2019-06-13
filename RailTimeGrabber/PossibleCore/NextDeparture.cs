using System;

namespace RailTimeGrabber
{
	/// <summary>
	/// The NextDeparture class maintains in persistent storage the departure time of the next journey for the current trip
	/// </summary>
	class NextDeparture
	{
		/// <summary>
		/// The DepartureTime property
		/// </summary>
		public static DateTime DepartureTime
		{
			get
			{
				DateTime departureTime = DateTime.MinValue;

				// Always get this from storage in case it has been changed by another activity
				string departureTimeString = PersistentStorage.GetStringItem( NextDepartureTimeName, "" );
				if ( departureTimeString.Length > 0 )
				{
					departureTime = DateTime.Parse( departureTimeString );
				}

				return departureTime;
			}

			set
			{

				PersistentStorage.SetStringItem( NextDepartureTimeName, value.ToString() );
			}
		}

		/// <summary>
		/// The departure time suspect flag
		/// </summary>
		public static bool TimeSuspect
		{
			get
			{
				return PersistentStorage.GetBoolItem( NextDepartureTimeSuspectName, true );
			}

			set
			{
				PersistentStorage.SetBoolItem( NextDepartureTimeSuspectName, value );
			}
		}

		/// <summary>
		/// Determine if a new time/suspect pairing differs from those currently stored
		/// </summary>
		/// <param name="newTime"></param>
		/// <param name="newSuspect"></param>
		/// <returns></returns>
		public static bool Differs( DateTime newTime, bool newSuspect )
		{
			return ( ( DepartureTime != newTime ) || ( TimeSuspect != newSuspect ) );
		}

		/// <summary>
		/// Persistent storage names
		/// </summary>
		private const string NextDepartureTimeName = "NextDepartureTime";
		private const string NextDepartureTimeSuspectName = "NextDepartureTimeSuspect";
	}
}