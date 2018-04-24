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
            Container.Bind<HexCellComposer>     ().AsSingle();
            Container.Bind<CivilizationComposer>().AsSingle();
            Container.Bind<CityComposer>        ().AsSingle();
            Container.Bind<UnitComposer>        ().AsSingle();
            Container.Bind<ImprovementComposer> ().AsSingle();
            Container.Bind<ResourceComposer>    ().AsSingle();
            Container.Bind<DiplomacyComposer>   ().AsSingle();

            Container.Bind<IMapComposer>().To<MapComposer>().AsSingle();

            Container.Bind<string>().WithId("Saved Game Path").FromInstance(SavedGamePath);
            Container.Bind<string>().WithId("Map Path")       .FromInstance(MapPath);

            Container.Bind<IFileSystemLiaison>().To<FileSystemLiaison>().AsSingle();
        }

        #endregion

        #endregion

    }

}
