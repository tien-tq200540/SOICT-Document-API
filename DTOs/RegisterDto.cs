using System.ComponentModel.DataAnnotations;

namespace SOICT.DocumentSystem.API.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email không được để trống!")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ!")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email phải có ký tự '@' và tên miền hợp lệ!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống!")]
        [RegularExpression(@"^[\p{L}\s]+$",
            ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng!")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự!")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{6,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường và 1 chữ số!")]
        public string Password { get; set; }
    }
}