using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitFactoryTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockPositionCanon;

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockPossessionCanon;

        private GameObject UnitPrefab; 

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockPositionCanon   = new Mock<IUnitPositionCanon>();
            MockPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();

            UnitPrefab = new GameObject();
            UnitPrefab.AddComponent<GameUnit>();

            Container.Bind<GameObject>().WithId("Unit Prefab").FromInstance(UnitPrefab);

            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockPositionCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockPossessionCanon.Object);

            Container.Bind<IUnitConfig>().FromMock();
            Container.Bind<IUnitTerrainCostLogic>().FromMock();

            Container.Bind<UnitSignals>().AsSingle();

            Container.Bind<UnitFactory>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "Whenever a unit is created, its transform should have its parent " +
            "set to the transform of the unit's location")]
        public void UnitCreated_TransformParentSet() {
            var tile = BuildTile(true);
            var template = BuildTemplate();
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.Create(tile, template, civilization);

            Assert.AreEqual(tile.transform, newUnit.gameObject.transform.parent,
                "newUnit's transform has an unexpected parent");
        }

        [Test(Description = "Whenever a unit is created, its template should be initialized " +
            "with the argued template")]
        public void UnitCreated_TemplateInitialized() {
            var tile = BuildTile(true);
            var template = BuildTemplate();
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.Create(tile, template, civilization);

            Assert.AreEqual(template, newUnit.Template, "newUnit.Template was not initialized correctly");
        }

        [Test(Description = "Whenever a unit is created, its CurrentMovement should be " + 
            "initialized to the MaxMovement of its template")]
        public void UnitCreated_MovementSetToMax() {
            var tile = BuildTile(true);
            var template = BuildTemplate(3);
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.Create(tile, template, civilization);

            Assert.AreEqual(3, newUnit.CurrentMovement, "newUnit.CurrentMovement has an unexpected value");
        }

        [Test(Description = "Whenever a unit is created, it should be assigned to the " +
            "argued tile via UnitPositionCanon")]
        public void UnitCreated_LocationInitialized() {
            var tile = BuildTile(true);
            var template = BuildTemplate();
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.Create(tile, template, civilization);

            MockPositionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(newUnit, tile),
                Times.AtLeastOnce, "UnitFactory did not check UnitPositionCanon for placement validity before " +
                "placing newUnit at its location"
            );

            MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(newUnit, tile),
                Times.Once, "UnitFactory did not initialize newUnit's location properly");
        }

        [Test(Description = "Whenever a unit is created, it should be assigned to the " +
            "argued civilization via UnitPossessionCanon")]
        public void UnitCreated_OwnerInitialized() {
            var tile = BuildTile(true);
            var template = BuildTemplate();
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.Create(tile, template, civilization);

            MockPossessionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(newUnit, civilization),
                Times.AtLeastOnce, "UnitFactory did not check UnitPossessionCanon for ownership validity before " +
                "assigning newUnit to its owner"
            );

            MockPossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(newUnit, civilization),
                Times.Once, "UnitFactory did not initialize newUnit's owner properly");
        }

        [Test(Description = "Whenever a unit is created, it should be added to the AllUnits collection")]
        public void UnitCreated_AddedToAllUnits() {
            var tile = BuildTile(true);
            var template = BuildTemplate();
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.Create(tile, template, civilization);

            CollectionAssert.Contains(factory.AllUnits, newUnit, "AllUnit does not contain the newly-created unit");
        }

        [Test(Description = "Create should throw a UnitCreationException when the created unit " +
            "cannot be placed upon the argued tile")]
        public void CreateCalled_ThrowsWhenPositionInvalid() {
            var tile = BuildTile(false);
            var template = BuildTemplate();
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            Assert.Throws<UnitCreationException>(() => factory.Create(tile, template, civilization),
                "Create did not throw properly when its created unit could not be placed upon the argued tile");
        }

        [Test(Description = "Create should throw a UnitCreationException when the created unit " +
            "cannot be given to the argued civilization")]
        public void CreateCalled_ThrowsWhenOwnerInvalid() {
            var tile = BuildTile(true);
            var template = BuildTemplate();
            var civilization = BuildCivilization(false);

            var factory = Container.Resolve<UnitFactory>();

            Assert.Throws<UnitCreationException>(() => factory.Create(tile, template, civilization),
                "Create did not throw properly when its created unit could not be given to the argued civilization");
        }

        [Test(Description = "Create should throw a ArgumentNullException when passed any null argument")]
        public void CreateCalled_ThrowsOnNullArguments() {
            var tile = BuildTile(true);
            var template = BuildTemplate();
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            Assert.Throws<ArgumentNullException>(() => factory.Create(null, template, civilization),
                "Create failed to throw on a null location argument");

            Assert.Throws<ArgumentNullException>(() => factory.Create(tile, null, civilization),
                "Create failed to throw on a null template argument");

            Assert.Throws<ArgumentNullException>(() => factory.Create(tile, template, null),
                "Create failed to throw on a null owner argument");
        }

        #endregion

        #region utilities

        private IHexCell BuildTile(bool isValidPlaceForUnit) {
            var mockTile = new Mock<IHexCell>();

            mockTile.Setup(tile => tile.transform).Returns(new GameObject().transform);

            MockPositionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IUnit>(), mockTile.Object))
                .Returns(isValidPlaceForUnit);

            return mockTile.Object;
        }

        private IUnitTemplate BuildTemplate(int maxMovement = 0) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.MaxMovement).Returns(maxMovement);

            return mockTemplate.Object;
        }

        private ICivilization BuildCivilization(bool isValidOwnerOfUnit) {
            var mockCivilization = new Mock<ICivilization>();

            MockPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IUnit>(), mockCivilization.Object))
                .Returns(isValidOwnerOfUnit);

            return mockCivilization.Object;
        }

        #endregion

        #endregion

    }

}
