using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.GameMap;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.Improvements {

    [TestFixture]
    public class ImprovementLocationCanonTests : ZenjectUnitTestFixture {

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<ImprovementLocationCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanChangeOwnerOfPossession should return false if the argued " +
            "tile already possesses an improvement")]
        public void CanChangeOwnerOfPossession_FalseIfOwnerAlreadyHasPossession() {
            var ownedImprovement = new Mock<IImprovement>().Object;
            var unownedImprovement = new Mock<IImprovement>().Object;

            var tile = new Mock<IMapTile>().Object;

            var locationCanon = Container.Resolve<ImprovementLocationCanon>();

            locationCanon.ChangeOwnerOfPossession(ownedImprovement, tile);

            Assert.IsFalse(locationCanon.CanChangeOwnerOfPossession(unownedImprovement, tile),
                "CanChangeOwnerOfPossession falsely permits a tile to have more than one improvement");
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
