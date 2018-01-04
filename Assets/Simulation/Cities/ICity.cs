using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units;

namespace Assets.Simulation.Cities {

    public interface ICity {

        #region properties

        IHexCell Location { get; }

        Transform transform { get; }

        int Population { get; set; }

        int FoodStockpile { get; set; }
        int CultureStockpile { get; set; }

        ResourceSummary LastIncome { get; }

        IProductionProject ActiveProject { get; }

        ResourceFocusType ResourceFocus { get; set; }

        IHexCell TileBeingPursued { get; }

        #endregion

        #region instance methods

        void SetActiveProductionProject(IBuildingTemplate template);
        void SetActiveProductionProject(IUnitTemplate template);

        void PerformGrowth();
        void PerformProduction();
        void PerformExpansion();
        void PerformDistribution();

        void PerformIncome();

        #endregion

    }

}
