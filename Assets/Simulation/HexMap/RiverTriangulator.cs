using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class RiverTriangulator : IRiverTriangulator {

        #region instance fields and properties

        private IHexGrid                  Grid;
        private IHexGridMeshBuilder       MeshBuilder;
        private IRiverTroughTriangulator  TroughTriangulator;
        private IRiverSurfaceTriangulator SurfaceTriangulator;

        #endregion

        #region constructors

        [Inject]
        public RiverTriangulator(
            IHexGrid grid, IHexGridMeshBuilder meshBuilder,
            IRiverTroughTriangulator troughTriangulator,
            IRiverSurfaceTriangulator surfaceTriangulator
        ){
            Grid                = grid;
            MeshBuilder         = meshBuilder;
            TroughTriangulator  = troughTriangulator;
            SurfaceTriangulator = surfaceTriangulator;
        }

        #endregion

        #region instance methods

        #region from IRiverTriangulator

        public bool ShouldTriangulateRiver(CellTriangulationData thisData) {
            return thisData.Direction <= HexDirection.SE && thisData.IsRiverCorner;
        }

        public void TriangulateRiver(CellTriangulationData data) {          
            IHexCell nextNeighbor = Grid.GetNeighbor(data.Center, data.Direction.Next());

            var nextData = MeshBuilder.GetTriangulationData(
                data.Center, data.Right, nextNeighbor, data.Direction.Next()
            );            

            if(data.Direction > HexDirection.SE) {
                return;
            }

            //Creates river edge troughs and surfaces in the direction if needed
            if(data.CenterToRightEdgeType == HexEdgeType.River) {
                TroughTriangulator.CreateRiverTrough_Edge(data);
                SurfaceTriangulator.CreateRiverSurface_EdgesAndCorners(data, nextData);
            }

            if(data.Direction > HexDirection.E || data.Left == null || !data.IsRiverCorner) {
                return;
            }

            CreateRiverTrough_Corner(data);
        }

        private void CreateRiverTrough_Corner(CellTriangulationData data) {
            if(data.AllEdgesHaveRivers) {
                //There is a confluence at the corner
                TroughTriangulator.CreateRiverTrough_Confluence(data);
                CreateRiverSurface_Confluence(data);

            }else if(data.TwoEdgesHaveRivers) {
                if(data.CenterToLeftEdgeType != HexEdgeType.River) {
                    //There is a river corner and Right is on its inside edge. Since
                    //CreateRiverTrough_Curve draws relative to the inside of the curve,
                    //We rotate our river data so that Right is in the center, Center
                    //is on the left, and Left is on the right
                    CreateRiverTrough_Curve(MeshBuilder.GetTriangulationData(
                        data.Right, data.Center, data.Left, data.Direction.Opposite().Next()
                    ));
                }else if(data.CenterToRightEdgeType != HexEdgeType.River) {
                    //There is a river corner and Left is on its inside edge. We must
                    //rotate so that Left is in the center, Right is on the left, and
                    //Center is on the right
                    CreateRiverTrough_Curve(MeshBuilder.GetTriangulationData(
                        data.Left, data.Right, data.Center, data.Direction.Next2()
                    ));
                }else {
                    //There is a river corner and Center is on its inside edge
                    CreateRiverTrough_Curve(data);
                }

            //We need to rotate endpoint cases to make sure that the endpoint is always
            //pointing towards Left
            }else if(data.CenterToLeftEdgeType == HexEdgeType.River) {
                //There is an endpoint pointing towards Right
                CreateRiverTrough_Endpoint(MeshBuilder.GetTriangulationData(
                    data.Left, data.Right, data.Center, data.Direction.Next2()
                ));

            }else if(data.CenterToRightEdgeType == HexEdgeType.River) {
                //There is an endpoint pointing towards Left
                CreateRiverTrough_Endpoint(data);

            }else if(data.LeftToRightEdgeType == HexEdgeType.River) {
                //There is an endpoint pointing towards Center
                CreateRiverTrough_Endpoint(MeshBuilder.GetTriangulationData(
                    data.Right, data.Center, data.Left, data.Direction.Opposite().Next()
                ));
            }
        }

        #endregion

        //This method currently places a static water triangle at the confluence as a first approximation.
        //Managing the UV for proper river flow is quite complex and is being deferred to a later date.
        private void CreateRiverSurface_Confluence(CellTriangulationData data) {
            SurfaceTriangulator.CreateRiverSurface_Confluence(data);

            float confluenceY = Mathf.Min(
                data.Center.RiverSurfaceY, data.Left.RiverSurfaceY, data.Right.RiverSurfaceY
            );

            //We need to handle waterfalls into confluences as a special case. Edge and corner
            //surface generation should account for this by not building its river edge to the
            //confluence's water triangle
            if( data.Center.EdgeElevation > data.Right.EdgeElevation &&
                data.Left  .EdgeElevation > data.Right.EdgeElevation
            ) {
                //There's a waterfall flowing from CenterLeft river into the confluence
                SurfaceTriangulator.CreateRiverSurface_ConfluenceWaterfall(data, confluenceY);
            }else if(
                data.Center.EdgeElevation > data.Left.EdgeElevation &&
                data.Right .EdgeElevation > data.Left.EdgeElevation
            ) {
                //There's a waterfall flowing from CenterRight river into the confluence.
                //We need to rotate our data to make sure that Right is the new center and
                //Left is the new right
                SurfaceTriangulator.CreateRiverSurface_ConfluenceWaterfall(
                    MeshBuilder.GetTriangulationData(
                        data.Right, data.Center, data.Left, data.Direction.Previous2()
                    ),
                    confluenceY
                );
            }else if(
                data.Left .EdgeElevation > data.Center.EdgeElevation &&
                data.Right.EdgeElevation > data.Center.EdgeElevation
            ) {
                //There's a waterwall flowing from LeftRight river into the confluence.
                //we need to rotate our data to make sure that Center is the new Right
                //and Left is the new center
                SurfaceTriangulator.CreateRiverSurface_ConfluenceWaterfall(
                    MeshBuilder.GetTriangulationData(
                        data.Left, data.Right, data.Center, data.Direction.Next2()
                    ),
                    confluenceY
                );
            }
        }

        //Non-rivered edge is between right and left. Center is on the inside of the curve
        private void CreateRiverTrough_Curve(CellTriangulationData data){
            TroughTriangulator.CreateRiverTrough_Curve_InnerEdge(data);

            //We've already taken care of River and Void edge types, so we don't need to check them again here
            if(data.LeftToRightEdgeType == HexEdgeType.Flat) {
                TroughTriangulator.CreateRiverTrough_Curve_OuterFlat(data);
            }else if(data.LeftToRightEdgeType == HexEdgeType.Slope) {
                if(data.Left.EdgeElevation < data.Right.EdgeElevation) {
                    TroughTriangulator.CreateRiverTrough_Curve_TerracesClockwiseUp(data);
                }else {
                    TroughTriangulator.CreateRiverTrough_Curve_TerracesClockwiseDown(data);
                }                
            }
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left
        private void CreateRiverTrough_Endpoint(CellTriangulationData data) {
            if(data.Left == null) {
                return;
            }

            if(data.CenterToLeftEdgeType == HexEdgeType.Flat) {
                if(data.LeftToRightEdgeType == HexEdgeType.Flat) {
                    TroughTriangulator.CreateRiverTrough_Endpoint_DoubleFlat(data);
                    SurfaceTriangulator.CreateRiverEndpointSurface_Default(data);

                }else if(data.LeftToRightEdgeType == HexEdgeType.Slope) {

                    if(data.Left.EdgeElevation > data.Right.EdgeElevation) {
                        TroughTriangulator.CreateRiverTrough_Endpoint_FlatTerraces_ElevatedLeft(data);
                    }else {
                        TroughTriangulator.CreateRiverTrough_Endpoint_FlatTerraces_ElevatedRight(data);
                    }
                    
                    SurfaceTriangulator.CreateRiverEndpointSurface_Default(data);

                }else if(data.LeftToRightEdgeType == HexEdgeType.Cliff) {
                    //This case should never come up, since cliffs only appear when Deep Water
                    //is in play, and rivers cannot appear next to those cells
                }
            }else if(data.CenterToLeftEdgeType == HexEdgeType.Slope) {
                if(data.LeftToRightEdgeType == HexEdgeType.Flat) {

                    if(data.Left.EdgeElevation > data.Center.EdgeElevation) {
                        TroughTriangulator.CreateRiverTrough_Endpoint_TerracesFlat_ElevatedLeft(data);
                    }else {
                        TroughTriangulator.CreateRiverTrough_Endpoint_TerracesFlat_ElevatedCenter(data);
                    }

                    SurfaceTriangulator.CreateRiverEndpointSurface_Default(data);

                }else if(data.LeftToRightEdgeType == HexEdgeType.Slope) {

                    //If Left is underwater, we know that it is also below Center and Right
                    //and that it requires the creation of an estuary. Otherwise, we can
                    //use the generic DoubleTerraces case, which handles both upward-
                    //and downward-facing cases
                    if(data.Left.IsUnderwater) {
                        TroughTriangulator.CreateRiverTrough_Endpoint_ShallowWaterRiverDelta(data);
                    }else {
                        TroughTriangulator.CreateRiverTrough_Endpoint_DoubleTerraces(data);
                    }

                }else if(data.LeftToRightEdgeType == HexEdgeType.Cliff) {
                    TroughTriangulator.CreateRiverTrough_Endpoint_TerracesCliff(data);
                }
            }else if(data.CenterToLeftEdgeType == HexEdgeType.Cliff) {
                if(data.LeftToRightEdgeType == HexEdgeType.Flat) {
                    //This case should never come up, since cliffs only appear when DeepWater
                    //is in play
                }else if(data.LeftToRightEdgeType == HexEdgeType.Slope) {
                    TroughTriangulator.CreateRiverTrough_Endpoint_CliffTerraces(data);
                }else if(data.LeftToRightEdgeType == HexEdgeType.Cliff) {

                    TroughTriangulator.CreateRiverTrough_Endpoint_DoubleCliff(data);

                    //If Left is underwater, that means we have a river ending in
                    //in an estuary. This can happen if Left is DeepWater or if
                    //Left is ShallowWater and both Right and Center are not flat.
                    //We should create a waterfall if and only if both Right and
                    //Center are Hills or Mountains, since those are the only cases
                    //where the water would fall from any distance
                    if( data.Left.IsUnderwater && data.Right.Shape != TerrainShape.Flatlands &&
                        data.Center.Shape != TerrainShape.Flatlands
                    ){
                        //We need to rotate to make sure that our water cell is on the Right
                        SurfaceTriangulator.CreateRiverSurface_EstuaryWaterfall(
                            MeshBuilder.GetTriangulationData(
                                data.Right, data.Center, data.Left, data.Direction.Previous2()
                            ),
                            data.Left.WaterSurfaceY
                        );
                    }
                }
            }
        }

        #endregion

    }

}
