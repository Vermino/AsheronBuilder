// File: AsheronBuilder.Core/DatTypes/Texture.cs

namespace AsheronBuilder.Core.DatTypes
{
    public class Texture
    {
        public uint Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] TextureData { get; set; }

        // Add methods for loading and manipulating texture data
    }
}