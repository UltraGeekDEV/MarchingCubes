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
            Init();

            var Cube = new Mesh(MaterialID.Solid, GetCube());

            var nativeSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Marching Cubes Demo"
            };

            using (var window = new GameWindow(GameWindowSettings.Default, nativeSettings))
            {

                window.RenderFrame += (FrameEventArgs args) =>
                {
                    while(actionQueue.TryDequeue(out var action))
                    {
                        action?.Invoke();
                    }


                    GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    Cube.Draw();

                    window.SwapBuffers();
                };

                window.Run();
            }
        }

        private static void Init()
        {
            DebugUtils.debugOutput = Console.WriteLine;

            Materials.materials[MaterialID.Solid] = new Shader("SolidMesh");
        }

        static List<Triangle> GetCube()
        {
            List<Triangle> cube = new List<Triangle>();

            Vector3[] v = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f)
            };

            // Front face
            cube.Add(new Triangle(v[0], v[1], v[2]));
            cube.Add(new Triangle(v[2], v[3], v[0]));

            // Back face
            cube.Add(new Triangle(v[5], v[4], v[7]));
            cube.Add(new Triangle(v[7], v[6], v[5]));

            // Left face
            cube.Add(new Triangle(v[4], v[0], v[3]));
            cube.Add(new Triangle(v[3], v[7], v[4]));

            // Right face
            cube.Add(new Triangle(v[1], v[5], v[6]));
            cube.Add(new Triangle(v[6], v[2], v[1]));

            // Top face
            cube.Add(new Triangle(v[3], v[2], v[6]));
            cube.Add(new Triangle(v[6], v[7], v[3]));

            // Bottom face
            cube.Add(new Triangle(v[4], v[5], v[1]));
            cube.Add(new Triangle(v[1], v[0], v[4]));

            return cube;
        }
    }
}
