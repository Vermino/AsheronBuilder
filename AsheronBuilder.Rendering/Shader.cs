using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AsheronBuilder.Rendering
{
    public class Shader
    {
        public readonly int Handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexShaderSource;
            string fragmentShaderSource;

            try
            {
                using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(vertexPath))
                {
                    if (stream == null)
                        throw new InvalidOperationException($"Could not load vertex shader resource: {vertexPath}");
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        vertexShaderSource = reader.ReadToEnd();
                    }
                }

                using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fragmentPath))
                {
                    if (stream == null)
                        throw new InvalidOperationException($"Could not load fragment shader resource: {fragmentPath}");
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        fragmentShaderSource = reader.ReadToEnd();
                    }
                }
            
                int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
                int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

                Handle = GL.CreateProgram();

                GL.AttachShader(Handle, vertexShader);
                GL.AttachShader(Handle, fragmentShader);

                LinkProgram(Handle);

                GL.DetachShader(Handle, vertexShader);
                GL.DetachShader(Handle, fragmentShader);
                GL.DeleteShader(fragmentShader);
                GL.DeleteShader(vertexShader);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading shaders: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private static int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Debug.WriteLine($"Error compiling {type} shader: {infoLog}");
            }

            return shader;
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Debug.WriteLine($"Error linking shader program: {infoLog}");
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
            Debug.WriteLine($"Shader program {Handle} in use");
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            var location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, true, ref data);
            Debug.WriteLine($"Set matrix4 uniform '{name}' at location {location}");
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(location, data);
            Debug.WriteLine($"Set vector3 uniform '{name}' at location {location}");
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, data);
            Debug.WriteLine($"Set float uniform '{name}' at location {location}");
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, data);
            Debug.WriteLine($"Set int uniform '{name}' at location {location}");
        }
    }
}
            