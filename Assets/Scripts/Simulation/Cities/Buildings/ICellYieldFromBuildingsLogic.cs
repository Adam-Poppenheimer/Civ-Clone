using System.Collections.Generic;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities.Buildings {

    public interface ICellYieldFromBuildingsLogic {

        #region methods

        YieldSummary GetBonusCellYieldFromBuildings(
            IHexCell cell, IEnumerable<IBuildingTemplate> buildings
        );

        YieldSummary GetBonusCellYieldFromYieldModifications(
            IHexCell cell, IEnumerable<ICellYieldModificationData> cellMods
        );

        #endregion

    }

}