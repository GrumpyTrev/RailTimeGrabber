using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Util;
using Android.Widget;
using System;

namespace RailTimeGrabber
{
	[BroadcastReceiver( Label = "@string/widget_name", Name = "RailTimeGrabber.AppWidget" )]
	[IntentFilter( new string[] { "android.appwidget.action.APPWIDGET_UPDATE", TripUpdated, NextDepartureSuspectChange, NextDepartureTimeChange, UpdateInProgress, UpdateFinished } )]
	[MetaData( "android.appwidget.provider", Resource = "@xml/appwidgetprovider" )]
	/// <summary>
	/// The AppWidget class controls the display of the next journey for the currently selected trip within an AppWidget on the home screen.
	/// </summary>
	public class AppWidget: AppWidgetProvider
	{
		/// <summary>
		/// Intent actions for externally generated events
		/// </summary>
		public const string TripUpdated = "RailTimeGrabber.TRIP_UPDATED";
		public const string NextDepartureSuspectChange = "RailTimeGrabber.DEPARTURE_SUSPECT_CHANGED";
		public const string NextDepartureTimeChange = "RailTimeGrabber.DEPARTURE_TIME_CHANGED";
		public const string UpdateInProgress = "RailTimeGrabber.UPDATE_IN_PROGRESS";
		public const string UpdateFinished = "RailTimeGrabber.UPDATE_FINISHED";

		/// <summary>
		/// Called when the first widget is displayed.
		/// </summary>
		/// <param name="context"></param>
		public override void OnEnabled( Context context )
		{
			// Initialise the persistent storage
			PersistentStorage.StorageMechanism = new StorageMechanism( context );
			PersistentStorage.UseCache = false;

			// Initialise the update service state to not running
			PersistentStorage.SetBoolItem( UpdateServiceRunningName, false );

			// Register for when the device wakes up and the keylock is off
			// Cannot register for this in the manifest
			// Also cannot use the provided context, must use the application context??
			context.ApplicationContext.RegisterReceiver( this, new IntentFilter( Intent.ActionUserPresent ) );

			base.OnEnabled( context );
		}

		/// <summary>
		/// Render the visual content of all instances of this widget
		/// </summary>
		/// <param name="context"></param>
		/// <param name="appWidgetManager"></param>
		/// <param name="appWidgetIds"></param>
		public override void OnUpdate( Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds )
		{
			// Render each of the app widgets
			foreach ( int widgetId in appWidgetIds )
			{
				appWidgetManager.UpdateAppWidget( widgetId, RenderWidgetContents( context, widgetId ) );
			}

			base.OnUpdate( context, appWidgetManager, appWidgetIds );
		}

		/// <summary>
		/// This method is called when the BroadcastReceiver has received an Intent
		/// </summary>
		/// <param name="context">The Context in which the receiver is running.</param>
		/// <param name="intent">The Intent being received.</param>
		public override void OnReceive( Context context, Intent intent )
		{
			base.OnReceive( context, intent );

			// Check if the click is from the details text being clicked
			if ( DetailsClick == intent.Action )
			{
				// Open the main QuTi activity
				context.StartActivity( new Intent( context, typeof( MainActivity ) ) );
			}
			// If a new trip has been selected then update all widgets to display it
			else if ( intent.Action == TripUpdated )
			{
				UpdateAllWidgets( context );
			}
			// If this is an update service state change then action it
			else if ( ( intent.Action == StartClick ) || ( intent.Action == StopClick ) )
			{
				// Initialise the persistent storage
				PersistentStorage.StorageMechanism = new StorageMechanism( context );
				PersistentStorage.UseCache = false;

				// Store the state of the update service
				PersistentStorage.SetBoolItem( UpdateServiceRunningName, ( intent.Action == StartClick ) );

				// Start or stop the service
				if ( intent.Action == StartClick )
				{
					context.StartService( new Intent( context, typeof( UpdateService ) ) );
				}
				else
				{
					context.StopService( new Intent( context, typeof( UpdateService ) ) );
				}

				// Display the updated state
				UpdateAllWidgets( context );
			}
			// If this is a departure time change then update all the widgets
			else if ( ( intent.Action == NextDepartureSuspectChange ) || ( intent.Action == NextDepartureTimeChange ) )
			{
				UpdateAllWidgets( context );
			}
			// If the device has just unlocked then update the widget
			else if ( intent.Action == Intent.ActionUserPresent )
			{
				UpdateAllWidgets( context );
			}
			else if ( ( intent.Action == UpdateInProgress ) || ( intent.Action == UpdateFinished ) )
			{
				// Store the state of the update operation
				PersistentStorage.SetBoolItem( UpdateInProgressName, ( intent.Action == UpdateInProgress ) );

				UpdateAllWidgets( context );
			}
		}

