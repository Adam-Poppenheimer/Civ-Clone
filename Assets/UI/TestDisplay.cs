using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapManagement;

namespace Assets.UI {

    public class TestDisplay : Civilizations.CivilizationDisplayBase {

        private IMapComposer MapManager;

        private IFileSystemLiaison FileSystemLiaison;

        [Inject]
        public void InjectDependencies(IMapComposer mapManager, IFileSystemLiaison fileSystemLiaison) {
            MapManager        = mapManager;
            FileSystemLiaison = fileSystemLiaison;
        }

        public void Serialize() {
            var mapData = MapManager.ComposeRuntimeIntoData();

            FileSystemLiaison.WriteMapDataAsSavedGameToFile(mapData, "Test Saved Game");
        }

        public void Deserialize() {
            FileSystemLiaison.RefreshSavedGames();

            var mapFile = FileSystemLiaison.SavedGames.First();

            MapManager.DecomposeDataIntoRuntime(mapFile.MapData);
        }

    }

}
