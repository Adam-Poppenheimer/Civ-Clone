using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Core {

    public interface IGameCore {

        #region properties

        ICivilization ActiveCivilization { get; set; }

        #endregion

        #region methods

        void BeginRound();
        void EndRound();

        #endregion

    }

}