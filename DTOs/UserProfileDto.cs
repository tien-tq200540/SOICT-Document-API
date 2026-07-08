namespace SOICT.DocumentSystem.API.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string? StudentCohort { get; set; }
        public string? School { get; set; }
    }
}
