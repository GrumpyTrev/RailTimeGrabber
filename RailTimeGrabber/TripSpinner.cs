using Android.Content;
using Android.Util;
using Android.Widget;
using System;

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

		public override bool PerformClick()
		{
			ClickedEvent?.Invoke();

			return base.PerformClick();
		}

		/// <summary>
		/// Call the private base method
		/// </summary>
		public void OnDetachedFromWindowPublic()
		{
			base.OnDetachedFromWindow();
		}

		/// <summary>
		/// Public event to call when spinner has been clicked
		/// </summary>
		public event Action ClickedEvent;
	}
}