using System.Text.Json.Serialization;

namespace ToDoApp.Application.DTOs.Auth
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "bearer";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("jti")]
        public string Jti { get; set; } = string.Empty;
    }
}
