using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class SpecialtyResourceDistributionLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class DistributeResourcesOfCivIntoCitiesTestData {

            public List<CityTestData> Cities = new List<CityTestData>();

            public List<ResourceTestData> AvailableResources = new List<ResourceTestData>();

        }

        public class CityTestData {

            public int Health;
            public int Happiness;

            public List<bool> ExpectedResourcesAssigned = new List<bool>();

        }

        public class ResourceTestData {

            public SpecialtyResourceType Type;

            public int Copies;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable DistributeResourcesOfCivIntoCitiesTestCases {
            get {
                yield return new TestCaseData(new DistributeResourcesOfCivIntoCitiesTestData() {

                }).SetName("");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IResourceAssignmentCanon> MockAssignmentCanon;

        private Mock<IHealthLogic> MockHealthLogic;

        private Mock<IHappinessLogic> MockHappinessLogic;

        #endregion

        #region instance methods

        #region setup



        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("DistributeResourcesOfCivIntoCitiesTestCases")]
        public void DistributeResourcesOfCivIntoCitiesTests(DistributeResourcesOfCivIntoCitiesTestData testData) {
            throw new NotImplementedException();
        }

        #endregion

        #region utilities

        private ICity BuildCity(int health, int happiness) {
            throw new NotImplementedException();
        }

        private ICivilization BuildCivilization(IEnumerable<ICity> cities) {
            throw new NotImplementedException();
        }

        private ISpecialtyResourceDefinition BuildResource(SpecialtyResourceType type, int freeCopies, ICivilization forCiv) {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

    }

}
