using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.MapManagement {

    public class MapComposer : IMapComposer {

        #region instance fields and properties

        private HexCellComposer      HexCellComposer;
        private CivilizationComposer CivilizationComposer;
        private CityComposer         CityComposer;
        private UnitComposer         UnitComposer;
        private ImprovementComposer  ImprovementComposer;
        private ResourceComposer     ResourceComposer;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            HexCellComposer hexCellComposer, CivilizationComposer civilizationComposer,
            CityComposer cityComposer, UnitComposer unitComposer, ImprovementComposer improvementComposer,
            ResourceComposer resourceComposer
        ) {
            HexCellComposer      = hexCellComposer;
            CivilizationComposer = civilizationComposer;
            CityComposer         = cityComposer;
            UnitComposer         = unitComposer;
            ImprovementComposer  = improvementComposer;
            ResourceComposer     = resourceComposer;
        }

        public SerializableMapData ComposeRuntimeIntoData() {
            var mapData = new SerializableMapData();

            HexCellComposer     .ComposeCells        (mapData);
            CivilizationComposer.ComposeCivilizations(mapData);
            CityComposer        .ComposeCities       (mapData);
            UnitComposer        .ComposeUnits        (mapData);
            ImprovementComposer .ComposeImprovements (mapData);
            ResourceComposer    .ComposeResources    (mapData);

            return mapData;
        }

        public void DecomposeDataIntoRuntime(SerializableMapData mapData) {
            ClearRuntime();

            HexCellComposer     .DecomposeCells        (mapData);
            CivilizationComposer.DecomposeCivilizations(mapData);
            CityComposer        .DecomposeCities       (mapData);
            UnitComposer        .DecomposeUnits        (mapData);
            ResourceComposer    .DecomposeResources    (mapData);
            ImprovementComposer .DecomposeImprovements (mapData);
        }

        public void ClearRuntime() {            
            ImprovementComposer .ClearRuntime();
            UnitComposer        .ClearRuntime();
            CityComposer        .ClearRuntime();
            CivilizationComposer.ClearRuntime();
            ResourceComposer    .ClearRuntime();
            HexCellComposer     .ClearRuntime();
        }

        #endregion

    }

}
