using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace RailTimeGrabber
{
	/// <summary>
	/// Station specific override of AutoCompleteTextView to always display the dropdown list and to do so when focus is gained
	/// </summary>
	public class StationAutoComplete: AutoCompleteTextView
	{
		public StationAutoComplete( Context context, IAttributeSet attrs ) : base( context, attrs )
		{
		}

		/// <summary>
		/// Always filter the dropdown list
		/// </summary>
		/// <returns></returns>
		public override bool EnoughToFilter()
		{
			return true;
		}

		/// <summary>
		/// Perform filtering when focus gained
		/// </summary>
		/// <param name="gainFocus"></param>
		/// <param name="direction"></param>
		/// <param name="previouslyFocusedRect"></param>
		protected override void OnFocusChanged( bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect )
		{
			base.OnFocusChanged( gainFocus, direction, previouslyFocusedRect );
			if ( ( gainFocus == true ) && ( Adapter != null ) )
			{
				// If there is no text make sure that the dropdown list is still displayed by forcing a text change
				if ( Text.Length == 0 )
				{
					Text = "";
				}
				else
				{
					// If there is some text then re-show the dropdown list.
					// Don't do this if there is no text as the drop dopwn will not have been initialised
					ShowDropDown();
				}
			}
		}
	}
}