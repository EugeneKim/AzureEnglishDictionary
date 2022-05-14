using Azure.Storage.Blobs;

namespace ArtifactClientService
{
	public class ArtifactClient : IArtifactClient
	{
		#region Fields

		private readonly BlobServiceClient blobServiceClient;

		#endregion

		#region Construction

		public ArtifactClient(string connectionString) => blobServiceClient = new BlobServiceClient(connectionString);

		#endregion

		#region Public Methods

		public async Task UploadAsync(string containerName, string localFilePath, bool overwrite)
		{
			var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
			var fileName = Path.GetFileName(localFilePath);
			var blobClient = blobContainerClient.GetBlobClient(fileName);
			await blobClient.UploadAsync(localFilePath, overwrite);
		}

		public async Task UploadAsync(string containerName, string blobName, Stream stream, bool overwrite)
		{
			var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = blobContainerClient.GetBlobClient(blobName);
			await blobClient.UploadAsync(stream, overwrite);
		}

		public async Task<bool> ExistsAsync(string containerName, string blobName)
		{
			var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = blobContainerClient.GetBlobClient(blobName);

			return await blobClient.ExistsAsync();
		}

		public async Task<bool> DeleteAsync(string containerName, string blobName)
		{
			var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = blobContainerClient.GetBlobClient(blobName);

			return await blobClient.DeleteIfExistsAsync();
		}

		public async Task<string> DownloadToAsync(string containerName, string blobName, string downloadFilePath)
		{
			var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = blobContainerClient.GetBlobClient(blobName);

			await blobClient.DownloadToAsync(downloadFilePath);

			return downloadFilePath;
		}

		public async Task<byte[]> DownloadToAsync(string containerName, string blobName)
		{
			var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = blobContainerClient.GetBlobClient(blobName);
			var response = await blobClient.DownloadContentAsync();

			return response.Value.Content.ToArray();
		}

		#endregion
	}
}