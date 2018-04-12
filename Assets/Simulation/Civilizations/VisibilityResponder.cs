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
        private ILineOfSightLogic                             LineOfSightLogic;
        private IUnitFactory                                  UnitFactory;
        private ICityFactory                                  CityFactory;
        private MonoBehaviour                                 CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public VisibilityResponder(
            UnitSignals unitSignals, CitySignals citySignals,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICellVisibilityCanon visibilityCanon, ILineOfSightLogic lineOfSightLogic,
            IUnitFactory unitFactory,  ICityFactory cityFactory, HexCellSignals cellSignals,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker
        ){
            unitSignals.LeftLocationSignal      .Subscribe(OnUnitLeftLocation);
            unitSignals.EnteredLocationSignal   .Subscribe(OnUnitEnteredLocation);

            citySignals.LostCellFromBoundariesSignal.Subscribe(OnCityLostCell);
            citySignals.GainedCellToBoundariesSignal.Subscribe(OnCityGainedCell);

            UnitPossessionCanon = unitPossessionCanon;
            CityPossessionCanon = cityPossessionCanon;
            VisibilityCanon     = visibilityCanon;
            LineOfSightLogic    = lineOfSightLogic;
            UnitFactory         = unitFactory;
            CityFactory         = cityFactory;
            CoroutineInvoker    = coroutineInvoker;

            cellSignals.FoundationElevationChangedSignal.Subscribe(cell => ResetVisibility());
            cellSignals.WaterLevelChangedSignal         .Subscribe(cell => ResetVisibility());
            cellSignals.ShapeChangedSignal              .Subscribe(cell => ResetVisibility());
            cellSignals.FeatureChangedSignal            .Subscribe(cell => ResetVisibility());
        }

        #endregion

        #region instance methods

        private IEnumerator ResetVisibility() {
            yield return new WaitForEndOfFrame();

            VisibilityCanon.ClearVisibility();

            foreach(var unit in UnitFactory.AllUnits) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                foreach(var visibleCell in LineOfSightLogic.GetCellsVisibleToUnit(unit)) {
                    VisibilityCanon.IncreaseVisibilityToCiv(visibleCell, unitOwner);
                }
            }

            foreach(var city in CityFactory.AllCities) {
                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

                foreach(var visibleCell in LineOfSightLogic.GetCellsVisibleToCity(city)) {
                    VisibilityCanon.IncreaseVisibilityToCiv(visibleCell, cityOwner);
                }
            }

            ResetVisionCoroutine = null;
        }

        private void OnUnitLeftLocation(Tuple<IUnit, IHexCell> args) {
            if(ResetVisionCoroutine == null && UpdateVisibility && !(CoroutineInvoker == null)) {
                ResetVisionCoroutine = CoroutineInvoker.StartCoroutine(ResetVisibility());
            }
        }

        private void OnUnitEnteredLocation(Tuple<IUnit, IHexCell> args) {
            if(ResetVisionCoroutine == null && UpdateVisibility && !(CoroutineInvoker == null)) {
                ResetVisionCoroutine = CoroutineInvoker.StartCoroutine(ResetVisibility());
            }
        }

        private void OnCityLostCell(Tuple<ICity, IHexCell> args) {
            if(ResetVisionCoroutine == null && UpdateVisibility && !(CoroutineInvoker == null)) {
                ResetVisionCoroutine = CoroutineInvoker.StartCoroutine(ResetVisibility());
            }
        }

        private void OnCityGainedCell(Tuple<ICity, IHexCell> args) {
            if(ResetVisionCoroutine == null && UpdateVisibility && !(CoroutineInvoker == null)) {
                ResetVisionCoroutine = CoroutineInvoker.StartCoroutine(ResetVisibility());
            }
        }

        #endregion

    }

}
