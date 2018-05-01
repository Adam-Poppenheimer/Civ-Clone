using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.MapManagement;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class ResourceComposerTests : ZenjectUnitTestFixture {

        #region internal types

        public class ComposeResourcesTestData {

            public List<ResourceNodeTestData> ResourceNodes;

        }

        public struct ResourceNodeTestData {

            public HexCoordinates LocationCoordinates;
            
            public string ResourceName;

            public int Copies;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable ComposeResourcesTestCases {
            get {
                yield return new TestCaseData(new ComposeResourcesTestData() {
                    ResourceNodes = new List<ResourceNodeTestData>() {
                        new ResourceNodeTestData() { LocationCoordinates = new HexCoordinates(0, 0) },
                        new ResourceNodeTestData() { LocationCoordinates = new HexCoordinates(1, 1) },
                        new ResourceNodeTestData() { LocationCoordinates = new HexCoordinates(2, 2) },
                    }
                }).SetName("Composes location as coordinates");

                yield return new TestCaseData(new ComposeResourcesTestData() {
                    ResourceNodes = new List<ResourceNodeTestData>() {
                        new ResourceNodeTestData() { ResourceName = "Resource One" },
                        new ResourceNodeTestData() { ResourceName = "Resource Two" },
                        new ResourceNodeTestData() { ResourceName = "Resource Three" },
                    }
                }).SetName("Composes resource as name");

                yield return new TestCaseData(new ComposeResourcesTestData() {
                    ResourceNodes = new List<ResourceNodeTestData>() {
                        new ResourceNodeTestData() { Copies = 1 },
                        new ResourceNodeTestData() { Copies = 20 },
                        new ResourceNodeTestData() { Copies = 300 },
                    }
                }).SetName("Composes Copies");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IResourceNodeFactory>                             MockNodeFactory;
        private Mock<IHexGrid>                                         MockGrid;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodeLocationCanon;

        private List<ISpecialtyResourceDefinition> AvailableSpecialtyResources =
            new List<ISpecialtyResourceDefinition>();

        private List<IResourceNode> AllNodes = new List<IResourceNode>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableSpecialtyResources.Clear();
            AllNodes                   .Clear();

            MockNodeFactory       = new Mock<IResourceNodeFactory>();
            MockGrid              = new Mock<IHexGrid>();
            MockNodeLocationCanon = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();

            MockNodeFactory.Setup(factory => factory.AllNodes).Returns(AllNodes);

            Container.Bind<IResourceNodeFactory>                            ().FromInstance(MockNodeFactory      .Object);
            Container.Bind<IHexGrid>                                        ().FromInstance(MockGrid             .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodeLocationCanon.Object);

            Container.Bind<IEnumerable<ISpecialtyResourceDefinition>>()
                     .WithId("Available Specialty Resources")
                     .FromInstance(AvailableSpecialtyResources);

            Container.Bind<ResourceComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearRuntime_AllNodesSetToNullLocation() {
            var nodeOne   = BuildResourceNode();
            var nodeTwo   = BuildResourceNode();
            var nodeThree = BuildResourceNode();

            var composer = Container.Resolve<ResourceComposer>();

            composer.ClearRuntime();

            MockNodeLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(nodeOne, null), Times.Once,
                "NodeOne did not have its location nulled in NodeLocationCanon"
            );

            MockNodeLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(nodeTwo, null), Times.Once,
                "NodeTwo did not have its location nulled in NodeLocationCanon"
            );

            MockNodeLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(nodeThree, null), Times.Once,
                "NodeThree did not have its location nulled in NodeLocationCanon"
            );
        }

        [Test]
        public void ClearRuntime_AllNodesDestroyed() {
            Mock<IResourceNode> mockOne, mockTwo, mockThree;

            BuildResourceNode(out mockOne);
            BuildResourceNode(out mockTwo);
            BuildResourceNode(out mockThree);

            var composer = Container.Resolve<ResourceComposer>();

            composer.ClearRuntime();

            mockOne  .Verify(node => node.Destroy(), Times.Once, "NodeOne was not destroyed");
            mockTwo  .Verify(node => node.Destroy(), Times.Once, "NodeTwo was not destroyed");
            mockThree.Verify(node => node.Destroy(), Times.Once, "NodeThree was not destroyed");
        }

        [Test]
        [TestCaseSource("ComposeResourcesTestCases")]
        public void ComposeResourcesTests(ComposeResourcesTestData testData) {
            foreach(var nodeTestData in testData.ResourceNodes) {
                BuildResourceNode(nodeTestData);
            }

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<ResourceComposer>();

            composer.ComposeResources(mapData);

            var resultsAsTestData = mapData.ResourceNodes.Select(data => new ResourceNodeTestData() {
                LocationCoordinates = data.Location,
                ResourceName        = data.ResourceName,
                Copies              = data.Copies
            });

            CollectionAssert.AreEquivalent(
                testData.ResourceNodes, resultsAsTestData,
                "The nodes passed in and the nodes placed into MapData are not equivalent sets"
            );
        }

        [Test]
        public void DecomposeResources_ConvertsLocationProperly() {
            var mapData = new SerializableMapData() {
                ResourceNodes = new List<SerializableResourceNodeData>() {
                    new SerializableResourceNodeData() {
                        Location = new HexCoordinates(2, 3), ResourceName = "Resource One",
                        Copies = 5
                    }
                }
            };

            BuildResourceDefinition("Resource One");

            var location = BuildHexCell(new HexCoordinates(2, 3));

            var composer = Container.Resolve<ResourceComposer>();

            composer.DecomposeResources(mapData);

            MockNodeFactory.Verify(
                factory => factory.BuildNode(location, It.IsAny<ISpecialtyResourceDefinition>(), It.IsAny<int>()),
                Times.Once, "NodeFactory.BuildNode was not called with the expected location argument"
            );
        }

        [Test]
        public void DecomposeResources_ConvertsResourceProperly() {
            var mapData = new SerializableMapData() {
                ResourceNodes = new List<SerializableResourceNodeData>() {
                    new SerializableResourceNodeData() {
                        Location = new HexCoordinates(2, 3), ResourceName = "Resource One",
                        Copies = 5
                    }
                }
            };

            var resourceInNewNode = BuildResourceDefinition("Resource One");

            BuildHexCell(new HexCoordinates(2, 3));

            var composer = Container.Resolve<ResourceComposer>();

            composer.DecomposeResources(mapData);

            MockNodeFactory.Verify(
                factory => factory.BuildNode(It.IsAny<IHexCell>(), resourceInNewNode, It.IsAny<int>()),
                Times.Once, "NodeFactory.BuildNode was not called with the expected resource argument"
            );
        }

        [Test]
        public void DecomposeResources_PassesPrimitivesProperly() {
            var mapData = new SerializableMapData() {
                ResourceNodes = new List<SerializableResourceNodeData>() {
                    new SerializableResourceNodeData() {
                        Location = new HexCoordinates(2, 3), ResourceName = "Resource One",
                        Copies = 5
                    }
                }
            };

            BuildResourceDefinition("Resource One");

            BuildHexCell(new HexCoordinates(2, 3));

            var composer = Container.Resolve<ResourceComposer>();

            composer.DecomposeResources(mapData);

            MockNodeFactory.Verify(
                factory => factory.BuildNode(
                    It.IsAny<IHexCell>(), It.IsAny<ISpecialtyResourceDefinition>(), 5
                ), Times.Once, "NodeFactory.BuildNode was not called with the expected copies argument"
            );
        }

        #endregion

        #region utilities

        private IResourceNode BuildResourceNode() {
            Mock<IResourceNode> mock;
            return BuildResourceNode(out mock);
        }

        private IResourceNode BuildResourceNode(out Mock<IResourceNode> mock) {
            mock = new Mock<IResourceNode>();

            var newNode = mock.Object;

            AllNodes.Add(newNode);

            return mock.Object;
        }

        private IResourceNode BuildResourceNode(ResourceNodeTestData testData) {
            var mockNode = new Mock<IResourceNode>();

            var mockResource = new Mock<ISpecialtyResourceDefinition>();

            mockResource.Setup(resource => resource.name).Returns(testData.ResourceName);

            mockNode.Setup(node => node.Resource).Returns(mockResource.Object);
            mockNode.Setup(node => node.Copies)  .Returns(testData.Copies);

            var newNode = mockNode.Object;

            var location = BuildHexCell(testData.LocationCoordinates);

            MockNodeLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newNode)).Returns(location);

            AllNodes.Add(newNode);

            return newNode;
        }

        private IHexCell BuildHexCell(HexCoordinates coordinates) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Coordinates).Returns(coordinates);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(coordinates)).Returns(newCell);

            return mockCell.Object;
        }

        private ISpecialtyResourceDefinition BuildResourceDefinition(string name) {
            var mockResource = new Mock<ISpecialtyResourceDefinition>();

            mockResource.Setup(definition => definition.name).Returns(name);

            var newResource = mockResource.Object;

            AvailableSpecialtyResources.Add(newResource);

            return newResource;
        }

        #endregion

        #endregion

    }

}
