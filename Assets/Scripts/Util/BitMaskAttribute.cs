using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public class BitMaskAttribute : PropertyAttribute {

        public Type PropertyType;

        public BitMaskAttribute(Type propertyType) {
            PropertyType = propertyType;
        }

    }

}
