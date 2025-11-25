using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParamedicos.API.Data;
using SistemaParamedicos.API.DTOs;

namespace SistemaParamedicos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventarioController> _logger;
        private const string ALMACEN_PARAMEDICOS = "ALM6";

        public InventarioController(
            ApplicationDbContext context,
            ILogger<InventarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GenerarIdMovimiento()
        {
            var ultimoMovimiento = _context.Movimientos
                .OrderByDescending(m => m.IdMovimiento)
                .Select(m => m.IdMovimiento)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(ultimoMovimiento))
                return "1";

            if (int.TryParse(ultimoMovimiento, out int numero))
            {
                return (numero + 1).ToString();
            }

            return DateTime.Now.Ticks.ToString();
        }

        private string GenerarIdMovimientoDetalle()
        {
            var ultimoDetalle = _context.MovimientosDetalle
                .OrderByDescending(d => d.IdMovimientoDetalles)
                .Select(d => d.IdMovimientoDetalles)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(ultimoDetalle))
                return "1";

            if (int.TryParse(ultimoDetalle, out int numero))
            {
                return (numero + 1).ToString();
            }

            return DateTime.Now.Ticks.ToString();
        }

        /// <summary>
        /// GET: api/inventario/existencias
        /// Obtiene todas las existencias del almacén de paramédicos
        /// </summary>
        [HttpGet("existencias")]
        public async Task<ActionResult<IEnumerable<ExistenciaDTO>>> GetExistencias()
        {
            try
            {
                _logger.LogInformation("Obteniendo existencias del almacén de paramédicos...");

                // Usar FromSqlRaw para consultar directamente la vista
                var existencias = await _context.ExistenciasParamedicos
                    .FromSqlRaw("SELECT * FROM V_EXISTENCIAS_PARAMEDICOS")
                    .Select(e => new ExistenciaDTO
                    {
                        Producto = e.Producto,
                        NombreDelProducto = e.NombreDelProducto,
                        Descripcion = e.Descripcion,
                        Marca = e.Marca,
                        NumeroDePieza = e.NumeroDePieza,
                        Entrada = e.Entrada,
                        Salida = e.Salida,
                        Existencia = e.Existencia,
                        Foto = e.Foto  
                    })
                    .OrderBy(e => e.NombreDelProducto)
                    .ToListAsync();

                _logger.LogInformation($"{existencias.Count} productos encontrados en inventario");
                return Ok(existencias);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener existencias: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: api/inventario/existencias/{idProducto}
        /// Obtiene la existencia de un producto específico
        /// </summary>
        [HttpGet("existencias/{idProducto}")]
        public async Task<ActionResult<ExistenciaDTO>> GetExistenciaProducto(string idProducto)
        {
            try
            {
                var existencia = await _context.ExistenciasParamedicos
                    .FromSqlRaw("SELECT * FROM V_EXISTENCIAS_PARAMEDICOS WHERE PRODUCTO = {0}", idProducto)
                    .Select(e => new ExistenciaDTO
                    {
                        Producto = e.Producto,
                        NombreDelProducto = e.NombreDelProducto,
                        Descripcion = e.Descripcion,
                        Marca = e.Marca,
                        NumeroDePieza = e.NumeroDePieza,
                        Entrada = e.Entrada,
                        Salida = e.Salida,
                        Existencia = e.Existencia
                    })
                    .FirstOrDefaultAsync();

                if (existencia == null)
                    return NotFound(new { message = $"Producto {idProducto} no encontrado" });

                return Ok(existencia);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/inventario/existencias/buscar?texto=...
        /// Busca productos en el inventario por nombre, descripción o código
        /// </summary>
        [HttpGet("existencias/buscar")]
        public async Task<ActionResult<IEnumerable<ExistenciaDTO>>> BuscarExistencias([FromQuery] string texto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(texto))
                    return await GetExistencias();

                var existencias = await _context.ExistenciasParamedicos
                    .FromSqlRaw("SELECT * FROM V_EXISTENCIAS_PARAMEDICOS WHERE NOMBRE_DEL_PRODUCTO LIKE {0} OR PRODUCTO LIKE {0} OR DESCRIPCION LIKE {0}",
                        $"%{texto}%")
                    .Select(e => new ExistenciaDTO
                    {
                        Producto = e.Producto,
                        NombreDelProducto = e.NombreDelProducto,
                        Descripcion = e.Descripcion,
                        Marca = e.Marca,
                        NumeroDePieza = e.NumeroDePieza,
                        Entrada = e.Entrada,
                        Salida = e.Salida,
                        Existencia = e.Existencia
                    })
                    .OrderBy(e => e.NombreDelProducto)
                    .ToListAsync();

                return Ok(existencias);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/inventario/movimientos
        /// Obtiene todos los movimientos del almacén de paramédicos
        /// </summary>
        [HttpGet("movimientos")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetMovimientos(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var query = _context.Movimientos
                    .Include(m => m.TipoMovimiento)
                    .Include(m => m.Empleado)
                    .Include(m => m.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(m => m.IdAlmacen == ALMACEN_PARAMEDICOS);

                if (fechaInicio.HasValue)
                    query = query.Where(m => m.FechaMovimiento >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(m => m.FechaMovimiento <= fechaFin.Value);

                var movimientos = await query
                    .OrderByDescending(m => m.FechaMovimiento)
                    .Select(m => new MovimientoDTO
                    {
                        IdMovimiento = m.IdMovimiento,
                        IdTipoMovimiento = m.IdTipoMovimiento,
                        TipoMovimientoNombre = m.TipoMovimiento != null ? m.TipoMovimiento.Nombre : "",
                        IdAlmacen = m.IdAlmacen,
                        FechaMovimiento = m.FechaMovimiento,
                        IdEmpleado = m.IdEmpleado,
                        NombreEmpleado = m.Empleado != null ? m.Empleado.Nombre : null,
                        Status = m.Status,
                        IdUsuario = m.IdUsuario,
                        Detalles = m.Detalles.Select(d => new MovimientoDetalleDTO
                        {
                            IdMovimientoDetalles = d.IdMovimientoDetalles,
                            IdProducto = d.IdProducto,
                            NombreProducto = d.Producto != null ? d.Producto.Nombre : "",
                            Cantidad = d.Cantidad,
                            PrecioFinal = d.PrecioFinal,
                            Status = d.Status,
                            CantidadUtilizada = d.CantidadUtilizada
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/inventario/movimientos/{id}
        /// Obtiene un movimiento específico por ID
        /// </summary>
        [HttpGet("movimientos/{id}")]
        public async Task<ActionResult<MovimientoDTO>> GetMovimiento(string id)
        {
            try
            {
                var movimiento = await _context.Movimientos
                    .Include(m => m.TipoMovimiento)
                    .Include(m => m.Empleado)
                    .Include(m => m.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(m => m.IdMovimiento == id && m.IdAlmacen == ALMACEN_PARAMEDICOS)
                    .Select(m => new MovimientoDTO
                    {
                        IdMovimiento = m.IdMovimiento,
                        IdTipoMovimiento = m.IdTipoMovimiento,
                        TipoMovimientoNombre = m.TipoMovimiento != null ? m.TipoMovimiento.Nombre : "",
                        IdAlmacen = m.IdAlmacen,
                        FechaMovimiento = m.FechaMovimiento,
                        IdEmpleado = m.IdEmpleado,
                        NombreEmpleado = m.Empleado != null ? m.Empleado.Nombre : null,
                        Status = m.Status,
                        IdUsuario = m.IdUsuario,
                        Detalles = m.Detalles.Select(d => new MovimientoDetalleDTO
                        {
                            IdMovimientoDetalles = d.IdMovimientoDetalles,
                            IdProducto = d.IdProducto,
                            NombreProducto = d.Producto != null ? d.Producto.Nombre : "",
                            Cantidad = d.Cantidad,
                            PrecioFinal = d.PrecioFinal,
                            Status = d.Status,
                            CantidadUtilizada = d.CantidadUtilizada
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (movimiento == null)
                    return NotFound(new { message = $"Movimiento {id} no encontrado" });

                return Ok(movimiento);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/inventario/salida
        /// Registra una nueva salida de productos del almacén de paramédicos
        /// </summary>
        [HttpPost("salida")]
        public async Task<ActionResult<MovimientoResponseDTO>> RegistrarSalida([FromBody] CrearSalidaDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Registrando salida para empleado {request.IdEmpleado}");

                // Validar que el empleado existe
                var empleadoExiste = await _context.Empleados
                    .AnyAsync(e => e.IdEmpleado == request.IdEmpleado);

                if (!empleadoExiste)
                {
                    return BadRequest(new MovimientoResponseDTO
                    {
                        Success = false,
                        Message = $"Empleado {request.IdEmpleado} no encontrado"
                    });
                }

                // Validar que hay suficiente stock de cada producto
                foreach (var producto in request.Productos)
                {
                    var existencia = await _context.ExistenciasParamedicos
                        .FromSqlRaw("SELECT * FROM V_EXISTENCIAS_PARAMEDICOS WHERE PRODUCTO = {0}", producto.IdProducto)
                        .FirstOrDefaultAsync();

                    if (existencia == null)
                    {
                        return BadRequest(new MovimientoResponseDTO
                        {
                            Success = false,
                            Message = $"Producto {producto.IdProducto} no encontrado en inventario"
                        });
                    }

                    if (existencia.Existencia < producto.Cantidad)
                    {
                        return BadRequest(new MovimientoResponseDTO
                        {
                            Success = false,
                            Message = $"Stock insuficiente para {existencia.NombreDelProducto}. Disponible: {existencia.Existencia}, Solicitado: {producto.Cantidad}"
                        });
                    }
                }

                // Crear el movimiento de salida (MV2)
                var movimiento = new Models.MovimientoModel
                {
                    IdMovimiento = GenerarIdMovimiento(),
                    IdTipoMovimiento = "MV2",
                    IdAlmacen = ALMACEN_PARAMEDICOS,
                    FechaMovimiento = DateTime.Now,
                    IdEmpleado = request.IdEmpleado,
                    Status = 1,
                    IdUsuario = request.IdUsuario,
                    EsTraspaso = 0  // ⬅️ Valor por defecto
                };

                _context.Movimientos.Add(movimiento);
                await _context.SaveChangesAsync();

                // Crear los detalles de la salida
                foreach (var producto in request.Productos)
                {
                    _logger.LogInformation($"Buscando entradas disponibles para producto {producto.IdProducto}");

                    // Obtener TODAS las entradas del producto para depuración
                    var todasLasEntradas = await _context.MovimientosDetalle
                        .Include(d => d.Movimiento)
                        .Where(d => d.IdProducto == producto.IdProducto)
                        .ToListAsync();

                    _logger.LogInformation($"Total de detalles encontrados para {producto.IdProducto}: {todasLasEntradas.Count}");

                    // Filtrar entradas del almacén de paramédicos
                    var entradasAlmacen = todasLasEntradas
                        .Where(d => d.Movimiento != null && d.Movimiento.IdAlmacen == ALMACEN_PARAMEDICOS)
                        .ToList();

                    _logger.LogInformation($"Detalles en almacén ALM6: {entradasAlmacen.Count}");

                    // Filtrar solo entradas (MV1)
                    var soloEntradas = entradasAlmacen
                        .Where(d => d.Movimiento.IdTipoMovimiento == "MV1")
                        .ToList();

                    _logger.LogInformation($"Entradas (MV1) encontradas: {soloEntradas.Count}");

                    // Filtrar por status activo
                    var entradasActivas = soloEntradas
                        .Where(d => d.Status == 1)
                        .ToList();

                    _logger.LogInformation($"Entradas con STATUS=1: {entradasActivas.Count}");

                    // Filtrar las que tienen cantidad disponible
                    var entradasDisponibles = entradasActivas
                        .Where(d => (d.Cantidad - (d.CantidadUtilizada ?? 0)) > 0.001f)
                        .OrderBy(d => d.Movimiento.FechaMovimiento)
                        .ToList();

                    _logger.LogInformation($"Entradas disponibles con stock: {entradasDisponibles.Count}");

                    // Log detallado de cada entrada
                    foreach (var entrada in entradasActivas)
                    {
                        var disponible = entrada.Cantidad - (entrada.CantidadUtilizada ?? 0);
                        _logger.LogInformation($"Entrada {entrada.IdMovimientoDetalles}: Cantidad={entrada.Cantidad}, Utilizada={entrada.CantidadUtilizada ?? 0}, Disponible={disponible}, Status={entrada.Status}");
                    }

                    if (!entradasDisponibles.Any())
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new MovimientoResponseDTO
                        {
                            Success = false,
                            Message = $"No hay suficientes entradas disponibles para el producto {producto.IdProducto}"
                        });
                    }

                    float cantidadRestante = producto.Cantidad;

                    foreach (var entrada in entradasDisponibles)
                    {
                        if (cantidadRestante <= 0) break;

                        float disponible = entrada.Cantidad - (entrada.CantidadUtilizada ?? 0);
                        float cantidadAUtilizar = Math.Min(disponible, cantidadRestante);

                        _logger.LogInformation($"Usando {cantidadAUtilizar} unidades de la entrada {entrada.IdMovimientoDetalles}");

                        // Crear detalle de salida
                        var detalle = new Models.MovimientoDetalleModel
                        {
                            IdMovimientoDetalles = GenerarIdMovimientoDetalle(),
                            IdMovimiento = movimiento.IdMovimiento,
                            IdProducto = producto.IdProducto,
                            Cantidad = cantidadAUtilizar,
                            Status = 1,
                            CantidadUtilizada = 0,
                            IdDetallePadre = entrada.IdMovimientoDetalles,
                            PrecioFinal = entrada.PrecioFinal,
                            IdProveedor = entrada.IdProveedor
                        };

                        _context.MovimientosDetalle.Add(detalle);

                        // Actualizar cantidad utilizada en la entrada
                        entrada.CantidadUtilizada = (entrada.CantidadUtilizada ?? 0) + cantidadAUtilizar;

                        // Si se agotó la entrada, cambiar status a 0
                        if (Math.Abs(entrada.Cantidad - (entrada.CantidadUtilizada ?? 0)) < 0.001f)
                        {
                            entrada.Status = 0;
                            _logger.LogInformation($"Entrada {entrada.IdMovimientoDetalles} agotada");
                        }

                        cantidadRestante -= cantidadAUtilizar;
                    }

                    if (cantidadRestante > 0)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new MovimientoResponseDTO
                        {
                            Success = false,
                            Message = $"No hay suficientes entradas disponibles para completar la cantidad solicitada del producto {producto.IdProducto}. Faltaron {cantidadRestante} unidades"
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Salida registrada exitosamente: {movimiento.IdMovimiento}");

                return Ok(new MovimientoResponseDTO
                {
                    Success = true,
                    Message = "Salida registrada exitosamente",
                    IdMovimiento = movimiento.IdMovimiento
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error al registrar salida: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new MovimientoResponseDTO
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }


        /// GET: api/inventario/historial-salidas/{idEmpleado}
        /// Obtiene el historial de salidas para un empleado específico

        [HttpGet("historial-salidas/{idEmpleado}")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetHistorialSalidas(string idEmpleado)
        {
            try
            {
                var movimientos = await _context.Movimientos
                    .Include(m => m.TipoMovimiento)
                    .Include(m => m.Empleado)
                    .Include(m => m.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(m => m.IdAlmacen == ALMACEN_PARAMEDICOS &&
                               m.IdTipoMovimiento == "MV2" &&
                               m.IdEmpleado == idEmpleado)
                    .OrderByDescending(m => m.FechaMovimiento)
                    .Select(m => new MovimientoDTO
                    {
                        IdMovimiento = m.IdMovimiento,
                        IdTipoMovimiento = m.IdTipoMovimiento,
                        TipoMovimientoNombre = m.TipoMovimiento != null ? m.TipoMovimiento.Nombre : "",
                        IdAlmacen = m.IdAlmacen,
                        FechaMovimiento = m.FechaMovimiento,
                        IdEmpleado = m.IdEmpleado,
                        NombreEmpleado = m.Empleado != null ? m.Empleado.Nombre : null,
                        Status = m.Status,
                        IdUsuario = m.IdUsuario,
                        Detalles = m.Detalles.Select(d => new MovimientoDetalleDTO
                        {
                            IdMovimientoDetalles = d.IdMovimientoDetalles,
                            IdProducto = d.IdProducto,
                            NombreProducto = d.Producto != null ? d.Producto.Nombre : "",
                            Cantidad = d.Cantidad,
                            PrecioFinal = d.PrecioFinal,
                            Status = d.Status,
                            CantidadUtilizada = d.CantidadUtilizada
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/inventario/entrada
        /// Registra una nueva entrada de productos al almacén de paramédicos
        /// </summary>
        [HttpPost("entrada")]
        public async Task<ActionResult<MovimientoResponseDTO>> RegistrarEntrada([FromBody] CrearEntradaDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Registrando entrada al almacén {ALMACEN_PARAMEDICOS}");

                // Validar que los productos existen
                foreach (var producto in request.Productos)
                {
                    var productoExiste = await _context.Productos
                        .AnyAsync(p => p.IdProducto == producto.IdProducto);

                    if (!productoExiste)
                    {
                        return BadRequest(new MovimientoResponseDTO
                        {
                            Success = false,
                            Message = $"Producto {producto.IdProducto} no encontrado"
                        });
                    }
                }

                // Crear el movimiento de entrada (MV1)
                var movimiento = new Models.MovimientoModel
                {
                    IdMovimiento = GenerarIdMovimiento(),
                    IdTipoMovimiento = "MV1",
                    IdAlmacen = ALMACEN_PARAMEDICOS,
                    FechaMovimiento = DateTime.Now,
                    IdEmpleado = null, // Las entradas no tienen empleado destino
                    Status = 1,
                    IdUsuario = request.IdUsuario,
                    EsTraspaso = 0
                };

                _context.Movimientos.Add(movimiento);
                await _context.SaveChangesAsync();

                // Crear los detalles de la entrada
                foreach (var producto in request.Productos)
                {
                    var detalle = new Models.MovimientoDetalleModel
                    {
                        IdMovimientoDetalles = GenerarIdMovimientoDetalle(),
                        IdMovimiento = movimiento.IdMovimiento,
                        IdProducto = producto.IdProducto,
                        Cantidad = producto.Cantidad,
                        Status = 1,  // Entrada activa con stock disponible
                        CantidadUtilizada = 0,
                        PrecioFinal = producto.PrecioFinal,
                        IdProveedor = producto.IdProveedor
                    };

                    _context.MovimientosDetalle.Add(detalle);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Entrada registrada exitosamente: {movimiento.IdMovimiento}");

                return Ok(new MovimientoResponseDTO
                {
                    Success = true,
                    Message = "Entrada registrada exitosamente",
                    IdMovimiento = movimiento.IdMovimiento
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error al registrar entrada: {ex.Message}");
                return StatusCode(500, new MovimientoResponseDTO
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // Agregar este DTO a tu archivo DTOs.cs
        public class CrearEntradaDTO
        {
            public string IdUsuario { get; set; }
            public List<DetalleEntradaDTO> Productos { get; set; }
        }

        public class DetalleEntradaDTO
        {
            public string IdProducto { get; set; }
            public float Cantidad { get; set; }
            public string? PrecioFinal { get; set; }
            public string? IdProveedor { get; set; }
        }
    }
}