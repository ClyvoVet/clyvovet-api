using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClyvoVet.API.Domain.Entities
{
    [Table("PET")]
    public class Pet
    {
        [Key]
        [Column("ID_PET")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdPet { get; set; }

        [Required]
        [Column("ID_TUTOR")]
        public int IdTutor { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        [Column("NOME")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A espécie é obrigatória.")]
        [StringLength(50)]
        [Column("ESPECIE")]
        public string Especie { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("RACA")]
        public string? Raca { get; set; }

        [Column("DATA_NASC")]
        public DateTime? DataNascimento { get; set; }

        [Column("PESO_KG", TypeName = "NUMBER(5,2)")]
        public decimal? PesoKg { get; set; }

        public Tutor? Tutor { get; set; }
        [JsonIgnore]
        public ICollection<Consulta>? Consultas { get; set; }

        [JsonIgnore]
        public ICollection<Medicacao>? Medicacoes { get; set; }
    }
}
