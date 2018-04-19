using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Civilizations;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class ResourceLockingCanonTests : ZenjectUnitTestFixture {

        #region internal types

        public enum LockingType { Lock, Unlock };

        public class LockingTestData {

            public List<LockingType> LockingSequence = new List<LockingType>();

        }

        #endregion

        #region static fields and properties

        public static IEnumerable CanLockCopyOfResourceForCivTestCases {
            get {
                yield return new TestCaseData(new LockingTestData() {
                    
                }).SetName("True when no locked copies").Returns(true);

                yield return new TestCaseData(new LockingTestData() {
                    LockingSequence = new List<LockingType>() {
                        LockingType.Lock, LockingType.Lock, LockingType.Lock, LockingType.Lock,
                    }
                }).SetName("True when many locked copies").Returns(true);

                yield return new TestCaseData(new LockingTestData() {
                    LockingSequence = new List<LockingType>() {
                        LockingType.Lock, LockingType.Lock, LockingType.Lock, LockingType.Lock,
                        LockingType.Unlock, LockingType.Unlock, LockingType.Unlock, LockingType.Unlock,
                    }
                }).SetName("True when copies have been locked and unlocked").Returns(true);
            }
        }

        public static IEnumerable LockCopyOfResourceForCivTestCases {
            get {
                yield return new TestCaseData(new LockingTestData() {
                    
                }).SetName("Increments locked copies by one when no locks").Returns(1);

                yield return new TestCaseData(new LockingTestData() {
                    LockingSequence = new List<LockingType>() {
                        LockingType.Lock, LockingType.Lock, LockingType.Lock, LockingType.Lock,
                    }
                }).SetName("Increments locked copies by one when many previous locks").Returns(5);
            }
        }

        public static IEnumerable CanUnlockCopyOfResourceForCivTestCases {
            get {
                yield return new TestCaseData(new LockingTestData() {
                    
                }).SetName("False when no locked copies").Returns(false);

                yield return new TestCaseData(new LockingTestData() {
                    LockingSequence = new List<LockingType>() {
                        LockingType.Lock
                    }
                }).SetName("True when locked copies").Returns(true);

                yield return new TestCaseData(new LockingTestData() {
                    LockingSequence = new List<LockingType>() {
                        LockingType.Lock, LockingType.Lock, LockingType.Lock,
                        LockingType.Unlock, LockingType.Unlock, LockingType.Unlock,
                    }
                }).SetName("False when net locked copies is zero").Returns(false);

                yield return new TestCaseData(new LockingTestData() {
                    LockingSequence = new List<LockingType>() {
                        LockingType.Lock, LockingType.Lock, LockingType.Lock,
                        LockingType.Unlock, LockingType.Unlock
                    }
                }).SetName("True when net locked copies is positive").Returns(true);
            }
        }

        public static IEnumerable UnlockCopyOfResourceForCivTestCases {
            get {
                yield return new TestCaseData(new LockingTestData() {
                    LockingSequence = new List<LockingType>() {
                        LockingType.Lock, LockingType.Lock, LockingType.Lock,
                    }
                }).SetName("Decrements locked copies by 1 when locks").Returns(2);

                yield return new TestCaseData(new LockingTestData() {
                    LockingSequence = new List<LockingType>() {
                        LockingType.Lock, LockingType.Lock, LockingType.Lock, LockingType.Lock,
                        LockingType.Unlock, LockingType.Unlock,
                    }
                }).SetName("Decrements locked copies by 1 when locks and unlocks").Returns(1);
            }
        }

        #endregion

        #region instance fields and properties



        #endregion

        #region instance methods

        #region setup
        
        [SetUp]
        public void CommonInstall() {
            Container.Bind<ResourceLockingCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("CanLockCopyOfResourceForCivTestCases")]
        public bool CanLockCopyOfResourceForCivTests(LockingTestData testData) {
            var resource = BuildResource();
            var civ = BuildCiv();

            var lockingCanon = Container.Resolve<ResourceLockingCanon>();

            ExecuteLockingSequence(lockingCanon, testData.LockingSequence, resource, civ);

            return lockingCanon.CanLockCopyOfResourceForCiv(resource, civ);
        }

        [Test]
        [TestCaseSource("LockCopyOfResourceForCivTestCases")]
        public int LockCopyOfResourceForCivTests(LockingTestData testData) {
            var resource = BuildResource();
            var civ = BuildCiv();

            var lockingCanon = Container.Resolve<ResourceLockingCanon>();

            ExecuteLockingSequence(lockingCanon, testData.LockingSequence, resource, civ);

            lockingCanon.LockCopyOfResourceForCiv(resource, civ);

            return lockingCanon.GetLockedCopiesOfResourceForCiv(resource, civ);
        }

        [Test]
        [TestCaseSource("CanUnlockCopyOfResourceForCivTestCases")]
        public bool CanUnlockCopyOfResourceForCivTests(LockingTestData testData) {
            var resource = BuildResource();
            var civ = BuildCiv();

            var lockingCanon = Container.Resolve<ResourceLockingCanon>();

            ExecuteLockingSequence(lockingCanon, testData.LockingSequence, resource, civ);

            return lockingCanon.CanUnlockCopyOfResourceForCiv(resource, civ);
        }

        [Test]
        [TestCaseSource("UnlockCopyOfResourceForCivTestCases")]
        public int UnlockCopyOfResourceForCivTests(LockingTestData testData) {
            var resource = BuildResource();
            var civ = BuildCiv();

            var lockingCanon = Container.Resolve<ResourceLockingCanon>();

            ExecuteLockingSequence(lockingCanon, testData.LockingSequence, resource, civ);

            lockingCanon.UnlockCopyOfResourceForCiv(resource, civ);

            return lockingCanon.GetLockedCopiesOfResourceForCiv(resource, civ);
        }

        [Test(Description = "UnlockCopyOfResourceForCiv should throw an InvalidOperationException " +
            "when GetLockedCopiesOfResourceForCiv would return a number less than or equal to zero " +
            "on the given arguments")]
        public void UnlockCopyOfResourceForCiv_ThrowsIfOperationInvalid() {
            var resource = BuildResource();
            var civ = BuildCiv();

            var lockingCanon = Container.Resolve<ResourceLockingCanon>();

            Assert.Throws<InvalidOperationException>(
                () => lockingCanon.UnlockCopyOfResourceForCiv(resource, civ),
                "UnlockCopyOfResourceForCiv failed to throw"
            );
        }

        #endregion

        #region utilities

        private ISpecialtyResourceDefinition BuildResource() {
            return new Mock<ISpecialtyResourceDefinition>().Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private  void ExecuteLockingSequence(
            ResourceLockingCanon lockingCanon, List<LockingType> sequence,
            ISpecialtyResourceDefinition resource, ICivilization civ
        ){
            foreach(var lockingOperation in sequence) {
                switch(lockingOperation) {
                    case LockingType.Lock:   lockingCanon.LockCopyOfResourceForCiv  (resource, civ); break;
                    case LockingType.Unlock: lockingCanon.UnlockCopyOfResourceForCiv(resource, civ); break;
                }
            }
        }

        #endregion

        #endregion

    }

}
