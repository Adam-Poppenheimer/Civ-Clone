using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public static class Vector2Extensions {

        #region static methods

        public static Vector2 Project(this Vector2 source, Vector2 onto) {
            Vector2 ontoNormalized = onto.normalized;

            return Vector2.Dot(source, ontoNormalized) * ontoNormalized;
        }

        public static Vector3 ToXYZ(this Vector2 self) {
            return new Vector3(self.x, 0f, self.y);
        }

        #endregion

    }

}
