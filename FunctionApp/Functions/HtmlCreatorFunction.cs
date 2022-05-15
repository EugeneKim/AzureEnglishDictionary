using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using ArtifactClientService;
using WordDbClientService;
using HtmlCreatorService;
using System.Text;

namespace FunctionApp.Functions
{
	public class HtmlCreatorFunction
	{
		#region Fields

		private readonly IWordDbClient wordDbClient;
		private readonly IArtifactClient artifactClient;
		private readonly IHtmlCreator htmlCreator;

		#endregion

		#region Construction

		public HtmlCreatorFunction(IWordDbClient wordDbClient, IArtifactClient artifactClient, IHtmlCreator htmlCreator)
		{
			this.wordDbClient = wordDbClient;
			this.artifactClient = artifactClient;
			this.htmlCreator = htmlCreator;
		}

		#endregion

		#region Public Methods

		[Function("create_html")]
		public async Task CreatePdfAsync([EventGridTrigger] WordsEventType input, FunctionContext context)
		{
			var logger = context.GetLogger(nameof(HtmlCreatorFunction));

			foreach (var word in input.Data)
			{
				var blobName = $"{word}.html";
				var containerName = Environment.GetEnvironmentVariable("HtmlBlobContainer");

				if (await artifactClient.ExistsAsync(containerName, blobName))
					logger.LogWarning($"Existing '{blobName}' will be overwritten.");

				var wordItem = await wordDbClient.GetWordAsync(word);
				var content = htmlCreator.Create(wordItem);

				using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

				await artifactClient.UploadAsync(containerName, blobName, stream, true);
				logger.LogInformation($"Successfully uploaded {blobName} to storage.");
			}
		}

		#endregion
	}
}
