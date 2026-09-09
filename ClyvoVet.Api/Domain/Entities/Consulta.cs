using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClyvoVet.API.Domain.Entities
{
    [Table("CONSULTA")]
    public class Consulta
    {
        [Key]
        [Column("ID_CONSULTA")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdConsulta { get; set; }

        [Required]
        [Column("ID_PET")]
        public int IdPet { get; set; }

        [Required]
        [Column("DATA_CONSULTA")]
        public DateTime DataConsulta { get; set; }

        [StringLength(100)]
        [Column("VETERINARIO")]
        public string? Veterinario { get; set; }

        [StringLength(300)]
        [Column("OBSERVACOES")]
        public string? Observacoes { get; set; }

        public Pet? Pet { get; set; }
    }
}
