using Assets.Simulation.Units;

namespace Assets.Simulation.Improvements {

    public interface IImprovementConstructionExecuter {

        #region methods

        void PerformImprovementConstruction(IUnit unit);

        #endregion

    }

}