using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.UI.Cities;
using Assets.UI.Civilizations;
using Assets.UI.HexMap;
using Assets.UI.Units;
using Assets.UI.Technology;

using Assets.UI.StateMachine.States;

namespace Assets.UI.StateMachine {

    public class PlayModeInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private List<CityDisplayBase>         AllCityDisplays;
        [SerializeField] private List<CivilizationDisplayBase> AllCivilizationDisplays;
        [SerializeField] private List<TileDisplayBase>         AllTileDisplays;
        [SerializeField] private List<UnitDisplayBase>         AllUnitDisplays;

        [SerializeField] private List<RectTransform> PlayModeDefaultPanels;

        [SerializeField] private RectTransform CombatSummaryPanel;

        [SerializeField] private TechTreeDisplay TechTreeDisplay;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<List<CityDisplayBase>>        ().FromInstance(AllCityDisplays);
            Container.Bind<List<CivilizationDisplayBase>>().FromInstance(AllCivilizationDisplays);
            Container.Bind<List<TileDisplayBase>>        ().FromInstance(AllTileDisplays);
            Container.Bind<List<UnitDisplayBase>>        ().FromInstance(AllUnitDisplays);

            Container.Bind<List<RectTransform>>().WithId("Play Mode Default Panels").FromInstance(PlayModeDefaultPanels);

            Container.Bind<RectTransform>().WithId("Combat Summary Panel").FromInstance(CombatSummaryPanel);

            Container.Bind<TechTreeDisplay>().FromInstance(TechTreeDisplay);
        }

        #endregion

        #endregion

    }

}
