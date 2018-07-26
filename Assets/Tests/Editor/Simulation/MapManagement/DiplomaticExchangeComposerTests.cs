using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.MapResources;
using Assets.Simulation.MapManagement;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class DiplomaticExchangeComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<IHexCell, ICity>>  MockCityLocationCanon;
        private Mock<IDiplomaticExchangeFactory>                MockExchangeFactory;
        private Mock<IHexGrid>                                  MockGrid;

        private List<IResourceDefinition> AvailableResources = new List<IResourceDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall(){
            AvailableResources.Clear();

            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockExchangeFactory   = new Mock<IDiplomaticExchangeFactory>();
            MockGrid              = new Mock<IHexGrid>();

            MockExchangeFactory.Setup(
                factory => factory.BuildExchangeForType(It.IsAny<ExchangeType>())
            ).Returns<ExchangeType>(
                type => BuildExchange(type, 0, null, null)
            );

            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);
            Container.Bind<IDiplomaticExchangeFactory>              ().FromInstance(MockExchangeFactory  .Object);
            Container.Bind<IHexGrid>                                ().FromInstance(MockGrid             .Object);

            Container.Bind<IEnumerable<IResourceDefinition>>()
                     .WithId("Available Specialty Resources")
                     .FromInstance(AvailableResources);

            Container.Bind<DiplomaticExchangeComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ComposeExchange_AssignsTypeAndIntegerInputDirectly() {
            var exchange = BuildExchange(ExchangeType.GoldLumpSum, 20, null, null);

            var composer = Container.Resolve<DiplomaticExchangeComposer>();

            var serialExchange = composer.ComposeExchange(exchange);

            Assert.AreEqual(serialExchange.Type, ExchangeType.GoldLumpSum, "SerialExchange has an incorrect Type");
            Assert.AreEqual(serialExchange.IntegerInput, 20, "SerialExchange has an incorrect IntegerInput");
        }

        [Test]
        public void ComposeExchange_AssignsCityInputAsLocationCoordinates() {
            var city = BuildCity(BuildHexCell(new HexCoordinates(1, 2)));

            var exchange = BuildExchange(ExchangeType.City, 0, city, null);

            var composer = Container.Resolve<DiplomaticExchangeComposer>();

            var serialExchange = composer.ComposeExchange(exchange);

            Assert.AreEqual(new HexCoordinates(1, 2), serialExchange.CityInputLocation);
        }

        [Test]
        public void ComposeExchange_AssignsResourceInputAsResourceName() {
            var resource = BuildResource("Resource One");

            var exchange = BuildExchange(ExchangeType.Resource, 0, null, resource);

            var composer = Container.Resolve<DiplomaticExchangeComposer>();

            var serialExchange = composer.ComposeExchange(exchange);

            Assert.AreEqual("Resource One", serialExchange.ResourceInput);
        }

        [Test]
        public void DecomposeExchange_PassesTypeIntoExchangeFactory() {
            var serialExchange = new SerializableDiplomaticExchangeData() {
                Type = ExchangeType.Peace
            };

            var composer = Container.Resolve<DiplomaticExchangeComposer>();

            var exchange = composer.DecomposeExchange(serialExchange);

            Assert.AreEqual(ExchangeType.Peace, exchange.Type);
        }

        [Test]
        public void DecomposeExchange_AssignsIntegerInputCorrectly() {
            var serialExchange = new SerializableDiplomaticExchangeData() {
                Type = ExchangeType.Peace, IntegerInput = 32
            };

            var composer = Container.Resolve<DiplomaticExchangeComposer>();

            var exchange = composer.DecomposeExchange(serialExchange);

            Assert.AreEqual(32, exchange.IntegerInput);
        }

        [Test]
        public void DecomposeExchange_AssignsCityInputCorrectly() {
            var cityOne = BuildCity(BuildHexCell(new HexCoordinates(1, 2)));

            var serialExchange = new SerializableDiplomaticExchangeData() {
                Type = ExchangeType.Peace, CityInputLocation = new HexCoordinates(1, 2)
            };

            var composer = Container.Resolve<DiplomaticExchangeComposer>();

            var exchange = composer.DecomposeExchange(serialExchange);

            Assert.AreEqual(cityOne, exchange.CityInput);
        }

        [Test]
        public void DecomposeExchange_AssignsResourceInputCorrectly() {
            var resourceOne = BuildResource("Resource One");

            var serialExchange = new SerializableDiplomaticExchangeData() {
                Type = ExchangeType.Peace, ResourceInput = "Resource One"
            };

            var composer = Container.Resolve<DiplomaticExchangeComposer>();

            var exchange = composer.DecomposeExchange(serialExchange);

            Assert.AreEqual(resourceOne, exchange.ResourceInput);
        }

        #endregion

        #region utilities

        private IDiplomaticExchange BuildExchange(
            ExchangeType type, int integerInput, ICity cityInput,
            IResourceDefinition resourceInput
        ) {
            var mockExchange = new Mock<IDiplomaticExchange>();

            mockExchange.SetupAllProperties();
            mockExchange.Setup(exchange => exchange.Type).Returns(type);

            var newExchange = mockExchange.Object;

            newExchange.IntegerInput  = integerInput;
            newExchange.CityInput     = cityInput;
            newExchange.ResourceInput = resourceInput;

            return newExchange;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                 .Returns(new List<ICity>() { newCity });

            return newCity;
        }

        private IHexCell BuildHexCell(HexCoordinates coordinates) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Coordinates).Returns(coordinates);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(coordinates)).Returns(newCell);

            return newCell;
        }

        private IResourceDefinition BuildResource(string name) {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Name = name;
            mockResource.Setup(resource => resource.name).Returns(name);

            var newResource = mockResource.Object;

            AvailableResources.Add(newResource);

            return newResource;
        }

        #endregion

        #endregion

    }

}
