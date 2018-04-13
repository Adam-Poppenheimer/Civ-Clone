using System;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Civilizations {

    public interface IConnectionPathCostLogic {

        #region methods

        Func<IHexCell, IHexCell, float> BuildPathCostFunction(ICivilization civOne, ICivilization civTwo);

        #endregion

    }
}