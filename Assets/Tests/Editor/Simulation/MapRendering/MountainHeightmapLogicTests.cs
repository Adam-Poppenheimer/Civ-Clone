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
using Assets.Util;

namespace Assets.Tests.Simulation.MapRendering {

    public class MountainHeightmapLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            
        }

        #endregion

        #region tests

        [Test]
        public void MissingTests() {
            throw new NotImplementedException();
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(CellShape shape) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Shape).Returns(shape);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
