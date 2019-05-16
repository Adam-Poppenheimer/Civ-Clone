using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IVegetationPainter {

        #region methods

        void PaintVegetation(MapRegion region, IRegionBiomeTemplate template);

        #endregion

    }

}
