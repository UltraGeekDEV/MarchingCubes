using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Concurrent;
using MarchingCubes.Utils;
using MarchingCubes.Shaders;

namespace MarchingCubes
{
    internal class Program
    {
        public static ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();
        static void Main(string[] args)
        {
            DebugUtils.debugOutput = Console.WriteLine;

            Materials.materials[MaterialID.Solid] = new Shader("SolidMesh");


            var nativeSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Marching Cubes Demo"
            };

            using (var window = new GameWindow(GameWindowSettings.Default, nativeSettings))
            {
                // Simple triangle vertices
                float[] vertices = {
                -0.5f, -0.5f, 0.0f,
                 0.5f, -0.5f, 0.0f,
                 0.0f,  0.5f, 0.0f
            };

                int vao = GL.GenVertexArray();
                int vbo = GL.GenBuffer();

                GL.BindVertexArray(vao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);

                var shader = Materials.materials[MaterialID.Solid];

                window.RenderFrame += (FrameEventArgs args) =>
                {
                    while(actionQueue.TryDequeue(out var action))
                    {
                        action?.Invoke();
                    }


                    GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    shader.SetActive();
                    GL.BindVertexArray(vao);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

                    window.SwapBuffers();
                };

                window.Run();
            }
        }
    }
}
