using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ACDungeonBuilder.Core.Assets;
using ACDungeonBuilder.Core.Dungeon;
using System;
using System.Linq;
using System.Diagnostics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ACDungeonBuilder.Rendering
{
    public class Renderer
    {
        private GameWindow _gameWindow;
        private Shader _shader;
        private DungeonLayout _dungeonLayout;
        private Camera _camera;
        private int _vao;
        private int _vbo;
        private int _ebo;
        private EnvironmentLoader.EnvironmentData _environmentData;
        private int _axesVao;
        private int _axesVbo;
        private int _planeVAO;
        private int _planeVBO;
        private int _planeEBO;
        private float _cameraSpeed = 0.05f;
        private bool _isRightMouseDown = false;
        private Vector2 _lastMousePosition;

        // Debug rendering
        private int _debugVao;
        private int _debugVbo;

        public Renderer(GameWindow gameWindow)
        {
            _gameWindow = gameWindow;
            _camera = new Camera(new Vector3(0, 5, 10), _gameWindow.Size.X / (float)_gameWindow.Size.Y);
            _camera.Front = -Vector3.Normalize(_camera.Position); // Look at the origin
        }

        public void Run()
        {
            try
            {
                GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f); // Change this to a lighter color
                GL.Enable(EnableCap.DepthTest);

                _shader = new Shader("ACDungeonBuilder.Rendering.Shaders.shader.vert",
                    "ACDungeonBuilder.Rendering.Shaders.shader.frag");
                Debug.WriteLine("Shader compiled successfully");

                SetupDebugRendering();
                SetupAxes();
                SetupPlane();
                Debug.WriteLine("Debug rendering and axes set up successfully");

                _environmentData = new EnvironmentLoader.EnvironmentData(); // Initialize with empty data
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Renderer.Run: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void SetupPlane()
        {
            float[] vertices =
            {
                -5.0f, 0.0f, -5.0f,
                5.0f, 0.0f, -5.0f,
                5.0f, 0.0f, 5.0f,
                -5.0f, 0.0f, 5.0f
            };

            uint[] indices =
            {
                0, 1, 2,
                2, 3, 0
            };

            GL.GenVertexArrays(1, out _planeVAO);
            GL.BindVertexArray(_planeVAO);

            GL.GenBuffers(1, out _planeVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _planeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out _planeEBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _planeEBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices,
                BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        private void SetupAxes()
        {
            float[] axesVertices =
            {
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, // X-axis start (red)
                5.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, // X-axis end (red)
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, // Y-axis start (green)
                0.0f, 5.0f, 0.0f, 0.0f, 1.0f, 0.0f, // Y-axis end (green)
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, // Z-axis start (blue)
                0.0f, 0.0f, 5.0f, 0.0f, 0.0f, 1.0f // Z-axis end (blue)
            };

            GL.GenVertexArrays(1, out _axesVao);
            GL.BindVertexArray(_axesVao);
            GL.DrawArrays(PrimitiveType.Lines, 0, 6);

            GL.GenBuffers(1, out _axesVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _axesVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, axesVertices.Length * sizeof(float), axesVertices,
                BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        public void LoadEnvironment(EnvironmentLoader.EnvironmentData environmentData)
        {
            _environmentData = environmentData;
            Debug.WriteLine($"Loading environment with {_environmentData.Vertices.Count} vertices and {_environmentData.Indices.Count} indices");

            // Calculate bounding box
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            foreach (var vertex in _environmentData.Vertices)
            {
                min = Vector3.ComponentMin(min, new Vector3(vertex.X, vertex.Y, vertex.Z));
                max = Vector3.ComponentMax(max, new Vector3(vertex.X, vertex.Y, vertex.Z));
            }

            Vector3 center = (min + max) / 2f;
            float scale = 10f / (max - min).Length; // Scale to fit in a 10-unit cube

            // Transform vertices
            for (int i = 0; i < _environmentData.Vertices.Count; i++)
            {
                var v = _environmentData.Vertices[i];
                _environmentData.Vertices[i] = new System.Numerics.Vector3(
                    (v.X - center.X) * scale,
                    (v.Y - center.Y) * scale,
                    (v.Z - center.Z) * scale
                );
            }

            SetupEnvironmentBuffers();
        }

        private void SetupEnvironmentBuffers()
        {
            GL.GenVertexArrays(1, out _vao);
            GL.BindVertexArray(_vao);

            GL.GenBuffers(1, out _vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            float[] vertexData = _environmentData.Vertices.SelectMany(v => new[] { v.X, v.Y, v.Z }).ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out _ebo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _environmentData.Indices.Count * sizeof(int), _environmentData.Indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        private void SetupDebugRendering()
        {
            float[] vertices =
            {
                -0.5f, -0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                0.0f, 0.5f, 0.0f
            };

            GL.GenVertexArrays(1, out _debugVao);
            GL.BindVertexArray(_debugVao);

            GL.GenBuffers(1, out _debugVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _debugVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        public void OnRenderFrame()
        {
            Debug.WriteLine("OnRenderFrame called");
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            // Draw axes with thicker lines and different colors
            GL.LineWidth(3.0f);
            GL.BindVertexArray(_axesVao);
            GL.DrawArrays(PrimitiveType.Lines, 0, 6);
            Debug.WriteLine("Axes drawn");

            // Draw plane with a different color
            GL.BindVertexArray(_planeVAO);
            _shader.SetVector3("color", new Vector3(0.5f, 0.5f, 0.5f)); // Grey color for the plane
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            Debug.WriteLine("Plane drawn");

            // Draw environment if available
            if (_environmentData != null && _environmentData.Indices.Count > 0)
            {
                GL.BindVertexArray(_vao);
                _shader.SetVector3("color", new Vector3(1.0f, 1.0f, 1.0f)); // White color for the environment
                GL.DrawElements(PrimitiveType.Lines, _environmentData.Indices.Count, DrawElementsType.UnsignedInt, 0);
                Debug.WriteLine($"Environment drawn with {_environmentData.Indices.Count} indices");
            }
            else
            {
                Debug.WriteLine("No environment data to render");
            }

            GL.Flush();
            Debug.WriteLine("Frame rendered and flushed");
        }


        public void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _camera.AspectRatio = e.Width / (float)e.Height;
        }

        public void HandleKeyUp(KeyboardKeyEventArgs e)
        {
            // Handle key up events if needed
        }

        public void HandleMouseMove(MouseMoveEventArgs e)
        {
            if (_isRightMouseDown)
            {
                float sensitivity = 0.1f;
                float deltaX = e.X - _lastMousePosition.X;
                float deltaY = e.Y - _lastMousePosition.Y;
                _camera.Rotate(deltaX * sensitivity, -deltaY * sensitivity);
            }

            _lastMousePosition = new Vector2(e.X, e.Y);
        }

        public void HandleMouseDown(MouseButton button)
        {
            if (button == MouseButton.Right)
            {
                _isRightMouseDown = true;
            }
        }

        public void HandleMouseUp(MouseButton button)
        {
            if (button == MouseButton.Right)
            {
                _isRightMouseDown = false;
            }
        }

        public void HandleKeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.W:
                    _camera.Move(_camera.Front * _cameraSpeed);
                    break;
                case Keys.S:
                    _camera.Move(-_camera.Front * _cameraSpeed);
                    break;
                case Keys.A:
                    _camera.Move(-Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * _cameraSpeed);
                    break;
                case Keys.D:
                    _camera.Move(Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * _cameraSpeed);
                    break;
            }
        }
    }
}