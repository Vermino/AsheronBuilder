using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Windows.Threading;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace ACDungeonBuilder.Rendering
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
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i((int)ActualWidth, (int)ActualHeight),
                WindowBorder = WindowBorder.Hidden,
                StartVisible = false,
                StartFocused = false,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3)
            };

            _gameWindow = new GameWindow(gameWindowSettings, nativeWindowSettings);
            _gameWindow.MakeCurrent();

            _renderer = new Renderer(_gameWindow);
            _renderer.Run();

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
                _renderer?.OnResize(new ResizeEventArgs((int)ActualWidth, (int)ActualHeight));

                _writeableBitmap = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
                _image.Source = _writeableBitmap;

                SetupFramebuffer();
            }
        }

        private void OnRenderTimer(object sender, EventArgs e)
        {
            _gameWindow.MakeCurrent();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
            _renderer.OnRenderFrame();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.BindTexture(TextureTarget.Texture2D, _texture);
            byte[] pixels = new byte[(int)ActualWidth * (int)ActualHeight * 4];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            _writeableBitmap.Lock();
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, _writeableBitmap.BackBuffer, pixels.Length);
            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, _writeableBitmap.PixelWidth, _writeableBitmap.PixelHeight));
            _writeableBitmap.Unlock();
        }
    }
}