using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarchingCubes
{
    public static class VoxelMeshGenerator
    {
        public static float[,,] GetVoxelMeshCPU(float resolution)
        {
            var voxelCount = (int)(1.0f / resolution);
            resolution = 1.0f / voxelCount;

            var center = Vector3.One * 0.5f;

            var voxelMesh = new float[voxelCount, voxelCount, voxelCount];

            for (int i = 0; i < voxelCount; i++)
            {
                for (int j = 0; j < voxelCount; j++)
                {
                    for (int k = 0; k < voxelCount; k++)
                    {
                        var vec = new Vector3(i, j, k) * resolution - center;
                        voxelMesh[i, j, k] = (MathF.Sqrt(vec.X * vec.X + vec.Z * vec.Z) < 0.4f) && (MathF.Abs(vec.Y) < 0.4f) ? 1.0f : 0.5f;
                    }
                }
            }


            return voxelMesh;
        }
        static float CylinderSDF(Vector3 p, float radius, float height)
        {
            Vector2 d = new Vector2(MathF.Sqrt(p.X * p.X + p.Z * p.Z) - radius, MathF.Abs(p.Y) - height * 0.5f);
            return MathF.Min(MathF.Max(d.X, d.Y), 0.0f) + new Vector2(MathF.Max(d.X, 0.0f), MathF.Max(d.Y, 0.0f)).Length;
        }
    }
}
