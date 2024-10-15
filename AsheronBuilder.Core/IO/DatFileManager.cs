// AsheronBuilder.Core/IO/DatFileManager.cs

using ACClientLib.DatReaderWriter;
using ACClientLib.DatReaderWriter.Options;
using AsheronBuilder.Core.DatTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = AsheronBuilder.Core.DatTypes.Environment;

namespace AsheronBuilder.Core.IO
{
    public class DatFileManager
    {
        private DatManager _datManager;

        public void Initialize(string datFilePath)
        {
            _datManager = new DatManager(options =>
            {
                options.DatDirectory = datFilePath;
                options.IndexCachingStrategy = IndexCachingStrategy.Upfront;
            });
        }

        public GfxObj LoadGfxObj(uint fileId)
        {
            if (_datManager.Portal.TryReadFile(fileId, out GfxObj model))
                return model;
            throw new Exception($"Failed to load model with ID {fileId}");
        }

        public Texture LoadTexture(uint fileId)
        {
            if (_datManager.Portal.TryReadFile(fileId, out Texture texture))
                return texture;
            throw new Exception($"Failed to load texture with ID {fileId}");
        }

        public Environment LoadEnvironment(uint fileId)
        {
            if (_datManager.Portal.TryReadFile(fileId, out Environment environment))
                return environment;
            throw new Exception($"Failed to load environment with ID {fileId}");
        }

        public CellLandblock LoadLandblock(uint fileId)
        {
            if (_datManager.Cell.TryReadFile(fileId, out CellLandblock landblock))
                return landblock;
            throw new Exception($"Failed to load landblock with ID {fileId}");
        }

        public List<uint> GetFileIdsOfType(uint typeId)
        {
            return _datManager.Portal.Tree.AsEnumerable().Where(f => f.Id >> 24 == typeId).Select(f => f.Id).ToList();
        }

        public void SaveGfxObj(GfxObj model)
        {
            // Implement saving logic using _datManager.Portal
        }

        public void SaveTexture(Texture texture)
        {
            // Implement saving logic using _datManager.Portal
        }

        public void SaveEnvironment(Environment environment)
        {
            // Implement saving logic using _datManager.Portal
        }

        public void SaveLandblock(CellLandblock landblock)
        {
            // Implement saving logic using _datManager.Cell
        }

        public bool FileExists(uint fileId)
        {
            return _datManager.Portal.Tree.AsEnumerable().Any(f => f.Id == fileId);
        }

        public void DeleteFile(uint fileId)
        {
            // Implement delete logic using _datManager.Portal
        }
    }
}