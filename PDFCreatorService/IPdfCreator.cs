using ServiceModels;

namespace PdfCreatorService
{
	public interface IPdfCreator
	{
		#region Definitions

		void Create(Stream stream, WordItem wordItem);
		void Create(string file, WordItem wordItem);

		#endregion
	}
}
