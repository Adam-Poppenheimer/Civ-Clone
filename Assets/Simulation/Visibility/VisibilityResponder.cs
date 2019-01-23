﻿using System;
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
using Assets.Simulation.MapResources;
using Assets.Simulation.Core;
using Assets.Simulation.Players;

namespace Assets.Simulation.Visibility {

    public class VisibilityResponder : IVisibilityResponder {

        #region instance fields and properties

        #region from IVisibilityResponder

        public bool UpdateVisibility { get; set; }

        #endregion

        private Coroutine ResetVisionCoroutine;
        private Coroutine ResetResourceVisibilityCoroutine;


        private IPossessionRelationship<ICivilization, IUnit>    UnitPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity>    CityPossessionCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private IVisibilityCanon                                 VisibilityCanon;
        private IExplorationCanon                                ExplorationCanon;
        private ICityLineOfSightLogic                            CityLineOfSightLogic;
        private IUnitVisibilityLogic                            UnitLineOfSightLogic;
        private IUnitFactory                                     UnitFactory;
        private ICityFactory                                     CityFactory;
        private IHexGrid                                         Grid;
        private MonoBehaviour                                    CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public VisibilityResponder(
            IPossessionRelationship<ICivilization, IUnit>    unitPossessionCanon,
            IPossessionRelationship<ICivilization, ICity>    cityPossessionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            IVisibilityCanon visibilityCanon, IExplorationCanon explorationCanon,
            ICityLineOfSightLogic cityLineOfSightLogic,
            IUnitVisibilityLogic unitLineOfSightLogic,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker,
            IUnitFactory unitFactory, 
            ICityFactory cityFactory,
            IHexGrid grid,
            UnitSignals unitSignals,
            CitySignals citySignals,
            HexCellSignals cellSignals,
            CivilizationSignals civSignals,
            VisibilitySignals visibilitySignals,
            CoreSignals coreSignals
        ){
            UnitPossessionCanon  = unitPossessionCanon;
            CityPossessionCanon  = cityPossessionCanon;
            NodeLocationCanon    = nodeLocationCanon;
            VisibilityCanon      = visibilityCanon;
            ExplorationCanon     = explorationCanon;
            CityLineOfSightLogic = cityLineOfSightLogic;
            UnitLineOfSightLogic = unitLineOfSightLogic;
            UnitFactory          = unitFactory;
            CityFactory          = cityFactory;
            Grid                 = grid;
            CoroutineInvoker     = coroutineInvoker;

            unitSignals.LeftLocation   .Subscribe(OnUnitLeftLocation);
            unitSignals.EnteredLocation.Subscribe(OnUnitEnteredLocation);

            citySignals.LostCellFromBoundariesSignal.Subscribe(OnCityLostCell);
            citySignals.GainedCellToBoundariesSignal.Subscribe(OnCityGainedCell);

            cellSignals.FoundationElevationChangedSignal.Subscribe(OnHexCellVisibilityPropertiesChanged);
            cellSignals.TerrainChangedSignal            .Subscribe(OnHexCellVisibilityPropertiesChanged);
            cellSignals.ShapeChangedSignal              .Subscribe(OnHexCellVisibilityPropertiesChanged);
            cellSignals.VegetationChangedSignal         .Subscribe(OnHexCellVisibilityPropertiesChanged);

            civSignals.CivLosingCity.Subscribe(OnCivLosingCity);
            civSignals.CivGainedCity.Subscribe(OnCivGainedCity);

            visibilitySignals.CellVisibilityModeChangedSignal .Subscribe(unit => TryResetCellVisibility());
            visibilitySignals.CellExplorationModeChangedSignal.Subscribe(unit => TryResetCellVisibility());

            coreSignals.ActivePlayerChangedSignal.Subscribe(OnActivePlayerChanged);
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
                cell.RefreshVisibility();
            }

            ResetVisionCoroutine = null;
        }

        private IEnumerator ResetResourceVisibility() {
            yield return new WaitForEndOfFrame();

            foreach(var cellWithNode in Grid.Cells.Where(cell => NodeLocationCanon.GetPossessionsOfOwner(cell).Any())) {
                cellWithNode.RefreshSelfOnly();
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

        private void OnHexCellVisibilityPropertiesChanged(IHexCell cell) {
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
