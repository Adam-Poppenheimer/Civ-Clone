﻿using System;
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

        #region static fields and properties

        #region test cases

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(
                    "Found City",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity }
                    },
                    true
                ).SetName("True on correct command type and valid unit location").Returns(true);

                yield return new TestCaseData(
                    "Chad",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity }
                    },
                    true
                ).SetName("True regardless of AbilityName").Returns(true);

                yield return new TestCaseData(
                    "Chad",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() {
                            CommandType = AbilityCommandType.FoundCity,
                            ArgsToPass = new List<string>() { "Arg1", "Arg2" }
                        }
                    },
                    true
                ).SetName("True regardless of Args").Returns(true);

                yield return new TestCaseData(
                    "Found City",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity }
                    },
                    false
                ).SetName("False on correct command type but invalid unit location").Returns(false);

                yield return new TestCaseData(
                    "Found City",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement }
                    },
                    true
                ).SetName("false on incorrect command type and valid unit location").Returns(false);

                yield return new TestCaseData(
                    "Found City",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity },
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity }
                    },
                    true
                ).SetName("True on redundant FoundCity commands").Returns(true);
            }
        }

        #endregion

        #endregion

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

        [Test(Description = "CanHandleAbilityOnUnit should return true if and only if the following conditions are met:\n" +
            "\t1. CommandRequests contains a request whose type if FoundCity\n" +
            "\t2. the tile the activating unit is on is a valid location for a city")]
        [TestCaseSource("TestCases")]
        public bool CanHandleAbilityOnUnitTests(
            string abilityName, IEnumerable<AbilityCommandRequest> commandRequests, bool unitTileValidForCity
        ){
            var ability = BuildAbility(abilityName, commandRequests);

            var unit = BuildUnit(BuildHexCell(unitTileValidForCity), BuildCivilization("City One"));

            var abilityHandler = Container.Resolve<FoundCityAbilityHandler>();

            return abilityHandler.CanHandleAbilityOnUnit(ability, unit);
        }

        [Test(Description = "TryHandleAbilityOnUnitTests should return the same value that CanHandleAbilityOnUnit does, " +
            "but should also create a city on the location and with the owner of the argued unit")]
        [TestCaseSource("TestCases")]
        public bool TryHandleAbilityOnUnit_CityCreatedWhenValid(
            string abilityName, IEnumerable<AbilityCommandRequest> commandRequests, bool unitTileValidForCity
        ){
            var ability = BuildAbility(abilityName, commandRequests);

            var unit = BuildUnit(BuildHexCell(unitTileValidForCity), BuildCivilization("City One"));

            var abilityHandler = Container.Resolve<FoundCityAbilityHandler>();

            var handleResponse = abilityHandler.TryHandleAbilityOnUnit(ability, unit);

            if(handleResponse.AbilityHandled) {
                MockCityFactory.Verify(
                    factory => factory.Create(
                        MockUnitPositionCanon .Object.GetOwnerOfPossession(unit),
                        MockUnitOwnershipCanon.Object.GetOwnerOfPossession(unit),
                        "City One"
                    ),
                    Times.Once,
                    "CityFactory.Create was not called as expected"
                );
            }else {
                MockCityFactory.Verify(
                    factory => factory.Create(
                        It.IsAny<IHexCell>(), It.IsAny<ICivilization>(), It.IsAny<string>()
                    ),
                    Times.Never, "CityFactory.Create was unexpectedly called"
                );
            }

            return handleResponse.AbilityHandled;
        }

        #endregion

        #region utilities

        private IAbilityDefinition BuildAbility(string name, IEnumerable<AbilityCommandRequest> commandRequests) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.name).Returns(name);
            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            return mockAbility.Object;
        }

        private IUnit BuildUnit(IHexCell location, ICivilization owner) {
            var mockUnit = new Mock<IUnit>();

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(mockUnit.Object)).Returns(location);

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
