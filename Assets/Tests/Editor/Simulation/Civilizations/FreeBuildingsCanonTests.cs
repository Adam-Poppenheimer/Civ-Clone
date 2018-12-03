using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Technology;

namespace Assets.Tests.Simulation.Civilizations {

    public class FreeBuildingsCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IFreeBuildingApplier>                          MockFreeBuildingApplier;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private CivilizationSignals                                 CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockFreeBuildingApplier = new Mock<IFreeBuildingApplier>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            CivSignals              = new CivilizationSignals();

            Container.Bind<IFreeBuildingApplier>                         ().FromInstance(MockFreeBuildingApplier.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<CivilizationSignals>                          ().FromInstance(CivSignals);

            Container.Bind<FreeBuildingsCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void SubscribeFreeBuildingToCiv_AndAppliesBuildingsToCitiesFalse_ReflectedInGetFreeBuildingsForCiv() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civ = BuildCiv(new List<ICity>());

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            CollectionAssert.AreEquivalent(
                new List<IEnumerable<IBuildingTemplate>>() { validTemplates },
                freeBuildingsCanon.GetFreeBuildingsForCiv(civ)
            );
        }

        [Test]
        public void SubscribeFreeBuildingToCiv_AndAppliesBuildingsToCitiesTrue_AppliesBuildingToSomeValidCity() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var cities = new List<ICity>() {
                BuildCity(), BuildCity(), BuildCity(),
            };

            var civ = BuildCiv(cities);

