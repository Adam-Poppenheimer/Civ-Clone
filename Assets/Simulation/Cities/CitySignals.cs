using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities.Production;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// Aggregates all city-related signals and events for convenient access.
    /// </summary>
    public class CitySignals {

        #region instance fields and properties

        /// <summary>
        /// Signal that fires whenever a city is clicked.
        /// </summary>
        public ISubject<ICity>                 CityClickedSignal           { get; private set; }

        /// <summary>
        /// Signal that fires whenever a city's ActiveProject is changed.
        /// </summary>
        public CityProjectChangedSignal        ProjectChangedSignal        { get; private set; }

        /// <summary>
        /// Signal that fires whenever a city has its PerformDistribution method called.
        /// </summary>
        public CityDistributionPerformedSignal DistributionPerformedSignal { get; private set; }

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cityClickedSignal"></param>
        /// <param name="projectChangedSignal"></param>
        /// <param name="distributionPerformedSignal"></param>
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

    /// <summary>
    /// 
    /// </summary>
    public class CityProjectChangedSignal : Signal<CityProjectChangedSignal, ICity, IProductionProject> { }

    /// <summary>
    /// 
    /// </summary>
    public class CityDistributionPerformedSignal : Signal<CityDistributionPerformedSignal, ICity> { }

}
