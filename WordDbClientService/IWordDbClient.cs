using ServiceModels;

namespace WordDbClientService
{
	public interface IWordDbClient
	{
		#region Definitions

		Task<bool> AddWordAsync(WordItem wordItem);

		Task<WordItem> GetWordAsync(string word);

		Task<bool> DoesWordExistAsync(string word);

		/// <summary>
		/// Get all word items.
		/// </summary>
		/// <returns>A collection of the word items.</returns>
		IEnumerable<WordItem> GetAllWords();

		/// <summary>
		/// Paginate the word items.
		/// </summary>
		/// <param name="offset">The number of word items that the results should skip. Zero-based index.</param>
		/// <param name="limit">The number of word items that the results should include.</param>
		/// <returns>A paginated collection of the word items.</returns>
		//IEnumerable<WordItem> GetWords(uint offset, uint limit);

		/// <summary>
		/// Paginate the word items.
		/// </summary>
		/// <param name="offset">The number of word items that the results should skip. Zero-based index.</param>
		/// <param name="limit">The number of word items that the results should include.</param>
		/// <param name="search">Search keyword (word) for the word items.</param>
		/// <returns>A paginated collection of the word items.</returns>
		IEnumerable<WordItem> GetWords(uint offset, uint limit, string? search = default);

		/// <summary>
		/// Get total number of the word items.
		/// </summary>
		/// <param name="search">Search keyword (word) for the word items.</param>
		/// <returns>Totan number of the word items.</returns>
		Task<int> GetTotalWordsAsync(string? search = default);

		Task DeleteWordAsync(string word);

		Task<bool> UpdateWordAsync(WordItem wordItem);

		#endregion
	}
}