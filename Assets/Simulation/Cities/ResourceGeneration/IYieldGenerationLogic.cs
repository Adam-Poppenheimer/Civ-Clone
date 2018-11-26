using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities.ResourceGeneration {
   
    public interface IYieldGenerationLogic {

        #region methods
        
        YieldSummary GetTotalYieldForCity(ICity city);

        YieldSummary GetTotalYieldForCity(ICity city, YieldSummary additionalBonuses);
        
        YieldSummary GetYieldOfUnemployedPersonForCity(ICity city);

        YieldSummary GetYieldOfCellForCity(IHexCell cell, ICity city);

        YieldSummary GetYieldOfBuildingForCity(IBuilding building, ICity city);

        YieldSummary GetYieldOfBuildingSlotsForCity(IBuilding building, ICity city);

        #endregion

    }

}
