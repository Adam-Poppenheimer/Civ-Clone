using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using UniRx;

namespace Assets.Simulation.HexMap {

    public class HexCellSignals {

        #region instance fields and properties

        public CellClickedSignal                           ClickedSignal      { get; private set; }
        public CellPointerEnterSignal                      PointerEnterSignal { get; private set; }
        public CellPointerExitSignal                       PointerExitSignal  { get; private set; }
        public ISubject<Tuple<IHexCell, PointerEventData>> PointerDownSignal  { get; private set; }
        public ISubject<Tuple<IHexCell, PointerEventData>> DraggedSignal      { get; private set; }

        public ISubject<IHexCell> FoundationElevationChangedSignal  { get; private set; }
        public ISubject<IHexCell> ShapeChangedSignal                { get; private set; }
        public ISubject<IHexCell> FeatureChangedSignal              { get; private set; }
        public ISubject<IHexCell> WaterLevelChangedSignal           { get; private set; }

        #endregion

        #region constructors

        public HexCellSignals(CellClickedSignal clickedSignal, CellPointerEnterSignal pointerEnterSignal,
            CellPointerExitSignal pointerExitSignal) {

            ClickedSignal      = clickedSignal;
            PointerEnterSignal = pointerEnterSignal;
            PointerExitSignal  = pointerExitSignal;
            PointerDownSignal  = new Subject<Tuple<IHexCell, PointerEventData>>();
            DraggedSignal      = new Subject<Tuple<IHexCell, PointerEventData>>();

            FoundationElevationChangedSignal  = new Subject<IHexCell>();
            ShapeChangedSignal                = new Subject<IHexCell>();
            FeatureChangedSignal              = new Subject<IHexCell>();
            WaterLevelChangedSignal           = new Subject<IHexCell>();
        }

        #endregion

    }

    public class CellClickedSignal : Signal<CellClickedSignal, IHexCell, Vector3> { }

    public class CellPointerEnterSignal : Signal<CellPointerEnterSignal, IHexCell> { }

    public class CellPointerExitSignal : Signal<CellPointerExitSignal, IHexCell> { }

}
