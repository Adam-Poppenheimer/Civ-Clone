using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.GameMap {

    public interface ITileClickedEventHandler : IEventSystemHandler {

        #region methods

        void OnTileClicked(IMapTile tile, PointerEventData eventData);

        #endregion

    }

}
