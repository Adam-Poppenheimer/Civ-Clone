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

        public Vector3 StandardIndicies {
            get { return new Vector3(Center.Index, Left.Index, Right.Index); }
        }

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

        public EdgeVertices CenterToNextRightEdge {
            get {
                if(_centerToNextRightEdge == null) {
                    _centerToNextRightEdge = GetYAdjustedEdge(Center, Direction.Next());
                }
                return _centerToNextRightEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToNextRightEdge;

        public EdgeVertices LeftToCenterEdge {
            get {
                if(_leftToCenterEdge == null) {
                    _leftToCenterEdge = GetYAdjustedEdge(Left, Direction.Next2(), true);
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
                    _leftToRightEdge = GetYAdjustedEdge(Left, Direction.Next());
                }
                return _leftToRightEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToRightEdge;

        public EdgeVertices RightToLeftEdge {
            get {
                if(_rightToLeftEdge == null) {
                    _rightToLeftEdge = GetYAdjustedEdge(Right, Direction.Previous2());
                }
                return _rightToLeftEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _rightToLeftEdge;

        public EdgeVertices CenterToRightEdgePerturbed {
            get {
                if(_centerToRightEdgePerturbed == null) {
                    _centerToRightEdgePerturbed = NoiseGenerator.Perturb(CenterToRightEdge, Center.RequiresYPerturb);
                }
                return _centerToRightEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToRightEdgePerturbed;

        public EdgeVertices CenterToLeftEdgePerturbed {
            get {
                if(_centerToLeftEdgePerturbed == null) {
                    _centerToLeftEdgePerturbed = NoiseGenerator.Perturb(CenterToLeftEdge, Center.RequiresYPerturb);
                }
                return _centerToLeftEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToLeftEdgePerturbed;

        public EdgeVertices CenterToNextRightEdgePerturbed {
            get {
                if(_centerToNextRightEdgePerturbed == null) {
                    _centerToNextRightEdgePerturbed = NoiseGenerator.Perturb(CenterToNextRightEdge, Center.RequiresYPerturb);
                }
                return _centerToNextRightEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToNextRightEdgePerturbed;

        public EdgeVertices LeftToCenterEdgePerturbed {
            get {
                if(_leftToCenterEdgePerturbed == null) {
                    _leftToCenterEdgePerturbed = NoiseGenerator.Perturb(LeftToCenterEdge, Left.RequiresYPerturb);
                }
                return _leftToCenterEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToCenterEdgePerturbed;

        public EdgeVertices RightToCenterEdgePerturbed {
            get {
                if(_rightToCenterEdgePerturbed == null) {
                    _rightToCenterEdgePerturbed = NoiseGenerator.Perturb(RightToCenterEdge, Right.RequiresYPerturb);
                }
                return _rightToCenterEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _rightToCenterEdgePerturbed;

        public EdgeVertices LeftToRightEdgePerturbed {
            get {
                if(_leftToRightEdgePerturbed == null) {
                    _leftToRightEdgePerturbed = NoiseGenerator.Perturb(LeftToRightEdge, Left.RequiresYPerturb);
                }
                return _leftToRightEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToRightEdgePerturbed;

        public EdgeVertices RightToLeftEdgePerturbed {
            get {
                if(_rightToLeftEdgePerturbed == null) {
                    _rightToLeftEdgePerturbed = NoiseGenerator.Perturb(RightToLeftEdge, Right.RequiresYPerturb);
                }
                return _rightToLeftEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _rightToLeftEdgePerturbed;

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
                    var trough = GetRiverTrough(CenterToLeftEdgePerturbed, LeftToCenterEdgePerturbed, false);

                    trough.V1 = Vector3.Lerp(trough.V1, trough.V2, RenderConfig.RiverTroughEndpointPullback);
                    trough.V5 = Vector3.Lerp(trough.V5, trough.V4, RenderConfig.RiverTroughEndpointPullback);

                    _centerLeftTrough = trough;
                }
                return _centerLeftTrough.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerLeftTrough;

        public EdgeVertices CenterRightTrough {
            get {
                if(_centerRightTrough == null) {
                    var trough = GetRiverTrough(CenterToRightEdgePerturbed, RightToCenterEdgePerturbed, false);

                    trough.V1 = Vector3.Lerp(trough.V1, trough.V2, RenderConfig.RiverTroughEndpointPullback);
                    trough.V5 = Vector3.Lerp(trough.V5, trough.V4, RenderConfig.RiverTroughEndpointPullback);

                    _centerRightTrough = trough;
                }
                return _centerRightTrough.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerRightTrough;

        public EdgeVertices LeftRightTrough {
            get {
                if(_leftRightTrough == null) {
                    var trough = GetRiverTrough(LeftToRightEdgePerturbed, RightToLeftEdgePerturbed, true);

                    trough.V1 = Vector3.Lerp(trough.V1, trough.V2, RenderConfig.RiverTroughEndpointPullback);
                    trough.V5 = Vector3.Lerp(trough.V5, trough.V4, RenderConfig.RiverTroughEndpointPullback);

                    _leftRightTrough = trough;
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

        public Vector3 PerturbedLeftCorner {
            get { return NoiseGenerator.Perturb(LeftCorner, Left.RequiresYPerturb); }
        }

        public Vector3 PerturbedCenterCorner {
            get { return NoiseGenerator.Perturb(CenterCorner, Center.RequiresYPerturb); }
        }

        public Vector3 PerturbedRightCorner {
            get { return NoiseGenerator.Perturb(RightCorner, Right.RequiresYPerturb); }
        }

        public HexEdgeType CenterToLeftEdgeType {
            get {
                if(_centerToLeftEdgeType == null) {
                    _centerToLeftEdgeType = EdgeTypeLogic.GetEdgeTypeBetweenCells(Center, Left);
                }
                return _centerToLeftEdgeType.GetValueOrDefault();
            }
        }
        private HexEdgeType? _centerToLeftEdgeType;

        public HexEdgeType CenterToRightEdgeType {
            get {
                if(_centerToRightEdgeType == null) {
                    _centerToRightEdgeType = EdgeTypeLogic.GetEdgeTypeBetweenCells(Center, Right);
                }
                return _centerToRightEdgeType.GetValueOrDefault();
            }
        }
        private HexEdgeType? _centerToRightEdgeType;

        public HexEdgeType LeftToRightEdgeType {
            get {
                if(_leftToRightEdgeType == null) {
                    _leftToRightEdgeType = EdgeTypeLogic.GetEdgeTypeBetweenCells(Left, Right);
                }
                return _leftToRightEdgeType.GetValueOrDefault();
            }
        }
        private HexEdgeType? _leftToRightEdgeType;

        public HexEdgeType CenterToNextRightEdgeType {
            get {
                if(_centerToNextRightEdgeType == null) {
                    _centerToNextRightEdgeType = EdgeTypeLogic.GetEdgeTypeBetweenCells(Center, NextRight);
                }
                return _centerToNextRightEdgeType.GetValueOrDefault();
            }
        }
        private HexEdgeType? _centerToNextRightEdgeType;

        public HexEdgeType RightToNextRightEdgeType {
            get {
                if(_rightToNextRightEdgeType == null) {
                    _rightToNextRightEdgeType = EdgeTypeLogic.GetEdgeTypeBetweenCells(Right, NextRight);
                }
                return _rightToNextRightEdgeType.GetValueOrDefault();
            }
        }
        private HexEdgeType? _rightToNextRightEdgeType;

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
                    _centerPeak = new Vector3(Center.GridRelativePosition.x, Center.PeakY, Center.GridRelativePosition.z);
                }

                return _centerPeak.GetValueOrDefault();
            }
        }
        private Vector3? _centerPeak;

        public Vector3 LeftPeak {
            get {
                if(_leftPeak == null) {
                    _leftPeak = new Vector3(Left.GridRelativePosition.x, Left.PeakY, Left.GridRelativePosition.z);
                }

                return _leftPeak.GetValueOrDefault();
            }
        }
        private Vector3? _leftPeak;

        public Vector3 RightPeak {
            get {
                if(_rightPeak == null) {
                    _rightPeak = new Vector3(Right.GridRelativePosition.x, Right.PeakY, Right.GridRelativePosition.z);
                }

                return _rightPeak.GetValueOrDefault();
            }
        }
        private Vector3? _rightPeak;

        public EdgeVertices CenterToRightInnerEdge {
            get {
                if(_centerToRightInnerEdge == null) {
                    _centerToRightInnerEdge = new EdgeVertices(
                        CenterPeak + RenderConfig.GetFirstInnerSolidCorner (Direction),
                        CenterPeak + RenderConfig.GetSecondInnerSolidCorner(Direction)
                    );
                }

                return _centerToRightInnerEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToRightInnerEdge;

        public EdgeVertices LeftToCenterInnerEdge {
            get {
                if(_leftToCenterInnerEdge == null) {
                    _leftToCenterInnerEdge = new EdgeVertices(
                        LeftPeak + RenderConfig.GetFirstInnerSolidCorner (Direction.Next2()),
                        LeftPeak + RenderConfig.GetSecondInnerSolidCorner(Direction.Next2())
                    );
                }

                return _leftToCenterInnerEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToCenterInnerEdge;

        public EdgeVertices LeftToRightInnerEdge {
            get {
                if(_leftToRightInnerEdge == null) {
                    _leftToRightInnerEdge = new EdgeVertices(
                        LeftPeak + RenderConfig.GetFirstInnerSolidCorner (Direction.Next()),
                        LeftPeak + RenderConfig.GetSecondInnerSolidCorner(Direction.Next())
                    );
                }

                return _leftToRightInnerEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToRightInnerEdge;

        public EdgeVertices RightToLeftInnerEdge {
            get {
                if(_rightToLeftInnerEdge == null) {
                    _rightToLeftInnerEdge = new EdgeVertices(
                        RightPeak + RenderConfig.GetFirstInnerSolidCorner (Direction.Previous2()),
                        RightPeak + RenderConfig.GetSecondInnerSolidCorner(Direction.Previous2())
                    );
                }

                return _rightToLeftInnerEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _rightToLeftInnerEdge;

        public Vector3 CenterWaterMidpoint {
            get {
                if(_centerWaterMidpoint == null) {
                    _centerWaterMidpoint = new Vector3(
                        Center.GridRelativePosition.x,
                        Center.WaterSurfaceY,
                        Center.GridRelativePosition.z
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
                        Center.GridRelativePosition + RenderConfig.GetFirstWaterCorner (Direction),
                        Center.GridRelativePosition + RenderConfig.GetSecondWaterCorner(Direction)
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
                        Right.GridRelativePosition + RenderConfig.GetSecondWaterCorner(Direction.Opposite()),
                        Right.GridRelativePosition + RenderConfig.GetFirstWaterCorner (Direction.Opposite())
                    );

                    newEdge.V1.y = newEdge.V2.y = newEdge.V3.y = newEdge.V4.y = newEdge.V5.y = Right.WaterSurfaceY;

                    _rightToCenterWaterEdge = newEdge;
                }

                return _rightToCenterWaterEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _rightToCenterWaterEdge;

        public EdgeVertices CenterToLeftWaterEdge {
            get {
                if(_centerToLeftWaterEdge == null) {
                    var newEdge = new EdgeVertices(
                        Center.GridRelativePosition + RenderConfig.GetFirstWaterCorner (Direction.Previous()),
                        Center.GridRelativePosition + RenderConfig.GetSecondWaterCorner(Direction.Previous())
                    );

                    newEdge.V1.y = newEdge.V2.y = newEdge.V3.y = newEdge.V4.y = newEdge.V5.y = Center.WaterSurfaceY;

                    _centerToLeftWaterEdge = newEdge;
                }

                return _centerToLeftWaterEdge.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToLeftWaterEdge;

        public EdgeVertices LeftToCenterWaterEdge {
            get {
                if(_leftToCenterWaterEdge == null) {
                    var newEdge = new EdgeVertices(
                        Left.GridRelativePosition + RenderConfig.GetFirstWaterCorner (Direction.Next2()),
                        Left.GridRelativePosition + RenderConfig.GetSecondWaterCorner(Direction.Next2())
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

        public EdgeVertices CenterToRightCultureStartEdgePerturbed {
            get {
                if(_centerToRightCultureStartEdgePerturbed == null) {
                    var perturbedInnerEdge = NoiseGenerator.Perturb(CenterToRightInnerEdge, Center.RequiresYPerturb);

                    _centerToRightCultureStartEdgePerturbed = new EdgeVertices(
                        (perturbedInnerEdge.V1 + CenterToRightEdgePerturbed.V1) / 2f,
                        (perturbedInnerEdge.V2 + CenterToRightEdgePerturbed.V2) / 2f,
                        (perturbedInnerEdge.V3 + CenterToRightEdgePerturbed.V3) / 2f,
                        (perturbedInnerEdge.V4 + CenterToRightEdgePerturbed.V4) / 2f,
                        (perturbedInnerEdge.V5 + CenterToRightEdgePerturbed.V5) / 2f
                    );
                }

                return _centerToRightCultureStartEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _centerToRightCultureStartEdgePerturbed;

        public EdgeVertices LeftToRightCultureStartEdgePerturbed {
            get {
                if(_leftToRightCultureStartEdgePerturbed == null) {
                    var perturbedInnerEdge = NoiseGenerator.Perturb(LeftToRightInnerEdge, Left.RequiresYPerturb);

                    _leftToRightCultureStartEdgePerturbed = new EdgeVertices(
                        (perturbedInnerEdge.V1 + LeftToRightEdgePerturbed.V1) / 2f,
                        (perturbedInnerEdge.V2 + LeftToRightEdgePerturbed.V2) / 2f,
                        (perturbedInnerEdge.V3 + LeftToRightEdgePerturbed.V3) / 2f,
                        (perturbedInnerEdge.V4 + LeftToRightEdgePerturbed.V4) / 2f,
                        (perturbedInnerEdge.V5 + LeftToRightEdgePerturbed.V5) / 2f
                    );
                }

                return _leftToRightCultureStartEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToRightCultureStartEdgePerturbed;

        public EdgeVertices LeftToCenterCultureStartEdgePerturbed {
            get {
                if(_leftToCenterCultureStartEdgePerturbed == null) {
                    var perturbedInnerEdge = NoiseGenerator.Perturb(LeftToCenterInnerEdge, Left.RequiresYPerturb);

                    _leftToCenterCultureStartEdgePerturbed = new EdgeVertices(
                        (perturbedInnerEdge.V1 + LeftToCenterEdgePerturbed.V1) / 2f,
                        (perturbedInnerEdge.V2 + LeftToCenterEdgePerturbed.V2) / 2f,
                        (perturbedInnerEdge.V3 + LeftToCenterEdgePerturbed.V3) / 2f,
                        (perturbedInnerEdge.V4 + LeftToCenterEdgePerturbed.V4) / 2f,
                        (perturbedInnerEdge.V5 + LeftToCenterEdgePerturbed.V5) / 2f
                    );
                }

                return _leftToCenterCultureStartEdgePerturbed.GetValueOrDefault();
            }
        }
        private EdgeVertices? _leftToCenterCultureStartEdgePerturbed;


        private INoiseGenerator     NoiseGenerator;
        private IHexMapRenderConfig RenderConfig;
        private IHexEdgeTypeLogic   EdgeTypeLogic;

        #endregion

        #region constructors

        public CellTriangulationData(
            IHexCell center, IHexCell left, IHexCell right, IHexCell nextRight,
            HexDirection direction, INoiseGenerator noiseGenerator,
            IHexMapRenderConfig renderConfig, IHexEdgeTypeLogic edgeTypeLogic
        ){
            Center    = center;
            Left      = left;
            Right     = right;
            NextRight = nextRight;

            Direction = direction;

            NoiseGenerator = noiseGenerator;
            RenderConfig   = renderConfig;
            EdgeTypeLogic  = edgeTypeLogic;
        }

        #endregion

        #region methods

        private EdgeVertices GetYAdjustedEdge(IHexCell cell, HexDirection direction, bool invertEdge = false) {
            var center = cell.GridRelativePosition;

            EdgeVertices retval;

            if(invertEdge) {
                retval = new EdgeVertices(
                    center + RenderConfig.GetSecondOuterSolidCorner(direction),
                    center + RenderConfig.GetFirstOuterSolidCorner (direction)
                );
            }else {
                retval = new EdgeVertices(
                    center + RenderConfig.GetFirstOuterSolidCorner (direction),
                    center + RenderConfig.GetSecondOuterSolidCorner(direction)
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

            float troughY = Mathf.Min(
                nearEdge.V1.y, nearEdge.V2.y, nearEdge.V3.y, nearEdge.V4.y, nearEdge.V5.y,
                farEdge .V1.y, farEdge .V2.y, farEdge .V3.y, farEdge .V4.y, farEdge .V5.y
            );

            troughY += RenderConfig.StreamBedElevationOffset;

            troughEdge.V1.y = troughEdge.V2.y = troughEdge.V3.y = troughEdge.V4.y = troughEdge.V5.y = troughY;

            return troughEdge;
        }

        #endregion

    }

}
