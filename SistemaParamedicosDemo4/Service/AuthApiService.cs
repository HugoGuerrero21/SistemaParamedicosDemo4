using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SistemaParamedicosDemo4.Data.Repositories;

namespace SistemaParamedicosDemo4.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly UsuarioAccesoRepositories _usuarioRepo;

        public AuthApiService()
        {
            _httpClient = ApiConfiguration.CreateHttpClient();
            _baseUrl = ApiConfiguration.BaseUrl;
            _usuarioRepo = new UsuarioAccesoRepositories();

            System.Diagnostics.Debug.WriteLine($"✓ AuthApiService inicializado");
            System.Diagnostics.Debug.WriteLine($"📡 URL: {_baseUrl}");
        }

        public async Task<LoginResponse> LoginAsync(string usuario, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔐 Intentando login para: {usuario}");

                // ⭐ 1. PRIMERO INTENTAR CON LA API
                var loginOnline = await LoginOnlineAsync(usuario, password);

                if (loginOnline.Success)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Login exitoso con API");
                    return loginOnline;
                }

                // ⭐ 2. SI FALLA LA API, INTENTAR CON SQLITE LOCAL
                System.Diagnostics.Debug.WriteLine("⚠️ API no disponible, intentando login local...");
                return LoginOffline(usuario, password);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en login: {ex.Message}");

                // ⭐ SI HAY ERROR, USAR SQLITE
                return LoginOffline(usuario, password);
            }
        }

        private async Task<LoginResponse> LoginOnlineAsync(string usuario, string password)
        {
            try
            {
                var url = $"{_baseUrl}/auth/login";
                System.Diagnostics.Debug.WriteLine($"📡 Conectando a: {url}");

                var request = new LoginRequest
                {
                    Usuario = usuario,
                    Password = password
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                System.Diagnostics.Debug.WriteLine($"📡 Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    // ⭐ GUARDAR USUARIO EN SQLITE PARA USO OFFLINE
                    if (result?.Success == true && result.Usuario != null)
                    {
                        GuardarUsuarioLocal(result.Usuario, password);
                    }

                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error API: {errorContent}");

                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
                throw; // Lanzar para que intente offline
            }
        }

        private LoginResponse LoginOffline(string usuario, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("💾 Intentando login con SQLite local...");

                var usuarioLocal = _usuarioRepo.GetUsuarioByNombreUsuario(usuario);

                if (usuarioLocal == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Usuario no encontrado (sin conexión)"
                    };
                }

                // Validar contraseña
                if (usuarioLocal.Password == password)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Login exitoso con SQLite local");

                    return new LoginResponse
                    {
                        Success = true,
                        Message = "Login exitoso (modo offline)",
                        Usuario = new UsuarioData
                        {
                            IdUsuarioAcc = usuarioLocal.IdUsuario,
                            Nombre = usuarioLocal.Nombre,
                            Usuario = usuarioLocal.Usuario,

                        }
                    };
                }
                else
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Contraseña incorrecta"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en login offline: {ex.Message}");

                return new LoginResponse
                {
                    Success = false,
                    Message = $"Error en modo offline: {ex.Message}"
                };
            }
        }

        private void GuardarUsuarioLocal(UsuarioData usuario, string password)
        {
            try
            {
                var usuarioExistente = _usuarioRepo.GetUsuarioByNombreUsuario(usuario.Usuario);

                var usuarioModel = new MVVM.Models.UsuariosAccesoModel
                {
                    IdUsuario = usuario.IdUsuarioAcc,
                    Nombre = usuario.Nombre,
                    Usuario = usuario.Usuario,
                    Password = password, 

                };

                if (usuarioExistente != null)
                {
                    _usuarioRepo.ActualizarUsuario(usuarioModel);
                    System.Diagnostics.Debug.WriteLine("✓ Usuario actualizado en SQLite");
                }
                else
                {
                    _usuarioRepo.InsertarUsuario(usuarioModel);
                    System.Diagnostics.Debug.WriteLine("✓ Usuario guardado en SQLite");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ No se pudo guardar usuario local: {ex.Message}");
            }
        }

        // ⭐ MÉTODO PARA VERIFICAR SI HAY CONEXIÓN CON LA API
        public async Task<bool> VerificarConexionAsync()
        {
            try
            {
                var url = $"{_baseUrl}/auth/test";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
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