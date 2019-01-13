
using Android.App;
using Android.OS;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Widget;
using Java.Lang;
using System.Collections.Generic;
using Android.Content;
using Android.Util;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using System.IO;
using System.Xml.Serialization;

namespace RailTimeGrabber
{
	[Activity( Label = "Add Trip" )]
	public class AddTripActivity: AppCompatActivity
	{
		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			// Set our view from the "main" layout resource
			SetContentView( Resource.Layout.activity_trip );

			// Initialise the action bar 
			SetSupportActionBar( FindViewById<Toolbar>( Resource.Id.toolbar ) );

			// Create a StorageMechanism instance
			PersistentStorage.StorageMechanism = new StorageMechanism( this );

			StationAdapter fromAdapter = new StationAdapter( this, Android.Resource.Layout.SimpleSpinnerDropDownItem );

			StationAutoComplete fromView = FindViewById<StationAutoComplete>( Resource.Id.autoCompleteTextViewFrom );
			fromView.Adapter = fromAdapter;

			StationStorage.ReadStations( Assets );
		}
	}

	public class StationAutoComplete: AutoCompleteTextView
	{
		public StationAutoComplete( Context context, IAttributeSet attrs ) : base( context, attrs )
		{
		}

		public override bool EnoughToFilter()
		{
			return true;
		}

		protected override void OnFocusChanged( bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect )
		{
			base.OnFocusChanged( gainFocus, direction, previouslyFocusedRect );
			if ( ( gainFocus == true ) && ( Adapter != null ) )
			{
				PerformFiltering( Text, 0 );
			}
		}

	}

	public class StationAdapter: ArrayAdapter<string>
	{
		public StationAdapter( Activity context, int textId ) : base( context, textId )
		{
		}

		private StationFilter filter;

		public override Filter Filter
		{
			get
			{
				if ( filter == null )
				{
					filter = new StationFilter();
					filter.adapter = this;
				}

				return filter;
			}
		}
	}

	public class StationFilter: Filter
	{
		public StationAdapter adapter { get; set; }

		protected override FilterResults PerformFiltering( ICharSequence constraint )
		{
			FilterResults results = new FilterResults();
			string searchString = "";

			if ( constraint != null )
			{
				searchString = constraint.ToString().ToLower();
			}

			if ( searchString.Length == 0 )
			{
				results.Values = new string[] { "Chippenham", "Bath Spa", "Bristol" };
				results.Count = 3;
			}
			else
			{
				List<string> filteredStations = new List<string>();

				foreach ( StationsStation station in StationStorage.Stations.Items )
				{
					if ( station.Name.ToLower().StartsWith( searchString ) == true )
					{
						filteredStations.Add( station.Name );
					}
				}

				results.Values = filteredStations.ToArray();
				results.Count = filteredStations.Count;
			}

			return results;
		}

		protected override void PublishResults( ICharSequence constraint, FilterResults results )
		{
			adapter.Clear();

			foreach ( string station in ( string[] )results.Values )
			{
				adapter.Add( station );
			}
		}
	}

}