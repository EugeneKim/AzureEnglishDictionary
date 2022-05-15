using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionApp.Functions
{
	/// <summary>
	/// Publish an event to Azure Event Grid when a new item is added to the Cosmos DB.
	/// </summary>
	internal class EventPublishFunction
	{
		#region Fields

		private readonly ILogger<EventPublishFunction> logger;

		#endregion

		#region Construction

		public EventPublishFunction(ILogger<EventPublishFunction> logger) => this.logger = logger;

		#endregion

		#region Public Methods

		[Function("publish")]
		[EventGridOutput(TopicEndpointUri = "TopicEndpointUri", TopicKeySetting = "TopicKeySetting")]
		public WordsEventType Publish(
			[CosmosDBTrigger(
				databaseName: "%DatabaseId%",
				collectionName: "%ContainerId%",
				ConnectionStringSetting = "ConnectionStrings:CosmosDB",
				LeaseCollectionName = "leases",
				CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<WordItem> newWordItems)
		{
			logger.LogInformation($"Triggered by cosmos DB.");

			var newWords = new List<string>();

			if ((newWordItems is not null) && (newWordItems.Count > 0))
			{
				newWords.AddRange(newWordItems.Select(wi => wi.Id));
				logger.LogInformation($"{newWords.Count} Word(s) added to the database: {string.Join(", ", newWords)}");
			}

			var wordsEventType = new WordsEventType()
			{
				Id = Guid.NewGuid().ToString(),
				Subject = "word-added-event-subject",
				EventType = "WordsAdded",
				DataVersion = "1.0",
				Data = newWords
			};

			return wordsEventType;
		}

		#endregion
	}
}
