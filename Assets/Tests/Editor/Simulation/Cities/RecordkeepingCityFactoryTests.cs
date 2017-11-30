using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Distribution;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class RecordkeepingCityFactoryTests : ZenjectUnitTestFixture {

        private Mock<IWorkerDistributionLogic> DistributionMock;

        [SetUp]
        public void CommonInstall() {
            var cityPrefab = new GameObject();
            cityPrefab.AddComponent<City>();

            Container.Bind<GameObject>().WithId("City Prefab").FromInstance(cityPrefab);

            DistributionMock = new Mock<IWorkerDistributionLogic>();

            Container.Bind<IWorkerDistributionLogic>().FromInstance(DistributionMock.Object);

            Container.Bind<RecordkeepingCityFactory>().AsSingle();
        }

    }

}
