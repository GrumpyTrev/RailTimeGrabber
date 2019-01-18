using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace RailTimeGrabber
{
	/// <summary>
	/// A custom adapter for the dropdown list of train trips
	/// </summary>
	class TripAdapter: ArrayAdapter< string >
	{
		/// <summary>
		/// Constructor. Save the view inflator and parent spinner control for later
		/// </summary>
		/// <param name="context"></param>
		/// <param name="layoutId"></param>
		/// <param name="tripStrings"></param>
		/// <param name="spinner"></param>
		public TripAdapter( Activity context, int layoutId, List<string> tripStrings, TripSpinner spinner ) : base( context, layoutId, tripStrings )
		{
			inflater = context.LayoutInflater;
			ownerSpinner = spinner;
		}

		/// <summary>
		/// Set the text for the specified item
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
				view = inflater.Inflate( Resource.Layout.spinner_item, null );
			}

			view.FindViewById<TextView>( Resource.Id.TripText ).Text = GetItem( position );

			return view;
		}

		/// <summary>
		/// Override to allow the click and longclick events to be grabbed.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public override View GetDropDownView( int position, View convertView, ViewGroup parent )
		{
			View view = convertView;
			if ( view == null )
			{
				// If this is a new view then let the base class create it and then link into its click events
				view = base.GetDropDownView( position, convertView, parent );
				view.Tag = position;

				// A longclick is assumed to be a request to delete the item.
				// Hide teh spinner and send the event to its handler
				view.LongClick += ( sender, e ) => {
					ownerSpinner.OnDetachedFromWindowPublic();

					EventHandler<LongClickEventArgs> handler = LongClickEvent;
					if ( handler != null )
					{
						handler( this, new LongClickEventArgs { TripPosition = ( int )view.Tag } );
					}
				};

				// When an item is clicked hide the dropdown items and select the item (via its view tag)
				view.Click += ( sender, e ) => {
					ownerSpinner.OnDetachedFromWindowPublic();
					ownerSpinner.SetSelection( ( int )( ( View )sender ).Tag );
				};
			}
			else
			{
				view = base.GetDropDownView( position, convertView, parent );
				// make sure that this view is associated with the item
				view.Tag = position;
			}

			return view;
		}

		/// <summary>
		/// Eventhandler used to link tinto the long click
		/// </summary>
		public event EventHandler<LongClickEventArgs> LongClickEvent;

		private LayoutInflater inflater = null;
		private TripSpinner ownerSpinner = null;

		/// <summary>
		/// The long click events - just the position
		/// </summary>
		public class LongClickEventArgs: EventArgs
		{
			public int TripPosition { get; set; }
		}
	}
}