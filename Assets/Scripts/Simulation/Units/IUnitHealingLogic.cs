using System;

namespace Assets.Simulation.Units {

    public interface IUnitHealingLogic {

        #region methods

        void PerformHealingOnUnit(IUnit unit);

        #endregion

    }

}