using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Diagnostics;

namespace ACDungeonBuilder.Core.Assets
{
    public class EnvironmentLoader
    {
        public class EnvironmentData
        {
            public List<Vector3> Vertices { get; set; } = new List<Vector3>();
            public List<int> Indices { get; set; } = new List<int>();
        }

        public static EnvironmentData LoadFromRawFile(string filePath)
        {
            var environmentData = new EnvironmentData();

            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                try
                {
                    byte fileVersion = reader.ReadByte();
                    Debug.WriteLine($"File version: {fileVersion}");

                    int cellStructCount = reader.ReadInt32();
                    Debug.WriteLine($"CellStruct count: {cellStructCount}");

                    reader.BaseStream.Position = 32;
                    Debug.WriteLine($"Skipped to position: {reader.BaseStream.Position}");

                    int vertexCount = reader.ReadInt32();
                    Debug.WriteLine($"Vertex count: {vertexCount}");

                    int maxVertices = 10000;
                    vertexCount = Math.Min(vertexCount, maxVertices);

                    Vector3 min = new Vector3(float.MaxValue);
                    Vector3 max = new Vector3(float.MinValue);

                    for (int i = 0; i < vertexCount && reader.BaseStream.Position < reader.BaseStream.Length - 12; i++)
                    {
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();

                        // Skip invalid vertices
                        if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z) ||
                            float.IsInfinity(x) || float.IsInfinity(y) || float.IsInfinity(z) ||
                            Math.Abs(x) > 1e10 || Math.Abs(y) > 1e10 || Math.Abs(z) > 1e10)
                        {
                            continue;
                        }

                        Vector3 vertex = new Vector3(x, y, z);
                        environmentData.Vertices.Add(vertex);

                        min = Vector3.Min(min, vertex);
                        max = Vector3.Max(max, vertex);

                        if (i < 5 || i > vertexCount - 5)
                        {
                            Debug.WriteLine($"Vertex {i}: ({x}, {y}, {z})");
                        }
                    }

                    Debug.WriteLine($"Read {environmentData.Vertices.Count} valid vertices");

                    if (environmentData.Vertices.Count > 0)
                    {
                        Vector3 center = (min + max) / 2f;
                        float maxDimension = Math.Max(Math.Max(max.X - min.X, max.Y - min.Y), max.Z - min.Z);
                        float scale = maxDimension > 0 ? 10f / maxDimension : 1f;

                        // Normalize vertices
                        for (int i = 0; i < environmentData.Vertices.Count; i++)
                        {
                            environmentData.Vertices[i] = (environmentData.Vertices[i] - center) * scale;
                        }

                        // Create indices for wireframe
                        for (int i = 0; i < environmentData.Vertices.Count; i++)
                        {
                            environmentData.Indices.Add(i);
                            environmentData.Indices.Add((i + 1) % environmentData.Vertices.Count);
                        }

                        Debug.WriteLine($"Created {environmentData.Indices.Count} indices for wireframe");
                        Debug.WriteLine($"Environment bounds: Min {min}, Max {max}, Center {center}, Scale {scale}");
                    }
                }
                catch (EndOfStreamException)
                {
                    Debug.WriteLine("Reached end of file unexpectedly. Proceeding with partial data.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading file: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

            return environmentData;
        }
    }
}