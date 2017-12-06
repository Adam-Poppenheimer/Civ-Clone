using System;

using UniRx;

using Assets.Simulation.Cities;

namespace Assets.UI {

    public interface IDisplaySignalLogic<T> where T : class {

        #region properties

        IObservable<T> CloseDisplayRequested { get; }
        IObservable<T> OpenDisplayRequested { get; }

        #endregion

    }

}