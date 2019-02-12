using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class TerrainAlphamapLogic : ITerrainAlphamapLogic {

        #region instance fields and properties

        private IMapRenderConfig       RenderConfig;
        private IHexGrid               Grid;
        private IPointOrientationLogic PointOrientationLogic;
        private ICellAlphamapLogic     CellAlphamapLogic;
        private ITerrainMixingLogic    TerrainMixingLogic;
        private INoiseGenerator        NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public TerrainAlphamapLogic(
            IMapRenderConfig renderConfig, IHexGrid grid, IPointOrientationLogic pointOrientationLogic,
            ICellAlphamapLogic cellAlphamapLogic, ITerrainMixingLogic terrainMixingLogic,
            INoiseGenerator noiseGenerator
        ) {
            RenderConfig          = renderConfig;
            Grid                  = grid;
            PointOrientationLogic = pointOrientationLogic;
            CellAlphamapLogic     = cellAlphamapLogic;
            TerrainMixingLogic    = terrainMixingLogic;
            NoiseGenerator        = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from ITerrainAlphamapLogic

        public float[] GetAlphamapForPosition(Vector3 position) {
            var perturbedPosition = NoiseGenerator.Perturb(position);

            if(Grid.HasCellAtLocation(perturbedPosition)) {
                var center = Grid.GetCellAtLocation(perturbedPosition);

                HexDirection sextant = PointOrientationLogic.GetSextantOfPointForCell(perturbedPosition, center);

                PointOrientation orientation = PointOrientationLogic.GetOrientationOfPointInCell(perturbedPosition, center, sextant);

                if(orientation == PointOrientation.Center || orientation == PointOrientation.Void) {
                    return CellAlphamapLogic.GetAlphamapForPositionForCell(perturbedPosition, center, sextant);
                }

                IHexCell right = Grid.GetNeighbor(center, sextant);

                if(orientation == PointOrientation.Edge) {
                    return TerrainMixingLogic.GetMixForEdgeAtPoint(
                        center, right, sextant, perturbedPosition, AlphamapSelector, AlphamapAggregator
                    );
                }

                if(orientation == PointOrientation.PreviousCorner) {
                    var left = Grid.GetNeighbor(center, sextant.Previous());

                    return TerrainMixingLogic.GetMixForPreviousCornerAtPoint(
                        center, left, right, sextant, perturbedPosition, AlphamapSelector, AlphamapAggregator
                    );
                }

                if(orientation == PointOrientation.NextCorner) {
                    var nextRight = Grid.GetNeighbor(center, sextant.Next());

                    return TerrainMixingLogic.GetMixForNextCornerAtPoint(
                        center, right, nextRight, sextant, perturbedPosition, AlphamapSelector, AlphamapAggregator
                    );
                }
            }

            return new float[RenderConfig.MapTextures.Count()];
        }

        #endregion

        private float[] AlphamapSelector(Vector3 position, IHexCell cell, HexDirection sextant, float weight) {
            var retval = CellAlphamapLogic.GetAlphamapForPositionForCell(position, cell, sextant);

            for(int i = 0; i < retval.Length; i++) {
                retval[i] *= weight;
            }

            return retval;
        }

        private float[] AlphamapAggregator(float[] alphamapOne, float[] alphamapTwo) {
            var newMap = new float[alphamapOne.Length];

            for(int i = 0; i < alphamapOne.Length; i++) {
                newMap[i] = alphamapOne[i] + alphamapTwo[i];
            }

            return newMap;
        }

        #endregion
        
    }

}
