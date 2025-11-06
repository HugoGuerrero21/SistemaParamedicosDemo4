using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParamedicos.API.Data;
using SistemaParamedicos.API.Models.DTOs;

namespace SistemaParamedicos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                _logger.LogInformation($"Intento de login para usuario: {request.Usuario}");

                if (string.IsNullOrWhiteSpace(request.Usuario) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new LoginResponseDTO
                    {
                        Success = false,
                        Message = "Usuario y contraseña son requeridos"
                    });
                }

                // Buscar usuario en la BD
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Usuario == request.Usuario);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado: {request.Usuario}");
                    return Unauthorized(new LoginResponseDTO
                    {
                        Success = false,
                        Message = "Usuario o contraseña incorrectos"
                    });
                }

                // Validar contraseña
                if (usuario.Password != request.Password)
                {
                    _logger.LogWarning($"Contraseña incorrecta para usuario: {request.Usuario}");
                    return Unauthorized(new LoginResponseDTO
                    {
                        Success = false,
                        Message = "Usuario o contraseña incorrectos"
                    });
                }

                _logger.LogInformation($"Login exitoso para usuario: {request.Usuario}");

                // Login exitoso
                return Ok(new LoginResponseDTO
                {
                    Success = true,
                    Message = "Login exitoso",
                    Usuario = new UsuarioData
                    {
                        IdUsuarioAcc = usuario.IdUsuarioAcc,
                        Nombre = usuario.Nombre,
                        Usuario = usuario.Usuario,
                        Area = usuario.Area,
                        Puesto = usuario.Puesto,
                        Departamento = usuario.Departamento,
                        AlmacenAsignado = usuario.AlmacenAsignado
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en login: {ex.Message}");
                return StatusCode(500, new LoginResponseDTO
                {
                    Success = false,
                    Message = $"Error en el servidor: {ex.Message}"
                });
            }
        }

        // GET: api/auth/test
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok("API funcionando correctamente");
        }
    }
}