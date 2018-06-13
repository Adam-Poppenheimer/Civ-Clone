using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexCellTriangulator : IHexCellTriangulator {

        #region instance fields and properties

        private IHexGrid                  Grid;
        private IRiverTriangulator        RiverTriangulator;
        private IHexGridMeshBuilder       MeshBuilder;
        private ICultureTriangulator      CultureTriangulator;
        private IBasicTerrainTriangulator BasicTerrainTriangulator;
        private IWaterTriangulator        WaterTriangulator;
        private IRoadTriangulator         RoadTriangulator;

        #endregion

        #region constructors

        [Inject]
        public HexCellTriangulator(
            IHexGrid grid, IRiverTriangulator riverTriangulator,
            IHexGridMeshBuilder meshBuilder, ICultureTriangulator cultureTriangulator,
            IBasicTerrainTriangulator basicTerrainTriangulator,
            IWaterTriangulator waterTriangulator, IRoadTriangulator roadTriangulator
        ) {
            Grid                     = grid;
            RiverTriangulator        = riverTriangulator;
            MeshBuilder              = meshBuilder;
            CultureTriangulator      = cultureTriangulator;
            BasicTerrainTriangulator = basicTerrainTriangulator;
            WaterTriangulator        = waterTriangulator;
            RoadTriangulator         = roadTriangulator;
        }

        #endregion

        #region instance methods

        #region HexCellTriangulationLogic

        public void TriangulateCell(IHexCell cell) {
            for(HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; ++direction) {
                TriangulateInDirection(direction, cell);
            }
        }

        #endregion

        private void TriangulateInDirection(HexDirection direction, IHexCell cell) {
            var previousNeighbor = Grid.GetNeighbor(cell, direction.Previous());
            var neighbor         = Grid.GetNeighbor(cell, direction);

            var data = MeshBuilder.GetTriangulationData(
                cell, previousNeighbor, neighbor, direction
            );

            BasicTerrainTriangulator.TriangulateTerrainCenter(data);

            if(RiverTriangulator.ShouldTriangulateRiver(data)) {
                RiverTriangulator.TriangulateRiver(data);
            }
            

            if(BasicTerrainTriangulator.ShouldTriangulateTerrainEdge(data)) {
                BasicTerrainTriangulator.TriangulateTerrainEdge(data);
            }

            if(WaterTriangulator.ShouldTriangulateWater(data)) {
                WaterTriangulator.TriangulateWater(data);
            }

            if(CultureTriangulator.ShouldTriangulateCulture(data)) {
                CultureTriangulator.TriangulateCulture(data);               
            }

            if(RoadTriangulator.ShouldTriangulateRoads(data)) {
                RoadTriangulator.TriangulateRoads(data);
            }
        }

        #endregion

    }

}
