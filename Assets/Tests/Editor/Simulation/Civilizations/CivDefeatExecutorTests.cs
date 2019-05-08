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
            var civToDefeat = BuildCiv(false);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();

            CivSignals.CivDefeated.Subscribe(delegate(ICivilization civ) {
                Assert.AreEqual(civToDefeat, civ, "Incorrect civ passed");
                Assert.Pass();
            });

            defeatExecutor.PerformDefeatOfCiv(civToDefeat);

            Assert.Fail("CivDefeatedSignal never fired");
        }

        [Test]
        public void PerformDefeatOfCiv_CivDestroyed() {
            Mock<ICivilization> mockCiv;

            var civToDefeat = BuildCiv(false, out mockCiv);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();

            defeatExecutor.PerformDefeatOfCiv(civToDefeat);

            mockCiv.Verify(civ => civ.Destroy(), Times.Once);
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCities_FalseIfHasAnyCities() {
            var civToCheck = BuildCiv(false);

            BuildCity(civToCheck);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            Assert.IsFalse(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCities_TrueIfHasNoCities() {
            var civToCheck = BuildCiv(false);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            Assert.IsTrue(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCities_TrueIfHasOnlyUnits() {
            var civToCheck = BuildCiv(false);

            BuildUnit(civToCheck);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            Assert.IsTrue(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCitiesOrUnits_FalseIfHasAnyCities() {
            var civToCheck = BuildCiv(false);

            BuildCity(civToCheck);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            Assert.IsFalse(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCitiesOrUnits_FalseIfHasOnlyUnits() {
            var civToCheck = BuildCiv(false);

            BuildUnit(civToCheck);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            Assert.IsFalse(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void ShouldCivBeDefeated_AndDefeatModeNoMoreCitiesOrUnits_TrueIfHasNoCitiesAndNoUnits() {
            var civToCheck = BuildCiv(false);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            Assert.IsTrue(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void OnLostUnit_CorrespondingCivDefeatedIfConditionsValid() {
            var civToCheck = BuildCiv(false);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            var unitBeingLost = BuildUnit(civToCheck);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            AllUnits.Remove(unitBeingLost);

            CivSignals.CivDefeated.Subscribe(civ => Assert.Pass());

            CivSignals.CivLostUnit.OnNext(new UniRx.Tuple<ICivilization, IUnit>(civToCheck, unitBeingLost));

            Assert.Fail("Civ not defeated as expected");
        }

        [Test]
        public void OnLostUnit_CorrespondingCivNotDefeatedIfConditionsInvalid() {
            var civToCheck = BuildCiv(false);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCitiesOrUnits);

            BuildUnit(civToCheck);
            var unitBeingLost = BuildUnit(civToCheck);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            AllUnits.Remove(unitBeingLost);

            CivSignals.CivDefeated.Subscribe(civ => Assert.Fail("Civ unexpectedly defeated"));

            CivSignals.CivLostUnit.OnNext(new UniRx.Tuple<ICivilization, IUnit>(civToCheck, unitBeingLost));
        }

        [Test]
        public void OnLostCity_CorrespondingCivDefeatedIfConditionsValid() {
            var civToCheck = BuildCiv(false);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var cityBeingLost = BuildCity(civToCheck);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            AllCities.Remove(cityBeingLost);

            CivSignals.CivDefeated.Subscribe(civ => Assert.Pass());

            CivSignals.CivLostCity.OnNext(new UniRx.Tuple<ICivilization, ICity>(civToCheck, cityBeingLost));

            Assert.Fail("Civ not defeated as expected");
        }

        [Test]
        public void OnLostCity_CorrespondingCivNotDefeatedIfConditionsInvalid() {
            var civToCheck = BuildCiv(false);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            BuildCity(civToCheck);
            var cityBeingLost = BuildCity(civToCheck);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            AllCities.Remove(cityBeingLost);

            CivSignals.CivDefeated.Subscribe(civ => Assert.Fail("Civ unexpectedly defeated"));

            CivSignals.CivLostCity.OnNext(new UniRx.Tuple<ICivilization, ICity>(civToCheck, cityBeingLost));
        }

        [Test]
        public void ShouldCivBeDefeated_AndIsActiveFalse_ReturnsFalseEvenIfDefeatConditionsOtherwiseValid() {
            var civToCheck = BuildCiv(false);

            MockCivConfig.Setup(config => config.DefeatMode).Returns(CivilizationDefeatMode.NoMoreCities);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = false;

            CivSignals.NewCivilizationCreated.OnNext(civToCheck);

            Assert.IsFalse(defeatExecutor.ShouldCivBeDefeated(civToCheck));
        }

        [Test]
        public void IsCheckingCiv_FalseByDefault() {
            var civ = BuildCiv(false);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            Assert.IsFalse(defeatExecutor.IsCheckingCiv(civ));
        }

        [Test]
        public void IsCheckingCiv_TrueIfCivCreationBroadcast() {
            var civ = BuildCiv(false);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civ);

            Assert.IsTrue(defeatExecutor.IsCheckingCiv(civ));
        }

        [Test]
        public void IsCheckingCiv_FalseIfCivIsBarbaric_EvenAfterCivCreationBroadcast() {
            var civ = BuildCiv(true);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civ);

            Assert.IsFalse(defeatExecutor.IsCheckingCiv(civ));
        }

        [Test]
        public void IsCheckingCiv_FalseAfterCivIsDefeated() {
            var civ = BuildCiv(false);

            var defeatExecutor = Container.Resolve<CivDefeatExecutor>();
            defeatExecutor.IsActive = true;

            CivSignals.NewCivilizationCreated.OnNext(civ);

            defeatExecutor.PerformDefeatOfCiv(civ);

            Assert.IsFalse(defeatExecutor.IsCheckingCiv(civ));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv(bool isBarbaric) {
            Mock<ICivilization> mockCiv;

            return BuildCiv(isBarbaric, out mockCiv);
        }

        private ICivilization BuildCiv(bool isBarbaric, out Mock<ICivilization> mockCiv) {
            mockCiv = new Mock<ICivilization>();

            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.IsBarbaric).Returns(isBarbaric);

            mockCiv.Setup(civ => civ.Template).Returns(mockTemplate.Object);

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
