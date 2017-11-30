using System;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Core {

    public interface ITurnExecuter {

        #region methods

        void BeginTurnOnCity(ICity city);
        void EndTurnOnCity  (ICity city);

        void BeginTurnOnCivilization(ICivilization civilization);
        void EndTurnOnCivilization  (ICivilization civilization);

        #endregion

    }

}