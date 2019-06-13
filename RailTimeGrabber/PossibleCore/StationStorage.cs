using System.Collections.Generic;
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

						// Only the station names are used most of the time so extract them into a simple string array
						// Also provide a 'code' lookup dictionary
						// Don't use Linq yet to avoid having to include the Linq library
						StationNames = new string[ stations.Items.Length ];
						int index = 0;
						foreach ( Station station in stations.Items )
						{
							StationNames[ index++ ] = station.Name;
							codeLookup[ station.Name ] = station.Code;
						}
					}
				} );
			}
		}

		/// <summary>
		/// The station names
		/// </summary>
		public static string[] StationNames { get; private set; }

		/// <summary>
		/// Get the station code for the specified station
		/// </summary>
		/// <param name="stationName"></param>
		/// <returns></returns>
		public static string GetCode( string stationName )
		{
			string code = "";
			if ( codeLookup.ContainsKey( stationName ) == true )
			{
				code = codeLookup[ stationName ];
			}

			return code;
		}

		/// <summary>
		/// Dictionary providing a station name to code lookup
		/// </summary>
		private static Dictionary< string, string > codeLookup = new Dictionary<string, string>();
	}
}