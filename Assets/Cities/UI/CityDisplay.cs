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

        private ICityEventBroadcaster CityEventBroadcaster;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityTileDisplay tileDisplay, ICityEventBroadcaster cityEventBroadcaster) {
            TileDisplay = tileDisplay;
            CityEventBroadcaster = cityEventBroadcaster;
        }

        #region from UIPanel

        protected override void DoOnEnable() {
            TileDisplay.CityToDisplay = CityEventBroadcaster.LastClickedCity;
            TileDisplay.Refresh();
        }

        protected override void DoOnDisable() {
            TileDisplay.CityToDisplay = null;
        }

        #endregion

        #endregion

    }

}
