using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using AsheronBuilder.Core.Assets;
using AsheronBuilder.Core.Dungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AsheronBuilder.Rendering
{
    public class Renderer : IDisposable
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
        private int _gridVao;
        private int _gridVbo;
        private float _gridCellSize = 1.0f;
        private int _gridSize = 20;
        private bool[] _keyStates;
        private Vector2 _lastMousePosition;
        private bool _isFocused = false;
        private bool disposed = false;
        private int _programID;


        
        public Renderer(GameWindow gameWindow)
        {
            _gameWindow = gameWindow;
            _camera = new Camera(new Vector3(0, 5, 10), _gameWindow.Size.X / (float)_gameWindow.Size.Y);
            _keyStates = new bool[Enum.GetValues(typeof(Keys)).Length];
        }

        public void Initialize()
        {
            try
            {
                GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
                GL.Enable(EnableCap.DepthTest);

                _shader = new Shader("AsheronBuilder.Rendering.Shaders.shader.vert",
                    "AsheronBuilder.Rendering.Shaders.shader.frag");
                Debug.WriteLine("Shader compiled successfully");

                SetupDebugRendering();
                SetupAxes();
                SetupGrid();
                Debug.WriteLine("Debug rendering and axes set up successfully");

                _environmentData = new EnvironmentLoader.EnvironmentData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Renderer.Initialize: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void SetupAxes()
        {
            float[] axesVertices =
            {
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                10.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 10.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 10.0f, 0.0f, 0.0f, 1.0f
            };

            GL.GenVertexArrays(1, out _axesVao);
            GL.BindVertexArray(_axesVao);

            GL.GenBuffers(1, out _axesVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _axesVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, axesVertices.Length * sizeof(float), axesVertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }
        
        private void SetupGrid()
        {
            List<float> gridVertices = new List<float>();
    
            for (int i = -_gridSize; i <= _gridSize; i++)
            {
                gridVertices.AddRange(new float[] { i * _gridCellSize, 0, -_gridSize * _gridCellSize, 0.5f, 0.5f, 0.5f });
                gridVertices.AddRange(new float[] { i * _gridCellSize, 0, _gridSize * _gridCellSize, 0.5f, 0.5f, 0.5f });
                gridVertices.AddRange(new float[] { -_gridSize * _gridCellSize, 0, i * _gridCellSize, 0.5f, 0.5f, 0.5f });
                gridVertices.AddRange(new float[] { _gridSize * _gridCellSize, 0, i * _gridCellSize, 0.5f, 0.5f, 0.5f });
            }

            GL.GenVertexArrays(1, out _gridVao);
            GL.BindVertexArray(_gridVao);

            GL.GenBuffers(1, out _gridVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _gridVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, gridVertices.Count * sizeof(float), gridVertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        public void SetGridCellSize(float size)
        {
            _gridCellSize = Math.Clamp(size, 0.1f, 10f);
            SetupGrid();
        }

        public void LoadEnvironmentData(EnvironmentLoader.EnvironmentData environmentData)
        {
            _environmentData = environmentData;
            Debug.WriteLine($"Loading environment with {_environmentData.Vertices.Count} vertices and {_environmentData.Indices.Count} indices");

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            foreach (var vertex in _environmentData.Vertices)
            {
                min = Vector3.ComponentMin(min, new Vector3(vertex.X, vertex.Y, vertex.Z));
                max = Vector3.ComponentMax(max, new Vector3(vertex.X, vertex.Y, vertex.Z));
            }

            Vector3 center = (min + max) / 2f;
            float scale = 10f / (max - min).Length;

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

            int debugVao = GL.GenVertexArray();
            GL.BindVertexArray(debugVao);

            int debugVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, debugVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        public void HandleMouseMove(MouseMoveEventArgs e)
        {
            if (_isFocused)
            {
                float deltaX = e.X - _lastMousePosition.X;
                float deltaY = e.Y - _lastMousePosition.Y;
                _camera.HandleMouseInput(deltaX, deltaY);
            }
            _lastMousePosition = new Vector2(e.X, e.Y);
        }

        public void HandleMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right)
            {
                _camera.SetRightMouseDown(true);
                _isFocused = true;
                _gameWindow.CursorGrabbed = true;
                _gameWindow.CursorVisible = false;
            }
        }

        public void HandleMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right)
            {
                _camera.SetRightMouseDown(false);
                _isFocused = false;
                _gameWindow.CursorGrabbed = false;
                _gameWindow.CursorVisible = true;
            }
        }

        public void HandleMouseLeave()
        {
            _camera.SetRightMouseDown(false);
            _isFocused = false;
            _gameWindow.CursorGrabbed = false;
            _gameWindow.CursorVisible = true;
        }

        public void HandleKeyDown(KeyboardKeyEventArgs e)
        {
            _keyStates[(int)e.Key] = true;
        }

        public void HandleKeyUp(KeyboardKeyEventArgs e)
        {
            _keyStates[(int)e.Key] = false;
        }

        public void HandleResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _camera.AspectRatio = e.Width / (float)e.Height;
        }

        public void RenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            GL.BindVertexArray(_gridVao);
            GL.DrawArrays(PrimitiveType.Lines, 0, _gridSize * 4 + 4);

            GL.BindVertexArray(_axesVao);
            GL.DrawArrays(PrimitiveType.Lines, 0, 6);

            if (_environmentData != null && _environmentData.Indices.Count > 0)
            {
                GL.BindVertexArray(_vao);
                GL.DrawElements(PrimitiveType.Lines, _environmentData.Indices.Count, DrawElementsType.UnsignedInt, 0);
            }

            GL.Flush();
        }

        public void SetDungeonLayout(DungeonLayout dungeonLayout)
        {
            _dungeonLayout = dungeonLayout;
            UpdateEnvironmentData();
        }

        private void UpdateEnvironmentData()
        {
            _environmentData = new EnvironmentLoader.EnvironmentData();
            foreach (var area in _dungeonLayout.Hierarchy.RootArea.GetAllAreas())
            {
                foreach (var envCell in area.EnvCells)
                {
                    AddEnvCellToEnvironmentData(envCell);
                }
            }
            SetupEnvironmentBuffers();
        }

        private void AddEnvCellToEnvironmentData(EnvCell envCell)
        {
            Vector3[] cubeVertices = {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f)
            };
        
            int[] cubeIndices = {
                0, 1, 1, 2, 2, 3, 3, 0,
                4, 5, 5, 6, 6, 7, 7, 4,
                0, 4, 1, 5, 2, 6, 3, 7
            };
        
            int baseIndex = _environmentData.Vertices.Count;

            Matrix4 scaleMatrix = Matrix4.CreateScale(envCell.Scale.X, envCell.Scale.Y, envCell.Scale.Z);
            OpenTK.Mathematics.Quaternion openTKQuaternion = new OpenTK.Mathematics.Quaternion(envCell.Rotation.X, envCell.Rotation.Y, envCell.Rotation.Z, envCell.Rotation.W);
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(openTKQuaternion);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(envCell.Position.X, envCell.Position.Y, envCell.Position.Z);

            Matrix4 transform = scaleMatrix * rotationMatrix * translationMatrix;
        
            foreach (var vertex in cubeVertices)
            { 
                Vector3 transformedVertex = Vector3.TransformPosition(vertex, transform);
                _environmentData.Vertices.Add(new System.Numerics.Vector3(transformedVertex.X, transformedVertex.Y, transformedVertex.Z));
            }
            
            foreach (var index in cubeIndices)
            {
                _environmentData.Indices.Add(baseIndex + index);
            }
        }

        public void UpdateFrame(FrameEventArgs e)
        {
            if (_isFocused)
            {
                _camera.HandleKeyboardInput(_keyStates, (float)e.Time);
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                // Free up the shader resources, e.g., deleting shaders, program, etc.
                GL.DeleteProgram(_programID);
                disposed = true;
            }
        }
    }
}