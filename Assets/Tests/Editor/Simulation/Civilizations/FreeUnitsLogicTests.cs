using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Civilizations {

    public class FreeUnitsLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationConfig> MockCivConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCivConfig = new Mock<ICivilizationConfig>();

            Container.Bind<ICivilizationConfig>().FromInstance(MockCivConfig.Object);

            Container.Bind<FreeUnitsLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetMaintenanceFreeUnitsForCiv_ReturnsConfiguredMaintenanceFreeUnits() {
            var civ = BuildCiv();

            MockCivConfig.Setup(config => config.MaintenanceFreeUnits).Returns(12);

            var freeUnitsLogic = Container.Resolve<FreeUnitsLogic>();

            Assert.AreEqual(12, freeUnitsLogic.GetMaintenanceFreeUnitsForCiv(civ));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
