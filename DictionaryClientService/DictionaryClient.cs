using ServiceModels;
using System.Net.Http.Json;

namespace DictionaryClientService
{
	/// <summary>
	/// The client using the Offline dictionary.
	/// </summary>
	/// <remarks>
	/// All credit of the offline dictionary goes to https://dictionaryapi.dev/
	/// </remarks>
	public sealed class DictionaryClient : IDictionaryClient
	{
		#region Public Methods

		public async Task<WordItem> GetAsync(string word)
		{
			word = word ?? throw new ArgumentNullException(nameof(word));
			word = word.Trim().ToLower();

			using var client = new HttpClient();

			try
			{
				var words = await client.GetFromJsonAsync<List<WordItem>>($"https://api.dictionaryapi.dev/api/v2/entries/en/{word}");

				// Normalise the structure.
				var meanings = words!
					.SelectMany(w => w.Meanings)
					.GroupBy(m => m.PartOfSpeech)
					.Select(g => new MeaningItem(g.Key, g.SelectMany(d => d.Definitions).ToList())).ToList();

				return new WordItem(word, meanings);
			}
			catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null!;
			}
		}

		#endregion
	}
}
