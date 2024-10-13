using ACClientLib.DatReaderWriter;
using ACClientLib.DatReaderWriter.Options;
using ACDungeonBuilder.Core.DatTypes;
using Environment = ACDungeonBuilder.Core.DatTypes.Environment;

namespace ACDungeonBuilder.Core.IO
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

        // Add more methods as needed for other asset types
    }
}