using System.ComponentModel.DataAnnotations;

namespace SOICT.DocumentSystem.API.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Chưa nhập Email!")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Chưa nhập Mật khẩu!")]
        public string Password { get; set; }
    }
}