using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;


using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class WaterRationalizer : IWaterRationalizer {

        #region instance fields and properties

        private IMapGenerationConfig   Config;
        private ICellModificationLogic ModLogic;
        private IGridTraversalLogic    GridTraversalLogic;

        #endregion

        #region constructors

        [Inject]
        public WaterRationalizer(
            IMapGenerationConfig config, ICellModificationLogic modLogic,
            IGridTraversalLogic gridTraversalLogic
        ) {
            Config             = config;
            ModLogic           = modLogic;
            GridTraversalLogic = gridTraversalLogic;
        }

        #endregion

        #region instance methods

        #region from IWaterRationalizationLogic

        public void RationalizeWater(IEnumerable<IHexCell> cells) {
            var unrationalizedWater = cells.Where(cell => cell.Terrain.IsWater()).ToList();

            while(unrationalizedWater.Count > 0) {
                var waterBodySeed = unrationalizedWater[0];
                
                var cellsInWaterBody = new HashSet<IHexCell>();

                var waterBodyCrawler = GridTraversalLogic.GetCrawlingEnumerator(
                    waterBodySeed, unrationalizedWater, cellsInWaterBody,
                    WaterBodyWeightFunction
                );

                while(waterBodyCrawler.MoveNext()) {
                    cellsInWaterBody.Add(waterBodyCrawler.Current);
                    unrationalizedWater.Remove(waterBodyCrawler.Current);
                }

                if(cellsInWaterBody.Count <= Config.MaxLakeSize) {
                    foreach(var cell in cellsInWaterBody) {
                        ModLogic.ChangeTerrainOfCell(cell, CellTerrain.FreshWater);
                    }
                }
            }
        }

        #endregion

        private int WaterBodyWeightFunction(IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells) {
            return cell.Terrain.IsWater() && !acceptedCells.Contains(cell) ? 1 : -1;
        }

        #endregion
        
    }

}
