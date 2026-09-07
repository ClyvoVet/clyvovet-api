using System.ComponentModel.DataAnnotations.Schema;

namespace ClyvoVet.API.Domain.Entities
{
    [Table("Pets")]
    public class Pet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public double Weight { get; set; }
        public string Color { get; set; } = string.Empty;
        public DateTime NextCheckup { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }
    }
}
