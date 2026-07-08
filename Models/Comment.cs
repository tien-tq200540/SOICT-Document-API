namespace SOICT.DocumentSystem.API.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } // Nội dung bình luận
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int DocumentId { get; set; }
        public Document Document { get; set; }

        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; } // Lưu tên hiển thị của người bình luận
    }
}