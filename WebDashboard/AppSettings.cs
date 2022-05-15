#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace WebDashboard
{
	public class AppSettings
	{
		public string DatabaseConnectionString { get; set; }
		public string BlobStorageConnectionString { get; set; }

		public string DatabaseId { get; set; }
		public string ContainerId { get; set; }
		public string FunctionAppUri { get; set; }

		public string HtmlBlobContainer { get; set; }
		public string JsonBlobContainer { get; set; }
		public string PdfBlobContainer { get; set; }
	}
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.