namespace RailTimeGrabber
{
	/// <summary>
	/// The IStorageMechanism specifies the storage methods that must be supported by the target hardware
	/// </summary>
	public interface IStorageMechanism
    {
		/// <summary>
		/// Get the value of the specified boolean item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		bool GetBoolItem( string itemName, bool defaultValue );

		/// <summary>
		/// Set the value of the specified boolean item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="value"></param>
		void SetBoolItem( string itemName, bool value );

		/// <summary>
		/// Get the value of the specified string item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		string GetStringItem( string itemName, string defaultValue );

		/// <summary>
		/// Set the value of the specified string item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		void SetStringItem( string itemName, string value );

		/// <summary>
		/// Get the value of the specified integer item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		int GetIntItem( string itemName, int defaultValue );

		/// <summary>
		/// Set the value of the specified integer item
		/// </summary>
		/// <param name="itemName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		void SetIntItem( string itemName, int value );

		/// <summary>
		/// Remove the specified item from the storage
		/// </summary>
		/// <param name="itemName"></param>
		void DeleteItem( string itemName );
	}
}
