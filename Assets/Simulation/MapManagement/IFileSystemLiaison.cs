using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapManagement {

    public interface IFileSystemLiaison {

        #region properties

        ReadOnlyCollection<MapFileData> SavedGames    { get; }
        ReadOnlyCollection<MapFileData> AvailableMaps { get; }

        #endregion

        #region methods

        void WriteMapDataToFile(SerializableMapData map, string filename);

        void DeleteMap(string filename);

        void RefreshMaps();

        #endregion

    }

}
