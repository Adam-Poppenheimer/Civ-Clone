using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities.Production;

namespace Assets.Simulation.Cities {

    public class CitySignals {

        #region instance fields and properties

        public ISubject<ICity>                 SelectRequestedSignal       { get; private set; }
        public CityProjectChangedSignal        ProjectChangedSignal        { get; private set; }
        public CityDistributionPerformedSignal DistributionPerformedSignal { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public CitySignals(
            [Inject(Id = "Select Requested Subject")] ISubject<ICity> selectRequestedSignal,
            CityProjectChangedSignal projectChangedSignal,
            CityDistributionPerformedSignal distributionPerformedSignal
        ){
            SelectRequestedSignal       = selectRequestedSignal;
            ProjectChangedSignal        = projectChangedSignal;
            DistributionPerformedSignal = distributionPerformedSignal;
        }

        #endregion

    }

    public class CityProjectChangedSignal : Signal<CityProjectChangedSignal, ICity, IProductionProject> { }

    public class CityDistributionPerformedSignal : Signal<CityDistributionPerformedSignal, ICity> { }

}
