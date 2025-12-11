using Microcharts;
using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.DTOS;
using SistemaParamedicosDemo4.Service;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class EstadisticasViewModel : INotifyPropertyChanged
    {
        private EstadisticasApiService _estadisticasApiService;
        private EstadisticasRepository _estadisticasRepo;

        public bool IsCargando { get; set; }
        public bool TieneEstadisticas { get; set; }

        // Filtros
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaMaxima { get; set; } = DateTime.Now;

        // Datos Generales
        public int TotalConsultas { get; set; }
        public string Periodo { get; set; }
        public string EnfermedadMasComun { get; set; }
        public int CantidadMasComun { get; set; }
        public decimal PromedioDiario { get; set; }

        // Listas divididas para la vista de 3 columnas
        public ObservableCollection<EstadisticaItem> EstadisticasIzquierda { get; set; }
        public ObservableCollection<EstadisticaItem> EstadisticasDerecha { get; set; }

        // Mantenemos la lista completa original también
        public ObservableCollection<EstadisticaItem> Estadisticas { get; set; }

        public Chart GraficaPastel { get; set; }

        public ICommand CargarEstadisticasCommand { get; }
        public ICommand ExportarPdfCommand { get; }

        public EstadisticasViewModel()
        {
            _estadisticasApiService = new EstadisticasApiService();
            _estadisticasRepo = new EstadisticasRepository();

            Estadisticas = new ObservableCollection<EstadisticaItem>();
            EstadisticasIzquierda = new ObservableCollection<EstadisticaItem>();
            EstadisticasDerecha = new ObservableCollection<EstadisticaItem>();

            FechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            FechaFin = DateTime.Now;

            CargarEstadisticasCommand = new Command(async () => await CargarEstadisticasAsync());
            ExportarPdfCommand = new Command(async () => await ExportarPdfAsync());

            PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(FechaInicio) || e.PropertyName == nameof(FechaFin))
                {
                    if (FechaInicio <= FechaFin && !IsCargando)
                        await CargarEstadisticasAsync();
                }
            };
        }

        public async Task InicializarAsync()
        {
            await CargarEstadisticasAsync();
        }

        private async Task CargarEstadisticasAsync()
        {
            try
            {
                IsCargando = true;
                TieneEstadisticas = false;
                GraficaPastel = null;

                EstadisticasResponseDto dto = null;
                try
                {
                    dto = await _estadisticasApiService.ObtenerEstadisticasAsync(FechaInicio, FechaFin);
                }
                catch { }

                if (dto != null)
                {
                    CargarDatos(dto);
                }
                else
                {
                    // Lógica local de respaldo (Offline)
                    var local = _estadisticasRepo.CalcularEstadisticasRango(FechaInicio, FechaFin);
                    if (local != null)
                    {
                        var items = local.Estadisticas.Select(x => new EstadisticaItem
                        {
                            NombreEnfermedad = x.NombreEnfermedad,
                            Cantidad = x.Cantidad,
                            Porcentaje = x.Porcentaje,
                            Color = x.Color
                        }).ToList();

                        // Creamos un DTO temporal. CORREGIDO: Usamos EstadisticaEnfermedadDto
                        var tempDto = new EstadisticasResponseDto
                        {
                            TotalConsultas = local.TotalConsultas,
                            Periodo = local.Periodo,
                            EnfermedadMasComun = local.EnfermedadMasComun,
                            CantidadMasComun = local.CantidadMasComun,
                            PromedioDiario = local.PromedioDiario,
                            Estadisticas = items.Select(x => new EstadisticaEnfermedadDto
                            {
                                NombreEnfermedad = x.NombreEnfermedad,
                                Cantidad = x.Cantidad,
                                Porcentaje = x.Porcentaje,
                                Color = x.Color
                            }).ToList()
                        };
                        CargarDatos(tempDto);
                    }
                }

                if (TieneEstadisticas) GenerarGrafica();
            }
            finally
            {
                IsCargando = false;
            }
        }

        private void CargarDatos(EstadisticasResponseDto dto)
        {
            TotalConsultas = dto.TotalConsultas;
            Periodo = dto.Periodo;
            EnfermedadMasComun = dto.EnfermedadMasComun;
            CantidadMasComun = dto.CantidadMasComun;
            PromedioDiario = dto.PromedioDiario;

            Estadisticas.Clear();
            EstadisticasIzquierda.Clear();
            EstadisticasDerecha.Clear();

            // Dividir datos en dos columnas
            int totalItems = dto.Estadisticas.Count;
            int mitad = (int)Math.Ceiling(totalItems / 2.0);

            int index = 0;
            foreach (var x in dto.Estadisticas)
            {
                var item = new EstadisticaItem
                {
                    NombreEnfermedad = x.NombreEnfermedad,
                    Cantidad = x.Cantidad,
                    Porcentaje = x.Porcentaje,
                    Color = x.Color
                };

                // Agregamos a la lista general (para la gráfica)
                Estadisticas.Add(item);

                // Distribuimos en las listas laterales
                if (index < mitad)
                    EstadisticasIzquierda.Add(item);
                else
                    EstadisticasDerecha.Add(item);

                index++;
            }

            TieneEstadisticas = TotalConsultas > 0;
        }

        private void GenerarGrafica()
        {
            var data = Estadisticas.Where(e => e.Cantidad > 0).ToList();
            if (data.Count == 0) return;

            var entries = data.Select(e => new ChartEntry((float)e.Cantidad)
            {
                Label = "",
                ValueLabel = "",
                Color = SKColor.Parse(e.Color)
            }).ToList();

            GraficaPastel = new DonutChart
            {
                Entries = entries,
                BackgroundColor = SKColors.Transparent,
                HoleRadius = 0.6f,
                LabelTextSize = 0,
                GraphPosition = GraphPosition.AutoFill
            };

            // Notificamos cambios
            OnPropertyChanged(nameof(GraficaPastel));
            OnPropertyChanged(nameof(EstadisticasIzquierda));
            OnPropertyChanged(nameof(EstadisticasDerecha));
        }

        private async Task ExportarPdfAsync()
        {
            await Application.Current.MainPage.DisplayAlert("PDF", "Función pendiente", "OK");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ⭐ ESTA CLASE FALTABA EN EL CÓDIGO ANTERIOR Y CAUSABA LOS ERRORES
    public class EstadisticaItem
    {
        public string NombreEnfermedad { get; set; }
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
        public string Color { get; set; }

        public Microsoft.Maui.Graphics.Color ColorMaui =>
            Microsoft.Maui.Graphics.Color.FromHex(this.Color);

        public string TextoDetalle => $"Número de consultas: {Cantidad}";
        public string TextoPorcentaje => $"{Porcentaje:F1}%";
    }
}