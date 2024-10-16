// AsheronBuilder.Rendering/OpenGLControl.cs

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using AsheronBuilder.Core.Dungeon;
using AsheronBuilder.Core.Landblock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AsheronBuilder.UI.Utils;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace AsheronBuilder.Rendering
{
    public partial class OpenGLControl : GLWpfControl
    {
        private LandblockRenderer _landblockRenderer;
        private Landblock _currentLandblock;
        private bool _isInitialized = false;
        private string _initializationError = null;
        private DungeonLayout _currentDungeon;
        private bool _showGrid = false;
        private bool _wireframeMode = false;
        private bool _showCollision = false;
        private Camera _camera;
        private Shader _shader;
        private Shader _pickingShader;
        private Dictionary<uint, (int VAO, int VBO, int EBO)> _envCellMeshes;
        private int _pickingFramebuffer;
        private int _pickingTexture;
        private uint _selectedObjectId = 0;
        private bool _isRightMouseDown = false;
        private Point _lastMousePos;
        private HashSet<Key> _pressedKeys = new HashSet<Key>();
        
        public Camera Camera => _camera;

        public OpenGLControl() : base()
        {
            GLWpfControlSettings settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3,
                GraphicsProfile = OpenTK.Windowing.Common.ContextProfile.Core
            };
            Start(settings);

            _camera = new Camera(new Vector3(0, 5, 10), 1.0f); // set the aspect ratio later
            _envCellMeshes = new Dictionary<uint, (int, int, int)>();
            Render += OnRender;
            SizeChanged += OpenGLControl_SizeChanged;
            MouseDown += OpenGLControl_MouseDown;
            MouseUp += OpenGLControl_MouseUp;
            MouseMove += OpenGLControl_MouseMove;
            KeyDown += OpenGLControl_KeyDown;
            KeyUp += OpenGLControl_KeyUp;
        }
        
        private void OpenGLControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                _isRightMouseDown = true;
                _lastMousePos = e.GetPosition(this);
                CaptureMouse();
                Cursor = Cursors.None;
            }
        }

        private void OpenGLControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Released)
            {
                _isRightMouseDown = false;
                ReleaseMouseCapture();
                Cursor = Cursors.Arrow;
            }
        }

        private void OpenGLControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isRightMouseDown)
            {
                var currentPos = e.GetPosition(this);
                float deltaX = (float)(currentPos.X - _lastMousePos.X);
                float deltaY = (float)(currentPos.Y - _lastMousePos.Y);
                _camera.Yaw += deltaX * 0.1f;
                _camera.Pitch += deltaY * 0.1f; // Inverted looking up and down
                _lastMousePos = currentPos;
            }
        }

        private void OpenGLControl_KeyDown(object sender, KeyEventArgs e)
        {
            _pressedKeys.Add(e.Key);
        }

        private void OpenGLControl_KeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.Key);
        }
        
        private void OpenGLControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _camera.AspectRatio = (float)(ActualWidth / ActualHeight);
        }
        
        public void SetLandblock(Landblock landblock)
        {
            _currentLandblock = landblock;
        }

        public Landblock GetCurrentLandblock()
        {
            return _currentLandblock;
        }

        private void RenderLandblock()
        {
            if (_currentLandblock != null)
            {
                _landblockRenderer.Render(_currentLandblock, _camera.GetViewMatrix(), _camera.GetProjectionMatrix());
            }
        }

        private void OnRender(TimeSpan obj)
        {
            if (!_isInitialized)
            {
                try
                {
                    InitializeOpenGL();
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    _initializationError = ex.Message;
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        MessageBox.Show($"Error initializing OpenGL: {_initializationError}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                    return;
                }
            }

            if (_initializationError != null)
            {
                // Render an error message or blank screen
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                return;
            }

            // Your normal rendering code here
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            var view = _camera.GetViewMatrix();
            var projection = _camera.GetProjectionMatrix();

            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);
            
            RenderGrid(); // Always render the grid
            UpdateCameraPosition();

            if (_currentDungeon != null)
            {
                RenderDungeon(_currentDungeon);
            }

            if (_showGrid)
            {
                RenderGrid();
            }
        }

        private void InitializeOpenGL()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _shader = new Shader("shader.vert", "shader.frag");
            _pickingShader = new Shader("picking.vert", "picking.frag");

            InitializePickingResources();
        }

        private void InitializePickingResources()
        {
            _pickingFramebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _pickingFramebuffer);

            _pickingTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _pickingTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32ui, (int)ActualWidth, (int)ActualHeight, 0, PixelFormat.RedInteger, PixelType.UnsignedInt, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _pickingTexture, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void RenderDungeon(DungeonLayout dungeon)
        {
            foreach (var envCell in dungeon.GetAllEnvCells())
            {
                if (!_envCellMeshes.ContainsKey(envCell.EnvironmentId))
                {
                    LoadEnvCellMesh(envCell.EnvironmentId);
                }

                var (vao, _, _) = _envCellMeshes[envCell.EnvironmentId];

                var model = Matrix4.CreateScale(envCell.Scale) *
                            Matrix4.CreateFromQuaternion(envCell.Rotation) *
                            Matrix4.CreateTranslation(envCell.Position);

                _shader.SetMatrix4("model", model);

                GL.BindVertexArray(vao);
                GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
            }
        }

        private void LoadEnvCellMesh(uint environmentId)
        {
            // This is a placeholder. In a real implementation, you'd load the actual mesh data for the environment.
            float[] vertices = {
                // Front face
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
                // Back face
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f,
            };

            uint[] indices = {
                0, 1, 2, 2, 3, 0,
                1, 5, 6, 6, 2, 1,
                5, 4, 7, 7, 6, 5,
                4, 0, 3, 3, 7, 4,
                3, 2, 6, 6, 7, 3,
                4, 5, 1, 1, 0, 4
            };

            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _envCellMeshes[environmentId] = (vao, vbo, ebo);
        }

        private void RenderGrid()
        {
            _shader.Use();
            _shader.SetMatrix4("model", Matrix4.Identity);

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            List<float> gridVertices = new List<float>();
            for (int i = -10; i <= 10; i++)
            {
                gridVertices.AddRange(new[] { i, 0f, -10f, 0.5f, 0.5f, 0.5f });
                gridVertices.AddRange(new[] { i, 0f, 10f, 0.5f, 0.5f, 0.5f });
                gridVertices.AddRange(new[] { -10f, 0f, i, 0.5f, 0.5f, 0.5f });
                gridVertices.AddRange(new[] { 10f, 0f, i, 0.5f, 0.5f, 0.5f });
            }

            GL.BufferData(BufferTarget.ArrayBuffer, gridVertices.Count * sizeof(float), gridVertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.DrawArrays(PrimitiveType.Lines, 0, gridVertices.Count / 6);

            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
        }
        
        public void SetGridVisibility(bool isVisible)
        {
            _showGrid = isVisible;
        }

        public void SetDungeonLayout(DungeonLayout dungeonLayout)
        {
            _currentDungeon = dungeonLayout;
        }

        public void ToggleGrid()
        {
            _showGrid = !_showGrid;
        }

        public void SetWireframeMode(bool enabled)
        {
            _wireframeMode = enabled;
            GL.PolygonMode(MaterialFace.FrontAndBack, _wireframeMode ? PolygonMode.Line : PolygonMode.Fill);
        }

        public void SetCollisionVisibility(bool visible)
        {
            _showCollision = visible;
        }
        

        public void RotateCamera(float yaw, float pitch)
        {
            _camera.Yaw += yaw;
            _camera.Pitch += pitch;
            _camera.UpdateVectors();
        }

        public void ZoomCamera(float zoomFactor)
        {
            _camera.Position += _camera.Front * zoomFactor;
        }

        public EnvCell PickObject(Vector2 mousePosition)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _pickingFramebuffer);
            GL.Viewport(0, 0, (int)ActualWidth, (int)ActualHeight);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _pickingShader.Use();
            _pickingShader.SetMatrix4("view", _camera.GetViewMatrix());
            _pickingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            foreach (var envCell in _currentDungeon.GetAllEnvCells())
            {
                var model = Matrix4.CreateScale(envCell.Scale) *
                            Matrix4.CreateFromQuaternion(envCell.Rotation) *
                            Matrix4.CreateTranslation(envCell.Position);

                _pickingShader.SetMatrix4("model", model);
                _pickingShader.SetUInt("objectId", envCell.Id);

                var (vao, _, _) = _envCellMeshes[envCell.EnvironmentId];
                GL.BindVertexArray(vao);
                GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
            }

            uint[] pixelData = new uint[1];
            GL.ReadPixels((int)mousePosition.X, (int)(ActualHeight - mousePosition.Y), 1, 1, PixelFormat.RedInteger, PixelType.UnsignedInt, pixelData);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            _selectedObjectId = pixelData[0];
            return _currentDungeon.GetEnvCellById(_selectedObjectId);
        }

        private void UpdateCameraPosition()
        {
            Vector3 movement = Vector3.Zero;

            if (_pressedKeys.Contains(Key.W)) movement += _camera.Front;
            if (_pressedKeys.Contains(Key.S)) movement -= _camera.Front;
            if (_pressedKeys.Contains(Key.A)) movement -= _camera.Right;
            if (_pressedKeys.Contains(Key.D)) movement += _camera.Right;
            if (_pressedKeys.Contains(Key.Space)) movement += Vector3.UnitY;
            if (_pressedKeys.Contains(Key.LeftShift)) movement -= Vector3.UnitY;

            if (movement != Vector3.Zero)
            {
                movement.Normalize();
                _camera.Position += movement * _camera.MovementSpeed;
            }
        }

        public void HandleResize(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            _camera.AspectRatio = width / (float)height;
        }

        public void Dispose()
        {
            foreach (var (vao, vbo, ebo) in _envCellMeshes.Values)
            {
                GL.DeleteVertexArray(vao);
                GL.DeleteBuffer(vbo);
                GL.DeleteBuffer(ebo);
            }

            GL.DeleteFramebuffer(_pickingFramebuffer);
            GL.DeleteTexture(_pickingTexture);

            _shader.Dispose();
            _pickingShader.Dispose();
        }

        public void Invalidate()
        {
            throw new NotImplementedException();
        }
    }

    public static class PointExtensions
    {
        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
    }
}