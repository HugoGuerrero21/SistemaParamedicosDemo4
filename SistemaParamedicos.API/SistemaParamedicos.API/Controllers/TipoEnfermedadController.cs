using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParamedicos.API.Data;
using SistemaParamedicos.API.Models;
using SistemaParamedicos.API.DTOs;

namespace SistemaParamedicos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoEnfermedadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TipoEnfermedadController> _logger;

        public TipoEnfermedadController(
            ApplicationDbContext context,
            ILogger<TipoEnfermedadController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok("Endpoint de tipos de enfermedad funcionando correctamente");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoEnfermedadDTO>>> GetTiposEnfermedad()
        {
            try
            {
                _logger.LogInformation("Obteniendo tipos de enfermedad...");

                var tipos = await _context.TiposEnfermedad
                    .OrderBy(t => t.NombreEnfermedad)
                    .Select(t => new TipoEnfermedadDTO
                    {
                        IdTipoEnfermedad = t.IdTipoEnfermedad,
                        NombreEnfermedad = t.NombreEnfermedad,
                        IdUsuarioAcc = t.IdUsuarioAcc
                    })
                    .ToListAsync();


                _logger.LogInformation($"{tipos.Count} tipos de enfermedad encontrados");
                return Ok(tipos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoEnfermedadDTO>> GetTipoEnfermedadById(int id)
        {
            try
            {
                var tipo = await _context.TiposEnfermedad
                    .Where(t => t.IdTipoEnfermedad == id)
                    .Select(t => new TipoEnfermedadDTO
                    {
                        IdTipoEnfermedad = t.IdTipoEnfermedad,
                        NombreEnfermedad = t.NombreEnfermedad,
                        IdUsuarioAcc = t.IdUsuarioAcc
                    })
                    .FirstOrDefaultAsync();

                if (tipo == null)
                    return NotFound(new { message = $"Tipo de enfermedad {id} no encontrado" });

                return Ok(tipo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<TipoEnfermedadDTO>> CrearTipoEnfermedad(
        [FromBody] CrearTipoEnfermedadDTO dto)
        {
            try
            {
                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(dto.NombreEnfermedad))
                {
                    return BadRequest(new { message = "El nombre de la enfermedad es requerido" });
                }

                // Validar que no exista un tipo con el mismo nombre
                var existe = await _context.TiposEnfermedad
                    .AnyAsync(t => t.NombreEnfermedad.ToLower() == dto.NombreEnfermedad.ToLower());

                if (existe)
                {
                    return Conflict(new { message = "Ya existe un tipo de enfermedad con ese nombre" });
                }

                // Crear el nuevo tipo
                var nuevoTipo = new TipoEnfermedadModel
                {
                    NombreEnfermedad = dto.NombreEnfermedad.Trim(),
                    IdUsuarioAcc = dto.IdUsuarioAcc
                };

                _context.TiposEnfermedad.Add(nuevoTipo);
                await _context.SaveChangesAsync();

                var resultado = new TipoEnfermedadDTO
                {
                    IdTipoEnfermedad = nuevoTipo.IdTipoEnfermedad,
                    NombreEnfermedad = nuevoTipo.NombreEnfermedad,
                    IdUsuarioAcc = nuevoTipo.IdUsuarioAcc
                };

                _logger.LogInformation($"Tipo de enfermedad creado: {nuevoTipo.NombreEnfermedad}");

                return CreatedAtAction(
                    nameof(GetTipoEnfermedadById),
                    new { id = nuevoTipo.IdTipoEnfermedad },
                    resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("verificar/{nombre}")]
        public async Task<ActionResult<bool>> VerificarNombreExistente(string nombre)
        {
            try
            {
                var existe = await _context.TiposEnfermedad
                    .AnyAsync(t => t.NombreEnfermedad.ToLower() == nombre.ToLower());

                return Ok(new { existe });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
