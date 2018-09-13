using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Improvements;

namespace Assets.Simulation.HexMap {

    public class FarmTriangulator : IFarmTriangulator {

        #region internal types

        private class FarmCorner {

            public Vector3 Position;
            public Vector2 UV;

            public FarmCorner(Vector3 position, Vector2 uv) {
                Position = position;
                UV       = uv;
            }

        }

        private class FarmQuad {

            #region instance fields and properties

            public FarmCorner BottomLeft;
            public FarmCorner BottomRight;
            public FarmCorner TopLeft;
            public FarmCorner TopRight;

            public Vector3 Midpoint {
                get { return (BottomLeft.Position + BottomRight.Position + TopLeft.Position + TopRight.Position) / 4f; }
            }

            #endregion

            #region instance methods

            public FarmQuad RotateClockwise() {
                return new FarmQuad() {
                    BottomLeft  = this.BottomRight,
                    BottomRight = this.TopRight,
                    TopLeft     = this.BottomLeft,
                    TopRight    = this.TopLeft
                };
            }

            #endregion

        }

        private class FarmData {

            #region instance fields and properties

            public Vector3 MiddlePoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition); } }

            public Vector3 NorthPoint     { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.NE)); } }
            public Vector3 NorthEastPoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.E));  } }
            public Vector3 SouthEastPoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.SE)); } }
            public Vector3 SouthPoint     { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.SW)); } }
            public Vector3 SouthWestPoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.W));  } }
            public Vector3 NorthWestPoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.NW)); } }

            public Vector3 NorthNorthEastPoint { get { return (NorthPoint + NorthEastPoint) / 2f; } }
            public Vector3 SouthSouthEastPoint { get { return (SouthPoint + SouthEastPoint) / 2f; } }
            public Vector3 SouthSouthWestPoint { get { return (SouthPoint + SouthWestPoint) / 2f; } }
            public Vector3 NorthNorthWestPoint { get { return (NorthPoint + NorthWestPoint) / 2f; } }

            public Vector3 MiddleNorthPoint { get { return (MiddlePoint    + NorthPoint)     / 2f; } }
            public Vector3 MiddleEastPoint  { get { return (NorthEastPoint + SouthEastPoint) / 2f; } }
            public Vector3 MiddleSouthPoint { get { return (MiddlePoint    + SouthPoint)     / 2f; } }
            public Vector3 MiddleWestPoint  { get { return (NorthWestPoint + SouthWestPoint) / 2f; } }
            
            public Vector3 MiddleNorthEastPoint { get { return (MiddlePoint + NorthEastPoint) / 2f; } }
            public Vector3 MiddleSouthEastPoint { get { return (MiddlePoint + SouthEastPoint) / 2f; } }
            public Vector3 MiddleSouthWestPoint { get { return (MiddlePoint + SouthWestPoint) / 2f; } }
            public Vector3 MiddleNorthWestPoint { get { return (MiddlePoint + NorthWestPoint) / 2f; } }


            private IHexCell Cell;

            private INoiseGenerator NoiseGenerator;

            #endregion

            #region constructors

            public FarmData(IHexCell cell, INoiseGenerator noiseGenerator) {
                Cell = cell;

                NoiseGenerator = noiseGenerator;
            }

            #endregion

        }

        #endregion

        #region instance fields and properties

        private IImprovementLocationCanon ImprovementLocationCanon;
        private IImprovementTemplate      FarmTemplate;
        private IHexGridMeshBuilder       MeshBuilder;
        private INoiseGenerator           NoiseGenerator;
        private IHexMapConfig             Config;

        #endregion

        #region constructors

        [Inject]
        public FarmTriangulator(
            IImprovementLocationCanon improvementLocationCanon,
            [Inject(Id = "Farm Template")] IImprovementTemplate farmTemplate,
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator,
            IHexMapConfig config
        ) {
            ImprovementLocationCanon = improvementLocationCanon;
            FarmTemplate             = farmTemplate;
            MeshBuilder              = meshBuilder;
            NoiseGenerator           = noiseGenerator;
            Config                   = config;
        }

        #endregion

        #region instance methods

        #region from IFarmTriangulator

        public bool ShouldTriangulateFarm(IHexCell cell) {
            return HasFarm(cell);
        }

        public void TriangulateFarm(IHexCell cell) {
            var quads = BuildFarmQuads(cell);

            foreach(var quad in quads) {
                float orientation = UnityEngine.Random.value;

                TriangulateFarmQuad(quad, cell);
            }
        }

        #endregion
        private void TriangulateFarmQuad(FarmQuad quad, IHexCell cell) {
            var cellHash = NoiseGenerator.SampleHashGrid(quad.Midpoint);

            var color = Config.FarmlandColors[Mathf.RoundToInt(cellHash.C * (Config.FarmlandColors.Count - 1))];

            MeshBuilder.AddQuadUnperturbed(
                quad.BottomLeft .Position, MeshBuilder.Weights1, quad.BottomLeft .UV,
                quad.BottomRight.Position, MeshBuilder.Weights1, quad.BottomRight.UV,
                quad.TopLeft    .Position, MeshBuilder.Weights1, quad.TopLeft    .UV, 
                quad.TopRight   .Position, MeshBuilder.Weights1, quad.TopRight   .UV, 
                new Vector3(cell.Index, cell.Index, cell.Index),
                MeshBuilder.Farmland
            );

            var rgUV = new Vector2(color.r, color.g);
            var baUV = new Vector2(color.b, color.a);

            MeshBuilder.Farmland.AddQuadUV2(rgUV, rgUV, rgUV, rgUV);
            MeshBuilder.Farmland.AddQuadUV3(baUV, baUV, baUV, baUV);
        }
        
        private List<FarmQuad> BuildFarmQuads(IHexCell cell) {
            var retval = new List<FarmQuad>();

            var farmData = new FarmData(cell, NoiseGenerator);

            if(cell.Shape == CellShape.Flatlands) {
                BuildNorthernQuads_Flatlands(cell, farmData, retval);
                BuildEasternQuads_Flatlands (cell, farmData, retval);
                BuildSouthernQuads_Flatlands(cell, farmData, retval);
                BuildWesternQuads_Flatlands (cell, farmData, retval);

            }else if(cell.Shape == CellShape.Hills) {
                BuildNorthernQuads_Hills(cell, farmData, retval);
            }

            return retval;
        }

        private void BuildNorthernQuads_Flatlands(IHexCell cell, FarmData farmData, List<FarmQuad> quads) {
            var quadrantHash = NoiseGenerator.SampleHashGrid(farmData.MiddleNorthPoint);

            if(quadrantHash.A < Config.FarmDivideIntoSquaresChance) {
                //Four square farms
                quads.Add(BuildSquareFarmQuad(
                    farmData.MiddlePoint,          farmData.MiddleNorthEastPoint,
                    farmData.MiddleNorthWestPoint, farmData.MiddleNorthPoint
                ));

                quads.Add(BuildSquareFarmQuad(
                    farmData.MiddleNorthEastPoint, farmData.NorthEastPoint,
                    farmData.MiddleNorthPoint,     farmData.NorthNorthEastPoint
                ));
                
                quads.Add(BuildSquareFarmQuad(
                    farmData.MiddleNorthWestPoint, farmData.MiddleNorthPoint,
                    farmData.NorthWestPoint,       farmData.NorthNorthWestPoint
                ));

                quads.Add(BuildSquareFarmQuad(
                    farmData.MiddleNorthPoint,    farmData.NorthNorthEastPoint,
                    farmData.NorthNorthWestPoint, farmData.NorthPoint
                ));

            }else {
                Vector2 bottomLeftUV  = new Vector2(0f, 0f);
                Vector2 bottomRightUV = new Vector2(1f, 0f);
                Vector2 topLeftUV     = new Vector2(0f, 1f);
                Vector2 topRightUV    = new Vector2(1f, 1f);

                if(quadrantHash.B <= 0.5f) {
                    //Top of the quad aligned with the northeast edge                    
                     quads.Add(new FarmQuad() {
                        BottomLeft  = new FarmCorner(farmData.NorthWestPoint,       bottomLeftUV),
                        BottomRight = new FarmCorner(farmData.MiddleNorthWestPoint, bottomRightUV),
                        TopLeft     = new FarmCorner(farmData.NorthPoint,           topLeftUV),
                        TopRight    = new FarmCorner(farmData.NorthNorthEastPoint,  topRightUV)
                    });

                    quads.Add(new FarmQuad() {
                        BottomLeft  = new FarmCorner(farmData.MiddleNorthWestPoint, bottomLeftUV),
                        BottomRight = new FarmCorner(farmData.MiddlePoint,          bottomRightUV),
                        TopLeft     = new FarmCorner(farmData.NorthNorthEastPoint,  topLeftUV),
                        TopRight    = new FarmCorner(farmData.NorthEastPoint,       topRightUV)
                    });

                }else {
                    //Top of the quad aligned with the northwest edge
                    quads.Add(new FarmQuad() {
                        BottomLeft  = new FarmCorner(farmData.MiddlePoint,          bottomLeftUV),
                        BottomRight = new FarmCorner(farmData.MiddleNorthEastPoint, bottomRightUV),
                        TopLeft     = new FarmCorner(farmData.NorthWestPoint,       topLeftUV),
                        TopRight    = new FarmCorner(farmData.NorthNorthWestPoint, topRightUV)
                    });

                    quads.Add(new FarmQuad() {
                        BottomLeft  = new FarmCorner(farmData.MiddleNorthEastPoint, bottomLeftUV),
                        BottomRight = new FarmCorner(farmData.NorthEastPoint,       bottomRightUV),
                        TopLeft     = new FarmCorner(farmData.NorthNorthWestPoint, topLeftUV),
                        TopRight    = new FarmCorner(farmData.NorthPoint,           topRightUV)
                    });
                }
            }           
        }

        private void BuildEasternQuads_Flatlands(IHexCell cell, FarmData farmData, List<FarmQuad> quads) {
            var quadrantHash = NoiseGenerator.SampleHashGrid(farmData.MiddleEastPoint);

            if(true || quadrantHash.A < Config.FarmDivideIntoSquaresChance) {
                quads.Add(BuildSquareFarmQuad(
                    farmData.MiddlePoint,          farmData.MiddleSouthEastPoint,
                    farmData.MiddleNorthEastPoint, farmData.MiddleEastPoint
                ));
            }
        }

        private void BuildSouthernQuads_Flatlands(IHexCell cell, FarmData farmData, List<FarmQuad> quads) {
            var quadrantHash = NoiseGenerator.SampleHashGrid(farmData.MiddleSouthPoint);

            if(quadrantHash.A < Config.FarmDivideIntoSquaresChance) {
                //Four square farms
                quads.Add(BuildSquareFarmQuad(
                    farmData.SouthPoint,          farmData.SouthSouthEastPoint,
                    farmData.SouthSouthWestPoint, farmData.MiddleSouthPoint
                ));

                quads.Add(BuildSquareFarmQuad(
                    farmData.SouthSouthEastPoint, farmData.SouthEastPoint,
                    farmData.MiddleSouthPoint,    farmData.MiddleSouthEastPoint
                ));
                
                quads.Add(BuildSquareFarmQuad(
                    farmData.SouthSouthWestPoint, farmData.MiddleSouthPoint,
                    farmData.SouthWestPoint,      farmData.MiddleSouthWestPoint
                ));

                quads.Add(BuildSquareFarmQuad(
                    farmData.MiddleSouthPoint,     farmData.MiddleSouthEastPoint,
                    farmData.MiddleSouthWestPoint, farmData.MiddlePoint
                ));

            }else {
                Vector2 bottomLeftUV  = new Vector2(0f, 0f);
                Vector2 bottomRightUV = new Vector2(1f, 0f);
                Vector2 topLeftUV     = new Vector2(0f, 1f);
                Vector2 topRightUV    = new Vector2(1f, 1f);

                if(quadrantHash.B <= 0.5f) {
                    //Bottom of the quad aligned with the southeast edge
                     quads.Add(new FarmQuad() {
                        BottomLeft  = new FarmCorner(farmData.SouthPoint,           bottomLeftUV),
                        BottomRight = new FarmCorner(farmData.SouthSouthEastPoint,  bottomRightUV),
                        TopLeft     = new FarmCorner(farmData.SouthWestPoint,       topLeftUV),
                        TopRight    = new FarmCorner(farmData.MiddleSouthWestPoint, topRightUV)
                    });

                    quads.Add(new FarmQuad() {
                        BottomLeft  = new FarmCorner(farmData.SouthSouthEastPoint,  bottomLeftUV),
                        BottomRight = new FarmCorner(farmData.SouthEastPoint,       bottomRightUV),
                        TopLeft     = new FarmCorner(farmData.MiddleSouthWestPoint, topLeftUV),
                        TopRight    = new FarmCorner(farmData.MiddlePoint,          topRightUV)
                    });

                }else {
                    //Bottom of the quad aligned with the southwest edge
                    quads.Add(new FarmQuad() {
                        BottomLeft  = new FarmCorner(farmData.SouthWestPoint,       bottomLeftUV),
                        BottomRight = new FarmCorner(farmData.SouthSouthWestPoint,  bottomRightUV),
                        TopLeft     = new FarmCorner(farmData.MiddlePoint,          topLeftUV),
                        TopRight    = new FarmCorner(farmData.MiddleSouthEastPoint, topRightUV)
                    });

                    quads.Add(new FarmQuad() {
                        BottomLeft  = new FarmCorner(farmData.SouthSouthWestPoint,  bottomLeftUV),
                        BottomRight = new FarmCorner(farmData.SouthPoint,           bottomRightUV),
                        TopLeft     = new FarmCorner(farmData.MiddleSouthEastPoint, topLeftUV),
                        TopRight    = new FarmCorner(farmData.SouthEastPoint,       topRightUV)
                    });
                }
            } 
        }

        private void BuildWesternQuads_Flatlands(IHexCell cell, FarmData farmData, List<FarmQuad> quads) {
            var quadrantHash = NoiseGenerator.SampleHashGrid(farmData.MiddleEastPoint);

            if(true || quadrantHash.A < Config.FarmDivideIntoSquaresChance) {
                quads.Add(BuildSquareFarmQuad(
                    farmData.MiddleSouthWestPoint, farmData.MiddlePoint,
                    farmData.MiddleWestPoint,      farmData.MiddleNorthWestPoint
                ));
            }
        }

        private void BuildNorthernQuads_Hills(IHexCell cell, FarmData farmData, List<FarmQuad> quads) {

        }

        private FarmQuad BuildSquareFarmQuad(
            Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight
        ) {
            Vector2 bottomLeftUV, bottomRightUV, topLeftUV, topRightUV;

            var quadHash = NoiseGenerator.SampleHashGrid(
                (bottomLeft + bottomRight + topLeft + topRight) / 4f
            );

            if(quadHash.C <= 0.5f) {
                bottomLeftUV  = new Vector2(0f, 0f);
                bottomRightUV = new Vector2(1f, 0f);
                topLeftUV     = new Vector2(0f, 1f);
                topRightUV    = new Vector2(1f, 1f);
            }else {
                bottomLeftUV  = new Vector2(1f, 0f);
                bottomRightUV = new Vector2(1f, 1f);
                topLeftUV     = new Vector2(0f, 0f);
                topRightUV    = new Vector2(0f, 1f);
            }

            return new FarmQuad() {
                BottomLeft  = new FarmCorner(bottomLeft,  bottomLeftUV),
                BottomRight = new FarmCorner(bottomRight, bottomRightUV),
                TopLeft     = new FarmCorner(topLeft,     topLeftUV),
                TopRight    = new FarmCorner(topRight,    topRightUV),
            };
        }

        private bool HasFarm(IHexCell cell) {
            var improvementAt = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            return improvementAt != null && improvementAt.Template == FarmTemplate;
        }

        #endregion

    }

}
