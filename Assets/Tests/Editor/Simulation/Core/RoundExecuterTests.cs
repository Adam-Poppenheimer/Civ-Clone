using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Core;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Core {

    [TestFixture]
    public class RoundExecuterTests : ZenjectUnitTestFixture {

        #region instance methods

        #region tests

        [Test(Description = "When BeginTurnOnCity is called on a city, some of that city's " +
            "Perform methods are called in a specific order")]
        public void BeginTurnOnCity_PerformanceHappensInOrder() {
            var mockCity = new Mock<ICity>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockCity.InSequence(executionSequence).Setup(city => city.PerformProduction());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformGrowth());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformExpansion());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformDistribution());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformHealing());

            var executer = new RoundExecuter();
            executer.BeginRoundOnCity(mockCity.Object);

            mockCity.VerifyAll();
        }

        [Test(Description = "When EndTurnOnCity is called on a city, some of that city's " +
            "Perform methods are called in a specific order")]
        public void EndTurnOnCity_PerformanceHappensInOrder() {
            var mockCity = new Mock<ICity>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockCity.InSequence(executionSequence).Setup(city => city.PerformIncome());

            var executer = new RoundExecuter();
            executer.EndRoundOnCity(mockCity.Object);

            mockCity.VerifyAll();
        }

        [Test(Description = "When BeginTurnOnCivilization is called on a civilization, that " +
            "city's PerformIncome method should be called")]
        public void BeginTurnOnCivilization_PerformanceHappensInOrder() {
            var mockCivilization = new Mock<ICivilization>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockCivilization.InSequence(executionSequence).Setup(civilization => civilization.PerformDistribution());
            mockCivilization.InSequence(executionSequence).Setup(civilization => civilization.PerformIncome());
            mockCivilization.InSequence(executionSequence).Setup(civilization => civilization.PerformResearch());

            var executer = new RoundExecuter();
            executer.EndRoundOnCivilization(mockCivilization.Object);

            mockCivilization.VerifyAll();
        }

        [Test(Description = "When EndTurnOnCivilization is called on a civilization, " +
            "nothing of note should happen")]
        public void EndTurnOnCivilization_PerformanceHappensInOrder() {
            Assert.Pass();
        }

        [Test(Description = "When BeginTurnOnUnit is called on a unit, that " +
            "unit should have its CurrentMovement reset to its template's MaxMovement")]
        public void BeginTurnOnUnit_CurrentMovementRefreshed() {
            var unit = BuildUnit(currentMovement: 0, maxMovement: 5);

            var executer = new RoundExecuter();

            executer.BeginRoundOnUnit(unit);

            Assert.AreEqual(unit.MaxMovement, unit.CurrentMovement, "unit.CurrentMovement has an unexpected value");
        }

        [Test(Description = "When EndTurnOnUnit is called on a unit, that " +
            "unit should have its PerformMovement method called")]
        public void EndTurnOnUnit_PerformanceHappensInOrder() {
            var mockUnit = new Mock<IUnit>();

            var executer = new RoundExecuter();

            executer.EndRoundOnUnit(mockUnit.Object);

            mockUnit.Verify(unit => unit.PerformMovement(), Times.Once,
                "unit.PerformMovement was not called as expected");
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(int currentMovement, int maxMovement) {
            var mockUnit = new Mock<IUnit>();
            mockUnit.SetupAllProperties();

            mockUnit.Setup(unit => unit.MaxMovement).Returns(maxMovement);
            mockUnit.Object.CurrentMovement = currentMovement;

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
