using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class ResourceTransferCanonTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetTradeableCopiesOfResourceForCivTestData {

            public int ExtractedCopies;

            public int LockedCopies;

            public int ExportedCopies;

        }

        public class CanExportCopiesOfResourceTestData {

            public int ExtractedCopies;

            public int LockedCopies;

            public int ExportedCopies;

            public int CopiesToExport;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetTradeableCopiesOfResourceForCivTestCases {
            get {
                yield return new TestCaseData(new GetTradeableCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 0, ExportedCopies = 0, LockedCopies = 0
                }).SetName("Returns zero by default").Returns(0);

                yield return new TestCaseData(new GetTradeableCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 5, ExportedCopies = 0, LockedCopies = 0
                }).SetName("Increased by extracted copies").Returns(5);

                yield return new TestCaseData(new GetTradeableCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 5, ExportedCopies = 2, LockedCopies = 0
                }).SetName("Decreased by exported copies").Returns(3);

                yield return new TestCaseData(new GetTradeableCopiesOfResourceForCivTestData() {
                    ExtractedCopies = 5, ExportedCopies = 0, LockedCopies = 3
                }).SetName("Decreased by locked copies").Returns(2);
            }
        }

        public static IEnumerable CanExportCopiesOfResourceTestCases {
            get {
                yield return new TestCaseData(new CanExportCopiesOfResourceTestData() {
                    ExtractedCopies = 3, LockedCopies = 0, ExportedCopies = 0, CopiesToExport = 2
                }).SetName("Available copies are less than extracted copies, no locked or exported copies").Returns(true);

                yield return new TestCaseData(new CanExportCopiesOfResourceTestData() {
                    ExtractedCopies = 3, LockedCopies = 0, ExportedCopies = 0, CopiesToExport = 3
                }).SetName("Available copies are equal to extracted copies, no locked or exported copies").Returns(true);

                yield return new TestCaseData(new CanExportCopiesOfResourceTestData() {
                    ExtractedCopies = 3, LockedCopies = 0, ExportedCopies = 0, CopiesToExport = 4
                }).SetName("Available copies are greater than extracted copies").Returns(false);

                yield return new TestCaseData(new CanExportCopiesOfResourceTestData() {
                    ExtractedCopies = 3, LockedCopies = 2, ExportedCopies = 0, CopiesToExport = 2
                }).SetName("Exported copies greater than extracted minus locked").Returns(false);

                yield return new TestCaseData(new CanExportCopiesOfResourceTestData() {
                    ExtractedCopies = 3, LockedCopies = 0, ExportedCopies = 2, CopiesToExport = 2
                }).SetName("Exported copies greater than extracted minus exported").Returns(false);

                yield return new TestCaseData(new CanExportCopiesOfResourceTestData() {
                    ExtractedCopies = 3, LockedCopies = 1, ExportedCopies = 1, CopiesToExport = 2
                }).SetName("Exported copies greater than extracted minus locked minus exported").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IResourceExtractionLogic> MockExtractionLogic;
        private Mock<IResourceLockingCanon>    MockLockingCanon;

        private CivilizationSignals CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockExtractionLogic = new Mock<IResourceExtractionLogic>();
            MockLockingCanon    = new Mock<IResourceLockingCanon>();

            CivSignals = new CivilizationSignals();

            Container.Bind<IResourceExtractionLogic>().FromInstance(MockExtractionLogic.Object);
            Container.Bind<IResourceLockingCanon>   ().FromInstance(MockLockingCanon   .Object);

            Container.Bind<CivilizationSignals>().FromInstance(CivSignals);

            Container.Bind<ResourceTransferCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetTradeableCopiesOfResourceForCivTestCases")]
        public int GetTradeableCopiesOfResourceForCivTests(GetTradeableCopiesOfResourceForCivTestData testData) {
            var resource = BuildResource();
            var civ      = BuildCiv();

            var importingCiv = BuildCiv();

            MockExtractionLogic
                .Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, civ))
                .Returns(testData.ExtractedCopies);

            MockLockingCanon
                .Setup(canon => canon.GetLockedCopiesOfResourceForCiv(resource, civ))
                .Returns(testData.LockedCopies);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            transferCanon.ExportCopiesOfResource(resource, testData.ExportedCopies, civ, importingCiv);

            return transferCanon.GetTradeableCopiesOfResourceForCiv(resource, civ);
        }

        [Test]
        [TestCaseSource("CanExportCopiesOfResourceTestCases")]
        public bool CanExportCopiesOfResourceTests(CanExportCopiesOfResourceTestData testData) {
            var resource = BuildResource();

            var exporter = BuildCiv();
            var importer = BuildCiv();

            var otherCiv = BuildCiv();

            MockExtractionLogic
                .Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter))
                .Returns(testData.ExtractedCopies);

            MockLockingCanon
                .Setup(canon => canon.GetLockedCopiesOfResourceForCiv(resource, exporter))
                .Returns(testData.LockedCopies);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            transferCanon.ExportCopiesOfResource(resource, testData.ExportedCopies, exporter, otherCiv);

            return transferCanon.CanExportCopiesOfResource(resource, testData.CopiesToExport, exporter, importer);
        }

        [Test]
        public void ExportCopiesOfResource_ReturnsWellFormedTransfer() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            var transfer = transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);

            Assert.AreEqual(
                new ResourceTransfer(exporter, importer, resource, 3),
                transfer,
                "ExportCopiesOfResource returned an unexpected transfer"
            );
        }

        [Test]
        public void ExportCopiesOfResource_TransferAppearsInExportTransfers() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            var transfer = transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);

            CollectionAssert.Contains(
                transferCanon.GetAllExportTransfersFromCiv(exporter), transfer,
                "Export transfers for civ Exporter does not contain the newly-established transfer"
            );
        }

        [Test]
        public void ExportCopiesOfResource_TransferAppearsInImportTransfers() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            var transfer = transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);

            CollectionAssert.Contains(
                transferCanon.GetAllImportTransfersFromCiv(importer), transfer,
                "Import transfers for civ Importer does not contain the newly-established transfer"
            );
        }

        [Test]
        public void ExportCopiesOfResource_TransferReflectedInExportedCopies() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);

            Assert.AreEqual(
                3, transferCanon.GetExportedCopiesOfResourceForCiv(resource, exporter),
                "GetExportedCopiesOfResourceForCiv returned an unexpected value"
            );
        }

        [Test]
        public void ExportCopiesOfResource_TransferReflectedInImportedCopies() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);

            Assert.AreEqual(
                3, transferCanon.GetImportedCopiesOfResourceForCiv(resource, importer),
                "GetImportedCopiesOfResourceForCiv returned an unexpected value"
            );
        }

        [Test]
        public void ExportCopiesOfResource_ThrowsIfInvalid() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            Assert.Throws<InvalidOperationException>(
                () => transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer),
                "ExportCopiesOfResource did not throw on the arguments as expected"
            );
        }

        [Test]
        public void CancelTransfer_ReflectedInExportTransfers() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            var transfer = transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);
            transferCanon.CancelTransfer(transfer);

            CollectionAssert.DoesNotContain(
                transferCanon.GetAllExportTransfersFromCiv(exporter), transfer,
                "GetAllExportTransfersFromCiv still contains the canceled transfer"
            );
        }

        [Test]
        public void CancelTransfer_ReflectedInImportTransfers() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            var transfer = transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);
            transferCanon.CancelTransfer(transfer);

            CollectionAssert.DoesNotContain(
                transferCanon.GetAllImportTransfersFromCiv(importer), transfer,
                "GetAllImportTransfersFromCiv still contains the canceled transfer"
            );
        }

        [Test]
        public void CancelTransfer_ReflectedInImportedCopies() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            var transfer = transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);
            transferCanon.CancelTransfer(transfer);

            Assert.AreEqual(
                0, transferCanon.GetExportedCopiesOfResourceForCiv(resource, exporter),
                "GetExportedCopiesOfResourceForCiv returned an unexpected value"
            );
        }

        [Test]
        public void CancelTransfer_ReflectedInExportedCopies() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            var transfer = transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);
            transferCanon.CancelTransfer(transfer);

            Assert.AreEqual(
                0, transferCanon.GetImportedCopiesOfResourceForCiv(resource, importer),
                "GetExportedCopiesOfResourceForCiv returned an unexpected value"
            );
        }

        [Test]
        public void CancelTransfer_FiresResourceTransferCanceledSignal() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(4);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            var transfer = transferCanon.ExportCopiesOfResource(resource, 3, exporter, importer);

            CivSignals.ResourceTransferCanceledSignal.Subscribe(delegate(ResourceTransfer canceledTransfer) {
                Assert.AreEqual(transfer, canceledTransfer, "Event was fired on an incorrect transfer");
                Assert.Pass();
            });

            transferCanon.CancelTransfer(transfer);

            Assert.Fail("ResourceTransferCanceledSignal was never fired");
        }

        [Test(Description = "When SynchronizeResourceForCiv is called and GetTradeableCopiesOfResourceForCiv " +
            "is negative, it attempts to cancel export transfers from the given civ involving the given resource " +
            "to prevent tradeable copies from becoming negative. It should do this until " + 
            "GetTradeableCopiesOfResourceForCiv would return a nonnegative value or until there are no more export " +
            "transfers to cancel. The order of transfer cancellation is unspecified")]
        public void SynchronizeResourceForCiv_CancelsDealsToPreventNegativeTradeableCopies() {
            var resource = BuildResource();
            var exporter = BuildCiv();
            var importer = BuildCiv();

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(6);

            var transferCanon = Container.Resolve<ResourceTransferCanon>();

            transferCanon.ExportCopiesOfResource(resource, 2, exporter, importer);
            transferCanon.ExportCopiesOfResource(resource, 2, exporter, importer);
            transferCanon.ExportCopiesOfResource(resource, 2, exporter, importer);

            MockExtractionLogic.Setup(logic => logic.GetExtractedCopiesOfResourceForCiv(resource, exporter)).Returns(2);

            transferCanon.SynchronizeResourceForCiv(resource, exporter);

            Assert.AreEqual(
                1, transferCanon.GetAllExportTransfersFromCiv(exporter).Count(),
                "An unexpected number of export transfers remained for civ Exporter"
            );

            Assert.LessOrEqual(
                2, transferCanon.GetExportedCopiesOfResourceForCiv(resource, exporter),
                "GetExportedCopiesOfResourceForCiv(resource, exporter) has a higher value than expected"
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

        #endregion

        #endregion

    }

}
