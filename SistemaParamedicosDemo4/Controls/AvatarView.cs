using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SistemaParamedicosDemo4.Controls
{
    /// <summary>
    /// Control personalizado que muestra foto de empleado con fallback a iniciales
    /// Incluye caché de imágenes y manejo robusto de errores para móvil
    /// </summary>
    public class AvatarView : ContentView
    {
        private readonly Border _container;
        private readonly Border _imageBorder;
        private readonly Image _image;
        private readonly Label _iniciales;
        private readonly ActivityIndicator _loading;

        // Caché estático para imágenes ya cargadas
        private static readonly Dictionary<string, ImageSource> _imageCache = new();
        private string _currentUrl;
        private CancellationTokenSource _loadingCts;

        public static readonly BindableProperty FotoUrlProperty =
            BindableProperty.Create(
                nameof(FotoUrl),
                typeof(string),
                typeof(AvatarView),
                null,
                propertyChanged: OnFotoUrlChanged);

        public static readonly BindableProperty InicialesProperty =
            BindableProperty.Create(
                nameof(Iniciales),
                typeof(string),
                typeof(AvatarView),
                "??");

        public static readonly BindableProperty TamañoProperty =
            BindableProperty.Create(
                nameof(Tamaño),
                typeof(double),
                typeof(AvatarView),
                50.0,
                propertyChanged: OnTamañoChanged);

        public static readonly BindableProperty ColorFondoProperty =
            BindableProperty.Create(
                nameof(ColorFondo),
                typeof(Color),
                typeof(AvatarView),
                Color.FromArgb("#2E86AB"));

        public string FotoUrl
        {
            get => (string)GetValue(FotoUrlProperty);
            set => SetValue(FotoUrlProperty, value);
        }

        public string Iniciales
        {
            get => (string)GetValue(InicialesProperty);
            set => SetValue(InicialesProperty, value);
        }

        public double Tamaño
        {
            get => (double)GetValue(TamañoProperty);
            set => SetValue(TamañoProperty, value);
        }

        public Color ColorFondo
        {
            get => (Color)GetValue(ColorFondoProperty);
            set => SetValue(ColorFondoProperty, value);
        }

        public AvatarView()
        {
            // Grid contenedor
            var grid = new Grid
            {
                WidthRequest = 50,
                HeightRequest = 50
            };

            // Border con iniciales (fallback)
            _container = new Border
            {
                BackgroundColor = ColorFondo,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                WidthRequest = 50,
                HeightRequest = 50
            };

            _iniciales = new Label
            {
                TextColor = Colors.White,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            _container.Content = _iniciales;

            // Border para la imagen con fondo blanco y borde
            _imageBorder = new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 2,
                Stroke = ColorFondo,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                WidthRequest = 50,
                HeightRequest = 50,
                IsVisible = false,
                Padding = 2
            };

            // Imagen con AspectFit para que se vea completa
            _image = new Image
            {
                Aspect = Aspect.AspectFit,
                BackgroundColor = Colors.White,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            _imageBorder.Content = _image;

            // Loading indicator
            _loading = new ActivityIndicator
            {
                Color = Colors.White,
                IsRunning = false,
                IsVisible = false,
                WidthRequest = 20,
                HeightRequest = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Agregar elementos al grid
            grid.Children.Add(_container);
            grid.Children.Add(_imageBorder);
            grid.Children.Add(_loading);

            Content = grid;

            // Bind de iniciales
            _iniciales.SetBinding(Label.TextProperty, new Binding(nameof(Iniciales), source: this));
        }

        private static void OnFotoUrlChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AvatarView avatarView)
            {
                avatarView.CargarFoto(newValue as string);
            }
        }

        private static void OnTamañoChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AvatarView avatarView && newValue is double tamaño)
            {
                avatarView.ActualizarTamaño(tamaño);
            }
        }

        private void ActualizarTamaño(double tamaño)
        {
            var radio = tamaño * 0.15;

            // Actualizar contenedor principal
            if (Content is Grid grid)
            {
                grid.WidthRequest = tamaño;
                grid.HeightRequest = tamaño;
            }

            // Actualizar border de iniciales
            _container.WidthRequest = tamaño;
            _container.HeightRequest = tamaño;
            _container.StrokeShape = new RoundRectangle { CornerRadius = radio };
            _container.BackgroundColor = ColorFondo;

            // Actualizar border de imagen
            _imageBorder.WidthRequest = tamaño;
            _imageBorder.HeightRequest = tamaño;
            _imageBorder.StrokeShape = new RoundRectangle { CornerRadius = radio };
            _imageBorder.Stroke = ColorFondo;

            // Actualizar tamaño de letra
            _iniciales.FontSize = tamaño * 0.36;
        }

        private async void CargarFoto(string url)
        {
            try
            {
                // Cancelar carga anterior si existe
                _loadingCts?.Cancel();
                _loadingCts = new CancellationTokenSource();

                // Si no hay URL válida, mostrar iniciales
                if (string.IsNullOrWhiteSpace(url) ||
                    (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                     !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                {
                    MostrarIniciales();
                    return;
                }

                // Si la URL no cambió, no hacer nada
                if (_currentUrl == url)
                {
                    return;
                }

                _currentUrl = url;

                // Verificar caché primero
                if (_imageCache.TryGetValue(url, out var cachedSource))
                {
                    Debug.WriteLine($"✅ Imagen en caché: {url}");
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _image.Source = cachedSource;
                        MostrarFoto();
                    });
                    return;
                }

                // Mostrar loading
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _loading.IsVisible = true;
                    _loading.IsRunning = true;
                    _container.IsVisible = false;
                    _imageBorder.IsVisible = false;
                });

                Debug.WriteLine($"🔄 Cargando imagen: {url}");

                // Intentar cargar imagen con timeout
                var loadTask = Task.Run(async () =>
                {
                    try
                    {
                        var imageSource = ImageSource.FromUri(new Uri(url));

                        // Esperar un momento para que la imagen intente cargar
                        await Task.Delay(100, _loadingCts.Token);

                        return imageSource;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ Error interno cargando imagen: {ex.Message}");
                        return null;
                    }
                }, _loadingCts.Token);

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), _loadingCts.Token);
                var completedTask = await Task.WhenAny(loadTask, timeoutTask);

                if (completedTask == timeoutTask || _loadingCts.Token.IsCancellationRequested)
                {
                    Debug.WriteLine($"⏱️ Timeout cargando imagen: {url}");
                    MostrarIniciales();
                    return;
                }

                var source = await loadTask;

                if (source != null && !_loadingCts.Token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        try
                        {
                            _image.Source = source;

                            // Agregar al caché
                            if (!_imageCache.ContainsKey(url))
                            {
                                _imageCache[url] = source;
                                Debug.WriteLine($"💾 Imagen guardada en caché: {url}");
                            }

                            // Esperar un momento y verificar si se cargó
                            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                            {
                                if (_image.Source != null)
                                {
                                    MostrarFoto();
                                    Debug.WriteLine($"✅ Imagen mostrada: {url}");
                                }
                                else
                                {
                                    MostrarIniciales();
                                    Debug.WriteLine($"❌ Imagen falló al cargar: {url}");
                                }
                                return false;
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"❌ Error mostrando imagen: {ex.Message}");
                            MostrarIniciales();
                        }
                    });
                }
                else
                {
                    MostrarIniciales();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"🚫 Carga cancelada: {url}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error general cargando foto: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MostrarIniciales();
            }
        }

        private void MostrarFoto()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _imageBorder.IsVisible = true;
                _container.IsVisible = false;
                _loading.IsVisible = false;
                _loading.IsRunning = false;
            });
        }

        private void MostrarIniciales()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _imageBorder.IsVisible = false;
                _container.IsVisible = true;
                _loading.IsVisible = false;
                _loading.IsRunning = false;
            });
        }

        /// <summary>
        /// Limpia el caché de imágenes (útil para liberar memoria)
        /// </summary>
        public static void LimpiarCache()
        {
            _imageCache.Clear();
            Debug.WriteLine("🧹 Caché de imágenes limpiado");
        }

        /// <summary>
        /// Limpia recursos al destruir el control
        /// </summary>
        protected override void OnHandlerChanging(HandlerChangingEventArgs args)
        {
            base.OnHandlerChanging(args);

            if (args.NewHandler == null)
            {
                _loadingCts?.Cancel();
                _loadingCts?.Dispose();
            }
        }
    }
}