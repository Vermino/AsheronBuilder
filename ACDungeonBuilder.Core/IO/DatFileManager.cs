using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using Environment = ACE.DatLoader.FileTypes.Environment;

namespace ACDungeonBuilder.Core.IO
{
    public class DatFileManager
    {
        private DatDatabase _portalDat;
        private DatDatabase _cellDat;

        public void Initialize(string datFilePath)
        {
            _portalDat = new DatDatabase(datFilePath + "\\client_portal.dat");
            _cellDat = new DatDatabase(datFilePath + "\\client_cell_1.dat");
        }

        public GfxObj LoadGfxObj(uint fileId)
        {
            return _portalDat.ReadFromDat<GfxObj>(fileId);
        }

        public Texture LoadTexture(uint fileId)
        {
            return _portalDat.ReadFromDat<Texture>(fileId);
        }

        public Environment LoadEnvironment(uint fileId)
        {
            return _portalDat.ReadFromDat<Environment>(fileId);
        }

        public CellLandblock LoadLandblock(uint fileId)
        {
            return _cellDat.ReadFromDat<CellLandblock>(fileId);
        }

        // Add more methods as needed for other asset types
    }
}