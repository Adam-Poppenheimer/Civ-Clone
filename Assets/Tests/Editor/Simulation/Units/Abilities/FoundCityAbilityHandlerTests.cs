using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.GameMap;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Units.Abilities {

    [TestFixture]
    public class FoundCityAbilityHandlerTests : ZenjectUnitTestFixture {

        #region static fields and properties

        #region test cases

        private static IEnumerable CanHandleAbilityCases {
            get {
                yield return new TestCaseData(
                    "Found City",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity }
                    },
                    true
                ).SetName("Correct command type and valid unit location").Returns(true);

                yield return new TestCaseData(
                    "Chad",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity }
                    },
                    true
                ).SetName("AbilityName isn't relevant").Returns(true);

                yield return new TestCaseData(
                    "Chad",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() {
                            CommandType = AbilityCommandType.FoundCity,
                            ArgsToPass = new List<string>() { "Arg1", "Arg2" }
                        }
                    },
                    true
                ).SetName("Args are ignored").Returns(true);

                yield return new TestCaseData(
                    "Found City",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity }
                    },
                    false
                ).SetName("Correct command type but invalid unit location").Returns(false);

                yield return new TestCaseData(
                    "Found City",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement }
                    },
                    true
                ).SetName("Incorrect command type and valid unit location").Returns(false);

                yield return new TestCaseData(
                    "Found City",
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity },
                        new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity }
                    },
                    true
                ).SetName("Redundant FoundCity commands").Returns(true);
            }
        }

        #endregion

        #endregion

        #region instance fields and properties

        private Mock<ICityValidityLogic> MockCityValidityLogic;

        private Mock<IUnitPositionCanon> MockUnitPositionCanon;

        private Mock<IRecordkeepingCityFactory> MockCityFactory;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityValidityLogic = new Mock<ICityValidityLogic>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockCityFactory       = new Mock<IRecordkeepingCityFactory>();

            Container.Bind<ICityValidityLogic>       ().FromInstance(MockCityValidityLogic.Object);
            Container.Bind<IUnitPositionCanon>       ().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IRecordkeepingCityFactory>().FromInstance(MockCityFactory      .Object);

            Container.Bind<FoundCityAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanHandleAbilityOnUnit should return true if and only if the following conditions are met:\n" +
            "\t1. CommandRequests contains a request whose type if FoundCity\n" +
            "\t2. the tile the activating unit is on is a valid location for a city")]
        [TestCaseSource("CanHandleAbilityCases")]
        public bool CanHandleAbilityOnUnitTests(string abilityName, IEnumerable<AbilityCommandRequest> commandRequests,
            bool unitTileValidForCity
        ){
            var ability = BuildAbility(abilityName, commandRequests);

            var unit = BuildUnit(BuildTile(unitTileValidForCity));

            var abilityHandler = Container.Resolve<FoundCityAbilityHandler>();

            return abilityHandler.CanHandleAbilityOnUnit(ability, unit);
        }

        [Test(Description = "")]
        public void TryHandleAbilityOnUnit() {
            throw new NotImplementedException("Need tests for valid, invalid, and exceptional cases");
        }

        #endregion

        #region utilities

        private IUnitAbilityDefinition BuildAbility(string name, IEnumerable<AbilityCommandRequest> commandRequests) {
            var mockAbility = new Mock<IUnitAbilityDefinition>();

            mockAbility.Setup(ability => ability.name).Returns(name);
            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            return mockAbility.Object;
        }

        private IUnit BuildUnit(IMapTile location) {
            var mockUnit = new Mock<IUnit>();

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(mockUnit.Object)).Returns(location);

            return mockUnit.Object;
        }

        private IMapTile BuildTile(bool validForCity) {
            var mockTile = new Mock<IMapTile>();

            MockCityValidityLogic.Setup(logic => logic.IsTileValidForCity(mockTile.Object)).Returns(validForCity);

            return mockTile.Object;
        }

        #endregion

        #endregion

    }

}
