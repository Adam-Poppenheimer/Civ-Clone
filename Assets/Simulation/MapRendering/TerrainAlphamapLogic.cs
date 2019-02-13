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

        private IMapRenderConfig         RenderConfig;
        private IHexGrid                 Grid;
        private IPointOrientationLogic   PointOrientationLogic;
        private ICellAlphamapLogic       CellAlphamapLogic;
        private ITerrainMixingLogic      TerrainMixingLogic;
        private INoiseGenerator          NoiseGenerator;
        private IAlphamapMixingFunctions AlphamapMixingFunctions;

        #endregion

        #region constructors

        [Inject]
        public TerrainAlphamapLogic(
            IMapRenderConfig renderConfig, IHexGrid grid, IPointOrientationLogic pointOrientationLogic,
            ICellAlphamapLogic cellAlphamapLogic, ITerrainMixingLogic terrainMixingLogic,
            INoiseGenerator noiseGenerator, IAlphamapMixingFunctions alphamapMixingFunctions
        ) {
            RenderConfig            = renderConfig;
            Grid                    = grid;
            PointOrientationLogic   = pointOrientationLogic;
            CellAlphamapLogic       = cellAlphamapLogic;
            TerrainMixingLogic      = terrainMixingLogic;
            NoiseGenerator          = noiseGenerator;
            AlphamapMixingFunctions = alphamapMixingFunctions;
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
                        center, right, sextant, perturbedPosition, AlphamapMixingFunctions.Selector, AlphamapMixingFunctions.Aggregator
                    );
                }

                if(orientation == PointOrientation.PreviousCorner) {
                    var left = Grid.GetNeighbor(center, sextant.Previous());

                    return TerrainMixingLogic.GetMixForPreviousCornerAtPoint(
                        center, left, right, sextant, perturbedPosition, AlphamapMixingFunctions.Selector, AlphamapMixingFunctions.Aggregator
                    );
                }

                if(orientation == PointOrientation.NextCorner) {
                    var nextRight = Grid.GetNeighbor(center, sextant.Next());

                    return TerrainMixingLogic.GetMixForNextCornerAtPoint(
                        center, right, nextRight, sextant, perturbedPosition, AlphamapMixingFunctions.Selector, AlphamapMixingFunctions.Aggregator
                    );
                }
            }

            return new float[RenderConfig.MapTextures.Count()];
        }

        #endregion

        #endregion
        
    }

}
