using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Territory;

using Assets.UI.HexMap;
using Assets.UI;

namespace Assets.Tests.UI.GameMap {

    [TestFixture]
    public class TileHoverDisplayTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Text TerrainTypeField;
        private Text TerrainShapeField;
        private Text TerrainFeatureField;

        private Mock<IResourceSummaryDisplay>  MockYieldDisplay;
        private Mock<IHexCellSignalLogic>      MockSignalLogic;

        private Mock<IResourceGenerationLogic> MockResourceLogic;
        private Mock<ITilePossessionCanon>     MockPossessionCanon;

        private ISubject<IHexCell> BeginHoverSubject;
        private ISubject<IHexCell> EndHoverSubject;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            TerrainTypeField    = Container.InstantiateComponentOnNewGameObject<Text>();
            TerrainShapeField   = Container.InstantiateComponentOnNewGameObject<Text>();
            TerrainFeatureField = Container.InstantiateComponentOnNewGameObject<Text>();

            Container.Bind<Text>().WithId("Terrain Type Field")   .FromInstance(TerrainTypeField   );
            Container.Bind<Text>().WithId("Terrain Shape Field")  .FromInstance(TerrainShapeField  );
            Container.Bind<Text>().WithId("Terrain Feature Field").FromInstance(TerrainFeatureField);

            MockYieldDisplay    = new Mock<IResourceSummaryDisplay>();
            MockSignalLogic     = new Mock<IHexCellSignalLogic>();

            MockResourceLogic   = new Mock<IResourceGenerationLogic>();
            MockPossessionCanon = new Mock<ITilePossessionCanon>();            

            BeginHoverSubject = new Subject<IHexCell>();
            EndHoverSubject   = new Subject<IHexCell>();
            
            MockSignalLogic.Setup(logic => logic.BeginHoverSignal).Returns(BeginHoverSubject);
            MockSignalLogic.Setup(logic => logic.EndHoverSignal)  .Returns(EndHoverSubject);

            Container.Bind<IResourceSummaryDisplay>().WithId("Tile Hover Yield Display").FromInstance(MockYieldDisplay.Object);
            Container.Bind<IHexCellSignalLogic>()                                       .FromInstance(MockSignalLogic.Object);

            Container.Bind<IResourceGenerationLogic>().FromInstance(MockResourceLogic.Object);
            Container.Bind<ITilePossessionCanon>()    .FromInstance(MockPossessionCanon.Object);

