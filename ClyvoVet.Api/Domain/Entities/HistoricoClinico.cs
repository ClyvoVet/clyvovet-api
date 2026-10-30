using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClyvoVet.API.Domain.Entities
{
    [Table("historico_clinico")]
    public class HistoricoClinico
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int IdPet { get; set; }
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public ICollection<string> Tags { get; set; } = [];
    }
}
