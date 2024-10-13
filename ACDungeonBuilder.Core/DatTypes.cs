using ACClientLib.DatReaderWriter.IO;
using System;

namespace ACDungeonBuilder.Core.DatTypes
{
    public class Texture : IDatFileType
    {
        public uint Id { get; set; }
        // Add other properties specific to Texture

        public bool Unpack(DatFileReader reader)
        {
            // Implement unpacking logic
            throw new NotImplementedException();
        }
    }

    public class GfxObj : IDatFileType
    {
        public uint Id { get; set; }
        // Add other properties specific to GfxObj

        public bool Unpack(DatFileReader reader)
        {
            // Implement unpacking logic
            throw new NotImplementedException();
        }
    }

    public class Environment : IDatFileType
    {
        public uint Id { get; set; }
        // Add other properties specific to Environment

        public bool Unpack(DatFileReader reader)
        {
            // Implement unpacking logic
            throw new NotImplementedException();
        }
    }

    public class CellLandblock : IDatFileType
    {
        public uint Id { get; set; }
        // Add other properties specific to CellLandblock

        public bool Unpack(DatFileReader reader)
        {
            // Implement unpacking logic
            throw new NotImplementedException();
        }
    }
}