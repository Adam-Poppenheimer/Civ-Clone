using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Core;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.Core {

    [TestFixture]
    public class RoundExecuterTests : ZenjectUnitTestFixture {

        #region internal types

        public class EndRoundOnUnitTestData {

            public HexCellTestData LocationOfUnit;

            public UnitTestData UnitToTest;

            public ImprovementTestData ImprovementAtLocation;

        }

        public class HexCellTestData { }

        public class UnitTestData {

            public bool LockedIntoConstruction;

            public int CurrentMovement;

            public int MaxMovement;

            public int ExpectedMovement;

        }

        public class ImprovementTestData {

            public bool IsConstructed;

            public int WorkInvested;

            public bool IsReadyToConstruct;

            public bool ExpectsToBeConstructed;

            public int ExpectedWorkInvested;

        }

        public class ImprovementTemplateTestData {

            public int TurnsToConstruct;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable EndRoundOnUnitTestCases {
            get {
                yield return new TestCaseData(new EndRoundOnUnitTestData() {
                    LocationOfUnit = new HexCellTestData() {  },
                    UnitToTest = new UnitTestData() {
                        CurrentMovement = 1, ExpectedMovement = 0,
                        LockedIntoConstruction = true, MaxMovement = 1
                    },
                    ImprovementAtLocation = new ImprovementTestData() {
                        WorkInvested = 0, IsConstructed = false, IsReadyToConstruct = false,
                        ExpectedWorkInvested = 1, ExpectsToBeConstructed = false
                    }
                }).SetName("Locked unit with spare movement, unconstructed improvement not ready for construction");

                yield return new TestCaseData(new EndRoundOnUnitTestData() {
                    LocationOfUnit = new HexCellTestData() {  },
                    UnitToTest = new UnitTestData() {
                        CurrentMovement = 1, ExpectedMovement = 0,
                        LockedIntoConstruction = true, MaxMovement = 1
                    },
                    ImprovementAtLocation = new ImprovementTestData() {
                        WorkInvested = 1, IsConstructed = false, IsReadyToConstruct = true,
                        ExpectedWorkInvested = 2, ExpectsToBeConstructed = true
                    }
                }).SetName("Locked unit with spare movement, unconstructed improvement ready for construction");

                yield return new TestCaseData(new EndRoundOnUnitTestData() {
                    LocationOfUnit = new HexCellTestData() {  },
                    UnitToTest = new UnitTestData() {
                        CurrentMovement = 0, ExpectedMovement = 0,
                        LockedIntoConstruction = true, MaxMovement = 1
                    },
                    ImprovementAtLocation = new ImprovementTestData() {
                        WorkInvested = 0, IsConstructed = false,
                        ExpectedWorkInvested = 0, ExpectsToBeConstructed = false
                    }
                }).SetName("Locked unit with no movement");

                yield return new TestCaseData(new EndRoundOnUnitTestData() {
                    LocationOfUnit = new HexCellTestData() {  },
                    UnitToTest = new UnitTestData() {
                        CurrentMovement = 1, ExpectedMovement = 1,
                        LockedIntoConstruction = false, MaxMovement = 1
                    },
                    ImprovementAtLocation = new ImprovementTestData() {
                        WorkInvested = 0, IsConstructed = false,
                        ExpectedWorkInvested = 0, ExpectsToBeConstructed = false
                    }
                }).SetName("Unlocked unit with movement");

                yield return new TestCaseData(new EndRoundOnUnitTestData() {
                    LocationOfUnit = new HexCellTestData() {  },
                    UnitToTest = new UnitTestData() {
                        CurrentMovement = 1, ExpectedMovement = 1,
                        LockedIntoConstruction = true, MaxMovement = 1
                    },
                    ImprovementAtLocation = new ImprovementTestData() {
                        WorkInvested = 0, IsConstructed = true,
                        ExpectedWorkInvested = 0, ExpectsToBeConstructed = false
                    }
                }).SetName("Locked unit with spare movement, constructed improvement");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitPositionCanon>        MockUnitPositionCanon;
        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;
        private Mock<IUnitHealingLogic>         MockHealingLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon        = new Mock<IUnitPositionCanon>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockHealingLogic             = new Mock<IUnitHealingLogic>();

            Container.Bind<IUnitPositionCanon>       ().FromInstance(MockUnitPositionCanon       .Object);
            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<IUnitHealingLogic>        ().FromInstance(MockHealingLogic            .Object);

            Container.Bind<RoundExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When BeginRoundOnCity is called on a city, some of that city's " +
            "Perform methods are called in a specific order")]
        public void BeginRoundOnCity_PerformanceHappensInOrder() {
            var mockCity = new Mock<ICity>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockCity.InSequence(executionSequence).Setup(city => city.PerformProduction());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformGrowth());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformExpansion());
            mockCity.InSequence(executionSequence).Setup(city => city.PerformDistribution());

            var executer = Container.Resolve<RoundExecuter>();
            executer.BeginRoundOnCity(mockCity.Object);

            mockCity.VerifyAll();
        }

        [Test(Description = "When EndRoundOnCity is called on a city, some of that city's " +
            "Perform methods are called in a specific order")]
        public void EndRoundOnCity_PerformanceHappensInOrder() {
            var mockCity = new Mock<ICity>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockCity.InSequence(executionSequence).Setup(city => city.PerformIncome());

            var executer = Container.Resolve<RoundExecuter>();
            executer.EndRoundOnCity(mockCity.Object);

            mockCity.VerifyAll();
        }

        [Test]
        public void BeginRoundOnCivilization_PerformanceHappensInOrder() {
            var mockCivilization = new Mock<ICivilization>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockCivilization.InSequence(executionSequence).Setup(civilization => civilization.PerformIncome());
            mockCivilization.InSequence(executionSequence).Setup(civilization => civilization.PerformResearch());
            mockCivilization.InSequence(executionSequence).Setup(civilization => civilization.PerformGreatPeopleGeneration());

            var executer = Container.Resolve<RoundExecuter>();
            executer.BeginRoundOnCivilization(mockCivilization.Object);

            mockCivilization.VerifyAll();
        }

        [Test(Description = "When BeginRoundOnUnit is called on a unit, that " +
            "unit should have its CurrentMovement reset to its template's MaxMovement")]
        public void BeginRoundOnUnit_CurrentMovementRefreshed() {
            var unit = BuildUnit(currentMovement: 0, maxMovement: 5);

            var executer = Container.Resolve<RoundExecuter>();

            executer.BeginRoundOnUnit(unit);

            Assert.AreEqual(unit.MaxMovement, unit.CurrentMovement, "unit.CurrentMovement has an unexpected value");
        }

        [Test]
        public void BeginRoundOnUnit_HealingPerformedOnUnit() {
            var unit = BuildUnit(0, 0);

            var executer = Container.Resolve<RoundExecuter>();

            executer.BeginRoundOnUnit(unit);

            MockHealingLogic.Verify(
                logic => logic.PerformHealingOnUnit(unit), Times.Once,
                "HealingLogic.PerformHealingOnUnit was not called as expected"
            );
        }

        [TestCaseSource("EndRoundOnUnitTestCases")]
        [Test(Description = "")]
        public void EndRoundOnUnitTests(EndRoundOnUnitTestData testData) {
            var locationOfUnit = BuildHexCell(testData.LocationOfUnit);

            Mock<IUnit> mockUnit;
            var unitToTest = BuildUnit(testData.UnitToTest, locationOfUnit, out mockUnit);

            Mock<IImprovement> mockImprovement = null;
            IImprovement improvementAtLocation = null;
            if(testData.ImprovementAtLocation != null) {
                improvementAtLocation = BuildImprovement(testData.ImprovementAtLocation, locationOfUnit, out mockImprovement);
            }            

            var executer = Container.Resolve<RoundExecuter>();

            executer.EndRoundOnUnit(unitToTest);

            Assert.AreEqual(
                testData.UnitToTest.ExpectedMovement, unitToTest.CurrentMovement,
                "unit has an unexpected CurrentMovement"
            );

            mockUnit.Verify(unit => unit.PerformMovement(), Times.Once, "Unit did not have its PerformMovement method called");
            
            mockUnit.VerifySet(unit => unit.CanAttack = true, Times.Once,
                "unit.CanAttack was not set to true");

            if(improvementAtLocation != null) {
                Assert.AreEqual(
                    testData.ImprovementAtLocation.ExpectedWorkInvested, improvementAtLocation.WorkInvested,
                    "ImprovementAtLocation.WorkInvested has an unexpected value"
                );

                if(testData.ImprovementAtLocation.ExpectsToBeConstructed) {
                    mockImprovement.Verify(
                        improvement => improvement.Construct(), Times.Once,
                        "ImprovementAtLocation was not constructed as expected"
                    );
                }else {
                    mockImprovement.Verify(
                        improvement => improvement.Construct(), Times.Never,
                        "ImprovementAtLocation was unexpectedly constructed"
                    );
                }                
            }
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(UnitTestData unitData, IHexCell location, out Mock<IUnit> mock) {
            mock = new Mock<IUnit>();
            mock.SetupAllProperties();

            mock.Setup(unit => unit.MaxMovement)           .Returns(unitData.MaxMovement);
            mock.Setup(unit => unit.LockedIntoConstruction).Returns(unitData.LockedIntoConstruction);

            var newUnit = mock.Object;

            newUnit.CurrentMovement = unitData.CurrentMovement;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IHexCell BuildHexCell(HexCellTestData cellData) {
            return new Mock<IHexCell>().Object;
        }

        private IImprovement BuildImprovement(ImprovementTestData improvementData, IHexCell location, out Mock<IImprovement> mock) {
            mock = new Mock<IImprovement>();

            mock.SetupAllProperties();

            mock.Setup(improvement => improvement.IsReadyToConstruct).Returns(improvementData.IsReadyToConstruct);
            mock.Setup(improvement => improvement.IsConstructed)     .Returns(improvementData.IsConstructed);

            var newImprovement = mock.Object;

            newImprovement.WorkInvested = improvementData.WorkInvested;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IImprovement>() { newImprovement });

            return newImprovement;
        }

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
