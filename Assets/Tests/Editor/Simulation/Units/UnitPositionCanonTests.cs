using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.GameMap;
using Assets.Simulation.Units;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitPositionCanonTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable PossessionChangeFailureCases {
            get {
                yield return new TestCaseData(
                    new Tuple<string, UnitType>("Unit to Consider", UnitType.LandMilitary),
                    new List<Tuple<string, UnitType>>() {
                        new Tuple<string, UnitType>("Land Military Unit", UnitType.LandMilitary)
                    }                    
                ).SetName("Placing LandMilitary when LandMilitary already present");

                yield return new TestCaseData(
                    new Tuple<string, UnitType>("Unit to Consider", UnitType.LandCivilian),
                    new List<Tuple<string, UnitType>>() {
                        new Tuple<string, UnitType>("Land Civilian Unit", UnitType.LandCivilian)
                    }                    
                ).SetName("Placing LandCivilian when LandCivilian already present");;
            }
        }

        private static IEnumerable PossessionChangeSuccessCases {
            get {
                yield return new TestCaseData(
                    new Tuple<string, UnitType>("Unit to Consider", UnitType.LandMilitary),
                    new List<Tuple<string, UnitType>>() {
                        new Tuple<string, UnitType>("Land Civilian Unit", UnitType.LandCivilian)
                    }                    
                ).SetName("Placing LandMilitary when LandCivilian already present");

                yield return new TestCaseData(
                    new Tuple<string, UnitType>("Unit to Consider", UnitType.LandCivilian),
                    new List<Tuple<string, UnitType>>() {
                        new Tuple<string, UnitType>("Land Military Unit", UnitType.LandMilitary)
                    }                    
                ).SetName("Placing LandCivilian when LandMilitary already present");;
            }
        }

        private static IEnumerable TerrainConsiderationCases {
            get {
                yield return new TestCaseData(UnitType.LandMilitary, TerrainType.ShallowWater).Returns(false).SetName("No Land Military on Shallow Water");
                yield return new TestCaseData(UnitType.LandMilitary, TerrainType.DeepWater   ).Returns(false).SetName("No Land Military on Deep Water");
                yield return new TestCaseData(UnitType.LandCivilian, TerrainType.ShallowWater).Returns(false).SetName("No Land Civilian on Shallow Water");
                yield return new TestCaseData(UnitType.LandCivilian, TerrainType.DeepWater   ).Returns(false).SetName("No Land Civilian on Deep Water");

                yield return new TestCaseData(UnitType.WaterMilitary, TerrainType.Grassland).Returns(false).SetName("No Water Military on Grassland");
                yield return new TestCaseData(UnitType.WaterMilitary, TerrainType.Plains   ).Returns(false).SetName("No Water Military on Plains");
                yield return new TestCaseData(UnitType.WaterMilitary, TerrainType.Desert   ).Returns(false).SetName("No Water Military on Desert");
                yield return new TestCaseData(UnitType.WaterCivilian, TerrainType.Grassland).Returns(false).SetName("No Water Civilian on Grassland");
                yield return new TestCaseData(UnitType.WaterCivilian, TerrainType.Plains   ).Returns(false).SetName("No Water Civilian on Plains");
                yield return new TestCaseData(UnitType.WaterCivilian, TerrainType.Desert   ).Returns(false).SetName("No Water Civilian on Desert");
            }
        }

        private static IEnumerable ShapeConsiderationCases {
            get {
                yield return new TestCaseData(UnitType.LandMilitary,  TerrainType.Grassland, TerrainShape.Mountains).Returns(false).SetName("No Land Military on Mountains");
                yield return new TestCaseData(UnitType.LandCivilian,  TerrainType.Grassland, TerrainShape.Mountains).Returns(false).SetName("No Land Civilian on Mountains");
                yield return new TestCaseData(UnitType.WaterMilitary, TerrainType.DeepWater, TerrainShape.Mountains).Returns(false).SetName("No Water Military on Mountains");
                yield return new TestCaseData(UnitType.WaterCivilian, TerrainType.DeepWater, TerrainShape.Mountains).Returns(false).SetName("No Water Civilian on Mountains");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICityFactory> MockCityFactory;

        private UnitSignals UnitSignals;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityFactory = new Mock<ICityFactory>();
            MockCityFactory.Setup(factory => factory.AllCities).Returns(() => AllCities.AsReadOnly());

            Container.Bind<ICityFactory>().FromInstance(MockCityFactory.Object);

            Container
                .Bind<List<TerrainType>>()
                .WithId("Land Terrain Types")
                .FromInstance(new List<TerrainType>() { TerrainType.Grassland, TerrainType.Plains, TerrainType.Desert });

            Container
                .Bind<List<TerrainShape>>()
                .WithId("Impassable Terrain Types")
                .FromInstance(new List<TerrainShape>() { TerrainShape.Mountains });

            Container.Bind<UnitPositionCanon>().AsSingle();

            UnitSignals = new UnitSignals();
            Container.Bind<UnitSignals>().FromInstance(UnitSignals);
        }

        #endregion

        #region tests

        [Test(Description = "CanChangeOwnerOfPossession should return false if there already exists a " +
            "Unit whose UnitType matches that of the Unit in question")]
        [TestCaseSource("PossessionChangeFailureCases")]
        public void CanChangeOwnerOfPossession_FalseIfTypeAlreadyRepresented(
            Tuple<string, UnitType> consideredUnitData,
            List<Tuple<string, UnitType>> presentUnitsData
        ){
            var location = BuildTile(TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None);

            var consideredUnit = BuildUnit(consideredUnitData);

            var presentUnits = presentUnitsData.Select(data => BuildUnit(data));

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            foreach(var unit in presentUnits) {
                positionCanon.ChangeOwnerOfPossession(unit, location);
            }

            Assert.IsFalse(positionCanon.CanChangeOwnerOfPossession(consideredUnit, location),
                "CanChangeOwnerOfPossession falsely permitted the repositioning of consideredUnit");
        }

        [Test(Description = "CanChangeOwnerOfPossession should not permit the placement of " + 
            "any land unit on any water terrain, nor the placement of any water unit on any " +
            "land terrain")]
        [TestCaseSource("TerrainConsiderationCases")]
        public bool CanChangeOwnerOfPossession_ConsidersTerrain(UnitType unitType, TerrainType terrain) {
            var unit = BuildUnit(unitType.ToString(), unitType);
            var tile = BuildTile(terrain, TerrainShape.Flat, TerrainFeatureType.None);

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            return positionCanon.CanChangeOwnerOfPossession(unit, tile);
        }

        [Test(Description = "CanChangeOwnerOfPossession should not permit the placement of " +
            "any unit on a tile whose shape is Mountains")]
        [TestCaseSource("ShapeConsiderationCases")]
        public bool CanChangeOwnerOfPossession_ConsidersShape(UnitType unitType, TerrainType terrain,
            TerrainShape shape
        ){
            var unit = BuildUnit(unitType.ToString(), unitType);
            var tile = BuildTile(terrain, shape, TerrainFeatureType.None);

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            return positionCanon.CanChangeOwnerOfPossession(unit, tile);
        }

        [Test(Description = "CanChangeOwnerOfPossession should permit water units to go on " +
            "any tile with a city, even if that tile is a land tile")]
        public void CanChangeOwnerOfPossession_WaterCanGoOnCity() {
            var waterMilitary = BuildUnit("Water Military", UnitType.WaterMilitary);
            var waterCivilian = BuildUnit("Water Civilian", UnitType.WaterMilitary);

            var location = BuildTile(TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None);
            BuildCity(location);

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            Assert.IsTrue(positionCanon.CanChangeOwnerOfPossession(waterMilitary, location),
                "CanChangeOwnerOfPossession failed to permit a Water Military unit from " +
                "moving onto a city");

            Assert.IsTrue(positionCanon.CanChangeOwnerOfPossession(waterCivilian, location),
                "CanChangeOwnerOfPossession failed to permit a Water Civilian unit from " +
                "moving onto a city");
        }        

        [Test(Description = "CanChangeOwnerOfPossession should return true if there are no Units " +
            "whose UnitType matches that of the Unit in question")]
        [TestCaseSource("PossessionChangeSuccessCases")]
        public void CanChangeOwnerOfPossession_TrueIfTypeNotRepresented(
            Tuple<string, UnitType> consideredUnitData,
            List<Tuple<string, UnitType>> presentUnitsData
        ){
            var location = BuildTile(TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None);

            var consideredUnit = BuildUnit(consideredUnitData);

            var presentUnits = presentUnitsData.Select(data => BuildUnit(data));

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            foreach(var unit in presentUnits) {
                positionCanon.ChangeOwnerOfPossession(unit, location);
            }

            Assert.IsTrue(positionCanon.CanChangeOwnerOfPossession(consideredUnit, location),
                "CanChangeOwnerOfPossession failed to permit the repositioning of consideredUnit");
        }

        [Test(Description = "ChangeOwnerOfPossession should set the parent of the relocated unit " +
            "to the transform of its new location, or null if its new location is null")]
        public void ChangeOwnerOfPossession_SetsParentToOwnerTransform() {
            var location = BuildTile(TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None);

            var unit = BuildUnit("Test Unit", UnitType.LandMilitary);

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            positionCanon.ChangeOwnerOfPossession(unit, location);

            Assert.AreEqual(location.transform, unit.gameObject.transform.parent,
                "Moving a unit to a location resulted in an unexpected parenting relationship");

            positionCanon.ChangeOwnerOfPossession(unit, null);
            Assert.Null(unit.gameObject.transform.parent, 
                "Moving a unit away from a location resulted in an unexpected parenting relationship");
        }

        [Test(Description = "Whenever a unit has its possession changed, UnitPositionCanon should fire " +
            "UnitSignals.UnitLocationChangedSignal with the unit and its new location")]
        public void ChangeOwnerOfPossession_LocationChangedSignalFired() {
            var location = BuildTile(TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None);

            var unit = BuildUnit("Test Unit", UnitType.LandMilitary);

            var positionCanon = Container.Resolve<UnitPositionCanon>();

            UnitSignals.UnitLocationChangedSignal.Subscribe(delegate(Tuple<IUnit, IMapTile> dataTuple) {
                Assert.AreEqual(dataTuple.Item1, unit,     "UnitLocationChangedSignal was fired on an unexpected unit");
                Assert.AreEqual(dataTuple.Item2, location, "UnitLocationChangedSignal was fired on an unexpected location");

                Assert.Pass();
            });

            positionCanon.ChangeOwnerOfPossession(unit, location);

            Assert.Fail("UnitLocationChangedSignal was not fired when location was non-null");

            location = null;

            positionCanon.ChangeOwnerOfPossession(unit, location);

            Assert.Fail("UnitLocationChangedSignal was not fired when location was null");
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(Tuple<string, UnitType> dataTuple) {
            return BuildUnit(dataTuple.Item1, dataTuple.Item2);
        }

        private IUnit BuildUnit(string name, UnitType type) {
            var mockUnit = new Mock<IUnit>();
            mockUnit.Setup(unit => unit.gameObject).Returns(new GameObject());

            var mockTemplate = new Mock<IUnitTemplate>();
            mockTemplate.Setup(template => template.Name).Returns(name);
            mockTemplate.Setup(template => template.Type).Returns(type);

            mockUnit.Setup(unit => unit.Template).Returns(mockTemplate.Object);
            return mockUnit.Object;
        }

        private IMapTile BuildTile(TerrainType terrain, TerrainShape shape, TerrainFeatureType feature) {
            var mockTile = new Mock<IMapTile>();
            mockTile.SetupAllProperties();

            mockTile.Setup(tile => tile.transform).Returns(new GameObject().transform);

            var newTile = mockTile.Object;

            newTile.Terrain = terrain;
            newTile.Shape   = shape;
            newTile.Feature = feature;

            return newTile;
        }

        private ICity BuildCity(IMapTile location) {
            var mockCity = new Mock<ICity>();
            mockCity.Setup(city => city.Location).Returns(location);

            AllCities.Add(mockCity.Object);

            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
