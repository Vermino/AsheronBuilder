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

        public AssetManager(string assetsFolderPath)
        {
            Console.WriteLine($"Initializing AssetManager with path: {assetsFolderPath}");

            if (!Directory.Exists(assetsFolderPath))
            {
                Console.WriteLine($"Directory not found: {assetsFolderPath}");
                throw new DirectoryNotFoundException($"Assets folder not found: {assetsFolderPath}");
            }

            string[] datFiles = Directory.GetFiles(assetsFolderPath, "*.dat");
            Console.WriteLine($"Found {datFiles.Length} DAT files in {assetsFolderPath}");

            foreach (var file in datFiles)
            {
                Console.WriteLine($"DAT file: {file}");
            }

            if (datFiles.Length == 0)
            {
                Console.WriteLine($"No DAT files found in the Assets folder: {assetsFolderPath}");
                throw new FileNotFoundException("No DAT files found in the Assets folder.");
            }

            foreach (string datFile in datFiles)
            {
                InitializeDatManager(datFile);
            }

            Console.WriteLine($"Initialized {_datManagers.Count} DAT managers");
        }

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

        public async Task LoadAssetsAsync()
        {
            try
            {
                var textureIds = GetAllTextureFileIds();
                var modelIds = GetAllModelFileIds();
                var environmentIds = GetAllEnvironmentFileIds();

                Console.WriteLine(
                    $"Found {textureIds.Count} textures, {modelIds.Count} models, and {environmentIds.Count} environments");

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadAssetsAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
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