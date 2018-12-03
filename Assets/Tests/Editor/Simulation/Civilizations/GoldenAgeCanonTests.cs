using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Civilizations {

    public class GoldenAgeCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<ICivilizationConfig>                           MockCivConfig;        
        private CivilizationSignals                                 CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCivConfig           = new Mock<ICivilizationConfig>();            
            CivSignals              = new CivilizationSignals();

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<ICivilizationConfig>                          ().FromInstance(MockCivConfig          .Object);
            Container.Bind<CivilizationSignals>                          ().FromInstance(CivSignals);

            Container.Bind<GoldenAgeCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetGoldenAgeProgressForCiv_DefaultsAtZero() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            Assert.AreEqual(0, goldenAgeCanon.GetGoldenAgeProgressForCiv(civ));
        }

        [Test]
        public void ChangeGoldenAgeProgressForCiv_ValueAddedToGoldenAgeProgress() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.ChangeGoldenAgeProgressForCiv(civ, 20f);
            goldenAgeCanon.ChangeGoldenAgeProgressForCiv(civ, 30f);
            goldenAgeCanon.ChangeGoldenAgeProgressForCiv(civ, 40f);

            Assert.AreEqual(90f, goldenAgeCanon.GetGoldenAgeProgressForCiv(civ));
        }

        [Test]
        public void SetGoldenAgeProgressForCiv_GoldenAgeProgressSetToValue() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.SetGoldenAgeProgressForCiv(civ, 20f);
            goldenAgeCanon.SetGoldenAgeProgressForCiv(civ, 30f);
            goldenAgeCanon.SetGoldenAgeProgressForCiv(civ, 40f);

            Assert.AreEqual(40f, goldenAgeCanon.GetGoldenAgeProgressForCiv(civ));
        }

        [Test]
        public void GetNextGoldenAgeCostForCiv_DefaultsToConfiguredBase() {
            var civ = BuildCiv();

            MockCivConfig.Setup(config => config.GoldenAgeBaseCost).Returns(200f);

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            Assert.AreEqual(200f, goldenAgeCanon.GetNextGoldenAgeCostForCiv(civ));
        }

        [Test]
        public void GetNextGoldenAgeCostForCiv_AddsCostFromPreviousGoldenAges() {
            var civ = BuildCiv();

            MockCivConfig.Setup(config => config.GoldenAgeBaseCost).Returns(200f);

            MockCivConfig.Setup(config => config.GoldenAgeCostPerPreviousAge).Returns(50f);

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.SetPreviousGoldenAgesForCiv(civ, 3);

            Assert.AreEqual(350f, goldenAgeCanon.GetNextGoldenAgeCostForCiv(civ));
        }

        [Test]
        public void GetNextGoldenAgeCostForCiv_BaseAndPreviousAgeCostModifiedByNumberOfCities() {
            var civ = BuildCiv(BuildCity(), BuildCity(), BuildCity());

            MockCivConfig.Setup(config => config.GoldenAgeBaseCost)          .Returns(200f);
            MockCivConfig.Setup(config => config.GoldenAgeCostPerPreviousAge).Returns(50f);
            MockCivConfig.Setup(config => config.GoldenAgePerCityMultiplier) .Returns(2f);

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.SetPreviousGoldenAgesForCiv(civ, 3);

            Assert.AreEqual(350f * 7f, goldenAgeCanon.GetNextGoldenAgeCostForCiv(civ));
        }

        [Test]
        public void GetPreviousGoldenAgesForCiv_DefaultsToZero() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            Assert.AreEqual(0f, goldenAgeCanon.GetPreviousGoldenAgesForCiv(civ));
        }

        [Test]
        public void SetPreviousGoldenAgesForCiv_OverridesPreviousGoldenAgesForCiv() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.SetPreviousGoldenAgesForCiv(civ, 2);
            goldenAgeCanon.SetPreviousGoldenAgesForCiv(civ, 3);
            goldenAgeCanon.SetPreviousGoldenAgesForCiv(civ, 4);

            Assert.AreEqual(4, goldenAgeCanon.GetPreviousGoldenAgesForCiv(civ));
        }

        [Test]
        public void IsCivInGoldenAge_DefaultsToFalse() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            Assert.IsFalse(goldenAgeCanon.IsCivInGoldenAge(civ));
        }

        [Test]
        public void StartGoldenAgeForCiv_IsCivInGoldenAgeBecomesTrue() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.StartGoldenAgeForCiv(civ, 10);

            Assert.IsTrue(goldenAgeCanon.IsCivInGoldenAge(civ));
        }

        [Test]
        public void StartGoldenAgeForCiv_ArguedTurnsBecomesTurnsLeftInGoldenAge() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.StartGoldenAgeForCiv(civ, 10);

            Assert.AreEqual(10, goldenAgeCanon.GetTurnsLeftOnGoldenAgeForCiv(civ));
        }

        [Test]
        public void StartGoldenAgeForCiv_PreviousGoldenAgesForCivIncremented() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.StartGoldenAgeForCiv(civ, 10);

            Assert.AreEqual(1, goldenAgeCanon.GetPreviousGoldenAgesForCiv(civ));
        }

        [Test]
        public void StartGoldenAgeForCiv_CivEnteredGoldenAgeFired() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            CivSignals.CivEnteredGoldenAge.Subscribe(delegate(ICivilization passedCiv) {
                Assert.AreEqual(civ, passedCiv, "CivEnteredGoldenAge fired on an unexpected value");
                Assert.Pass();
            });

            goldenAgeCanon.StartGoldenAgeForCiv(civ, 10);

            Assert.Fail("CivEnteredGoldenAge never fired");
        }

        [Test]
        public void StopGoldenAgeForCiv_IsCivInGoldenAgeBecomesFalse() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.StartGoldenAgeForCiv(civ, 10);
            goldenAgeCanon.StopGoldenAgeForCiv (civ);

            Assert.IsFalse(goldenAgeCanon.IsCivInGoldenAge(civ));
        }

        [Test]
        public void StopGoldenAgeForCiv_TurnsLeftInGoldenAgeBecomesZero() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.StartGoldenAgeForCiv(civ, 10);
            goldenAgeCanon.StopGoldenAgeForCiv (civ);

            Assert.AreEqual(0f, goldenAgeCanon.GetTurnsLeftOnGoldenAgeForCiv(civ));
        }

        [Test]
        public void StopGoldenAgeForCiv_CivLeftGoldenAgeFired() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.StartGoldenAgeForCiv(civ, 10);

            CivSignals.CivLeftGoldenAge.Subscribe(delegate(ICivilization passedCiv) {
                Assert.AreEqual(civ, passedCiv, "CivLeftGoldenAge was passed an unexpected value");
                Assert.Pass();
            });

            goldenAgeCanon.StopGoldenAgeForCiv(civ);

            Assert.Fail("CivLeftGoldenAge was never called");
        }

        [Test]
        public void ChangeTurnsOfGoldenAgeForCiv_TurnsLeftInGoldenAgeIncreasedByArguedValue() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.StartGoldenAgeForCiv(civ, 10);

            goldenAgeCanon.ChangeTurnsOfGoldenAgeForCiv(civ, 5);

            Assert.AreEqual(15, goldenAgeCanon.GetTurnsLeftOnGoldenAgeForCiv(civ));
        }




        [Test]
        public void StartGoldenAgeForCiv_ThrowsIfCivAlreadyInGoldenAge() {
             var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            goldenAgeCanon.StartGoldenAgeForCiv(civ, 10);

            Assert.Throws<InvalidOperationException>(() => goldenAgeCanon.StartGoldenAgeForCiv(civ, 15));
        }

        [Test]
        public void StopGoldenAgeForCiv_ThrowsIfCivNotInGoldenAge() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            Assert.Throws<InvalidOperationException>(() => goldenAgeCanon.StopGoldenAgeForCiv(civ));
        }

        [Test]
        public void ChangeTurnsOfGoldenAgeForCiv_ThrowsIfCivNotInGoldenAge() {
            var civ = BuildCiv();

            var goldenAgeCanon = Container.Resolve<GoldenAgeCanon>();

            Assert.Throws<InvalidOperationException>(() => goldenAgeCanon.ChangeTurnsOfGoldenAgeForCiv(civ, 10));
        }

        #endregion

        #region utilities

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        private ICivilization BuildCiv(params ICity[] cities) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
