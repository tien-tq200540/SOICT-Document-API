using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOICT.DocumentSystem.API.Constants;
using SOICT.DocumentSystem.API.Data;
using SOICT.DocumentSystem.API.DTOs;
using SOICT.DocumentSystem.API.Models;

namespace SOICT.DocumentSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubjectController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await _context.Subjects.ToListAsync();
            return Ok(subjects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            var subject = await _context.Subjects.Include(s => s.Documents)
                                                 .FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null) return NotFound("Không tìm thấy môn học này!");

            return Ok(new
            {
                subject.Id,
                subject.SubjectCode,
                subject.Name,
                Documents = subject.Documents.Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.Description,
                    d.FileUrl,
                    d.UploadedAt,
                    d.DownloadCount
                })
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.SUPER_ADMIN},{UserRoles.ADMIN}")]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDto dto)
        {
            if (await _context.Subjects.AnyAsync(s => s.SubjectCode.ToLower() == dto.SubjectCode.ToLower()))
            {
                return BadRequest($"Mã môn học {dto.SubjectCode} đã tồn tại trên hệ thống!");
            }

            var newSubject = new Subject
            {
                SubjectCode = dto.SubjectCode.ToUpper(),
                Name = dto.Name
            };

            _context.Subjects.Add(newSubject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubjectById), new { id = newSubject.Id }, newSubject);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{UserRoles.SUPER_ADMIN},{UserRoles.ADMIN}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectDto dto)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound("Không tìm thấy môn học để sửa!");

            if (!string.IsNullOrWhiteSpace(dto.SubjectCode) && dto.SubjectCode.ToUpper() != subject.SubjectCode)
            {
                if (await _context.Subjects.AnyAsync(s => s.SubjectCode.ToLower() == dto.SubjectCode.ToLower()))
                {
                    return BadRequest($"Không thể sửa thành mã {dto.SubjectCode} vì mã này đã thuộc về môn khác!");
                }
            }

            subject.SubjectCode = !string.IsNullOrWhiteSpace(dto.SubjectCode) ? dto.SubjectCode.ToUpper() : subject.SubjectCode;
            subject.Name = !string.IsNullOrWhiteSpace(dto.Name) ? dto.Name : subject.Name;

            await _context.SaveChangesAsync();
            return Ok(subject);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{UserRoles.SUPER_ADMIN},{UserRoles.ADMIN}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _context.Subjects.Include(s => s.Documents).FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null) return NotFound("Không tìm thấy môn học để xóa!");

            if (subject.Documents.Any())
            {
                return BadRequest("Không thể xóa môn học này vì vẫn còn tài liệu thuộc về môn học!");
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return Ok("Xóa môn học thành công!");
        }
    }
}