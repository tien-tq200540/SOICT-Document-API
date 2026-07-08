using System.ComponentModel.DataAnnotations;

namespace SOICT.DocumentSystem.API.DTOs
{
    public class CreateRatingDto
    {
        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5")]
        public int Score { get; set; }
    }
}
