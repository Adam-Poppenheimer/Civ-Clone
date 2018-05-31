using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public class RiverData {

        #region instance fields and properties

        public IHexCell Center { get; private set; }
        public IHexCell Left   { get; private set; }
        public IHexCell Right  { get; private set; }

        public HexDirection Direction { get; private set; }

        public EdgeVertices CenterToLeftEdge {
            get {
                if(_centerToleftEdge == null) {
                    _centerToleftEdge = GetYAdjustedEdge(Center, Direction.Previous());
                }
                return _centerToleftEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToleftEdge;

        public EdgeVertices CenterToRightEdge {
            get {
                if(_centerToRightEdge == null) {
                    _centerToRightEdge = GetYAdjustedEdge(Center, Direction);
                }
                return _centerToRightEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToRightEdge;

        public EdgeVertices LeftToCenterEdge {
            get {
                if(_leftToCenterEdge == null) {
                    _leftToCenterEdge = GetYAdjustedEdge(Left, Direction.Previous().Opposite());
                }
                return _leftToCenterEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToCenterEdge;

        public EdgeVertices RightToCenterEdge {
            get {
                if(_rightToCenterEdge == null) {
                    _rightToCenterEdge = GetYAdjustedEdge(Right, Direction.Opposite());
                }
                return _rightToCenterEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _rightToCenterEdge;

        public EdgeVertices LeftToRightEdge {
            get {
                if(_leftToRightEdge == null) {
                    _leftToRightEdge = GetYAdjustedEdge(Left, Direction.Previous().Opposite().Previous());
                }
                return _leftToRightEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToRightEdge;

        public EdgeVertices RightToLeftEdge {
            get {
                if(_rightToLeftEdge == null) {
                    _rightToLeftEdge = GetYAdjustedEdge(Right, Direction.Opposite().Next());
                }
                return _rightToLeftEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _rightToLeftEdge;

        public Vector3 CenterCorner {
            get { return CenterToLeftEdge.V5; }
        }

        public Vector3 LeftCorner {
            get { return LeftToCenterEdge.V1; }
        }

        public Vector3 RightCorner {
            get { return RightToCenterEdge.V5; }
        }

        public EdgeVertices CenterLeftTrough {
            get {
                if(_centerLeftTrough == null) {
                    _centerLeftTrough = GetRiverTrough(CenterToLeftEdge, LeftToCenterEdge, true);
                }
                return _centerLeftTrough.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerLeftTrough;

        public EdgeVertices CenterRightTrough {
            get {
                if(_centerRightTrough == null) {
                    _centerRightTrough = GetRiverTrough(CenterToRightEdge, RightToCenterEdge, true);
                }
                return _centerRightTrough.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerRightTrough;

        public EdgeVertices LeftRightTrough {
            get {
                if(_leftRightTrough == null) {
                    _leftRightTrough = GetRiverTrough(LeftToRightEdge, RightToLeftEdge, true);
                }
                return _leftRightTrough.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftRightTrough;

        public Vector3 CenterLeftTroughPoint {
            get { return CenterLeftTrough.V5; }
        }

        public Vector3 CenterRightTroughPoint {
            get { return CenterRightTrough.V1; }
        }

        public Vector3 LeftRightTroughPoint {
            get { return LeftRightTrough.V5; }
        }

        public Vector3 ConfluencePoint {
            get { return (CenterLeftTroughPoint + CenterRightTroughPoint + LeftRightTroughPoint) / 3f; }
        }

        public Vector3 PerturbedCenterLeftTroughPoint {
            get { return NoiseGenerator.Perturb(CenterLeftTroughPoint, false); }
        }

        public Vector3 PerturbedLeftCorner {
            get { return NoiseGenerator.Perturb(LeftCorner, Left.RequiresYPerturb); }
        }

        public Vector3 PerturbedCenterCorner {
            get { return NoiseGenerator.Perturb(CenterCorner, Center.RequiresYPerturb); }
        }

        public Vector3 PerturbedRightCorner {
            get { return NoiseGenerator.Perturb(RightCorner, Right.RequiresYPerturb); }
        }

        public Vector3 PerturbedCenterRightTroughPoint {
            get { return NoiseGenerator.Perturb(CenterRightTroughPoint, false); }
        }

        public Vector3 PertrubedLeftRightTroughPoint {
            get { return NoiseGenerator.Perturb(LeftRightTroughPoint, false); }
        }

        private INoiseGenerator NoiseGenerator;

        #endregion

        #region constructors

        public RiverData(
            IHexCell center, IHexCell left,
            IHexCell right, HexDirection direction,
            INoiseGenerator noiseGenerator
        ){
            Center = center;
            Left   = left;
            Right  = right;

            Direction = direction;

            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region methods

        private EdgeVertices GetYAdjustedEdge(IHexCell cell, HexDirection direction) {
            var center = cell.transform.localPosition;

            var retval = new EdgeVertices(
                center + HexMetrics.GetFirstOuterSolidCorner (direction),
                center + HexMetrics.GetSecondOuterSolidCorner(direction)
            );

            retval.V1.y = cell.EdgeY;
            retval.V2.y = cell.EdgeY;
            retval.V3.y = cell.EdgeY;
            retval.V4.y = cell.EdgeY;
            retval.V5.y = cell.EdgeY;

            return retval;
        }

        private EdgeVertices GetRiverTrough(EdgeVertices nearEdge, EdgeVertices farEdge, bool invertFarEdge) {
            var troughEdge = new EdgeVertices(
                (nearEdge.V1 + (invertFarEdge ? farEdge.V5 : farEdge.V1)) / 2f,
                (nearEdge.V2 + (invertFarEdge ? farEdge.V4 : farEdge.V2)) / 2f,
                (nearEdge.V3 + farEdge.V3)                                / 2f,
                (nearEdge.V4 + (invertFarEdge ? farEdge.V2 : farEdge.V4)) / 2f,
                (nearEdge.V5 + (invertFarEdge ? farEdge.V1 : farEdge.V5)) / 2f
            );

            troughEdge.V1.y = Mathf.Min(nearEdge.V1.y, (invertFarEdge ? farEdge.V5 : farEdge.V1).y) + HexMetrics.StreamBedElevationOffset;
            troughEdge.V2.y = Mathf.Min(nearEdge.V2.y, (invertFarEdge ? farEdge.V4 : farEdge.V2).y) + HexMetrics.StreamBedElevationOffset;
            troughEdge.V3.y = Mathf.Min(nearEdge.V3.y, farEdge.V3.y)                                + HexMetrics.StreamBedElevationOffset;
            troughEdge.V4.y = Mathf.Min(nearEdge.V4.y, (invertFarEdge ? farEdge.V2 : farEdge.V4).y) + HexMetrics.StreamBedElevationOffset;
            troughEdge.V5.y = Mathf.Min(nearEdge.V5.y, (invertFarEdge ? farEdge.V1 : farEdge.V5).y) + HexMetrics.StreamBedElevationOffset;

            return troughEdge;
        }

        #endregion

    }

}
