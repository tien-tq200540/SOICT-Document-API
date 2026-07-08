using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOICT.DocumentSystem.API.Data;
using SOICT.DocumentSystem.API.DTOs;
using SOICT.DocumentSystem.API.Models;
using System.Security.Claims;

namespace SOICT.DocumentSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubjectRequestController : Controller
    {
        private readonly AppDbContext _context;

        public SubjectRequestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("request")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateSubjectRequestDto dto)
        {
            var codeExists = await _context.Subjects.AnyAsync(s => s.SubjectCode.ToLower() == dto.SubjectCode.ToLower());
            if (codeExists) return BadRequest("Môn học với mã này đã tồn tại trên hệ thống rồi!");

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var newRequest = new SubjectRequest
            {
                SubjectCode = dto.SubjectCode.ToUpper(),
                Name = dto.Name,
                RequestedBy = userEmail,
                Status = "Pending"
            };

            _context.SubjectRequests.Add(newRequest);
            await _context.SaveChangesAsync();

            return Ok("Gửi yêu cầu tạo môn học thành công! Chờ Admin phê duyệt nhé cậu.");
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _context.SubjectRequests.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(requests);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _context.SubjectRequests.FindAsync(id);
            if (request == null) return NotFound("Không tìm thấy yêu cầu này!");
            if (request.Status != "Pending") return BadRequest("Yêu cầu này đã được xử lý từ trước!");

            request.Status = "Approved";

            var newSubject = new Subject
            {
                SubjectCode = request.SubjectCode,
                Name = request.Name
            };
            _context.Subjects.Add(newSubject);

            await _context.SaveChangesAsync();
            return Ok($"Đã phê duyệt! Môn học {newSubject.SubjectCode} đã chính thức được tạo.");
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.SubjectRequests.FindAsync(id);
            if (request == null) return NotFound("Không tìm thấy yêu cầu!");
            if (request.Status != "Pending") return BadRequest("Yêu cầu này đã được xử lý từ trước!");

            request.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok("Đã từ chối yêu cầu tạo môn học này.");
        }
    }
}
