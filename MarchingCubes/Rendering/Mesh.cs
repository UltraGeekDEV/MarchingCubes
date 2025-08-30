using MarchingCubes.Shaders;
using MarchingCubes.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MarchingCubes.Rendering
{
    public class Mesh
    {
        public Matrix4 transform;

        int triCount;
        int VAO;
        int VBO;

        Shader material;

        public Mesh(MaterialID material, List<Triangle> tris)
        {
            transform = Matrix4.Identity;
            var vertices = tris.SelectMany(tri =>
            {
                var normal = Vector3.Cross(tri.b - tri.a, tri.c - tri.a).Normalized();
                return new Vector3[] { tri.a, normal, tri.b, normal, tri.c, normal };
            }).SelectMany(vert => new float[] { vert.X, vert.Y, vert.Z }).ToArray();
            this.material = Materials.materials[material];
            triCount = tris.Count * 3;

            Program.actionQueue.Enqueue(() =>
            {
                VAO = GL.GenVertexArray();
                VBO = GL.GenBuffer();

                GL.BindVertexArray(VAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
            });
        }

        public void Draw(Camera camera)
        {
            material.SetActive();
            
            var transformLoc = GL.GetUniformLocation(material.shaderProgram, "transform");
            GL.UniformMatrix4(transformLoc, false, ref transform);
            DebugUtils.GetGLError("Draw Mesh Set Transform");

            var projectionLoc = GL.GetUniformLocation(material.shaderProgram, "projection");
            GL.UniformMatrix4(projectionLoc, false, ref camera.perspective);
            DebugUtils.GetGLError("Draw Mesh Set Projection");

            var viewLoc = GL.GetUniformLocation(material.shaderProgram, "view");
            GL.UniformMatrix4(viewLoc, false, ref camera.transform);
            DebugUtils.GetGLError("Draw Mesh Set View");

            GL.BindVertexArray(VAO);

            GL.DrawArrays(PrimitiveType.Triangles, 0, triCount);
        }
    }
}
