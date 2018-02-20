using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.MapManagement;

namespace Assets.UI {

    public class TestDisplay : Civilizations.CivilizationDisplayBase {

        private SerializableMapData MapData;

        [SerializeField] private MapManager MapManager;

        public void Serialize() {
            MapData = MapManager.ComposeRuntimeIntoData();
        }

        public void Deserialize() {
            MapManager.DecomposeDataIntoRuntime(MapData);
        }

    }

}
