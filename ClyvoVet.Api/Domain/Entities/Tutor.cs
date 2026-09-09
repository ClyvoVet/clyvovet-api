using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClyvoVet.API.Domain.Entities
{
    [Table("TUTOR")]
    public class Tutor
    {
        [Key]
        [Column("ID_TUTOR")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdTutor { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        [Column("NOME")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
        [StringLength(100)]
        [Column("EMAIL")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("TELEFONE")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [StringLength(14)]
        [Column("CPF")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
        [Column("SENHA")]
        [JsonIgnore]
        public string Senha { get; set; } = string.Empty;

        public ICollection<Pet>? Pets { get; set; }
    }
}
