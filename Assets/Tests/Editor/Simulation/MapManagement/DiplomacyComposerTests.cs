using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class DiplomacyComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IDiplomacyCore>                           MockDiplomacyCore;
        private Mock<ICivilizationFactory>                     MockCivFactory;
        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;
        private Mock<IDiplomaticExchangeFactory>               MockExchangeFactory;
        private Mock<IHexGrid>                                 MockGrid;
        private Mock<IWarCanon>                                MockWarCanon;

        private List<ISpecialtyResourceDefinition> AvailableResources =
            new List<ISpecialtyResourceDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableResources.Clear();

            MockDiplomacyCore     = new Mock<IDiplomacyCore>();
            MockCivFactory        = new Mock<ICivilizationFactory>();
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockExchangeFactory   = new Mock<IDiplomaticExchangeFactory>();
            MockGrid              = new Mock<IHexGrid>();
            MockWarCanon          = new Mock<IWarCanon>();

            Container.Bind<IDiplomacyCore>                          ().FromInstance(MockDiplomacyCore    .Object);
            Container.Bind<ICivilizationFactory>                    ().FromInstance(MockCivFactory       .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);
            Container.Bind<IDiplomaticExchangeFactory>              ().FromInstance(MockExchangeFactory  .Object);
            Container.Bind<IHexGrid>                                ().FromInstance(MockGrid             .Object);
            Container.Bind<IWarCanon>                               ().FromInstance(MockWarCanon         .Object);

            Container.Bind<IEnumerable<ISpecialtyResourceDefinition>>()
                     .WithId("Available Specialty Resources")
                     .FromInstance(AvailableResources);

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

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
