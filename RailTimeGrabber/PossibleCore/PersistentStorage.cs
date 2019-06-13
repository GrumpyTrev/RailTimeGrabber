
using System.Collections.Generic;

namespace RailTimeGrabber
{
	public class PersistentStorage
	{
		public static bool GetBoolItem( string itemName, bool defaultState )
		{
			if ( ( UseCache == false ) || ( cachedItems.ContainsKey( itemName ) == false ) )
			{
				cachedItems[ itemName ] = StorageMechanism.GetBoolItem( itemName, defaultState );
			}

			return ( bool )cachedItems[ itemName ];
		}

		public static void SetBoolItem( string itemName, bool state )
		{
			cachedItems[ itemName ] = state;
			StorageMechanism.SetBoolItem( itemName, state );
		}

		public static string GetStringItem( string itemName, string defaultState )
		{
			if ( ( UseCache == false ) || ( cachedItems.ContainsKey( itemName ) == false ) )
			{
				cachedItems[ itemName ] = StorageMechanism.GetStringItem( itemName, defaultState );
			}

			return ( string )cachedItems[ itemName ];
		}

		public static void SetStringItem( string itemName, string state )
		{
			cachedItems[ itemName ] = state;
			StorageMechanism.SetStringItem( itemName, state );
		}

		public static int GetIntItem( string itemName, int defaultState )
		{
			if ( ( UseCache == false ) || ( cachedItems.ContainsKey( itemName ) == false ) )
			{
				cachedItems[ itemName ] = StorageMechanism.GetIntItem( itemName, defaultState );
			}

			return ( int )cachedItems[ itemName ];
		}

		public static void SetIntItem( string itemName, int state )
		{
			cachedItems[ itemName ] = state;
			StorageMechanism.SetIntItem( itemName, state );
		}

		public void DeleteItem( string itemName )
		{
			cachedItems.Remove( itemName );
			StorageMechanism.DeleteItem( itemName );
		}

		public static IStorageMechanism StorageMechanism
		{
			private get;
			set;
		} 
		= null;

		/// <summary>
		/// Allow the use of the cache for reading to be controlled. It will be used by default.
		/// </summary>
		public static bool UseCache
		{
			private get;
			set;
		}
		= true;

		/// <summary>
		/// Some items that have already been retrived from persistent storage
		/// </summary>
		private static Dictionary<string, object> cachedItems = new Dictionary<string, object>();
	}
}