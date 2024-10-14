// AsheronBuilder.Core/Assets/AssetManager.cs
using ACClientLib.DatReaderWriter;
using ACClientLib.DatReaderWriter.Options;
using AsheronBuilder.Core.DatTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = AsheronBuilder.Core.DatTypes.Environment;

namespace AsheronBuilder.Core.Assets
{
    public class AssetManager
    {
        private readonly DatManager _datManager;
        private readonly AssetCache<Texture> _textureCache = new AssetCache<Texture>();
        private readonly AssetCache<GfxObj> _modelCache = new AssetCache<GfxObj>();
        private readonly AssetCache<Environment> _environmentCache = new AssetCache<Environment>();

        public AssetManager(string datFilePath)
        {
            if (!Directory.Exists(datFilePath))
            {
                throw new DirectoryNotFoundException($"The directory '{datFilePath}' does not exist.");
            }

            _datManager = new DatManager(options =>
            {
                options.DatDirectory = datFilePath;
                options.IndexCachingStrategy = IndexCachingStrategy.Upfront;
            });
        }
        // TODO Need help connecting the DAT manager to Trevis DatReaderWriter Library
        // public List<uint> GetTextureFileIds()
        // {
        //     return _datManager.Portal.Tree.GetEnumerator().Where(f => f.Id >> 24 == 0x05).Select(f => f.Id).ToList();
        // }
        //
        // public List<uint> GetModelFileIds()
        // {
        //     return _datManager.Portal.Tree.GetEnumerator().Where(f => f.Id >> 24 == 0x01).Select(f => f.Id).ToList();
        // }
        //
        // public List<uint> GetEnvironmentFileIds()
        // { 
        //     return _datManager.Portal.Tree.GetEnumerator().Where(f => f.Id >> 24 == 0x0D).Select(f => f.Id).ToList();
        // } 

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

        public void ClearCaches()
        {
            _textureCache.Clear();
            _modelCache.Clear();
            _environmentCache.Clear();
        }

        public void TrimCaches()
        {
            _textureCache.Trim();
            _modelCache.Trim();
            _environmentCache.Trim();
        }

        public int GetCachedTextureCount() => _textureCache.Count;
        public int GetCachedModelCount() => _modelCache.Count;
        public int GetCachedEnvironmentCount() => _environmentCache.Count;

        public bool TryGetCachedTexture(uint fileId, out Texture texture) => _textureCache.TryGetValue(fileId, out texture);
        public bool TryGetCachedModel(uint fileId, out GfxObj model) => _modelCache.TryGetValue(fileId, out model);
        public bool TryGetCachedEnvironment(uint fileId, out Environment environment) => _environmentCache.TryGetValue(fileId, out environment);
    }
}