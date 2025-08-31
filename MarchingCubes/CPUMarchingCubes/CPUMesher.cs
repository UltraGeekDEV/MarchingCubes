using MarchingCubes.Rendering;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MarchingCubes.CPUMarchingCubes
{
    public class CPUMesher
    {
        static float isoValue = 0.5f;
        public static List<Triangle> GetTriangles(int voxelsPerEdge)
        {
            var trianglesOut = new List<Triangle>();

            Vector3[] corners = new Vector3[8]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 1),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1),
                new Vector3(1, 1, 1),
                new Vector3(1, 1, 0)
            };

            (int x, int y, int z)[] cornerOffsets = new (int, int, int)[]
            {
                (-1, -1, -1),
                (-1, -1,  0),
                ( 0, -1,  0),
                ( 0, -1, -1),
                (-1,  0, -1),
                (-1,  0,  0),
                ( 0,  0,  0),
                ( 0,  0, -1),
            };

            (int a, int b)[] edges = new (int a, int b)[]
            {
                (0,1),
                (1,2),
                (2,3),
                (3,0),
                (4,5),
                (5,6),
                (6,7),
                (7,4),
                (0,4),
                (1,5),
                (2,6),
                (3,7)
            };

            var voxels = VoxelMeshGenerator.GetVoxelMeshCPU(1.0f / voxelsPerEdge);
            var dualContouring = new Vector3?[voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2)];

            int lenX = voxels.GetLength(0);
            int lenY = voxels.GetLength(1);
            int lenZ = voxels.GetLength(2);
            float trueVoxelSize = 1.0f/ lenX;
            for (int i = 1; i < lenX; i++)
            {
                for (int j = 1; j < lenY; j++)
                {
                    for (int k = 1; k < lenZ; k++)
                    {
                        var vertexList = new Vector3[12];
                        var isActiveEdge = new bool[12];

                        var M = new float[13, 3];
                        var Mt = new float[3, 13];
                        var b = new float[13];

                        int validEdges = 0;

                        for (int x = 0; x < 12; x++)
                        {
                            var edge = edges[x];
                            var offsetA = cornerOffsets[edge.a];
                            var offsetB = cornerOffsets[edge.b];
                            (int x, int y, int z) IDA = (i + offsetA.x, j + offsetA.y, k + offsetA.z);
                            (int x, int y, int z) IDB = (i + offsetB.x, j + offsetB.y, k + offsetB.z);
                            var A = voxels[IDA.x, IDA.y, IDA.z];
                            var B = voxels[IDB.x, IDB.y, IDB.z];

                            if (Math.Sign(A-isoValue) != Math.Sign(B-isoValue))
                            {
                                validEdges++;
                                vertexList[x] = VertexInterp(corners[edge.a], corners[edge.b], A, B);
                                //vertexList[x] =(corners[edge.a]+corners[edge.b])*0.5f;
                                isActiveEdge[x] = true;
                                //Vector3 normal = VertexInterp(GetGradient(IDA, voxels), GetGradient(IDB, voxels),A,B).Normalized();
                                Vector3 normal = ((GetGradient(IDA, voxels)+ GetGradient(IDB, voxels)) * 0.5f).Normalized();

                                //Vector3 normal = corners[edge.b] - corners[edge.a];
                                //{
                                //    var temp = -normal.Y;
                                //    normal.Y = normal.X;
                                //    normal.X = temp;
                                //}

                                M[x, 0] = normal.X;
                                M[x, 1] = normal.Y;
                                M[x, 2] = normal.Z;

                                Mt[0,x] = normal.X;
                                Mt[1,x] = normal.Y;
                                Mt[2,x] = normal.Z;

                                b[x] = Vector3.Dot(normal, vertexList[x] );
                            }
                            else
                            {
                                M[x, 0] = 0;
                                M[x, 1] = 0;
                                M[x, 2] = 0;

                                Mt[0, x] =0;
                                Mt[1, x] =0;
                                Mt[2, x] =0;
                            }
                        }

                        float divergence = 0;
                        int count = 0;
                        for (int x = 0; x < 12; x++)
                        {
                            for (int y = 0; y < 12; y++)
                            {
                                if (isActiveEdge[x] && isActiveEdge[y])
                                {
                                    var Vec1 = new Vector3(M[x, 0], M[x, 1], M[x, 2]);
                                    var Vec2 = new Vector3(M[y, 0], M[y, 1], M[y, 2]);

                                    divergence += MathF.Abs(Vector3.Dot(Vec1, Vec2));
                                    count++;
                                }
                            }
                        }
                        divergence = divergence / count;

                        if (validEdges < 2)
                        {
                            continue;
                        }

                        Func<Vector3?> avgP = () =>
                        {
                            var vOut = Vector3.Zero;
                            int count = 0;

                            for (int x = 0; x < 12; x++)
                            {
                                if (isActiveEdge[x])
                                {
                                    vOut += vertexList[x];
                                    count++;
                                }
                            }
                            if (count == 0)
                            {
                                return null;
                            }
                            return vOut / count;
                        };

                        if (divergence > 0.99)
                        {
                            dualContouring[i, j, k] = (avgP() + new Vector3(i, j, k)) *trueVoxelSize - Vector3.One * 0.5f;
                            continue;
                        }

                        var M2 = new float[3,3];

                        float bias = 0.00001f;
                        for (int y1 = 0; y1 < 3; y1++)
                        {
                            M2[y1, y1] += bias;
                            b[y1] += bias*0.5f;
                            for (int y2 = 0; y2 < 3; y2++)
                            {
                                
                                for (int x = 0; x < 12; x++)
                                {
                                    M2[y1, y2] += M[x, y2] * Mt[y1, x];
                                }
                            }
                        }

                        var b2 = new float[3];

                        for (int x = 0; x < 12; x++)
                        {
                            for (int y = 0; y < 3; y++)
                            {
                                b2[y] += b[x] * Mt[y, x];
                            }
                        }
                        
                        {                         //var system = new Vector4[3];

                            //for (int x = 0; x < 3; x++)
                            //{
                            //    system[x] = new Vector4(M2[x, 0], M2[x, 1], M2[x, 2], b2[x]);
                            //}



                            //float X;
                            //float Y;
                            //float Z;

                            //int bestRow = 0;
                            //float bestAbs = MathF.Abs(system[0].X);
                            //for (int r = 1; r < 3; r++)
                            //{
                            //    float a = MathF.Abs(system[r].X);
                            //    if (a > bestAbs)
                            //    {
                            //        bestAbs = a;
                            //        bestRow = r;
                            //    }
                            //}
                            //if (bestRow != 0)
                            //{
                            //    var tmp = system[0];
                            //    system[0] = system[bestRow];
                            //    system[bestRow] = tmp;
                            //}

                            //if (Math.Abs(system[0].X) > bias + 1e-3f)
                            //{
                            //    system[1] = system[1] - system[0] * (system[1].X / system[0].X);
                            //    system[2] = system[2] - system[0] * (system[2].X / system[0].X);

                            //    if (MathF.Abs(system[1].Y) < MathF.Abs(system[2].Y))
                            //    {
                            //        var temp = system[1];
                            //        system[1] = system[2];
                            //        system[2] = temp;
                            //    }

                            //    if (Math.Abs(system[1].Y) > bias + 1e-3f)
                            //    {
                            //        system[2] = system[2] - system[1] * (system[2].Y / system[1].Y);

                            //        Z = system[2].W / system[2].Z;
                            //        Y = (system[1].W - system[1].Z * Z) / system[1].Y;
                            //        X = (system[0].W - system[0].Y * Y - system[0].Z * Z) / system[0].X;
                            //    }
                            //    else
                            //    {
                            //        var p = avgP();
                            //        if (p is null)
                            //        {
                            //            continue;
                            //        }
                            //        Z = p.Value.Z;
                            //        Y = p.Value.Y;
                            //        X = p.Value.X;
                            //    }
                            //}
                            //else
                            //{
                            //    var p = avgP();
                            //    if (p is null)
                            //    {
                            //        continue;
                            //    }
                            //    Z = p.Value.Z;
                            //    Y = p.Value.Y;
                            //    X = p.Value.X;
                            //}

                            //Z = MathF.Min(MathF.Max(Z, -2), 2);
                            //X = MathF.Min(MathF.Max(X, -2), 2);
                            //Y = MathF.Min(MathF.Max(Y, -2), 2);
                        }

                        var biasPoint = avgP();
                        M[12, 0] = bias;
                        M[12, 1] = bias;
                        M[12, 2] = bias;

                        b[12] = bias * (biasPoint.Value.X + biasPoint.Value.Y + biasPoint.Value.Z) / 3.0f;

                        var svdA = Matrix<float>.Build.DenseOfArray(M);
                        var svdB = Vector<float>.Build.Dense(b);

                        var svd = svdA.Svd(true);
                        var p = svd.Solve(svdB);

                        dualContouring[i,j,k] = (new Vector3(   Math.Clamp(p[0], 0, 1), 
                                                                Math.Clamp(p[1], 0, 1), 
                                                                Math.Clamp(p[2], 0, 1)) + new Vector3(i, j, k)) *trueVoxelSize - Vector3.One * 0.5f;
                    }
                }
            }

            for (int i = 0; i < lenX-1; i++)
            {
                for (int j = 0; j < lenY-1; j++)
                {
                    for (int k = 0; k < lenZ - 1; k++)
                    {
                        var v0 = dualContouring[i, j, k];

                        if(v0 is null)
                        {
                            continue;
                        }

                        ////Z loop
                        {
                            var v1 = dualContouring[i + 1, j, k];
                            var v2 = dualContouring[i, j + 1, k];
                            var v3 = dualContouring[i + 1, j + 1, k];

                            if (!(v1 is null || v2 is null || v3 is null))
                            {
                                var tri1 = new Triangle(v0.Value, v1.Value, v2.Value);
                                var tri2 = new Triangle(v1.Value, v3.Value, v2.Value);

                                var gradient = GetGradient((i, j, k), voxels);

                                if (Vector3.Dot(tri1.GetNormal(), gradient) < 0)
                                {
                                    var swap = tri1.a;
                                    tri1.a = tri1.b;
                                    tri1.b = swap;
                                    tri1.normal = tri1.GetNormal();
                                }

                                if (Vector3.Dot(tri2.GetNormal(), gradient) < 0)
                                {
                                    var swap = tri2.a;
                                    tri2.a = tri2.b;
                                    tri2.b = swap;
                                    tri2.normal = tri2.GetNormal();
                                }

                                trianglesOut.Add(tri1);
                                trianglesOut.Add(tri2);
                            }
                        }
                        //Y loop
                        {
                            var v1 = dualContouring[i + 1, j, k];
                            var v2 = dualContouring[i, j , k + 1];
                            var v3 = dualContouring[i + 1, j, k + 1];

                            if (!(v1 is null || v2 is null || v3 is null))
                            {
                                var tri1 = new Triangle(v0.Value, v1.Value, v2.Value);
                                var tri2 = new Triangle(v1.Value, v3.Value, v2.Value);

                                var gradient = GetGradient((i, j, k), voxels);

                                if (Vector3.Dot(tri1.GetNormal(), gradient) < 0)
                                {
                                    var swap = tri1.a;
                                    tri1.a = tri1.b;
                                    tri1.b = swap;
                                    tri1.normal = tri1.GetNormal();
                                }

                                if (Vector3.Dot(tri2.GetNormal(), gradient) < 0)
                                {
                                    var swap = tri2.a;
                                    tri2.a = tri2.b;
                                    tri2.b = swap;
                                    tri2.normal = tri2.GetNormal();
                                }

                                trianglesOut.Add(tri1);
                                trianglesOut.Add(tri2);
                            }
                        }
                        //X loop
                        {
                            var v1 = dualContouring[i , j + 1, k];
                            var v2 = dualContouring[i, j, k + 1];
                            var v3 = dualContouring[i , j + 1, k + 1];

                            if (!(v1 is null || v2 is null || v3 is null))
                            {
                                var tri1 = new Triangle(v0.Value, v1.Value, v2.Value);
                                var tri2 = new Triangle(v1.Value, v3.Value, v2.Value);

                                var gradient = GetGradient((i, j, k), voxels);

                                if (Vector3.Dot(tri1.GetNormal(), gradient) < 0)
                                {
                                    var swap = tri1.a;
                                    tri1.a = tri1.b;
                                    tri1.b = swap;
                                    tri1.normal = tri1.GetNormal();
                                }

                                if (Vector3.Dot(tri2.GetNormal(), gradient) < 0)
                                {
                                    var swap = tri2.a;
                                    tri2.a = tri2.b;
                                    tri2.b = swap;
                                    tri2.normal = tri2.GetNormal();
                                }

                                trianglesOut.Add(tri1);
                                trianglesOut.Add(tri2);
                            }
                        }
                    }
                }
            }

            Dictionary<(int, int, int), Vector3> normals = new Dictionary<(int, int, int), Vector3>();
            float searchRange = 1f * trueVoxelSize;
            for (int i = 0; i < trianglesOut.Count; i++)
            {
                var tri = trianglesOut[i];
                var key = GetKey(tri.a, searchRange);
                normals[key] = normals.GetValueOrDefault(key, Vector3.Zero) + tri.normal;

                key = GetKey(tri.b, searchRange);
                normals[key] = normals.GetValueOrDefault(key, Vector3.Zero) + tri.normal;

                key = GetKey(tri.c, searchRange);
                normals[key] = normals.GetValueOrDefault(key, Vector3.Zero) + tri.normal;
            }

            foreach (var item in normals.Keys)
            {
                normals[item] = normals[item].Normalized();
            }

            for (int i = 0; i < trianglesOut.Count; i++)
            {
                var tri = trianglesOut[i];
                var a = normals[GetKey(tri.a, searchRange)];
                if (Vector3.Dot(tri.normal,a) > 0.7f)
                {
                    tri.normA = a;
                }
                else
                {
                    tri.normA = tri.normal;
                }
                var b = normals[GetKey(tri.b, searchRange)];
                if (Vector3.Dot(tri.normal, b) > 0.7f)
                {
                    tri.normB = b;
                }
                else
                {
                    tri.normB = tri.normal;
                }
                var c = normals[GetKey(tri.c, searchRange)];
                if (Vector3.Dot(tri.normal, c) > 0.7f)
                {
                    tri.normC = c;
                }
                else
                {
                    tri.normC = tri.normal;
                }
            }

            return trianglesOut;
        }
        private static Vector3 GetSobelGradient(int x, int y, int z, float[,,] voxels)
        {
            int lenX = voxels.GetLength(0);
            int lenY = voxels.GetLength(1);
            int lenZ = voxels.GetLength(2);

            float dx = 0, dy = 0, dz = 0;
            int size =10;
            // Iterate over neighbors in a 3x3x3 cube
            for (int i = -size; i <= size; i++)
            {
                int xi = Math.Clamp(x + i, 0, lenX - 1); 
                for (int j = -size; j <= size; j++)
                {
                    int yj = Math.Clamp(y + j, 0, lenY - 1); 
                    for (int k = -size; k <= size; k++)
                    {
                        int zk = Math.Clamp(z + k, 0, lenZ - 1); 
                        float val = voxels[xi, yj, zk]; 
                        dx += val * Math.Sign(i); 
                        dy += val * Math.Sign(j); 
                        dz += val * Math.Sign(k); 
                    } 
                } 
            }

                        Vector3 grad = new Vector3(dx, dy, dz);
            return Vector3.Normalize(grad);
        }
        private static Vector3 GetGradient((int x, int y, int z) id, float[,,] voxels)
        {
            int lenX = voxels.GetLength(0) - 1;
            int lenY = voxels.GetLength(1) - 1;
            int lenZ = voxels.GetLength(2) - 1;

            return new Vector3(voxels[Math.Min(id.x + 1, lenX), id.y, id.z] - voxels[Math.Max(id.x - 1, 0), id.y, id.z]
                               , voxels[id.x, Math.Min(id.y + 1, lenY), id.z] - voxels[id.x, Math.Max(id.y - 1, 0), id.z]
                               , voxels[id.x, id.y, Math.Min(id.z + 1, lenZ)] - voxels[id.x, id.y, Math.Max(id.z - 1, 0)]).Normalized();
            return GetSobelGradient(id.x,id.y,id.z,voxels).Normalized();
        }

        static Vector3 VertexInterp(Vector3 p1, Vector3 p2,float valp1,float valp2)
        {
            float mu;
            Vector3 p = new Vector3();

            if (MathF.Abs(isoValue - valp1) < 0.001f) return (p1);
            if (MathF.Abs(isoValue - valp2) < 0.001f) return (p2);
            if (MathF.Abs(valp2 - valp1) < 0.001f) return (p1);
            mu = (isoValue - valp1) / (valp2 - valp1);

            p.X = p1.X + mu * (p2.X - p1.X);
            p.Y = p1.Y + mu * (p2.Y - p1.Y);
            p.Z = p1.Z + mu * (p2.Z - p1.Z);
        
           return(p);
        }

    private static (int x,int y, int z) GetKey(Vector3 pos,float resolution)
        {
            return ((int)(pos.X / resolution), (int)(pos.Y /resolution), (int)(pos.Z /resolution));
        }
    }
}
