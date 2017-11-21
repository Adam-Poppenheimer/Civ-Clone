using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.GameMap;
using Assets.Cities.Production;
using Assets.Cities.Buildings;

namespace Assets.Cities {

    public interface ICity {

        #region properties

        IMapTile Location { get; }

        int Population { get; set; }

        int FoodStockpile { get; set; }
        int CultureStockpile { get; set; }

        ResourceSummary LastIncome { get; }

        IProductionProject ActiveProject { get; }

        DistributionPreferences DistributionPreferences { get; set; }

        IMapTile TileBeingPursued { get; }

        #endregion

        #region instance methods

        void SetActiveProductionProject(IBuildingTemplate template);

        void PerformGrowth();
        void PerformProduction();
        void PerformExpansion();
        void PerformDistribution();

        void PerformIncome();

        #endregion

    }

}
