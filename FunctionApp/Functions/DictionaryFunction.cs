using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DictionaryClientService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WordDbClientService;

namespace FunctionApp.Functions
{
	internal class DictionaryFunction
	{
		#region Fields

		private readonly IWordDbClient wordDbClient;
		private readonly IDictionaryClient dictionaryClient;

		#endregion

		#region Construction

		public DictionaryFunction(IWordDbClient wordDbClient, IDictionaryClient dictionaryClient)
		{
			this.wordDbClient = wordDbClient;
			this.dictionaryClient= dictionaryClient;
		}

		#endregion

		#region Public Methods

		[Function("add")]
		public async Task<HttpResponseData> AddWordAsync(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData request,
			FunctionContext context,
			string word)
		{
			var logger = context.GetLogger(nameof(DictionaryFunction));
			logger.LogInformation($"Requested to add a new word: {word}");

			var response = request.CreateResponse();

			var regEx = new Regex("^[a-zA-Z]+([a-zA-Z]+ )*[a-zA-Z]+$");

			if (regEx.IsMatch(word) is not true)
			{
				response.StatusCode = HttpStatusCode.Forbidden;
				await response.WriteStringAsync($"Invalid word: {word}");
			}
			else
			{
				// Normalise.
				word = word.ToLower();

				if (await wordDbClient.DoesWordExistAsync(word))
				{
					response.StatusCode = HttpStatusCode.Forbidden;
					await response.WriteStringAsync($"Already exist: {word}");
				}
				else
				{
					var wordItem = await dictionaryClient.GetAsync(word);

					if (wordItem is not null)
					{
						await wordDbClient.AddWordAsync(wordItem);

						response.StatusCode = HttpStatusCode.OK;
						await response.WriteStringAsync($"Successfully added: {word}");
					}
					else
					{
						response.StatusCode = HttpStatusCode.NotFound;
						await response.WriteStringAsync($"Cannot find the word in the dictionary: {word}");
					}
				}
			}

			return response;
		}

		#endregion
	}
}
