using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

namespace Assets.Simulation.GameMap {

    public interface ITileEventBroadcaster {

        #region properties

        IMapTile LastClickedTile { get; }

        #endregion

        #region methods

        void BroadcastTileClicked(IMapTile tile, PointerEventData eventData);

        void SubscribeTileClickedHandler(ITileClickedEventHandler handler);
        void UnsubscribeTileClickedHandler(ITileClickedEventHandler handler);

        #endregion

    }

}
