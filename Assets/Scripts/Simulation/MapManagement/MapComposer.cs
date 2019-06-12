using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Visibility;
using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Core;

namespace Assets.Simulation.MapManagement {

    public class MapComposer : IMapComposer {

        #region instance fields and properties

        #region from IMapComposer

        public bool IsProcessing {
            get { return DecomposeCoroutine != null; }
        }

        #endregion

        private Coroutine DecomposeCoroutine;
        private Coroutine ClearCoroutine;



        private IHexCellComposer                HexCellComposer;
        private ICivilizationComposer           CivilizationComposer;
        private IPlayerComposer                 PlayerComposer;
        private ICityComposer                   CityComposer;
        private IBuildingComposer               BuildingComposer;
        private IUnitComposer                   UnitComposer;
        private IImprovementComposer            ImprovementComposer;
        private IResourceComposer               ResourceComposer;
        private IDiplomacyComposer              DiplomacyComposer;
        private ICapitalCityComposer            CapitalCityComposer;
        private IBarbarianComposer              BarbarianComposer;
        private IVisibilityResponder            VisibilityResponder;
        private IVisibilityCanon                VisibilityCanon;
        private ICapitalCitySynchronizer        CapitalCitySynchronizer;
        private List<IPlayModeSensitiveElement> PlayModeSensitiveElements;
        private MonoBehaviour                   CoroutineInvoker;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexCellComposer         hexCellComposer,
            ICivilizationComposer    civilizationComposer,
            IPlayerComposer          playerComposer,
            ICityComposer            cityComposer,
            IBuildingComposer        buildingComposer,
            IUnitComposer            unitComposer,
            IImprovementComposer     improvementComposer,
            IResourceComposer        resourceComposer,
            IDiplomacyComposer       diplomacyComposer,
            ICapitalCityComposer     capitalCityComposer,
            IBarbarianComposer       barbarianComposer,
            IVisibilityResponder     visibilityResponder,
            IVisibilityCanon         visibilityCanon,
            ICapitalCitySynchronizer capitalCitySynchronizer,
            List<IPlayModeSensitiveElement> playModeSensitiveElements,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker
        ) {
            HexCellComposer           = hexCellComposer;
            CivilizationComposer      = civilizationComposer;
            PlayerComposer            = playerComposer;
            CityComposer              = cityComposer;
            BuildingComposer          = buildingComposer;
            UnitComposer              = unitComposer;
            ImprovementComposer       = improvementComposer;
            ResourceComposer          = resourceComposer;
            DiplomacyComposer         = diplomacyComposer;
            CapitalCityComposer       = capitalCityComposer;
            BarbarianComposer         = barbarianComposer;
            VisibilityResponder       = visibilityResponder;
            VisibilityCanon           = visibilityCanon;
            CapitalCitySynchronizer   = capitalCitySynchronizer;
            PlayModeSensitiveElements = playModeSensitiveElements;
            CoroutineInvoker          = coroutineInvoker;
        }

        public SerializableMapData ComposeRuntimeIntoData() {
            var mapData = new SerializableMapData();

            HexCellComposer     .ComposeCells        (mapData);
            CivilizationComposer.ComposeCivilizations(mapData);
            PlayerComposer      .ComposePlayers      (mapData);
            CityComposer        .ComposeCities       (mapData);
            BuildingComposer    .ComposeBuildings    (mapData);
            UnitComposer        .ComposeUnits        (mapData);
            ImprovementComposer .ComposeImprovements (mapData);
            ResourceComposer    .ComposeResources    (mapData);
            DiplomacyComposer   .ComposeDiplomacy    (mapData);
            CapitalCityComposer .ComposeCapitalCities(mapData);
            BarbarianComposer   .ComposeBarbarians   (mapData);

            return mapData;
        }

        public void DecomposeDataIntoRuntime(SerializableMapData mapData, Action performAfterDecomposition = null) {
            if(DecomposeCoroutine == null) {
                DecomposeCoroutine = CoroutineInvoker.StartCoroutine(DecomposeDataIntoRuntime_Coroutine(mapData, performAfterDecomposition));
            }
        }

        private IEnumerator DecomposeDataIntoRuntime_Coroutine(SerializableMapData mapData, Action performAfterDecomposition) {
            yield return ClearRuntime_Coroutine(false);

            HexCellComposer.DecomposeCells(mapData);

            yield return new WaitForEndOfFrame();

            CapitalCitySynchronizer.SetCapitalUpdating(false);

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = false;
            }

            CivilizationComposer.DecomposeCivilizations(mapData);
            PlayerComposer      .DecomposePlayers      (mapData);
            CityComposer        .DecomposeCities       (mapData);
            BuildingComposer    .DecomposeBuildings    (mapData);
            UnitComposer        .DecomposeUnits        (mapData);
            ResourceComposer    .DecomposeResources    (mapData);
            ImprovementComposer .DecomposeImprovements (mapData);
            BarbarianComposer   .DecomposeBarbarians   (mapData);

            CapitalCitySynchronizer.SetCapitalUpdating(true);

            yield return new WaitForEndOfFrame();

            DiplomacyComposer  .DecomposeDiplomacy    (mapData);
            CapitalCityComposer.DecomposeCapitalCities(mapData);
            

            if(performAfterDecomposition != null) {
                performAfterDecomposition();
            }

            DecomposeCoroutine = null;
        }

        public void ClearRuntime(bool immediateMode) {
            if(ClearCoroutine == null) {
                ClearCoroutine = CoroutineInvoker.StartCoroutine(ClearRuntime_Coroutine(immediateMode));
            }
        }

        private IEnumerator ClearRuntime_Coroutine(bool immediateMode) {
            var oldVisibility = VisibilityResponder.UpdateVisibility;

            VisibilityResponder.UpdateVisibility = false;

            CapitalCitySynchronizer.SetCapitalUpdating(false);

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = false;
            }

            ImprovementComposer .ClearRuntime(immediateMode);
            CityComposer        .ClearRuntime(immediateMode);
            BuildingComposer    .ClearRuntime();
            UnitComposer        .ClearRuntime();
            ResourceComposer    .ClearRuntime();
            DiplomacyComposer   .ClearRuntime();
            CivilizationComposer.ClearRuntime();
            PlayerComposer      .ClearRuntime();
            BarbarianComposer   .ClearRuntime();

            if(!immediateMode) { yield return new WaitForEndOfFrame(); }

            CapitalCitySynchronizer.SetCapitalUpdating(true);

            HexCellComposer.ClearRuntime();

            VisibilityCanon.ClearCellVisibility();

            VisibilityResponder.UpdateVisibility = oldVisibility;

            ClearCoroutine = null;
        }

        #endregion

    }

}
