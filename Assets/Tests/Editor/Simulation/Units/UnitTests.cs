using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable HealthSetTestCases {
            get {
                yield return new TestCaseData(100, 140);
                yield return new TestCaseData(100, 60);
                yield return new TestCaseData(100, -20);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitConfig> MockConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig = new Mock<IUnitConfig>();

            Container.Bind<IUnitConfig>().FromInstance(MockConfig.Object);

            Container.Bind<Unit>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When Health is set, its value is always kept between 0 and " +
            "Config.MaxHealth inclusive")]
        [TestCaseSource("HealthSetTestCases")]
        public void HealthSet_StaysWithinProperBounds(int maxHealth, int newHealthValue) {
            MockConfig.Setup(config => config.MaxHealth).Returns(maxHealth);

            var unitToTest = Container.Resolve<Unit>();
            unitToTest.Health = newHealthValue;

            Assert.AreEqual(
                newHealthValue.Clamp(0, maxHealth),
                unitToTest.Health,
                "UnitToTest.Health has an unexpected value"
            );
        }

        [Test(Description = "")]
        public void OnPointerClick_ClickedSignalFired() {
            throw new NotImplementedException();
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
