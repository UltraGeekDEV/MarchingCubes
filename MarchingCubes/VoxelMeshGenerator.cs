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
        public static byte[,,] GetVoxelMeshCPU(float resolution)
        {
            var voxelCount = (int)(1.0f / resolution);
            resolution = 1.0f / voxelCount;

            var center = Vector3.One * 0.5f;

            var voxelMesh = new byte[voxelCount, voxelCount, voxelCount];

            for (int i = 0; i < voxelCount; i++)
            {
                for (int j = 0; j < voxelCount; j++)
                {
                    for (int k = 0; k < voxelCount; k++)
                    {
                        voxelMesh[i, j, k] = (byte)((center - new Vector3(i, j, k) * resolution).Length < 0.4f ? 1 : 0);
                    }
                }
            }


            return voxelMesh;
        }
    }
}
