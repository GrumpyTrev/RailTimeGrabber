using Android.Content;
using Android.Util;
using Android.Widget;

namespace RailTimeGrabber
{
	/// <summary>
	/// Override the standard Spinner class in order allow the private OnDetachedFromWindow method to be called
	/// </summary>
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