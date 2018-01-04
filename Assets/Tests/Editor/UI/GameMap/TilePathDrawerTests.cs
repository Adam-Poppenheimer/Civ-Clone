using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.UI.HexMap;

using Assets.Simulation.HexMap;

namespace Assets.Tests.UI.GameMap {

    public class TilePathDrawerTests : ZenjectUnitTestFixture {

        #region instance fields and properties



        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.BindMemoryPool<PathIndicator, PathIndicator.MemoryPool>().FromNewComponentOnNewGameObject();

            Container.Bind<TilePathDrawer>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When DrawPath is called, TilePathDrawer creates a single " +
            "indicator for each tile. It then sets that indicator's parent to some canvas " +
            "that is a child of the child the indicator is assigned to")]
        public void DrawPath_IndicatorsCreatedAndParented() {
            var pathToDraw = new List<IHexCell>() {
                BuildTile(), BuildTile(), BuildTile(), BuildTile(), BuildTile()
            };

            var pathDrawer = Container.Resolve<TilePathDrawer>();

            pathDrawer.DrawPath(pathToDraw);

            var indicatorPool = Container.Resolve<PathIndicator.MemoryPool>();

            Assert.AreEqual(pathToDraw.Count, indicatorPool.NumActive,
                "PathIndicator.MemoryPool has an unexpected number of active Indicators");

            foreach(var tile in pathToDraw) {
                Assert.AreEqual(1, tile.transform.childCount, 
                    string.Format("Tile {0} in pathToDraw doesn't have an indicator parented under it", tile));
            }
        }

        [Test(Description = "When DrawPath is called multiple times, TilePathDrawer will draw multiple paths, " +
            "each one with its own set of indicators, and without removing any existing paths.")]
        public void DrawPath_MultipleCallsCreateMultiplePaths() {
            var pathOne   = new List<IHexCell>() { BuildTile(), BuildTile() };
            var pathTwo   = new List<IHexCell>() { BuildTile(), BuildTile(), BuildTile() };
            var pathThree = new List<IHexCell>() { BuildTile(), BuildTile(), BuildTile(), BuildTile() };

            var pathDrawer = Container.Resolve<TilePathDrawer>();

            pathDrawer.DrawPath(pathOne);
            pathDrawer.DrawPath(pathTwo);
            pathDrawer.DrawPath(pathThree);

            foreach(var tile in pathOne.Concat(pathTwo).Concat(pathThree)) {
                Assert.AreEqual(1, tile.transform.childCount, 
                    string.Format("Tile {0} in pathToDraw doesn't have an indicator parented under it", tile));
            }
        }

        [Test(Description = "When ClearPath is called, all instantiated indicators are despawned")]
        public void ClearPath_AllIndicatorsDeactivated() {
            var pathOne   = new List<IHexCell>() { BuildTile(), BuildTile() };
            var pathTwo   = new List<IHexCell>() { BuildTile(), BuildTile(), BuildTile() };

            var pathDrawer = Container.Resolve<TilePathDrawer>();

            pathDrawer.DrawPath(pathOne);
            pathDrawer.DrawPath(pathTwo);

            pathDrawer.ClearAllPaths();

            var indicatorPool = Container.Resolve<PathIndicator.MemoryPool>();

            Assert.AreEqual(0, indicatorPool.NumActive, "PathIndicator.MemoryPool still has active Indicators");
            
        }

        #endregion

        #region utilities

        private IHexCell BuildTile() {
            var mockTile = new Mock<IHexCell>();

            var gameObject = new GameObject();
            gameObject.AddComponent<Canvas>();

            mockTile.Setup(tile => tile.transform).Returns(gameObject.transform);

            return mockTile.Object;
        }

        #endregion

        #endregion

    }

}
