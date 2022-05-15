using ServiceModels;
using System.Text;

namespace HtmlCreatorService
{
	public class HtmlCreator : IHtmlCreator
	{
		#region Public Methods

		public void Create(string file, WordItem wordItem) => File.WriteAllText(file, Create(wordItem));

		public string Create(WordItem wordItem)
		{
			var sb = new StringBuilder();
			sb.Append("<!DOCTYPE html>");
			sb.Append("<html>");
			sb.Append("<style>");
			sb.Append("body{font-size:medium}");
			sb.Append("#title{font-style:italic;font-weight:bold;font-size:large}");
			sb.Append("#partOfSpeech{font-weight:bold;font-size:medium;background-color:#92a8d1}");
			sb.Append("#sa{margin-left:10px;text-decoration:underline;font-size:small}");
			sb.Append("#example{margin-left:5px;color:#505050;font-style:italic;font-size:small}");
			sb.Append("</style>");
			sb.Append("<body>");
			sb.Append($"<div id=\"title\">{wordItem.Id}</div>");

			foreach (var meaning in wordItem.Meanings)
			{
				sb.Append($"<div id=\"partOfSpeech\">{meaning.PartOfSpeech}</div>");

				foreach (var definition in meaning.Definitions)
				{
					sb.Append("<div>");
					sb.Append($"<p>{definition.Definition}</p>");

					if (definition.Synonyms.Any())
						sb.Append($"<p id=\"sa\">(S) {string.Join(", ", definition.Synonyms)}</p>");

					if (definition.Antonyms.Any())
						sb.Append($"<p id=\"sa\">(A) {string.Join(", ", definition.Antonyms)}</p>");

					sb.Append($"<p id=\"example\">{definition.Example}</p>");
					sb.Append("</div>");
				}
			}
			sb.Append("</body>");
			sb.Append("</html>");

			return sb.ToString();
		}

		#endregion
	}
}