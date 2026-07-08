namespace SOICT.DocumentSystem.API.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int Score { get; set; } // Số sao: 1, 2, 3, 4, 5

        public int DocumentId { get; set; }
        public Document Document { get; set; }

        public int UserId { get; set; }
    }
}
