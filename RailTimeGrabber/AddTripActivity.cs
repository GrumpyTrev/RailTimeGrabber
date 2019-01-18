
using Android.App;
using Android.OS;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Widget;
using System;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Content;

namespace RailTimeGrabber
{
	[Activity( Label = "Add Trip" )]
	public class AddTripActivity: AppCompatActivity
	{
		protected override async void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			// Set our view from the "main" layout resource
			SetContentView( Resource.Layout.activity_trip );

			// Initialise the action bar 
			SetSupportActionBar( FindViewById<Toolbar>( Resource.Id.toolbar ) );

			// Create a StorageMechanism instance
			PersistentStorage.StorageMechanism = new StorageMechanism( this );

			// Link a custom adapter to the 'from' station selecter
			fromView = FindViewById<StationAutoComplete>( Resource.Id.autoCompleteTextViewFrom );
			fromView.Adapter = new StationAdapter( this, Android.Resource.Layout.SimpleSpinnerDropDownItem );
			fromView.InputType = Android.Text.InputTypes.TextFlagNoSuggestions | Android.Text.InputTypes.TextVariationVisiblePassword;

			// Link a custom adapter to the 'to' station selecter
			toView = FindViewById<StationAutoComplete>( Resource.Id.autoCompleteTextViewTo );
			toView.Adapter = new StationAdapter( this, Android.Resource.Layout.SimpleSpinnerDropDownItem );
			toView.InputType = Android.Text.InputTypes.TextFlagNoSuggestions | Android.Text.InputTypes.TextVariationVisiblePassword;

			// Validate the station names when the text is changed
			fromView.TextChanged += ValidateStationFields;
			toView.TextChanged += ValidateStationFields;

			// Disable the add button by default
			addButton = FindViewById<Button>( Resource.Id.buttonAdd );
			addButton.Enabled = false;

			// Validate the station names again when the add button is pressed
			addButton.Click += AddButtonClicked;

			// Read in the fixed (and static) list of stations (aynchronously)
			StationStorage.ReadStations( Assets );
		}

		/// <summary>
		/// Validate that the from and to station fileds contain valid stations names, are not the same, and
		/// do not represent a valid trip
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddButtonClicked( object sender, System.EventArgs e )
		{
			bool validTrip = true;
			string errorMessage = "";

			// Check for valid station names
			if ( Array.IndexOf( StationStorage.StationNames, fromView.Text ) < 0 )
			{
				errorMessage += string.Format( "'{0}' is not a valid station name\n", fromView.Text );
				validTrip = false;
			}

			if ( Array.IndexOf( StationStorage.StationNames, toView.Text ) < 0 )
			{
				errorMessage += string.Format( "'{0}' is not a valid station name\n", toView.Text );
				validTrip = false;
			}

			if ( validTrip == true )
			{
				// Make sure they are not the same
				if ( fromView.Text == toView.Text )
				{
					errorMessage += "Station anmes cannot be the same\n";
					validTrip = false;
				}
			}

			if ( validTrip == true )
			{
				// Check for a duplicate trip
				if ( TrainTrips.IsDuplicateTrip( fromView.Text, toView.Text ) == true )
				{
					errorMessage += string.Format( "A trip from {0} to {1} already exists\n", fromView.Text, toView.Text );
					validTrip = false;
				}
			}

			if ( validTrip == false )
			{
				// Display the reason why this trip cannot be added
				new AlertDialog.Builder( this )
					.SetTitle( "Cannot add trip" )
					.SetMessage( errorMessage )
					.SetPositiveButton( "OK", ( EventHandler< DialogClickEventArgs > )null )
					.Create()
					.Show();
			}
			else
			{
				// Add the trip and close this activity
				TrainTrips.AddTrip( new TrainTrip { From = fromView.Text, To = toView.Text } );
				SetResult( Result.Ok );
				Finish();
			}
		}

		/// <summary>
		/// Enable or disable the add button according to the contents of the station fields
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ValidateStationFields( object sender, Android.Text.TextChangedEventArgs e )
		{
			// If either station field is empty then disable the Add button
			addButton.Enabled = ( ( fromView.Text.Length > 0 ) && ( toView.Text.Length > 0 ) );
		}

		/// <summary>
		/// The from and to station fields
		/// </summary>
		private StationAutoComplete fromView = null;
		private StationAutoComplete toView = null;

		/// <summary>
		/// The add trip button
		/// </summary>
		private Button addButton = null;

	}



}