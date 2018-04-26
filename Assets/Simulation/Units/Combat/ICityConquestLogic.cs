using System;

namespace Assets.Simulation.Units.Combat {

    public interface ICityConquestLogic {

        #region methods

        void HandleCityCaptureFromCombat(IUnit attacker, IUnit defendingFacade, CombatInfo combatInfo);

        #endregion

    }

}