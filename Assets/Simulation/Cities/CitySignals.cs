using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;

using Assets.Simulation.Cities.Production;

namespace Assets.Simulation.Cities {

    public class CitySignals {

        #region instance fields and properties

        public CityClickedSignal               ClickedSignal               { get; private set; }
        public CityProjectChangedSignal        ProjectChangedSignal        { get; private set; }
        public CityDistributionPerformedSignal DistributionPerformedSignal { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public CitySignals(CityClickedSignal clickedSignal, CityProjectChangedSignal projectChangedSignal,
            CityDistributionPerformedSignal distributionPerformedSignal) {
            ClickedSignal               = clickedSignal;
            ProjectChangedSignal        = projectChangedSignal;
            DistributionPerformedSignal = distributionPerformedSignal;
        }

        #endregion

    }

    public class CityClickedSignal : Signal<CityClickedSignal, ICity, PointerEventData> { }

    public class CityProjectChangedSignal : Signal<CityProjectChangedSignal, ICity, IProductionProject> { }

    public class CityDistributionPerformedSignal : Signal<CityDistributionPerformedSignal, ICity> { }

}
