
using Android.App;
using Android.Widget;

namespace RailTimeGrabber
{
	/// <summary>
	/// Station specific override of ArrayAdapter in order to provide a filter for the dropdown list
	/// </summary>
	public class StationAdapter: ArrayAdapter<string>
	{
		public StationAdapter( Activity context, int textId ) : base( context, textId )
		{
		}

		/// <summary>
		/// Override the Filter property in order to supply a StationFilter instance
		/// </summary>
		public override Filter Filter
		{
			get
			{
				if ( filter == null )
				{
					filter = new StationFilter();

					// Link this adapter in to the filter so that the filtered list can be applied
					filter.Adapter = this;
				}

				return filter;
			}
		}

		/// <summary>
		/// The StationFilter override
		/// </summary>
		private StationFilter filter = null;
	}
}