using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapManagement {

    public interface IFileSystemLiaison {

        #region properties

        ReadOnlyCollection<SerializableMapData> SavedGames    { get; }
        ReadOnlyCollection<SerializableMapData> AvailableMaps { get; }

        #endregion

        #region methods

        void WriteMapDataAsSavedGameToFile(SerializableMapData map, string filename);

        void WriteMapDataAsMapToFile(SerializableMapData map, string filename);

        void DeleteSavedGame(string filename);

        void RefreshSavedGames();

        void RefreshMaps();

        #endregion

    }

}
