using System.ComponentModel.DataAnnotations;

namespace SOICT.DocumentSystem.API.DTOs
{
    public class UpdateSubjectDto
    {
        [RegularExpression(@"^[a-zA-Z]{2}\d{4}$",
            ErrorMessage = "Mã môn học phải đúng chuẩn 2 chữ cái và 4 chữ số (Ví dụ: IT3100, MI1110)")]
        public string? SubjectCode { get; set; }

        [RegularExpression(@"^[\p{L}\d\s\-,.+()]+$",
            ErrorMessage = "Tên môn học chứa ký tự không hợp lệ!")]
        public string? Name { get; set; }
    }
}
