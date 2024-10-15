using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AsheronBuilder.Core.Assets;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Image = System.Windows.Controls.Image;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace AsheronBuilder.Rendering
{
    public class OpenGLControl : UserControl
    {
        private Renderer _renderer;
        private GameWindow _gameWindow;
        private WriteableBitmap _writeableBitmap;
        private Image _image;
        private DispatcherTimer _renderTimer;
        private int _framebuffer;
        private int _texture;

        public OpenGLControl()
        {
            _image = new Image();
            Content = _image;

            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;

            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseLeave += OnMouseLeave;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            _renderer.HandleMouseMove(new MouseMoveEventArgs((float)pos.X, (float)pos.Y, 0f, 0f));
        }

        public void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                _renderer.HandleMouseDown(new OpenTK.Windowing.Common.MouseButtonEventArgs(MouseButton.Right, InputAction.Press, 0));
                CaptureMouse();
            }
        }

        public void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Released)
            {
                _renderer.HandleMouseUp(new OpenTK.Windowing.Common.MouseButtonEventArgs(MouseButton.Right, InputAction.Release, 0));
                ReleaseMouseCapture();
            }
        }

        public void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _renderer.HandleMouseLeave();
            ReleaseMouseCapture();
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            _renderer.HandleKeyDown(new KeyboardKeyEventArgs(MapKey(e.Key), 0, MapKeyModifiers(Keyboard.Modifiers), e.IsRepeat));
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            _renderer.HandleKeyUp(new KeyboardKeyEventArgs(MapKey(e.Key), 0, MapKeyModifiers(Keyboard.Modifiers), e.IsRepeat));
        }

        private Keys MapKey(Key key)
        {
            switch (key)
            {
                case Key.W: return Keys.W;
                case Key.A: return Keys.A;
                case Key.S: return Keys.S;
                case Key.D: return Keys.D;
                case Key.Space: return Keys.Space;
                case Key.LeftShift: return Keys.LeftShift;
                default: return Keys.Unknown;
            }
        }

        private KeyModifiers MapKeyModifiers(ModifierKeys modifiers)
        {
            KeyModifiers result = 0;
            if ((modifiers & ModifierKeys.Alt) != 0) result |= KeyModifiers.Alt;
            if ((modifiers & ModifierKeys.Control) != 0) result |= KeyModifiers.Control;
            if ((modifiers & ModifierKeys.Shift) != 0) result |= KeyModifiers.Shift;
            return result;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = new NativeWindowSettings
            {
                Size = new Vector2i((int)ActualWidth, (int)ActualHeight),
                WindowBorder = WindowBorder.Hidden,
                StartVisible = false,
                StartFocused = false,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3)
            };

            try
            {
                _gameWindow = new GameWindow(gameWindowSettings, nativeWindowSettings);
                _gameWindow.MakeCurrent();
                Debug.WriteLine("OpenGL context created successfully");
                Debug.WriteLine($"OpenGL version: {GL.GetString(StringName.Version)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create OpenGL context: {ex.Message}");
                return;
            }

            _renderer = new Renderer(_gameWindow);
            _renderer.Initialize();

            SetupFramebuffer();

            _writeableBitmap = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
            _image.Source = _writeableBitmap;

            _renderTimer = new DispatcherTimer();
            _renderTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _renderTimer.Tick += OnRenderTimer;
            _renderTimer.Start();
        }

        private void SetupFramebuffer()
        {
            _framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

            _texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)ActualWidth, (int)ActualHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _texture, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_gameWindow != null)
            {
                _gameWindow.Size = new Vector2i((int)ActualWidth, (int)ActualHeight);
                _renderer?.HandleResize(new ResizeEventArgs((int)ActualWidth, (int)ActualHeight));

                _writeableBitmap = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
                _image.Source = _writeableBitmap;

                SetupFramebuffer();
            }
        }
        
        public void Invalidate()
        {
            _gameWindow.MakeCurrent();
            _renderer.RenderFrame(new FrameEventArgs());
            _gameWindow.SwapBuffers();
        }

        public void LoadEnvironment(EnvironmentLoader.EnvironmentData environmentData)
        {
            Debug.WriteLine($"OpenGLControl.LoadEnvironment called with {environmentData.Vertices.Count} vertices and {environmentData.Indices.Count} indices");
            _renderer.LoadEnvironmentData(environmentData);
            Debug.WriteLine("Environment data passed to Renderer");
        }

        private void OnRenderTimer(object sender, EventArgs e)
        {
            Debug.WriteLine("Render timer tick");
            _gameWindow.MakeCurrent();

            GL.Viewport(0, 0, (int)ActualWidth, (int)ActualHeight);
            Debug.WriteLine($"Viewport set to {ActualWidth}x{ActualHeight}");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
            _renderer.RenderFrame(new FrameEventArgs());
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.BindTexture(TextureTarget.Texture2D, _texture);
            byte[] pixels = new byte[(int)ActualWidth * (int)ActualHeight * 4];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            _writeableBitmap.Lock();
            Marshal.Copy(pixels, 0, _writeableBitmap.BackBuffer, pixels.Length);
            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, _writeableBitmap.PixelWidth, _writeableBitmap.PixelHeight));
            _writeableBitmap.Unlock();

            _image.InvalidateVisual();

            Debug.WriteLine($"Copied {pixels.Length} bytes to WriteableBitmap");
        }
    }
}