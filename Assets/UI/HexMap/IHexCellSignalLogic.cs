using System;

using UniRx;

using Assets.Simulation.HexMap;


namespace Assets.UI.HexMap {

    public interface IHexCellSignalLogic {

        #region properties

        UniRx.IObservable<IHexCell> BeginHoverSignal { get; }
        UniRx.IObservable<IHexCell> EndHoverSignal { get; }

        #endregion

    }

}