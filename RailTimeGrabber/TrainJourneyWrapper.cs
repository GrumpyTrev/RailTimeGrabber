using Android.App;
using Android.Views;
using Android.Widget;
using System;

namespace RailTimeGrabber
{
	/// <summary>
	/// Adapter used to show a list of journeys
	/// </summary>
	class TrainJourneyWrapper : BaseAdapter<TrainJourney>
	{
		/// <summary>
		/// The list of journeys
		/// </summary>
		public TrainJourney[] Items { get; set; }

		/// <summary>
		/// TrainJourneyWrapper constructor.
		/// </summary>
		/// <param name="context">The context used to resolve layouts</param>
		/// <param name="items"></param>
		public TrainJourneyWrapper( Activity context, TrainJourney[] items )
		{
			this.context = context;
			Items = items;
		}
		
		/// <summary>
		/// Return the id of the item at the specified position.
		/// By default this is the position
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public override long GetItemId( int position )
		{
			return position;
		}

		/// <summary>
		/// Return the number of items to display.
		/// If there are items to display, then alllow for an extra item to display the 'More Journeys' button.
		/// </summary>
		public override int Count
		{
			get
			{
				return ( Items.Length > 0 ) ? Items.Length + 1 : 0;
			}
		}

		/// <summary>
		/// Return the TrainJourney at the specified position
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public override TrainJourney this[ int position ]
		{
			get
			{
				return Items[ position ];
			}
		}
		
		/// <summary>
		/// Display the journey details
		/// </summary>
		/// <param name="position"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public override View GetView( int position, View convertView, ViewGroup parent )
		{
			View view = convertView;

			// If no view to reuse then create a new view
			if ( view == null )
			{
				view = context.LayoutInflater.Inflate( Resource.Layout.list_item, null );
			}

			// If this item is beyond the end of then show the More Journeys layout
			if ( position == Items.Length )
			{
				view.FindViewById<ViewGroup>( Resource.Id.MoreJourneysLayout ).Visibility = ViewStates.Visible;
				view.FindViewById<ViewGroup>( Resource.Id.JourneyLayout ).Visibility = ViewStates.Invisible;

				// Access the button view and check whether or not its click event is already linked in
				Button moreButton = view.FindViewById<Button>( Resource.Id.MoreButton );
				if ( moreButton.Tag == null )
				{
					moreButton.Click += MoreJourneysClick;

					// Set the Tag to non-null to make sure it is only linked once
					moreButton.Tag = moreButton;
				}
			}
			else
			{
				// Show the item layout
				view.FindViewById<ViewGroup>( Resource.Id.MoreJourneysLayout ).Visibility = ViewStates.Invisible;
				view.FindViewById<ViewGroup>( Resource.Id.JourneyLayout ).Visibility = ViewStates.Visible;

				TrainJourney itemToDisplay = Items[ position ];

				// If this entry is the start of a new day then show the date
				TextView dateChange = view.FindViewById<TextView>( Resource.Id.DateChange );
				if ( itemToDisplay.DateChange == true )
				{
					dateChange.Visibility = ViewStates.Visible;
					dateChange.Text = itemToDisplay.DepartureDateTime.ToString( "ddd dd MMM" );
				}
				else
				{
					dateChange.Visibility = ViewStates.Gone;
				}

				view.FindViewById<TextView>( Resource.Id.Departure ).Text = itemToDisplay.DepartureTime;
				view.FindViewById<TextView>( Resource.Id.Arrival ).Text = itemToDisplay.ArrivalTime;
				view.FindViewById<TextView>( Resource.Id.Duration ).Text = itemToDisplay.Duration;
				view.FindViewById<TextView>( Resource.Id.Status ).Text = itemToDisplay.Status;
			}

			return view;
		}

		/// <summary>
		/// Public event to call when the More Journeys button has been clicked
		/// </summary>
		public event Action MoreJourneysEvent;

		/// <summary>
		/// Called when the use has clicked on the More Journeys button.
		/// Pass on to the supplied delegate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void MoreJourneysClick( object sender, System.EventArgs args )
		{
			MoreJourneysEvent?.Invoke();
		}

		/// <summary>
		/// The context used to get the layout
		/// </summary>
		private Activity context = null;
	}
}