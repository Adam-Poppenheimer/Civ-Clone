using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.MapResources;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class DiplomacyComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IDiplomacyCore>              MockDiplomacyCore;
        private Mock<ICivilizationFactory>        MockCivFactory;
        private Mock<IWarCanon>                   MockWarCanon;
        private Mock<IDiplomaticProposalComposer> MockProposalComposer;
        private Mock<IOngoingDealComposer>        MockOngoingDealComposer;

        private List<ICivilization> AllCivs = new List<ICivilization>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCivs.Clear();

            MockDiplomacyCore       = new Mock<IDiplomacyCore>();
            MockCivFactory          = new Mock<ICivilizationFactory>();
            MockWarCanon            = new Mock<IWarCanon>();
            MockProposalComposer    = new Mock<IDiplomaticProposalComposer>();
            MockOngoingDealComposer = new Mock<IOngoingDealComposer>();

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivs.AsReadOnly());

            Container.Bind<IDiplomacyCore>             ().FromInstance(MockDiplomacyCore      .Object);
            Container.Bind<ICivilizationFactory>       ().FromInstance(MockCivFactory         .Object);
            Container.Bind<IWarCanon>                  ().FromInstance(MockWarCanon           .Object);
            Container.Bind<IDiplomaticProposalComposer>().FromInstance(MockProposalComposer   .Object);
            Container.Bind<IOngoingDealComposer>       ().FromInstance(MockOngoingDealComposer.Object);

            Container.Bind<DiplomacyComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        public void ClearRuntime_WarCanonCleared() {
            var composer = Container.Resolve<DiplomacyComposer>();

            composer.ClearRuntime();

            MockWarCanon.Verify(canon => canon.Clear(), Times.Once);
        }

        [Test(Description = "")]
        public void ClearRuntime_DiplomacyCoreCleared() {
            var composer = Container.Resolve<DiplomacyComposer>();

            composer.ClearRuntime();

            MockDiplomacyCore.Verify(
                core => core.ClearProposals(), Times.Once, "ClearProposals() was not called"
            );

            MockDiplomacyCore.Verify(
                core => core.ClearOngoingDeals(), Times.Once, "ClearOngoingDeals() was not called"
            );
        }

        [Test]
        public void ComposeDiplomacy_StoresWarsAsPairsOfCivNames() {
            var civOne   = BuildCivilization("Civ One");
            var civTwo   = BuildCivilization("Civ Two");
            var civThree = BuildCivilization("Civ Three");

            MockWarCanon.Setup(canon => canon.GetAllActiveWars()).Returns(new List<WarData>() {
                new WarData() { Attacker = civOne,   Defender = civTwo   },
                new WarData() { Attacker = civTwo,   Defender = civThree },
                new WarData() { Attacker = civThree, Defender = civOne   },
            });

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<DiplomacyComposer>();

            composer.ComposeDiplomacy(mapData);

            CollectionAssert.AreEquivalent(
                new List<Tuple<string, string>>() {
                    new Tuple<string, string>("Civ One",   "Civ Two"),
                    new Tuple<string, string>("Civ Two",   "Civ Three"),
                    new Tuple<string, string>("Civ Three", "Civ One"),
                },
                mapData.DiplomacyData.ActiveWars
            );
        }

        [Test]
        public void ComposeDiplomacy_ComposesProposalsThroughProposalComposer() {
            var civOne   = BuildCivilization("Civ One");
            var civTwo   = BuildCivilization("Civ Two");
            BuildCivilization("Civ Three");

            var serializableProposalOne   = new SerializableProposalData();
            var serializableProposalTwo   = new SerializableProposalData();
            var serializableProposalThree = new SerializableProposalData();

            var proposalOne   = BuildProposal();
            var proposalTwo   = BuildProposal();
            var proposalThree = BuildProposal();

            MockDiplomacyCore.Setup(core => core.GetProposalsSentFromCiv(civOne))
                             .Returns(new List<IDiplomaticProposal>() { proposalOne });

            MockDiplomacyCore.Setup(core => core.GetProposalsSentFromCiv(civTwo))
                             .Returns(new List<IDiplomaticProposal>() { proposalTwo, proposalThree });

            MockProposalComposer.Setup(composer => composer.ComposeProposal(proposalOne))  .Returns(serializableProposalOne);
            MockProposalComposer.Setup(composer => composer.ComposeProposal(proposalTwo))  .Returns(serializableProposalTwo);
            MockProposalComposer.Setup(composer => composer.ComposeProposal(proposalThree)).Returns(serializableProposalThree);

            var mapData = new SerializableMapData();

            var diplomacyComposer = Container.Resolve<DiplomacyComposer>();

            diplomacyComposer.ComposeDiplomacy(mapData);

            CollectionAssert.AreEquivalent(
                new List<SerializableProposalData>() {
                    serializableProposalOne, serializableProposalTwo, serializableProposalThree
                },
                mapData.DiplomacyData.ActiveProposals
            );
        }

        [Test]
        public void ComposeDiplomacy_ComposesOngoingDealsThroughOngoingDealComposer() {
            var civOne   = BuildCivilization("Civ One");
            var civTwo   = BuildCivilization("Civ Two");
             BuildCivilization("Civ Three");

            var serializableDealOne   = new SerializableOngoingDealData();
            var serializableDealTwo   = new SerializableOngoingDealData();
            var serializableDealThree = new SerializableOngoingDealData();

            var proposalOne   = BuildOngoingDeal();
            var proposalTwo   = BuildOngoingDeal();
            var proposalThree = BuildOngoingDeal();

            MockDiplomacyCore.Setup(core => core.GetOngoingDealsSentFromCiv(civOne))
                             .Returns(new List<IOngoingDeal>() { proposalOne });

            MockDiplomacyCore.Setup(core => core.GetOngoingDealsSentFromCiv(civTwo))
                             .Returns(new List<IOngoingDeal>() { proposalTwo, proposalThree });

            MockOngoingDealComposer.Setup(composer => composer.ComposeOngoingDeal(proposalOne))  .Returns(serializableDealOne);
            MockOngoingDealComposer.Setup(composer => composer.ComposeOngoingDeal(proposalTwo))  .Returns(serializableDealTwo);
            MockOngoingDealComposer.Setup(composer => composer.ComposeOngoingDeal(proposalThree)).Returns(serializableDealThree);

            var mapData = new SerializableMapData();

            var diplomacyComposer = Container.Resolve<DiplomacyComposer>();

            diplomacyComposer.ComposeDiplomacy(mapData);

            CollectionAssert.AreEquivalent(
                new List<SerializableOngoingDealData>() {
                    serializableDealOne, serializableDealTwo, serializableDealThree
                },
                mapData.DiplomacyData.ActiveOngoingDeals
            );
        }

        [Test]
        public void DecomposeDiplomacy_AssemblesWarRelationsCorrectly() {
            var civOne   = BuildCivilization("Civ One");
            var civTwo   = BuildCivilization("Civ Two");
            var civThree = BuildCivilization("Civ Three");

            var mapData = new SerializableMapData() {
                DiplomacyData = new SerializableDiplomacyData() {
                    ActiveWars = new List<Tuple<string, string>>() {
                        new Tuple<string, string>("Civ One",   "Civ Two"),
                        new Tuple<string, string>("Civ Two",   "Civ Three"),
                        new Tuple<string, string>("Civ Three", "Civ One")
                    }
                }
            };

            MockWarCanon.Setup(canon => canon.CanDeclareWar(It.IsAny<ICivilization>(), It.IsAny<ICivilization>()))
                        .Returns(true);

            var composer = Container.Resolve<DiplomacyComposer>();

            composer.DecomposeDiplomacy(mapData);

            MockWarCanon.Verify(canon => canon.CanDeclareWar(civOne,   civTwo),   Times.Once, "War validity between CivOne and CivTwo not checked");
            MockWarCanon.Verify(canon => canon.CanDeclareWar(civTwo,   civThree), Times.Once, "War validity between CivTwo and CivThree not checked");
            MockWarCanon.Verify(canon => canon.CanDeclareWar(civThree, civOne),   Times.Once, "War validity between CivThree and CivOne not checked");

            MockWarCanon.Verify(canon => canon.DeclareWar(civOne,   civTwo),   Times.Once, "War not declared between CivOne and CivTwo");
            MockWarCanon.Verify(canon => canon.DeclareWar(civTwo,   civThree), Times.Once, "War not declared between CivTwo and CivThree");
            MockWarCanon.Verify(canon => canon.DeclareWar(civThree, civOne),   Times.Once, "War not declared between CivThree and CivOne");
        }

        [Test]
        public void DecomposeDiplomacy_AssemblesProposalsThroughProposalComposer() {
            var serialProposalOne   = new SerializableProposalData();
            var serialProposalTwo   = new SerializableProposalData();
            var serialProposalThree = new SerializableProposalData();

            var proposalOne   = BuildProposal();
            var proposalTwo   = BuildProposal();
            var proposalThree = BuildProposal();

            var mapData = new SerializableMapData() {
                DiplomacyData = new SerializableDiplomacyData() {
                    ActiveProposals = new List<SerializableProposalData>() {
                        serialProposalOne, serialProposalTwo, serialProposalThree
                    }
                }
            };

            MockProposalComposer.Setup(composer => composer.DecomposeProposal(serialProposalOne))  .Returns(proposalOne);
            MockProposalComposer.Setup(composer => composer.DecomposeProposal(serialProposalTwo))  .Returns(proposalTwo);
            MockProposalComposer.Setup(composer => composer.DecomposeProposal(serialProposalThree)).Returns(proposalThree);

            var diplomacyComposer = Container.Resolve<DiplomacyComposer>();

            diplomacyComposer.DecomposeDiplomacy(mapData);

            MockDiplomacyCore.Verify(core => core.SendProposal(proposalOne),   Times.Once, "ProposalOne was never sent");
            MockDiplomacyCore.Verify(core => core.SendProposal(proposalTwo),   Times.Once, "ProposalTwo was never sent");
            MockDiplomacyCore.Verify(core => core.SendProposal(proposalThree), Times.Once, "ProposalThree was never sent");
        }

        [Test]
        public void DecomposeDiplomacy_AssemblesOngoingDealsThroughOngoingDealComposer() {
            var serialDealOne   = new SerializableOngoingDealData();
            var serialDealTwo   = new SerializableOngoingDealData();
            var serialDealThree = new SerializableOngoingDealData();

            var dealOne   = BuildOngoingDeal();
            var dealTwo   = BuildOngoingDeal();
            var dealThree = BuildOngoingDeal();

            var mapData = new SerializableMapData() {
                DiplomacyData = new SerializableDiplomacyData() {
                    ActiveOngoingDeals = new List<SerializableOngoingDealData>() {
                        serialDealOne, serialDealTwo, serialDealThree
                    }
                }
            };

            MockOngoingDealComposer.Setup(composer => composer.DecomposeOngoingDeal(serialDealOne))  .Returns(dealOne);
            MockOngoingDealComposer.Setup(composer => composer.DecomposeOngoingDeal(serialDealTwo))  .Returns(dealTwo);
            MockOngoingDealComposer.Setup(composer => composer.DecomposeOngoingDeal(serialDealThree)).Returns(dealThree);

            var diplomacyComposer = Container.Resolve<DiplomacyComposer>();

            diplomacyComposer.DecomposeDiplomacy(mapData);

            MockDiplomacyCore.Verify(core => core.SubscribeOngoingDeal(dealOne),   Times.Once, "DealOne was never sent");
            MockDiplomacyCore.Verify(core => core.SubscribeOngoingDeal(dealTwo),   Times.Once, "DealTwo was never sent");
            MockDiplomacyCore.Verify(core => core.SubscribeOngoingDeal(dealThree), Times.Once, "DealThree was never sent");
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(string name = "") {
            var mockCiv = new Mock<ICivilization>();

            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.Name).Returns(name);

            mockCiv.Name = name;
            mockCiv.Setup(civ => civ.Template).Returns(mockTemplate.Object);

            var newCiv = mockCiv.Object;

            AllCivs.Add(newCiv);

            return newCiv;
        }

        private IDiplomaticProposal BuildProposal() {
            return new Mock<IDiplomaticProposal>().Object;
        }

        private IOngoingDeal BuildOngoingDeal() {
            return new Mock<IOngoingDeal>().Object;
        }

        #endregion

        #endregion

    }

}