            Container.Bind<TileHoverDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When BeginHoverSignal is fired, TileHoverDisplay should activate its GameObject " +
            "and displays the information in the firing IMapTile in its various fields")]
        public void BeginHoverSignalFired_DisplayActivatesAndFillsFields() {
            var tile = BuildTile(
                Vector3.zero, TerrainType.Plains, TerrainShape.Hills, TerrainFeature.Forest,
                BuildSlot(ResourceSummary.Empty, ResourceSummary.Empty), null
            );

            var hoverDisplay = Container.Resolve<TileHoverDisplay>();
            hoverDisplay.gameObject.SetActive(false);

            BeginHoverSubject.OnNext(tile);

            Assert.IsTrue(hoverDisplay.gameObject.activeSelf, "TileHoverDisplay is not activated");

            Assert.AreEqual(TerrainTypeField.text, tile.Terrain.ToString(), 
                "TerrainTypeField.text had an unexpected value");

            Assert.AreEqual(TerrainShapeField.text, tile.Shape.ToString(),
                "TerrainShapeField.text had an unexpected value");

            Assert.AreEqual(TerrainFeatureField.text, tile.Feature.ToString(),
                "TerrainFeatureField.text had an unexpected value");
        }

        [Test(Description = "When BeginHoverSignal is fired and the firing tile is not owned " +
            "by any city, TileHoverDisplay should pass YieldDisplay the base yield of the tile")]
        public void BeginHoverSignalFired_AndTileUnowned_YieldDisplayGivenBaseYield() {
            var baseYield = new ResourceSummary(food: 1);
            var cityYield = new ResourceSummary(production: 2, gold: 3);

            var tile = BuildTile(
                Vector3.zero, TerrainType.Plains, TerrainShape.Hills, TerrainFeature.Forest,
                BuildSlot(baseYield, cityYield), null
            );

            Container.Resolve<TileHoverDisplay>();

            BeginHoverSubject.OnNext(tile);

            MockYieldDisplay.Verify(display => display.DisplaySummary(baseYield), Times.AtLeastOnce,
                "YieldDisplay was not told to display the tile's base yield");

            MockYieldDisplay.Verify(display => display.DisplaySummary(cityYield), Times.Never,
                "YieldDisplay was falsely told to display the tile's city yield");
        }

        [Test(Description = "When BeginHoverSignal is fired and the firing tile is owned " +
            "by a city, TileHoverDisplay should pass YieldDisplay the yield of that tile " +
            "for the city that owns it, as determined by ResourceGenerationLogic")]
        public void BeginHoverSignalFired_AndTileOwned_YieldDisplayGivenCityYield() {
            var baseYield = new ResourceSummary(food: 1);
            var cityYield = new ResourceSummary(production: 2, gold: 3);

            var city = new Mock<ICity>().Object;

            var tile = BuildTile(
                Vector3.zero, TerrainType.Plains, TerrainShape.Hills, TerrainFeature.Forest,
                BuildSlot(baseYield, cityYield), city
            );

            Container.Resolve<TileHoverDisplay>();

            BeginHoverSubject.OnNext(tile);

            MockYieldDisplay.Verify(display => display.DisplaySummary(baseYield), Times.Never,
                "YieldDisplay was falsely told to display the tile's base yield");

            MockYieldDisplay.Verify(display => display.DisplaySummary(cityYield), Times.AtLeastOnce,
                "YieldDisplay was not told to display the tile's city yield");
        }

        [Test(Description = "When BeginHoverSignal is fired, TileHoverDisplay should be repositioned " +
            "to the screen-space coordinates of its new tile")]
        public void BeginHoverSignalFired_DisplayRepositionedToTile() {
            var tile = BuildTile(
                new Vector3(1, -2, 3), TerrainType.Plains, TerrainShape.Flat, TerrainFeature.Forest,
                BuildSlot(ResourceSummary.Empty, ResourceSummary.Empty), null
            );

            var hoverDisplay = Container.Resolve<TileHoverDisplay>();

            BeginHoverSubject.OnNext(tile);

            var tileInScreenSpace = Camera.main.WorldToScreenPoint(tile.transform.position);

            Assert.AreEqual(tileInScreenSpace, hoverDisplay.transform.position,
                "TileHoverDisplay was not relocated to the screen-space coordinates of the tile it's displaying");
        }

        [Test(Description = "When EndHoverSignal is fired, TileHoverDisplay should deactivate its GameObject")]
        public void EndHoverSignalFired_DisplayDeactivates() {
            var tile = BuildTile(
                Vector3.zero, TerrainType.Plains, TerrainShape.Hills, TerrainFeature.Forest,
                BuildSlot(ResourceSummary.Empty, ResourceSummary.Empty), null
            );

            var hoverDisplay = Container.Resolve<TileHoverDisplay>();
            
            EndHoverSubject.OnNext(tile);

            Assert.IsFalse(hoverDisplay.gameObject.activeSelf, "TileHoverDisplay did not deactivate its gameObject");
        }

        #endregion

        #region utilities

        private IHexCell BuildTile(Vector3 position, TerrainType terrain,
            TerrainShape shape, TerrainFeature feature, IWorkerSlot slot,
            ICity owner
        ){
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();
            mockTile.Setup(tile => tile.WorkerSlot).Returns(slot);

            var newTransform = new GameObject().transform;
            newTransform.position = position;
            mockTile.Setup(tile => tile.transform).Returns(newTransform);

            var newTile = mockTile.Object;
            newTile.Terrain = terrain;
            newTile.Shape = shape;
            newTile.Feature = feature;

            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(newTile)).Returns(owner);

            return newTile;
        }

        private IWorkerSlot BuildSlot(ResourceSummary baseYield, ResourceSummary cityYield) {
            var slotMock = new Mock<IWorkerSlot>();

            slotMock.Setup(slot => slot.BaseYield).Returns(baseYield);

            MockResourceLogic
                .Setup(logic => logic.GetYieldOfSlotForCity(slotMock.Object, It.IsAny<ICity>()))
                .Returns(cityYield);

            return slotMock.Object;
        }

        #endregion

        #endregion

    }

}
