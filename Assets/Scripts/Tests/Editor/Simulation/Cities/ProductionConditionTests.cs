using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Cities {

    public class ProductionConditionTests : ZenjectUnitTestFixture {

        #region internal types

        public class IsMetByTestData {

            public ProductionCondition Condition;

            public ProjectTestData Project;

            public CityTestData City;

        }

        public class ProjectTestData {

            public UnitTestData Unit;

            public BuildingTestData Building;

        }

        public class UnitTestData {

            public string Name;

            public UnitType Type;

        }

        public class BuildingTestData {

            public string Name;

            public BuildingType Type;

        }

        public class CityTestData {



        }
        
        #endregion

        #region static fields and properties

        public static IEnumerable IsMetByTestCases_UnitProject {
            get {
                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitRestriction = ProductionCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() {
                            UnitType.Melee, UnitType.Archery, UnitType.Civilian
                        }
                    },
                    Project = new ProjectTestData() {
                        Unit = new UnitTestData() { Type = UnitType.Melee }
                    }
                }).SetName("Project must have unit of type, and project has a unit of an argued type").Returns(true);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitRestriction = ProductionCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() {
                            UnitType.Melee, UnitType.Archery, UnitType.Civilian
                        }
                    },
                    Project = new ProjectTestData() {
                        Unit = new UnitTestData() { Type = UnitType.NavalMelee }
                    }
                }).SetName("Project must have unit of type, and project has a unit of an unargued type").Returns(false);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustNotBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitRestriction = ProductionCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() {
                            UnitType.Melee, UnitType.Archery, UnitType.Civilian
                        }
                    },
                    Project = new ProjectTestData() {
                        Unit = new UnitTestData() { Type = UnitType.Melee }
                    }
                }).SetName("Project must not have unit of type, and project has a unit of an argued type").Returns(false);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustNotBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitRestriction = ProductionCondition.UnitRestrictionCategory.OfType,
                        UnitTypeArguments = new List<UnitType>() {
                            UnitType.Melee, UnitType.Archery, UnitType.Civilian
                        }
                    },
                    Project = new ProjectTestData() {
                        Unit = new UnitTestData() { Type = UnitType.NavalMelee }
                    }
                }).SetName("Project must not have unit of type, and project has a unit of an unargued type").Returns(true);




                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitRestriction = ProductionCondition.UnitRestrictionCategory.OfName,
                        NameArguments = new List<string>() {
                            "Unit One", "Unit Two", "Unit Three"
                        }
                    },
                    Project = new ProjectTestData() {
                        Unit = new UnitTestData() { Name = "Unit Two" }
                    }
                }).SetName("Project must have unit of name, and project has a unit of an argued name").Returns(true);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitRestriction = ProductionCondition.UnitRestrictionCategory.OfName,
                        NameArguments = new List<string>() {
                            "Unit One", "Unit Two", "Unit Three"
                        }
                    },
                    Project = new ProjectTestData() {
                        Unit = new UnitTestData() { Name = "Unit Four" }
                    }
                }).SetName("Project must have unit of name, and project has a unit of an unargued name").Returns(false);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustNotBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitRestriction = ProductionCondition.UnitRestrictionCategory.OfName,
                        NameArguments = new List<string>() {
                            "Unit One", "Unit Two", "Unit Three"
                        }
                    },
                    Project = new ProjectTestData() {
                        Unit = new UnitTestData() { Name = "Unit Two" }
                    }
                }).SetName("Project must not have unit of name, and project has a unit of an argued name").Returns(false);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustNotBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Unit,
                        UnitRestriction = ProductionCondition.UnitRestrictionCategory.OfName,
                        NameArguments = new List<string>() {
                            "Unit One", "Unit Two", "Unit Three"
                        }
                    },
                    Project = new ProjectTestData() {
                        Unit = new UnitTestData() { Name = "Unit Four" }
                    }
                }).SetName("Project must not have unit of name, and project has a unit of an unargued name").Returns(true);
            }
        }

        public static IEnumerable IsMetByTestCases_BuildingProject {
            get {
                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Building,
                        BuildingRestriction = ProductionCondition.BuildingRestrictionCategory.OfType,
                        BuildingTypeArguments = new List<BuildingType>() {
                            BuildingType.Normal, BuildingType.NationalWonder
                        }
                    },
                    Project = new ProjectTestData() {
                        Building = new BuildingTestData() { Type = BuildingType.NationalWonder }
                    }
                }).SetName("Project must have building of type, and project has a building of an argued type").Returns(true);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Building,
                        BuildingRestriction = ProductionCondition.BuildingRestrictionCategory.OfType,
                        BuildingTypeArguments = new List<BuildingType>() {
                            BuildingType.Normal, BuildingType.NationalWonder
                        }
                    },
                    Project = new ProjectTestData() {
                        Building = new BuildingTestData() { Type = BuildingType.WorldWonder }
                    }
                }).SetName("Project must have building of type, and project has a building of an unargued type").Returns(false);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustNotBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Building,
                        BuildingRestriction = ProductionCondition.BuildingRestrictionCategory.OfType,
                        BuildingTypeArguments = new List<BuildingType>() {
                            BuildingType.Normal, BuildingType.NationalWonder
                        }
                    },
                    Project = new ProjectTestData() {
                        Building = new BuildingTestData() { Type = BuildingType.NationalWonder }
                    }
                }).SetName("Project must not have building of type, and project has a building of an argued type").Returns(false);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustNotBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Building,
                        BuildingRestriction = ProductionCondition.BuildingRestrictionCategory.OfType,
                        BuildingTypeArguments = new List<BuildingType>() {
                            BuildingType.Normal, BuildingType.NationalWonder
                        }
                    },
                    Project = new ProjectTestData() {
                        Building = new BuildingTestData() { Type = BuildingType.WorldWonder }
                    }
                }).SetName("Project must not have building of type, and project has a building of an unargued type").Returns(true);




                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Building,
                        BuildingRestriction = ProductionCondition.BuildingRestrictionCategory.OfName,
                        NameArguments = new List<string>() {
                            "Building One", "Building Two", "Building Three"
                        }
                    },
                    Project = new ProjectTestData() {
                        Building = new BuildingTestData() { Name = "Building Two" }
                    }
                }).SetName("Project must have building of name, and project has a building of an argued name").Returns(true);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Building,
                        BuildingRestriction = ProductionCondition.BuildingRestrictionCategory.OfName,
                        NameArguments = new List<string>() {
                            "Building One", "Building Two", "Building Three"
                        }
                    },
                    Project = new ProjectTestData() {
                        Building = new BuildingTestData() { Name = "Building Four" }
                    }
                }).SetName("Project must have building of name, and project has a building of an unargued name").Returns(false);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustNotBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Building,
                        BuildingRestriction = ProductionCondition.BuildingRestrictionCategory.OfName,
                        NameArguments = new List<string>() {
                            "Building One", "Building Two", "Building Three"
                        }
                    },
                    Project = new ProjectTestData() {
                        Building = new BuildingTestData() { Name = "Building Two" }
                    }
                }).SetName("Project must not have building of name, and project has a building of an argued name").Returns(false);

                yield return new TestCaseData(new IsMetByTestData() {
                    Condition = new ProductionCondition() {
                        Target = ProductionCondition.TargetType.Project,
                        Restriction = ProductionCondition.RestrictionType.MustNotBe,
                        ProjectRestriction = ProductionCondition.ProjectRestrictionCategory.Building,
                        BuildingRestriction = ProductionCondition.BuildingRestrictionCategory.OfName,
                        NameArguments = new List<string>() {
                            "Building One", "Building Two", "Building Three"
                        }
                    },
                    Project = new ProjectTestData() {
                        Building = new BuildingTestData() { Name = "Building Four" }
                    }
                }).SetName("Project must not have building of name, and project has a building of an unargued name").Returns(true);
            }
        }

        #endregion

        #region instance methods

        #region tests

        [Test]
        [TestCaseSource("IsMetByTestCases_UnitProject")]
        [TestCaseSource("IsMetByTestCases_BuildingProject")]
        public bool IsMetByTests(IsMetByTestData testData) {
            var project = BuildProject(testData.Project);
            var city = BuildCity(testData.City);

            return testData.Condition.IsMetBy(project, city);
        }

        #endregion

        #region utilities

        private IProductionProject BuildProject(ProjectTestData projectData) {
            var mockProject = new Mock<IProductionProject>();

            if(projectData.Unit != null) {
                mockProject.Setup(project => project.UnitToConstruct).Returns(BuildUnit(projectData.Unit));
            }
            
            if(projectData.Building != null) {
                mockProject.Setup(project => project.BuildingToConstruct).Returns(BuildBuilding(projectData.Building));
            }

            return mockProject.Object;
        }

        private ICity BuildCity(CityTestData cityData) {
            return new Mock<ICity>().Object;
        }

        private IUnitTemplate BuildUnit(UnitTestData unitData) {
            var mockUnit = new Mock<IUnitTemplate>();

            mockUnit.Setup(unit => unit.Type).Returns(unitData.Type);
            mockUnit.Setup(unit => unit.name).Returns(unitData.Name);

            return mockUnit.Object;
        }

        private IBuildingTemplate BuildBuilding(BuildingTestData buildingData) {
            var mockBuilding = new Mock<IBuildingTemplate>();

            mockBuilding.Setup(unit => unit.Type).Returns(buildingData.Type);
            mockBuilding.Setup(unit => unit.name).Returns(buildingData.Name);

            return mockBuilding.Object;
        }

        #endregion

        #endregion

    }

}
