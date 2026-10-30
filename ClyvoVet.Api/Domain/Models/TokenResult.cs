namespace ClyvoVet.API.Domain.Models
{
    public class TokenResult
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
