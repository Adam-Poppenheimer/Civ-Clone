using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    public interface ICityConfig {

        #region properties

        int MaxBorderRange { get; }

        int   TileCostBase                 { get; }
        int   PreviousTileCountCoefficient { get; }
        float PreviousTileCountExponent    { get; }

        ResourceSummary UnemployedYield { get; }

        float HurryCostPerProduction { get; }

        int FoodConsumptionPerPerson { get; }

        int   BaseGrowthStockpile                 { get; }
        int   GrowthPreviousPopulationCoefficient { get; }
        float GrowthPreviousPopulationExponent    { get; }

        #endregion

    }

}
