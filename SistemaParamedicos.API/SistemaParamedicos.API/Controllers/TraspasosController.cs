using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParamedicos.API.Data;
using SistemaParamedicos.API.DTOs;
using SistemaParamedicos.API.Models;

namespace SistemaParamedicos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraspasosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TraspasosController> _logger;

        public TraspasosController(ApplicationDbContext context, ILogger<TraspasosController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // GET: api/Traspasos/pendientes
        [HttpGet("pendientes")]
        public async Task<ActionResult<IEnumerable<TraspasoPendienteDto>>> GetTraspasosPendientes()
        {
            try
            {
                const string almacenParamedicos = "ALM6";

                var traspasos = await _context.Traspasos
                    .Include(t => t.Empleado)
                    .Include(t => t.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(t => t.AlmacenDestino == almacenParamedicos && t.Status == 0)
                    .OrderByDescending(t => t.FechaEnvio)
                    .Select(t => new TraspasoPendienteDto
                    {
                        IdTraspaso = t.IdTraspaso,
                        AlmacenOrigen = t.AlmacenOrigen,
                        NombreAlmacenOrigen = "Almacén Hermosillo",
                        AlmacenDestino = t.AlmacenDestino,
                        NombreAlmacenDestino = "Almacén Paramédicos",
                        FechaEnvio = t.FechaEnvio,
                        NombreChofer = t.Empleado != null ? t.Empleado.Nombre : null,
                        NoEconomico = t.IdUnidad,
                        Status = t.Status,
                        StatusTexto = "Pendiente",
                        Detalles = t.Detalles.Select(d => new TraspasoDetalleDTO
                        {
                            IdTraspasoDetalle = d.IdTraspasoDetalle,
                            IdProducto = d.IdProducto,
                            NombreProducto = d.Producto != null ? d.Producto.Nombre : "",
                            Descripcion = d.Producto != null ? d.Producto.Descripcion : "",
                            NumeroPieza = d.Producto != null ? d.Producto.NumeroPieza : "",
                            Cantidad = d.Cantidad,
                            CantidadRecibida = d.CantidadRecibida ?? 0,
                            CantidadFaltante = d.Cantidad - (d.CantidadRecibida ?? 0),
                            CantidadARecibir = 0,
                            Completada = d.Completada,
                            FechaCompletado = d.FechaCompletado,
                            IdProveedor = d.IdProveedor
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation($"✓ {traspasos.Count} traspasos pendientes recuperados");
                return Ok(traspasos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener traspasos pendientes: {ex.Message}");
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // GET: api/Traspasos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TraspasoPendienteDto>> GetTraspaso(string id)
        {
            try
            {
                var traspaso = await _context.Traspasos
                    .Include(t => t.Empleado)
                    .Include(t => t.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(t => t.IdTraspaso == id)
                    .Select(t => new TraspasoPendienteDto
                    {
                        IdTraspaso = t.IdTraspaso,
                        AlmacenOrigen = t.AlmacenOrigen,
                        NombreAlmacenOrigen = "Almacén Hermosillo",
                        AlmacenDestino = t.AlmacenDestino,
                        NombreAlmacenDestino = "Almacén Paramédicos",
                        FechaEnvio = t.FechaEnvio,
                        NombreChofer = t.Empleado != null ? t.Empleado.Nombre : null,
                        NoEconomico = t.IdUnidad,
                        Status = t.Status,
                        StatusTexto = t.Status == 0 ? "Pendiente" : t.Status == 1 ? "Completado" : "Cancelado",
                        Detalles = t.Detalles.Select(d => new TraspasoDetalleDTO
                        {
                            IdTraspasoDetalle = d.IdTraspasoDetalle,
                            IdProducto = d.IdProducto,
                            NombreProducto = d.Producto != null ? d.Producto.Nombre : "",
                            Descripcion = d.Producto != null ? d.Producto.Descripcion : "",
                            NumeroPieza = d.Producto != null ? d.Producto.NumeroPieza : "",
                            Cantidad = d.Cantidad,
                            CantidadRecibida = d.CantidadRecibida ?? 0,
                            CantidadFaltante = d.Cantidad - (d.CantidadRecibida ?? 0),
                            CantidadARecibir = 0,
                            Completada = d.Completada,
                            FechaCompletado = d.FechaCompletado,
                            IdProveedor = d.IdProveedor
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (traspaso == null)
                {
                    return NotFound(new { mensaje = $"Traspaso con ID {id} no encontrado" });
                }

                return Ok(traspaso);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener traspaso {id}: {ex.Message}");
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // POST: api/Traspasos/completar-detalle
        [HttpPost("completar-detalle")]
        public async Task<ActionResult<TraspasoResultadoDto>> CompletarDetalle(CompletarDetalleDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation($"📦 Iniciando recepción de detalle {dto.IdTraspasoDetalle}");

                var detalle = await _context.TraspasosDetalle
                    .Include(d => d.Traspaso)
                    .Include(d => d.Producto)
                    .FirstOrDefaultAsync(d => d.IdTraspasoDetalle == dto.IdTraspasoDetalle && d.Completada == 0);

                if (detalle == null)
                {
                    return BadRequest(new TraspasoResultadoDto
                    {
                        Exito = false,
                        Mensaje = "El detalle ya está completado, cancelado o no existe."
                    });
                }

                if (dto.CantidadRecibida <= 0)
                {
                    return BadRequest(new TraspasoResultadoDto
                    {
                        Exito = false,
                        Mensaje = "La cantidad recibida debe ser mayor a 0."
                    });
                }

                decimal nuevaCantidadRecibida = (detalle.CantidadRecibida ?? 0) + (decimal)dto.CantidadRecibida;
                bool detalleCompletado = nuevaCantidadRecibida >= detalle.Cantidad;

                decimal? precioOriginal = null;
                if (!string.IsNullOrEmpty(detalle.SalidaPadre))
                {
                    var salidaPadre = await _context.MovimientosDetalle
                        .Where(md => md.IdMovimientoDetalles == detalle.SalidaPadre)
                        .Select(md => md.PrecioFinal)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(salidaPadre) && decimal.TryParse(salidaPadre, out decimal precio))
                    {
                        precioOriginal = precio;
                    }
                }

                string idMovimiento = Guid.NewGuid().ToString("N").Substring(0, 25).ToUpper();

                var movimiento = new MovimientoModel
                {
                    IdMovimiento = idMovimiento,
                    IdTipoMovimiento = "MV1",
                    IdAlmacen = detalle.Traspaso.AlmacenDestino,
                    FechaMovimiento = DateTime.Now,
                    IdEmpleado = detalle.Traspaso.IdEmpleado,
                    Status = 1,
                    EsTraspaso = 1,
                    IdUsuario = dto.IdUsuarioReceptor
                };

                _context.Movimientos.Add(movimiento);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✓ Movimiento de entrada creado: {idMovimiento}");

                string idMovimientoDetalle = Guid.NewGuid().ToString("N").Substring(0, 25).ToUpper();

                var movimientoDetalle = new MovimientoDetalleModel
                {
                    IdMovimientoDetalles = idMovimientoDetalle,
                    IdMovimiento = idMovimiento,
                    IdProducto = detalle.IdProducto,
                    Cantidad = (float)dto.CantidadRecibida,
                    PrecioFinal = precioOriginal?.ToString(),
                    IdProveedor = detalle.IdProveedor,
                    Status = 1,
                    CantidadUtilizada = 0
                };

                _context.MovimientosDetalle.Add(movimientoDetalle);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✓ Detalle de movimiento creado: {idMovimientoDetalle}");

                detalle.CantidadRecibida = nuevaCantidadRecibida;
                detalle.Completada = (byte)(detalleCompletado ? 1 : 0);

                if (string.IsNullOrEmpty(detalle.EntradaHija))
                {
                    detalle.EntradaHija = idMovimientoDetalle;
                }
                else
                {
                    detalle.EntradaHija += $",{idMovimientoDetalle}";
                }

                if (detalleCompletado)
                {
                    detalle.FechaCompletado = DateTime.Now;
                }

                _context.TraspasosDetalle.Update(detalle);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✓ Detalle del traspaso actualizado");

                var traspaso = detalle.Traspaso;
                if (traspaso.FechaRecepcion == null)
                {
                    traspaso.FechaRecepcion = DateTime.Now;
                    traspaso.IdUsuarioD = dto.IdUsuarioReceptor;
                    _context.Traspasos.Update(traspaso);
                    await _context.SaveChangesAsync();
                }

                var quedanDetallesPendientes = await _context.TraspasosDetalle
                    .AnyAsync(d => d.IdTraspaso == detalle.IdTraspaso && d.Completada == 0);

                if (!quedanDetallesPendientes)
                {
                    traspaso.Status = 1;
                    traspaso.FechaCompletado = DateTime.Now;
                    _context.Traspasos.Update(traspaso);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Traspaso {detalle.IdTraspaso} completado totalmente");
                }

                await transaction.CommitAsync();

                decimal cantidadRestante = detalle.Cantidad - nuevaCantidadRecibida;
                string mensaje = detalleCompletado
                    ? $"Detalle completado exitosamente. Total recibido: {nuevaCantidadRecibida} de {detalle.Cantidad}"
                    : $"Recepción parcial registrada: {dto.CantidadRecibida} unidades. " +
                      $"Total recibido: {nuevaCantidadRecibida}/{detalle.Cantidad}. " +
                      $"Pendiente: {cantidadRestante}";

                return Ok(new TraspasoResultadoDto
                {
                    Exito = true,
                    Mensaje = mensaje,
                    IdMovimiento = idMovimiento,
                    DetallesEntrada = new Dictionary<string, string>
                    {
                        { detalle.IdProducto, idMovimientoDetalle }
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"❌ Error al completar detalle: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new TraspasoResultadoDto
                {
                    Exito = false,
                    Mensaje = $"Error inesperado: {ex.Message}"
                });
            }
        }

        // POST: api/Traspasos/completar-traspaso
        [HttpPost("completar-traspaso")]
        public async Task<ActionResult<TraspasoResultadoDto>> CompletarTraspaso(CompletarTraspasoDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation($"📦 Iniciando recepción completa de traspaso {dto.IdTraspaso}");

                var traspaso = await _context.Traspasos
                    .Include(t => t.Detalles)
                    .FirstOrDefaultAsync(t => t.IdTraspaso == dto.IdTraspaso && t.Status == 0);

                if (traspaso == null)
                {
                    return BadRequest(new TraspasoResultadoDto
                    {
                        Exito = false,
                        Mensaje = "El traspaso no existe o ya fue completado/cancelado."
                    });
                }

                int detallesCompletados = 0;
                var detallesEntrada = new Dictionary<string, string>();

                foreach (var detalleRecepcion in dto.Detalles)
                {
                    var detalle = traspaso.Detalles
                        .FirstOrDefault(d => d.IdTraspasoDetalle == detalleRecepcion.IdTraspasoDetalle && d.Completada == 0);

                    if (detalle == null)
                    {
                        _logger.LogWarning($"⚠️ Detalle {detalleRecepcion.IdTraspasoDetalle} ya completado o no encontrado");
                        continue;
                    }

                    var resultadoDetalle = await CompletarDetalleInterno(
                        detalle,
                        (decimal)detalleRecepcion.CantidadRecibida,
                        dto.IdUsuarioReceptor,
                        traspaso.AlmacenDestino);

                    if (resultadoDetalle.Exito)
                    {
                        detallesCompletados++;
                        if (resultadoDetalle.DetallesEntrada != null)
                        {
                            foreach (var kvp in resultadoDetalle.DetallesEntrada)
                            {
                                detallesEntrada[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }

                if (traspaso.FechaRecepcion == null)
                {
                    traspaso.FechaRecepcion = DateTime.Now;
                    traspaso.IdUsuarioD = dto.IdUsuarioReceptor;
                }

                var quedanPendientes = await _context.TraspasosDetalle
                    .AnyAsync(d => d.IdTraspaso == dto.IdTraspaso && d.Completada == 0);

                if (!quedanPendientes)
                {
                    traspaso.Status = 1;
                    traspaso.FechaCompletado = DateTime.Now;
                }

                _context.Traspasos.Update(traspaso);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new TraspasoResultadoDto
                {
                    Exito = true,
                    Mensaje = $"Se completaron {detallesCompletados} detalles del traspaso exitosamente.",
                    DetallesEntrada = detallesEntrada
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"❌ Error al completar traspaso: {ex.Message}");
                return StatusCode(500, new TraspasoResultadoDto
                {
                    Exito = false,
                    Mensaje = $"Error inesperado: {ex.Message}"
                });
            }
        }

        private async Task<TraspasoResultadoDto> CompletarDetalleInterno(
            MovimientoTraspasoDetalle detalle,
            decimal cantidadRecibida,
            string idUsuarioReceptor,
            string almacenDestino)
        {
            try
            {
                decimal nuevaCantidadRecibida = (detalle.CantidadRecibida ?? 0) + cantidadRecibida;
                bool detalleCompletado = nuevaCantidadRecibida >= detalle.Cantidad;

                decimal? precioOriginal = null;
                if (!string.IsNullOrEmpty(detalle.SalidaPadre))
                {
                    var salidaPadre = await _context.MovimientosDetalle
                        .Where(md => md.IdMovimientoDetalles == detalle.SalidaPadre)
                        .Select(md => md.PrecioFinal)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(salidaPadre) && decimal.TryParse(salidaPadre, out decimal precio))
                    {
                        precioOriginal = precio;
                    }
                }

                string idMovimiento = Guid.NewGuid().ToString("N").Substring(0, 25).ToUpper();
                var movimiento = new MovimientoModel
                {
                    IdMovimiento = idMovimiento,
                    IdTipoMovimiento = "MV1",
                    IdAlmacen = almacenDestino,
                    FechaMovimiento = DateTime.Now,
                    IdEmpleado = await _context.Traspasos
                        .Where(t => t.IdTraspaso == detalle.IdTraspaso)
                        .Select(t => t.IdEmpleado)
                        .FirstOrDefaultAsync(),
                    Status = 1,
                    EsTraspaso = 1,
                    IdUsuario = idUsuarioReceptor
                };

                _context.Movimientos.Add(movimiento);
                await _context.SaveChangesAsync();

                string idMovimientoDetalle = Guid.NewGuid().ToString("N").Substring(0, 25).ToUpper();
                var movimientoDetalle = new MovimientoDetalleModel
                {
                    IdMovimientoDetalles = idMovimientoDetalle,
                    IdMovimiento = idMovimiento,
                    IdProducto = detalle.IdProducto,
                    Cantidad = (float)cantidadRecibida,
                    PrecioFinal = precioOriginal?.ToString(),
                    IdProveedor = detalle.IdProveedor,
                    Status = 1,
                    CantidadUtilizada = 0
                };

                _context.MovimientosDetalle.Add(movimientoDetalle);
                await _context.SaveChangesAsync();

                detalle.CantidadRecibida = nuevaCantidadRecibida;
                detalle.Completada = (byte)(detalleCompletado ? 1 : 0);

                if (string.IsNullOrEmpty(detalle.EntradaHija))
                {
                    detalle.EntradaHija = idMovimientoDetalle;
                }
                else
                {
                    detalle.EntradaHija += $",{idMovimientoDetalle}";
                }

                if (detalleCompletado)
                {
                    detalle.FechaCompletado = DateTime.Now;
                }

                _context.TraspasosDetalle.Update(detalle);
                await _context.SaveChangesAsync();

                return new TraspasoResultadoDto
                {
                    Exito = true,
                    Mensaje = "Detalle completado",
                    IdMovimiento = idMovimiento,
                    DetallesEntrada = new Dictionary<string, string>
                    {
                        { detalle.IdProducto, idMovimientoDetalle }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error en CompletarDetalleInterno: {ex.Message}");
                return new TraspasoResultadoDto
                {
                    Exito = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }

        // POST: api/Traspasos/rechazar-detalle
        [HttpPost("rechazar-detalle")]
        public async Task<ActionResult<TraspasoResultadoDto>> RechazarDetalle(RechazarDetalleDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation($"❌ Rechazando detalle {dto.IdTraspasoDetalle}");

                var detalle = await _context.TraspasosDetalle
                    .Include(d => d.Traspaso)
                    .FirstOrDefaultAsync(d => d.IdTraspasoDetalle == dto.IdTraspasoDetalle && d.Completada == 0);

                if (detalle == null)
                {
                    return BadRequest(new TraspasoResultadoDto
                    {
                        Exito = false,
                        Mensaje = "El detalle ya está completado/rechazado o no existe."
                    });
                }

                // Marcar como completado SIN generar entrada
                detalle.Completada = 1;
                detalle.FechaCompletado = DateTime.Now;
                detalle.MotivoCancelacion = "Producto no recibido";

                _context.TraspasosDetalle.Update(detalle);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✓ Detalle {dto.IdTraspasoDetalle} marcado como rechazado");

                // Verificar si el traspaso está completo
                var traspaso = detalle.Traspaso;
                if (traspaso.FechaRecepcion == null)
                {
                    traspaso.FechaRecepcion = DateTime.Now;
                    traspaso.IdUsuarioD = dto.IdUsuarioReceptor;
                    _context.Traspasos.Update(traspaso);
                    await _context.SaveChangesAsync();
                }

                var quedanDetallesPendientes = await _context.TraspasosDetalle
                    .AnyAsync(d => d.IdTraspaso == detalle.IdTraspaso && d.Completada == 0);

                if (!quedanDetallesPendientes)
                {
                    traspaso.Status = 1;
                    traspaso.FechaCompletado = DateTime.Now;
                    _context.Traspasos.Update(traspaso);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Traspaso {detalle.IdTraspaso} completado totalmente");
                }

                await transaction.CommitAsync();

                return Ok(new TraspasoResultadoDto
                {
                    Exito = true,
                    Mensaje = $"Producto marcado como NO recibido. No se generó entrada en inventario."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"❌ Error al rechazar detalle: {ex.Message}");
                return StatusCode(500, new TraspasoResultadoDto
                {
                    Exito = false,
                    Mensaje = $"Error inesperado: {ex.Message}"
                });
            }
        }

        // GET: api/Traspasos/todos
        [HttpGet("todos")]
        public async Task<ActionResult<IEnumerable<TraspasoPendienteDto>>> GetTodosTraspasos()
        {
            try
            {
                const string almacenParamedicos = "ALM6";

                var traspasos = await _context.Traspasos
                    .Include(t => t.Empleado)
                    .Include(t => t.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(t => t.AlmacenDestino == almacenParamedicos)
                    .OrderByDescending(t => t.FechaEnvio)
                    .Select(t => new TraspasoPendienteDto
                    {
                        IdTraspaso = t.IdTraspaso,
                        AlmacenOrigen = t.AlmacenOrigen,
                        NombreAlmacenOrigen = "Almacén Hermosillo",
                        AlmacenDestino = t.AlmacenDestino,
                        NombreAlmacenDestino = "Almacén Paramédicos",
                        FechaEnvio = t.FechaEnvio,
                        NombreChofer = t.Empleado != null ? t.Empleado.Nombre : null,
                        NoEconomico = t.IdUnidad,
                        Status = t.Status,
                        StatusTexto = t.Status == 0 ? "Pendiente" : t.Status == 1 ? "Completado" : "Cancelado",
                        Detalles = t.Detalles.Select(d => new TraspasoDetalleDTO
                        {
                            IdTraspasoDetalle = d.IdTraspasoDetalle,
                            IdProducto = d.IdProducto,
                            NombreProducto = d.Producto != null ? d.Producto.Nombre : "",
                            Descripcion = d.Producto != null ? d.Producto.Descripcion : "",
                            NumeroPieza = d.Producto != null ? d.Producto.NumeroPieza : "",
                            Cantidad = d.Cantidad,
                            CantidadRecibida = d.CantidadRecibida ?? 0,
                            CantidadFaltante = d.Cantidad - (d.CantidadRecibida ?? 0),
                            CantidadARecibir = 0,
                            Completada = d.Completada,
                            FechaCompletado = d.FechaCompletado,
                            IdProveedor = d.IdProveedor
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation($"✓ {traspasos.Count} traspasos recuperados");
                return Ok(traspasos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al obtener traspasos: {ex.Message}");
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { mensaje = "API de Traspasos funcionando correctamente", fecha = DateTime.Now });
        }
    }
}