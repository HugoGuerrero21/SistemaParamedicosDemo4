using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SistemaParamedicosDemo4.Data;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.MVVM.Models;
using SistemaParamedicosDemo4.MVVM.Views;

namespace SistemaParamedicosDemo4
{
    public partial class App : Application
    {
        private const string CrashFileName = "crash.log";

        public App()
        {



            // 1) Suscribir manejadores globales antes de inicializar demás componentes
            SubscribeGlobalExceptionHandlers();

            // 2) Evitar que el linker elimine converters referenciados sólo en XAML (si aplica)
            TryPreserveXamlConverters();

            // Inicializar DB y UI
            var db = DatabaseManager.Instance;
            InicializarDatosBase();

            MainPage = new NavigationPage(new LoginView());

            // Leer/mostrar crash guardado (si existe) en background
            _ = CheckSavedCrashAsync();

            //SOLO LO DEBO DE USAR CUANDO ESTE DESARROLLANDO
            DatabaseManager.Instance.ReiniciarBaseDeDatos();


        }

        private void SubscribeGlobalExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try { LogException(e.ExceptionObject as Exception, "AppDomain.UnhandledException"); } catch { }
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try
                {
                    LogException(e.Exception, "TaskScheduler.UnobservedTaskException");
                    e.SetObserved();
                }
                catch { }
            };
        }

        private void LogException(Exception ex, string source)
        {
            try
            {
                if (ex == null) return;
                var path = Path.Combine(FileSystem.AppDataDirectory, CrashFileName);
                var text = $"[{DateTime.Now:O}] {source}: {ex}\n---\n";
                File.AppendAllText(path, text);
            }
            catch { }
        }

        private async Task CheckSavedCrashAsync()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, CrashFileName);
                if (!File.Exists(path)) return;

                var content = File.ReadAllText(path);
                var shortContent = content.Length > 4000 ? content.Substring(0, 4000) + "\n(...)" : content;

                await Task.Delay(500); // esperar a que MainPage esté disponible
                try { await MainPage.DisplayAlert("Registro de crash detectado", shortContent, "OK"); } catch { }

                File.Delete(path);
            }
            catch { }
        }

        private void TryPreserveXamlConverters()
        {
            try
            {
                // Evita que el linker elimine converters referenciados sólo desde XAML
                _ = new SistemaParamedicosDemo4.Converters.StringToBoolConverter();
                // si existen:
                // _ = new SistemaParamedicosDemo4.Converters.InverseBoolConverter();
                // _ = new SistemaParamedicosDemo4.Converters.ImageSourceConverter();
            }
            catch { }
        }

        private void InicializarDatosBase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("========== INICIALIZANDO DATOS BASE ==========");
                System.Diagnostics.Debug.WriteLine("========== DATOS BASE INICIALIZADOS ==========\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR AL INICIALIZAR DATOS: {ex.Message}");
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            try { System.Diagnostics.Debug.WriteLine("😴 App entrando en modo sleep"); } catch { }
        }

        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                System.Diagnostics.Debug.WriteLine("👁️ App resumiendo...");
                var testQuery = DatabaseManager.Instance.Connection.Table<EmpleadoModel>().Take(1).ToList();
                System.Diagnostics.Debug.WriteLine("✅ Conexión a BD verificada en OnResume");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR CRÍTICO en OnResume: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                try { var _ = DatabaseManager.Instance; } catch (Exception ex2) { System.Diagnostics.Debug.WriteLine($"❌ No se pudo recuperar BD: {ex2.Message}"); }
            }
        }
    }
}