            MockFreeBuildingApplier.Setup(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, cities[1])
            ).Returns(true);

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = true;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            MockFreeBuildingApplier.Verify(
                applier => applier.ApplyFreeBuildingToCity(validTemplates, It.IsAny<ICity>()),
                Times.Once, "ApplyFreeBuildingToCity called an unexpected number of times"
            );

            MockFreeBuildingApplier.Verify(
                applier => applier.ApplyFreeBuildingToCity(validTemplates, cities[1]),
                Times.Once, "ApplyFreeBuildingToCity not called on cities[1]"
            );

            CollectionAssert.IsEmpty(
                freeBuildingsCanon.GetFreeBuildingsForCiv(civ),
                "GetFreeBuildingsForCiv isn't empty as expected"
            );
        }

        [Test]
        public void SubscribeFreeBuildingToCiv_AndAppliesBuildingsToCitiesTrue_AddsBuildingToListIfNoValidCity() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var cities = new List<ICity>() {
                BuildCity(), BuildCity(), BuildCity(),
            };

            var civ = BuildCiv(cities);

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = true;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            MockFreeBuildingApplier.Verify(
                applier => applier.ApplyFreeBuildingToCity(validTemplates, It.IsAny<ICity>()),
                Times.Never, "ApplyFreeBuildingToCity unexpectedly called"
            );

            CollectionAssert.AreEquivalent(
                new List<IEnumerable<IBuildingTemplate>>() { validTemplates },
                freeBuildingsCanon.GetFreeBuildingsForCiv(civ),
                "GetFreeBuildingsForCiv has an unexpected value"
            );
        }

        [Test]
        public void RemoveFreeBuildingFromCiv_RemovedFromGetFreeBuildingsForCiv() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civ = BuildCiv(new List<ICity>());

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            freeBuildingsCanon.RemoveFreeBuildingFromCiv (validTemplates, civ);

            CollectionAssert.IsEmpty(freeBuildingsCanon.GetFreeBuildingsForCiv(civ));
        }

        [Test]
        public void RemoveFreeBuildingFromCiv_OnlyRemovesOneCopyIfDuplicatesExist() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civ = BuildCiv(new List<ICity>());

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);
            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);
            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            freeBuildingsCanon.RemoveFreeBuildingFromCiv (validTemplates, civ);

            CollectionAssert.AreEquivalent(
                new List<IEnumerable<IBuildingTemplate>>() { validTemplates, validTemplates },
                freeBuildingsCanon.GetFreeBuildingsForCiv(civ)
            );
        }

        [Test]
        public void ClearForCiv_AllFreeBuildingsForCivCleared() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civOne = BuildCiv(new List<ICity>());

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civOne);
            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civOne);
            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civOne);

            freeBuildingsCanon.ClearForCiv(civOne);

            CollectionAssert.IsEmpty(freeBuildingsCanon.GetFreeBuildingsForCiv(civOne));
        }

        [Test]
        public void ClearForCiv_FreeBuildingsForOtherCivsNotCleared() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civOne = BuildCiv(new List<ICity>());
            var civTwo = BuildCiv(new List<ICity>());

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civOne);
            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civOne);

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civTwo);
            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civTwo);

            freeBuildingsCanon.ClearForCiv(civOne);

            CollectionAssert.AreEquivalent(
                new List<IEnumerable<IBuildingTemplate>>() { validTemplates, validTemplates },
                freeBuildingsCanon.GetFreeBuildingsForCiv(civTwo)
            );
        }

        [Test]
        public void Clear_AllFreeBuildingsForAllCivsCleared() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civOne = BuildCiv(new List<ICity>());
            var civTwo = BuildCiv(new List<ICity>());

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civOne);
            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civOne);

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civTwo);
            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civTwo);

            freeBuildingsCanon.Clear();

            CollectionAssert.IsEmpty(
                freeBuildingsCanon.GetFreeBuildingsForCiv(civOne), "CivOne buildings not cleared as expected"
            );
            CollectionAssert.IsEmpty(
                freeBuildingsCanon.GetFreeBuildingsForCiv(civTwo), "CivTwo buildings not cleared as expected"
            );
        }

        [Test]
        public void CivGainedCityFired_AndAppliesBuildingsToCitiesTrue_AttemptsToApplyBuildingsToCity() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civ = BuildCiv(new List<ICity>());

            var city = BuildCity();

            MockFreeBuildingApplier.Setup(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, city)
            ).Returns(true);

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            freeBuildingsCanon.ApplyBuildingsToCities = true;

            CivSignals.CivGainedCity.OnNext(new Tuple<ICivilization, ICity>(civ, city));

            MockFreeBuildingApplier.Verify(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, city),
                Times.Once, "Did not check for free building applicability"
            );

            MockFreeBuildingApplier.Verify(
                applier => applier.ApplyFreeBuildingToCity(validTemplates, city),
                Times.Once, "Did not apply free building to city"
            );
        }

        [Test]
        public void CivGainedCityFired_AndAppliesBuildingsToCitiesTrue_DoesntApplyIfApplicationNotValid() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civ = BuildCiv(new List<ICity>());

            var city = BuildCity();

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            freeBuildingsCanon.ApplyBuildingsToCities = true;

            CivSignals.CivGainedCity.OnNext(new Tuple<ICivilization, ICity>(civ, city));

            MockFreeBuildingApplier.Verify(
                applier => applier.ApplyFreeBuildingToCity(validTemplates, city),
                Times.Never, "Unexpectedly applied free building to city"
            );
        }

        [Test]
        public void CivGainedCityFired_AndAppliesBuildingsToCitiesTrue_AppliedBuildingsRemovedFromFreeBuildings() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civ = BuildCiv(new List<ICity>());

            var city = BuildCity();

            MockFreeBuildingApplier.Setup(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, city)
            ).Returns(true);

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            freeBuildingsCanon.ApplyBuildingsToCities = true;

            CivSignals.CivGainedCity.OnNext(new Tuple<ICivilization, ICity>(civ, city));

            CollectionAssert.IsEmpty(freeBuildingsCanon.GetFreeBuildingsForCiv(civ));
        }

        [Test]
        public void CivGainedCityFired_AndAppliesBuildingsToCitiesFalse_DoesNotAttemptToApplyBuildingsToCity() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var civ = BuildCiv(new List<ICity>());

            var city = BuildCity();

            MockFreeBuildingApplier.Setup(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, city)
            ).Returns(true);

            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            CivSignals.CivGainedCity.OnNext(new Tuple<ICivilization, ICity>(civ, city));

            MockFreeBuildingApplier.Verify(
                applier => applier.ApplyFreeBuildingToCity(validTemplates, city),
                Times.Never, "Unexpectedly applied free building to city"
            );
        }

        [Test]
        public void CivDiscoveredTechFired_AndAppliesBuildingsToCitiesTrue_AttemptsToApplyFreeBuildingsToCitiesOfCiv() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var cityOne = BuildCity();
            var cityTwo = BuildCity();

            MockFreeBuildingApplier.Setup(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, cityTwo)
            ).Returns(true);

            var civ = BuildCiv(new List<ICity>() { cityOne, cityTwo });

            var tech = BuildTech(validTemplates);
            
            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            freeBuildingsCanon.ApplyBuildingsToCities = true;

            CivSignals.CivDiscoveredTech.OnNext(new Tuple<ICivilization, ITechDefinition>(civ, tech));

            MockFreeBuildingApplier.Verify(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, cityOne), Times.Once,
                "Did not check cityOne for free building applicability"
            );

            MockFreeBuildingApplier.Verify(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, cityTwo), Times.Once,
                "Did not check cityTwo for free building applicability"
            );

            MockFreeBuildingApplier.Verify(
                applier => applier.ApplyFreeBuildingToCity(validTemplates, cityTwo), Times.Once,
                "Did not apply free building to cityTwo"
            );
        }

        [Test]
        public void CivDiscoveredTechFired_FreeBuildingRemovedIfApplied() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var city = BuildCity();

            MockFreeBuildingApplier.Setup(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, city)
            ).Returns(true);

            var civ = BuildCiv(new List<ICity>() { city });

            var tech = BuildTech(validTemplates);
            
            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            freeBuildingsCanon.ApplyBuildingsToCities = true;

            CivSignals.CivDiscoveredTech.OnNext(new Tuple<ICivilization, ITechDefinition>(civ, tech));

            CollectionAssert.IsEmpty(freeBuildingsCanon.GetFreeBuildingsForCiv(civ));
        }

        [Test]
        public void CivDiscoveredTechFired_AndAppliesBuildingsToCitiesTrue_DoesNothingIfTechDoesntEnableAnyFreeBuildings() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var city = BuildCity();

            MockFreeBuildingApplier.Setup(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, city)
            ).Returns(true);

            var civ = BuildCiv(new List<ICity>() { city });

            var tech = BuildTech(new List<IBuildingTemplate>());
            
            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            freeBuildingsCanon.ApplyBuildingsToCities = true;

            CivSignals.CivDiscoveredTech.OnNext(new Tuple<ICivilization, ITechDefinition>(civ, tech));

            CollectionAssert.IsNotEmpty(freeBuildingsCanon.GetFreeBuildingsForCiv(civ));
        }

        [Test]
        public void CivDiscoveredTechFired_AndAppliesBuildingsToCitiesFalse_DoesNotAttemptToApplyBuildings() {
            var validTemplates = new List<IBuildingTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            var city = BuildCity();

            MockFreeBuildingApplier.Setup(
                applier => applier.CanApplyFreeBuildingToCity(validTemplates, city)
            ).Returns(true);

            var civ = BuildCiv(new List<ICity>() { city });

            var tech = BuildTech(validTemplates);
            
            var freeBuildingsCanon = Container.Resolve<FreeBuildingsCanon>();

            freeBuildingsCanon.ApplyBuildingsToCities = false;

            freeBuildingsCanon.SubscribeFreeBuildingToCiv(validTemplates, civ);

            CivSignals.CivDiscoveredTech.OnNext(new Tuple<ICivilization, ITechDefinition>(civ, tech));

            CollectionAssert.IsNotEmpty(freeBuildingsCanon.GetFreeBuildingsForCiv(civ));
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildTemplate() {
            return new Mock<IBuildingTemplate>().Object;

        }

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        private ICivilization BuildCiv(List<ICity> cities) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            return newCiv;
        }

        private ITechDefinition BuildTech(IEnumerable<IBuildingTemplate> buildingsEnabled) {
            var mockTech = new Mock<ITechDefinition>();

            mockTech.Setup(tech => tech.BuildingsEnabled).Returns(buildingsEnabled);

            return mockTech.Object;
        }

        #endregion

        #endregion

    }

}
