using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.AI {

    public interface IUnitComparativeStrengthEstimator {

        #region methods

        float EstimateComparativeDefensiveStrength(IUnit unit, IUnit attacker, IHexCell location);
        float EstimateComparativeStrength         (IUnit unit, IUnit defender, IHexCell location);

        #endregion

    }

}