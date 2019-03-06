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

        bool AreLineSegmentsCrossing(
            Vector2 pointA1, Vector2 pointA2, out Vector2 closestPointA,
            Vector2 pointB1, Vector2 pointB2, out Vector2 closestPointB
        );

        bool ClosestPointsOnTwoLines(
            Vector2 lineOnePoint, Vector2 lineOneVector,
            Vector2 lineTwoPoint, Vector2 lineTwoVector,
            out Vector2 closestPointLineOne, out Vector2 closestPointLineTwo
        );

        #endregion

    }

}
