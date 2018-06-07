using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public class CellTriangulationData {

        #region instance fields and properties

        public IHexCell Center    { get; private set; }
        public IHexCell Left      { get; private set; }
        public IHexCell Right     { get; private set; }
        public IHexCell NextRight { get; private set; }

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
                    _leftToCenterEdge = GetYAdjustedEdge(Left, Direction.Previous().Opposite(), true);
                }
                return _leftToCenterEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToCenterEdge;

        public EdgeVertices RightToCenterEdge {
            get {
                if(_rightToCenterEdge == null) {
                    _rightToCenterEdge = GetYAdjustedEdge(Right, Direction.Opposite(), true);
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
            get { return LeftToCenterEdge.V5; }
        }

        public Vector3 RightCorner {
            get { return RightToCenterEdge.V1; }
        }

        public EdgeVertices CenterLeftTrough {
            get {
                if(_centerLeftTrough == null) {
                    _centerLeftTrough = GetRiverTrough(CenterToLeftEdge, LeftToCenterEdge, false);
                }
                return _centerLeftTrough.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerLeftTrough;

        public EdgeVertices CenterRightTrough {
            get {
                if(_centerRightTrough == null) {
                    _centerRightTrough = GetRiverTrough(CenterToRightEdge, RightToCenterEdge, false);
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

        public HexEdgeType CenterToLeftEdgeType {
            get {
                if(_centerToLeftEdgeType == null) {
                    _centerToLeftEdgeType = GetEdgeType(Center, Left, Direction.Previous());
                }
                return _centerToLeftEdgeType.GetValueOrDefault();
            }
        }
        private HexEdgeType? _centerToLeftEdgeType;

        public HexEdgeType CenterToRightEdgeType {
            get {
                if(_centerToRightEdgeType == null) {
                    _centerToRightEdgeType = GetEdgeType(Center, Right, Direction);
                }
                return _centerToRightEdgeType.GetValueOrDefault();
            }
        }
        private HexEdgeType? _centerToRightEdgeType;

        public HexEdgeType LeftToRightEdgeType {
            get {
                if(_leftToRightEdgeType == null) {
                    _leftToRightEdgeType = GetEdgeType(Left, Right, Direction.Next());
                }
                return _leftToRightEdgeType.GetValueOrDefault();
            }
        }
        private HexEdgeType? _leftToRightEdgeType;

        public HexEdgeType CenterToNextRightEdgeType {
            get {
                if(_centerToNextRightEdgeType == null) {
                    _centerToNextRightEdgeType = GetEdgeType(Center, NextRight, Direction.Next());
                }
                return _centerToNextRightEdgeType.GetValueOrDefault();
            }
        }
        private HexEdgeType? _centerToNextRightEdgeType;

        public bool AllEdgesHaveRivers {
            get {
                if(_allEdgesHaveRivers == null) {
                    _allEdgesHaveRivers = CenterToLeftEdgeType  == HexEdgeType.River
                                       && CenterToRightEdgeType == HexEdgeType.River
                                       && LeftToRightEdgeType   == HexEdgeType.River;
                }
                return _allEdgesHaveRivers.GetValueOrDefault();
            }
        }
        private bool? _allEdgesHaveRivers;

        public bool TwoEdgesHaveRivers {
            get {
                if(_twoEdgesHaveRivers == null) {
                    int riverCount = 0;
                    riverCount += CenterToLeftEdgeType  == HexEdgeType.River ? 1 : 0;
                    riverCount += CenterToRightEdgeType == HexEdgeType.River ? 1 : 0;
                    riverCount += LeftToRightEdgeType   == HexEdgeType.River ? 1 : 0;

                    _twoEdgesHaveRivers = riverCount == 2;
                }
                return _twoEdgesHaveRivers.GetValueOrDefault();
            }
        }
        private bool? _twoEdgesHaveRivers;

        public bool IsRiverCorner {
            get {
                if(_isRiverCorner == null) {
                    _isRiverCorner = CenterToLeftEdgeType  == HexEdgeType.River ||
                                     CenterToRightEdgeType == HexEdgeType.River ||
                                     LeftToRightEdgeType   == HexEdgeType.River;
                }
                return _isRiverCorner.GetValueOrDefault();
            }
        }
        private bool? _isRiverCorner;

        public Vector3 CenterPeak {
            get {
                if(_centerPeak == null) {
                    _centerPeak = new Vector3(
                        Center.LocalPosition.x,
                        Center.PeakY,
                        Center.LocalPosition.z
                    );
                }

                return _centerPeak.GetValueOrDefault();
            }
        }
        private Vector3? _centerPeak;

        public EdgeVertices CenterToRightInnerEdge {
            get {
                if(_centerToRightInnerEdge == null) {
                    _centerToRightInnerEdge = new EdgeVertices(
                        CenterPeak + HexMetrics.GetFirstInnerSolidCorner (Direction),
                        CenterPeak + HexMetrics.GetSecondInnerSolidCorner(Direction)
                    );
                }

                return _centerToRightInnerEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToRightInnerEdge;

        public IEnumerable<Vector3> CenterFeatureLocations {
            get {
                if(_featureLocations == null) {
                    _featureLocations = new List<Vector3>() {
                        (Center.LocalPosition + CenterToRightEdge.V1 + CenterToRightEdge.V5) * (1f / 3f)
                    };
                }

                return _featureLocations;
            }
        }
        private List<Vector3> _featureLocations;

        public Vector3 CenterWaterMidpoint {
            get {
                if(_centerWaterMidpoint == null) {
                    _centerWaterMidpoint = new Vector3(
                        Center.LocalPosition.x,
                        Center.WaterSurfaceY,
                        Center.LocalPosition.z
                    );
                }

                return _centerWaterMidpoint.GetValueOrDefault();
            }
        }
        private Vector3? _centerWaterMidpoint;

        public EdgeVertices CenterToRightWaterEdge {
            get {
                if(_centerToRightWaterEdge == null) {
                    var newEdge = new EdgeVertices(
                        Center.LocalPosition + HexMetrics.GetFirstWaterCorner (Direction),
                        Center.LocalPosition + HexMetrics.GetSecondWaterCorner(Direction)
                    );

                    newEdge.V1.y = newEdge.V2.y = newEdge.V3.y = newEdge.V4.y = newEdge.V5.y = Center.WaterSurfaceY;

                    _centerToRightWaterEdge = newEdge;
                }

                return _centerToRightWaterEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToRightWaterEdge;

        public EdgeVertices RightToCenterWaterEdge {
            get {
                if(_rightToCenterWaterEdge == null) {
                    var newEdge = new EdgeVertices(
                        Right.LocalPosition + HexMetrics.GetSecondWaterCorner(Direction.Opposite()),
                        Right.LocalPosition + HexMetrics.GetFirstWaterCorner (Direction.Opposite())
                    );

                    newEdge.V1.y = newEdge.V2.y = newEdge.V3.y = newEdge.V4.y = newEdge.V5.y = Right.WaterSurfaceY;

                    _rightToCenterWaterEdge = newEdge;
                }

                return _rightToCenterWaterEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _rightToCenterWaterEdge;

        public EdgeVertices LeftToCenterWaterEdge {
            get {
                if(_leftToCenterWaterEdge == null) {
                    var newEdge = new EdgeVertices(
                        Left.LocalPosition + HexMetrics.GetFirstWaterCorner (Direction.Next2()),
                        Left.LocalPosition + HexMetrics.GetSecondWaterCorner(Direction.Next2())
                    );

                    newEdge.V1.y = newEdge.V2.y = newEdge.V3.y = newEdge.V4.y = newEdge.V5.y = Left.WaterSurfaceY;

                    _leftToCenterWaterEdge = newEdge;
                }

                return _leftToCenterWaterEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToCenterWaterEdge;

        public bool HasRoadToLeft {
            get { return Left != null && Left.HasRoads && CenterToLeftEdgeType != HexEdgeType.Cliff; }
        }

        public bool HasRoadToRight {
            get { return Right != null && Right.HasRoads && CenterToRightEdgeType != HexEdgeType.Cliff; }
        }

        public bool HasRoadToNextRight {
            get { return NextRight != null && NextRight.HasRoads && CenterToNextRightEdgeType != HexEdgeType.Cliff; }
        }



        private INoiseGenerator NoiseGenerator;
        private IRiverCanon     RiverCanon;

        #endregion

        #region constructors

        public CellTriangulationData(
            IHexCell center, IHexCell left,
            IHexCell right, IHexCell nextRight,
            HexDirection direction,
            INoiseGenerator noiseGenerator, IRiverCanon riverCanon
        ){
            Center    = center;
            Left      = left;
            Right     = right;
            NextRight = nextRight;

            Direction = direction;

            NoiseGenerator = noiseGenerator;
            RiverCanon     = riverCanon;
        }

        #endregion

        #region methods

        private EdgeVertices GetYAdjustedEdge(IHexCell cell, HexDirection direction, bool invertEdge = false) {
            var center = cell.LocalPosition;

            EdgeVertices retval;

            if(invertEdge) {
                retval = new EdgeVertices(
                    center + HexMetrics.GetSecondOuterSolidCorner(direction),
                    center + HexMetrics.GetFirstOuterSolidCorner (direction)
                );
            }else {
                retval = new EdgeVertices(
                    center + HexMetrics.GetFirstOuterSolidCorner (direction),
                    center + HexMetrics.GetSecondOuterSolidCorner(direction)
                );
            }

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

        private HexEdgeType GetEdgeType(IHexCell cellOne, IHexCell cellTwo, HexDirection direction) {
            if(cellOne == null || cellTwo == null) {
                return HexEdgeType.Void;
            }else if(RiverCanon.HasRiverAlongEdge(cellOne, direction)) {
                return HexEdgeType.River;
            }else {
                return HexMetrics.GetEdgeType(cellOne, cellTwo);
            }
        }

        #endregion

    }

}
