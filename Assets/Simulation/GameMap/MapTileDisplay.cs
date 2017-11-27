using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using BetterUI;

using Assets.Simulation.Core;

namespace Assets.Simulation.GameMap.UI {

    public class MapTileDisplay : UIPanel {

        #region instance fields and properties

        [SerializeField] private Button CreateCityButton;

        [SerializeField] private CityBuilder CityBuilder;

        private ITileEventBroadcaster EventBroadcaster;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITileEventBroadcaster eventBroadcaster) {
            EventBroadcaster = eventBroadcaster;
        }

        #region from UIPanel

        protected override void DoOnEnable() {
            var clickedTile = EventBroadcaster.LastClickedTile;

            CreateCityButton.onClick.AddListener(() => CityBuilder.BuildFullCityOnTile(clickedTile));
        }

        protected override void DoOnDisable() {
            CreateCityButton.onClick.RemoveAllListeners();
        }

        #endregion

        #endregion

    }

}
