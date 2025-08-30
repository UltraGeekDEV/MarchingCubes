using MarchingCubes.Utils;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MarchingCubes
{
    public class Shader
    {
        int shaderProgram;

        public Shader(string vertexShaderName, string fragmentShaderName)
        {
            string vertexShader;
            string fragmentShader;

            int vertexID;
            int fragmentID;

            vertexShader = File.ReadAllText($"./Shaders/{vertexShaderName}/{vertexShaderName}.vert");
            fragmentShader = File.ReadAllText($"./Shaders/{fragmentShaderName}/{fragmentShaderName}.frag");

            Program.actionQueue.Enqueue(() =>
            {
                vertexID = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexID, vertexShader);
                GL.CompileShader(vertexID);
                DebugUtils.GetGLError($"{vertexShaderName} Vertex shader");

                fragmentID = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentID, fragmentShader);
                GL.CompileShader(fragmentID);
                DebugUtils.GetGLError($"{fragmentShaderName} Fragment shader");

                shaderProgram = GL.CreateProgram();

                GL.AttachShader(shaderProgram, vertexID);
                GL.AttachShader(shaderProgram, fragmentID);
                GL.LinkProgram(shaderProgram);
                DebugUtils.GetGLError($"{fragmentShaderName} Linking Shader Program");

                GL.DeleteShader(fragmentID);
                GL.DeleteShader(vertexID);
            });
        }
        public Shader(string ShaderName): this(ShaderName, ShaderName)
        { }
        public void SetActive()
        {
            GL.UseProgram(shaderProgram);
        }
    }
}
