using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionResourceTemplate {

        #region properties

        float StrategicNodesPerCell  { get; }
        float StrategicCopiesPerCell { get; }

        bool BalanceResources { get; }

        float MinFoodPerCell       { get; }
        float MinProductionPerCell { get; }

        float MinScorePerCell { get; }
        float MaxScorePerCell { get; }

        #endregion

    }

}
