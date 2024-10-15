// File: AsheronBuilder.Core/DatTypes/Environment.cs

using System.Collections.Generic;
using OpenTK.Mathematics;

namespace AsheronBuilder.Core.DatTypes
{
    public class Environment
    {
        public uint Id { get; set; }
        public List<CellStruct> Cells { get; set; }

        public Environment()
        {
            Cells = new List<CellStruct>();
        }

        // Add methods for loading and manipulating environment data
    }

    public class CellStruct
    {
        public uint Id { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<int> Indices { get; set; }

        public CellStruct()
        {
            Vertices = new List<Vector3>();
            Indices = new List<int>();
        }
    }
}