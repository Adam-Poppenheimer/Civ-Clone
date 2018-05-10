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
using Assets.Simulation.Units.Promotions;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitFactoryTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>                            MockPositionCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockPossessionCanon;
        private Mock<IHexGrid>                                      MockGrid;

        private GameObject UnitPrefab;
        private Transform  UnitContainer;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockPositionCanon   = new Mock<IUnitPositionCanon>();
            MockPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockGrid            = new Mock<IHexGrid>();

            UnitPrefab = new GameObject();
            UnitPrefab.AddComponent<GameUnit>();

            UnitContainer = new GameObject().transform;

            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockPositionCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockPossessionCanon.Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid           .Object);

            Container.Bind<Transform>().WithId("Unit Container").FromInstance(UnitContainer);

            Container.Bind<IUnitConfig>          ().FromMock();
            Container.Bind<IUnitTerrainCostLogic>().FromMock();

            Container.Bind<UnitSignals>().AsSingle();

            Container.Bind<UnitFactory>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "Whenever a unit is created, its template should be initialized " +
            "with the argued template")]
        public void UnitCreated_TemplateInitialized() {
            var cell         = BuildHexCell(true);
            var template     = BuildTemplate(BuildPromotionTreeData());
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.BuildUnit(cell, template, civilization);

            Assert.AreEqual(template, (newUnit as GameUnit).Template, "newUnit.Template was not initialized correctly");
        }

        [Test(Description = "Whenever a unit is created, its CurrentMovement should be " + 
            "initialized to the MaxMovement of its template")]
        public void UnitCreated_MovementSetToMax() {
            var cell         = BuildHexCell(true);
            var template     = BuildTemplate(BuildPromotionTreeData(), 3);
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.BuildUnit(cell, template, civilization);

            Assert.AreEqual(3, newUnit.CurrentMovement, "newUnit.CurrentMovement has an unexpected value");
        }

        [Test(Description = "Whenever a unit is created, it should be assigned to the " +
            "argued tile via UnitPositionCanon")]
        public void UnitCreated_LocationInitialized() {
            var cell         = BuildHexCell(true);
            var template     = BuildTemplate(BuildPromotionTreeData());
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.BuildUnit(cell, template, civilization);

            MockPositionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(newUnit, cell),
                Times.AtLeastOnce, "UnitFactory did not check UnitPositionCanon for placement validity before " +
                "placing newUnit at its location"
            );

            MockPositionCanon.Verify(canon => canon.ChangeOwnerOfPossession(newUnit, cell),
                Times.Once, "UnitFactory did not initialize newUnit's location properly");
        }

        [Test(Description = "Whenever a unit is created, it should be assigned to the " +
            "argued civilization via UnitPossessionCanon")]
        public void UnitCreated_OwnerInitialized() {
            var cell         = BuildHexCell(true);
            var template     = BuildTemplate(BuildPromotionTreeData());
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.BuildUnit(cell, template, civilization);

            MockPossessionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(newUnit, civilization),
                Times.AtLeastOnce, "UnitFactory did not check UnitPossessionCanon for ownership validity before " +
                "assigning newUnit to its owner"
            );

            MockPossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(newUnit, civilization),
                Times.Once, "UnitFactory did not initialize newUnit's owner properly");
        }

        [Test(Description = "Whenever a unit is created, it should be added to the AllUnits collection")]
        public void UnitCreated_AddedToAllUnits() {
            var cell         = BuildHexCell(true);
            var template     = BuildTemplate(BuildPromotionTreeData());
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.BuildUnit(cell, template, civilization);

            CollectionAssert.Contains(factory.AllUnits, newUnit, "AllUnit does not contain the newly-created unit");
        }

        [Test(Description = "")]
        public void UnitCreated_StartsAtFirstLevel() {
            var cell         = BuildHexCell(true);
            var template     = BuildTemplate(BuildPromotionTreeData());
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.BuildUnit(cell, template, civilization);

            Assert.AreEqual(1, newUnit.Level);
        }

        [Test]
        public void UnitCreated_PromotionTreeInitializedFromPassedValue() {
            var promotionTree = BuildPromotionTree();

            var cell         = BuildHexCell(true);
            var template     = BuildTemplate(BuildPromotionTreeData());
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            var newUnit = factory.BuildUnit(cell, template, civilization, promotionTree);

            Assert.AreEqual(promotionTree, newUnit.PromotionTree);
        }

        [Test(Description = "Create should throw a UnitCreationException when the created unit " +
            "cannot be placed upon the argued tile")]
        public void CreateCalled_ThrowsWhenPositionInvalid() {
            var cell         = BuildHexCell(false);
            var template     = BuildTemplate(BuildPromotionTreeData());
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            Assert.Throws<UnitCreationException>(() => factory.BuildUnit(cell, template, civilization),
                "Create did not throw properly when its created unit could not be placed upon the argued tile");
        }

        [Test(Description = "Create should throw a UnitCreationException when the created unit " +
            "cannot be given to the argued civilization")]
        public void CreateCalled_ThrowsWhenOwnerInvalid() {
            var cell         = BuildHexCell(true);
            var template     = BuildTemplate(BuildPromotionTreeData());
            var civilization = BuildCivilization(false);

            var factory = Container.Resolve<UnitFactory>();

            Assert.Throws<UnitCreationException>(() => factory.BuildUnit(cell, template, civilization),
                "Create did not throw properly when its created unit could not be given to the argued civilization");
        }

        [Test(Description = "Create should throw a ArgumentNullException when passed any null argument")]
        public void CreateCalled_ThrowsOnNullArguments() {
            var cell         = BuildHexCell(true);
            var template     = BuildTemplate(BuildPromotionTreeData());
            var civilization = BuildCivilization(true);

            var factory = Container.Resolve<UnitFactory>();

            Assert.Throws<ArgumentNullException>(() => factory.BuildUnit(null, template, civilization),
                "Create failed to throw on a null location argument");

            Assert.Throws<ArgumentNullException>(() => factory.BuildUnit(cell, null, civilization),
                "Create failed to throw on a null template argument");

            Assert.Throws<ArgumentNullException>(() => factory.BuildUnit(cell, template, null),
                "Create failed to throw on a null owner argument");
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(bool isValidPlaceForUnit) {
            var mockTile = new Mock<IHexCell>();

            mockTile.Setup(tile => tile.transform).Returns(new GameObject().transform);

            MockPositionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IUnit>(), mockTile.Object))
                .Returns(isValidPlaceForUnit);

            return mockTile.Object;
        }

        private IUnitTemplate BuildTemplate(
            IPromotionTreeTemplate promotionTreeData, int maxMovement = 0
        ){
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.MaxMovement)      .Returns(maxMovement);
            mockTemplate.Setup(template => template.Prefab)           .Returns(UnitPrefab);
            mockTemplate.Setup(template => template.PromotionTreeData).Returns(promotionTreeData);

            return mockTemplate.Object;
        }

        private IPromotionTreeTemplate BuildPromotionTreeData() {
            var mockData = new Mock<IPromotionTreeTemplate>();

            return mockData.Object;
        }

        private IPromotionTree BuildPromotionTree() {
            return new Mock<IPromotionTree>().Object;
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
