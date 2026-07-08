using SOICT.DocumentSystem.API.Constants;
using System.ComponentModel.DataAnnotations;

namespace SOICT.DocumentSystem.API.DTOs
{
    public class UpdateRoleDto
    {
        [RegularExpression($"^({UserRoles.ADMIN}|{UserRoles.STUDENT})$",
            ErrorMessage = "Chỉ được phép chuyển đổi giữa 2 quyền: Admin hoặc Student")]
        public string Role { get; set; }
    }
}
