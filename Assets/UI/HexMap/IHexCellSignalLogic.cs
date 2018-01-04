using System;

using UniRx;

using Assets.Simulation.HexMap;


namespace Assets.UI.HexMap {

    public interface IHexCellSignalLogic {

        #region properties

        IObservable<IHexCell> BeginHoverSignal { get; }
        IObservable<IHexCell> EndHoverSignal { get; }

        #endregion

    }

}