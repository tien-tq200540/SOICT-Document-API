namespace SOICT.DocumentSystem.API.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? DocumentType { get; set; }
        public string? SchoolYear { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public int DownloadCount { get; set; } = 0;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public string? UploadedBy { get; set; }
        public string Status { get; set; } = "Pending";
        public double AverageRating { get; set; } = 0.0;
    }
}
