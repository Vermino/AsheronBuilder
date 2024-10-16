// AsheronBuilder.Core/Assets/AssetManager.cs

using System.Collections.Concurrent;
using AsheronBuilder.Core.DatTypes;
using AsheronBuilder.Core.Constants;
using Environment = AsheronBuilder.Core.DatTypes.Environment;
using ACClientLib.DatReaderWriter;
using ACClientLib.DatReaderWriter.Options;
using AsheronBuilder.Core.Constants;

namespace AsheronBuilder.Core.Assets
{
    public class AssetManager
    {
        private readonly List<DatManager> _datManagers = new List<DatManager>();
        private readonly ConcurrentDictionary<uint, Texture> _textures = new();
        private readonly ConcurrentDictionary<uint, GfxObj> _models = new();
        private readonly ConcurrentDictionary<uint, Environment> _environments = new();
        private readonly string _assetsFolderPath;
        private bool _datFilesLoaded = false;

        public AssetManager(string assetsFolderPath)
        {
            _assetsFolderPath = assetsFolderPath;
        }

        public bool AreDatFilesPresent()
        {
            return Directory.GetFiles(_assetsFolderPath, "*.dat").Length > 0;
        }
        
        public async Task LoadAssetsAsync()
        {
            if (!AreDatFilesPresent())
            {
                _datFilesLoaded = false;
                return; // Don't attempt to load if no DAT files are present
            }
            
            try
            {
                var textureIds = GetAllTextureFileIds();
                var modelIds = GetAllModelFileIds();
                var environmentIds = GetAllEnvironmentFileIds();

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

                _datFilesLoaded = true;
                await Task.CompletedTask; // Placeholder for actual async operations
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadAssetsAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
            
        }
        
        public bool AreAssetsLoaded() => _datFilesLoaded;

        private void InitializeDatManager(string datFilePath)
        {
            var datManager = new DatManager(options =>
            {
                options.DatDirectory = Path.GetDirectoryName(datFilePath);
                options.IndexCachingStrategy = IndexCachingStrategy.Upfront;
            });

            _datManagers.Add(datManager);
        }

        public List<uint> GetAllTextureFileIds()
        {
            var textureIds = new List<uint>();
            foreach (var datManager in _datManagers)
            {
                textureIds.AddRange(datManager.Portal.Tree.Where(f => f.Id >> 24 == (uint)FileType.Texture).Select(f => f.Id));
            }
            return textureIds;
        }

        public List<uint> GetAllModelFileIds()
        {
            var modelIds = new List<uint>();
            foreach (var datManager in _datManagers)
            {
                modelIds.AddRange(datManager.Portal.Tree.Where(f => f.Id >> 24 == (uint)FileType.GraphicsObject).Select(f => f.Id));
            }
            return modelIds;
        }

        public List<uint> GetAllEnvironmentFileIds()
        {
            var environmentIds = new List<uint>();
            foreach (var datManager in _datManagers)
            {
                environmentIds.AddRange(datManager.Portal.Tree.Where(f => f.Id >> 24 == (uint)FileType.Environment).Select(f => f.Id));
            }
            return environmentIds;
        }
        
        private List<uint> GetFileIdsByType(FileType fileType)
        {
            var fileIds = new List<uint>();
            foreach (var datManager in _datManagers)
            {
                fileIds.AddRange(datManager.Portal.Tree.Where(f => f.Id >> 24 == (uint)fileType).Select(f => f.Id));
            }
            return fileIds;
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

        private Texture LoadTexture(uint fileId)
        {
            foreach (var datManager in _datManagers)
            {
                if (datManager.Portal.TryReadFile(fileId, out Texture texture))
                {
                    return texture;
                }
            }

            throw new FileNotFoundException($"Texture with ID {fileId} not found.");
        }

        private GfxObj LoadModel(uint fileId)
        {
            foreach (var datManager in _datManagers)
            {
                if (datManager.Portal.TryReadFile(fileId, out GfxObj model))
                {
                    return model;
                }
            }

            throw new FileNotFoundException($"Model with ID {fileId} not found.");
        }

        private Environment LoadEnvironment(uint fileId)
        {
            foreach (var datManager in _datManagers)
            {
                if (datManager.Portal.TryReadFile(fileId, out Environment environment))
                {
                    return environment;
                }
            }

            throw new FileNotFoundException($"Environment with ID {fileId} not found.");
        }
    }
}