using System.IO;
using System.Xml.Serialization;
using Android.Content.Res;

namespace RailTimeGrabber
{
	class StationStorage
	{
		public static void ReadStations( AssetManager manager )
		{
			if ( Stations == null )
			{
				using ( StreamReader sr = new StreamReader( manager.Open( "Stations.xml" ) ) )
				{
					XmlSerializer serializer = new XmlSerializer( typeof( Stations ) );
					Stations = ( Stations )serializer.Deserialize( sr );
				}
			}
		}

		public static Stations Stations { get; set; }
	}
}