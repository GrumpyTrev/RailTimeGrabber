using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

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

		public TrainJourneyWrapper( Activity context, TrainJourney[] items )
		{
			this.context = context;
			Items = items;
		}
		
		public override long GetItemId( int position )
		{
			return position;
		}

		public override int Count
		{
			get
			{
				return Items.Length;
			}
		}

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

			if ( view == null )
			{
				view = context.LayoutInflater.Inflate( Resource.Layout.list_item, null );
			}

			TrainJourney itemToDisplay = Items[ position ];

			view.FindViewById<TextView>( Resource.Id.Departure ).Text = itemToDisplay.DepartureTime;
			view.FindViewById<TextView>( Resource.Id.Arrival ).Text = itemToDisplay.ArrivalTime;
			view.FindViewById<TextView>( Resource.Id.Duration ).Text = itemToDisplay.Duration;
			view.FindViewById<TextView>( Resource.Id.Status ).Text = itemToDisplay.Status;

			return view;
		}

		/// <summary>
		/// The context used to get the layout
		/// </summary>
		private Activity context = null;
	}
}