using Android.Content;
using Android.Util;
using Android.Widget;

namespace RailTimeGrabber
{
	class TripSpinner : Spinner
	{
		public TripSpinner( Context context, IAttributeSet attrs ) : base ( context, attrs )
		{
		}

		public void OnDetachedFromWindowPublic()
		{
			base.OnDetachedFromWindow();
		}
	}
}