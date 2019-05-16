using Assets.Simulation.Players;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Core {

    public interface IGameCore {

        #region properties

        IPlayer       ActivePlayer { get; set; }
        ICivilization ActiveCiv    { get; }

        #endregion

        #region methods

        void BeginRound();
        void EndRound();

        #endregion

    }

}