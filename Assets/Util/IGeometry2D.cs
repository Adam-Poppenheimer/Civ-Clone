using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public interface IGeometry2D {

        #region methods

        bool IsPointWithinTriangle(Vector2 point, Vector2 v1, Vector2 v2, Vector2 v3);

        void GetBarycentric2D(Vector2 point, Vector2 a, Vector2 b, Vector2 c, out float aCoord, out float bCoord, out float cCoord);

        Vector2 ProjectPointOntoLineSegment(Vector2 segmentA, Vector2 segmentB, Vector2 point);

        #endregion

    }

}
