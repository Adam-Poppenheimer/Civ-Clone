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

namespace Assets.Tests.Simulation.Core {

    [TestFixture]
    public class TurnExecuterTests : ZenjectUnitTestFixture {

        [Test(Description = "When BeginTurnOnCity is called on a city, some of that city's " +
            "Perform methods are called in a specific order")]
        public void BeginTurnOnCity_PerformanceHappensInOrder() {
            var mockCity = new Mock<ICity>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockCity.InSequence(executionSequence).Setup(city => city.PerformProduction());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformGrowth());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformExpansion());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformDistribution());

            var executer = new TurnExecuter();
            executer.BeginTurnOnCity(mockCity.Object);

            mockCity.VerifyAll();
        }

        [Test(Description = "When EndTurnOnCity is called on a city, some of that city's " +
            "Perform methods are called in a specific order")]
        public void EndTurnOnCity_PerformanceHappensInOrder() {
            var mockCity = new Mock<ICity>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockCity.InSequence(executionSequence).Setup(city => city.PerformIncome());

            var executer = new TurnExecuter();
            executer.EndTurnOnCity(mockCity.Object);

            mockCity.VerifyAll();
        }

        [Test(Description = "When BeginTurnOnCivilization is called on a civilization, that " +
            "city's PerformIncome method should be called")]
        public void BeginTurnOnCivilization_PerformanceHappensInOrder() {
            var mockCivilization = new Mock<ICivilization>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockCivilization.InSequence(executionSequence).Setup(civilization => civilization.PerformIncome());

            var executer = new TurnExecuter();
            executer.EndTurnOnCivilization(mockCivilization.Object);

            mockCivilization.VerifyAll();
        }

        [Test(Description = "When EndTurnOnCivilization is called on a civilization, " +
            "nothing of note should happen")]
        public void EndTurnOnCivilization_PerformanceHappensInOrder() {
            Assert.Pass();
        }

    }

}
