using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace SOICT.DocumentSystem.API.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string ContainerName = "soict-documents";

        public BlobService(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("AzureStorage")["ConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }

        public string GenerateDownloadUrl(string fileUrl)
        {
            try
            {
                var uri = new Uri(fileUrl);
                var blobName = uri.Segments.Last();

                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = ContainerName,
                    BlobName = blobName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                return sasUri.ToString();
            }
            catch
            {
                return fileUrl;
            }
        }

        public async Task<bool> DeleteBlobAsync(string blobName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa file trên Blob Storage: {ex.Message}");
                return false;
            }
        }
    }
}