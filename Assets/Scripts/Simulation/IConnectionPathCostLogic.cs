using System;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation {

    public interface IConnectionPathCostLogic {

        #region methods

        Func<IHexCell, IHexCell, float> BuildCivConnectionPathCostFunction    (ICivilization civOne, ICivilization civTwo);
        Func<IHexCell, IHexCell, float> BuildCapitalConnectionPathCostFunction(ICivilization domesticCiv);

        #endregion

    }
}