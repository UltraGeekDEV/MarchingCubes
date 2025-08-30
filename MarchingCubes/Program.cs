using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Concurrent;
using MarchingCubes.Utils;
using MarchingCubes.Shaders;
using MarchingCubes.Rendering;
using System.Drawing;
using MarchingCubes.CPUMarchingCubes;

namespace MarchingCubes
{
    internal class Program
    {
        public static ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();
        public static List<Mesh> SolidMeshes = new List<Mesh>();
        static void Main(string[] args)
        {
            Init();

            var mesh = new Mesh(MaterialID.Solid, CPUMesher.GetTriangles(64));

            SolidMeshes.Add(mesh);

            var camera = new Camera();
            Vector3 cameraPos = new Vector3(1f, 1f, 0);
            camera.transform = Matrix4.LookAt(cameraPos, Vector3.Zero, Vector3.UnitY);

            var nativeSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Marching Cubes Demo"
            };

            using (var window = new GameWindow(GameWindowSettings.Default, nativeSettings))
            {
                window.UpdateFrequency = 60;
                window.Resize += (ResizeEventArgs) =>
                {
                    GL.Viewport(0,0,ResizeEventArgs.Width,ResizeEventArgs.Height);
                    camera.SetPerspective(60,ResizeEventArgs.Size.X/(float)ResizeEventArgs.Size.Y);
                };
                window.RenderFrame += (FrameEventArgs args) =>
                {
                    //mesh.transform = Matrix4.CreateRotationY((((DateTime.Now.Second + DateTime.Now.Millisecond / 1000.0f) / 60.0f * 4.0f) % 1.0f) *2* MathF.PI);

                    while (actionQueue.TryDequeue(out var action))
                    {
                        action?.Invoke();
                    }
                    GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);
                    GL.Enable(EnableCap.DepthTest);
                    GL.CullFace(TriangleFace.Back);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


                    foreach (var mesh in SolidMeshes)
                    {
                        mesh.Draw(camera);
                    }

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
