using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.TestTools;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.HexMap {

    [TestFixture]
    public class HexFeatureManagerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<INoiseGenerator>       MockNoiseGenerator;
        private Mock<IFeatureLocationLogic> MockFeatureLocationLogic;
        private Mock<IFeaturePlacer>        MockCityFeaturePlacer;
        private Mock<IFeaturePlacer>        MockResourceFeaturePlacer;
        private Mock<IFeaturePlacer>        MockImprovementFeaturePlacer;
        private Mock<IFeaturePlacer>        MockRuinsFeaturePlacer;
        private Mock<IFeaturePlacer>        MockTreeFeaturePlacer;


        private Transform FeatureContainer;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockNoiseGenerator           = new Mock<INoiseGenerator>();
            MockFeatureLocationLogic     = new Mock<IFeatureLocationLogic>();
            MockCityFeaturePlacer        = new Mock<IFeaturePlacer>();
            MockResourceFeaturePlacer    = new Mock<IFeaturePlacer>();
            MockImprovementFeaturePlacer = new Mock<IFeaturePlacer>();
            MockRuinsFeaturePlacer       = new Mock<IFeaturePlacer>();
            MockTreeFeaturePlacer        = new Mock<IFeaturePlacer>();

            Container.Bind<INoiseGenerator>      ().FromInstance(MockNoiseGenerator      .Object);
            Container.Bind<IFeatureLocationLogic>().FromInstance(MockFeatureLocationLogic.Object);

            Container.Bind<IFeaturePlacer>().WithId("City Feature Placer")       .FromInstance(MockCityFeaturePlacer       .Object);
            Container.Bind<IFeaturePlacer>().WithId("Resource Feature Placer")   .FromInstance(MockResourceFeaturePlacer   .Object);
            Container.Bind<IFeaturePlacer>().WithId("Improvement Feature Placer").FromInstance(MockImprovementFeaturePlacer.Object);
            Container.Bind<IFeaturePlacer>().WithId("Ruins Feature Placer")      .FromInstance(MockRuinsFeaturePlacer      .Object);
            Container.Bind<IFeaturePlacer>().WithId("Tree Feature Placer")       .FromInstance(MockTreeFeaturePlacer       .Object);

            Container.Bind<HexFeatureManager>().AsSingle();
        }

        #endregion

        #region tests

        [UnityTest]
        public IEnumerator Clear_DestroysAllChildrenOfFeatureContainer() {
            FeatureContainer = new GameObject().transform;

            Container.Bind<Transform>().WithId("Feature Container").FromInstance(FeatureContainer);

            GameObject childOne = new GameObject(), childTwo = new GameObject(), childThree = new GameObject();

            childOne  .transform.SetParent(FeatureContainer);
            childTwo  .transform.SetParent(FeatureContainer);
            childThree.transform.SetParent(FeatureContainer);

            yield return null;

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.Clear();

            yield return null;

            Assert.AreEqual(0, FeatureContainer.childCount);
        }

        [Test]
        public void AddFeatureLocationsForCell_GetsCenterLocationsFromFeatureLocationLogic() {
            var cell = BuildCell();

            var centerLocations = new List<Vector3>() {
                new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3)
            };

            MockFeatureLocationLogic.Setup(logic => logic.GetCenterFeaturePoints(cell))
                                    .Returns(centerLocations);

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.AddFeatureLocationsForCell(cell);

            CollectionAssert.AreEquivalent(centerLocations, featureManager.GetFeatureLocationsForCell(cell));
        }

        [Test]
        public void AddFeatureLocationsForCell_GetsDirectionalLocationsFromFeatureLocationLogic() {
            var cell = BuildCell();

            var seLocations = new List<Vector3>() { new Vector3(3, 4), new Vector3(5, 6) };
            var nwLocations = new List<Vector3>() { new Vector3(1, 2) };            

            var otherLocations = new List<Vector3>();

            MockFeatureLocationLogic.Setup(logic => logic.GetDirectionalFeaturePoints(cell, HexDirection.NE)).Returns(otherLocations);
            MockFeatureLocationLogic.Setup(logic => logic.GetDirectionalFeaturePoints(cell, HexDirection.E)) .Returns(otherLocations);
            MockFeatureLocationLogic.Setup(logic => logic.GetDirectionalFeaturePoints(cell, HexDirection.SE)).Returns(seLocations);
            MockFeatureLocationLogic.Setup(logic => logic.GetDirectionalFeaturePoints(cell, HexDirection.SW)).Returns(otherLocations);
            MockFeatureLocationLogic.Setup(logic => logic.GetDirectionalFeaturePoints(cell, HexDirection.W)) .Returns(otherLocations);
            MockFeatureLocationLogic.Setup(logic => logic.GetDirectionalFeaturePoints(cell, HexDirection.NW)).Returns(nwLocations);

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.AddFeatureLocationsForCell(cell);

            CollectionAssert.AreEquivalent(
                seLocations.Concat(nwLocations),
                featureManager.GetFeatureLocationsForCell(cell)
            );
        }

        [Test]
        public void Apply_TriesToHandleEveryPointThroughCityFeaturePlacer() {
            var cell = BuildCell();

            var centerLocations = new List<Vector3>() {
                new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3)
            };

            MockFeatureLocationLogic.Setup(logic => logic.GetCenterFeaturePoints(cell))
                                    .Returns(centerLocations);

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.AddFeatureLocationsForCell(cell);

            featureManager.Apply();

            for(int i = 0; i < centerLocations.Count; ++i) {
                var location = centerLocations[i];

                MockCityFeaturePlacer.Verify(
                    placer => placer.TryPlaceFeatureAtLocation(cell, location, i, It.IsAny<HexHash>()),
                    Times.Once, 
                    string.Format("Did not call TryPlaceFeatureAtLocation on location {0} as expected", location)
                );
            }
        }

        [Test]
        public void Apply_PassesPointsToImprovementFeaturePlacerIfCityFeaturePlacerFails() {
            var cell = BuildCell();

            var locations = new List<Vector3>() {
                new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3)
            };

            MockFeatureLocationLogic.Setup(logic => logic.GetCenterFeaturePoints(cell))
                                    .Returns(locations);

            MockCityFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[0], 0, It.IsAny<HexHash>())).Returns(true);
            MockCityFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[1], 1, It.IsAny<HexHash>())).Returns(false);
            MockCityFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[2], 2, It.IsAny<HexHash>())).Returns(true);

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.AddFeatureLocationsForCell(cell);

            featureManager.Apply();

            MockImprovementFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[0], 0, It.IsAny<HexHash>()),
                Times.Never, "TryPlaceFeatureAtLocation unexpectedly called on locations[0]"
            );

            MockImprovementFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[1], 1, It.IsAny<HexHash>()),
                Times.Once, "TryPlaceFeatureAtLocation not called on locations[1] as expected"
            );

            MockImprovementFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[2], 2, It.IsAny<HexHash>()),
                Times.Never, "TryPlaceFeatureAtLocation unexpectedly called on locations[2]"
            );
        }

        [Test]
        public void Apply_PassesPointsToResourceFeaturePlacerIfImprovementFeaturePlacerFails() {
            var cell = BuildCell();

            var locations = new List<Vector3>() {
                new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3)
            };

            MockFeatureLocationLogic.Setup(logic => logic.GetCenterFeaturePoints(cell))
                                    .Returns(locations);

            MockImprovementFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[0], 0, It.IsAny<HexHash>())).Returns(true);
            MockImprovementFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[1], 1, It.IsAny<HexHash>())).Returns(false);
            MockImprovementFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[2], 2, It.IsAny<HexHash>())).Returns(true);

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.AddFeatureLocationsForCell(cell);

            featureManager.Apply();

            MockResourceFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[0], 0, It.IsAny<HexHash>()),
                Times.Never, "TryPlaceFeatureAtLocation unexpectedly called on locations[0]"
            );

            MockResourceFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[1], 1, It.IsAny<HexHash>()),
                Times.Once, "TryPlaceFeatureAtLocation not called on locations[1] as expected"
            );

            MockResourceFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[2], 2, It.IsAny<HexHash>()),
                Times.Never, "TryPlaceFeatureAtLocation unexpectedly called on locations[2]"
            );
        }

        [Test]
        public void Apply_PassesPointsToRuinsFeaturePlacerIfResourceFeaturePlacerFails() {
            var cell = BuildCell();

            var locations = new List<Vector3>() {
                new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3)
            };

            MockFeatureLocationLogic.Setup(logic => logic.GetCenterFeaturePoints(cell))
                                    .Returns(locations);

            MockResourceFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[0], 0, It.IsAny<HexHash>())).Returns(true);
            MockResourceFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[1], 1, It.IsAny<HexHash>())).Returns(false);
            MockResourceFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[2], 2, It.IsAny<HexHash>())).Returns(true);

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.AddFeatureLocationsForCell(cell);

            featureManager.Apply();

            MockRuinsFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[0], 0, It.IsAny<HexHash>()),
                Times.Never, "TryPlaceFeatureAtLocation unexpectedly called on locations[0]"
            );

            MockRuinsFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[1], 1, It.IsAny<HexHash>()),
                Times.Once, "TryPlaceFeatureAtLocation not called on locations[1] as expected"
            );

            MockRuinsFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[2], 2, It.IsAny<HexHash>()),
                Times.Never, "TryPlaceFeatureAtLocation unexpectedly called on locations[2]"
            );
        }

        [Test]
        public void Apply_PassesPointsToTreeFeaturePlacerIfRuinsFeaturePlacerFails() {
            var cell = BuildCell();

            var locations = new List<Vector3>() {
                new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3)
            };

            MockFeatureLocationLogic.Setup(logic => logic.GetCenterFeaturePoints(cell))
                                    .Returns(locations);

            MockRuinsFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[0], 0, It.IsAny<HexHash>())).Returns(true);
            MockRuinsFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[1], 1, It.IsAny<HexHash>())).Returns(false);
            MockRuinsFeaturePlacer.Setup(placer => placer.TryPlaceFeatureAtLocation(cell, locations[2], 2, It.IsAny<HexHash>())).Returns(true);

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.AddFeatureLocationsForCell(cell);

            featureManager.Apply();

            MockTreeFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[0], 0, It.IsAny<HexHash>()),
                Times.Never, "TryPlaceFeatureAtLocation unexpectedly called on locations[0]"
            );

            MockTreeFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[1], 1, It.IsAny<HexHash>()),
                Times.Once, "TryPlaceFeatureAtLocation not called on locations[1] as expected"
            );

            MockTreeFeaturePlacer.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, locations[2], 2, It.IsAny<HexHash>()),
                Times.Never, "TryPlaceFeatureAtLocation unexpectedly called on locations[2]"
            );
        }

        [Test]
        public void Apply_AllFeaturePositionsCleared() {
            var cell = BuildCell();

            var locations = new List<Vector3>() {
                new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3)
            };

            MockFeatureLocationLogic.Setup(logic => logic.GetCenterFeaturePoints(cell))
                                    .Returns(locations);

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.AddFeatureLocationsForCell(cell);

            featureManager.Apply();

            CollectionAssert.IsEmpty(featureManager.GetFeatureLocationsForCell(cell));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
