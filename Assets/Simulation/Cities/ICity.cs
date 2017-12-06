using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities {

    public interface ICity {

        #region properties

        IMapTile Location { get; }

        Transform transform { get; }

        int Population { get; set; }

        int FoodStockpile { get; set; }
        int CultureStockpile { get; set; }

        ResourceSummary LastIncome { get; }

        IProductionProject ActiveProject { get; }

        ResourceFocusType ResourceFocus { get; set; }

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
