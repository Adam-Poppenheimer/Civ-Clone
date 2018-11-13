using System;
using System.Collections.ObjectModel;
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
using Assets.Simulation.Visibility;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class CivilizationComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationFactory>  MockCivilizationFactory;
        private Mock<ITechCanon>            MockTechCanon;
        private Mock<IGameCore>             MockGameCore;
        private Mock<ISocialPolicyComposer> MockPolicyComposer;
        private Mock<IExplorationCanon>     MockExplorationCanon;
        private Mock<IHexGrid>              MockGrid;

        private List<ITechDefinition>       AvailableTechs        = new List<ITechDefinition>();
        private List<ICivilization>         AllCivilizations      = new List<ICivilization>();
        private List<IHexCell>              AllCells              = new List<IHexCell>();
        private List<ICivilizationTemplate> AvailableCivTemplates = new List<ICivilizationTemplate>();
        

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTechs       .Clear();
            AllCivilizations     .Clear();
            AllCells             .Clear();
            AvailableCivTemplates.Clear();

            MockCivilizationFactory = new Mock<ICivilizationFactory>();
            MockTechCanon           = new Mock<ITechCanon>();
            MockGameCore            = new Mock<IGameCore>();
            MockPolicyComposer      = new Mock<ISocialPolicyComposer>();
            MockExplorationCanon    = new Mock<IExplorationCanon>();
            MockGrid                = new Mock<IHexGrid>();

            MockCivilizationFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivilizations.AsReadOnly());

            MockCivilizationFactory.Setup(
                factory => factory.Create(It.IsAny<ICivilizationTemplate>())
            ).Returns<ICivilizationTemplate>(
                template => BuildCivilization(template, 0, 0)
            );

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<ICivilizationFactory> ().FromInstance(MockCivilizationFactory.Object);
            Container.Bind<ITechCanon>           ().FromInstance(MockTechCanon          .Object);
            Container.Bind<IGameCore>            ().FromInstance(MockGameCore           .Object);
            Container.Bind<ISocialPolicyComposer>().FromInstance(MockPolicyComposer     .Object);
            Container.Bind<IExplorationCanon>    ().FromInstance(MockExplorationCanon   .Object);
            Container.Bind<IHexGrid>             ().FromInstance(MockGrid               .Object);

            Container.Bind<List<ITechDefinition>>().WithId("Available Techs").FromInstance(AvailableTechs);

            Container.Bind<ReadOnlyCollection<ICivilizationTemplate>>().FromInstance(AvailableCivTemplates.AsReadOnly());

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
        public void ClearRuntime_PolicyComposerToldToClearRuntime() {
            var civComposer = Container.Resolve<CivilizationComposer>();

            civComposer.ClearRuntime();

            MockPolicyComposer.Verify(composer => composer.ClearPolicyRuntime(), Times.Once);
        }

        [Test]
        public void ComposeCivilizations_RecordsIntrinsicCivilizationData() {
            var templateOne = BuildCivTemplate("Civ One", Color.black);
            var templateTwo = BuildCivTemplate("Civ Two", Color.white);

            var civOne = BuildCivilization(templateOne, 1, 10);
            var civTwo = BuildCivilization(templateTwo, 2, 20);

            MockGameCore.Setup(core => core.ActiveCivilization).Returns(civOne);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CivilizationComposer>();

            composer.ComposeCivilizations(mapData);

            var dataLikeCivOne = mapData.Civilizations.Where(
                civ => civ.TemplateName.Equals(civOne.Template.Name)   &&
                       civ.GoldStockpile    == civOne.GoldStockpile    &&
                       civ.CultureStockpile == civOne.CultureStockpile
            );

            var dataLikeCivTwo = mapData.Civilizations.Where(
                civ => civ.TemplateName.Equals(templateTwo.Name)       &&
                       civ.GoldStockpile    == civTwo.GoldStockpile    &&
                       civ.CultureStockpile == civTwo.CultureStockpile
            );

            Assert.AreEqual(1, dataLikeCivOne.Count(), "Did not find the expected number of serializable civs like civOne");
            Assert.AreEqual(1, dataLikeCivTwo.Count(), "Did not find the expected number of serializable civs like civTwo");
        }

        [Test]
        public void ComposeCivilizations_RecordsTechQueue() {
            var templateOne = BuildCivTemplate("Civ One", Color.black);

            var civOne = BuildCivilization(templateOne, 1, 10);

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
            var templateOne = BuildCivTemplate("Civ One", Color.black);

            var civOne = BuildCivilization(templateOne, 1, 10);

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
            var templateOne = BuildCivTemplate("Civ One", Color.black);

            var civOne = BuildCivilization(templateOne, 1, 10);

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
        public void ComposeCivilizations_CallsIntoPolicyComposerForPolicyData() {
            var templateOne = BuildCivTemplate("Civ One", Color.black);

            var civOne = BuildCivilization(templateOne, 1, 10);

            MockGameCore.Setup(core => core.ActiveCivilization).Returns(civOne);

            var policyData = new SerializableSocialPolicyData();
            MockPolicyComposer.Setup(composer => composer.ComposePoliciesFromCiv(civOne)).Returns(policyData);

            var mapData = new SerializableMapData();

            var civComposer = Container.Resolve<CivilizationComposer>();

            civComposer.ComposeCivilizations(mapData);

            Assert.AreEqual(policyData, mapData.Civilizations[0].SocialPolicies);
        }

        [Test]
        public void ComposeCivilizations_RecordsActiveCivilization() {
            var templateOne = BuildCivTemplate("Civ One", Color.black);

            var civOne = BuildCivilization(templateOne, 1, 10);

            MockGameCore.Setup(core => core.ActiveCivilization).Returns(civOne);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CivilizationComposer>();

            composer.ComposeCivilizations(mapData);

            Assert.AreEqual("Civ One", mapData.ActiveCivilization);
        }

        [Test]
        public void ComposeCivilizations_RecordsExploredCells() {
            var templateOne = BuildCivTemplate("Civ One", Color.black);

            var civOne = BuildCivilization(templateOne, 1, 10);

            MockGameCore.Setup(core => core.ActiveCivilization).Returns(civOne);

            var exploredCell   = BuildCell(new HexCoordinates(1, 1));
            var unexploredCell = BuildCell(new HexCoordinates(2, 2));

            MockExplorationCanon.Setup(canon => canon.IsCellExploredByCiv(exploredCell,   civOne)).Returns(true);
            MockExplorationCanon.Setup(canon => canon.IsCellExploredByCiv(unexploredCell, civOne)).Returns(false);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CivilizationComposer>();

            composer.ComposeCivilizations(mapData);

            CollectionAssert.AreEquivalent(
                new List<HexCoordinates>() { new HexCoordinates(1, 1) },
                mapData.Civilizations[0].ExploredCells
            );
        }




        [Test]
        public void DecomposeCivilizations_CallsIntoFactoryWithTemplate() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        TemplateName = "Template One"
                    },
                    new SerializableCivilizationData() {
                        TemplateName = "Template Two"
                    }
                }
            };

            var templateOne = BuildCivTemplate("Template One", Color.white);
            var templateTwo = BuildCivTemplate("Template Two", Color.red);

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            MockCivilizationFactory.Verify(
                factory => factory.Create(templateOne), Times.Once,
                "CivFactory did not receive the expected call to create Civ One"
            );

            MockCivilizationFactory.Verify(
                factory => factory.Create(templateTwo), Times.Once,
                "CivFactory did not receive the expected call to create Civ Two"
            );
        }

        [Test]
        public void DecomposeCivilizations_AssignsStockpilesCorrectly() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        TemplateName = "Template One", GoldStockpile = 1, CultureStockpile = 10
                    },
                    new SerializableCivilizationData() {
                        TemplateName = "Template Two", GoldStockpile = 2, CultureStockpile = 20
                    }
                }
            };

            BuildCivTemplate("Template One", Color.white);
            BuildCivTemplate("Template Two", Color.red);

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civOne = AllCivilizations.Where(civ => civ.Template.Name.Equals("Template One")).First();
            var civTwo = AllCivilizations.Where(civ => civ.Template.Name.Equals("Template Two")).First();

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
                        TemplateName = "Template One", TechQueue = civOneQueue
                    },
                    new SerializableCivilizationData() {
                        TemplateName = "Template Two", TechQueue = civTwoQueue
                    }
                }
            };

            var techOne   = BuildTechDefinition("Tech One");
            var techTwo   = BuildTechDefinition("Tech Two");
            var techThree = BuildTechDefinition("Tech Three");

            BuildCivTemplate("Template One", Color.white);
            BuildCivTemplate("Template Two", Color.red);

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civOne = AllCivilizations.Where(civ => civ.Template.Name.Equals("Template One")).First();
            var civTwo = AllCivilizations.Where(civ => civ.Template.Name.Equals("Template Two")).First();

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
                        TemplateName = "Template One", DiscoveredTechs = new List<string>() {
                            "Tech One", "Tech Two"
                        }
                    },
                    new SerializableCivilizationData() {
                        TemplateName = "Template Two", DiscoveredTechs = new List<string>() {
                            "Tech Two", "Tech Three"
                        }
                    }
                }
            };

            var techOne   = BuildTechDefinition("Tech One");
            var techTwo   = BuildTechDefinition("Tech Two");
            var techThree = BuildTechDefinition("Tech Three");

            BuildCivTemplate("Template One", Color.white);
            BuildCivTemplate("Template Two", Color.black);

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civOne = AllCivilizations.Where(civ => civ.Template.Name.Equals("Template One")).First();
            var civTwo = AllCivilizations.Where(civ => civ.Template.Name.Equals("Template Two")).First();

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
                        TemplateName = "Template One", ProgressOnTechs = new Dictionary<string, int>() {
                            { "Tech One", 1 }, { "Tech Two", 2 },
                        }
                    },
                    new SerializableCivilizationData() {
                        TemplateName = "Template Two", ProgressOnTechs = new Dictionary<string, int>() {
                            { "Tech Two", 20 }, { "Tech Three", 30 },
                        }
                    }
                }
            };

            var techOne   = BuildTechDefinition("Tech One");
            var techTwo   = BuildTechDefinition("Tech Two");
            var techThree = BuildTechDefinition("Tech Three");

            BuildCivTemplate("Template One", Color.white);
            BuildCivTemplate("Template Two", Color.red);

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civOne = AllCivilizations.Where(civ => civ.Template.Name.Equals("Template One")).First();
            var civTwo = AllCivilizations.Where(civ => civ.Template.Name.Equals("Template Two")).First();

            MockTechCanon.Verify(
                canon => canon.SetProgressOnTechByCiv(techOne, civOne, 1),
                Times.Once, "SetProgressOnTechByCiv was not called as expected between CivOne and TechOne"
            );

            MockTechCanon.Verify(
                canon => canon.SetProgressOnTechByCiv(techTwo, civOne, 2),
                Times.Once, "SetProgressOnTechByCiv was not called as expected between CivOne and TechTwo"
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
        public void DecomposeCivilizations_CallsIntoPolicyComposerCorrectly() {
            var policyDataOne = new SerializableSocialPolicyData();
            var policyDataTwo = new SerializableSocialPolicyData();

            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        TemplateName = "Template One", SocialPolicies = policyDataOne
                    },
                    new SerializableCivilizationData() {
                        TemplateName = "Template Two", SocialPolicies = policyDataTwo
                    }
                }
            };

            BuildCivTemplate("Template One", Color.white);
            BuildCivTemplate("Template Two", Color.red);

            var civComposer = Container.Resolve<CivilizationComposer>();

            civComposer.DecomposeCivilizations(mapData);

            MockPolicyComposer.Verify(
                composer => composer.DecomposePoliciesIntoCiv(
                    policyDataOne, It.Is<ICivilization>(civ => civ.Template.Name.Equals("Template One"))
                ),
                Times.Once,
                "DecomposePoliciesIntoCiv not called as expected on anticipated civOne and policyDataOne"
            );

            MockPolicyComposer.Verify(
                composer => composer.DecomposePoliciesIntoCiv(
                    policyDataTwo, It.Is<ICivilization>(civ => civ.Template.Name.Equals("Template Two"))
                ),
                Times.Once,
                "DecomposePoliciesIntoCiv not called as expected on anticipated civTwo and policyDataTwo"
            );
        }

        [Test]
        public void DecomposeCivilizations_SetsActiveCivilizationProperly() {
            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        TemplateName = "Template One"
                    },
                    new SerializableCivilizationData() {
                        TemplateName = "Template Two"
                    }
                },
                ActiveCivilization = "Template Two"
            };

            BuildCivTemplate("Template One", Color.white);
            BuildCivTemplate("Template Two", Color.red);

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civTwo = AllCivilizations.Where(civ => civ.Template.Name.Equals("Template Two")).First();

            MockGameCore.VerifySet(core => core.ActiveCivilization = civTwo);
        }

        [Test]
        public void DecomposeCivilizations_RebuildsExploredCellsProperly() {
            var cellOne   = BuildCell(new HexCoordinates(1, 1));
            var cellTwo   = BuildCell(new HexCoordinates(2, 2));
            var cellThree = BuildCell(new HexCoordinates(3, 3));

            var mapData = new SerializableMapData() {
                Civilizations = new List<SerializableCivilizationData>() {
                    new SerializableCivilizationData() {
                        TemplateName = "Template One",
                        ExploredCells = new List<HexCoordinates>() {
                            new HexCoordinates(1, 1), new HexCoordinates(3, 3)
                        }
                    }
                },
                ActiveCivilization = "Template One"
            };

            BuildCivTemplate("Template One", Color.white);

            var composer = Container.Resolve<CivilizationComposer>();

            composer.DecomposeCivilizations(mapData);

            var civ = AllCivilizations[0];

            MockExplorationCanon.Verify(canon => canon.SetCellAsExploredByCiv(cellOne,   civ), Times.Once,  "CellOne incorrectly left unexplored");
            MockExplorationCanon.Verify(canon => canon.SetCellAsExploredByCiv(cellTwo,   civ), Times.Never, "CellTwo incorrectly set as explored");
            MockExplorationCanon.Verify(canon => canon.SetCellAsExploredByCiv(cellThree, civ), Times.Once,  "CellThree incorrectly left unexplored");
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
            ICivilizationTemplate template, int goldStockpile, int cultureStockpile
        ){
            var mockCiv = new Mock<ICivilization>();

            mockCiv.SetupAllProperties();

            mockCiv.Setup(civ => civ.Template) .Returns(template);
            mockCiv.Setup(civ => civ.TechQueue).Returns(new Queue<ITechDefinition>());

            var newCiv = mockCiv.Object;

            newCiv.GoldStockpile    = goldStockpile;
            newCiv.CultureStockpile = cultureStockpile;

            AllCivilizations.Add(newCiv);

            return newCiv;
        }

        private ICivilizationTemplate BuildCivTemplate(string name, Color color) {
            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.Name) .Returns(name);
            mockTemplate.Setup(template => template.Color).Returns(color);

            var newTemplate = mockTemplate.Object;

            AvailableCivTemplates.Add(newTemplate);

            return newTemplate;
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

        private IHexCell BuildCell(HexCoordinates coords) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Coordinates).Returns(coords);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(coords)).Returns(newCell);

            AllCells.Add(newCell);

            return newCell;
        }

        #endregion

        #endregion

    }

}