		/// <summary>
		/// Update all instances of this widget
		/// Get an array containing the identities of all the displayed widgets and get them updated
		/// </summary>
		/// <param name="context"></param>
		private void UpdateAllWidgets( Context context )
		{
			AppWidgetManager appManager = AppWidgetManager.GetInstance( context );
			int[] appWidgetIds = appManager.GetAppWidgetIds( new ComponentName( context, Java.Lang.Class.FromType( typeof( AppWidget ) ) ) );

			OnUpdate( context, appManager, appWidgetIds );
		}

		/// <summary>
		/// Renders the visual contents of a specific widget contents into a RemoteViews object
		/// </summary>
		/// <param name="widgetContext">Widget context.</param>
		/// <param name="widgetId">Widget identifier.</param>
		private RemoteViews RenderWidgetContents( Context context, int widgetId )
		{
			// Initialise the persistent storage
			PersistentStorage.StorageMechanism = new StorageMechanism( context );
			PersistentStorage.UseCache = false;

			// Create a RemoteView for the widget
			RemoteViews widgetView = new RemoteViews( context.PackageName, Resource.Layout.widget );

			// Extract the current trip details and display them. 
			// The trip details and selected trip can be changed independently by the main app so a new set of train trip details need to be read
			TrainTrips.Reset();
			TrainTrip selectedTrip = TrainTrips.SelectedTrip;

			if ( selectedTrip != null )
			{
				widgetView.SetTextViewText( Resource.Id.widgetTrip, string.Format( "{0}:{1}", selectedTrip.FromCode, selectedTrip.ToCode ) );
			}

			// Extract the next departure time and display it
			DateTime departureTime = NextDeparture.DepartureTime;
			widgetView.SetTextViewText( Resource.Id.widgetDeparture, departureTime.ToString( "HH:mm" ) );

			// Register pending intents for clicking on the displayed fields
			RegisterClicks( context, widgetView );

			// Show the correct image for the running state of the update service
			if ( PersistentStorage.GetBoolItem( UpdateInProgressName, false ) == true )
			{
				// An update is actually in progress, so show the progress indicator and hide
				// the service status flags
				widgetView.SetViewVisibility( Resource.Id.layoutProgressBar, Android.Views.ViewStates.Visible );
				widgetView.SetViewVisibility( Resource.Id.imageStart, Android.Views.ViewStates.Gone );
				widgetView.SetViewVisibility( Resource.Id.imageStop, Android.Views.ViewStates.Gone );
			}
			else
			{
				// Hide the progress indicator and show the servide state
				widgetView.SetViewVisibility( Resource.Id.layoutProgressBar, Android.Views.ViewStates.Gone );

				if ( PersistentStorage.GetBoolItem( UpdateServiceRunningName, false ) == true )
				{
					widgetView.SetViewVisibility( Resource.Id.imageStart, Android.Views.ViewStates.Gone );
					widgetView.SetViewVisibility( Resource.Id.imageStop, Android.Views.ViewStates.Visible );
				}
				else
				{
					widgetView.SetViewVisibility( Resource.Id.imageStart, Android.Views.ViewStates.Visible );
					widgetView.SetViewVisibility( Resource.Id.imageStop, Android.Views.ViewStates.Gone );
				}
			}

			return widgetView;
		}

		/// <summary>
		/// Register pending intents for clicking on the dispalyed fields
		/// </summary>
		/// <param name="context"></param>
		/// <param name="widgetView"></param>
		private void RegisterClicks( Context context, RemoteViews widgetView )
		{
			widgetView.SetOnClickPendingIntent( Resource.Id.widgetTrip, GetPendingSelfIntent( context, DetailsClick ) );
			widgetView.SetOnClickPendingIntent( Resource.Id.widgetDeparture, GetPendingSelfIntent( context, DetailsClick ) );
			widgetView.SetOnClickPendingIntent( Resource.Id.imageStart, GetPendingSelfIntent( context, StartClick ) );
			widgetView.SetOnClickPendingIntent( Resource.Id.imageStop, GetPendingSelfIntent( context, StopClick ) );
		}

		/// <summary>
		/// Get a PendingIntent back to this widget
		/// </summary>
		/// <param name="context"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		private PendingIntent GetPendingSelfIntent( Context context, string action )
		{
			return PendingIntent.GetBroadcast( context, 0, new Intent( context, typeof( AppWidget ) ).SetAction( action ), 0 );
		}

		/// <summary>
		/// Unique name used to identify when the journey details are clicked
		/// </summary>
		private const string DetailsClick = "RailTimeGrabber.CLICK_ACTION";

		/// <summary>
		/// Unique name used to identify when the start update button is clicked
		/// </summary>
		private const string StartClick = "RailTimeGrabber.START_CLICK_ACTION";

		/// <summary>
		/// Unique name used to identify when the start update button is clicked
		/// </summary>
		private const string StopClick = "RailTimeGrabber.STOP_CLICK_ACTION";

		/// <summary>
		/// Persistent storage names
		/// </summary>
		private const string UpdateServiceRunningName = "UpdateServiceRunning";
		private const string UpdateInProgressName = "UpdateInProgressRunning";
	}
}