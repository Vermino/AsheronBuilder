using ACClientLib.DatReaderWriter.IO;
using System.Buffers.Binary;

namespace AsheronBuilder.Core.Extensions
{
    public static class DatFileWriterExtensions
    {
        public static void WriteSingle(this DatFileWriter writer, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            writer.WriteBytes(bytes, bytes.Length);
        }
    }
}