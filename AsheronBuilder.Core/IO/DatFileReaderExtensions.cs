// AsheronBuilder.Core/IO/DatFileReaderExtensions.cs
using ACClientLib.DatReaderWriter.IO;
using System;

namespace AsheronBuilder.Core.IO
{
    public static class DatFileReaderExtensions
    {
        public static float ReadSingle(this DatFileReader reader)
        {
            return BitConverter.ToSingle(reader.ReadBytes(4), 0);
        }
    }
}