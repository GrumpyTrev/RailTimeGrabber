using System;
using System.Threading;

namespace RailTimeGrabber
{
	/// <summary>
	/// The ResultsTimer class keeps track of how old a set of results are and reports this age in a text message 
	/// </summary>
	class ResultsTimer
	{
		/// <summary>
		/// Constructor - start the timer
		/// </summary>
		public ResultsTimer()
		{
			// Start a timer to increment the update time
			updateTimer = new Timer( x => UpdateMessage(), null, TimeSpan.FromSeconds( 30 ), TimeSpan.FromSeconds( 30 ) );
		}

		/// <summary>
		/// Clear message by resetting the last updated time
		/// </summary>
		public void ResetTimer()
		{
			lastUpdate = DateTime.MinValue;
			UpdateMessage();
		}

		/// <summary>
		/// Start the timing process at the current time
		/// </summary>
		public void JustUpdated()
		{
			lastUpdate = DateTime.Now;
			UpdateMessage();
		}

		/// <summary>
		/// Update the message displaying how long since the results were updated
		/// </summary>
		private void UpdateMessage()
		{
			string newText = textMessage;

			// If there has been an update then display how old it is
			if ( lastUpdate != DateTime.MinValue )
			{
				TimeSpan updateSpan = DateTime.Now - lastUpdate;

				if ( updateSpan.TotalMinutes < 1 )
				{
					newText = "Updated a moment ago";
				}
				else if ( updateSpan.TotalMinutes < 60 )
				{
					if ( updateSpan.TotalMinutes < 2 )
					{
						newText = "Updated a minute ago";
					}
					else
					{
						newText = string.Format( "Updated {0} minutes ago", ( int )updateSpan.TotalMinutes );
					}
				}
				else if ( updateSpan.TotalHours < 24 )
				{
					if ( updateSpan.TotalHours < 2 )
					{
						newText = "Updated an hour ago";
					}
					else
					{
						newText = string.Format( "Updated {0} hours ago", ( int )updateSpan.TotalHours );
					}
				}
				else
				{
					newText = "Updated more than a day ago";
				}
			}
			// If there is a trip selected then prompts for an update
			else if ( TrainTrips.Selected != -1 )
			{
				newText = "Click to update";
			}
			else
			{
				// Prevent an update if nothing is selected
				newText = "";
			}

			// If there has been a change then report it
			if ( newText != textMessage )
			{
				textMessage = newText;
				TextChangedEvent?.Invoke( this, new TextChangedEventArgs { Text = textMessage } );
			}
		}


		/// <summary>
		/// Eventhandler used to link into the text message changes
		/// </summary>
		public event EventHandler<TextChangedEventArgs> TextChangedEvent;

		/// <summary>
		/// The updated text message
		/// </summary>
		public class TextChangedEventArgs: EventArgs
		{
			public string Text { get; set; }
		}

		/// <summary>
		/// Timer used to change the update text
		/// </summary>
		private readonly Timer updateTimer = null;

		/// <summary>
		/// THe last reported message
		/// </summary>
		private string textMessage = "";

		/// <summary>
		/// The last time the journeys were updated
		/// </summary>
		private DateTime lastUpdate = DateTime.MinValue;
	}
}