using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Services
{
    public static class ApiConfiguration
    {
        // Configuración
        private const string LOCAL_IP = "192.168.100.108";
        private const string LOCAL_PORT = "7285";
        private const string PRODUCTION_URL = "https://sistemastrs.com/api";
        private static bool USE_LOCAL = true;

        // Singleton HttpClient
        private static HttpClient _httpClient;

        public static string BaseUrl => USE_LOCAL
            ? $"https://{LOCAL_IP}:{LOCAL_PORT}/api"
            : PRODUCTION_URL;

        // Método recomendado: obtiene el HttpClient singleton
        public static HttpClient GetHttpClient()
        {
            if (_httpClient == null)
            {
                var handler = new HttpClientHandler
                {
                    // Solo ignorar certificados en desarrollo local
                    ServerCertificateCustomValidationCallback = USE_LOCAL
                        ? (message, cert, chain, errors) => true
                        : null
                };

                _httpClient = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(60)
                };

                System.Diagnostics.Debug.WriteLine("✓ HttpClient singleton creado");
            }

            return _httpClient;
        }

        // Compatibilidad: mantener CreateHttpClient() para el código existente
        [Obsolete("Use GetHttpClient() en su lugar. Este método existe por compatibilidad.")]
        public static HttpClient CreateHttpClient() => GetHttpClient();

        public static void UsarModoLocal()
        {
            USE_LOCAL = true;
            System.Diagnostics.Debug.WriteLine("🏠 Usando API LOCAL");
        }

        public static void UsarModoProduccion()
        {
            USE_LOCAL = false;
            System.Diagnostics.Debug.WriteLine("☁️ Usando API en PRODUCCIÓN");
        }

        // Utilidad: ejecutar operación con reintentos y backoff exponencial
        public static async Task<T> EjecutarConReintentos<T>(
            Func<Task<T>> operacion,
            int maxIntentos = 3,
            int delayInicialMs = 1000)
        {
            int intento = 0;
            int delay = delayInicialMs;
            Exception ultimaEx = null;

            while (intento < maxIntentos)
            {
                intento++;
                try
                {
                    System.Diagnostics.Debug.WriteLine($"📡 Intento {intento}/{maxIntentos}");
                    return await operacion();
                }
                catch (HttpRequestException ex) when (intento < maxIntentos)
                {
                    ultimaEx = ex;
                    System.Diagnostics.Debug.WriteLine($"⚠️ HttpRequestException intento {intento}: {ex.Message}. Reintentando en {delay}ms");
                    await Task.Delay(delay);
                    delay *= 2;
                }
                catch (OperationCanceledException ex) when (intento < maxIntentos)  
                {
                    ultimaEx = ex;
                    System.Diagnostics.Debug.WriteLine($"⚠️ Timeout intento {intento}. Reintentando en {delay}ms");
                    await Task.Delay(delay);
                    delay *= 2;
                }
                catch (Exception ex)
                {
                    // Errores no recuperables: lanzar inmediatamente
                    throw;
                }
            }

            throw ultimaEx ?? new Exception("Operación fallida después de varios intentos");
        }
    }
}