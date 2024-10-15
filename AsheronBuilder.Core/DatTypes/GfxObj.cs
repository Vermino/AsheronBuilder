using OpenTK.Mathematics;
using System.Collections.Generic;

namespace AsheronBuilder.Core.DatTypes
{
    public class GfxObj
    {
        public uint Id { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Vector2> TexCoords { get; set; }
        public List<int> Indices { get; set; }

        public GfxObj()
        {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            TexCoords = new List<Vector2>();
            Indices = new List<int>();
        }

        // Add methods for loading and manipulating model data
    }
}