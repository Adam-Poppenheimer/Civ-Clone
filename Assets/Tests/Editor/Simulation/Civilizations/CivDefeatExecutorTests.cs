using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Moq;
using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Civilizations {

    public class CivDefeatExecutorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationConfig>                           MockCivConfig;
        private Mock<ICanBuildCityLogic>                            MockCanBuildCityLogic;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;

        private CivilizationSignals                                 CivSignals;


        private List<ICity> AllCities = new List<ICity>();
        private List<IUnit> AllUnits  = new List<IUnit>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();
            AllUnits .Clear();

            MockCivConfig           = new Mock<ICivilizationConfig>();
            MockCanBuildCityLogic   = new Mock<ICanBuildCityLogic>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();

            CivSignals              = new CivilizationSignals();

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(It.IsAny<ICivilization>()))
                                   .Returns(AllCities);

            MockUnitPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(It.IsAny<ICivilization>()))
                                   .Returns(AllUnits);

            Container.Bind<ICivilizationConfig>                          ().FromInstance(MockCivConfig          .Object);
            Container.Bind<ICanBuildCityLogic>                           ().FromInstance(MockCanBuildCityLogic  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);

            Container.Bind<CivilizationSignals>                          ().FromInstance(CivSignals);

            Container.Bind<CivDefeatExecutor>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void PerformDefeatOfCiv_CivDefeatSignalFired() {
            var civToDefeat = BuildCiv();

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();

            CivSignals.CivDefeatedSignal.Subscribe(delegate(ICivilization civ) {
                Assert.AreEqual(civToDefeat, civ, "Incorrect civ passed");
                Assert.Pass();
            });

            defeatExecutor.PerformDefeatOfCiv(civToDefeat);

            Assert.Fail("CivDefeatedSignal never fired");
        }

        [Test]
        public void PerformDefeatOfCiv_CivDestroyed() {
            Mock<ICivilization> mockCiv;

            var civToDefeat = BuildCiv(out mockCiv);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();

            defeatExecutor.PerformDefeatOfCiv(civToDefeat);

            mockCiv.Verify(civ => civ.Destroy(), Times.Once);
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCities_FalseIfHasAnyCities() {
            var civToCheck = BuildCiv();

            BuildCity(civToCheck);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            Assert.IsFalse(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCities_TrueIfHasNoCities() {
            var civToCheck = BuildCiv();

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            Assert.IsTrue(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCities_TrueIfHasOnlyUnits() {
            var civToCheck = BuildCiv();

            BuildUnit(civToCheck);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            Assert.IsTrue(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCitiesOrUnits_FalseIfHasAnyCities() {
            var civToCheck = BuildCiv();

            BuildCity(civToCheck);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            Assert.IsFalse(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCitiesOrUnits_FalseIfHasOnlyUnits() {
            var civToCheck = BuildCiv();

            BuildUnit(civToCheck);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            Assert.IsFalse(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCitiesOrUnits_TrueIfHasNoCitiesAndNoUnits() {
            var civToCheck = BuildCiv();

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            Assert.IsTrue(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void OnLostUnit_CorrespondingCivDefeatedIfConditionsValid() {
            var civToCheck = BuildCiv();

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            var unitBeingLost = BuildUnit(civToCheck);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            AllUnits.Remove(unitBeingLost);

            CivSignals.CivDefeatedSignal.Subscribe(civ => Assert.Pass());

            CivSignals.CivLostUnitSignal.OnNext(new Tuple<ICivilization, IUnit>(civToCheck, unitBeingLost));

            Assert.Fail("Civ not defeated as expected");
        }

        [Test]
        public void OnLostUnit_CorrespondingCivNotDefeatedIfConditionsInvalid() {
            var civToCheck = BuildCiv();

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            BuildUnit(civToCheck);
            var unitBeingLost = BuildUnit(civToCheck);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            AllUnits.Remove(unitBeingLost);

            CivSignals.CivDefeatedSignal.Subscribe(civ => Assert.Fail("Civ unexpectedly defeated"));

            CivSignals.CivLostUnitSignal.OnNext(new Tuple<ICivilization, IUnit>(civToCheck, unitBeingLost));
        }

        [Test]
        public void OnLostCity_CorrespondingCivDefeatedIfConditionsValid() {
            var civToCheck = BuildCiv();

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var cityBeingLost = BuildCity(civToCheck);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            AllCities.Remove(cityBeingLost);

            CivSignals.CivDefeatedSignal.Subscribe(civ => Assert.Pass());

            CivSignals.CivLostCitySignal.OnNext(new Tuple<ICivilization, ICity>(civToCheck, cityBeingLost));

            Assert.Fail("Civ not defeated as expected");
        }

        [Test]
        public void OnLostCity_CorrespondingCivNotDefeatedIfConditionsInvalid() {
            var civToCheck = BuildCiv();

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            BuildCity(civToCheck);
            var cityBeingLost = BuildCity(civToCheck);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = true;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            AllCities.Remove(cityBeingLost);

            CivSignals.CivDefeatedSignal.Subscribe(civ => Assert.Fail("Civ unexpectedly defeated"));

            CivSignals.CivLostCitySignal.OnNext(new Tuple<ICivilization, ICity>(civToCheck, cityBeingLost));
        }

        [Test]
        public void ShouldCivBeDefeated_AndCheckForDefeatFalse_ReturnsFalseEvenIfDefeatConditionsOtherwiseValid() {
            var civToCheck = BuildCiv();

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.CheckForDefeat = false;

            CivSignals.NewCivilizationCreatedSignal.OnNext(civToCheck);

            Assert.IsFalse(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            Mock<ICivilization> mockCiv;

            return BuildCiv(out mockCiv);
        }

        private ICivilization BuildCiv(out Mock<ICivilization> mockCiv) {
            mockCiv = new Mock<ICivilization>();

            return mockCiv.Object;
        }

        private ICity BuildCity(ICivilization owner) {
            var newCity = new Mock<ICity>().Object;

            AllCities.Add(newCity);

            return newCity;
        }

        private IUnit BuildUnit(ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            AllUnits.Add(newUnit);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
