// using ACClientLib.DatReaderWriter.IO;
// using AsheronBuilder.Core.IO;
// using AsheronBuilder.Core.Extensions;
// using System;
// using System.Collections.Generic;
// using System.Numerics;
//
// namespace AsheronBuilder.Core.DatTypes
// {
//     public class Texture : IDatFileType
//     {
//         public uint Id { get; set; }
//         public int Width { get; set; }
//         public int Height { get; set; }
//         public byte[] TextureData { get; set; }
//
//         public bool Unpack(DatFileReader reader)
//         {
//             try
//             {
//                 Id = reader.ReadUInt32();
//                 Width = reader.ReadInt32();
//                 Height = reader.ReadInt32();
//                 int dataSize = Width * Height * 4; // Assuming 32-bit color (RGBA)
//                 TextureData = reader.ReadBytes(dataSize);
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error unpacking Texture: {ex.Message}");
//                 return false;
//             }
//         }
//         
//         public int GetSize()
//         {
//             return sizeof(uint) + sizeof(int) + sizeof(int) + TextureData.Length;
//         }
//
//         public bool Pack(DatFileWriter writer)
//         {
//             try
//             {
//                 writer.WriteUInt32(Id);
//                 writer.WriteInt32(Width);
//                 writer.WriteInt32(Height);
//                 writer.WriteBytes(TextureData, TextureData.Length);
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error packing Texture: {ex.Message}");
//                 return false;
//             }
//         }
//     }
//
//     public class GfxObj : IDatFileType
//     {
//         public uint Id { get; set; }
//         public List<Vector3> Vertices { get; set; }
//         public List<Vector3> Normals { get; set; }
//         public List<Vector2> TexCoords { get; set; }
//         public List<int> Indices { get; set; }
//
//         public bool Unpack(DatFileReader reader)
//         {
//             try
//             {
//                 Id = reader.ReadUInt32();
//                 int vertexCount = reader.ReadInt32();
//                 Vertices = new List<Vector3>(vertexCount);
//                 Normals = new List<Vector3>(vertexCount);
//                 TexCoords = new List<Vector2>(vertexCount);
//
//                 for (int i = 0; i < vertexCount; i++)
//                 {
//                     Vertices.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
//                     Normals.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
//                     TexCoords.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
//                 }
//
//                 int indexCount = reader.ReadInt32();
//                 Indices = new List<int>(indexCount);
//                 for (int i = 0; i < indexCount; i++)
//                 {
//                     Indices.Add(reader.ReadInt32());
//                 }
//
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error unpacking GfxObj: {ex.Message}");
//                 return false;
//             }
//         }
//
//         public int GetSize()
//         {
//             return sizeof(uint) + sizeof(int) + (Vertices.Count * 12) + (Normals.Count * 12) + (TexCoords.Count * 8) + sizeof(int) + (Indices.Count * sizeof(int));
//         }
//
//         public bool Pack(DatFileWriter writer)
//         {
//             try
//             {
//                 writer.WriteUInt32(Id);
//                 writer.WriteInt32(Vertices.Count);
//                 foreach (var vertex in Vertices)
//                 {
//                     writer.WriteSingle(vertex.X);
//                     writer.WriteSingle(vertex.Y);
//                     writer.WriteSingle(vertex.Z);
//                 }
//                 foreach (var normal in Normals)
//                 {
//                     writer.WriteSingle(normal.X);
//                     writer.WriteSingle(normal.Y);
//                     writer.WriteSingle(normal.Z);
//                 }
//                 foreach (var texCoord in TexCoords)
//                 {
//                     writer.WriteSingle(texCoord.X);
//                     writer.WriteSingle(texCoord.Y);
//                 }
//                 writer.WriteInt32(Indices.Count);
//                 foreach (var index in Indices)
//                 {
//                     writer.WriteInt32(index);
//                 }
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error packing GfxObj: {ex.Message}");
//                 return false;
//             }
//         }
//     }
//
//     public class Environment : IDatFileType
//     {
//         public uint Id { get; set; }
//         public List<CellStruct> Cells { get; set; }
//
//         public bool Unpack(DatFileReader reader)
//         {
//             try
//             {
//                 Id = reader.ReadUInt32();
//                 int cellCount = reader.ReadInt32();
//                 Cells = new List<CellStruct>(cellCount);
//
//                 for (int i = 0; i < cellCount; i++)
//                 {
//                     var cell = new CellStruct();
//                     cell.Unpack(reader);
//                     Cells.Add(cell);
//                 }
//
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error unpacking Environment: {ex.Message}");
//                 return false;
//             }
//         }
//
//         public int GetSize()
//         {
//             int size = sizeof(uint) + sizeof(int);
//             foreach (var cell in Cells)
//             {
//                 size += cell.GetSize();
//             }
//             return size;
//         }
//
//         public bool Pack(DatFileWriter writer)
//         {
//             try
//             {
//                 writer.WriteUInt32(Id);
//                 writer.WriteInt32(Cells.Count);
//                 foreach (var cell in Cells)
//                 {
//                     cell.Pack(writer);
//                 }
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error packing Environment: {ex.Message}");
//                 return false;
//             }
//         }
//     }
//
//     public class CellStruct
//     {
//         public uint Id { get; set; }
//         public List<Vector3> Vertices { get; set; }
//         public List<int> Indices { get; set; }
//
//         public void Unpack(DatFileReader reader)
//         {
//             Id = reader.ReadUInt32();
//             int vertexCount = reader.ReadInt32();
//             Vertices = new List<Vector3>(vertexCount);
//
//             for (int i = 0; i < vertexCount; i++)
//             {
//                 Vertices.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
//             }
//
//             int indexCount = reader.ReadInt32();
//             Indices = new List<int>(indexCount);
//
//             for (int i = 0; i < indexCount; i++)
//             {
//                 Indices.Add(reader.ReadInt32());
//             }
//         }
//
//         public int GetSize()
//         {
//             return sizeof(uint) + sizeof(int) + (Vertices.Count * 12) + sizeof(int) + (Indices.Count * sizeof(int));
//         }
//
//         public void Pack(DatFileWriter writer)
//         {
//             writer.WriteUInt32(Id);
//             writer.WriteInt32(Vertices.Count);
//             foreach (var vertex in Vertices)
//             {
//                 writer.WriteSingle(vertex.X);
//                 writer.WriteSingle(vertex.Y);
//                 writer.WriteSingle(vertex.Z);
//             }
//             writer.WriteInt32(Indices.Count);
//             foreach (var index in Indices)
//             {
//                 writer.WriteInt32(index);
//             }
//         }
//     }
//
//     public class CellLandblock : IDatFileType
//     {
//         public uint Id { get; set; }
//         public List<Vector3> Vertices { get; set; }
//         public List<int> Indices { get; set; }
//
//         public bool Unpack(DatFileReader reader)
//         {
//             try
//             {
//                 Id = reader.ReadUInt32();
//                 int vertexCount = reader.ReadInt32();
//                 Vertices = new List<Vector3>(vertexCount);
//
//                 for (int i = 0; i < vertexCount; i++)
//                 {
//                     Vertices.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
//                 }
//
//                 int indexCount = reader.ReadInt32();
//                 Indices = new List<int>(indexCount);
//
//                 for (int i = 0; i < indexCount; i++)
//                 {
//                     Indices.Add(reader.ReadInt32());
//                 }
//
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error unpacking CellLandblock: {ex.Message}");
//                 return false;
//             }
//         }
//
//         public int GetSize()
//         {
//             return sizeof(uint) + sizeof(int) + (Vertices.Count * 12) + sizeof(int) + (Indices.Count * sizeof(int));
//         }
//
//         public bool Pack(DatFileWriter writer)
//         {
//             try
//             {
//                 writer.WriteUInt32(Id);
//                 writer.WriteInt32(Vertices.Count);
//                 foreach (var vertex in Vertices)
//                 {
//                     writer.WriteSingle(vertex.X);
//                     writer.WriteSingle(vertex.Y);
//                     writer.WriteSingle(vertex.Z);
//                 }
//                 writer.WriteInt32(Indices.Count);
//                 foreach (var index in Indices)
//                 {
//                     writer.WriteInt32(index);
//                 }
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error packing CellLandblock: {ex.Message}");
//                 return false;
//             }
//         }
//     }
// }