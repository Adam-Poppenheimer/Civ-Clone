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

        private List<Mock<IFeaturePlacer>> MockFeaturePlacers = new List<Mock<IFeaturePlacer>>();


        private Transform FeatureContainer;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockFeaturePlacers.Clear();

            MockNoiseGenerator       = new Mock<INoiseGenerator>();
            MockFeatureLocationLogic = new Mock<IFeatureLocationLogic>();

            Container.Bind<INoiseGenerator>      ().FromInstance(MockNoiseGenerator      .Object);
            Container.Bind<IFeatureLocationLogic>().FromInstance(MockFeatureLocationLogic.Object);

            Container.Bind<List<IFeaturePlacer>>().FromMethod(context => MockFeaturePlacers.Select(mock => mock.Object).ToList());

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
        public void Apply_PlacesFeaturesOnEachLocation_UsingFirstFeaturePlacerThatCanHandleIt() {
            var cell = BuildCell();

            var centerLocations = new List<Vector3>() {
                new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3)
            };

            MockFeatureLocationLogic.Setup(logic => logic.GetCenterFeaturePoints(cell))
                                    .Returns(centerLocations);

            var mockPlacerOne   = BuildMockFeaturePlacer(new Vector3(2, 2, 2));
            var mockPlacerTwo   = BuildMockFeaturePlacer(new Vector3(1, 1, 1), new Vector3(2, 2, 2));
            var mockPlacerThree = BuildMockFeaturePlacer(new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3));

            MockFeaturePlacers.Add(mockPlacerOne);
            MockFeaturePlacers.Add(mockPlacerTwo);
            MockFeaturePlacers.Add(mockPlacerThree);

            var featureManager = Container.Resolve<HexFeatureManager>();

            featureManager.AddFeatureLocationsForCell(cell);

            featureManager.Apply();

            mockPlacerOne.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, new Vector3(2, 2, 2), It.IsAny<int>(), It.IsAny<HexHash>()),
                Times.Once, "PlacerOne did not handle (2, 2, 2) as expected"
            );


            mockPlacerTwo.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, new Vector3(1, 1, 1), It.IsAny<int>(), It.IsAny<HexHash>()),
                Times.Once, "PlacerTwo did not handle (1, 1, 1) as expected"
            );

            mockPlacerTwo.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, new Vector3(2, 2, 2), It.IsAny<int>(), It.IsAny<HexHash>()),
                Times.Never, "PlacerTwo unexpectedly handled (2, 2, 2)"
            );


            mockPlacerThree.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, new Vector3(1, 1, 1), It.IsAny<int>(), It.IsAny<HexHash>()),
                Times.Never, "PlacerThree unexpectedly handled (1, 1, 1)"
            );

            mockPlacerThree.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, new Vector3(2, 2, 2), It.IsAny<int>(), It.IsAny<HexHash>()),
                Times.Never, "PlacerThree unexpectedly handled (2, 2, 2)"
            );

            mockPlacerThree.Verify(
                placer => placer.TryPlaceFeatureAtLocation(cell, new Vector3(3, 3, 3), It.IsAny<int>(), It.IsAny<HexHash>()),
                Times.Once, "PlacerThree did not handle (3, 3, 3) as expected"
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

        private Mock<IFeaturePlacer> BuildMockFeaturePlacer(params Vector3[] handledPoints) {
            var mockPlacer = new Mock<IFeaturePlacer>();

            foreach(var point in handledPoints) {
                mockPlacer.Setup(
                    placer => placer.TryPlaceFeatureAtLocation(
                        It.IsAny<IHexCell>(), point, It.IsAny<int>(), It.IsAny<HexHash>()
                    )
                ).Returns(true);
            }

            return mockPlacer;
        }

        #endregion

        #endregion

    }

}
