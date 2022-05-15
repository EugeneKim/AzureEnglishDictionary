using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using ArtifactClientService;
using PdfCreatorService;
using WordDbClientService;

namespace FunctionApp.Functions
{
	public class PdfCreatorFunction
	{
		#region Fields

		private readonly IWordDbClient wordDbClient;
		private readonly IArtifactClient artifactClient;
		private readonly IPdfCreator pdfCreator;

		#endregion

		#region Construction

		public PdfCreatorFunction(IWordDbClient wordDbClient, IArtifactClient artifactClient, IPdfCreator pdfCreator)
		{
			this.wordDbClient = wordDbClient;
			this.artifactClient = artifactClient;
			this.pdfCreator = pdfCreator;
		}

		#endregion

		#region Public Methods

		[Function("create_pdf")]
		public async Task CreatePdfAsync([EventGridTrigger] WordsEventType input, FunctionContext context)
		{
			var logger = context.GetLogger(nameof(PdfCreatorFunction));

			foreach (var word in input.Data)
			{
				var blobName = $"{word}.pdf";
				var containerName = Environment.GetEnvironmentVariable("PdfBlobContainer");

				if (await artifactClient.ExistsAsync(containerName, blobName))
					logger.LogWarning($"Existing '{blobName}' will be overwritten.");

				using var stream = new MemoryStream();
				var wordItem = await wordDbClient.GetWordAsync(word);

				pdfCreator.Create(stream, wordItem);

				await artifactClient.UploadAsync(containerName, blobName, stream, true);
				logger.LogInformation($"Successfully uploaded {blobName} to storage.");
			}
		}

		#endregion
	}
}
