using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SistemaParamedicosDemo4.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://localhost:7285/api";

        public AuthApiService()
        {
            // Configurar HttpClient para aceptar certificados SSL en desarrollo
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<LoginResponse> LoginAsync(string usuario, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    Usuario = usuario,
                    Password = password
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{BASE_URL}/auth/login",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"Error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }
    }

    // DTOs
    public class LoginRequest
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UsuarioData Usuario { get; set; }
    }

    public class UsuarioData
    {
        public string IdUsuarioAcc { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public string Area { get; set; }
        public string Puesto { get; set; }
        public string Departamento { get; set; }
        public string AlmacenAsignado { get; set; }
    }
}