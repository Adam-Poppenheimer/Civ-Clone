using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Simulation.Barbarians;

namespace Assets.Tests.Simulation.Barbarians {

    public class EncampmentLocationCanonTests : ZenjectUnitTestFixture {

        #region internal types

        public class CanCellAcceptAnEncampmentTestData {

            public HexCellTestData Cell;

        }

        public class HexCellTestData {

            public CellTerrain Terrain;
            public CellShape   Shape;

            public bool HasEncampment;
            public bool HasImprovement;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable CanCellAcceptAnEncampmentTestCases {
            get {
                yield return new TestCaseData(new CanCellAcceptAnEncampmentTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands
                    }
                }).SetName("Returns true on flat empty grassland").Returns(true);

                yield return new TestCaseData(new CanCellAcceptAnEncampmentTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Hills
                    }
                }).SetName("Returns true on hilly empty grassland").Returns(true);

                yield return new TestCaseData(new CanCellAcceptAnEncampmentTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Mountains
                    }
                }).SetName("Returns false on mountainous empty grassland").Returns(false);

                yield return new TestCaseData(new CanCellAcceptAnEncampmentTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.ShallowWater, Shape = CellShape.Flatlands
                    }
                }).SetName("Returns false on empty shallow water").Returns(false);

                yield return new TestCaseData(new CanCellAcceptAnEncampmentTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.DeepWater, Shape = CellShape.Flatlands
                    }
                }).SetName("Returns false on empty deep water").Returns(false);

                yield return new TestCaseData(new CanCellAcceptAnEncampmentTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.FreshWater, Shape = CellShape.Flatlands
                    }
                }).SetName("Returns false on empty fresh water").Returns(false);

                yield return new TestCaseData(new CanCellAcceptAnEncampmentTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        HasEncampment = true
                    }
                }).SetName("Returns false on flat grassland with an encampment").Returns(false);

                yield return new TestCaseData(new CanCellAcceptAnEncampmentTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        HasImprovement = true
                    }
                }).SetName("Returns false on flat grassland with an improvement").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();

            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);

            Container.Bind<EncampmentLocationCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("CanCellAcceptAnEncampmentTestCases")]
        public bool CanCellAcceptAnEncampmentTests(CanCellAcceptAnEncampmentTestData testData) {
            var locationCanon = Container.Resolve<EncampmentLocationCanon>();

            var cell = BuildCell(testData.Cell, locationCanon);

            return locationCanon.CanCellAcceptAnEncampment(cell);
        }

        #endregion

        #region utilities

        private IImprovement BuildImprovement() {
            return new Mock<IImprovement>().Object;
        }

        private IEncampment BuildEncampment() {
            return new Mock<IEncampment>().Object;
        }

        private IHexCell BuildCell(HexCellTestData cellData, EncampmentLocationCanon encampmentLocationCanon) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(cellData.Terrain);
            mockCell.Setup(cell => cell.Shape)  .Returns(cellData.Shape);

            var newCell = mockCell.Object;

            if(cellData.HasImprovement) {
                MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell))
                    .Returns(new List<IImprovement>() { BuildImprovement() });
            }

            if(cellData.HasEncampment) {
                encampmentLocationCanon.ChangeOwnerOfPossession(BuildEncampment(), newCell);
            }

            return newCell;
        }

        #endregion

        #endregion

    }

}
