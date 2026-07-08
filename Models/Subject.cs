using System.Reflection.Metadata;

namespace SOICT.DocumentSystem.API.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string SubjectCode { get; set; }
        public string Name { get; set; }

        public List<Document> Documents { get; set; } = new List<Document>();
    }
}
