using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using ServiceModels;
using System.Net;

namespace WordDbClientService
{
	public sealed class WordDbClient : IWordDbClient
	{
		#region Fields

		private readonly Container container;

		#endregion

		#region Construction

		public WordDbClient(string connectionString, string databaseId, string containerId)
		{
			var cosmosClient = new CosmosClient(connectionString);
			container = cosmosClient.GetContainer(databaseId, containerId);
		}

		#endregion

		#region Public Methods

		public async Task<bool> AddWordAsync(WordItem wordItem)
		{
			try
			{
				var response = await container.CreateItemAsync(wordItem, new PartitionKey(wordItem.Id));
				return response.StatusCode == HttpStatusCode.Created;
			}
			catch (CosmosException e) when (e.StatusCode == HttpStatusCode.Conflict)
			{
				return false;
			}
		}

		public async Task<WordItem> GetWordAsync(string word)
		{
			try
			{
				var response = await container.ReadItemAsync<WordItem>(word, new PartitionKey(word));
				return response.Resource;
			}
			catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
			{
				return null!;
			}
		}

		public async Task<bool> DoesWordExistAsync(string word) => await GetWordAsync(word) != null;

		public IEnumerable<WordItem> GetAllWords()
		{
			var iterator = container.GetItemLinqQueryable<WordItem>().ToFeedIterator();

			while (iterator.HasMoreResults)
			{
				foreach (var item in iterator.ReadNextAsync().GetAwaiter().GetResult())
					yield return item;
			}
		}

		public IEnumerable<WordItem> GetWords(uint offset, uint limit, string? search = default)
		{
			var queryString = NormalizeQueryStringForSearchKeyword($"SELECT * FROM container<> OFFSET {offset} LIMIT {limit}", search);
			var query = new QueryDefinition(queryString);

			using var iterator = container.GetItemQueryIterator<WordItem>(query);

			while (iterator.HasMoreResults)
			{
				foreach (var item in iterator.ReadNextAsync().GetAwaiter().GetResult())
					yield return item;
			}
		}

		public async Task<int> GetTotalWordsAsync(string? search = default)
		{
			var queryString = NormalizeQueryStringForSearchKeyword($"SELECT VALUE COUNT(1) FROM container<>", search);
			var query = container.GetItemQueryIterator<int>(queryString);
			var count = 0;

			while (query.HasMoreResults)
				count = (await query.ReadNextAsync()).SingleOrDefault();

			return count;
		}

		public async Task DeleteWordAsync(string word)
		{
			try
			{
				await container.DeleteItemAsync<WordItem>(word, new PartitionKey(word));
			}
			catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
			{
				// Swallow the not found exception.
			}
		}

		public async Task<bool> UpdateWordAsync(WordItem wordItem)
		{
			var response = await container.ReplaceItemAsync(wordItem, wordItem.Id, new PartitionKey(wordItem.Id));
			return response.StatusCode == HttpStatusCode.OK;
		}

		#endregion

		#region Private Methods

		private static string NormalizeQueryStringForSearchKeyword(string query, string? search)
			 => query.Replace("<>", (string.IsNullOrEmpty(search) || string.IsNullOrWhiteSpace(search)) ? "" : $" WHERE container.id LIKE \"%{search.ToLower()}%\"");

		#endregion
	}
}