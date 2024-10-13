using ACClientLib.DatReaderWriter;
using ACClientLib.DatReaderWriter.Options;
using ACDungeonBuilder.Core.DatTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = ACDungeonBuilder.Core.DatTypes.Environment;

namespace ACDungeonBuilder.Core.Assets
{
    public class AssetManager
    {
        private readonly DatManager _datManager;
        private readonly AssetCache<Texture> _textureCache = new AssetCache<Texture>();
        private readonly AssetCache<GfxObj> _modelCache = new AssetCache<GfxObj>();
        private readonly AssetCache<Environment> _environmentCache = new AssetCache<Environment>();

        public AssetManager()
        {
            string datFilePath = @"C:\Turbine\Asheron's Call";
            _datManager = new DatManager(options =>
            {
                options.DatDirectory = datFilePath;
                options.IndexCachingStrategy = IndexCachingStrategy.Upfront;
            });
        }

        public List<uint> GetTextureFileIds()
        {
            // This is a placeholder. You'll need to implement this based on how file IDs are stored and retrieved in the DatReaderWriter library
            return new List<uint>();
        }

        public List<uint> GetModelFileIds()
        {
            // This is a placeholder. You'll need to implement this based on how file IDs are stored and retrieved in the DatReaderWriter library
            return new List<uint>();
        }

        public List<uint> GetEnvironmentFileIds()
        {
            // This is a placeholder. You'll need to implement this based on how file IDs are stored and retrieved in the DatReaderWriter library
            return new List<uint>();
        }

        public Texture LoadTexture(uint fileId)
        {
            return _textureCache.GetOrLoad(fileId, id => 
            {
                if (_datManager.Portal.TryReadFile(id, out Texture texture))
                    return texture;
                throw new Exception($"Failed to load texture with ID {id}");
            });
        }

        public GfxObj LoadModel(uint fileId)
        {
            return _modelCache.GetOrLoad(fileId, id => 
            {
                if (_datManager.Portal.TryReadFile(id, out GfxObj model))
                    return model;
                throw new Exception($"Failed to load model with ID {id}");
            });
        }

        public Environment LoadEnvironment(uint fileId)
        {
            return _environmentCache.GetOrLoad(fileId, id => 
            {
                if (_datManager.Portal.TryReadFile(id, out Environment environment))
                    return environment;
                throw new Exception($"Failed to load environment with ID {id}");
            });
        }
    }
}