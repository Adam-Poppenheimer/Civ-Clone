using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Improvements;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Abilities {

    public class PillageAbilityHandlerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IImprovementLocationCanon>                     MockImprovementLocationCanon;
        private Mock<ICivilizationTerritoryLogic>                   MockCivTerritoryLogic;
        private Mock<IWarCanon>                                     MockWarCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon        = new Mock<IUnitPositionCanon>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockCivTerritoryLogic        = new Mock<ICivilizationTerritoryLogic>();
            MockWarCanon                 = new Mock<IWarCanon>();
            MockUnitPossessionCanon      = new Mock<IPossessionRelationship<ICivilization, IUnit>>();

            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon       .Object);
            Container.Bind<IImprovementLocationCanon>                    ().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<ICivilizationTerritoryLogic>                  ().FromInstance(MockCivTerritoryLogic       .Object);
            Container.Bind<IWarCanon>                                    ().FromInstance(MockWarCanon                .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon     .Object);

            Container.Bind<PillageAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanHandleAbilityOnUnit_FalseIfAbilityDoesntHavePillagingRequest() {
            var domesticCiv = BuildCiv();

            var location = BuildCell(true, null, new List<IImprovement>());

            var unit = BuildUnit(location, domesticCiv, 0);

            var ability = BuildAbility(AbilityCommandType.Fortify);

            var handler = Container.Resolve<PillageAbilityHandler>();

            Assert.IsFalse(handler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_FalseIfNoImprovementsOrRoadsAtLocation() {
            var domesticCiv = BuildCiv();

            var location = BuildCell(false, null, new List<IImprovement>());

            var unit = BuildUnit(location, domesticCiv, 0);

            var ability = BuildAbility(AbilityCommandType.Pillage);

            var handler = Container.Resolve<PillageAbilityHandler>();

            Assert.IsFalse(handler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_TrueIfImprovementsAndNoCellOwner() {
            var domesticCiv = BuildCiv();

            var location = BuildCell(
                false, null, new List<IImprovement>() { BuildImprovement() }
            );

            var unit = BuildUnit(location, domesticCiv, 0);

            var ability = BuildAbility(AbilityCommandType.Pillage);

            var handler = Container.Resolve<PillageAbilityHandler>();

            Assert.IsTrue(handler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_TrueIfRoadsAndNoCellOwner() {
            var domesticCiv = BuildCiv();

            var location = BuildCell(true, null, new List<IImprovement>());

            var unit = BuildUnit(location, domesticCiv, 0);

            var ability = BuildAbility(AbilityCommandType.Pillage);

            var handler = Container.Resolve<PillageAbilityHandler>();

            Assert.IsTrue(handler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_FalseIfCellIsOwnedAndAtPeaceWithOwner() {
            var domesticCiv = BuildCiv();
            var foreignCiv  = BuildCiv();

            var location = BuildCell(true, foreignCiv, new List<IImprovement>());

            var unit = BuildUnit(location, domesticCiv, 0);

            var ability = BuildAbility(AbilityCommandType.Pillage);

            var handler = Container.Resolve<PillageAbilityHandler>();

            Assert.IsFalse(handler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_TrueIfCellIsOwnedAndAtWarWithOwner() {
            var domesticCiv = BuildCiv();
            var foreignCiv  = BuildCiv();

            MockWarCanon.Setup(canon => canon.AreAtWar(domesticCiv, foreignCiv)) .Returns(true);
            MockWarCanon.Setup(canon => canon.AreAtWar(foreignCiv,  domesticCiv)).Returns(true);

            var location = BuildCell(true, foreignCiv, new List<IImprovement>());

            var unit = BuildUnit(location, domesticCiv, 0);

            var ability = BuildAbility(AbilityCommandType.Pillage);

            var handler = Container.Resolve<PillageAbilityHandler>();

            Assert.IsTrue(handler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void TryHandleAbilityOnUnit_PillagesOneImprovementIfOneExists() {
            var domesticCiv = BuildCiv();

            Mock<IImprovement> mockImprovementOne, mockImprovementTwo;
            var location = BuildCell(
                true, null, new List<IImprovement>() {
                    BuildImprovement(out mockImprovementOne), BuildImprovement(out mockImprovementTwo)
                }
            );

            var unit = BuildUnit(location, domesticCiv, 0);

            var ability = BuildAbility(AbilityCommandType.Pillage);

            var handler = Container.Resolve<PillageAbilityHandler>();

            handler.TryHandleAbilityOnUnit(ability, unit);

            mockImprovementOne.Verify(improvement => improvement.Pillage(), Times.Once,  "ImprovementOne not pillaged");
            mockImprovementTwo.Verify(improvement => improvement.Pillage(), Times.Never, "ImprovementTwo unexpectedly pillaged");

            Assert.IsTrue(location.HasRoads, "Location's roads unexpectedly removed");
        }

        [Test]
        public void TryHandleAbilityOnUnit_RemovesRoadsIfNoImprovementExists() {
            var domesticCiv = BuildCiv();

            var location = BuildCell(true, null, new List<IImprovement>());

            var unit = BuildUnit(location, domesticCiv, 0);

            var ability = BuildAbility(AbilityCommandType.Pillage);

            var handler = Container.Resolve<PillageAbilityHandler>();

            handler.TryHandleAbilityOnUnit(ability, unit);

            Assert.IsFalse(location.HasRoads);
        }

        [Test]
        public void TryHandleAbilityOnUnit_MovementDecreasedByOne() {
            var domesticCiv = BuildCiv();

            var location = BuildCell(true, null, new List<IImprovement>());

            var unit = BuildUnit(location, domesticCiv, 2);

            var ability = BuildAbility(AbilityCommandType.Pillage);

            var handler = Container.Resolve<PillageAbilityHandler>();

            handler.TryHandleAbilityOnUnit(ability, unit);

            Assert.AreEqual(1, unit.CurrentMovement);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(bool hasRoads, ICivilization owner, IEnumerable<IImprovement> improvements) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            var newCell = mockCell.Object;

            newCell.HasRoads = hasRoads;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(improvements);

            MockCivTerritoryLogic.Setup(logic => logic.GetCivClaimingCell(newCell)).Returns(owner);

            return newCell;
        }

        private IImprovement BuildImprovement() {
            Mock<IImprovement> mock;
            return BuildImprovement(out mock);
        }

        private IImprovement BuildImprovement(out Mock<IImprovement> mock) {
            mock = new Mock<IImprovement>();

            return mock.Object;
        }

        private IUnit BuildUnit(IHexCell location, ICivilization owner, int currentMovement) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            var newUnit = mockUnit.Object;

            newUnit.CurrentMovement = currentMovement;

            MockUnitPositionCanon  .Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);
            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);
            
            return newUnit;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IAbilityDefinition BuildAbility(AbilityCommandType commandType) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.CommandRequests).Returns(
                new List<AbilityCommandRequest>() {
                    new AbilityCommandRequest() { CommandType = commandType }
                }
            );

            return mockAbility.Object;
        }

        #endregion

        #endregion

    }

}
