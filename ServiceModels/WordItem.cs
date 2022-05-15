using Newtonsoft.Json;

namespace ServiceModels
{
	#region Definitions

	public record WordItem(
		[property: JsonProperty("id")]
		string Id,
		[property: JsonProperty("meanings")]
		List<MeaningItem> Meanings);
	public record MeaningItem(
		[property: JsonProperty("partOfSpeech")]
		string PartOfSpeech,
		[property: JsonProperty("definitions")]
		List<DefinitionItem> Definitions);
	public record DefinitionItem(
		[property: JsonProperty("definition")]
		string Definition,
		[property: JsonProperty("synonyms")]
		List<string> Synonyms,
		[property: JsonProperty("antonyms")]
		List<string> Antonyms,
		[property: JsonProperty("example")]
		string Example);

	#endregion
}