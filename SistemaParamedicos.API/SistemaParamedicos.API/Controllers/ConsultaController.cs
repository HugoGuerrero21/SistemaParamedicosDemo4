using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParamedicos.API.Data;
using SistemaParamedicos.API.DTOs;
using SistemaParamedicos.API.Models;

namespace SistemaParamedicos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsultasController> _logger;

        public ConsultasController(ApplicationDbContext context, ILogger<ConsultasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Consultas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsultaResumenDto>>> GetConsultas()
        {
            try
            {
                var consultas = await _context.Consultas
                    .Include(c => c.Empleado)
                    .Include(c => c.TipoEnfermedad)
                    .Include(c => c.Usuario)
                    .OrderByDescending(c => c.FechaConsulta)
                    .ToListAsync();

                // ⭐ CARGAR MEDICAMENTOS MANUALMENTE
                var consultasDto = new List<ConsultaResumenDto>();

                foreach (var c in consultas)
                {
                    // Obtener medicamentos si hay movimiento
                    List<MedicamentoConsultaDto> medicamentos = null;

                    if (!string.IsNullOrEmpty(c.IdMovimiento))
                    {
                        medicamentos = await _context.MovimientosDetalle
                            .Include(d => d.Producto)
                            .Where(d => d.IdMovimiento == c.IdMovimiento && d.Status == 1)
                            .Select(d => new MedicamentoConsultaDto
                            {
                                IdProducto = d.IdProducto,
                                Cantidad = d.Cantidad,
                                Observaciones = d.Producto != null ? d.Producto.Nombre : null
                            })
                            .ToListAsync();
                    }

                    consultasDto.Add(new ConsultaResumenDto
                    {
                        IdConsulta = c.IdConsulta,
                        IdEmpleado = c.IdEmpleado,
                        NombreEmpleado = c.Empleado?.Nombre,
                        IdUsuarioAcc = c.IdUsuarioAcc,
                        NombreUsuario = c.Usuario?.Usuario,
                        IdTipoEnfermedad = c.IdTipoEnfermedad,
                        NombreTipoEnfermedad = c.TipoEnfermedad?.NombreEnfermedad,
                        TipoEnfermedad = c.TipoEnfermedad?.NombreEnfermedad,
                        IdMovimiento = c.IdMovimiento,
                        MotivoConsulta = c.MotivoConsulta,
                        Diagnostico = c.Diagnostico,
                        FechaConsulta = c.FechaConsulta,
                        FrecuenciaRespiratoria = c.FrecuenciaRespiratoria,
                        FrecuenciaCardiaca = c.FrecuenciaCardiaca,
                        Temperatura = c.Temperatura,
                        PresionArterial = c.PresionArterial,
                        Observaciones = c.Observaciones,
                        UltimaComida = c.UltimaComida,
                        Medicamentos = medicamentos // ⭐ AGREGAR MEDICAMENTOS
                    });
                }

                _logger.LogInformation($"✓ {consultasDto.Count} consultas recuperadas con medicamentos");
                return Ok(consultasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener consultas: {ex.Message}");
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // GET: api/Consultas/empleado/{idEmpleado}
        [HttpGet("empleado/{idEmpleado}")]
        public async Task<ActionResult<IEnumerable<ConsultaResumenDto>>> GetConsultasPorEmpleado(string idEmpleado)
        {
            try
            {
                var consultas = await _context.Consultas
                    .Include(c => c.Empleado)
                    .Include(c => c.TipoEnfermedad)
                    .Include(c => c.Usuario)
                    .Where(c => c.IdEmpleado == idEmpleado)
                    .OrderByDescending(c => c.FechaConsulta)
                    .ToListAsync();

                // ⭐ CARGAR MEDICAMENTOS MANUALMENTE
                var consultasDto = new List<ConsultaResumenDto>();

                foreach (var c in consultas)
                {
                    // Obtener medicamentos si hay movimiento
                    List<MedicamentoConsultaDto> medicamentos = null;

                    if (!string.IsNullOrEmpty(c.IdMovimiento))
                    {
                        medicamentos = await _context.MovimientosDetalle
                            .Include(d => d.Producto)
                            .Where(d => d.IdMovimiento == c.IdMovimiento && d.Status == 1)
                            .Select(d => new MedicamentoConsultaDto
                            {
                                IdProducto = d.IdProducto,
                                Cantidad = d.Cantidad,
                                Observaciones = d.Producto != null ? d.Producto.Nombre : null
                            })
                            .ToListAsync();
                    }

                    consultasDto.Add(new ConsultaResumenDto
                    {
                        IdConsulta = c.IdConsulta,
                        IdEmpleado = c.IdEmpleado,
                        NombreEmpleado = c.Empleado?.Nombre,
                        IdUsuarioAcc = c.IdUsuarioAcc,
                        NombreUsuario = c.Usuario?.Usuario,
                        IdTipoEnfermedad = c.IdTipoEnfermedad,
                        NombreTipoEnfermedad = c.TipoEnfermedad?.NombreEnfermedad,
                        TipoEnfermedad = c.TipoEnfermedad?.NombreEnfermedad,
                        IdMovimiento = c.IdMovimiento,
                        MotivoConsulta = c.MotivoConsulta,
                        Diagnostico = c.Diagnostico,
                        FechaConsulta = c.FechaConsulta,
                        FrecuenciaRespiratoria = c.FrecuenciaRespiratoria,
                        FrecuenciaCardiaca = c.FrecuenciaCardiaca,
                        Temperatura = c.Temperatura,
                        PresionArterial = c.PresionArterial,
                        Observaciones = c.Observaciones,
                        UltimaComida = c.UltimaComida,
                        Medicamentos = medicamentos // ⭐ AGREGAR MEDICAMENTOS
                    });
                }

                _logger.LogInformation($"✓ {consultasDto.Count} consultas encontradas para empleado {idEmpleado}");
                return Ok(consultasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener consultas del empleado {idEmpleado}: {ex.Message}");
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // GET: api/Consultas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultaDto>> GetConsulta(int id)
        {
            try
            {
                var consulta = await _context.Consultas
                    .Include(c => c.Empleado)
                    .Include(c => c.TipoEnfermedad)
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.IdConsulta == id);

                if (consulta == null)
                {
                    return NotFound(new { mensaje = $"Consulta con ID {id} no encontrada" });
                }

                // ⭐ CARGAR MEDICAMENTOS SI HAY MOVIMIENTO
                List<MedicamentoConsultaDto> medicamentos = null;

                if (!string.IsNullOrEmpty(consulta.IdMovimiento))
                {
                    medicamentos = await _context.MovimientosDetalle
                        .Include(d => d.Producto)
                        .Where(d => d.IdMovimiento == consulta.IdMovimiento && d.Status == 1)
                        .Select(d => new MedicamentoConsultaDto
                        {
                            IdProducto = d.IdProducto,
                            Cantidad = d.Cantidad,
                            Observaciones = d.Producto != null ? d.Producto.Nombre : null
                        })
                        .ToListAsync();
                }

                var consultaDto = new ConsultaDto
                {
                    IdConsulta = consulta.IdConsulta,
                    IdEmpleado = consulta.IdEmpleado,
                    NombreEmpleado = consulta.Empleado?.Nombre,
                    IdUsuarioAcc = consulta.IdUsuarioAcc,
                    NombreUsuario = consulta.Usuario?.Usuario,
                    IdTipoEnfermedad = consulta.IdTipoEnfermedad,
                    NombreTipoEnfermedad = consulta.TipoEnfermedad?.NombreEnfermedad,
                    IdMovimiento = consulta.IdMovimiento,
                    FrecuenciaRespiratoria = consulta.FrecuenciaRespiratoria,
                    FrecuenciaCardiaca = consulta.FrecuenciaCardiaca,
                    Temperatura = consulta.Temperatura,
                    PresionArterial = consulta.PresionArterial,
                    Observaciones = consulta.Observaciones,
                    UltimaComida = consulta.UltimaComida,
                    MotivoConsulta = consulta.MotivoConsulta,
                    FechaConsulta = consulta.FechaConsulta,
                    Diagnostico = consulta.Diagnostico,
                    Medicamentos = medicamentos // ⭐ AGREGAR MEDICAMENTOS
                };

                return Ok(consultaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener consulta {id}: {ex.Message}");
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // POST: api/Consultas
        [HttpPost]
        public async Task<ActionResult<ConsultaDto>> PostConsulta(CrearConsultaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validar que el empleado existe
                var empleadoExiste = await _context.Empleados.AnyAsync(e => e.IdEmpleado == dto.IdEmpleado);
                if (!empleadoExiste)
                {
                    return BadRequest(new { error = "El empleado especificado no existe" });
                }

                // 2. Validar que el tipo de enfermedad existe
                var tipoExiste = await _context.TiposEnfermedad.AnyAsync(t => t.IdTipoEnfermedad == dto.IdTipoEnfermedad);
                if (!tipoExiste)
                {
                    return BadRequest(new { error = "El tipo de enfermedad especificado no existe" });
                }

                string idMovimiento = null;

                // 3. SI HAY MEDICAMENTOS VÁLIDOS, crear el movimiento y sus detalles
                var medicamentosValidos = dto.Medicamentos?
                    .Where(m => !string.IsNullOrWhiteSpace(m.IdProducto) && m.Cantidad > 0)
                    .ToList() ?? new List<MedicamentoConsultaDto>();

                if (medicamentosValidos.Count > 0)
                {
                    _logger.LogInformation($"📦 Creando movimiento con {medicamentosValidos.Count} medicamentos");

                    // 3.1 Crear el movimiento
                    idMovimiento = Guid.NewGuid().ToString("N").Substring(0, 25).ToUpper();

                    var movimiento = new MovimientoModel
                    {
                        IdMovimiento = idMovimiento,
                        IdTipoMovimiento = "MV2",
                        IdAlmacen = "ALM6",
                        FechaMovimiento = DateTime.Now,
                        IdEmpleado = dto.IdEmpleado,
                        IdUsuario = dto.IdUsuarioAcc,
                        Status = 1,
                        EsTraspaso = 0
                    };

                    _context.Movimientos.Add(movimiento);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✓ Movimiento creado: {idMovimiento}");

                    // 3.2 Crear los detalles del movimiento
                    foreach (var medicamento in medicamentosValidos)
                    {
                        var detalle = new MovimientoDetalleModel
                        {
                            IdMovimientoDetalles = Guid.NewGuid().ToString("N").Substring(0, 25),
                            IdMovimiento = idMovimiento,
                            IdProducto = medicamento.IdProducto,
                            Cantidad = medicamento.Cantidad,
                            CantidadUtilizada = medicamento.Cantidad,
                            Status = 1
                        };

                        _context.MovimientosDetalle.Add(detalle);
                        _logger.LogInformation($"✓ Detalle agregado: {medicamento.IdProducto} x {medicamento.Cantidad}");
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✓ {medicamentosValidos.Count} detalles guardados");
                }
                else
                {
                    _logger.LogInformation("ℹ️ No se agregaron medicamentos, consulta sin movimiento");
                }

                // 4. Crear la consulta
                var consulta = new ConsultaModel
                {
                    IdEmpleado = dto.IdEmpleado,
                    IdUsuarioAcc = dto.IdUsuarioAcc,
                    IdTipoEnfermedad = dto.IdTipoEnfermedad,
                    IdMovimiento = idMovimiento,
                    FrecuenciaRespiratoria = dto.FrecuenciaRespiratoria,
                    FrecuenciaCardiaca = dto.FrecuenciaCardiaca,
                    Temperatura = dto.Temperatura,
                    PresionArterial = dto.PresionArterial,
                    Observaciones = dto.Observaciones,
                    UltimaComida = dto.UltimaComida,
                    MotivoConsulta = dto.MotivoConsulta,
                    FechaConsulta = dto.FechaConsulta,
                    Diagnostico = dto.Diagnostico
                };

                _context.Consultas.Add(consulta);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✓ Consulta creada con ID: {consulta.IdConsulta}");

                // 5. Commit de la transacción
                await transaction.CommitAsync();

                // 6. Cargar relaciones para devolver
                await _context.Entry(consulta)
                    .Reference(c => c.Empleado)
                    .LoadAsync();
                await _context.Entry(consulta)
                    .Reference(c => c.TipoEnfermedad)
                    .LoadAsync();

                var consultaDto = new ConsultaDto
                {
                    IdConsulta = consulta.IdConsulta,
                    IdEmpleado = consulta.IdEmpleado,
                    NombreEmpleado = consulta.Empleado?.Nombre,
                    IdUsuarioAcc = consulta.IdUsuarioAcc,
                    IdTipoEnfermedad = consulta.IdTipoEnfermedad,
                    NombreTipoEnfermedad = consulta.TipoEnfermedad?.NombreEnfermedad,
                    IdMovimiento = consulta.IdMovimiento,
                    FrecuenciaRespiratoria = consulta.FrecuenciaRespiratoria,
                    FrecuenciaCardiaca = consulta.FrecuenciaCardiaca,
                    Temperatura = consulta.Temperatura,
                    PresionArterial = consulta.PresionArterial,
                    Observaciones = consulta.Observaciones,
                    UltimaComida = consulta.UltimaComida,
                    MotivoConsulta = consulta.MotivoConsulta,
                    FechaConsulta = consulta.FechaConsulta,
                    Diagnostico = consulta.Diagnostico,
                    Medicamentos = dto.Medicamentos
                };

                return CreatedAtAction(nameof(GetConsulta), new { id = consulta.IdConsulta }, consultaDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"❌ Error al crear consulta: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // GET: api/Consultas/estadisticas
        // GET: api/Consultas/estadisticas
        [HttpGet("estadisticas")]
        public async Task<ActionResult<EstadisticasResponseDto>> GetEstadisticas(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                // 1️⃣ AJUSTE DE FECHAS
                // Asegurar que fechaInicio sea el comienzo del día y fechaFin el final del día
                DateTime inicio = fechaInicio.Date;
                DateTime fin = fechaFin.Date.AddDays(1).AddTicks(-1); // 23:59:59.999

                _logger.LogInformation($"📊 Obteniendo estadísticas: Desde {inicio} hasta {fin}");

                // VALIDACIÓN BÁSICA
                if (inicio > fin)
                {
                    return BadRequest(new { error = "La fecha de inicio no puede ser mayor a la fecha final." });
                }

                string periodo = $"Del {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}";

                // 2️⃣ OBTENER TODOS LOS TIPOS DE ENFERMEDAD
                var todosTiposEnfermedad = await _context.TiposEnfermedad
                    .OrderBy(t => t.NombreEnfermedad)
                    .ToListAsync();

                // 3️⃣ CONSULTAR DATOS AGRUPADOS POR RANGO DE FECHAS
                var consultasAgrupadas = await _context.Consultas
                    .Include(c => c.TipoEnfermedad)
                    .Where(c => c.FechaConsulta >= inicio && c.FechaConsulta <= fin)
                    .GroupBy(c => new
                    {
                        c.IdTipoEnfermedad,
                        c.TipoEnfermedad.NombreEnfermedad
                    })
                    .Select(g => new
                    {
                        IdTipoEnfermedad = g.Key.IdTipoEnfermedad,
                        NombreEnfermedad = g.Key.NombreEnfermedad,
                        Cantidad = g.Count()
                    })
                    .ToListAsync();

                // 4️⃣ CALCULAR TOTAL
                int totalConsultas = consultasAgrupadas.Sum(x => x.Cantidad);

                // 5️⃣ COLORES PREDEFINIDOS
                var coloresPredefinidos = new List<string>
                {
                    "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8",
                    "#F7DC6F", "#BB8FCE", "#85C1E2", "#F8B739", "#52B788",
                    "#E07A5F", "#81C3D7", "#F4A261", "#2A9D8F", "#E76F51"
                };

                // 6️⃣ CREAR ESTADÍSTICAS
                var estadisticas = new List<EstadisticaEnfermedadDto>();
                int colorIndex = 0;

                foreach (var tipo in todosTiposEnfermedad)
                {
                    var consultaTipo = consultasAgrupadas.FirstOrDefault(c => c.IdTipoEnfermedad == tipo.IdTipoEnfermedad);
                    int cantidad = consultaTipo?.Cantidad ?? 0;

                    decimal porcentaje = totalConsultas > 0
                        ? Math.Round((decimal)cantidad / totalConsultas * 100, 2)
                        : 0;

                    estadisticas.Add(new EstadisticaEnfermedadDto
                    {
                        IdTipoEnfermedad = tipo.IdTipoEnfermedad,
                        NombreEnfermedad = tipo.NombreEnfermedad,
                        Cantidad = cantidad,
                        Porcentaje = porcentaje,
                        Color = coloresPredefinidos[colorIndex % coloresPredefinidos.Count]
                    });

                    colorIndex++;
                }

                // Ordenar: Mayor a menor cantidad
                estadisticas = estadisticas
                    .OrderByDescending(e => e.Cantidad)
                    .ThenBy(e => e.NombreEnfermedad)
                    .ToList();

                // 7️⃣ CALCULAR DATOS ADICIONALES (PROMEDIO CORREGIDO)
                var masComun = estadisticas.FirstOrDefault(e => e.Cantidad > 0);

                // Calcular diferencia de días exactos (mínimo 1 día para evitar división por cero)
                double totalDias = (fin - inicio).TotalDays;
                if (totalDias < 1) totalDias = 1;

                // Redondear días hacia arriba (ej: si seleccionas hoy a las 8am hasta hoy a las 8pm, cuenta como 1 día)
                int diasEnPeriodo = (int)Math.Ceiling(totalDias);

                decimal promedioDiario = diasEnPeriodo > 0
                    ? Math.Round((decimal)totalConsultas / diasEnPeriodo, 2)
                    : 0;

                // 8️⃣ CONSTRUIR RESPUESTA
                var response = new EstadisticasResponseDto
                {
                    TotalConsultas = totalConsultas,
                    Periodo = periodo,
                    FechaInicio = inicio,
                    FechaFin = fin,
                    Estadisticas = estadisticas,
                    EnfermedadMasComun = masComun?.NombreEnfermedad ?? "N/A",
                    CantidadMasComun = masComun?.Cantidad ?? 0,
                    PromedioDiario = promedioDiario
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al generar estadísticas: {ex.Message}");
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // GET: api/Consultas/test
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { mensaje = "API de Consultas funcionando correctamente", fecha = DateTime.Now });
        }
    }
}