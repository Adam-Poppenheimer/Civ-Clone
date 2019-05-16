using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Combat {

    public class CombatConditionTests : ZenjectUnitTestFixture {

        #region internal types

        public class IsConditionMetTestData {

            public UnitTestData Subject  = new UnitTestData();
            public UnitTestData Opponent = new UnitTestData();

            public HexCellTestData Location = new HexCellTestData();

            public CombatType CombatType;

            public CombatCondition Condition;

        }

        public class UnitTestData {

            public UnitType Type;

            public bool IsWounded;

        }

        public class HexCellTestData {

            public bool IsRoughTerrain;

            public CellTerrain    Terrain;
            public CellShape      Shape;
            public CellVegetation Vegetation;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable IsConditionMetTestCases_Subject {
            get {
                yield return new TestCaseData(new IsConditionMetTestData() {
                    Subject = new UnitTestData() { Type = UnitType.Mounted },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Subject,
                        Restriction       = CombatCondition.RestrictionType        .MustBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee, UnitType.Mounted }
                    }
                }).SetName("Subject MustBe OfType | Subject is of argued type").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Subject = new UnitTestData() { Type = UnitType.NavalMelee },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Subject,
                        Restriction       = CombatCondition.RestrictionType        .MustBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee, UnitType.Mounted }
                    }
                }).SetName("Subject MustBe OfType | Subject is not of argued type").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Subject = new UnitTestData() { IsWounded = true },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Subject,
                        Restriction       = CombatCondition.RestrictionType        .MustBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.Wounded
                    }
                }).SetName("Subject MustBe Wounded | Subject is wounded").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Subject = new UnitTestData() { IsWounded = false },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Subject,
                        Restriction       = CombatCondition.RestrictionType        .MustBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.Wounded
                    }
                }).SetName("Subject MustBe Wounded | Subject is not wounded").Returns(false);


                yield return new TestCaseData(new IsConditionMetTestData() {
                    Subject = new UnitTestData() { Type = UnitType.Mounted },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Subject,
                        Restriction       = CombatCondition.RestrictionType        .MustNotBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee, UnitType.Mounted }
                    }
                }).SetName("Subject MustNotBe OfType | Subject is of argued type").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Subject = new UnitTestData() { Type = UnitType.NavalMelee },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Subject,
                        Restriction       = CombatCondition.RestrictionType        .MustNotBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee, UnitType.Mounted }
                    }
                }).SetName("Subject MustNotBe OfType | Subject is not of argued type").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Subject = new UnitTestData() { IsWounded = true },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Subject,
                        Restriction       = CombatCondition.RestrictionType        .MustNotBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.Wounded
                    }
                }).SetName("Subject MustNotBe Wounded | Subject is wounded").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Subject = new UnitTestData() { IsWounded = false },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Subject,
                        Restriction       = CombatCondition.RestrictionType        .MustNotBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.Wounded
                    }
                }).SetName("Subject MustNotBe Wounded | Subject is not wounded").Returns(true);
            }
        }

        public static IEnumerable IsConditionMetTestCases_Opponent {
            get {
                yield return new TestCaseData(new IsConditionMetTestData() {
                    Opponent = new UnitTestData() { Type = UnitType.Mounted },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Opponent,
                        Restriction       = CombatCondition.RestrictionType        .MustBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee, UnitType.Mounted }
                    }
                }).SetName("Opponent MustBe OfType | Opponent is of argued type").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Opponent = new UnitTestData() { Type = UnitType.NavalMelee },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Opponent,
                        Restriction       = CombatCondition.RestrictionType        .MustBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee, UnitType.Mounted }
                    }
                }).SetName("Opponent MustBe OfType | Opponent is not of argued type").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Opponent = new UnitTestData() { IsWounded = true },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Opponent,
                        Restriction       = CombatCondition.RestrictionType        .MustBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.Wounded
                    }
                }).SetName("Opponent MustBe Wounded | Opponent is wounded").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Opponent = new UnitTestData() { IsWounded = false },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Opponent,
                        Restriction       = CombatCondition.RestrictionType        .MustBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.Wounded
                    }
                }).SetName("Opponent MustBe Wounded | Opponent is not wounded").Returns(false);


                yield return new TestCaseData(new IsConditionMetTestData() {
                    Opponent = new UnitTestData() { Type = UnitType.Mounted },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Opponent,
                        Restriction       = CombatCondition.RestrictionType        .MustNotBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee, UnitType.Mounted }
                    }
                }).SetName("Opponent MustNotBe OfType | Opponent is of argued type").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Opponent = new UnitTestData() { Type = UnitType.NavalMelee },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Opponent,
                        Restriction       = CombatCondition.RestrictionType        .MustNotBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() { UnitType.Melee, UnitType.Mounted }
                    }
                }).SetName("Opponent MustNotBe OfType | Opponent is not of argued type").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Opponent = new UnitTestData() { IsWounded = true },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Opponent,
                        Restriction       = CombatCondition.RestrictionType        .MustNotBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.Wounded
                    }
                }).SetName("Opponent MustNotBe Wounded | Opponent is wounded").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Opponent = new UnitTestData() { IsWounded = false },
                    Condition = new CombatCondition() {
                        Target            = CombatCondition.TargetType             .Opponent,
                        Restriction       = CombatCondition.RestrictionType        .MustNotBe,
                        UnitRestriction   = CombatCondition.UnitRestrictionCategory.Wounded
                    }
                }).SetName("Opponent MustNotBe Wounded | Opponent is not wounded").Returns(true);
            }
        }

        public static IEnumerable IsConditionMetTestCases_Location {
            get {
                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.FloodPlains
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfTerrain,
                        TerrainArguments    = new List<CellTerrain>() { CellTerrain.FloodPlains, CellTerrain.Desert }
                    }
                }).SetName("Location MustBe OfTerrain | Location is of terrain").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.Snow
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfTerrain,
                        TerrainArguments    = new List<CellTerrain>() { CellTerrain.FloodPlains, CellTerrain.Desert }
                    }
                }).SetName("Location MustBe OfTerrain | Location is not of terrain").Returns(false);


                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Shape = CellShape.Hills
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfShape,
                        ShapeArguments      = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    }
                }).SetName("Location MustBe OfShape | Location is of shape").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Shape = CellShape.Flatlands
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfShape,
                        ShapeArguments      = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    }
                }).SetName("Location MustBe OfShape | Location is not of shape").Returns(false);


                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Vegetation = CellVegetation.Marsh
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfVegetation,
                        VegetationArguments = new List<CellVegetation>() { CellVegetation.Jungle, CellVegetation.Marsh }
                    }
                }).SetName("Location MustBe OfVegetation | Location is of vegetation").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Vegetation = CellVegetation.Forest
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfVegetation,
                        VegetationArguments = new List<CellVegetation>() { CellVegetation.Jungle, CellVegetation.Marsh }
                    }
                }).SetName("Location MustBe OfVegetation | Location is not of vegetation").Returns(false);


                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        IsRoughTerrain = true
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.RoughTerrain
                    }
                }).SetName("Location MustBe RoughTerrain | Location is rough terrain").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        IsRoughTerrain = false
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.RoughTerrain
                    }
                }).SetName("Location MustBe RoughTerrain | Location is not rough terrain").Returns(false);



                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.FloodPlains
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustNotBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfTerrain,
                        TerrainArguments    = new List<CellTerrain>() { CellTerrain.FloodPlains, CellTerrain.Desert }
                    }
                }).SetName("Location MustNotBe OfTerrain | Location is of terrain").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.Snow
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustNotBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfTerrain,
                        TerrainArguments    = new List<CellTerrain>() { CellTerrain.FloodPlains, CellTerrain.Desert }
                    }
                }).SetName("Location MustNotBe OfTerrain | Location is not of terrain").Returns(true);


                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Shape = CellShape.Hills
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustNotBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfShape,
                        ShapeArguments      = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    }
                }).SetName("Location MustNotBe OfShape | Location is of shape").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Shape = CellShape.Flatlands
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustNotBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfShape,
                        ShapeArguments      = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    }
                }).SetName("Location MustNotBe OfShape | Location is not of shape").Returns(true);


                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Vegetation = CellVegetation.Marsh
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustNotBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfVegetation,
                        VegetationArguments = new List<CellVegetation>() { CellVegetation.Jungle, CellVegetation.Marsh }
                    }
                }).SetName("Location MustNotBe OfVegetation | Location is of vegetation").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        Vegetation = CellVegetation.Forest
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustNotBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.OfVegetation,
                        VegetationArguments = new List<CellVegetation>() { CellVegetation.Jungle, CellVegetation.Marsh }
                    }
                }).SetName("Location MustNotBe OfVegetation | Location is not of vegetation").Returns(true);


                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        IsRoughTerrain = true
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustNotBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.RoughTerrain
                    }
                }).SetName("Location MustNotBe RoughTerrain | Location is rough terrain").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    Location = new HexCellTestData() {
                        IsRoughTerrain = false
                    },
                    Condition = new CombatCondition() {
                        Target              = CombatCondition.TargetType                 .Location,
                        Restriction         = CombatCondition.RestrictionType            .MustNotBe,
                        LocationRestriction = CombatCondition.LocationRestrictionCategory.RoughTerrain
                    }
                }).SetName("Location MustNotBe RoughTerrain | Location is not rough terrain").Returns(true);
            }
        }

        public static IEnumerable IsConditionMetTestCases_CombatType {
            get {
                yield return new TestCaseData(new IsConditionMetTestData() {
                    CombatType = CombatType.Ranged,
                    Condition = new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Ranged
                    }
                }).SetName("CombatType MustBe Ranged | Combat type is Ranged").Returns(true);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    CombatType = CombatType.Melee,
                    Condition = new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustBe,
                        CombatTypeArgument = CombatType.Ranged
                    }
                }).SetName("CombatType MustBe Ranged | Combat type is not Ranged").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    CombatType = CombatType.Ranged,
                    Condition = new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustNotBe,
                        CombatTypeArgument = CombatType.Ranged
                    }
                }).SetName("CombatType MustNotBe Ranged | Combat type is Ranged").Returns(false);

                yield return new TestCaseData(new IsConditionMetTestData() {
                    CombatType = CombatType.Melee,
                    Condition = new CombatCondition() {
                        Target             = CombatCondition.TargetType.CombatType,
                        Restriction        = CombatCondition.RestrictionType.MustNotBe,
                        CombatTypeArgument = CombatType.Ranged
                    }
                }).SetName("CombatType MustNotBe Ranged | Combat type is not Ranged").Returns(true);
            }
        }

        #endregion

        #region instance methods

        #region tests

        [Test]
        [TestCaseSource("IsConditionMetTestCases_CombatType")]
        [TestCaseSource("IsConditionMetTestCases_Location")]
        [TestCaseSource("IsConditionMetTestCases_Opponent")]
        [TestCaseSource("IsConditionMetTestCases_Subject")]
        public bool IsConditionMetTests(IsConditionMetTestData testData) {
            var subject  = BuildUnit(testData.Subject);
            var opponent = BuildUnit(testData.Opponent);

            var location = BuildHexCell(testData.Location);

            return testData.Condition.IsConditionMet(subject, opponent, location, testData.CombatType);
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(UnitTestData unitData) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type)     .Returns(unitData.Type);
            mockUnit.Setup(unit => unit.IsWounded).Returns(unitData.IsWounded);

            return mockUnit.Object;
        }

        private IHexCell BuildHexCell(HexCellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.IsRoughTerrain).Returns(cellData.IsRoughTerrain);
            mockCell.Setup(cell => cell.Terrain)       .Returns(cellData.Terrain);
            mockCell.Setup(cell => cell.Shape)         .Returns(cellData.Shape);
            mockCell.Setup(cell => cell.Vegetation)    .Returns(cellData.Vegetation);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
