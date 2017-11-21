using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.GameMap;
using Assets.Cities.Production;


namespace Assets.Cities {

    public interface ICity {

        #region properties

        IMapTile Location { get; }

        int Population { get; set; }

        int FoodStockpile { get; set; }
        int CultureStockpile { get; set; }

        ResourceSummary LastIncome { get; }

        IProductionProject CurrentProject { get; }

        DistributionPreferences DistributionPreferences { get; set; }

        IMapTile TileBeingPursued { get; }

        #endregion

        #region instance methods

        void SetCurrentProject(IProductionProject project);

        void PerformGrowth();
        void PerformProduction();
        void PerformExpansion();
        void PerformDistribution();

        void PerformIncome();

        #endregion

    }

}
