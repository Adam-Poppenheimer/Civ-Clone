using System;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianBrainFilterLogic {

        #region methods

        Func<IHexCell, bool> GetCaptureCivilianFilter(IUnit captor);
        Func<IHexCell, bool> GetMeleeAttackFilter    (IUnit attacker);
        Func<IHexCell, bool> GetRangedAttackFilter   (IUnit attacker);

        #endregion

    }

}