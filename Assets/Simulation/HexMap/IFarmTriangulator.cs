using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IFarmTriangulator {

        bool ShouldTriangulateFarmCenter(CellTriangulationData data);
        void TriangulateFarmCenter      (CellTriangulationData data);

        bool ShouldTriangulateFarmEdge(CellTriangulationData data);
        void TriangulateFarmEdge      (CellTriangulationData data);

        bool ShouldTriangulateFarmCorner(CellTriangulationData data);
        void TriangulateFarmCorner      (CellTriangulationData data);

    }

}
