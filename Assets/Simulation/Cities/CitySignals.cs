using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// Aggregates all city-related signals and events for convenient access.
    /// </summary>
    public class CitySignals {

        #region instance fields and properties

        /// <summary>
        /// Signal that fires whenever a city is clicked.
        /// </summary>
        public ISubject<ICity> PointerClicked { get; private set; }

        public ISubject<ICity> PointerEntered { get; private set; }
        public ISubject<ICity> PointerExited  { get; private set; }

        /// <summary>
        /// Signal that fires whenever a city's ActiveProject is changed.
        /// </summary>
        public ISubject<Tuple<ICity, IProductionProject>> ProjectChanged { get; private set; }

        /// <summary>
        /// Signal that fires whenever a city has its PerformDistribution method called.
        /// </summary>
        public ISubject<ICity> DistributionPerformed { get; private set; }

        public ISubject<ICity> BeingDestroyed { get; private set; }

        public ISubject<Tuple<ICity, IHexCell>> LostCellFromBoundaries { get; private set; }
        public ISubject<Tuple<ICity, IHexCell>> GainedCellToBoundaries { get; private set; }

        public ISubject<CityCaptureData> CityCaptured { get; private set; }

        public ISubject<Tuple<ICity, IBuilding>> GainedBuilding { get; private set; }
        public ISubject<Tuple<ICity, IBuilding>> LostBuilding   { get; private set; }

        public ISubject<ICity> PopulationChanged { get; private set; }

        public ISubject<ICity> FoodStockpileChanged { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public CitySignals(){
            ProjectChanged        = new Subject<Tuple<ICity, IProductionProject>>();
            DistributionPerformed = new Subject<ICity>();

            PointerClicked = new Subject<ICity>();
            PointerEntered = new Subject<ICity>();
            PointerExited  = new Subject<ICity>();

            BeingDestroyed = new Subject<ICity>();

            LostCellFromBoundaries = new Subject<Tuple<ICity, IHexCell>>();
            GainedCellToBoundaries = new Subject<Tuple<ICity, IHexCell>>();

            CityCaptured = new Subject<CityCaptureData>();

            GainedBuilding = new Subject<Tuple<ICity, IBuilding>>();
            LostBuilding   = new Subject<Tuple<ICity, IBuilding>>();

            PopulationChanged = new Subject<ICity>();

            FoodStockpileChanged = new Subject<ICity>();
        }

        #endregion

    }

}
