using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOICT.DocumentSystem.API.Constants;
using SOICT.DocumentSystem.API.Data;
using SOICT.DocumentSystem.API.DTOs;
using SOICT.DocumentSystem.API.Services;

namespace SOICT.DocumentSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly BlobService _blobService;

        public AdminController(AppDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        [HttpGet("documents/pending")]
        [Authorize(Roles = $"{UserRoles.SUPER_ADMIN},{UserRoles.ADMIN}")]
        public async Task<IActionResult> GetPendingDocuments()
        {
            var pendingDocs = await _context.Documents
                .Where(d => d.Status == DocumentStatus.PENDING)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
            return Ok(pendingDocs);
        }

        [HttpPut("documents/{id}/approve")]
        [Authorize(Roles = $"{UserRoles.SUPER_ADMIN},{UserRoles.ADMIN}")]
        public async Task<IActionResult> ApproveDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound("Không tìm thấy tài liệu!");
            if (document.Status != DocumentStatus.PENDING) return BadRequest("Tài liệu này đã được xử lý từ trước!");

            document.Status = DocumentStatus.APPROVED;
            await _context.SaveChangesAsync();

            return Ok($"Đã duyệt tài liệu: {document.Title}. Hiện tại mọi sinh viên đã có thể xem và tải về.");
        }

        [HttpDelete("documents/{id}/reject")]
        [Authorize(Roles = $"{UserRoles.SUPER_ADMIN},{UserRoles.ADMIN}")]
        public async Task<IActionResult> RejectDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound("Không tìm thấy tài liệu!");
            if (document.Status != DocumentStatus.PENDING) return BadRequest("Tài liệu này đã được xử lý từ trước!");

            try
            {
                var uri = new Uri(document.FileUrl);
                var blobName = uri.Segments.Last();
                await _blobService.DeleteBlobAsync(blobName);
            }
            catch
            {
 
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return Ok("Đã từ chối và xóa hoàn toàn tài liệu vi phạm khỏi hệ thống.");
        }

        [HttpPut("users/{id}/role")]
        [Authorize(Roles = $"{UserRoles.SUPER_ADMIN}")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy tài khoản này!");

            if (user.Role == UserRoles.SUPER_ADMIN)
            {
                return BadRequest($"Không thể thay đổi quyền của tài khoản {UserRoles.SUPER_ADMIN}!");
            }

            string oldRole = user.Role;
            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            return Ok($"[{UserRoles.SUPER_ADMIN}] Đã chuyển đổi quyền của {user.FullName} từ '{oldRole}' sang '{user.Role}' thành công!");
        }

        [HttpGet("users")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,      // Tìm kiếm theo tên hoặc email
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(u => u.Email.ToLower().Contains(searchLower) ||
                                         u.FullName.ToLower().Contains(searchLower));
            }

            int totalItems = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role
                })
                .ToListAsync();

            var result = new PagedResultDto<object>
            {
                Items = users.Cast<object>().ToList(),
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(result);
        }
    }
}
