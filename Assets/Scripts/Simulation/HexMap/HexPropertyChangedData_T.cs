using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public class HexPropertyChangedData<T> {

        public IHexCell Cell;

        public T OldValue;
        public T NewValue;

        public HexPropertyChangedData(IHexCell cell, T oldValue, T newValue) {
            Cell     = cell;
            OldValue = oldValue;
            NewValue = newValue;
        }

    }

}
