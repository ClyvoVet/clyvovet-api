using System.ComponentModel.DataAnnotations;

namespace ClyvoVet.API.Application.Dtos
{
    public class PetRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Species { get; set; } = string.Empty;

        [Required]
        public string Breed { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public double Weight { get; set; }

        [Required]
        public string Color { get; set; } = string.Empty;

        public DateTime NextCheckup { get; set; }

        [Required]
        public Guid OwnerId { get; set; }
    }
}
