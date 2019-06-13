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
using Assets.Simulation.Core;
using Assets.Simulation.Players;
using Assets.Simulation.MapRendering;

namespace Assets.Simulation.Visibility {

    public class VisibilityResponder : IVisibilityResponder {

        #region instance fields and properties

        #region from IVisibilityResponder

        public bool UpdateVisibility {
            get { return _updateVisibility; }
            set { _updateVisibility = value; }
        }
        private bool _updateVisibility;

        #endregion

        private Coroutine ResetVisionCoroutine;
        private Coroutine ResetResourceVisibilityCoroutine;


        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IVisibilityCanon                              VisibilityCanon;
        private IExplorationCanon                             ExplorationCanon;
        private ICityLineOfSightLogic                         CityLineOfSightLogic;
        private IUnitVisibilityLogic                          UnitLineOfSightLogic;
        private IUnitFactory                                  UnitFactory;
        private ICityFactory                                  CityFactory;
        private IHexGrid                                      Grid;
        private MonoBehaviour                                 CoroutineInvoker;
        private IHexCellShaderData                            HexCellShaderData;

        #endregion

        #region constructors

        [Inject]
        public VisibilityResponder(
            IPossessionRelationship<ICivilization, IUnit>    unitPossessionCanon,
            IPossessionRelationship<ICivilization, ICity>    cityPossessionCanon,
            IVisibilityCanon visibilityCanon, IExplorationCanon explorationCanon,
            ICityLineOfSightLogic cityLineOfSightLogic,
            IUnitVisibilityLogic unitLineOfSightLogic,           
            IUnitFactory unitFactory, 
            ICityFactory cityFactory,
            IHexGrid grid,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker,
            IHexCellShaderData hexCellShaderData,
            UnitSignals unitSignals,
            CitySignals citySignals,
            HexCellSignals cellSignals,
            CivilizationSignals civSignals,
            VisibilitySignals visibilitySignals,
            CoreSignals coreSignals,
            MapRenderingSignals mapRenderingSignals
        ){
            UnitPossessionCanon  = unitPossessionCanon;
            CityPossessionCanon  = cityPossessionCanon;
            VisibilityCanon      = visibilityCanon;
            ExplorationCanon     = explorationCanon;
            CityLineOfSightLogic = cityLineOfSightLogic;
            UnitLineOfSightLogic = unitLineOfSightLogic;
            UnitFactory          = unitFactory;
            CityFactory          = cityFactory;
            Grid                 = grid;
            CoroutineInvoker     = coroutineInvoker;
            HexCellShaderData    = hexCellShaderData;

            unitSignals.LeftLocation   .Subscribe(OnUnitLeftLocation);
            unitSignals.EnteredLocation.Subscribe(OnUnitEnteredLocation);

            citySignals.LostCellFromBoundaries.Subscribe(OnCityLostCell);
            citySignals.GainedCellToBoundaries.Subscribe(OnCityGainedCell);

            cellSignals.TerrainChanged   .Subscribe(data => TryResetCellVisibility());
            cellSignals.ShapeChanged     .Subscribe(data => TryResetCellVisibility());
            cellSignals.VegetationChanged.Subscribe(data => TryResetCellVisibility());

            civSignals.CivLosingCity      .Subscribe(OnCivLosingCity);
            civSignals.CivGainedCity      .Subscribe(OnCivGainedCity);
            civSignals.CivDiscoveredTech  .Subscribe(data => TryResetResourceVisibility());
            civSignals.CivUndiscoveredTech.Subscribe(data => TryResetResourceVisibility());

            visibilitySignals.CellVisibilityModeChanged    .Subscribe(unit => TryResetCellVisibility    ());
            visibilitySignals.CellExplorationModeChanged   .Subscribe(unit => TryResetCellVisibility    ());
            visibilitySignals.ResourceVisibilityModeChanged.Subscribe(unit => TryResetResourceVisibility());

            coreSignals.ActivePlayerChanged.Subscribe(OnActivePlayerChanged);

            mapRenderingSignals.MapFinishedLoading.Subscribe(Unit => TryResetCellVisibility    ());
            mapRenderingSignals.MapFinishedLoading.Subscribe(Unit => TryResetResourceVisibility());
        }

        #endregion

        #region instance methods

        #region from IVisibilityResponder

        public void TryResetCellVisibility() {
            if(ResetVisionCoroutine == null && UpdateVisibility && !(CoroutineInvoker == null)) {
                ResetVisionCoroutine = CoroutineInvoker.StartCoroutine(ResetCellVisibility());
            }
        }

        public void TryResetResourceVisibility() {
            if(ResetResourceVisibilityCoroutine == null && UpdateVisibility && !(CoroutineInvoker == null)) {
                ResetResourceVisibilityCoroutine = CoroutineInvoker.StartCoroutine(ResetResourceVisibility());
            }
        }

        #endregion

        private IEnumerator ResetCellVisibility() {
            yield return new WaitForEndOfFrame();

            while(Grid.Chunks.Any(chunk => chunk.IsRefreshing)) {
                yield return new WaitForEndOfFrame();
            }

            VisibilityCanon.ClearCellVisibility();

            foreach(var unit in UnitFactory.AllUnits) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                foreach(var visibleCell in UnitLineOfSightLogic.GetCellsVisibleToUnit(unit)) {
                    VisibilityCanon.IncreaseCellVisibilityToCiv(visibleCell, unitOwner);
                    ExplorationCanon.SetCellAsExploredByCiv(visibleCell, unitOwner);
                }
            }

            foreach(var city in CityFactory.AllCities) {
                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

                foreach(var visibleCell in CityLineOfSightLogic.GetCellsVisibleToCity(city)) {
                    VisibilityCanon.IncreaseCellVisibilityToCiv(visibleCell, cityOwner);
                    ExplorationCanon.SetCellAsExploredByCiv(visibleCell, cityOwner);
                }
            }

            foreach(var cell in Grid.Cells) {
                HexCellShaderData.RefreshVisibility(cell);
            }

            ResetVisionCoroutine = null;

            yield break;
        }

        private IEnumerator ResetResourceVisibility() {
            yield return new WaitForEndOfFrame();

            foreach(var chunk in Grid.Chunks) {
                chunk.Refresh(TerrainRefreshType.Features);
            }

            ResetResourceVisibilityCoroutine = null;
        }

        private void OnUnitLeftLocation(Tuple<IUnit, IHexCell> args) {
            TryResetCellVisibility();
        }

        private void OnUnitEnteredLocation(Tuple<IUnit, IHexCell> args) {
            TryResetCellVisibility();
        }

        private void OnCityLostCell(Tuple<ICity, IHexCell> args) {
            TryResetCellVisibility();
        }

        private void OnCityGainedCell(Tuple<ICity, IHexCell> args) {
            TryResetCellVisibility();
        }

        private void OnCivLosingCity(Tuple<ICivilization, ICity> data) {
            TryResetCellVisibility();
        }

        private void OnCivGainedCity(Tuple<ICivilization, ICity> data) {
            TryResetCellVisibility();
        }

        private void OnActivePlayerChanged(IPlayer newActivePlayer) {
            TryResetCellVisibility();
            TryResetResourceVisibility();
        }

        #endregion

    }

}
