using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.Tests.Simulation.Units.Abilities {

    public class AnnexAdjacentTerritoryAbilityHandlerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IHexCell>>      MockCellPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<ICivilizationTerritoryLogic>                   MockCivTerritoryLogic;
        private Mock<IHexGrid>                                      MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCellPossessionCanon = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockCivTerritoryLogic   = new Mock<ICivilizationTerritoryLogic>();
            MockGrid                = new Mock<IHexGrid>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>     ().FromInstance(MockCellPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<ICivilizationTerritoryLogic>                  ().FromInstance(MockCivTerritoryLogic  .Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid               .Object);

            Container.Bind<AnnexAdjacentTerritoryAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanHandleCommandOnUnit_TrueIfUnitInDomesticTerritory_OwnerHasCities_AndCommandHasRightType() {
            var unitLocation = BuildCell();

            var domesticCity = BuildCity(BuildCell());

            var domesticCiv = BuildCiv(new List<IHexCell>() { unitLocation }, new List<ICity>() { domesticCity });

            var unit = BuildUnit(unitLocation, domesticCiv);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.AnnexAdjacentTerritory };

            var abilityHandler = Container.Resolve<AnnexAdjacentTerritoryAbilityHandler>();

            Assert.IsTrue(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfCommandHasWrongType() {
            var unitLocation = BuildCell();

            var domesticCity = BuildCity(BuildCell());

            var domesticCiv = BuildCiv(new List<IHexCell>() { unitLocation }, new List<ICity>() { domesticCity });

            var unit = BuildUnit(unitLocation, domesticCiv);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.ClearVegetation };

            var abilityHandler = Container.Resolve<AnnexAdjacentTerritoryAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfUnitNotInItsOwnersTerritory() {
            var unitLocation = BuildCell();

            var domesticCity = BuildCity(BuildCell());

            var domesticCiv = BuildCiv(new List<IHexCell>(), new List<ICity>() { domesticCity });

            var unit = BuildUnit(unitLocation, domesticCiv);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.AnnexAdjacentTerritory };

            var abilityHandler = Container.Resolve<AnnexAdjacentTerritoryAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfUnitOwnerHasNoCities() {
            var unitLocation = BuildCell();

            var domesticCiv = BuildCiv(new List<IHexCell>() { unitLocation }, new List<ICity>());

            var unit = BuildUnit(unitLocation, domesticCiv);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.AnnexAdjacentTerritory };

            var abilityHandler = Container.Resolve<AnnexAdjacentTerritoryAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void HandleCommandOnUnit_AndHandlingValid_AssignsAllCellsAdjacentToUnitToNearestDomesticCity() {
            var unitLocation = BuildCell();

            var adjacentCells = new List<IHexCell>() {
                BuildCell(true), BuildCell(true), BuildCell(true)
            };

            MockGrid.Setup(grid => grid.GetNeighbors(unitLocation)).Returns(adjacentCells);

            var nearestCell = BuildCell();
            var nearCell    = BuildCell();
            var farCell     = BuildCell();

            MockGrid.Setup(grid => grid.GetDistance(unitLocation, nearestCell)).Returns(2);
            MockGrid.Setup(grid => grid.GetDistance(unitLocation, nearCell))   .Returns(3);
            MockGrid.Setup(grid => grid.GetDistance(unitLocation, farCell))    .Returns(4);

            var nearDomesticCity   = BuildCity(nearCell);
            var farDomesticCity    = BuildCity(farCell);
            BuildCity(nearestCell);

            var domesticCiv = BuildCiv(new List<IHexCell>() { unitLocation }, new List<ICity>() { nearDomesticCity, farDomesticCity });

            var unit = BuildUnit(unitLocation, domesticCiv);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.AnnexAdjacentTerritory };

            var abilityHandler = Container.Resolve<AnnexAdjacentTerritoryAbilityHandler>();

            abilityHandler.HandleCommandOnUnit(command, unit);

            MockCellPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(adjacentCells[0], nearDomesticCity),
                Times.Once, "AdjacentCells[0] not assigned to the expected city"
            );

            MockCellPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(adjacentCells[1], nearDomesticCity),
                Times.Once, "AdjacentCells[1] not assigned to the expected city"
            );

            MockCellPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(adjacentCells[2], nearDomesticCity),
                Times.Once, "AdjacentCells[2] not assigned to the expected city"
            );
        }

        [Test]
        public void HandleCommandOnUnit_AndHandlingValid_DoesNotAssignCellsThatCannotBeAssignedTo() {
            var unitLocation = BuildCell();

            var adjacentCells = new List<IHexCell>() {
                BuildCell(false), BuildCell(false), BuildCell(true)
            };

            MockGrid.Setup(grid => grid.GetNeighbors(unitLocation)).Returns(adjacentCells);

            var nearestCell = BuildCell();
            var nearCell    = BuildCell();
            var farCell     = BuildCell();

            MockGrid.Setup(grid => grid.GetDistance(unitLocation, nearestCell)).Returns(2);
            MockGrid.Setup(grid => grid.GetDistance(unitLocation, nearCell))   .Returns(3);
            MockGrid.Setup(grid => grid.GetDistance(unitLocation, farCell))    .Returns(4);

            var nearDomesticCity   = BuildCity(nearCell);
            var farDomesticCity    = BuildCity(farCell);
            BuildCity(nearestCell);

            var domesticCiv = BuildCiv(new List<IHexCell>() { unitLocation }, new List<ICity>() { nearDomesticCity, farDomesticCity });

            var unit = BuildUnit(unitLocation, domesticCiv);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.AnnexAdjacentTerritory };

            var abilityHandler = Container.Resolve<AnnexAdjacentTerritoryAbilityHandler>();

            abilityHandler.HandleCommandOnUnit(command, unit);

            MockCellPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(adjacentCells[0], It.IsAny<ICity>()),
                Times.Never, "AdjacentCells[0] unexpectedly assigned to a city"
            );

            MockCellPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(adjacentCells[1], It.IsAny<ICity>()),
                Times.Never, "AdjacentCells[1] unexpectedly assigned to a city"
            );

            MockCellPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(adjacentCells[2], nearDomesticCity),
                Times.Once, "AdjacentCells[2] not assigned to the expected city"
            );
        }

        [Test]
        public void HandleCommandOnUnit_AndHandlingValid_DoesNotThrow() {
            var unitLocation = BuildCell();

            var domesticCity = BuildCity(BuildCell());

            var domesticCiv = BuildCiv(new List<IHexCell>() { unitLocation }, new List<ICity>() { domesticCity });

            var unit = BuildUnit(unitLocation, domesticCiv);

            MockGrid.Setup(grid => grid.GetNeighbors(unitLocation)).Returns(new List<IHexCell>());

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.AnnexAdjacentTerritory };

            var abilityHandler = Container.Resolve<AnnexAdjacentTerritoryAbilityHandler>();

            Assert.DoesNotThrow(() => abilityHandler.HandleCommandOnUnit(command, unit));
        }

        [Test]
        public void HandleCommandOnUnit_AndHandlingInvalid_ThrowsInvalidOperationException() {
            var unitLocation = BuildCell();

            var domesticCity = BuildCity(BuildCell());

            var domesticCiv = BuildCiv(new List<IHexCell>() { unitLocation }, new List<ICity>() { domesticCity });

            var unit = BuildUnit(unitLocation, domesticCiv);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.BuildImprovement };

            var abilityHandler = Container.Resolve<AnnexAdjacentTerritoryAbilityHandler>();

            Assert.Throws<InvalidOperationException>(() => abilityHandler.HandleCommandOnUnit(command, unit));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IHexCell BuildCell(bool canChangePossession = false) {
            var newCell = new Mock<IHexCell>().Object;

            MockCellPossessionCanon.Setup(
                canon => canon.CanChangeOwnerOfPossession(newCell, It.IsAny<ICity>())
            ).Returns(canChangePossession);

            return newCell;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);

            return newCity;
        }

        private ICivilization BuildCiv(List<IHexCell> territory, List<ICity> cities) {
            var newCiv = new Mock<ICivilization>().Object;

            foreach(var cell in territory) {
                MockCivTerritoryLogic.Setup(logic => logic.GetCivClaimingCell(cell)).Returns(newCiv);
            }

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            return newCiv;
        }

        private IUnit BuildUnit(IHexCell location, ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private IAbilityDefinition BuildAbility(IEnumerable<AbilityCommandRequest> commandRequests) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            return mockAbility.Object;
        }

        #endregion

        #endregion

    }

}
