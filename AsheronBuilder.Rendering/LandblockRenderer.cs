// File: AsheronBuilder.Rendering/LandblockRenderer.cs

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using AsheronBuilder.Core.Landblock;

namespace AsheronBuilder.Rendering
{
    public class LandblockRenderer
    {
        private Shader _shader;
        private int _vao;
        private int _vbo;
        private int _ebo;

        public LandblockRenderer(Shader shader)
        {
            _shader = shader;
            InitializeBuffers();
        }

        private void InitializeBuffers()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        public void Render(Landblock landblock, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            UpdateBuffers(landblock);

            _shader.Use();
            _shader.SetMatrix4("view", viewMatrix);
            _shader.SetMatrix4("projection", projectionMatrix);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, (8 * 8 * 6), DrawElementsType.UnsignedInt, 0);
        }

        private void UpdateBuffers(Landblock landblock)
        {
            float[] vertices = new float[9 * 9 * 3];
            uint[] indices = new uint[8 * 8 * 6];

            int vertexIndex = 0;
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Vector3 vertex = landblock.HeightMap[x, y];
                    vertices[vertexIndex++] = vertex.X;
                    vertices[vertexIndex++] = vertex.Y;
                    vertices[vertexIndex++] = vertex.Z;
                }
            }

            int indexIndex = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    uint topLeft = (uint)(x * 9 + y);
                    uint topRight = (uint)(x * 9 + y + 1);
                    uint bottomLeft = (uint)((x + 1) * 9 + y);
                    uint bottomRight = (uint)((x + 1) * 9 + y + 1);

                    indices[indexIndex++] = topLeft;
                    indices[indexIndex++] = bottomLeft;
                    indices[indexIndex++] = topRight;

                    indices[indexIndex++] = topRight;
                    indices[indexIndex++] = bottomLeft;
                    indices[indexIndex++] = bottomRight;
                }
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
        }
    }
}