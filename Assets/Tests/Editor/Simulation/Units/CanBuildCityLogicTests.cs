using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.Tests.Simulation.Units {

    public class CanBuildCityLogicTests : ZenjectUnitTestFixture {

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<CanBuildCityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanUnitBuildCity_TrueIfSomeCommandRequestHasTypeFoundCity() {
            var abilityOne = BuildAbility(
                new List<AbilityCommandRequest>() {
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement },
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.SetUpToBombard },
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity },
                }
            );

            var abilityTwo = BuildAbility(
                new List<AbilityCommandRequest>() {
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement },
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.SetUpToBombard },
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.ClearVegetation },
                }
            );

            var unit = BuildUnit(abilityOne, abilityTwo);

            var buildCityLogic = Container.Resolve<CanBuildCityLogic>();

            Assert.IsTrue(buildCityLogic.CanUnitBuildCity(unit));
        }

        [Test]
        public void CanUnitBuildCity_FalseIfNoCommandRequestHasTypeFoundCity() {
            var abilityOne = BuildAbility(
                new List<AbilityCommandRequest>() {
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement },
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.SetUpToBombard },
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildRoad },
                }
            );

            var abilityTwo = BuildAbility(
                new List<AbilityCommandRequest>() {
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement },
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.SetUpToBombard },
                    new AbilityCommandRequest() { CommandType = AbilityCommandType.ClearVegetation },
                }
            );

            var unit = BuildUnit(abilityOne, abilityTwo);

            var buildCityLogic = Container.Resolve<CanBuildCityLogic>();

            Assert.IsFalse(buildCityLogic.CanUnitBuildCity(unit));
        }

        #endregion

        #region utilities

        private IAbilityDefinition BuildAbility(IEnumerable<AbilityCommandRequest> commandRequests) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            return mockAbility.Object;
        }

        private IUnit BuildUnit(params IAbilityDefinition[] abilities) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Abilities).Returns(abilities);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
