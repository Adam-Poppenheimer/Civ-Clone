using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.MapManagement;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class ImprovementComposerTests : ZenjectUnitTestFixture {

        #region internal types

        public class ComposeImprovementsTestData {

            public List<ImprovementTestData> Improvements;

        }

        public struct ImprovementTestData {

            public HexCoordinates LocationCoordinates;

            public string TemplateName;

            public float WorkInvested;

            public bool IsConstructed;

            public bool IsPillaged;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable ComposeImprovementsTestCases {
            get {
                yield return new TestCaseData(new ComposeImprovementsTestData() {
                    Improvements = new List<ImprovementTestData>() {
                        new ImprovementTestData() { LocationCoordinates = new HexCoordinates(0, 0) },
                        new ImprovementTestData() { LocationCoordinates = new HexCoordinates(1, 1) },
                        new ImprovementTestData() { LocationCoordinates = new HexCoordinates(2, 2) },
                    }
                }).SetName("Composes location as coordinates");

                yield return new TestCaseData(new ComposeImprovementsTestData() {
                    Improvements = new List<ImprovementTestData>() {
                        new ImprovementTestData() { TemplateName = "Template One" },
                        new ImprovementTestData() { TemplateName = "Template Two" },
                        new ImprovementTestData() { TemplateName = "Template Three" },
                    }
                }).SetName("Composes template as template name");

                yield return new TestCaseData(new ComposeImprovementsTestData() {
                    Improvements = new List<ImprovementTestData>() {
                        new ImprovementTestData() { WorkInvested = 1 },
                        new ImprovementTestData() { WorkInvested = 20 },
                        new ImprovementTestData() { WorkInvested = 300 },
                    }
                }).SetName("Composes WorkInvested");

                yield return new TestCaseData(new ComposeImprovementsTestData() {
                    Improvements = new List<ImprovementTestData>() {
                        new ImprovementTestData() { IsConstructed = true },
                        new ImprovementTestData() { IsConstructed = false },
                        new ImprovementTestData() { IsConstructed = true },
                    }
                }).SetName("Composes IsConstructed");

                yield return new TestCaseData(new ComposeImprovementsTestData() {
                    Improvements = new List<ImprovementTestData>() {
                        new ImprovementTestData() { IsPillaged = true },
                        new ImprovementTestData() { IsPillaged = false },
                        new ImprovementTestData() { IsPillaged = true },
                    }
                }).SetName("Composes IsPillaged");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IImprovementFactory>       MockImprovementFactory;
        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;
        private Mock<IHexGrid>                  MockGrid;

        private List<IImprovement>         AllImprovements    = new List<IImprovement>();
        private List<IImprovementTemplate> AvailableTemplates = new List<IImprovementTemplate>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllImprovements   .Clear();
            AvailableTemplates.Clear();

            MockImprovementFactory       = new Mock<IImprovementFactory>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockGrid                     = new Mock<IHexGrid>();

            MockImprovementFactory.Setup(factory => factory.AllImprovements).Returns(AllImprovements);

            Container.Bind<IImprovementFactory>      ().FromInstance(MockImprovementFactory      .Object);
            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<IHexGrid>                 ().FromInstance(MockGrid                    .Object);

            Container.Bind<IEnumerable<IImprovementTemplate>>()
                     .WithId("Available Improvement Templates")
                     .FromInstance(AvailableTemplates);

            Container.Bind<ImprovementComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearRuntime_AllImprovementsRelocatedToNull() {
            var improvementOne   = BuildImprovement();
            var improvementTwo   = BuildImprovement();
            var improvementThree = BuildImprovement();

            var composer = Container.Resolve<ImprovementComposer>();

            composer.ClearRuntime();

            MockImprovementLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(improvementOne, null), Times.Once,
                "ImprovementOne did not have its location nulled in ImprovementLocationCanon"
            );

            MockImprovementLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(improvementTwo, null), Times.Once,
                "ImprovementTwo did not have its location nulled in ImprovementLocationCanon"
            );

            MockImprovementLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(improvementThree, null), Times.Once,
                "ImprovementThree did not have its location nulled in ImprovementLocationCanon"
            );
        }

        [Test]
        public void ClearRuntime_AllImprovementsDestroyed() {
            Mock<IImprovement> mockOne, mockTwo, mockThree;

            BuildImprovement(out mockOne);
            BuildImprovement(out mockTwo);
            BuildImprovement(out mockThree);

            var composer = Container.Resolve<ImprovementComposer>();

            composer.ClearRuntime();

            mockOne  .Verify(improvement => improvement.Destroy(), Times.Once, "ImprovementOne was not destroyed");
            mockTwo  .Verify(improvement => improvement.Destroy(), Times.Once, "ImprovementTwo was not destroyed");
            mockThree.Verify(improvement => improvement.Destroy(), Times.Once, "ImprovementThree was not destroyed");
        }

        [Test]
        [TestCaseSource("ComposeImprovementsTestCases")]
        public void ComposeImprovementsTests(ComposeImprovementsTestData testData) {
            foreach(var improvementTestData in testData.Improvements) {
                BuildImprovement(improvementTestData);
            }

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<ImprovementComposer>();

            composer.ComposeImprovements(mapData);

            var resultsAsTestData = mapData.Improvements.Select(data => new ImprovementTestData() {
                LocationCoordinates = data.Location,
                TemplateName        = data.Template,
                WorkInvested        = data.WorkInvested,
                IsConstructed       = data.IsConstructed,
                IsPillaged          = data.IsPillaged
            });

            CollectionAssert.AreEquivalent(
                testData.Improvements, resultsAsTestData,
                "The improvements passed in and the improvements placed into MapData are not equivalent sets"
            );
        }

        [Test]
        public void DecomposeImprovements_ConvertsLocationProperly() {
            var mapData = new SerializableMapData() {
                Improvements = new List<SerializableImprovementData>() {
                    new SerializableImprovementData() {
                        Location = new HexCoordinates(0, 0), Template = "Template One"
                    }
                }
            };

            BuildImprovementTemplate("Template One");

            var location = BuildHexCell(new HexCoordinates(0, 0));

            var composer = Container.Resolve<ImprovementComposer>();

            composer.DecomposeImprovements(mapData);

            MockImprovementFactory.Verify(
                factory => factory.BuildImprovement(
                    It.IsAny<IImprovementTemplate>(), location, It.IsAny<float>(),
                    It.IsAny<bool>(), It.IsAny<bool>()
                ), Times.Once, "BuildImprovement was not called with the expected location argument"
            );
        }

        [Test]
        public void DecomposeImprovements_ConvertsTemplateProperly() {
            var mapData = new SerializableMapData() {
                Improvements = new List<SerializableImprovementData>() {
                    new SerializableImprovementData() {
                        Location = new HexCoordinates(0, 0), Template = "Template One"
                    }
                }
            };

            var templateToBuild = BuildImprovementTemplate("Template One");

            BuildHexCell(new HexCoordinates(0, 0));

            var composer = Container.Resolve<ImprovementComposer>();

            composer.DecomposeImprovements(mapData);

            MockImprovementFactory.Verify(
                factory => factory.BuildImprovement(
                    templateToBuild, It.IsAny<IHexCell>(), It.IsAny<float>(),
                    It.IsAny<bool>(), It.IsAny<bool>()
                ), Times.Once, "BuildImprovement was not called with the expected template argument"
            );
        }

        [Test]
        public void DecomposeImprovements_PassesPrimitivesProperly() {
            var mapData = new SerializableMapData() {
                Improvements = new List<SerializableImprovementData>() {
                    new SerializableImprovementData() {
                        Location = new HexCoordinates(0, 0), Template = "Template One",
                        IsConstructed = false, IsPillaged = true, WorkInvested = 10
                    }
                }
            };

            BuildImprovementTemplate("Template One");

            BuildHexCell(new HexCoordinates(0, 0));

            var composer = Container.Resolve<ImprovementComposer>();

            composer.DecomposeImprovements(mapData);

            MockImprovementFactory.Verify(
                factory => factory.BuildImprovement(
                    It.IsAny<IImprovementTemplate>(), It.IsAny<IHexCell>(),
                    10, false, true
                ), Times.Once, "BuildImprovement was not called with the expected primitive arguments"
            );
        }

        #endregion

        #region utilities

        private IImprovement BuildImprovement() {
            Mock<IImprovement> mock;
            return BuildImprovement(out mock);
        }

        private IImprovement BuildImprovement(out Mock<IImprovement> mock) {
            mock = new Mock<IImprovement>();

            var newImprovement = mock.Object;

            AllImprovements.Add(newImprovement);

            return mock.Object;
        }

        private IImprovement BuildImprovement(ImprovementTestData testData) {
            var mockImprovement = new Mock<IImprovement>();

            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.name).Returns(testData.TemplateName);

            mockImprovement.Setup(improvement => improvement.Template)     .Returns(mockTemplate.Object);
            mockImprovement.Setup(improvement => improvement.WorkInvested) .Returns(testData.WorkInvested);
            mockImprovement.Setup(improvement => improvement.IsConstructed).Returns(testData.IsConstructed);
            mockImprovement.Setup(improvement => improvement.IsPillaged)   .Returns(testData.IsPillaged);

            var newImprovement = mockImprovement.Object;

            var location = BuildHexCell(testData.LocationCoordinates);

            MockImprovementLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newImprovement))
                                        .Returns(location);

            AllImprovements.Add(newImprovement);

            return newImprovement;
        }

        private IHexCell BuildHexCell(HexCoordinates coordinates) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Coordinates).Returns(coordinates);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(coordinates)).Returns(newCell);

            return mockCell.Object;
        }

        private IImprovementTemplate BuildImprovementTemplate(string name) {
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.name).Returns(name);

            var newTemplate = mockTemplate.Object;

            AvailableTemplates.Add(newTemplate);

            return newTemplate;
        }

        #endregion

        #endregion

    }

}
