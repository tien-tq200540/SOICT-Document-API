using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOICT.DocumentSystem.API.Data;
using SOICT.DocumentSystem.API.DTOs;
using System.Security.Claims;

namespace SOICT.DocumentSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token không hợp lệ!");

            int userId = int.Parse(userIdClaim);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Không tìm thấy người dùng!");

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                StudentCohort = user.StudentCohort,
                School = user.School
            };

            return Ok(profileDto);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token không hợp lệ!");

            int userId = int.Parse(userIdClaim);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Không tìm thấy người dùng!");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            if (!string.IsNullOrWhiteSpace(dto.StudentCohort))
            {
                user.StudentCohort = dto.StudentCohort;
            }

            if (!string.IsNullOrWhiteSpace(dto.School))
            {
                user.School = dto.School;
            }

            await _context.SaveChangesAsync();

            return Ok("Cập nhật thông tin hồ sơ thành công!");
        }
    }
}
