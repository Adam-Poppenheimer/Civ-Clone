using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

using Assets.UI.StateMachine.States;

namespace Assets.UI.StateMachine {

    public class MapEditorInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private TerrainEditingPanel TerrainEditor;

        [SerializeField] private List<RectTransform> MapEditorDefaultPanels;

        [SerializeField] private CivManagementPanel CivPanel;

        [SerializeField] private UnitPaintingPanel UnitPaintingPanel;

        [SerializeField] private CityPaintingPanel CityPaintingPanel;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<List<RectTransform>>().WithId("Map Editor Default Panels").FromInstance(MapEditorDefaultPanels);

            Container.Bind<CivManagementPanel>().FromInstance(CivPanel);          

            Container.Bind<TerrainEditingPanel>().FromInstance(TerrainEditor);

            Container.Bind<UnitPaintingPanel>().FromInstance(UnitPaintingPanel);

            Container.Bind<CityPaintingPanel>().FromInstance(CityPaintingPanel);
        }

        #endregion

        #endregion

    }

}
