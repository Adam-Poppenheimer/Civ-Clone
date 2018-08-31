using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class JungleBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name { get { return "Jungle Balance Strategy"; } }

        #endregion

        private int ProductionIncreaseWeight_Hills  = 1000;
        private int ProductionIncreaseWeight_Plains = 1;




        private IHexGrid               Grid;
        private ICellModificationLogic ModLogic;
        private ICellScorer            CellScorer;

        #endregion

        #region constructors

        [Inject]
        public JungleBalanceStrategy(
            IHexGrid grid, ICellModificationLogic modLogic, ICellScorer cellScorer
        ) {
            Grid       = grid;
            ModLogic   = modLogic;
            CellScorer = cellScorer;
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(
            MapRegion region, RegionData regionData, YieldType type, out YieldSummary yieldAdded
        ) {
            yieldAdded = YieldSummary.Empty;
            return false;
        }

        public bool TryIncreaseScore(MapRegion region, RegionData regionData, out float scoreAdded) {
            var addJungleCandidates = region.Cells.Where(IncreaseScoreFilter);

            if(addJungleCandidates.Any()) {
                var newJungle = addJungleCandidates.Random();

                var oldScore = CellScorer.GetScoreOfCell(newJungle);

                ModLogic.ChangeVegetationOfCell(newJungle, CellVegetation.Jungle);

                scoreAdded = CellScorer.GetScoreOfCell(newJungle) - oldScore;
                return true;
            }else {
                scoreAdded = 0f;
                return false;
            }
        }

        public bool TryDecreaseScore(MapRegion region, RegionData regionData, out float scoreRemoved) {
            var removeJungleCandidates = region.Cells.Where(DecreaseScoreFilter);

            if(removeJungleCandidates.Any()) {
                var oldJungle = removeJungleCandidates.Random();

                var oldScore = CellScorer.GetScoreOfCell(oldJungle);

                ModLogic.ChangeVegetationOfCell(oldJungle, CellVegetation.None);

                scoreRemoved = oldScore - CellScorer.GetScoreOfCell(oldJungle);
                return true;
            }else {
                scoreRemoved = 0f;
                return false;
            }
        }

        #endregion

        private bool IncreaseScoreFilter(IHexCell cell) {
            if(ModLogic.CanChangeVegetationOfCell(cell, CellVegetation.Jungle)) {
                return Grid.GetNeighbors(cell).Count(neighbor => neighbor.Vegetation == CellVegetation.Jungle) >= 3;
            }else {
                return false;
            }
        }

        private bool DecreaseScoreFilter(IHexCell cell) {
            if(cell.Vegetation == CellVegetation.Jungle && ModLogic.CanChangeVegetationOfCell(cell, CellVegetation.None)) {
                return Grid.GetNeighbors(cell).Count(neighbor => neighbor.Vegetation == CellVegetation.None) >= 3;
            }else {
                return false;
            }
        }

        private int IncreaseProductionWeightFunction(IHexCell cell) {
            if(cell.Vegetation != CellVegetation.Jungle || !ModLogic.CanChangeVegetationOfCell(cell, CellVegetation.None)) {
                return 0;
            }else if(cell.Shape == CellShape.Hills) {
                return ProductionIncreaseWeight_Hills;
            }else if(cell.Terrain == CellTerrain.Plains) {
                return ProductionIncreaseWeight_Plains;
            }else {
                return 0;
            }
        }

        #endregion

    }

}
