using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    public class MapComposer : IMapComposer {

        #region instance fields and properties

        private HexCellComposer      HexCellComposer;
        private CivilizationComposer CivilizationComposer;
        private CityComposer         CityComposer;
        private UnitComposer         UnitComposer;
        private ImprovementComposer  ImprovementComposer;
        private ResourceComposer     ResourceComposer;
        private DiplomacyComposer    DiplomacyComposer;
        private VisibilityResponder  VisibilityResponder;
        private ICellVisibilityCanon CellVisibilityCanon;
        private MonoBehaviour        CoroutineInvoker;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            HexCellComposer      hexCellComposer,
            CivilizationComposer civilizationComposer,
            CityComposer         cityComposer,
            UnitComposer         unitComposer,
            ImprovementComposer  improvementComposer,
            ResourceComposer     resourceComposer,
            DiplomacyComposer    diplomacyComposer,
            VisibilityResponder  visibilityResponder,
            ICellVisibilityCanon cellVisibilityCanon,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker
        ) {
            HexCellComposer      = hexCellComposer;
            CivilizationComposer = civilizationComposer;
            CityComposer         = cityComposer;
            UnitComposer         = unitComposer;
            ImprovementComposer  = improvementComposer;
            ResourceComposer     = resourceComposer;
            DiplomacyComposer    = diplomacyComposer;
            VisibilityResponder  = visibilityResponder;
            CellVisibilityCanon  = cellVisibilityCanon;
            CoroutineInvoker     = coroutineInvoker;
        }

        public SerializableMapData ComposeRuntimeIntoData() {
            var mapData = new SerializableMapData();

            HexCellComposer     .ComposeCells        (mapData);
            CivilizationComposer.ComposeCivilizations(mapData);
            CityComposer        .ComposeCities       (mapData);
            UnitComposer        .ComposeUnits        (mapData);
            ImprovementComposer .ComposeImprovements (mapData);
            ResourceComposer    .ComposeResources    (mapData);
            DiplomacyComposer   .ComposeDiplomacy    (mapData);

            return mapData;
        }

        public void DecomposeDataIntoRuntime(SerializableMapData mapData, Action performAfterDecomposition = null) {
            CoroutineInvoker.StartCoroutine(DecomposeDataIntoRuntimeCoroutine(mapData, performAfterDecomposition));
        }

        private IEnumerator DecomposeDataIntoRuntimeCoroutine(SerializableMapData mapData, Action performAfterDecomposition) {
            yield return ClearRuntimeCoroutine();

            HexCellComposer     .DecomposeCells        (mapData);
            CivilizationComposer.DecomposeCivilizations(mapData);
            CityComposer        .DecomposeCities       (mapData);
            UnitComposer        .DecomposeUnits        (mapData);
            ResourceComposer    .DecomposeResources    (mapData);
            ImprovementComposer .DecomposeImprovements (mapData);

            yield return new WaitForEndOfFrame();

            DiplomacyComposer.DecomposeDiplomacy(mapData);

            if(performAfterDecomposition != null) {
                performAfterDecomposition();
            }
        }

        public void ClearRuntime() {
            CoroutineInvoker.StartCoroutine(ClearRuntimeCoroutine());
        }

        private IEnumerator ClearRuntimeCoroutine() {
            VisibilityResponder.UpdateVisibility = false;

            ImprovementComposer .ClearRuntime();
            CityComposer        .ClearRuntime();
            UnitComposer        .ClearRuntime();
            ResourceComposer    .ClearRuntime();
            CivilizationComposer.ClearRuntime();

            yield return new WaitForEndOfFrame();

            HexCellComposer.ClearRuntime();

            CellVisibilityCanon.ClearVisibility();

            VisibilityResponder.UpdateVisibility = true;
        }

        #endregion

    }

}
