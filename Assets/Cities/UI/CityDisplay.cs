using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using BetterUI;

namespace Assets.Cities.UI {

    public class CityDisplay : UIPanel {

        #region instance fields and properties

        private ICityTileDisplay TileDisplay;

        private IProductionDisplay ProductionDisplay;

        private ICityEventBroadcaster CityEventBroadcaster;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityTileDisplay tileDisplay, ICityEventBroadcaster cityEventBroadcaster,
            IProductionDisplay productionDisplay) {
            TileDisplay = tileDisplay;
            CityEventBroadcaster = cityEventBroadcaster;
            ProductionDisplay = productionDisplay;
        }

        #region from UIPanel

        protected override void DoOnEnable() {
            TileDisplay.CityToDisplay = CityEventBroadcaster.LastClickedCity;
            TileDisplay.Refresh();

            ProductionDisplay.CityToDisplay = CityEventBroadcaster.LastClickedCity;
            ProductionDisplay.Refresh();
        }

        protected override void DoOnDisable() {
            TileDisplay.CityToDisplay = null;
            ProductionDisplay.CityToDisplay = null;
        }

        #endregion

        #endregion

    }

}
