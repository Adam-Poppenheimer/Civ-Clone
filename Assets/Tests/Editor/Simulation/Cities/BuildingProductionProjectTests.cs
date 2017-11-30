using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    class BuildingProductionProjectTests : ZenjectUnitTestFixture {

        private Mock<IBuildingTemplate> MockTemplate;
        private Mock<IBuildingFactory> MockFactory;

        [SetUp]
        public void CommonInstall() {
            MockTemplate = new Mock<IBuildingTemplate>();
            MockFactory = new Mock<IBuildingFactory>();

            Container.Bind<IBuildingTemplate>().FromInstance(MockTemplate.Object);
            Container.Bind<IBuildingFactory>().FromInstance(MockFactory.Object);

            Container.Bind<BuildingProductionProject>().AsSingle();
        }

        [Test(Description = "When Execute is called, this project should call " +
            "BuildingFactory.Create on the template it was constructed with and the argued city")]
        public void ExecuteProject_BuildingFactoryCalled() {
            var city = new Mock<ICity>().Object;

            var project = Container.Resolve<BuildingProductionProject>();

            project.Execute(city);

            MockFactory.Verify(factory => factory.Create(MockTemplate.Object, city),
                "Execute did not call into BuildingFactory.Create correctly");
        }

    }

}
