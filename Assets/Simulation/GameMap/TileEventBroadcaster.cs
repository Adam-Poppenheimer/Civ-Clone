using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.GameMap {

    public class TileEventBroadcaster : ITileEventBroadcaster {

        #region instance fields and properties

        #region from ITileEventBroadcaster

        public IMapTile LastClickedTile { get; private set; }

        #endregion

        private HashSet<ITileClickedEventHandler> TileClickedHandlers;

        #endregion

        #region constructors

        [Inject]
        public TileEventBroadcaster() {
            TileClickedHandlers = new HashSet<ITileClickedEventHandler>();
        }

        #endregion

        #region instance methods

        #region from ITileEventBroadcaster

        public void BroadcastTileClicked(IMapTile tile, PointerEventData eventData) {
            if(tile == null) {
                throw new NotImplementedException("tile");
            }else if(eventData == null) {
                throw new NotImplementedException("eventData");
            }

            LastClickedTile = tile;

            foreach(var handler in TileClickedHandlers) {
                handler.OnTileClicked(tile, eventData);
            }
        }

        public void SubscribeTileClickedHandler(ITileClickedEventHandler handler) {
            if(handler == null) {
                throw new ArgumentNullException("handler");
            }
            TileClickedHandlers.Add(handler);
        }

        public void UnsubscribeTileClickedHandler(ITileClickedEventHandler handler) {
            if(handler == null) {
                throw new ArgumentNullException("handler");
            }
            TileClickedHandlers.Remove(handler);
        }

        #endregion

        #endregion
        
    }

}
