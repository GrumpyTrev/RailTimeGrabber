using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Android.Content.Res;

namespace RailTimeGrabber
{
	/// <summary>
	/// Class responsible for reading the fixed list of station names
	/// </summary>
	class StationStorage
	{
		/// <summary>
		/// Read the station information from the assets
		/// </summary>
		/// <param name="manager"></param>
		/// <returns></returns>
		public static async void ReadStations( AssetManager manager )
		{
			if ( StationNames == null )
			{
				await Task.Run( () => {

					using ( StreamReader sr = new StreamReader( manager.Open( "Stations.xml" ) ) )
					{
						Stations stations = ( Stations )new XmlSerializer( typeof( Stations ) ).Deserialize( sr );

						// All we actually ever use are the station names so extract them into a simple string array
						// Don't use Linq yet to avoid having to include the Linq library
						StationNames = new string[ stations.Items.Length ];
						int index = 0;
						foreach ( Station station in stations.Items )
						{
							StationNames[ index++ ] = station.Name;
						}
					}
				} );
			}
		}

		/// <summary>
		/// The station names
		/// </summary>
		public static string[] StationNames { get; private set; }
	}
}