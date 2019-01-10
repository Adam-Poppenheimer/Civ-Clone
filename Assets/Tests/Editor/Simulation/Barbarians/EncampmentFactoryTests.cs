using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Barbarians;

namespace Assets.Tests.Simulation.Barbarians {

    public class EncampmentFactoryTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IEncampmentLocationCanon> MockEncampmentLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockEncampmentLocationCanon = new Mock<IEncampmentLocationCanon>();

            Container.Bind<IEncampmentLocationCanon>().FromInstance(MockEncampmentLocationCanon.Object);

            Container.Bind<EncampmentFactory>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanCreateEncampment_TrueIfCellCanAcceptAnEncampment() {
            var cell = BuildCell();

            MockEncampmentLocationCanon.Setup(canon => canon.CanCellAcceptAnEncampment(cell)).Returns(true);

            var factory = Container.Resolve<EncampmentFactory>();

            Assert.IsTrue(factory.CanCreateEncampment(cell));
        }

        [Test]
        public void CanCreateEncampment_FalseIfCellCannotAcceptAnEncampment() {
            var cell = BuildCell();

            MockEncampmentLocationCanon.Setup(canon => canon.CanCellAcceptAnEncampment(cell)).Returns(false);

            var factory = Container.Resolve<EncampmentFactory>();

            Assert.IsFalse(factory.CanCreateEncampment(cell));
        }

        [Test]
        public void CreateEncampment_AndCreationValid_ReturnedEncampmentAssignedToTheCorrectLocation() {
            var cell = BuildCell();

            MockEncampmentLocationCanon.Setup(canon => canon.CanCellAcceptAnEncampment(cell)).Returns(true);

            var factory = Container.Resolve<EncampmentFactory>();

            var newEncampment = factory.CreateEncampment(cell);

            MockEncampmentLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(newEncampment, cell), Times.Once
            );
        }

        [Test]
        public void CreateEncampment_AndCreationValid_ReturnedEncampmentAddedToAllEncampments() {
            var cell = BuildCell();

            MockEncampmentLocationCanon.Setup(canon => canon.CanCellAcceptAnEncampment(cell)).Returns(true);

            var factory = Container.Resolve<EncampmentFactory>();

            var newEncampment = factory.CreateEncampment(cell);

            CollectionAssert.Contains(factory.AllEncampments, newEncampment);
        }

        [Test]
        public void CreateEncampment_AndCreationInvalid_ThrowsInvalidOperationException() {
            var cell = BuildCell();

            MockEncampmentLocationCanon.Setup(canon => canon.CanCellAcceptAnEncampment(cell)).Returns(false);

            var factory = Container.Resolve<EncampmentFactory>();

            Assert.Throws<InvalidOperationException>(() => factory.CreateEncampment(cell));
        }

        [Test]
        public void CreateEncampment_ThrowsOnNullArgument() {
            var factory = Container.Resolve<EncampmentFactory>();

            Assert.Throws<ArgumentNullException>(() => factory.CreateEncampment(null));
        }

        [Test]
        public void DestroyEncampment_EncampmentRemovedFromEncampmentsList() {
            var cell = BuildCell();

            MockEncampmentLocationCanon.Setup(canon => canon.CanCellAcceptAnEncampment(cell)).Returns(true);

            var factory = Container.Resolve<EncampmentFactory>();

            var newEncampment = factory.CreateEncampment(cell);

            factory.DestroyEncampment(newEncampment);

            CollectionAssert.DoesNotContain(factory.AllEncampments, newEncampment);
        }

        [Test]
        public void DestroyEncampment_EncampmentRemovedFromLocation() {
            var cell = BuildCell();

            MockEncampmentLocationCanon.Setup(canon => canon.CanCellAcceptAnEncampment(cell)).Returns(true);

            var factory = Container.Resolve<EncampmentFactory>();

            var newEncampment = factory.CreateEncampment(cell);

            MockEncampmentLocationCanon.ResetCalls();

            factory.DestroyEncampment(newEncampment);

            MockEncampmentLocationCanon.Verify(canon => canon.ChangeOwnerOfPossession(newEncampment, null), Times.Once);
        }

        [Test]
        public void DestroyEncampment_ThrowsOnNullArgument() {
            var factory = Container.Resolve<EncampmentFactory>();

            Assert.Throws<ArgumentNullException>(() => factory.DestroyEncampment(null));
        }


        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
