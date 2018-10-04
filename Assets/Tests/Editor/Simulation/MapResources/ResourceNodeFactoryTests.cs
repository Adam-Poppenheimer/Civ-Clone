using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.MapResources {

    [TestFixture]
    public class ResourceNodeFactoryTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IResourceRestrictionLogic>                        MockRestrictionCanon;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockResourceNodeLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRestrictionCanon          = new Mock<IResourceRestrictionLogic>();
            MockResourceNodeLocationCanon = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();

            Container.Bind<IResourceRestrictionLogic>                       ().FromInstance(MockRestrictionCanon         .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockResourceNodeLocationCanon.Object);

            Container.Bind<ResourceSignals>().AsSingle();

            Container.Bind<ResourceNodeFactory>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanBuildNode_FalseIfCellAlreadyHasANode() {
            var cell = BuildHexCell();
            var existingNode = BuildResourceNode();

            var resource = BuildResourceDefinition();

            MockResourceNodeLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(cell))
                                         .Returns(new IResourceNode[] { existingNode });

            MockRestrictionCanon.Setup(canon => canon.IsResourceValidOnCell(resource, cell))
                                .Returns(true);

            var nodeFactory = Container.Resolve<ResourceNodeFactory>();

            Assert.IsFalse(nodeFactory.CanBuildNode(cell, resource));
        }

        [Test]
        public void CanBuildNode_FalseIfResourceInvalidOnCell() {
            var cell = BuildHexCell();

            var resource = BuildResourceDefinition();

            MockResourceNodeLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(cell))
                                         .Returns(new List<IResourceNode>());

            MockRestrictionCanon.Setup(canon => canon.IsResourceValidOnCell(resource, cell))
                                .Returns(false);

            var nodeFactory = Container.Resolve<ResourceNodeFactory>();

            Assert.IsFalse(nodeFactory.CanBuildNode(cell, resource));
        }

        [Test]
        public void CanBuildNode_TrueIfNoNodeAndResourceValid() {
            var cell = BuildHexCell();

            var resource = BuildResourceDefinition();

            MockResourceNodeLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(cell))
                                         .Returns(new List<IResourceNode>());

            MockRestrictionCanon.Setup(canon => canon.IsResourceValidOnCell(resource, cell))
                                .Returns(true);

            var nodeFactory = Container.Resolve<ResourceNodeFactory>();

            Assert.IsTrue(nodeFactory.CanBuildNode(cell, resource));
        }

        [Test]
        public void CanBuildNode_ThrowsOnNullArguments() {
            var cell = BuildHexCell();

            var resource = BuildResourceDefinition();

            var nodeFactory = Container.Resolve<ResourceNodeFactory>();

            Assert.Throws<ArgumentNullException>(
                () => nodeFactory.CanBuildNode(null, resource),
                "Failed to throw on null location argument"
            );

            Assert.Throws<ArgumentNullException>(
                () => nodeFactory.CanBuildNode(cell, null),
                "Failed to throw on null resource argument"
            );
        }

        [Test]
        public void BuildNode_ThrowsIfCanBuildNodeReturnsFalse() {
            var cell = BuildHexCell();

            var resource = BuildResourceDefinition();

            MockResourceNodeLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(cell))
                                         .Returns(new List<IResourceNode>());

            MockRestrictionCanon.Setup(canon => canon.IsResourceValidOnCell(resource, cell))
                                .Returns(false);

            var nodeFactory = Container.Resolve<ResourceNodeFactory>();
            
            Assert.Throws<InvalidOperationException>(() => nodeFactory.BuildNode(cell, resource, 1));
        }

        [Test]
        public void BuildNode_NewNodeGivenCorrectResource() {
            var cell = BuildHexCell();

            var resource = BuildResourceDefinition();

            MockResourceNodeLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(cell))
                                         .Returns(new List<IResourceNode>());

            MockRestrictionCanon.Setup(canon => canon.IsResourceValidOnCell(resource, cell))
                                .Returns(true);

            var nodeFactory = Container.Resolve<ResourceNodeFactory>();

            var newNode = nodeFactory.BuildNode(cell, resource, 5);

            Assert.AreEqual(resource, newNode.Resource);
        }

        [Test]
        public void BuildNode_NewNodeGivenCorrectCopies() {
            var cell = BuildHexCell();

            var resource = BuildResourceDefinition();

            MockResourceNodeLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(cell))
                                         .Returns(new List<IResourceNode>());

            MockRestrictionCanon.Setup(canon => canon.IsResourceValidOnCell(resource, cell))
                                .Returns(true);

            var nodeFactory = Container.Resolve<ResourceNodeFactory>();

            var newNode = nodeFactory.BuildNode(cell, resource, 5);

            Assert.AreEqual(5, newNode.Copies);
        }

        [Test]
        public void BuildNode_NodeAssignedToLocationCorrectly() {
            var cell = BuildHexCell();

            var resource = BuildResourceDefinition();

            MockResourceNodeLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(cell))
                                         .Returns(new List<IResourceNode>());

            MockRestrictionCanon.Setup(canon => canon.IsResourceValidOnCell(resource, cell))
                                .Returns(true);

            var nodeFactory = Container.Resolve<ResourceNodeFactory>();

            var newNode = nodeFactory.BuildNode(cell, resource, 5);

            MockResourceNodeLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(newNode, cell), Times.Once
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell() {
            return new Mock<IHexCell>().Object;
        }

        private IResourceDefinition BuildResourceDefinition() {
            return new Mock<IResourceDefinition>().Object;
        }

        private IResourceNode BuildResourceNode() {
            return new Mock<IResourceNode>().Object;
        }

        #endregion

        #endregion

    }
}
