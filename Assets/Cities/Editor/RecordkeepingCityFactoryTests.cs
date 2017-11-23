using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

namespace Assets.Cities.Editor {

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
