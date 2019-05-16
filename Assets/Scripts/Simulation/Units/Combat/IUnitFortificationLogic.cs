using System;

namespace Assets.Simulation.Units.Combat {

    public interface IUnitFortificationLogic {

        #region methods

        bool GetFortificationStatusForUnit(IUnit unit);
        void SetFortificationStatusForUnit(IUnit unit, bool isFortified);

        float GetFortificationModifierForUnit(IUnit unit);

        #endregion

    }

}