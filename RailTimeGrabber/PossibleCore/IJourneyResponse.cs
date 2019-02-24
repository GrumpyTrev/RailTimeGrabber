namespace RailTimeGrabber
{
	/// <summary>
	/// IJourneyResponse specifies the JourneyRequest reponses that a user of JourneyRequest must handle
	/// </summary>
	interface IJourneyResponse
	{
		/// <summary>
		/// Called when the set of journeys have been cleared, either at the start of a request or if no journeys have been found
		/// </summary>
		void JourneysCleared();

		/// <summary>
		/// Called when either a new set of journeys or an extended set of journeys have been retrieved
		/// </summary>
		/// <param name="journeys"></param>
		void JourneysAvailable( TrainJourneys journeys );

		/// <summary>
		/// Called when the request has completed
		/// </summary>
		void JourneyRequestComplete( bool networkProblem, bool noJourneysFound );
	}
}