using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Units.Abilities {

    [TestFixture]
    public class FoundCityAbilityHandlerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICityValidityLogic>                            MockCityValidityLogic;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<ICityFactory>                                  MockCityFactory;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitOwnershipCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityValidityLogic   = new Mock<ICityValidityLogic>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockCityFactory         = new Mock<ICityFactory>();
            MockUnitOwnershipCanon  = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(It.IsAny<ICivilization>()))
                                   .Returns(AllCities);

            Container.Bind<ICityValidityLogic>                           ().FromInstance(MockCityValidityLogic  .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<ICityFactory>                                 ().FromInstance(MockCityFactory        .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitOwnershipCanon .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);

            Container.Bind<FoundCityAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanHandleCommandOnUnit_ReturnsTrueIfUnitLocationIsValidCellForCity() {
            var unitLocation = BuildHexCell(true);

            var owner = BuildCivilization("");

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.FoundCity };

            var unit = BuildUnit(unitLocation, owner);

            var handler = Container.Resolve<FoundCityAbilityHandler>();

            Assert.IsTrue(handler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfCommandTypeNotFoundCity() {
            var unitLocation = BuildHexCell(true);

            var owner = BuildCivilization("");

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.RepairAdjacentShips };

            var unit = BuildUnit(unitLocation, owner);

            var handler = Container.Resolve<FoundCityAbilityHandler>();

            Assert.IsFalse(handler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfUnitLocationNotValidForCity() {
            var unitLocation = BuildHexCell(false);

            var owner = BuildCivilization("");

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.FoundCity };

            var unit = BuildUnit(unitLocation, owner);

            var handler = Container.Resolve<FoundCityAbilityHandler>();

            Assert.IsFalse(handler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void HandleCommandOnUnit_AndCommandCanBeHandled_CityCreatedAtUnitLocation_WithCorrectNextName() {
            var unitLocation = BuildHexCell(true);

            var owner = BuildCivilization("Washington");

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.FoundCity };

            var unit = BuildUnit(unitLocation, owner);

            var handler = Container.Resolve<FoundCityAbilityHandler>();

            handler.HandleCommandOnUnit(command, unit);

            MockCityFactory.Verify(factory => factory.Create(unitLocation, owner, "Washington"), Times.Once);
        }

        [Test]
        public void HandleCommandOnUnit_AndCommandCannotBeHandled_ThrowsInvalidOperationException() {
            var unitLocation = BuildHexCell(true);

            var owner = BuildCivilization("Washington");

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.BuildRoad };

            var unit = BuildUnit(unitLocation, owner);

            var handler = Container.Resolve<FoundCityAbilityHandler>();

            Assert.Throws<InvalidOperationException>(() => handler.HandleCommandOnUnit(command, unit));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(IHexCell location, ICivilization owner) {
            var mockUnit = new Mock<IUnit>();

            MockUnitPositionCanon .Setup(canon => canon.GetOwnerOfPossession(mockUnit.Object)).Returns(location);
            MockUnitOwnershipCanon.Setup(canon => canon.GetOwnerOfPossession(mockUnit.Object)).Returns(owner);

            return mockUnit.Object;
        }

        private IHexCell BuildHexCell(bool validForCity) {
            var mockCell = new Mock<IHexCell>();

            MockCityValidityLogic.Setup(logic => logic.IsCellValidForCity(mockCell.Object)).Returns(validForCity);

            return mockCell.Object;
        }

        private ICivilization BuildCivilization(string cityName) {
            var mockCiv = new Mock<ICivilization>();

            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.GetNextName(It.IsAny<IEnumerable<ICity>>())).Returns(cityName);

            mockCiv.Setup(civ => civ.Template).Returns(mockTemplate.Object);

            return mockCiv.Object;
        }

        #endregion

        #endregion

    }

}
