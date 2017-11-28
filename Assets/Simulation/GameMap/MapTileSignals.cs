using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.GameMap {

    public class MapTileSignals {

        #region instance fields and properties

        public TileClickedSignal      ClickedSignal      { get; private set; }
        public TilePointerEnterSignal PointerEnterSignal { get; private set; }
        public TilePointerExitSignal  PointerExitSignal  { get; private set; }

        #endregion

        #region constructors

        public MapTileSignals(TileClickedSignal clickedSignal, TilePointerEnterSignal pointerEnterSignal,
            TilePointerExitSignal pointerExitSignal) {

            ClickedSignal = clickedSignal;
            PointerEnterSignal = pointerEnterSignal;
            PointerExitSignal = pointerExitSignal;
        }

        #endregion

    }

    public class TileClickedSignal : Signal<TileClickedSignal, IMapTile, PointerEventData> { }

    public class TilePointerEnterSignal : Signal<TilePointerEnterSignal, IMapTile, PointerEventData> { }

    public class TilePointerExitSignal : Signal<TilePointerExitSignal, IMapTile, PointerEventData> { }

}
