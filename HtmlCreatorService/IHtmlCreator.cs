using ServiceModels;

namespace HtmlCreatorService
{
	public interface IHtmlCreator
	{
		#region Definitions

		void Create(string file, WordItem wordItem);
		string Create(WordItem wordItem);

		#endregion
	}
}