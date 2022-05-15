using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Colors;
using ServiceModels;

namespace PdfCreatorService
{
	public sealed class PdfCreator : IPdfCreator
	{
		#region Public Methods

		public void Create(Stream stream, WordItem wordItem)
		{
			var writer = new PdfWriter(stream);

			// TODO: Check if the writer instance needs to be disposed manually
			// or disposing the stream would do it automatically.
			writer.SetCloseStream(false);
			Create(writer, wordItem);

			// Rewind to use the stream later.
			stream.Position = 0L;
		}

		public void Create(string file, WordItem wordItem)
		{
			using var writer = new PdfWriter(file);
			Create(writer, wordItem);
		}

		#endregion

		#region Private Methods

		private static void Create(PdfWriter writer, WordItem wordItem)
		{
			using var pdf = new PdfDocument(writer);
			using var document = new Document(pdf);

			Style headerStyle = new();
			headerStyle.SetBold().SetItalic().SetFontSize(20.0f);

			Style partOfSpeechStyle = new();
			partOfSpeechStyle.SetBold().SetBackgroundColor(new DeviceRgb(200, 200, 200));

			Style exampleStyle = new();
			exampleStyle.SetItalic().SetMarginLeft(20.0f).SetFontColor(new DeviceRgb(100, 100, 100));

			Style indexStyle = new();
			indexStyle.SetBold().SetFontColor(new DeviceRgb(50, 50, 50)).SetBackgroundColor(new DeviceRgb(220, 220, 220));

			Style synonymsAndAntonymStyle = new();
			synonymsAndAntonymStyle.SetFontColor(new DeviceRgb(80, 80, 80)).SetFontSize(10);

			document.Add(new Paragraph(wordItem.Id).AddStyle(headerStyle));

			foreach (var meaning in wordItem.Meanings)
			{
				document.Add(new Paragraph(meaning.PartOfSpeech).AddStyle(partOfSpeechStyle));

				var index = 1;
				foreach (var definition in meaning.Definitions)
				{
					document.Add(new Paragraph().Add(new Text($"{index++}").AddStyle(indexStyle)).Add($" {definition.Definition}"));

					if (definition.Synonyms.Count > 0)
						document.Add(new Paragraph($"(S) {string.Join(", ", definition.Synonyms)}").AddStyle(synonymsAndAntonymStyle));

					if (definition.Antonyms.Count > 0)
						document.Add(new Paragraph($"(A) {string.Join(", ", definition.Antonyms)}").AddStyle(synonymsAndAntonymStyle));

					if (!string.IsNullOrEmpty(definition.Example))
						document.Add(new Paragraph($"; {definition.Example}").AddStyle(exampleStyle));
				}
			}
		}

		#endregion
	}
}
