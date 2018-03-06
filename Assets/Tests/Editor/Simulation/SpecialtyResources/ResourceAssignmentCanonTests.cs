using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Tests.Simulation.SpecialtyResources {

    [TestFixture]
    public class ResourceAssignmentCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ISpecialtyResourcePossessionLogic> MockResourcePossessionCanon;

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockResourcePossessionCanon = new Mock<ISpecialtyResourcePossessionLogic>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<ISpecialtyResourcePossessionLogic>            ().FromInstance(MockResourcePossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);

            Container.Bind<CivilizationSignals>().AsSingle();

            Container.Bind<ResourceAssignmentCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanReserveCopyOfResourceForCiv should be true even when GetFreeCopiesOfResourceForCiv " +
            "returns a zero or negative value. That means a civilization can reserve resources beyond the number of " +
            "copies it has")]
        public void CanReserveCopyOfResourceForCiv_TrueEvenWhenNoFreeCopiesExist() {
            var resource = BuildDefinition();

            var civilization = BuildCivilization(new List<ICity>());

            var assignmentCanon = Container.Resolve<ResourceAssignmentCanon>();

            Assert.IsTrue(assignmentCanon.CanReserveCopyOfResourceForCiv(resource, civilization),
                "CanReserveCopyOfResourceForCiv returned an unexpected value");
        }

        [Test(Description = "ReserveCopyOfResourceForCiv should reduce by one the number returned by " +
            "GetFreeCopiesOfResourceForCiv when called on the same arguments")]
        public void ReserveCopyOfResourceForCiv_ReflectedInFreeCopies() {
            var resource = BuildDefinition();

            var civilization = BuildCivilization(new List<ICity>());

            var assignmentCanon = Container.Resolve<ResourceAssignmentCanon>();

            assignmentCanon.ReserveCopyOfResourceForCiv(resource, civilization);

            Assert.AreEqual(-1, assignmentCanon.GetFreeCopiesOfResourceForCiv(resource, civilization));
        }

        [Test(Description = "CanUnreserveCopyOfResourceForCiv should only return true if more than one copy " +
            "of the argued resource has been reserved by the argued civilization")]
        public void CanUnreserveCopyOfResourceForCiv_TrueOnlyIfCopyIsReserved() {
            var resource = BuildDefinition();

            var civilization = BuildCivilization(new List<ICity>());

            var assignmentCanon = Container.Resolve<ResourceAssignmentCanon>();

            Assert.IsFalse(assignmentCanon.CanUnreserveCopyOfResourceForCiv(resource, civilization),
                "CanUnreserveCopyOfResourceForCiv returned true when no copy of the resource had been reserved");

            assignmentCanon.ReserveCopyOfResourceForCiv(resource, civilization);

            Assert.IsTrue(assignmentCanon.CanUnreserveCopyOfResourceForCiv(resource, civilization),
                "CanUnreserveCopyOfResourceForCiv returned false when a copy of the resource had been reserved");
        }
        
        [Test(Description = "UnreserveCopyOfResourceForCiv should increase by one the number " +
            "returned by GetFreeCopiesOfResourceForCiv when called on the same arguments")]
        public void UnreserveCopyOfResourceForCiv_ReflectedInFreeCopies() {
            var resource = BuildDefinition();

            var civilization = BuildCivilization(new List<ICity>());

            var assignmentCanon = Container.Resolve<ResourceAssignmentCanon>();

            assignmentCanon.ReserveCopyOfResourceForCiv(resource, civilization);
            assignmentCanon.UnreserveCopyOfResourceForCiv(resource, civilization);

            Assert.AreEqual(0, assignmentCanon.GetFreeCopiesOfResourceForCiv(resource, civilization),
                "GetFreeCopiesOfResourceForCiv returned an unexpected value");
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(List<ICity> cities) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            return newCiv;
        }

        private ISpecialtyResourceDefinition BuildDefinition() {
            return new Mock<ISpecialtyResourceDefinition>().Object;
        }

        #endregion

        #endregion

    }

}
