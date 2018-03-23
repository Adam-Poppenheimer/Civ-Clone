using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities.ResourceGeneration {
   
    public interface IResourceGenerationLogic {

        #region methods
        
        ResourceSummary GetTotalYieldForCity(ICity city);

        ResourceSummary GetTotalYieldForCity(ICity city, ResourceSummary additionalBonuses);

        ResourceSummary GetBaseYieldOfCity(ICity city);
        
        ResourceSummary GetYieldOfUnemployedPersonForCity(ICity city);

        ResourceSummary GetYieldOfCellForCity(IHexCell cell, ICity city);

        ResourceSummary GetYieldOfBuildingForCity(IBuilding building, ICity city);

        ResourceSummary GetYieldOfBuildingSlotsForCity(IBuilding building, ICity city);

        #endregion

    }

}
