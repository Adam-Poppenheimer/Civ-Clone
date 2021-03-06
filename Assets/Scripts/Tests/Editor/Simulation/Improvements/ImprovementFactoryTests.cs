﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Improvements {

    [TestFixture]
    public class ImprovementFactoryTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private GameObject ImprovementPrefab;

        private Mock<IImprovementLocationCanon> MockLocationCanon;
        private Mock<IHexGrid>                  MockGrid;
        private Mock<ICellModificationLogic>    MockCellModificationLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            ImprovementPrefab = new GameObject("Improvement Prefab");
            ImprovementPrefab.AddComponent<Improvement>();

            MockLocationCanon         = new Mock<IImprovementLocationCanon>();
            MockGrid                  = new Mock<IHexGrid>();
            MockCellModificationLogic = new Mock<ICellModificationLogic>();

            Container.Bind<IImprovementLocationCanon>().FromInstance(MockLocationCanon        .Object);
            Container.Bind<IHexGrid>                 ().FromInstance(MockGrid                 .Object);
            Container.Bind<ICellModificationLogic>   ().FromInstance(MockCellModificationLogic.Object);

            Container.Bind<GameObject>().WithId("Improvement Prefab").FromInstance(ImprovementPrefab);

            Container.Bind<ImprovementSignals>().AsSingle();

            Container.Bind<ImprovementFactory>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When a new improvement is created, it should have its " +
            "Template property initialized with the argued template")]
        public void ImprovementCreated_TemplateInitialized() {
            var template = BuildTemplate();
            var location = BuildCell(true);

            var factory = Container.Resolve<ImprovementFactory>();

            var newImprovement = factory.BuildImprovement(template, location);

            Assert.AreEqual(template, newImprovement.Template,
                "newImprovement.Template has an unexpected value");
        }

        [Test(Description = "When a new improvement is created, ImprovementFactory " +
            "should assign that improvement to the argued tile via ImprovementLocationCanon")]
        public void ImprovementCreated_LocationSet() {
            var template = BuildTemplate();
            var location = BuildCell(true);

            var factory = Container.Resolve<ImprovementFactory>();

            var newImprovement = factory.BuildImprovement(template, location);

            MockLocationCanon.Verify(canon => canon.CanChangeOwnerOfPossession(newImprovement, location),
                "ImprovementFactory didn't check the assignment for validity before attempting it");

            MockLocationCanon.Verify(canon => canon.ChangeOwnerOfPossession(newImprovement, location),
                "ImprovementFactory failed to assign newImprovement to the argued location");
        }

        [Test(Description = "Create should throw an ArgumentNullException when passed a null argument")]
        public void CreateCalled_ThrowsOnNullArguments() {
            var template = BuildTemplate();
            var location = BuildCell(true);

            var factory = Container.Resolve<ImprovementFactory>();

            Assert.Throws<ArgumentNullException>(() => factory.BuildImprovement(null, location),
                "Create failed to throw on a null template argument");

            Assert.Throws<ArgumentNullException>(() => factory.BuildImprovement(template, null),
                "Create failed to throw on a null location argument");
        }

        [Test(Description = "Create should throw an ImprovementCreationException when the location it's passed " +
            "cannot accept the improvement it's created")]
        public void CreateCalled_ThrowsIfImprovementInvalidAtLocation() {
            var template = BuildTemplate();
            var location = BuildCell(false);

            var factory = Container.Resolve<ImprovementFactory>();

            Assert.Throws<ImprovementCreationException>(() => factory.BuildImprovement(template, location),
                "Create did not throw correctly when passed a location invalid for the new improvement");
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(bool validForImprovement) {
            var mockCell = new Mock<IHexCell>();

            MockLocationCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IImprovement>(), mockCell.Object))
                .Returns(validForImprovement);

            return mockCell.Object;
        }

        private IImprovementTemplate BuildTemplate() {
            return new Mock<IImprovementTemplate>().Object;
        }

        #endregion

        #endregion 

    }

}
