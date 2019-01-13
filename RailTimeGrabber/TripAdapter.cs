using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace RailTimeGrabber
{
	class TripAdapter: ArrayAdapter< string >
	{
		LayoutInflater inflater = null;
		TripSpinner ownerSpinner = null;

		public TripAdapter( Activity context, int layoutId, List<string> tripStrings, TripSpinner spinner ) : base( context, layoutId, tripStrings )
		{
			inflater = context.LayoutInflater;
			ownerSpinner = spinner;
		}

		public override View GetView( int position, View convertView, ViewGroup parent )
		{
			var view = convertView;

			if ( view == null )
			{
				view = inflater.Inflate( Resource.Layout.spinner_item, null );
			}

			view.FindViewById<TextView>( Resource.Id.TripText ).Text = GetItem( position );

			return view;
		}

		public override View GetDropDownView( int position, View convertView, ViewGroup parent )
		{
			/*			var view = convertView;

						if ( view == null )
						{
							view = inflater.Inflate( Android.Resource.Layout.SimpleSpinnerDropDownItem, null );
						}

						view.FindViewById<TextView>( Android.Resource.Id.Text1 ).Text = GetItem( position );
			*/

			View view = convertView;
			if ( view == null )
			{
				view = base.GetDropDownView( position, convertView, parent );
				view.Tag = position;

				view.LongClick += ( sender, e ) => {
					ownerSpinner.OnDetachedFromWindowPublic();
					Toast.MakeText( this.Context, "Trip long selected: " + GetItem( position ), ToastLength.Short ).Show();
				};
				view.Click += ( sender, e ) => {
					//Toast.MakeText( this.Context, "Trip selected: " + GetItem( position ), ToastLength.Short ).Show();
					ownerSpinner.OnDetachedFromWindowPublic();
					ownerSpinner.SetSelection( ( int )( ( View )sender ).Tag );
				};


			}
			else
			{
				view = base.GetDropDownView( position, convertView, parent );
			}

			return view;
		}

	}
}