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

        public ISubject<Tuple<IHexCell, PointerEventData>> ClickedSignal      { get; private set; }
        public ISubject<Tuple<IHexCell, PointerEventData>> PointerDownSignal  { get; private set; }
        public ISubject<IHexCell>                          PointerEnterSignal { get; private set; }
        public ISubject<IHexCell>                          PointerExitSignal  { get; private set; }

        public ISubject<HexCellDragData> BeginDragSignal { get; private set; }
        public ISubject<HexCellDragData> DragSignal      { get; private set; }
        public ISubject<HexCellDragData> EndDragSignal   { get; private set; }

        public ISubject<IHexCell> FoundationElevationChangedSignal { get; private set; }
        public ISubject<IHexCell> ShapeChangedSignal               { get; private set; }
        public ISubject<IHexCell> VegetationChangedSignal          { get; private set; }
        public ISubject<IHexCell> WaterLevelChangedSignal          { get; private set; }

        public ISubject<Unit> MapBeingClearedSignal { get; set; }

        #endregion

        #region constructors

        public HexCellSignals() {

            ClickedSignal      = new Subject<Tuple<IHexCell, PointerEventData>>();
            PointerDownSignal  = new Subject<Tuple<IHexCell, PointerEventData>>();
            PointerEnterSignal = new Subject<IHexCell>();
            PointerExitSignal  = new Subject<IHexCell>();

            BeginDragSignal = new Subject<HexCellDragData>();
            DragSignal      = new Subject<HexCellDragData>();
            EndDragSignal   = new Subject<HexCellDragData>();

            FoundationElevationChangedSignal = new Subject<IHexCell>();
            ShapeChangedSignal               = new Subject<IHexCell>();
            VegetationChangedSignal          = new Subject<IHexCell>();
            WaterLevelChangedSignal          = new Subject<IHexCell>();

            MapBeingClearedSignal = new Subject<Unit>();
        }

        #endregion

    }

}
