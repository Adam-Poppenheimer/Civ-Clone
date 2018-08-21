using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class OceanGenerator : IOceanGenerator {

        #region instance fields and properties

        private ICellModificationLogic ModLogic;

        #endregion

        #region constructors

        [Inject]
        public OceanGenerator(
            ICellModificationLogic modLogic
        ) {
            ModLogic = modLogic;
        }

        #endregion

        #region instance methods

        #region from IOceanGenerator

        public OceanData GetOceanData(IEnumerable<MapSection> oceanSections, IMapTemplate mapTemplate) {
            var oceanCells = oceanSections.SelectMany(section => section.Cells).ToList();

            var oceanRegions = new List<MapRegion>() {
                new MapRegion(new List<IHexCell>(), oceanCells)
            };

            return new OceanData(oceanRegions);
        }

        public void GenerateTopologyAndEcology(OceanData oceanData) {
            foreach(var region in oceanData.OceanRegions) {
                foreach(var cell in region.Cells) {
                    ModLogic.ChangeTerrainOfCell(cell, CellTerrain.DeepWater);
                }
            }
        }

        public void DistributeYieldAndResources(OceanData data) {
            
        }

        #endregion

        #endregion

    }

}
