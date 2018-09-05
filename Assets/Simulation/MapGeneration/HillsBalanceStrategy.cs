using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class HillsBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name { get { return "Hills Balance Strategy"; } }

        #endregion

        private IYieldEstimator                                  YieldEstimator;
        private IMapScorer                                       MapScorer;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private ICellModificationLogic                           ModLogic;
        private ITechCanon                                       TechCanon;

        #endregion

        #region constructors

        [Inject]
        public HillsBalanceStrategy(
            IYieldEstimator yieldEstimator, IMapScorer mapScorer,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            ICellModificationLogic modLogic, ITechCanon techCanon
        ) {
            YieldEstimator    = yieldEstimator;
            MapScorer         = mapScorer;
            NodeLocationCanon = nodeLocationCanon;
            ModLogic          = modLogic;
            TechCanon         = techCanon;
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(
            MapRegion region, RegionData regionData, YieldType type, out YieldSummary yieldAdded
        ) {
            if(type != YieldType.Production) {
                yieldAdded = YieldSummary.Empty;
                return false;
            }

            var candidates = region.Cells.Where(HillCandidateFilter_ProductionIncreasing);

            if(candidates.Any()) {
                var newHill = candidates.Random();

                var oldYield = YieldEstimator.GetYieldEstimateForCell(newHill, TechCanon.AvailableTechs);

                ModLogic.ChangeShapeOfCell(newHill, CellShape.Hills);

                var newYield = YieldEstimator.GetYieldEstimateForCell(newHill, TechCanon.AvailableTechs);

                yieldAdded = newYield - oldYield;
                return true;
            }else {
                yieldAdded = YieldSummary.Empty;
                return false;
            }
        }

        public bool TryIncreaseScore(MapRegion region, RegionData regionData, out float scoreAdded) {
            YieldSummary yieldAdded;

            if(TryIncreaseYield(region, regionData, YieldType.Production, out yieldAdded)) {
                scoreAdded = MapScorer.GetScoreOfYield(yieldAdded);
                return true;
            }else {
                scoreAdded = 0f;
                return false;
            }
        }

        public bool TryDecreaseScore(MapRegion region,RegionData regionData, out float scoreRemoved) {
            scoreRemoved = 0f;
            return false;
        }

        #endregion

        private bool HillCandidateFilter_ScoreIncreasing(IHexCell cell) {
            if(NodeLocationCanon.GetPossessionsOfOwner(cell).Any()) {
                return false;
            }

            return cell.Shape == CellShape.Flatlands && cell.Vegetation == CellVegetation.None && (
                cell.Terrain == CellTerrain.Desert || cell.Terrain == CellTerrain.Tundra ||
                cell.Terrain == CellTerrain.Snow
            );
        }

        private bool HillCandidateFilter_ProductionIncreasing(IHexCell cell) {
            return !NodeLocationCanon.GetPossessionsOfOwner(cell).Any()
                && !cell.Terrain.IsWater()
                && cell.Shape != CellShape.Hills
                && cell.Vegetation == CellVegetation.None;
        }

        #endregion

    }

}
