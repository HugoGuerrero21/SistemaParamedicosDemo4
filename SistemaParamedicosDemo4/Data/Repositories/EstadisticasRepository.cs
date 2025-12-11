using SistemaParamedicosDemo4.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class EstadisticasRepository
    {
        private readonly ConsultaRepository _consultaRepo;
        private readonly TipoEnfermedadRepository _tipoEnfermedadRepo;

        public EstadisticasRepository()
        {
            _consultaRepo = new ConsultaRepository();
            _tipoEnfermedadRepo = new TipoEnfermedadRepository();
        }

        // ⭐ MÉTODO UNIFICADO: Recibe rango exacto
        public EstadisticasLocal CalcularEstadisticasRango(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // Ajustar fin del día para la fecha fin
                DateTime inicio = fechaInicio.Date;
                DateTime fin = fechaFin.Date.AddDays(1).AddTicks(-1);
                string periodo = $"Del {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}";

                // 1. Obtener todas y filtrar
                var todasConsultas = _consultaRepo.GetAllConsultas();
                var consultasPeriodo = todasConsultas
                    .Where(c => c.FechaConsulta >= inicio && c.FechaConsulta <= fin)
                    .ToList();

                if (consultasPeriodo.Count == 0)
                {
                    return new EstadisticasLocal
                    {
                        TotalConsultas = 0,
                        Periodo = periodo,
                        Estadisticas = new List<EstadisticaLocal>(),
                        EnfermedadMasComun = "N/A",
                        PromedioDiario = 0
                    };
                }

                // 2. Agrupar
                var tiposEnfermedad = _tipoEnfermedadRepo.GetAllTypes();
                var grupos = consultasPeriodo
                    .GroupBy(c => c.IdTipoEnfermedad)
                    .Select(g => new { Id = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(x => x.Cantidad)
                    .ToList();

                // 3. Colores
                var colores = new List<string> { "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E2" };

                // 4. Crear estadísticas
                var totalConsultas = consultasPeriodo.Count;
                var estadisticas = new List<EstadisticaLocal>();

                for (int i = 0; i < grupos.Count; i++)
                {
                    var grupo = grupos[i];
                    var tipo = tiposEnfermedad.FirstOrDefault(t => t.IdTipoEnfermedad == grupo.Id);
                    var porcentaje = Math.Round((decimal)grupo.Cantidad / totalConsultas * 100, 2);

                    estadisticas.Add(new EstadisticaLocal
                    {
                        IdTipoEnfermedad = grupo.Id,
                        NombreEnfermedad = tipo?.NombreEnfermedad ?? "Desconocido",
                        Cantidad = grupo.Cantidad,
                        Porcentaje = porcentaje,
                        Color = colores[i % colores.Count]
                    });
                }

                // 5. Calcular promedio diario corregido
                double totalDias = (fin - inicio).TotalDays;
                if (totalDias < 1) totalDias = 1;
                int diasEnPeriodo = (int)Math.Ceiling(totalDias);

                var promedioDiario = Math.Round((decimal)totalConsultas / diasEnPeriodo, 2);

                return new EstadisticasLocal
                {
                    TotalConsultas = totalConsultas,
                    Periodo = periodo,
                    Estadisticas = estadisticas,
                    EnfermedadMasComun = estadisticas.FirstOrDefault()?.NombreEnfermedad ?? "N/A",
                    CantidadMasComun = grupos.FirstOrDefault()?.Cantidad ?? 0,
                    PromedioDiario = promedioDiario
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Repo: {ex.Message}");
                return null;
            }
        }
    }

    public class EstadisticasLocal
    {
        public int TotalConsultas { get; set; }
        public string Periodo { get; set; }
        public List<EstadisticaLocal> Estadisticas { get; set; }
        public string EnfermedadMasComun { get; set; }
        public int CantidadMasComun { get; set; }
        public decimal PromedioDiario { get; set; }
    }

    public class EstadisticaLocal
    {
        public int IdTipoEnfermedad { get; set; }
        public string NombreEnfermedad { get; set; }
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
        public string Color { get; set; }
    }
}