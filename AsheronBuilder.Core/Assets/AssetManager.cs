// AsheronBuilder.Core/Assets/AssetManager.cs

using System.Collections.Concurrent;
using AsheronBuilder.Core.DatTypes;
using AsheronBuilder.Core.Constants;
using Environment = AsheronBuilder.Core.DatTypes.Environment;
using ACClientLib.DatReaderWriter;
using ACClientLib.DatReaderWriter.Options;

namespace AsheronBuilder.Core.Assets
{
    public class AssetManager
    {
        private readonly string _datPath;
        private readonly ConcurrentDictionary<uint, Texture> _textures = new();
        private readonly ConcurrentDictionary<uint, GfxObj> _models = new();
        private readonly ConcurrentDictionary<uint, Environment> _environments = new();
        private DatManager _datManager;

        public AssetManager(string datPath)
        {
            _datPath = datPath;
            InitializeDatManager();
        }

        private void InitializeDatManager()
        {
            _datManager = new DatManager(options =>
            {
                options.DatDirectory = _datPath;
                options.IndexCachingStrategy = IndexCachingStrategy.Upfront;
            });
        }

        public async Task LoadAssetsAsync()
        {
            var textureIds = GetTextureFileIds();
            var modelIds = GetModelFileIds();
            var environmentIds = GetEnvironmentFileIds();

            var tasks = new List<Task>();

            foreach (var id in textureIds)
            {
                tasks.Add(LoadTextureAsync(id));
            }

            foreach (var id in modelIds)
            {
                tasks.Add(LoadModelAsync(id));
            }

            foreach (var id in environmentIds)
            {
                tasks.Add(LoadEnvironmentAsync(id));
            }

            await Task.WhenAll(tasks);
        }

        private async Task LoadTextureAsync(uint fileId)
        {
            await Task.Run(() =>
            {
                var texture = LoadTexture(fileId);
                _textures[fileId] = texture;
            });
        }

        private async Task LoadModelAsync(uint fileId)
        {
            await Task.Run(() =>
            {
                var model = LoadModel(fileId);
                _models[fileId] = model;
            });
        }

        private async Task LoadEnvironmentAsync(uint fileId)
        {
            await Task.Run(() =>
            {
                var environment = LoadEnvironment(fileId);
                _environments[fileId] = environment;
            });
        }

        public List<uint> GetTextureFileIds()
        {
            return new List<uint>(_datManager.Portal.Tree.Where(f => f.Id >> 24 == (uint)FileType.Texture).Select(f => f.Id));
        }

        public List<uint> GetModelFileIds()
        {
            return new List<uint>(_datManager.Portal.Tree.Where(f => f.Id >> 24 == (uint)FileType.GraphicsObject).Select(f => f.Id));
        }

        public List<uint> GetEnvironmentFileIds()
        {
            return new List<uint>(_datManager.Portal.Tree.Where(f => f.Id >> 24 == (uint)FileType.Environment).Select(f => f.Id));
        }

        private Texture LoadTexture(uint fileId)
        {
            if (_datManager.Portal.TryReadFile(fileId, out Texture texture))
            {
                return texture;
            }
            throw new FileNotFoundException($"Texture with ID {fileId} not found.");
        }

        private GfxObj LoadModel(uint fileId)
        {
            if (_datManager.Portal.TryReadFile(fileId, out GfxObj model))
            {
                return model;
            }
            throw new FileNotFoundException($"Model with ID {fileId} not found.");
        }

        private Environment LoadEnvironment(uint fileId)
        {
            if (_datManager.Portal.TryReadFile(fileId, out Environment environment))
            {
                return environment;
            }
            throw new FileNotFoundException($"Environment with ID {fileId} not found.");
        }
    }
}