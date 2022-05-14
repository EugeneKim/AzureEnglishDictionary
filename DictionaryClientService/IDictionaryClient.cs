using ServiceModels;

namespace DictionaryClientService
{
	public interface IDictionaryClient
	{
		#region Definitions

		Task<WordItem> GetAsync(string word);

		#endregion
	}
}
