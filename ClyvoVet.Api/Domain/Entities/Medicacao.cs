using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClyvoVet.API.Domain.Entities
{
    [Table("MEDICACAO")]
    public class Medicacao
    {
        [Key]
        [Column("ID_MEDICACAO")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdMedicacao { get; set; }

        [Required]
        [Column("ID_PET")]
        public int IdPet { get; set; }

        [Required(ErrorMessage = "O nome da medicação é obrigatório.")]
        [StringLength(100)]
        [Column("NOME")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("DOSE")]
        public string? Dose { get; set; }

        [StringLength(50)]
        [Column("FREQUENCIA")]
        public string? Frequencia { get; set; }

        [Column("DATA_INICIO")]
        public DateTime? DataInicio { get; set; }

        [Column("DATA_FIM")]
        public DateTime? DataFim { get; set; }

        public Pet? Pet { get; set; }
    }
}
