using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;

namespace MarchingCubes.Rendering
{
    public class Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 normal;
        public Vector3 normA;
        public Vector3 normB;
        public Vector3 normC;

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            normal = normA = normB = normC = GetNormal();
        }

        public Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normA, Vector3 normB, Vector3 normC)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.normA = normA;
            this.normB = normB;
            this.normC = normC;
            normal = GetNormal();
        }

        public bool IsTouching(Vector3 point,float range)
        {
            return (point - a).LengthSquared < range || (point - b).LengthSquared < range || (point - c).LengthSquared < range;
        }

        public Vector3 GetNormal()
        {
            return Vector3.Cross(a - b, a - c);
        }
    }
}
