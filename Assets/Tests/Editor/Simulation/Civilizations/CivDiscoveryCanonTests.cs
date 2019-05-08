using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Civilizations {

    public class CivDiscoveryCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private CivilizationSignals CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            CivSignals = new CivilizationSignals();

            Container.Bind<CivilizationSignals>().FromInstance(CivSignals);

            Container.Bind<CivDiscoveryCanon>().AsSingle();
        }

        #endregion

        #region tests 

        [Test]
        public void GetCivsDiscoveredByCiv_DefaultsToAnEmptySet() {
            var civOne = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            CollectionAssert.IsEmpty(discoveryCanon.GetCivsDiscoveredByCiv(civOne));
        }

        [Test]
        public void HaveCivsDiscoveredEachOther_DefaultsToFalse() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civOne, civTwo));
            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civTwo, civOne));
        }

        [Test]
        public void CanEstablishDiscoveryBetweenCivs_TrueIfNoDiscoveryEstablished() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            Assert.IsTrue(discoveryCanon.CanEstablishDiscoveryBetweenCivs(civOne, civTwo));
            Assert.IsTrue(discoveryCanon.CanEstablishDiscoveryBetweenCivs(civTwo, civOne));
        }

        [Test]
        public void CanEstablishDiscoveryBetweenCivs_FalseIfDiscoveryAlreadyEstablished() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne, civTwo);

            Assert.IsFalse(discoveryCanon.CanEstablishDiscoveryBetweenCivs(civOne, civTwo));
            Assert.IsFalse(discoveryCanon.CanEstablishDiscoveryBetweenCivs(civTwo, civOne));
        }

        [Test]
        public void CanEstablishDiscoveryBetweenCivs_FalseIfArguedCivsAreTheSame() {
            var civOne = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            Assert.IsFalse(discoveryCanon.CanEstablishDiscoveryBetweenCivs(civOne, civOne));
        }

        [Test]
        public void EstablishDiscoveryBetweenCivs_ReflectedInGetCivsDiscoveredByCiv_OfBothArguedCivs() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne, civTwo);

            CollectionAssert.AreEquivalent(
                new List<ICivilization>() { civTwo }, discoveryCanon.GetCivsDiscoveredByCiv(civOne)
            );

            CollectionAssert.AreEquivalent(
                new List<ICivilization>() { civOne }, discoveryCanon.GetCivsDiscoveredByCiv(civTwo)
            );
        }

        [Test]
        public void EstablishDiscoveryBetweenCivs_ReflectedInHaveCivsDiscoveredEachOther_InBothDirections() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne, civTwo);

            Assert.IsTrue(discoveryCanon.HaveCivsDiscoveredEachOther(civOne, civTwo));
            Assert.IsTrue(discoveryCanon.HaveCivsDiscoveredEachOther(civTwo, civOne));
        }

        [Test]
        public void EstablishDiscoveryBetweenCivs_AndDiscoveryInvalid_ThrowsInvalidOperationException() {
            var civOne = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            Assert.Throws<InvalidOperationException>(
                () => discoveryCanon.EstablishDiscoveryBetweenCivs(civOne, civOne)
            );
        }

        [Test]
        public void CanRevokeDiscoveryBetweenCivs_TrueIfDiscoveryEstablished() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne, civTwo);

            Assert.IsTrue(discoveryCanon.CanRevokeDiscoveryBetweenCivs(civOne, civTwo));
            Assert.IsTrue(discoveryCanon.CanRevokeDiscoveryBetweenCivs(civTwo, civOne));
        }

        [Test]
        public void CanRevokeDiscoveryBetweenCivs_FalseIsDiscoveryNotEstablished() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            Assert.IsFalse(discoveryCanon.CanRevokeDiscoveryBetweenCivs(civOne, civTwo));
            Assert.IsFalse(discoveryCanon.CanRevokeDiscoveryBetweenCivs(civTwo, civOne));
        }

        [Test]
        public void RevokeDiscoveryBetweenCivs_ReflectedInGetCivsDiscoveredByCiv_OfBothArguedCivs() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne, civTwo);

            discoveryCanon.RevokeDiscoveryBetweenCivs(civOne, civTwo);

            CollectionAssert.IsEmpty(discoveryCanon.GetCivsDiscoveredByCiv(civOne));
            CollectionAssert.IsEmpty(discoveryCanon.GetCivsDiscoveredByCiv(civTwo));
        }

        [Test]
        public void RevokeDiscoveryBetweenCivs_ReflectedInHaveCivsDiscoveredEachOther_InBothDirections() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne, civTwo);

            discoveryCanon.RevokeDiscoveryBetweenCivs(civOne, civTwo);

            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civOne, civTwo));
            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civTwo, civOne));
        }

        [Test]
        public void RevokeDiscoveryBetweenCivs_AndRevocationInvalid_ThrowsInvalidOperationException() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            Assert.Throws<InvalidOperationException>(
                () => discoveryCanon.RevokeDiscoveryBetweenCivs(civOne, civTwo)
            );
        }

        [Test]
        public void GetDiscoveryPairs_ReturnsNonRedundantPairsOfCivsWhoveDiscoveredEachOther() {
            var civOne   = BuildCiv();
            var civTwo   = BuildCiv();
            var civThree = BuildCiv();
            var civFour  = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne,  civTwo);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne,  civThree);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne,  civFour);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civTwo,  civThree);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civFour, civThree);

            var pairs = discoveryCanon.GetDiscoveryPairs();

            Assert.AreEqual(5, pairs.Count, "Unexpected number of pairs");
            
            Assert.That(pairs.Any(pair => IsPairOfCivs(pair, civOne,  civTwo)),   "Lacked a pair between civOne and civTwo");
            Assert.That(pairs.Any(pair => IsPairOfCivs(pair, civOne,  civThree)), "Lacked a pair between civOne and civThree");
            Assert.That(pairs.Any(pair => IsPairOfCivs(pair, civOne,  civFour)),  "Lacked a pair between civOne and civFour");
            Assert.That(pairs.Any(pair => IsPairOfCivs(pair, civTwo,  civThree)), "Lacked a pair between civTwo and civThree");
            Assert.That(pairs.Any(pair => IsPairOfCivs(pair, civFour, civThree)), "Lacked a pair between civFour and civThree");
        }

        [Test]
        public void Clear_AllDiscoveryDataCleared() {
            var civOne   = BuildCiv();
            var civTwo   = BuildCiv();
            var civThree = BuildCiv();
            var civFour  = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne,  civTwo);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne,  civThree);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne,  civFour);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civTwo,  civThree);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civFour, civThree);

            discoveryCanon.Clear();

            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civOne,  civTwo),   "civOne and civTwo have still discovered each other");
            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civOne,  civThree), "civOne and civThree have still discovered each other");
            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civOne,  civFour),  "civOne and civFour have still discovered each other");
            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civTwo,  civThree), "civTwo and civThree have still discovered each other");
            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civFour, civThree), "civFour and civThree have still discovered each other");
        }

        [Test]
        public void CivDestroyedSignalCalled_AllDiscoveryRelationshipsWithThatCivRevoked() {
            var civOne   = BuildCiv();
            var civTwo   = BuildCiv();
            var civThree = BuildCiv();
            var civFour  = BuildCiv();

            var discoveryCanon = Container.Resolve<CivDiscoveryCanon>();

            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne, civTwo);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civOne, civThree);
            discoveryCanon.EstablishDiscoveryBetweenCivs(civTwo, civThree);

            CivSignals.CivBeingDestroyed.OnNext(civOne);

            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civOne, civTwo),   "CivOne and CivTwo have incorrectly discovered each-other");
            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civOne, civThree), "CivOne and CivThree have incorrectly discovered each-other");
            Assert.IsFalse(discoveryCanon.HaveCivsDiscoveredEachOther(civOne, civFour),  "CivOne and CivFour have incorrectly discovered each-other");

            Assert.IsTrue(discoveryCanon.HaveCivsDiscoveredEachOther(civTwo, civThree), "CivOne and CivTwo have not discovered each-other as expected");
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private bool IsPairOfCivs(UniRx.Tuple<ICivilization, ICivilization> pair, ICivilization firstCiv, ICivilization secondCiv) {
            return (pair.Item1 == firstCiv  && pair.Item2 == secondCiv)
                || (pair.Item1 == secondCiv && pair.Item2 == firstCiv);
        }

        #endregion

        #endregion

    }

}
