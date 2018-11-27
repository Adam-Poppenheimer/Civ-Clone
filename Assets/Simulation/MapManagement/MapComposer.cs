using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Visibility;

namespace Assets.Simulation.MapManagement {

    public class MapComposer : IMapComposer {

        #region instance fields and properties

        private IHexCellComposer         HexCellComposer;
        private ICivilizationComposer    CivilizationComposer;
        private ICityComposer            CityComposer;
        private IBuildingComposer        BuildingComposer;
        private IUnitComposer            UnitComposer;
        private IImprovementComposer     ImprovementComposer;
        private IResourceComposer        ResourceComposer;
        private IDiplomacyComposer       DiplomacyComposer;
        private ICapitalCityComposer     CapitalCityComposer;
        private IVisibilityResponder     VisibilityResponder;
        private IVisibilityCanon         VisibilityCanon;
        private ICapitalCitySynchronizer CapitalCitySynchronizer;
        private IFreeBuildingsCanon      FreeBuildingsCanon;
        private MonoBehaviour            CoroutineInvoker;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexCellComposer         hexCellComposer,
            ICivilizationComposer    civilizationComposer,
            ICityComposer            cityComposer,
            IBuildingComposer        buildingComposer,
            IUnitComposer            unitComposer,
            IImprovementComposer     improvementComposer,
            IResourceComposer        resourceComposer,
            IDiplomacyComposer       diplomacyComposer,
            ICapitalCityComposer     capitalCityComposer,
            IVisibilityResponder     visibilityResponder,
            IVisibilityCanon         visibilityCanon,
            ICapitalCitySynchronizer capitalCitySynchronizer,
            IFreeBuildingsCanon      freeBuildingsCanon,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker
        ) {
            HexCellComposer         = hexCellComposer;
            CivilizationComposer    = civilizationComposer;
            CityComposer            = cityComposer;
            BuildingComposer        = buildingComposer;
            UnitComposer            = unitComposer;
            ImprovementComposer     = improvementComposer;
            ResourceComposer        = resourceComposer;
            DiplomacyComposer       = diplomacyComposer;
            CapitalCityComposer     = capitalCityComposer;
            VisibilityResponder     = visibilityResponder;
            VisibilityCanon         = visibilityCanon;
            CapitalCitySynchronizer = capitalCitySynchronizer;
            FreeBuildingsCanon      = freeBuildingsCanon;
            CoroutineInvoker        = coroutineInvoker;
        }

        public SerializableMapData ComposeRuntimeIntoData() {
            var mapData = new SerializableMapData();

            HexCellComposer     .ComposeCells        (mapData);
            CivilizationComposer.ComposeCivilizations(mapData);
            CityComposer        .ComposeCities       (mapData);
            BuildingComposer    .ComposeBuildings    (mapData);
            UnitComposer        .ComposeUnits        (mapData);
            ImprovementComposer .ComposeImprovements (mapData);
            ResourceComposer    .ComposeResources    (mapData);
            DiplomacyComposer   .ComposeDiplomacy    (mapData);
            CapitalCityComposer .ComposeCapitalCities(mapData);

            return mapData;
        }

        public void DecomposeDataIntoRuntime(SerializableMapData mapData, Action performAfterDecomposition = null) {
            CoroutineInvoker.StartCoroutine(DecomposeDataIntoRuntimeCoroutine(mapData, performAfterDecomposition));
        }

        private IEnumerator DecomposeDataIntoRuntimeCoroutine(SerializableMapData mapData, Action performAfterDecomposition) {
            yield return ClearRuntimeCoroutine();

            HexCellComposer.DecomposeCells(mapData);

            yield return new WaitForEndOfFrame();

            CapitalCitySynchronizer.SetCapitalUpdating(false);
            FreeBuildingsCanon.ApplyBuildingsToCities = false;

            CivilizationComposer.DecomposeCivilizations(mapData);
            CityComposer        .DecomposeCities       (mapData);
            BuildingComposer    .DecomposeBuildings    (mapData);
            UnitComposer        .DecomposeUnits        (mapData);
            ResourceComposer    .DecomposeResources    (mapData);
            ImprovementComposer .DecomposeImprovements (mapData);

            CapitalCitySynchronizer.SetCapitalUpdating(true);

            yield return new WaitForEndOfFrame();

            DiplomacyComposer  .DecomposeDiplomacy    (mapData);
            CapitalCityComposer.DecomposeCapitalCities(mapData);
            

            if(performAfterDecomposition != null) {
                performAfterDecomposition();
            }
        }

        public void ClearRuntime() {
            CoroutineInvoker.StartCoroutine(ClearRuntimeCoroutine());
        }

        private IEnumerator ClearRuntimeCoroutine() {
            var oldVisibility = VisibilityResponder.UpdateVisibility;

            VisibilityResponder.UpdateVisibility = false;

            CapitalCitySynchronizer.SetCapitalUpdating(false);

            ImprovementComposer .ClearRuntime();
            CityComposer        .ClearRuntime();
            BuildingComposer    .ClearRuntime();
            UnitComposer        .ClearRuntime();
            ResourceComposer    .ClearRuntime();
            DiplomacyComposer   .ClearRuntime();
            CivilizationComposer.ClearRuntime();

            yield return new WaitForEndOfFrame();

            CapitalCitySynchronizer.SetCapitalUpdating(true);

            HexCellComposer.ClearRuntime();

            VisibilityCanon.ClearCellVisibility();

            VisibilityResponder.UpdateVisibility = oldVisibility;
        }

        #endregion

    }

}
