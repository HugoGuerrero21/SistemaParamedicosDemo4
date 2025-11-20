using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParamedicos.API.Data;
using SistemaParamedicos.API.Models;
using SistemaParamedicos.API.DTOs; // ⭐ NUEVO

namespace SistemaParamedicos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmpleadosController> _logger;

        public EmpleadosController(
            ApplicationDbContext context,
            ILogger<EmpleadosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok("Endpoint de empleados funcionando correctamente");
        }

        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<EmpleadoDTO>>> GetEmpleadosActivos()
        {
            try
            {
                _logger.LogInformation("Obteniendo empleados activos...");

                var empleados = await _context.Empleados
                    .Include(e => e.Puesto)
                    .Where(e => e.Estado == "ALTA")
                    .OrderBy(e => e.Nombre)
                    .Select(e => new EmpleadoDTO
                    {
                        IdEmpleado = e.IdEmpleado,
                        Rfid = e.Rfid,
                        Nombre = e.Nombre,
                        Sexo = e.Sexo,
                        Telefono = e.Telefono,
                        Alergias = e.Alergias,
                        TipoSangre = e.TipoSangre,
                        IdPuesto = e.IdPuesto,
                        IdDepartamento = e.IdDepartamento,
                        IdArea = e.IdArea,
                        Nacimiento = e.Nacimiento,
                        Foto = e.Foto,
                        Estado = e.Estado,
                        Puesto = e.Puesto != null ? new PuestoDTO
                        {
                            IdPuesto = e.Puesto.IdPuesto,
                            IdDepartamento = e.Puesto.IdDepartamento,
                            Nombre = e.Puesto.Nombre,
                            Fecha = e.Puesto.Fecha
                        } : null
                    })
                    .ToListAsync();

                _logger.LogInformation($"{empleados.Count} empleados activos encontrados");
                return Ok(empleados);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmpleadoDTO>> GetEmpleadoById(string id)
        {
            try
            {
                var empleado = await _context.Empleados
                    .Include(e => e.Puesto)
                    .Where(e => e.IdEmpleado == id)
                    .Select(e => new EmpleadoDTO
                    {
                        IdEmpleado = e.IdEmpleado,
                        Rfid = e.Rfid,
                        Nombre = e.Nombre,
                        Sexo = e.Sexo,
                        Telefono = e.Telefono,
                        Alergias = e.Alergias,
                        TipoSangre = e.TipoSangre,
                        IdPuesto = e.IdPuesto,
                        IdDepartamento = e.IdDepartamento,
                        IdArea = e.IdArea,
                        Nacimiento = e.Nacimiento,
                        Foto = e.Foto,
                        Estado = e.Estado,
                        Puesto = e.Puesto != null ? new PuestoDTO
                        {
                            IdPuesto = e.Puesto.IdPuesto,
                            IdDepartamento = e.Puesto.IdDepartamento,
                            Nombre = e.Puesto.Nombre,
                            Fecha = e.Puesto.Fecha
                        } : null
                    })
                    .FirstOrDefaultAsync();

                if (empleado == null)
                    return NotFound(new { message = $"Empleado {id} no encontrado" });

                return Ok(empleado);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<EmpleadoDTO>>> BuscarEmpleados([FromQuery] string texto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(texto))
                    return await GetEmpleadosActivos();

                var empleados = await _context.Empleados
                    .Include(e => e.Puesto)
                    .Where(e => e.Estado == "ALTA" &&
                           (e.Nombre.Contains(texto) ||
                            e.IdEmpleado.Contains(texto) ||
                            (e.Puesto != null && e.Puesto.Nombre.Contains(texto))))
                    .OrderBy(e => e.Nombre)
                    .Select(e => new EmpleadoDTO
                    {
                        IdEmpleado = e.IdEmpleado,
                        Rfid = e.Rfid,
                        Nombre = e.Nombre,
                        Sexo = e.Sexo,
                        Telefono = e.Telefono,
                        Alergias = e.Alergias,
                        TipoSangre = e.TipoSangre,
                        IdPuesto = e.IdPuesto,
                        IdDepartamento = e.IdDepartamento,
                        IdArea = e.IdArea,
                        Nacimiento = e.Nacimiento,
                        Foto = e.Foto,
                        Estado = e.Estado,
                        Puesto = e.Puesto != null ? new PuestoDTO
                        {
                            IdPuesto = e.Puesto.IdPuesto,
                            IdDepartamento = e.Puesto.IdDepartamento,
                            Nombre = e.Puesto.Nombre,
                            Fecha = e.Puesto.Fecha
                        } : null
                    })
                    .ToListAsync();

                return Ok(empleados);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}