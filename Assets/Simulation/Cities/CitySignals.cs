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

        public ISubject<UniRx.Tuple<ICity, IHexCell>> CityAddedToLocation     { get; private set; }
        public ISubject<UniRx.Tuple<ICity, IHexCell>> CityRemovedFromLocation { get; private set; }

        public ISubject<ICity> PointerClicked { get; private set; }

        public ISubject<ICity> PointerEntered { get; private set; }
        public ISubject<ICity> PointerExited  { get; private set; }

        public ISubject<UniRx.Tuple<ICity, IProductionProject>> ProjectChanged { get; private set; }

        public ISubject<ICity> DistributionPerformed { get; private set; }

        public ISubject<ICity> BeingDestroyed { get; private set; }

        public ISubject<UniRx.Tuple<ICity, IHexCell>> LostCellFromBoundaries { get; private set; }
        public ISubject<UniRx.Tuple<ICity, IHexCell>> GainedCellToBoundaries { get; private set; }

        public ISubject<CityCaptureData> CityCaptured { get; private set; }

        public ISubject<UniRx.Tuple<ICity, IBuilding>> GainedBuilding { get; private set; }
        public ISubject<UniRx.Tuple<ICity, IBuilding>> LostBuilding   { get; private set; }

        public ISubject<ICity> PopulationChanged { get; private set; }

        public ISubject<ICity> FoodStockpileChanged { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public CitySignals(){
            CityAddedToLocation     = new Subject<UniRx.Tuple<ICity, IHexCell>>();
            CityRemovedFromLocation = new Subject<UniRx.Tuple<ICity, IHexCell>>();

            ProjectChanged        = new Subject<UniRx.Tuple<ICity, IProductionProject>>();
            DistributionPerformed = new Subject<ICity>();

            PointerClicked = new Subject<ICity>();
            PointerEntered = new Subject<ICity>();
            PointerExited  = new Subject<ICity>();

            BeingDestroyed = new Subject<ICity>();

            LostCellFromBoundaries = new Subject<UniRx.Tuple<ICity, IHexCell>>();
            GainedCellToBoundaries = new Subject<UniRx.Tuple<ICity, IHexCell>>();

            CityCaptured = new Subject<CityCaptureData>();

            GainedBuilding = new Subject<UniRx.Tuple<ICity, IBuilding>>();
            LostBuilding   = new Subject<UniRx.Tuple<ICity, IBuilding>>();

            PopulationChanged = new Subject<ICity>();

            FoodStockpileChanged = new Subject<ICity>();
        }

        #endregion

    }

}
