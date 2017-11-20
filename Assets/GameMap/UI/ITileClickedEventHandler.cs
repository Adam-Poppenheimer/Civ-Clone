using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

namespace Assets.GameMap.UI {

    public interface ITileClickedEventHandler : IEventSystemHandler {

        #region methods

        void OnTileClicked(IMapTile tile, PointerEventData eventData);

        #endregion

    }

}
