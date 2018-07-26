using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Civilizations;
using Assets.Simulation.MapResources;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class FreeResourceLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetFreeCopiesOfResourceForCivTestData {

            public int ExtractedCopies;
            public int ImportedCopies;
            public int ExportedCopies;
            public int LockedCopies;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetFreeCopiesOfResourceForCivTestCases {
            get {
                yield return new TestCaseData(new GetFreeCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 0, ImportedCopies = 0,
                    ExportedCopies = 0, LockedCopies = 0
                }).SetName("Defaults to zero").Returns(0);

                yield return new TestCaseData(new GetFreeCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 5, ImportedCopies = 0,
                    ExportedCopies = 0, LockedCopies = 0
                }).SetName("Adds extracted copies").Returns(5);

                yield return new TestCaseData(new GetFreeCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 0, ImportedCopies = 4,
                    ExportedCopies = 0, LockedCopies = 0
                }).SetName("Adds imported copies").Returns(4);

                yield return new TestCaseData(new GetFreeCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 0, ImportedCopies = 0,
                    ExportedCopies = 3, LockedCopies = 0
                }).SetName("Subtracts exported copies").Returns(-3);

                yield return new TestCaseData(new GetFreeCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 0, ImportedCopies = 0,
                    ExportedCopies = 0, LockedCopies = 2
                }).SetName("Subtracts locked copies").Returns(-2);

                yield return new TestCaseData(new GetFreeCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 5, ImportedCopies = 4,
                    ExportedCopies = 3, LockedCopies = 2
                }).SetName("Sums all factors together sensibly").Returns(4);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IResourceExtractionLogic> MockExtractionLogic;
        private Mock<IResourceLockingCanon>    MockLockingCanon;
        private Mock<IResourceTransferCanon>   MockTransferCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockExtractionLogic = new Mock<IResourceExtractionLogic>();
            MockLockingCanon    = new Mock<IResourceLockingCanon>();
            MockTransferCanon   = new Mock<IResourceTransferCanon>();

            Container.Bind<IResourceExtractionLogic>().FromInstance(MockExtractionLogic.Object);
            Container.Bind<IResourceLockingCanon>   ().FromInstance(MockLockingCanon   .Object);
            Container.Bind<IResourceTransferCanon>  ().FromInstance(MockTransferCanon  .Object);

            Container.Bind<FreeResourcesLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetFreeCopiesOfResourceForCivTestCases")]
        public int GetFreeCopiesOfResourceForCivTests(GetFreeCopiesOfResourceForCivTestData testData) {
            var resource = BuildResource();
            var civ      = BuildCiv();

            MockExtractionLogic
                .Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, civ))
                .Returns(testData.ExtractedCopies);

            MockTransferCanon
                .Setup(canon => canon.GetImportedCopiesOfResourceForCiv(resource, civ))
                .Returns(testData.ImportedCopies);

            MockTransferCanon
                .Setup(canon => canon.GetExportedCopiesOfResourceForCiv(resource, civ))
                .Returns(testData.ExportedCopies);

            MockLockingCanon
                .Setup(canon => canon.GetLockedCopiesOfResourceForCiv(resource, civ))
                .Returns(testData.LockedCopies);

            var freeResourceLogic = Container.Resolve<FreeResourcesLogic>();

            return freeResourceLogic.GetFreeCopiesOfResourceForCiv(resource, civ);
        }

        #endregion

        #region utilities

        private IResourceDefinition BuildResource() {
            return new Mock<IResourceDefinition>().Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
