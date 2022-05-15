using System.Collections.Generic;

namespace FunctionApp
{
	public class WordsEventType
	{
		#region Properties

		public string Id { get; set; }
		public string Subject { get; set; }
		public string EventType { get; set; }
		public string DataVersion { get; set; }
		public IReadOnlyList<string> Data { get; set; }

		#endregion
	}
}