using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.Barbarians;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianWaitUntilHealedBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IBarbarianConfig> MockBarbarianConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBarbarianConfig = new Mock<IBarbarianConfig>();

            Container.Bind<IBarbarianConfig>().FromInstance(MockBarbarianConfig.Object);

            Container.Bind<IUnitFortificationLogic>().FromInstance(new Mock<IUnitFortificationLogic>().Object);
            Container.Bind<IAbilityExecuter>       ().FromInstance(new Mock<IAbilityExecuter>       ().Object);

            Container.Bind<BarbarianWaitUntilHealedBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetUtilityForUnit_ReturnsZeroIfUnitIsCivilian() {
            var unit = BuildUnit(UnitType.Civilian, 50, 100);

            MockBarbarianConfig.Setup(config => config.WaitUntilHealedMaxUtility).Returns(10f);

            var brain = Container.Resolve<BarbarianWaitUntilHealedBrain>();

            Assert.AreEqual(0f, brain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_ReturnsZeroIfUnitIsWaterMilitary() {
            var unit = BuildUnit(UnitType.NavalMelee, 50, 100);

            MockBarbarianConfig.Setup(config => config.WaitUntilHealedMaxUtility).Returns(10f);

            var brain = Container.Resolve<BarbarianWaitUntilHealedBrain>();

            Assert.AreEqual(0f, brain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_ReturnsZeroIfUnitAtFullHealth() {
            var unit = BuildUnit(UnitType.Melee, 100, 100);

            MockBarbarianConfig.Setup(config => config.WaitUntilHealedMaxUtility).Returns(10f);

            var brain = Container.Resolve<BarbarianWaitUntilHealedBrain>();

            Assert.AreEqual(0f, brain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_ReturnsConfiguredMaxUtilityIfUnitAtZeroHealth() {
            var unit = BuildUnit(UnitType.Melee, 0, 100);

            MockBarbarianConfig.Setup(config => config.WaitUntilHealedMaxUtility).Returns(10f);

            var brain = Container.Resolve<BarbarianWaitUntilHealedBrain>();

            Assert.AreEqual(10f, brain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_ReturnsRatioOfMaxUtilityEqualToRatioOfMissingHealth() {
            var unit = BuildUnit(UnitType.Melee, 45, 100);

            MockBarbarianConfig.Setup(config => config.WaitUntilHealedMaxUtility).Returns(12f);

            var brain = Container.Resolve<BarbarianWaitUntilHealedBrain>();

            Assert.That(Mathf.Approximately(12f * (55f / 100f), brain.GetUtilityForUnit(unit, new InfluenceMaps())));
        }

        [Test]
        public void GetCommandsForUnit_ReturnsASingleFortifyCommand() {
            var unit = BuildUnit(UnitType.Civilian, 100, 100);

            var brain = Container.Resolve<BarbarianWaitUntilHealedBrain>();

            var commands = brain.GetCommandsForUnit(unit, new InfluenceMaps());

            Assert.AreEqual(1, commands.Count, "Unexpected number of commands");

            Assert.IsTrue(commands[0] is FortifyUnitCommand, "Command is of an unexpected type");
        }

        [Test]
        public void GetCommandsForUnit_FortifyCommandGivenCorrectUnitToFortify() {
            var unit = BuildUnit(UnitType.Civilian, 100, 100);

            var brain = Container.Resolve<BarbarianWaitUntilHealedBrain>();

            var fortifyCommand = brain.GetCommandsForUnit(unit, new InfluenceMaps())[0] as FortifyUnitCommand;

            Assert.AreEqual(unit, fortifyCommand.UnitToFortify);
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(UnitType type, int currentHitpoints, int maxHitpoints) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type)            .Returns(type);
            mockUnit.Setup(unit => unit.CurrentHitpoints).Returns(currentHitpoints);
            mockUnit.Setup(unit => unit.MaxHitpoints)    .Returns(maxHitpoints);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
