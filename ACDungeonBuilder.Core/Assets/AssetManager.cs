// ACDungeonBuilder.Core/Assets/AssetManager.cs

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = ACE.DatLoader.FileTypes.Environment;

namespace ACDungeonBuilder.Core.Assets
{
    public class AssetManager
    {
        private readonly DatDatabase _portalDat;
        private readonly DatDatabase _cellDat;
        private readonly AssetCache<Texture> _textureCache = new AssetCache<Texture>();
        private readonly AssetCache<GfxObj> _modelCache = new AssetCache<GfxObj>();
        private readonly AssetCache<Environment> _environmentCache = new AssetCache<Environment>();

        public AssetManager()
        {
            string datFilePath = @"C:\Turbine\Asheron's Call";
            _portalDat = new DatDatabase(datFilePath + "\\client_portal.dat");
            _cellDat = new DatDatabase(datFilePath + "\\client_cell_1.dat");
        }

        public List<uint> GetTextureFileIds()
        {
            return GetFileIdsOfType(DatDatabaseType.Portal, DatFileType.Texture);
        }

        public List<uint> GetModelFileIds()
        {
            return GetFileIdsOfType(DatDatabaseType.Portal, DatFileType.GraphicsObject);
        }

        public List<uint> GetEnvironmentFileIds()
        {
            return GetFileIdsOfType(DatDatabaseType.Portal, DatFileType.Environment);
        }

        public Texture LoadTexture(uint fileId)
        {
            return _textureCache.GetOrLoad(fileId, id => _portalDat.ReadFromDat<Texture>(id));
        }

        public GfxObj LoadModel(uint fileId)
        {
            return _modelCache.GetOrLoad(fileId, id => _portalDat.ReadFromDat<GfxObj>(id));
        }

        public Environment LoadEnvironment(uint fileId)
        {
            return _environmentCache.GetOrLoad(fileId, id => _portalDat.ReadFromDat<Environment>(id));
        }

        private List<uint> GetFileIdsOfType(DatDatabaseType databaseType, DatFileType fileType)
        {
            var database = databaseType == DatDatabaseType.Portal ? _portalDat : _cellDat;
            return database.AllFiles.Where(f => f.Value.ObjectId == (uint)fileType).Select(f => f.Key).ToList();
        }
    }

    public enum DatDatabaseType
    {
        Portal,
        Cell
    }
}