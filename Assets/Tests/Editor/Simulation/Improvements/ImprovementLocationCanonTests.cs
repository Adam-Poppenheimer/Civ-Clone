using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.Improvements {

    [TestFixture]
    public class ImprovementLocationCanonTests : ZenjectUnitTestFixture {

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<ImprovementSignals>().AsSingle();
            Container.Bind<HexCellSignals>    ().AsSingle();

            Container.Bind<ImprovementLocationCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanChangeOwnerOfPossession should return false if the argued " +
            "tile already possesses an improvement")]
        public void CanChangeOwnerOfPossession_FalseIfOwnerAlreadyHasPossession() {
            var ownedImprovement   = BuildImprovement();
            var unownedImprovement = BuildImprovement();

            var tile = new Mock<IHexCell>().Object;

            var locationCanon = Container.Resolve<ImprovementLocationCanon>();

            locationCanon.ChangeOwnerOfPossession(ownedImprovement, tile);

            Assert.IsFalse(locationCanon.CanChangeOwnerOfPossession(unownedImprovement, tile),
                "CanChangeOwnerOfPossession falsely permits a tile to have more than one improvement");
        }

        [Test(Description = "CanPlaceImprovementOfTemplateAtLocation should return false if the argued " +
            "tile already possesses an improvement")]
        public void CanPlaceImprovementOfTemplateAtLocation_FalseIfLocationAlreadyHasPossession() {
            var ownedImprovement   = BuildImprovement();
            var unownedImprovement = BuildImprovement();

            var tile = new Mock<IHexCell>().Object;

            var locationCanon = Container.Resolve<ImprovementLocationCanon>();

            locationCanon.ChangeOwnerOfPossession(ownedImprovement, tile);

            Assert.IsFalse(locationCanon.CanPlaceImprovementOfTemplateAtLocation(unownedImprovement.Template, tile),
                "CanPlaceImprovementOfTemplateAtLocation falsely permits a tile to have more than one improvement");
        }

        #endregion

        #region utilities

        private IImprovement BuildImprovement() {
            var template = new Mock<IImprovementTemplate>().Object;

            var improvementMock = new Mock<IImprovement>();
            improvementMock.Setup(improvement => improvement.Template).Returns(template);

            return improvementMock.Object;
        }

        #endregion

        #endregion

    }

}
