using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;
using Assets.Simulation.MapResources;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class CivilizationFactoryTests : ZenjectUnitTestFixture {

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<ICivilizationConfig>                          ().FromMock();
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromMock();
            Container.Bind<ICivilizationYieldLogic>                      ().FromMock();
            Container.Bind<ITechCanon>                                   ().FromMock();

            Container.Bind<CivilizationFactory>().AsSingle();

            Container.Bind<CivilizationSignals>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When a civilization is created, it should be initialized with " +
            "the argued template")]
        public void CivilizationCreated_NameInitialized() {
            var factory = Container.Resolve<CivilizationFactory>();

            var template = new Mock<ICivilizationTemplate>().Object;

            var newCivilization = factory.Create(template);

            Assert.AreEqual(template, newCivilization.Template, "newCivilization did not have the expected name");
        }
        
        [Test(Description = "When a civilization is created, it should be added to the " +
            "AllCivilizations collection")]
        public void CivilizationCreated_AddedToAllCivilizations() {
            var factory = Container.Resolve<CivilizationFactory>();

            var newCivilization = factory.Create(new Mock<ICivilizationTemplate>().Object);

            CollectionAssert.Contains(factory.AllCivilizations, newCivilization,
                "newCivilization did not appear in CivilizationFactory.AllCivilizations");
        }

        [Test(Description = "CivilizationFactory.Create should throw an ArgumentNullException " +
            "when passed a null template")]
        public void Create_ThrowsOnNullArgument() {
            var factory = Container.Resolve<CivilizationFactory>();

            Assert.Throws<ArgumentNullException>(() => factory.Create(null),
                "Create did not throw on a null name as expected");
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
