using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexCellSignals {

        #region instance fields and properties

        public CellClickedSignal      ClickedSignal      { get; private set; }
        public CellPointerEnterSignal PointerEnterSignal { get; private set; }
        public CellPointerExitSignal  PointerExitSignal  { get; private set; }

        #endregion

        #region constructors

        public HexCellSignals(CellClickedSignal clickedSignal, CellPointerEnterSignal pointerEnterSignal,
            CellPointerExitSignal pointerExitSignal) {

            ClickedSignal = clickedSignal;
            PointerEnterSignal = pointerEnterSignal;
            PointerExitSignal = pointerExitSignal;
        }

        #endregion

    }

    public class CellClickedSignal : Signal<CellClickedSignal, IHexCell, Vector3> { }

    public class CellPointerEnterSignal : Signal<CellPointerEnterSignal, IHexCell> { }

    public class CellPointerExitSignal : Signal<CellPointerExitSignal, IHexCell> { }

}
