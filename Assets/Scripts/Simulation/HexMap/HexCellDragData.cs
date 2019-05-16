using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Simulation.HexMap {

    public class HexCellDragData {

        #region instance fields and properties

        public IHexCell CellBeingDragged;

        public PointerEventData EventData;

        #endregion

    }

}
