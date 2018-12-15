using Android.Content;

namespace RailTimeGrabber
{
	class StorageMechanism : IStorageMechanism
	{
		public StorageMechanism( Context context ) => StorageContext = context;

		/// <summary>
		/// Get a boolean item from the storage
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public bool GetBoolItem( string itemName, bool defaultValue ) =>
			StorageContext.GetSharedPreferences( StorageName, FileCreationMode.Private ).GetBoolean( itemName, defaultValue );

		/// <summary>
		/// Set a boolean item in the storage
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="value"></param>
		public void SetBoolItem( string itemName, bool value ) =>
			StorageContext.GetSharedPreferences( StorageName, FileCreationMode.Private ).Edit().PutBoolean( itemName, value ).Commit();

		/// <summary>
		/// Get the value of the specified string item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public string GetStringItem( string itemName, string defaultValue ) =>
			StorageContext.GetSharedPreferences( StorageName, FileCreationMode.Private ).GetString( itemName, defaultValue );

		/// <summary>
		/// Set the value of the specified string item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public void SetStringItem( string itemName, string value ) =>
			StorageContext.GetSharedPreferences( StorageName, FileCreationMode.Private ).Edit().PutString( itemName, value ).Commit();

		/// <summary>
		/// Get the value of the specified integer item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public int GetIntItem( string itemName, int defaultValue ) =>
			StorageContext.GetSharedPreferences( StorageName, FileCreationMode.Private ).GetInt( itemName, defaultValue );

		/// <summary>
		/// Set the value of the specified integer item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public void SetIntItem( string itemName, int value ) =>
			StorageContext.GetSharedPreferences( StorageName, FileCreationMode.Private ).Edit().PutInt( itemName, value ).Commit();

		/// <summary>
		/// Remove the specified item from the storage
		/// </summary>
		/// <param name="itemName"></param>
		public void DeleteItem( string itemName ) =>
			StorageContext.GetSharedPreferences( StorageName, FileCreationMode.Private ).Edit().Remove( itemName ).Commit();

		/// <summary>
		/// The Context object to access the storage object
		/// </summary>
		private static Context StorageContext { get; set; } = null;

		/// <summary>
		/// Storage names
		/// </summary>
		private const string StorageName = "RailTimeGrabberStorage";
	}
}