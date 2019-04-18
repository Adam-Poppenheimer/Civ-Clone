using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using UniRx;

using Assets.Simulation.Barbarians;

namespace Assets.Simulation.HexMap {

    public class HexCellSignals {

        #region instance fields and properties

        public ISubject<Tuple<IHexCell, PointerEventData>> Clicked      { get; private set; }
        public ISubject<Tuple<IHexCell, PointerEventData>> PointerDown  { get; private set; }
        public ISubject<Tuple<IHexCell, PointerEventData>> PointerUp    { get; private set; }
        public ISubject<IHexCell>                          PointerEnter { get; private set; }
        public ISubject<IHexCell>                          PointerExit  { get; private set; }

        public ISubject<HexCellDragData> BeginDrag { get; private set; }
        public ISubject<HexCellDragData> Drag      { get; private set; }
        public ISubject<HexCellDragData> EndDrag   { get; private set; }

        public ISubject<HexPropertyChangedData<CellTerrain>>    TerrainChanged    { get; private set; }
        public ISubject<HexPropertyChangedData<CellShape>>      ShapeChanged      { get; private set; }
        public ISubject<HexPropertyChangedData<CellVegetation>> VegetationChanged { get; private set; }        
        public ISubject<HexPropertyChangedData<CellFeature>>    FeatureChanged    { get; private set; }
        public ISubject<HexPropertyChangedData<bool>>           RoadStatusChanged { get; private set; }

        public ISubject<IHexCell> GainedRiveredEdge { get; private set; }
        public ISubject<IHexCell> LostRiveredEdge   { get; private set; }

        public ISubject<Tuple<IHexCell, IEncampment>> GainedEncampment { get; private set; }
        public ISubject<Tuple<IHexCell, IEncampment>> LostEncampment   { get; private set; }

        public ISubject<Unit> MapBeingClearedSignal { get; set; }

        #endregion

        #region constructors

        public HexCellSignals() {
            Clicked      = new Subject<Tuple<IHexCell, PointerEventData>>();
            PointerDown  = new Subject<Tuple<IHexCell, PointerEventData>>();
            PointerUp    = new Subject<Tuple<IHexCell, PointerEventData>>();
            PointerEnter = new Subject<IHexCell>();
            PointerExit  = new Subject<IHexCell>();

            BeginDrag = new Subject<HexCellDragData>();
            Drag      = new Subject<HexCellDragData>();
            EndDrag   = new Subject<HexCellDragData>();

            TerrainChanged    = new Subject<HexPropertyChangedData<CellTerrain>>();
            ShapeChanged      = new Subject<HexPropertyChangedData<CellShape>>();
            VegetationChanged = new Subject<HexPropertyChangedData<CellVegetation>>();
            FeatureChanged    = new Subject<HexPropertyChangedData<CellFeature>>();
            RoadStatusChanged = new Subject<HexPropertyChangedData<bool>>();

            GainedRiveredEdge = new Subject<IHexCell>();
            LostRiveredEdge   = new Subject<IHexCell>();

            GainedEncampment = new Subject<Tuple<IHexCell, IEncampment>>();
            LostEncampment   = new Subject<Tuple<IHexCell, IEncampment>>();

            MapBeingClearedSignal = new Subject<Unit>();
        }

        #endregion

    }

}
