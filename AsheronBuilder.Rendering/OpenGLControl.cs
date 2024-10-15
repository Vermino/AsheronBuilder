// File: AsheronBuilder.Rendering/OpenGLControl.cs

using System;
using System.Collections.Generic;
using System.Windows;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using AsheronBuilder.Core.Dungeon;
using AsheronBuilder.Core.Landblock;

namespace AsheronBuilder.Rendering
{
    public partial class OpenGLControl : GLWpfControl
    {
        private LandblockRenderer _landblockRenderer;
        private Landblock _currentLandblock;
        private bool _isInitialized = false;
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

        public OpenGLControl() : base(new GLWpfControlSettings
        {
            MajorVersion = 3,
            MinorVersion = 3
        })
        {
            _camera = new Camera(new Vector3(0, 5, 10), Vector3.UnitY);
            _envCellMeshes = new Dictionary<uint, (int, int, int)>();
            Render += OnRender;
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
                InitializeOpenGL();
                _isInitialized = true;
                RenderLandblock();
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            var view = _camera.GetViewMatrix();
            var projection = _camera.GetProjectionMatrix();

            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);

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

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _pickingShader = new Shader("Shaders/picking.vert", "Shaders/picking.frag");

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
            _shader.SetMatrix4("model", Matrix4.Identity);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(0.5f, 0.5f, 0.5f);

            for (int i = -10; i <= 10; i++)
            {
                GL.Vertex3(i, 0, -10);
                GL.Vertex3(i, 0, 10);
                GL.Vertex3(-10, 0, i);
                GL.Vertex3(10, 0, i);
            }

            GL.End();
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

        public void ResetCamera()
        {
            _camera.Position = new Vector3(0, 5, 10);
            _camera.Yaw = -90f;
            _camera.Pitch = 0f;
            _camera.UpdateVectors();
        }

        public void SetTopView()
        {
            _camera.Position = new Vector3(0, 20, 0);
            _camera.Pitch = -90f;
            _camera.Yaw = -90f;
            _camera.UpdateVectors();
        }

        public void MoveCamera(Vector3 movement)
        {
            _camera.Position += movement;
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
    }

    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Front { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }
        public float Yaw { get; set; } = -90f;
        public float Pitch { get; set; } = 0f;

        private float _aspectRatio = 1.0f;

        public Camera(Vector3 position, Vector3 up)
        {
            Position = position;
            Up = up;
            UpdateVectors();
        }

        public void UpdateVectors()
        {
            Front.X = (float)Math.Cos(MathHelper.DegreesToRadians(Yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(Pitch));
            Front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
            Front.Z = (float)Math.Sin(MathHelper.DegreesToRadians(Yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(Pitch));
            Front = Vector3.Normalize(Front);

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45.0f),
                _aspectRatio,
                0.1f,
                100.0f);
        }

        public void SetAspectRatio(float aspectRatio)
        {
            _aspectRatio = aspectRatio;
        }
    }

    public class Shader
    {
        public int Handle { get; private set; }

        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexShaderSource = System.IO.File.ReadAllText(vertexPath);
            string fragmentShaderSource = System.IO.File.ReadAllText(fragmentPath);

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling shader: {infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Error linking program: {infoLog}");
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void SetVector3(string name, Vector3 vector)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(location, vector);
        }

        public void SetFloat(string name, float value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }

        public void SetUInt(string name, uint value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }
    }
}