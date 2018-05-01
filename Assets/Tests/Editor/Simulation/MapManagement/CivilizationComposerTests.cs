using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Core;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class CivilizationComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationFactory> MockCivilizationFactory;
        private Mock<ITechCanon>           MockTechCanon;
        private Mock<IGameCore>            MockGameCore;

        private List<ITechDefinition> AvailableTechs = new List<ITechDefinition>();

        private List<ICivilization> AllCivilizations = new List<ICivilization>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTechs  .Clear();
            AllCivilizations.Clear();

            MockCivilizationFactory = new Mock<ICivilizationFactory>();
            MockTechCanon           = new Mock<ITechCanon>();
            MockGameCore            = new Mock<IGameCore>();

            MockCivilizationFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivilizations.AsReadOnly());

            MockCivilizationFactory.Setup(
                factory => factory.Create(It.IsAny<string>(), It.IsAny<Color>())
            ).Returns<string, Color>(
                (name, color) => BuildCivilization(name, color, 0, 0)
            );

            Container.Bind<ICivilizationFactory>().FromInstance(MockCivilizationFactory.Object);
            Container.Bind<ITechCanon>          ().FromInstance(MockTechCanon          .Object);
            Container.Bind<IGameCore>           ().FromInstance(MockGameCore           .Object);

            Container.Bind<List<ITechDefinition>>().WithId("Available Techs").FromInstance(AvailableTechs);

            Container.Bind<CivilizationComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearRuntime_AllCivsDestroyed() {
            Mock<ICivilization> mockOne, mockTwo, mockThree;
            
            BuildCivilization(out mockOne);
            BuildCivilization(out mockTwo);
            BuildCivilization(out mockThree);

            var composer = Container.Resolve<CivilizationComposer>();

            composer.ClearRuntime();

            mockOne  .Verify(civ => civ.Destroy(), Times.Once, "CivOne was not destroyed as expected");
            mockTwo  .Verify(civ => civ.Destroy(), Times.Once, "CivTwo was not destroyed as expected");
            mockThree.Verify(civ => civ.Destroy(), Times.Once, "CivThree was not destroyed as expected");
        }

        [Test]
        public void ComposeCivilizations_RecordsIntrinsicCivilizationData() {
            var civOne = BuildCivilization("Civ One", Color.black, 1, 10);
            var civTwo = BuildCivilization("Civ Two", Color.white, 2, 20);

            MockGameCore.Setup(core => core.ActiveCivilization).Returns(civOne);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CivilizationComposer>();

            composer.ComposeCivilizations(mapData);

            var dataLikeCivOne = mapData.Civilizations.Where(
                civ => civ.Name          .Equals(civOne.Name)  &&
                       ((Color)civ.Color).Equals(civOne.Color) &&
                       civ.GoldStockpile    == civOne.GoldStockpile &&
                       civ.CultureStockpile == civOne.CultureStockpile
            );

            var dataLikeCivTwo = mapData.Civilizations.Where(
                civ => civ.Name          .Equals(civTwo.Name)  &&
                       ((Color)civ.Color).Equals(civTwo.Color) &&
                       civ.GoldStockpile    == civTwo.GoldStockpile &&
                       civ.CultureStockpile == civTwo.CultureStockpile
            );

            Assert.AreEqual(1, dataLikeCivOne.Count(), "Did not find the expected number of serializable civs like civOne");
            Assert.AreEqual(1, dataLikeCivTwo.Count(), "Did not find the expected number of serializable civs like civTwo");
        }

        [Test]
        public void ComposeCivilizations_RecordsTechQueue() {
            var civOne = BuildCivilization("Civ One", Color.black, 0, 0);

            var queuedTechOne = BuildTechDefinition("Queued One");
            var queuedTechTwo = BuildTechDefinition("Queued Two");

            BuildTechDefinition("Unqueued");

            civOne.TechQueue.Enqueue(queuedTechOne);
            civOne.TechQueue.Enqueue(queuedTechTwo);

            MockGameCore.Setup(core => core.ActiveCivilization).Returns(civOne);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CivilizationComposer>();

            composer.ComposeCivilizations(mapData);

            CollectionAssert.AreEqual(
                new List<string>() { "Queued One", "Queued Two" },
                mapData.Civilizations[0].TechQueue,
                "Serializable civilization data has an unexpected tech queue"
            );
        }

        [Test]
        public void ComposeCivilizations_RecordsDiscoveredTechs() {
            var civOne = BuildCivilization("Civ One", Color.black, 0, 0);

            var discoveredTechs = new List<ITechDefinition>() {
                BuildTechDefinition("Discovered One"), BuildTechDefinition("Discovered Two")
            };

            BuildTechDefinition("Undiscovered");

            MockTechCanon.Setup(canon => canon.GetTechsDiscoveredByCiv(civOne)).Returns(discoveredTechs);

            MockGameCore.Setup(core => core.ActiveCivilization).Returns(civOne);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CivilizationComposer>();

            composer.ComposeCivilizations(mapData);

            CollectionAssert.AreEquivalent(
                discoveredTechs.Select(tech => tech.Name),
                mapData.Civilizations[0].DiscoveredTechs,
                "Serializable civilization data has an unexpected collection of discovered techs"
            );
        }

        [Test]
        public void ComposeCivilizations_RecordsProgressOnAvailableTechs() {
            var civOne = BuildCivilization("Civ One", Color.black, 0, 0);

            var techOne   = BuildTechDefinition("Tech One",   1);
            var techTwo   = BuildTechDefinition("Tech Two",   2);
            var techThree = BuildTechDefinition("Tech Three", 0);

            BuildTechDefinition("Tech Four",  4);

            var availableTechs = new List<ITechDefinition>() {
                techOne, techTwo, techThree
            };

            MockTechCanon.Setup(canon => canon.GetTechsAvailableToCiv(civOne))
                         .Returns(availableTechs);

            MockGameCore.Setup(core => core.ActiveCivilization).Returns(civOne);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CivilizationComposer>();

            composer.ComposeCivilizations(mapData);

            var progressDict = mapData.Civilizations[0].ProgressOnTechs;
            
            Assert.AreEqual(1, progressDict["Tech One"], "ProgressOnTechs has an incorrect value for Tech One");
            Assert.AreEqual(2, progressDict["Tech Two"], "ProgressOnTechs has an incorrect value for Tech Two");

            Assert.IsFalse(
                progressDict.ContainsKey("Tech Three"),
                "ProgressDict incorrectly records unprogressed tech Tech Three"
            );

            Assert.IsFalse(
                progressDict.ContainsKey("Tech Four"),
                "ProgressDict incorrectly records unavailable tech Tech Four"
            );
        }

        [Test]
        public void ComposeCivilizations_RecordsActiveCivilization() {
            var civOne = BuildCivilization("Civ One", Color.black, 0, 0);

            MockGameCore.Setup(core => core.ActiveCivilization).Returns(civOne);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CivilizationComposer>();

            composer.ComposeCivilizations(mapData);

            Assert.AreEqual("Civ One", mapData.ActiveCivilization);
        }

        [Test]
        public void DecomposeCivilizations_CallsIntoFactoryWithNameAndColor() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        Name = "Civ One", Color = Color.white
                    },
                    new SerializableCivilizationData() {
                        Name = "Civ Two", Color = Color.red
                    }
                }
            };

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            MockCivilizationFactory.Verify(
                factory => factory.Create("Civ One", Color.white), Times.Once,
                "CivFactory did not receive the expected call to create Civ One"
            );

            MockCivilizationFactory.Verify(
                factory => factory.Create("Civ Two", Color.red), Times.Once,
                "CivFactory did not receive the expected call to create Civ Two"
            );
        }

        [Test]
        public void DecomposeCivilizations_AssignsStockpilesCorrectly() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        Name = "Civ One", GoldStockpile = 1, CultureStockpile = 10
                    },
                    new SerializableCivilizationData() {
                        Name = "Civ Two", GoldStockpile = 2, CultureStockpile = 20
                    }
                }
            };

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civOne = AllCivilizations.Where(civ => civ.Name.Equals("Civ One")).First();
            var civTwo = AllCivilizations.Where(civ => civ.Name.Equals("Civ Two")).First();

            Assert.AreEqual(1,  civOne.GoldStockpile,    "CivOne.GoldStockpile has an unexpected value");
            Assert.AreEqual(10, civOne.CultureStockpile, "CivOne.CultureStockpile has an unexpected value");

            Assert.AreEqual(2,  civTwo.GoldStockpile,    "CivTwo.GoldStockpile has an unexpected value");
            Assert.AreEqual(20, civTwo.CultureStockpile, "CivTwo.CultureStockpile has an unexpected value");
        }

        [Test]
        public void DecomposeCivilizations_RebuildsTechQueueProperly() {
            var civOneQueue = new List<string>() {  "Tech One", "Tech Three", "Tech Two" };
            var civTwoQueue = new List<string>() {  "Tech Three", "Tech Two", "Tech One" };

            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        Name = "Civ One", TechQueue = civOneQueue
                    },
                    new SerializableCivilizationData() {
                        Name = "Civ Two", TechQueue = civTwoQueue
                    }
                }
            };

            var techOne   = BuildTechDefinition("Tech One");
            var techTwo   = BuildTechDefinition("Tech Two");
            var techThree = BuildTechDefinition("Tech Three");

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civOne = AllCivilizations.Where(civ => civ.Name.Equals("Civ One")).First();
            var civTwo = AllCivilizations.Where(civ => civ.Name.Equals("Civ Two")).First();

            CollectionAssert.AreEqual(
                new List<ITechDefinition>() { techOne, techThree, techTwo },
                civOne.TechQueue,
                "CivOne has an unexpected TechQueue"
            );

            CollectionAssert.AreEqual(
                new List<ITechDefinition>() { techThree, techTwo, techOne },
                civTwo.TechQueue,
                "CivTwo has an unexpected TechQueue"
            );
        }

        [Test]
        public void DecomposeCivilizations_SetsDiscoveredTechsProperly() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        Name = "Civ One", DiscoveredTechs = new List<string>() {
                            "Tech One", "Tech Two"
                        }
                    },
                    new SerializableCivilizationData() {
                        Name = "Civ Two", DiscoveredTechs = new List<string>() {
                            "Tech Two", "Tech Three"
                        }
                    }
                }
            };

            var techOne   = BuildTechDefinition("Tech One");
            var techTwo   = BuildTechDefinition("Tech Two");
            var techThree = BuildTechDefinition("Tech Three");

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civOne = AllCivilizations.Where(civ => civ.Name.Equals("Civ One")).First();
            var civTwo = AllCivilizations.Where(civ => civ.Name.Equals("Civ Two")).First();

            MockTechCanon.Verify(
                canon => canon.SetTechAsDiscoveredForCiv(techOne, civOne),
                Times.Once, "TechOne was not set as discovered for CivOne"
            );

            MockTechCanon.Verify(
                canon => canon.SetTechAsDiscoveredForCiv(techTwo, civOne),
                Times.Once, "TechTwo was not set as discovered for CivOne"
            );

            MockTechCanon.Verify(
                canon => canon.SetTechAsDiscoveredForCiv(techTwo, civTwo),
                Times.Once, "TechTwo was not set as discovered for CivTwo"
            );

            MockTechCanon.Verify(
                canon => canon.SetTechAsDiscoveredForCiv(techThree, civTwo),
                Times.Once, "TechThree was not set as discovered for CivTwo"
            );
        }

        [Test]
        public void DecomposeCivilizations_SetsProgressOnAvailableTechsProperly() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        Name = "Civ One", ProgressOnTechs = new Dictionary<string, int>() {
                            { "Tech One", 1 }, { "Tech Two", 2 },
                        }
                    },
                    new SerializableCivilizationData() {
                        Name = "Civ Two", ProgressOnTechs = new Dictionary<string, int>() {
                            { "Tech Two", 20 }, { "Tech Three", 30 },
                        }
                    }
                }
            };

            var techOne   = BuildTechDefinition("Tech One");
            var techTwo   = BuildTechDefinition("Tech Two");
            var techThree = BuildTechDefinition("Tech Three");

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civOne = AllCivilizations.Where(civ => civ.Name.Equals("Civ One")).First();
            var civTwo = AllCivilizations.Where(civ => civ.Name.Equals("Civ Two")).First();

            MockTechCanon.Verify(
                canon => canon.SetProgressOnTechByCiv(techOne, civOne, 1),
                Times.Once, "SetProgressOnTechByCiv was not calld as expected between CivOne and TechOne"
            );

            MockTechCanon.Verify(
                canon => canon.SetProgressOnTechByCiv(techTwo, civOne, 2),
                Times.Once, "SetProgressOnTechByCiv was not calld as expected between CivOne and TechTwo"
            );

            MockTechCanon.Verify(
                canon => canon.SetProgressOnTechByCiv(techTwo, civTwo, 20),
                Times.Once, "SetProgressOnTechByCiv was not calld as expected between CivTwo and TechTwo"
            );

            MockTechCanon.Verify(
                canon => canon.SetProgressOnTechByCiv(techThree, civTwo, 30),
                Times.Once, "SetProgressOnTechByCiv was not calld as expected between CivTwo and TechThree"
            );
        }

        [Test]
        public void DecomposeCivilizations_SetsActiveCivilizationProperly() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        Name = "Civ One"
                    },
                    new SerializableCivilizationData() {
                        Name = "Civ Two"
                    }
                },
                ActiveCivilization = "Civ Two"
            };

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civTwo = AllCivilizations.Where(civ => civ.Name.Equals("Civ Two")).First();

            MockGameCore.VerifySet(core => core.ActiveCivilization = civTwo);
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(out Mock<ICivilization> mock) {
            mock = new Mock<ICivilization>();

            var newCiv = mock.Object;

            AllCivilizations.Add(newCiv);

            return newCiv;
        }

        private ICivilization BuildCivilization(
            string name, Color color, int goldStockpile, int cultureStockpile
        ){
            var mockCiv = new Mock<ICivilization>();

            mockCiv.SetupAllProperties();

            mockCiv.Setup(civ => civ.Name)     .Returns(name);
            mockCiv.Setup(civ => civ.Color)    .Returns(color);
            mockCiv.Setup(civ => civ.TechQueue).Returns(new Queue<ITechDefinition>());

            var newCiv = mockCiv.Object;

            newCiv.GoldStockpile    = goldStockpile;
            newCiv.CultureStockpile = cultureStockpile;

            AllCivilizations.Add(newCiv);

            return newCiv;
        }

        private ITechDefinition BuildTechDefinition(string name, int progress = 0) {
            var mockTech = new Mock<ITechDefinition>();

            mockTech.Setup(tech => tech.Name).Returns(name);
            
            mockTech.Name = name;

            var newTech = mockTech.Object;

            MockTechCanon.Setup(
                canon => canon.GetProgressOnTechByCiv(newTech, It.IsAny<ICivilization>())
            ).Returns(progress);

            AvailableTechs.Add(newTech);

            return newTech;
        }

        #endregion

        #endregion

    }

}
