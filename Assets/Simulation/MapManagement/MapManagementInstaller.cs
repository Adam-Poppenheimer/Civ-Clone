using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapManagement {

    public class MapManagementInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private string SavedGamePath;
        [SerializeField] private string MapPath;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IDiplomaticProposalComposer>       ().To<DiplomaticProposalComposer>       ().AsSingle();
            Container.Bind<IOngoingDealComposer>              ().To<OngoingDealComposer>              ().AsSingle();
            Container.Bind<IDiplomaticExchangeComposer>       ().To<DiplomaticExchangeComposer>       ().AsSingle();
            Container.Bind<IOngoingDiplomaticExchangeComposer>().To<OngoingDiplomaticExchangeComposer>().AsSingle();

            Container.Bind<IPromotionTreeComposer>().To<PromotionTreeComposer>().AsSingle();

            Container.Bind<ISocialPolicyComposer>().To<SocialPolicyComposer>().AsSingle();

            Container.Bind<IHexCellComposer>     ().To<HexCellComposer>     ().AsSingle();
            Container.Bind<ICivilizationComposer>().To<CivilizationComposer>().AsSingle();
            Container.Bind<ICityComposer>        ().To<CityComposer>        ().AsSingle();
            Container.Bind<IBuildingComposer>    ().To<BuildingComposer>    ().AsSingle();
            Container.Bind<IUnitComposer>        ().To<UnitComposer>        ().AsSingle();
            Container.Bind<IImprovementComposer> ().To<ImprovementComposer> ().AsSingle();
            Container.Bind<IResourceComposer>    ().To<ResourceComposer>    ().AsSingle();
            Container.Bind<IDiplomacyComposer>   ().To<DiplomacyComposer>   ().AsSingle();
            Container.Bind<ICapitalCityComposer> ().To<CapitalCityComposer> ().AsSingle();

            Container.Bind<IMapComposer>().To<MapComposer>().AsSingle();

            Container.Bind<string>().WithId("Saved Game Path").FromInstance(SavedGamePath);
            Container.Bind<string>().WithId("Map Path")       .FromInstance(MapPath);

            Container.Bind<IFileSystemLiaison>().To<FileSystemLiaison>().AsSingle();
        }

        #endregion

        #endregion

    }

}
