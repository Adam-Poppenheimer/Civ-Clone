using Assets.Simulation.GameMap;
using UniRx;

namespace Assets.UI.GameMap {

    public interface IMapTileSignalLogic {

        #region properties

        IObservable<IMapTile> BeginHoverSignal { get; }
        IObservable<IMapTile> EndHoverSignal { get; }

        #endregion

    }

}