using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitProductionValidityLogicTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(UnitType.LandMilitary, new List<UnitType>())
                    .SetName("LandMilitary with no types forbidden")
                    .Returns(true);

                yield return new TestCaseData(UnitType.LandMilitary, new List<UnitType>() { UnitType.LandMilitary  })
                    .SetName("LandMilitary with LandMilitary forbidden")
                    .Returns(false);

                yield return new TestCaseData(UnitType.LandMilitary, new List<UnitType>() { UnitType.LandCivilian  })
                    .SetName("LandMilitary with LandCivilian forbidden")
                    .Returns(true);

                yield return new TestCaseData(UnitType.LandMilitary, new List<UnitType>() { UnitType.WaterMilitary })
                    .SetName("LandMilitary with WaterMilitary forbidden")
                    .Returns(true);

                yield return new TestCaseData(UnitType.LandMilitary, new List<UnitType>() { UnitType.WaterCivilian })
                    .SetName("LandMilitary with WaterCivilian forbidden")
                    .Returns(true);

                yield return new TestCaseData(UnitType.LandMilitary, new List<UnitType>() {
                    UnitType.LandCivilian, UnitType.WaterMilitary, UnitType.WaterCivilian
                }).SetName("LandMilitary with all other types forbidden").Returns(true);

                yield return new TestCaseData(UnitType.LandCivilian,  new List<UnitType>() { UnitType.LandCivilian  })
                    .SetName("LandCivilian with LandCivilian forbidden")
                    .Returns(false);

                yield return new TestCaseData(UnitType.WaterMilitary, new List<UnitType>() { UnitType.WaterMilitary })
                    .SetName("WaterMilitary with WaterMilitary forbidden")
                    .Returns(false);

                yield return new TestCaseData(UnitType.WaterCivilian, new List<UnitType>() { UnitType.WaterCivilian })
                    .SetName("WaterCivilian with WaterCivilian forbidden")
                    .Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockPositionCanon;

        private List<IUnitTemplate> AllTemplates = new List<IUnitTemplate>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllTemplates.Clear();

            MockPositionCanon = new Mock<IUnitPositionCanon>();
            
            Container.Bind<IUnitPositionCanon>().FromInstance(MockPositionCanon.Object);

            Container.Bind<IEnumerable<IUnitTemplate>>().WithId("Available Unit Templates").FromInstance(AllTemplates);

            Container.Bind<UnitProductionValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "IsTemplateValidForProductionInCity returns true if " +
            "IUnitPositionCanon.CanPlaceUnitOfTypeAtLocation returns true on the " +
            "type of the argued unit and the argued city")]
        [TestCaseSource("TestCases")]
        public bool IsTemplateValidForProductionInCity_ChecksPositionCanon(
            UnitType typeOfUnit, List<UnitType> typesNotPermitted
        ){
            var template = BuildTemplate(typeOfUnit);
            var city = BuildCity(typesNotPermitted);

            var validityLogic = Container.Resolve<UnitProductionValidityLogic>();

            return validityLogic.IsTemplateValidForCity(template, city);
        }

        [Test(Description = "GetTemplatesValidForCity should return all templates " +
            "in AllTemplates for which IsTemplateValidForProductionInCity returns true")]
        public void GetTemplatesValidForCity_ReturnsAllValidTemplates() {
            var city = BuildCity(new List<UnitType>() { UnitType.WaterCivilian, UnitType.LandMilitary });

            var landCivilianTemplate = BuildTemplate(UnitType.LandCivilian);
            var waterCivilianTemplate = BuildTemplate(UnitType.WaterCivilian);

            var validityLogic = Container.Resolve<UnitProductionValidityLogic>();

            var validTemplates = validityLogic.GetTemplatesValidForCity(city);

            CollectionAssert.Contains(validTemplates, landCivilianTemplate, "GetTemplatesValidForCity is missing an expected template");
            CollectionAssert.DoesNotContain(validTemplates, waterCivilianTemplate, "GetTemplatesValidForCity contains an unexpected template");
        }

        #endregion

        #region utilities

        private IUnitTemplate BuildTemplate(UnitType typeOfUnit) {
            var mockTemplate = new Mock<IUnitTemplate>();
            mockTemplate.Setup(template => template.Type).Returns(typeOfUnit);

            AllTemplates.Add(mockTemplate.Object);

            return mockTemplate.Object;
        }

        private ICity BuildCity(List<UnitType> typesNotPermitted) {
            var mockTile = new Mock<IHexCell>();

            MockPositionCanon
                .Setup(canon => canon.CanPlaceUnitOfTypeAtLocation(It.IsAny<UnitType>(), mockTile.Object, false))
                .Returns<UnitType, IHexCell, bool>((type, tile, ignoreOccupancy) => !typesNotPermitted.Contains(type));

            var mockCity = new Mock<ICity>();
            mockCity.Setup(city => city.Location).Returns(mockTile.Object);

            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
