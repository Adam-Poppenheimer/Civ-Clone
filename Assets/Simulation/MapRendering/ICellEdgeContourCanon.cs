using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using UnityEngine;

using Assets.Simulation.HexMap;


namespace Assets.Simulation.MapRendering {

    public interface ICellEdgeContourCanon {

        #region methods

        void SetContourForCellEdge(IHexCell cell, HexDirection edge, List<Vector2> contour);

        ReadOnlyCollection<Vector2> GetContourForCellEdge(IHexCell cell, HexDirection edge);

        void Clear();

        bool IsPointWithinContour(Vector2 xzPoint, IHexCell cell, HexDirection direction);

        bool IsPointBetweenContours(
            Vector2 xzPoint, ReadOnlyCollection<Vector2> contourOne, ReadOnlyCollection<Vector2> contourTwo
        );

        Vector2 GetClosestPointOnContour(Vector2 xzPoint, ReadOnlyCollection<Vector2> contour);

        Vector2 GetClosestPointOnContours(Vector2 xzPoint, params ReadOnlyCollection<Vector2>[] contours);

        #endregion

    }

}