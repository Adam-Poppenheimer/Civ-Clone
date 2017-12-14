using System;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Simulation.Core {

    public interface ITurnExecuter {

        #region methods

        void BeginTurnOnCity(ICity city);
        void EndTurnOnCity  (ICity city);

        void BeginTurnOnCivilization(ICivilization civilization);
        void EndTurnOnCivilization  (ICivilization civilization);

        void BeginTurnOnUnit(IUnit unit);
        void EndTurnOnUnit  (IUnit unit);

        #endregion

    }

}