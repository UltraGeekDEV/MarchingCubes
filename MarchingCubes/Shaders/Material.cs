using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarchingCubes.Rendering;

namespace MarchingCubes.Shaders
{
    public static class Materials
    {

        public static Dictionary<MaterialID,Shader> materials = new Dictionary<MaterialID,Shader>();
    }

    public enum MaterialID
    {
        Solid
    }
}
