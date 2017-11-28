﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

using Zenject;

namespace Assets.Simulation.GameMap {

    public class MapTileInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private TileConfig TileDisplayConfig;
        
        [SerializeField] private TileResourceConfig TileResourceConfig;

        #endregion

        #region static methods

        public static void PerformEditorTimeInject() {
            var container = StaticContext.Container;

            InstallOntoContainer(container);          

            foreach(var tile in GameObject.FindObjectsOfType<MapTile>()) {
                container.InjectGameObject(tile.gameObject);

                tile.Refresh();
            }
        }

        private static void InstallOntoContainer(DiContainer container) {
            container.Rebind<ITileConfig>().To<TileConfig>().FromScriptableObjectResource("Tile Config").AsSingle();
        }

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            InstallOntoContainer(Container);

            Container.Bind<ITileResourceLogic>().To<TileResourceLogic>().AsSingle();
            Container.Bind<ITileResourceConfig>().To<TileResourceConfig>().FromInstance(TileResourceConfig);

            Container.DeclareSignal<TileClickedSignal>();
            Container.DeclareSignal<TilePointerEnterSignal>();
            Container.DeclareSignal<TilePointerExitSignal>();

            Container.Bind<MapTileSignals>().AsSingle();
        }

        #endregion        

        #endregion

    }

}
