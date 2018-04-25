using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Civilizations {

    public class VisibilityResponder {

        #region instance fields and properties

        public bool UpdateVisibility { get; set; }

        private Coroutine ResetVisionCoroutine;



        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICellVisibilityCanon                          VisibilityCanon;
        private ICityLineOfSightLogic                         CityLineOfSightLogic;
        private IUnitLineOfSightLogic                         UnitLineOfSightLogic;
        private IUnitFactory                                  UnitFactory;
        private ICityFactory                                  CityFactory;
        private MonoBehaviour                                 CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public VisibilityResponder(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICellVisibilityCanon visibilityCanon,
            ICityLineOfSightLogic cityLineOfSightLogic,
            IUnitLineOfSightLogic unitLineOfSightLogic,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker,
            IUnitFactory unitFactory, 
            ICityFactory cityFactory,
            UnitSignals unitSignals,
            CitySignals citySignals,
            HexCellSignals cellSignals,
            CivilizationSignals civSignals
        ){
            UnitPossessionCanon  = unitPossessionCanon;
            CityPossessionCanon  = cityPossessionCanon;
            VisibilityCanon      = visibilityCanon;
            CityLineOfSightLogic = cityLineOfSightLogic;
            UnitLineOfSightLogic = unitLineOfSightLogic;
            UnitFactory          = unitFactory;
            CityFactory          = cityFactory;
            CoroutineInvoker     = coroutineInvoker;

            unitSignals.LeftLocationSignal   .Subscribe(OnUnitLeftLocation);
            unitSignals.EnteredLocationSignal.Subscribe(OnUnitEnteredLocation);

            citySignals.LostCellFromBoundariesSignal.Subscribe(OnCityLostCell);
            citySignals.GainedCellToBoundariesSignal.Subscribe(OnCityGainedCell);

            cellSignals.FoundationElevationChangedSignal.Subscribe(OnHexCellVisibilityPropertiesChanged);
            cellSignals.WaterLevelChangedSignal         .Subscribe(OnHexCellVisibilityPropertiesChanged);
            cellSignals.ShapeChangedSignal              .Subscribe(OnHexCellVisibilityPropertiesChanged);
            cellSignals.FeatureChangedSignal            .Subscribe(OnHexCellVisibilityPropertiesChanged);

            civSignals.CivLosingCitySignal.Subscribe(OnCivLosingCity);
            civSignals.CivGainedCitySignal.Subscribe(OnCivGainedCity);
        }

        #endregion

        #region instance methods

        private IEnumerator ResetVisibility() {
            yield return new WaitForEndOfFrame();

            VisibilityCanon.ClearVisibility();

            foreach(var unit in UnitFactory.AllUnits) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                foreach(var visibleCell in UnitLineOfSightLogic.GetCellsVisibleToUnit(unit)) {
                    VisibilityCanon.IncreaseVisibilityToCiv(visibleCell, unitOwner);
                }
            }

            foreach(var city in CityFactory.AllCities) {
                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

                foreach(var visibleCell in CityLineOfSightLogic.GetCellsVisibleToCity(city)) {
                    VisibilityCanon.IncreaseVisibilityToCiv(visibleCell, cityOwner);
                }
            }

            ResetVisionCoroutine = null;
        }

        private void OnUnitLeftLocation(Tuple<IUnit, IHexCell> args) {
            TryResetAllVisibility();
        }

        private void OnUnitEnteredLocation(Tuple<IUnit, IHexCell> args) {
            TryResetAllVisibility();
        }

        private void OnCityLostCell(Tuple<ICity, IHexCell> args) {
            TryResetAllVisibility();
        }

        private void OnCityGainedCell(Tuple<ICity, IHexCell> args) {
            TryResetAllVisibility();
        }

        private void OnHexCellVisibilityPropertiesChanged(IHexCell cell) {
            TryResetAllVisibility();
        }

        private void OnCivLosingCity(Tuple<ICivilization, ICity> data) {
            TryResetAllVisibility();
        }

        private void OnCivGainedCity(Tuple<ICivilization, ICity> data) {
            TryResetAllVisibility();
        }

        private void TryResetAllVisibility() {
            if(ResetVisionCoroutine == null && UpdateVisibility && !(CoroutineInvoker == null)) {
                ResetVisionCoroutine = CoroutineInvoker.StartCoroutine(ResetVisibility());
            }
        }

        #endregion

    }

}
