using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOICT.DocumentSystem.API.Constants;
using SOICT.DocumentSystem.API.Data;
using SOICT.DocumentSystem.API.DTOs;
using SOICT.DocumentSystem.API.Models;
using SOICT.DocumentSystem.API.Services;
using System.Security.Claims;

namespace SOICT.DocumentSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BlobService _blobService;

        public DocumentController(AppDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments(
            [FromQuery] string? search,
            [FromQuery] int? subjectId,
            [FromQuery] string? documentType,
            [FromQuery] string? schoolYear,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Documents.AsQueryable();

            query = query.Where(d => d.Status == DocumentStatus.APPROVED);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Title.Contains(search) || d.Description.Contains(search));
            }

            if (subjectId.HasValue)
            {
                query = query.Where(d => d.SubjectId == subjectId.Value);
            }

            if (!string.IsNullOrWhiteSpace(documentType))
            {
                query = query.Where(d => d.DocumentType == documentType);
            }

            if (!string.IsNullOrWhiteSpace(schoolYear))
            {
                query = query.Where(d => d.SchoolYear == schoolYear);
            }

            int totalItems = await query.CountAsync();

            var documents = await query
                .OrderByDescending(d => d.AverageRating)
                .ThenByDescending(d => d.UploadedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.Description,
                    d.FileUrl,
                    d.UploadedAt,
                    d.DownloadCount,
                    d.SubjectId,
                    d.AverageRating,
                    d.DocumentType,
                    d.SchoolYear
                })
                .ToListAsync();

            var result = new PagedResultDto<object>
            {
                Items = documents.Cast<object>().ToList(),
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound("Không tìm thấy tài liệu này!");

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (document.Status != DocumentStatus.APPROVED && currentUserRole != UserRoles.SUPER_ADMIN && currentUserRole != UserRoles.ADMIN)
            {
                return StatusCode(403, "Tài liệu này đang trong quá trình kiểm duyệt, vui lòng quay lại sau!");
            }

            return Ok(new
            {
                document.Id,
                document.Title,
                document.DocumentType,
                document.SchoolYear,
                document.Description,
                document.FileUrl,
                document.UploadedAt,
                document.DownloadCount,
                document.SubjectId,
                document.Status
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateDocument([FromForm] UploadDocumentDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest("Vui lòng chọn một file tài liệu hợp lệ!");
            }

            var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId);
            if (!subjectExists)
            {
                return BadRequest("Môn học không tồn tại!");
            }

            string fileUrlFromCloud = await _blobService.UploadFileAsync(dto.File);

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var newDocument = new Document
            {
                Title = dto.Title,
                Description = dto.Description,
                DocumentType = dto.DocumentType,
                SchoolYear = dto.SchoolYear,
                FileUrl = fileUrlFromCloud,
                SubjectId = dto.SubjectId,
                UploadedBy = userEmail
            };

            _context.Documents.Add(newDocument);
            await _context.SaveChangesAsync();

            return Ok(newDocument);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateDocument(int id, [FromForm] UpdateDocumentDto dto)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound("Không tìm thấy tài liệu!");

            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (document.UploadedBy != currentUserEmail)
            {
                return StatusCode(403, "Bạn không có quyền chỉnh sửa tài liệu của người khác!");
            }

            document.Title = !string.IsNullOrWhiteSpace(dto.Title) ? dto.Title : document.Title;
            document.Description = !string.IsNullOrWhiteSpace(dto.Description) ? dto.Description : document.Description;
            document.DocumentType = !string.IsNullOrWhiteSpace(dto.DocumentType) ? dto.DocumentType : document.DocumentType;
            document.SchoolYear = !string.IsNullOrWhiteSpace(dto.SchoolYear) ? dto.SchoolYear : document.SchoolYear;

            if (dto.File != null && dto.File.Length > 0)
            {
                try
                {
                    var oldUri = new Uri(document.FileUrl);
                    var oldBlobName = oldUri.Segments.Last();

                    await _blobService.DeleteBlobAsync(oldBlobName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Không xóa được file cũ: {ex.Message}");
                }

                string newFileUrl = await _blobService.UploadFileAsync(dto.File);

                document.FileUrl = newFileUrl;
            }

            document.Status = DocumentStatus.PENDING;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật thông tin tài liệu thành công!");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound("Không tìm thấy tài liệu!");

            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (document.UploadedBy != currentUserEmail && currentUserRole != UserRoles.ADMIN && currentUserRole != UserRoles.SUPER_ADMIN)
            {
                return StatusCode(403, "Bạn không có quyền xóa tài liệu này!");
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa tài liệu thành công khỏi hệ thống!");
        }

        [HttpGet("{id}/download-url")]
        [Authorize]
        public async Task<IActionResult> GetDownloadUrl(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound("Không tìm thấy tài liệu này trên hệ thống!");

            string secureDownloadUrl = _blobService.GenerateDownloadUrl(document.FileUrl);

            document.DownloadCount += 1;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                DocumentId = document.Id,
                Title = document.Title,
                DownloadCount = document.DownloadCount,
                SecureUrl = secureDownloadUrl
            });
        }

        [HttpPost("{id}/comments")]
        [Authorize]
        public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content)) return BadRequest("Nội dung bình luận không được để trống!");

            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound("Không tìm thấy tài liệu này!");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userFullName = User.FindFirst(ClaimTypes.Name)?.Value;

            var newComment = new Comment
            {
                Content = dto.Content,
                DocumentId = id,
                UserId = int.Parse(userIdClaim),
                UserEmail = userEmail,
                UserFullName = userFullName
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            return Ok("Đăng bình luận thành công!");
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(int id)
        {
            var documentExists = await _context.Documents.AnyAsync(d => d.Id == id);
            if (!documentExists) return NotFound("Không tìm thấy tài liệu!");

            var comments = await _context.Comments
                .Where(c => c.DocumentId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    c.UserFullName,
                    c.UserEmail
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost("{id}/rate")]
        [Authorize]
        public async Task<IActionResult> RateDocument(int id, [FromBody] CreateRatingDto dto)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound("Không tìm thấy tài liệu này!");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.Parse(userIdClaim);

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.DocumentId == id && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Score = dto.Score;
            }
            else
            {
                var newRating = new Rating
                {
                    DocumentId = id,
                    UserId = userId,
                    Score = dto.Score
                };
                _context.Ratings.Add(newRating);
            }
            await _context.SaveChangesAsync();

            var allRatings = await _context.Ratings.Where(r => r.DocumentId == id).ToListAsync();
            double avgScore = allRatings.Average(r => r.Score);

            document.AverageRating = Math.Round(avgScore, 1);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Cảm ơn bạn đã đánh giá tài liệu!",
                NewAverageRating = document.AverageRating
            });
        }
    }
}