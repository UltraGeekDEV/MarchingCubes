using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarchingCubes.Utils
{
    public static class DebugUtils
    {
        public static Action<string> debugOutput;

        public static void GetGLError(string location)
        {
            var err = GL.GetError();

            if (err != ErrorCode.NoError)
            {
                debugOutput?.Invoke($"ERROR in {location}: {err.ToString()}\n");
            }
        }
        public static void GetGLCompileError(string location,int shaderID)
        {
            var err = GL.GetShaderInfoLog(shaderID).Trim();

            if (err.Length > 0)
            {
                debugOutput?.Invoke($"COMPILE ERROR in {location}: {err}");
            }
        }
    }
}
