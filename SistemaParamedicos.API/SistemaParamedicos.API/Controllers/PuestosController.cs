using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParamedicos.API.Data;
using SistemaParamedicos.API.Models;
using SistemaParamedicos.API.DTOs; // ⭐ NUEVO

namespace SistemaParamedicos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PuestosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PuestosController> _logger;

        public PuestosController(
            ApplicationDbContext context,
            ILogger<PuestosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PuestoDTO>>> GetPuestos()
        {
            try
            {
                var puestos = await _context.Puestos
                    .OrderBy(p => p.Nombre)
                    .Select(p => new PuestoDTO
                    {
                        IdPuesto = p.IdPuesto,
                        IdDepartamento = p.IdDepartamento,
                        Nombre = p.Nombre,
                        Fecha = p.Fecha
                    })
                    .ToListAsync();

                return Ok(puestos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener puestos: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PuestoDTO>> GetPuestoById(string id)
        {
            try
            {
                var puesto = await _context.Puestos
                    .Where(p => p.IdPuesto == id)
                    .Select(p => new PuestoDTO
                    {
                        IdPuesto = p.IdPuesto,
                        IdDepartamento = p.IdDepartamento,
                        Nombre = p.Nombre,
                        Fecha = p.Fecha
                    })
                    .FirstOrDefaultAsync();

                if (puesto == null)
                    return NotFound(new { message = $"Puesto {id} no encontrado" });

                return Ok(puesto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}