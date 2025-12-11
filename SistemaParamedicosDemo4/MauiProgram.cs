using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.MVVM.ViewModels;
using SistemaParamedicosDemo4.MVVM.Views;
using SistemaParamedicosDemo4.Service;
using SQLitePCL;

namespace SistemaParamedicosDemo4
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Batteries_V2.Init();
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })

            .UseMicrocharts();

#if DEBUG
    		builder.Logging.AddDebug();
            // ⭐ REGISTRAR REPOSITORIOS COMO SINGLETON
            builder.Services.AddSingleton<ConsultaRepository>();
            builder.Services.AddSingleton<EmpleadoRepository>();
            builder.Services.AddSingleton<ProductoRepository>();
            builder.Services.AddSingleton<TipoEnfermedadRepository>();
            builder.Services.AddSingleton<MovimientoDetalleRepository>();
            builder.Services.AddSingleton<UsuarioAccesoRepositories>();

            // ⭐ REGISTRAR SERVICIOS API COMO SINGLETON
            builder.Services.AddSingleton<ConsultaApiService>();
            builder.Services.AddSingleton<EmpleadoApiService>();
            builder.Services.AddSingleton<InventarioApiService>();
            builder.Services.AddSingleton<TipoEnfermedadApiService>();

            // ⭐ REGISTRAR VIEWMODELS COMO TRANSIENT (nueva instancia cada vez, pero limpia)
            builder.Services.AddTransient<ConsultaViewModel>();
            builder.Services.AddTransient<HistorialConsultasViewModel>();
            builder.Services.AddTransient<EmpleadosListViewModel>();

            // ⭐ REGISTRAR VISTAS
            builder.Services.AddTransient<ConsultaView>();
            builder.Services.AddTransient<HistorialConsultasView>();
            builder.Services.AddTransient<EmpleadosListView>();
#endif

            return builder.Build();

        }
    }

    public class HistorialMovimientosViewModel
    {
        // En HistorialMovimientosViewModel, agregar esta propiedad:
        private List<string> _listaTiposMovimiento = new() { "Todos", "Entrada", "Salida" };
        public List<string> ListaFiltrosTipos => _listaTiposMovimiento;

        internal async Task InicializarVistaAsync()
        {
            throw new NotImplementedException();
        }
    }
}
