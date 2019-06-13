
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using System;
using System.Threading;

namespace RailTimeGrabber
{
	/// <summary>
	/// The UpdateService service is used to periodically retrieve the next journey for the currently selected trip
	/// </summary>
	[Service]
	class UpdateService : Service
	{
		public override IBinder OnBind( Intent intent )
		{
			return null;
		}

		/// <summary>
		/// Called when this service is created (before the command is sent)
		/// </summary>
		public override void OnCreate()
		{
			// Start a timer to perform the update
			updateTimer = new Timer( x => PerformUpdate(), null, TimeSpan.FromSeconds( 60 ), TimeSpan.FromSeconds( 60 ) );

			base.OnCreate();
		}

		/// <summary>
		/// Called when the service is first started.
		/// Get the next departure time
		/// </summary>
		/// <param name="intent"></param>
		/// <param name="flags"></param>
		/// <param name="startId"></param>
		/// <returns></returns>
		[return: GeneratedEnum]
		public override StartCommandResult OnStartCommand( Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId )
		{
			// Make sure a non-cached set of TrainTrips is used
			TrainTrips.Reset();
			trainJourneyRetrieval.GetJourneys( TrainTrips.SelectedTrip );

			// The update is now in progress, so report this
			Application.Context.SendBroadcast( new Intent( AppWidget.UpdateInProgress ) );

			return StartCommandResult.NotSticky;
		}

		/// <summary>
		/// Called when the service is terminated
		/// </summary>
		public override void OnDestroy()
		{
			trainJourneyRetrieval.CancelRequest();

			updateTimer.Dispose();

			Application.Context.SendBroadcast( new Intent( AppWidget.UpdateFinished ) );

			base.OnDestroy();
		}

		/// <summary>
		/// Called periodically to get the next departure time
		/// </summary>
		private void PerformUpdate()
		{
			// Make sure a non-cached set of TrainTrips is used
			TrainTrips.Reset();
			trainJourneyRetrieval.GetJourneys( TrainTrips.SelectedTrip );

			// The update is now in progress, so report this
			Application.Context.SendBroadcast( new Intent( AppWidget.UpdateInProgress ) );
		}

		/// <summary>
		/// Timer used to perform the update
		/// </summary>
		private Timer updateTimer = null;

		/// <summary>
		/// NextDepartureRetrieval instance used to get journeys for teh selected trip
		/// </summary>
		private NextDepartureRetrieval trainJourneyRetrieval = new NextDepartureRetrieval();
	}
}