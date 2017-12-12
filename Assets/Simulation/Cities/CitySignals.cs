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

        public ISubject<ICity>                 CityClickedSignal           { get; private set; }
        public CityProjectChangedSignal        ProjectChangedSignal        { get; private set; }
        public CityDistributionPerformedSignal DistributionPerformedSignal { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public CitySignals(
            [Inject(Id = "City Clicked Subject")] ISubject<ICity> cityClickedSignal,
            CityProjectChangedSignal projectChangedSignal,
            CityDistributionPerformedSignal distributionPerformedSignal
        ){
            CityClickedSignal           = cityClickedSignal;
            ProjectChangedSignal        = projectChangedSignal;
            DistributionPerformedSignal = distributionPerformedSignal;
        }

        #endregion

    }

    public class CityProjectChangedSignal : Signal<CityProjectChangedSignal, ICity, IProductionProject> { }

    public class CityDistributionPerformedSignal : Signal<CityDistributionPerformedSignal, ICity> { }

}
