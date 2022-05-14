namespace ArtifactClientService
{
	public interface IArtifactClient
	{
		#region Definitions

		Task UploadAsync(string containerName, string localFilePath, bool overwrite);
		Task UploadAsync(string containerName, string blobName, Stream stream, bool overwrite);
		Task<bool> ExistsAsync(string containerName, string blobName);
		Task<bool> DeleteAsync(string containerName, string blobName);
		Task<string> DownloadToAsync(string containerName, string blobName, string downloadFilePath);
		Task<byte[]> DownloadToAsync(string containerName, string blobName);

		#endregion
	}
}
