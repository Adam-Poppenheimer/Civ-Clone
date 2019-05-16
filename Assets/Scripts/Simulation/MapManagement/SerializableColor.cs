using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapManagement {

    [Serializable]
    public struct SerializableColor {

        #region instance fields and properties

        public float r;
        public float g;
        public float b;
        public float a;

        #endregion

        #region constructors

        public SerializableColor(float r, float g, float b, float a) {
            this.r = r;
            this.b = b;
            this.g = g;
            this.a = a;
        }

        #endregion

        #region operators

        public static implicit operator Color(SerializableColor color) {
            return new Color(color.r, color.g, color.b, color.a);
        }

        public static implicit operator SerializableColor(Color color) {
            return new SerializableColor(color.r, color.g, color.b, color.a);
        }

        #endregion

    }

}
