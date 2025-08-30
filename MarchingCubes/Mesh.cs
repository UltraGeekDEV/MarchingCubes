using MarchingCubes.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarchingCubes
{
    public class Mesh
    {
        int triCount;
        int VAO;
        int VBO;

        Shader material;

        public Mesh(MaterialID material,List<Triangle> tris) 
        {
            var vertices = tris.SelectMany(tri => new Vector3[] { tri.a, tri.b, tri.c }).SelectMany(vert => new float[] { vert.X, vert.Y, vert.Z }).ToArray();
            this.material = Materials.materials[material];
            triCount = tris.Count * 3;

            Program.actionQueue.Enqueue(() =>
            {
                VAO = GL.GenVertexArray();
                VBO = GL.GenBuffer();

                GL.BindVertexArray(VAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
            });
        }

        public void Draw()
        {
            material.SetActive();
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, triCount);
        }
    }
}
