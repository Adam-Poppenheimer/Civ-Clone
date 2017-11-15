using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

using Zenject;

namespace Assets.GameMap {

    public class MapTileInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private TileConfig TileDisplayConfig;

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
            container.Rebind<TileConfig>().FromScriptableObjectResource("Config/Tile Config").AsSingle();
        }

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            InstallOntoContainer(Container);
            
        }

        #endregion

        

        #endregion

    }

}
