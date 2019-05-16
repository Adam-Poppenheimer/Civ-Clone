using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

namespace Assets.Tests.Simulation {

    [TestFixture]
    public class PossessionRelationshipTests : ZenjectUnitTestFixture {

        #region internal types

        private class TestOwner { }
        private class TestPossession {  }

        #endregion

        [Test(Description = "When ChangeOwnerOfPossession is called, GetOwnerOfPossession " +
            "should respond to the new possession relationship correctly, even if the new " +
            "owner is null")]
        public void ChangeOwnerOfPossession_ChangesReflectedInGetOwnerOfPossession() {
            var possession = new TestPossession();

            var ownerOne = new TestOwner();
            var ownerTwo = new TestOwner();

            var relationship = new PossessionRelationship<TestOwner, TestPossession>();

            relationship.ChangeOwnerOfPossession(possession, ownerOne);

            Assert.AreEqual(ownerOne, relationship.GetOwnerOfPossession(possession),
                "GetOwnerOfPossession returned an unexpected value");

            relationship.ChangeOwnerOfPossession(possession, ownerTwo);

            Assert.AreEqual(ownerTwo, relationship.GetOwnerOfPossession(possession),
                "GetOwnerOfPossession returned an unexpected value");

            relationship.ChangeOwnerOfPossession(possession, null);
            Assert.AreEqual(null, relationship.GetOwnerOfPossession(possession),
                "GetOwnerOfPossession returned an unexpected value");
        }

        [Test(Description = "When ChangeOwnerOfPossession is called, GetPossessionsOfOwner " +
            "should respond to the new possession relationship correctly, even if the new " +
            "owner is null")]
        public void ChangeOwnerOfPossession_ChangesReflectedInGetPossessionsOfOwner() {
            var possession = new TestPossession();

            var ownerOne = new TestOwner();
            var ownerTwo = new TestOwner();

            var relationship = new PossessionRelationship<TestOwner, TestPossession>();

            relationship.ChangeOwnerOfPossession(possession, ownerOne);

            CollectionAssert.Contains(relationship.GetPossessionsOfOwner(ownerOne), possession,
                "OwnerOne does not have the possession");

            relationship.ChangeOwnerOfPossession(possession, ownerTwo);

            CollectionAssert.DoesNotContain(relationship.GetPossessionsOfOwner(ownerOne), possession,
                "OwnerOne incorrectly has the possession");
            CollectionAssert.Contains(relationship.GetPossessionsOfOwner(ownerTwo), possession,
                "OwnerTwo does not have the possession");            

            relationship.ChangeOwnerOfPossession(possession, null);
            CollectionAssert.DoesNotContain(relationship.GetPossessionsOfOwner(ownerTwo), possession,
                "OwnerTwo falsely has the possession");
        }

        [Test(Description = "When ChangeOwnerOfPossession is called on arguments that " +
            "CanChangeOwnerOfPossession would return false on, it should throw an " +
            "InvalidOperationException")]
        public void ChangeOwnerOfPossession_ThrowsIfNotPermitted() {
            var possession = new TestPossession();

            var relationship = new PossessionRelationship<TestOwner, TestPossession>();

            Assert.Throws<InvalidOperationException>(() => relationship.ChangeOwnerOfPossession(possession, null),
                "ChangeOwnerOfPossession did not throw correctly on an invalid request");
        }

        [Test(Description = "CanChangeOwnerOfPossession should return false if the " +
            "owner argument is already the owner of the possession")]
        public void CanChangeOwnerOfPossession_FalseIfAlreadyOwnedBy() {
            var possession = new TestPossession();

            var owner = new TestOwner();

            var relationship = new PossessionRelationship<TestOwner, TestPossession>();

            Assert.IsFalse(relationship.CanChangeOwnerOfPossession(possession, null),
                "CanChangeOwnerOfPossession returned an unexpected value");

            relationship.ChangeOwnerOfPossession(possession, owner);

            Assert.IsFalse(relationship.CanChangeOwnerOfPossession(possession, owner),
                "CanChangeOwnerOfPossession returned an unexpected value");
        }

        [Test(Description = "All methods should throw an ArgumentNullException on any null " +
            "possession arguments")]
        public void AllMethods_ThrowOnNullPossessionArgument() {
            var owner = new TestOwner();

            var relationship = new PossessionRelationship<TestOwner, TestPossession>();

            Assert.Throws<ArgumentNullException>(() => relationship.GetOwnerOfPossession(null),
                "GetOwnerOfPossession did not throw correctly");

            Assert.Throws<ArgumentNullException>(() => relationship.CanChangeOwnerOfPossession(null, owner),
                "CanChangeOwnerOfPossession did not throw correctly");

            Assert.Throws<ArgumentNullException>(() => relationship.ChangeOwnerOfPossession(null, owner),
                "ChangeOwnerOfPossession did not throw correctly");
        }

        [Test(Description = "GetPossessionsOfOwner should throw an ArgumentNullException on " +
            "any null argument")]
        public void GetPossessionsOfOwner_ThrowsOnNullArgument() {
            var relationship = new PossessionRelationship<TestOwner, TestPossession>();

            Assert.Throws<ArgumentNullException>(() => relationship.GetPossessionsOfOwner(null),
                "GetPossessionOfOwner did not throw correctly");
        }

    }
}
