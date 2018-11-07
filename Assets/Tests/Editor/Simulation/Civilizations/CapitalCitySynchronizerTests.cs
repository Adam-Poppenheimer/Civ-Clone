using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Civilizations {

    public class CapitalCitySynchronizerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICapitalCityCanon>                             MockCapitalCityCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private CivilizationSignals                                 CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCapitalCityCanon    = new Mock<ICapitalCityCanon>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            CivSignals              = new CivilizationSignals();

            Container.Bind<ICapitalCityCanon>                            ().FromInstance(MockCapitalCityCanon   .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<CivilizationSignals>                          ().FromInstance(CivSignals);

            Container.Bind<CapitalCitySynchronizer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsUpdatingCapitals_DefaultsToTrue() {
            var synchronizer = Container.Resolve<CapitalCitySynchronizer>();

            Assert.IsTrue(synchronizer.IsUpdatingCapitals);
        }

        [Test]
        public void SetCapitalUpdating_ReflectedInAreCapitalsUpdating() {
            var synchronizer = Container.Resolve<CapitalCitySynchronizer>();

            synchronizer.SetCapitalUpdating(false);

            Assert.IsFalse(synchronizer.IsUpdatingCapitals);
        }

        [Test]
        public void OnCivGainedCity_CityMadeCapitalIfCivHasNoCapital() {
            var civ = BuildCiv();
            var city = BuildCity();

            Container.Resolve<CapitalCitySynchronizer>();

            CivSignals.CivGainedCitySignal.OnNext(new UniRx.Tuple<ICivilization, ICity>(civ, city));

            MockCapitalCityCanon.Verify(canon => canon.SetCapitalOfCiv(civ, city), Times.Once);
        }

        [Test]
        public void OnCivGainedCity_NothingDoneIfCivAlreadyHasCapital() {
            var civ             = BuildCiv();
            var existingCapital = BuildCity();
            var newCity         = BuildCity();

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(civ)).Returns(existingCapital);

            Container.Resolve<CapitalCitySynchronizer>();

            CivSignals.CivGainedCitySignal.OnNext(new UniRx.Tuple<ICivilization, ICity>(civ, newCity));

            MockCapitalCityCanon.Verify(canon => canon.SetCapitalOfCiv(civ, newCity), Times.Never);
        }

        [Test]
        public void OnCivGainedCity_NothingDoneIfAreCapitalsUpdatingIsFalse() {
            var civ = BuildCiv();
            var city = BuildCity();

            var synchronizer = Container.Resolve<CapitalCitySynchronizer>();

            synchronizer.SetCapitalUpdating(false);

            CivSignals.CivGainedCitySignal.OnNext(new UniRx.Tuple<ICivilization, ICity>(civ, city));

            MockCapitalCityCanon.Verify(canon => canon.SetCapitalOfCiv(civ, city), Times.Never);
        }

        [Test]
        public void OnCivLostCity_AndCityWasCapital_SomeOtherCityMadeCapital() {
            var civ = BuildCiv();
            var oldCapital = BuildCity();
            var otherCity  = BuildCity();

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(civ)).Returns(oldCapital);

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(civ)).Returns(new List<ICity>() { otherCity });

            Container.Resolve<CapitalCitySynchronizer>();

            CivSignals.CivLostCitySignal.OnNext(new UniRx.Tuple<ICivilization, ICity>(civ, oldCapital));

            MockCapitalCityCanon.Verify(canon => canon.SetCapitalOfCiv(civ, otherCity), Times.Once);
        }

        [Test]
        public void OnCivLostCity_AndCityWasCapital_CapitalSetToNullIfNoCitiesLeft() {
            var civ = BuildCiv();
            var oldCapital = BuildCity();

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(civ)).Returns(oldCapital);

            Container.Resolve<CapitalCitySynchronizer>();

            CivSignals.CivLostCitySignal.OnNext(new UniRx.Tuple<ICivilization, ICity>(civ, oldCapital));

            MockCapitalCityCanon.Verify(canon => canon.SetCapitalOfCiv(civ, null), Times.Once);
        }

        [Test]
        public void OnCivLostCity_AndCityWasCapital_NothingDoneIfAreCapitalsUpdatingIsFalse() {
            var civ = BuildCiv();
            var oldCapital = BuildCity();
            var otherCity  = BuildCity();

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(civ)).Returns(oldCapital);

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(civ)).Returns(new List<ICity>() { otherCity });

            var synchronizer = Container.Resolve<CapitalCitySynchronizer>();

            synchronizer.SetCapitalUpdating(false);

            CivSignals.CivLostCitySignal.OnNext(new UniRx.Tuple<ICivilization, ICity>(civ, oldCapital));

            MockCapitalCityCanon.Verify(canon => canon.SetCapitalOfCiv(civ, otherCity), Times.Never);
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        #endregion

        #endregion

    }

}
