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

        private Mock<ISpecialtyResourcePossessionCanon> MockResourcePossessionCanon;

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockResourcePossessionCanon = new Mock<ISpecialtyResourcePossessionCanon>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<ISpecialtyResourcePossessionCanon>            ().FromInstance(MockResourcePossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);

            Container.Bind<ResourceAssignmentCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanAssignResourceToCity requires that the city's owner has free copies of that resource")]
        public void CanAssignResourceToCity_RequiresFreeResourcesForOwner() {
            var resource = BuildDefinition();

            var testCity = BuildCity();

            var otherCity = BuildCity();

            var civilization = BuildCivilization(new List<ICity>() { testCity, otherCity });

            MockResourcePossessionCanon.Setup(canon => canon.GetCopiesOfResourceBelongingToCiv(resource, civilization))
                .Returns(0);

            var assignmentCanon = Container.Resolve<ResourceAssignmentCanon>();

            Assert.IsFalse(assignmentCanon.CanAssignResourceToCity(resource, testCity),
                "CanAssignResourceToCity did not forbid assignment when no copies of the resource were available");

            MockResourcePossessionCanon.Setup(canon => canon.GetCopiesOfResourceBelongingToCiv(resource, civilization))
                .Returns(1);

            Assert.IsTrue(assignmentCanon.CanAssignResourceToCity(resource, testCity),
                "CanAssignResourceToCity falsely forbade assignment when a copy of the resource was available");

            assignmentCanon.AssignResourceToCity(resource, otherCity);

            Assert.IsFalse(assignmentCanon.CanAssignResourceToCity(resource, testCity),
                "CanAssignResourceToCity did not forbid assignment when all copies of the resource were assigned");
        }

        [Test(Description = "CanAssignResourceToCity should not permit multiple resource assignments")]
        public void CanAssignResourceToCity_FalseIfResourceAlreadyAssigned() {
            var resource = BuildDefinition();

            var city = BuildCity();

            var civilization = BuildCivilization(new List<ICity>() { city });

            MockResourcePossessionCanon.Setup(canon => canon.GetCopiesOfResourceBelongingToCiv(resource, civilization))
                .Returns(2);

            var assignmentCanon = Container.Resolve<ResourceAssignmentCanon>();

            assignmentCanon.AssignResourceToCity(resource, city);

            Assert.IsFalse(assignmentCanon.CanAssignResourceToCity(resource, city),
                "CanAssignResourceToCity did not forbid assignment when the city had already had the resource assigned to it");
        }

        [Test(Description = "When AssignResourceToCity is called, the new assignment should be " +
            "reflected in HasResourceBeenAssignedToCity, GetAllResourcesAssignedToCity, " +
            "and GetFreeCopiesOfResourceForCiv")]
        public void AssignResourceToCity_ReflectedInGetMethods() {
            var resource = BuildDefinition();

            var city = BuildCity();

            var civilization = BuildCivilization(new List<ICity>() { city });

            MockResourcePossessionCanon.Setup(canon => canon.GetCopiesOfResourceBelongingToCiv(resource, civilization))
                .Returns(2);

            var assignmentCanon = Container.Resolve<ResourceAssignmentCanon>();

            assignmentCanon.AssignResourceToCity(resource, city);

            Assert.IsTrue(assignmentCanon.HasResourceBeenAssignedToCity(resource, city),
                "HasResourceBeenAssignedToCity does not reflect the resource assignment");

            CollectionAssert.AreEqual(
                new List<ISpecialtyResourceDefinition>() { resource },
                assignmentCanon.GetAllResourcesAssignedToCity(city),
                "GetAllResourcesAssignedToCity does not reflect the resource assignment"
            );

            Assert.AreEqual(1, assignmentCanon.GetFreeCopiesOfResourceForCiv(resource, civilization),
                "GetFreeCopiesOfResourceForCiv does not reflect the resource assignment");
        }

        [Test(Description = "")]
        public void CanUnassignResourceFromCity_RequiresResourceBeAssigned() {
            var resource = BuildDefinition();

            var city = BuildCity();

            var civilization = BuildCivilization(new List<ICity>() { city });

            MockResourcePossessionCanon.Setup(canon => canon.GetCopiesOfResourceBelongingToCiv(resource, civilization))
                .Returns(2);

            var assignmentCanon = Container.Resolve<ResourceAssignmentCanon>();

            Assert.IsFalse(assignmentCanon.CanUnassignResourceFromCity(resource, city),
                "CanUnassignResourceFromCity falsely permits unassignment of a resource that hasn't been assigned");

            assignmentCanon.AssignResourceToCity(resource, city);

            Assert.IsTrue(assignmentCanon.CanUnassignResourceFromCity(resource, city),
                "CanUnassignResourceFromCity fails to permit unassignment of an assigned resource");
        }

        [Test(Description = "")]
        public void UnassignResourceFromCity_ReflectedInGetMethods() {
            var resource = BuildDefinition();

            var city = BuildCity();

            var civilization = BuildCivilization(new List<ICity>() { city });

            MockResourcePossessionCanon.Setup(canon => canon.GetCopiesOfResourceBelongingToCiv(resource, civilization))
                .Returns(2);

            var assignmentCanon = Container.Resolve<ResourceAssignmentCanon>();

            assignmentCanon.AssignResourceToCity(resource, city);

            assignmentCanon.UnassignResourceFromCity(resource, city);

            Assert.IsFalse(assignmentCanon.HasResourceBeenAssignedToCity(resource, city),
                "HasResourceBeenAssignedToCity does not reflect the resource unassignment");

            CollectionAssert.IsEmpty(
                assignmentCanon.GetAllResourcesAssignedToCity(city),
                "GetAllResourcesAssignedToCity does not reflect the resource unassignment"
            );

            Assert.AreEqual(2, assignmentCanon.GetFreeCopiesOfResourceForCiv(resource, civilization),
                "GetFreeCopiesOfResourceForCiv does not reflect the resource unassignment");
        }

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

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

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
