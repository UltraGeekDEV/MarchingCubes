using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarchingCubes.Rendering
{
    public class Camera
    {
        public Matrix4 perspective;
        public Matrix4 transform;

        public Camera()
        {
        }

        public void SetPerspective(float fov, float aspectRatio)
        {
            perspective = Matrix4.CreatePerspectiveFieldOfView(fov * MathF.PI / 180.0f, aspectRatio, 0.001f, 100f);
        }
    }
}
