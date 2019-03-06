using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class AlphamapMixingFunctionsTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICellAlphamapLogic> MockCellAlphamapLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCellAlphamapLogic = new Mock<ICellAlphamapLogic>();

            Container.Bind<ICellAlphamapLogic>().FromInstance(MockCellAlphamapLogic.Object);

            Container.Bind<AlphamapMixingFunctions>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void Selector_DefaultsToAlphamapForPosition() {
            var position = new Vector3(1f, 2f, 3f);

            var cell = BuildCell();

            var alphamaps = new float[] { 1f, 35f, -2.2f, 3.37f, 0f };

            MockCellAlphamapLogic.Setup(logic => logic.GetAlphamapForPointForCell(position, cell, HexDirection.E))
                                 .Returns(alphamaps);

            var mixingFunctions = Container.Resolve<AlphamapMixingFunctions>();

            CollectionAssert.AreEqual(alphamaps, mixingFunctions.Selector(position, cell, HexDirection.E, 1f));
        }

        [Test]
        public void Selector_MultipliesAllAlphamapElementsByWeight() {
            var position = new Vector3(1f, 2f, 3f);

            var cell = BuildCell();

            var alphamaps = new float[] { 1f, 35f, -2.2f, 3.37f, 0f };

            MockCellAlphamapLogic.Setup(logic => logic.GetAlphamapForPointForCell(position, cell, HexDirection.E))
                                 .Returns(alphamaps);

            var mixingFunctions = Container.Resolve<AlphamapMixingFunctions>();

            CollectionAssert.AreEqual(
                new float[] { 1f * 2.7f, 35f * 2.7f, -2.2f * 2.7f, 3.37f * 2.7f, 0f },
                mixingFunctions.Selector(position, cell, HexDirection.E, 2.7f)
            );
        }

        [Test]
        public void Aggregator_PerformsPairwiseAdditionAcrossAlphamaps() {
            var mapOne = new float[] { 1f,  35f,   -2.2f, 3.37f, 0f };
            var mapTwo = new float[] { 16f, 19.2f, -66f, -8.2f, -9f };

            var mixingFunctions = Container.Resolve<AlphamapMixingFunctions>();

            var sum = mixingFunctions.Aggregator(mapOne, mapTwo);

            for(int i = 0; i < sum.Length; i++) {
                Assert.AreEqual(mapOne[i] + mapTwo[i], sum[i], "Returned array has unexpected value at index " + i);
            }
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
