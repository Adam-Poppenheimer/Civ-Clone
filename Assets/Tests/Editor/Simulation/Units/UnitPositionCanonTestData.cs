using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units {

    public static class UnitPositionCanonTestData {

        #region internal types

        public class UnitAtLocationData {

            public UnitData Unit = new UnitData();

            public HexCellData Location = new HexCellData();

            public bool IsMeleeAttacking;

            public ConfigData Config = new ConfigData();

        }

        public class TemplateAtLocationData {

            public UnitTemplateData Template;

            public HexCellData Location = new HexCellData();

            public ConfigData Config = new ConfigData();

        }

        public class TraversalCostData {

            public UnitData Unit = new UnitData();

            public HexCellData CurrentCell = new HexCellData();

            public HexCellData NextCell = new HexCellData();

            public bool IsMeleeAttacking;

            public ConfigData Config = new ConfigData();

        }

        public class UnitData {

            public UnitType Type;

            public int MaxMovement;

            public UnitMovementSummary MovementSummary = new UnitMovementSummary();

            public bool BelongsToDomesticCiv;

        }

        public class UnitTemplateData {

            public UnitType type;

            public UnitMovementSummary MovementSummary = new UnitMovementSummary();

        }

        public class HexCellData {

            public CellTerrain    Terrain;
            public CellShape      Shape;
            public CellVegetation Vegetation;
            public CellFeature    Feature;

            public bool HasRoads;

            public CityData City;

            public List<UnitData> Units = new List<UnitData>();

        }

        public class CityData {

            public bool BelongsToDomesticCiv;

        }

        public class ConfigData {
            
            public int CityMoveCost = 1;

            public int TerrainMoveCost    = 1;
            public int ShapeMoveCost      = 1;
            public int VegetationMoveCost = 1;
            public int FeatureMoveCost    = 1;

            public float RoadMoveCostMultipler = 0.25f;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable CanPlaceUnitAtLocationNormalTestCases {
            get {
                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = null
                }).SetName("Location is null").Returns(true);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.None, Feature = CellFeature.None
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).SetName("Cell is empty land and unit can traverse land").Returns(true);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.None, Feature = CellFeature.None
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = false }
                    }
                }).SetName("Cell is land and unit cannot traverse land").Returns(false);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() { Terrain = CellTerrain.ShallowWater },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseShallowWater = true }
                    }
                }).SetName("Cell is shallow water and unit can traverse shallow water").Returns(true);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() { Terrain = CellTerrain.FreshWater },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseShallowWater = true }
                    }
                }).SetName("Cell is fresh water and unit can traverse shallow water").Returns(true);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() { Terrain = CellTerrain.DeepWater },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseDeepWater = true }
                    }
                }).SetName("Cell is deep water and unit can traverse deep water").Returns(true);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() { Terrain = CellTerrain.DeepWater },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseShallowWater = true }
                    }
                }).SetName("Cell is deep water and unit can only traverse shallow water").Returns(false);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() { Vegetation = CellVegetation.Forest },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    },
                    Config = new ConfigData() { VegetationMoveCost = -1 }
                }).SetName("Cell's vegetation has a negative base move cost").Returns(false);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() { Shape = CellShape.Hills },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    },
                    Config = new ConfigData() { ShapeMoveCost = -1 }
                }).SetName("Cell's shape has a negative base move cost").Returns(false);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() { Feature = CellFeature.Oasis },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    },
                    Config = new ConfigData() { FeatureMoveCost = -1 }
                }).SetName("Cell's feature has a negative base move cost").Returns(false);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() {
                        Terrain = CellTerrain.Grassland,
                        City = new CityData() { BelongsToDomesticCiv = true }
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = false, CanTraverseShallowWater = true
                        },
                        BelongsToDomesticCiv = true
                    }
                }).SetName("Cell is land and has a domestic city, and unit can only traverse water").Returns(true);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() {
                        Terrain = CellTerrain.Grassland,
                        City = new CityData() { BelongsToDomesticCiv = false }
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true },
                        BelongsToDomesticCiv = true
                    }
                }).SetName("Cell is land and has a foreign city, unit can traverse land").Returns(false);



                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() {
                        Terrain = CellTerrain.Grassland,
                        Units = new List<UnitData>() {
                            new UnitData() {
                                BelongsToDomesticCiv = true, Type = UnitType.NavalMelee,
                                MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                            }
                        }
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true },
                        Type = UnitType.Melee, BelongsToDomesticCiv = true
                    }
                }).SetName("Cell has domestic units, but with different supertypes than the unit").Returns(true);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() {
                        Terrain = CellTerrain.Grassland,
                        Units = new List<UnitData>() {
                            new UnitData() {
                                BelongsToDomesticCiv = true, Type = UnitType.Archery,
                                MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                            }
                        }
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true },
                        Type = UnitType.Melee, BelongsToDomesticCiv = true
                    }
                }).SetName("Cell has domestic units with the same supertypes as the unit").Returns(false);

                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() {
                        Terrain = CellTerrain.Grassland,
                        Units = new List<UnitData>() {
                            new UnitData() {
                                BelongsToDomesticCiv = false, Type = UnitType.Civilian,
                                MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                            }
                        }
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true },
                        Type = UnitType.Melee, BelongsToDomesticCiv = true
                    }
                }).SetName("Cell has foreign units with different supertypes than the unit").Returns(false);
            }
        }

        public static IEnumerable CanPlaceUnitAtLocationAttackingTestCases {
            get {
                yield return new TestCaseData(new UnitAtLocationData() {
                    Location = new HexCellData() {
                        Terrain = CellTerrain.Grassland,
                        Units = new List<UnitData>() {
                            new UnitData() {
                                BelongsToDomesticCiv = false, Type = UnitType.Civilian,
                                MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                            }
                        }
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true },
                        Type = UnitType.Melee, BelongsToDomesticCiv = true
                    },
                    IsMeleeAttacking = true
                }).SetName("Cell has foreign units, but unit is melee attacking").Returns(true);
            }
        }

        public static IEnumerable CanPlaceUnitTemplateAtLocationTestCases {
            get {
                foreach(var testCase in CanPlaceUnitAtLocationNormalTestCases) {
                    var castTestCase = testCase as TestCaseData;

                    var testData = castTestCase.Arguments[0] as UnitAtLocationData;

                    yield return new TestCaseData(new TemplateAtLocationData() {
                        Template = new UnitTemplateData() {
                            type = testData.Unit.Type, MovementSummary = testData.Unit.MovementSummary
                        },
                        Location = testData.Location,
                        Config   = testData.Config
                    }).SetName(castTestCase.TestName).Returns(castTestCase.ExpectedResult);
                }
            }
        }

        public static IEnumerable GetTraversalCostForUnitTestCases {
            get {
                yield return new TestCaseData(new TraversalCostData() {
                    NextCell = new HexCellData() { Terrain = CellTerrain.Grassland },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = false }
                    }
                }).SetName("NextCell is impassable for unit").Returns(-1f);

                yield return new TestCaseData(new TraversalCostData() {
                    NextCell = new HexCellData() {
                        Terrain = CellTerrain.Grassland,
                        City = new CityData() { BelongsToDomesticCiv = true }
                    },
                    Unit = new UnitData() {
                        BelongsToDomesticCiv = true,
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    },
                    Config = new ConfigData() {
                        CityMoveCost = 5
                    }
                }).SetName("NextCell has city").Returns(5f);

                yield return new TestCaseData(new TraversalCostData() {
                    NextCell = new HexCellData() {
                        Terrain = CellTerrain.Desert, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.Forest, Feature = CellFeature.None
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    },
                    Config = new ConfigData() {
                        TerrainMoveCost = 1, ShapeMoveCost = 2, VegetationMoveCost = 3, FeatureMoveCost = 4
                    }
                }).SetName("Unit has no special properties | terrain, shape, vegetation, and feature costs all added").Returns(10);

                yield return new TestCaseData(new TraversalCostData() {
                    NextCell = new HexCellData() {
                        Terrain = CellTerrain.Desert, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.Forest, Feature = CellFeature.None
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true,
                            TerrainsWithIgnoredCosts = new HashSet<CellTerrain>() { CellTerrain.Desert }
                        }
                    },
                    Config = new ConfigData() {
                        TerrainMoveCost = 1, ShapeMoveCost = 2, VegetationMoveCost = 3, FeatureMoveCost = 4
                    }
                }).SetName("Unit ignores costs on terrain of next cell | only terrain costs are considered").Returns(1);

                yield return new TestCaseData(new TraversalCostData() {
                    NextCell = new HexCellData() {
                        Terrain = CellTerrain.Desert, Shape = CellShape.Mountains,
                        Vegetation = CellVegetation.Forest, Feature = CellFeature.None
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true,
                            ShapesWithIgnoredCosts = new HashSet<CellShape>() { CellShape.Mountains }
                        }
                    },
                    Config = new ConfigData() {
                        TerrainMoveCost = 1, ShapeMoveCost = 2, VegetationMoveCost = 3, FeatureMoveCost = 4
                    }
                }).SetName("Unit ignores costs on shape of next cell | only terrain costs are considered").Returns(1);

                yield return new TestCaseData(new TraversalCostData() {
                    NextCell = new HexCellData() {
                        Terrain = CellTerrain.Desert, Shape = CellShape.Mountains,
                        Vegetation = CellVegetation.Forest, Feature = CellFeature.None
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true,
                            VegetationsWithIgnoredCosts = new HashSet<CellVegetation>() { CellVegetation.Forest }
                        }
                    },
                    Config = new ConfigData() {
                        TerrainMoveCost = 1, ShapeMoveCost = 2, VegetationMoveCost = 3, FeatureMoveCost = 4
                    }
                }).SetName("Unit ignores costs on vegetation of next cell | only terrain costs are considered").Returns(1);

                yield return new TestCaseData(new TraversalCostData() {
                    CurrentCell = new HexCellData() {
                        HasRoads = true
                    },
                    NextCell = new HexCellData() {
                        HasRoads = true
                    },
                    Config = new ConfigData() {
                        TerrainMoveCost = 4, ShapeMoveCost = 2, VegetationMoveCost = 3,
                        FeatureMoveCost = 5, RoadMoveCostMultipler = 0.25f
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true
                        }
                    }
                }).SetName("Current and next cell have roads | returns only terrain costs multiplied by road cost modifier").Returns(4 * 0.25f);

                yield return new TestCaseData(new TraversalCostData() {
                    CurrentCell = new HexCellData() {
                        HasRoads = true
                    },
                    NextCell = new HexCellData() {
                        Terrain = CellTerrain.Desert, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.Forest, Feature = CellFeature.None
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    },
                    Config = new ConfigData() {
                        TerrainMoveCost = 1, ShapeMoveCost = 2, VegetationMoveCost = 3, FeatureMoveCost = 4,
                        RoadMoveCostMultipler = 0f
                    }
                }).SetName("Current cell has roads | road modification not triggered").Returns(10);

                yield return new TestCaseData(new TraversalCostData() {
                    CurrentCell = new HexCellData() {
                        
                    },
                    NextCell = new HexCellData() {
                        Terrain = CellTerrain.Desert, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.Forest, Feature = CellFeature.None,
                        HasRoads = true
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    },
                    Config = new ConfigData() {
                        TerrainMoveCost = 1, ShapeMoveCost = 2, VegetationMoveCost = 3, FeatureMoveCost = 4,
                        RoadMoveCostMultipler = 0f
                    }
                }).SetName("Next cell has roads | road modification not triggered").Returns(10);

                yield return new TestCaseData(new TraversalCostData() {
                    NextCell = new HexCellData() {
                        Shape = CellShape.Hills
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true, ShapesConsumingFullMovement = new HashSet<CellShape>() { CellShape.Hills }
                        },
                        MaxMovement = 15
                    }
                }).SetName("Shape of next cell consumes full movement | returns max movement of unit").Returns(15);

                yield return new TestCaseData(new TraversalCostData() {
                    NextCell = new HexCellData() {
                        Vegetation = CellVegetation.Forest
                    },
                    Unit = new UnitData() {
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true, VegetationConsumingFullMovement = new HashSet<CellVegetation>() { CellVegetation.Forest }
                        },
                        MaxMovement = 15
                    }
                }).SetName("Vegetation of next cell consumes full movement | returns max movement of unit").Returns(15);
            }
        }

        #endregion

    }

}
