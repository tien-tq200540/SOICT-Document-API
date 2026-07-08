using System.ComponentModel.DataAnnotations;

namespace SOICT.DocumentSystem.API.DTOs
{
    public class UploadDocumentDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public IFormFile File { get; set; }

        [Required]
        [RegularExpression("^(Slide|Exam|Exercise|Syllabus|Book)$",
            ErrorMessage = "Loại tài liệu phải thuộc các nhóm: Slide, Exam (Đề thi), Exercise (Bài tập), Syllabus (Đề cương) hoặc Book (Sách)")]
        public string? DocumentType { get; set; }

        [Required]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Mã năm học phải gồm 5 chữ số đúng chuẩn SOICT (Ví dụ: 20241, 20242)")]
        public string? SchoolYear { get; set; }
    }
}