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

        [SerializeField] private RectTransform MapEditorContainer;

        [SerializeField] private List<RectTransform> MapEditorDefaultPanels;

        [SerializeField] private TerrainEditingPanel   TerrainEditingPanel;
        [SerializeField] private CivManagementPanel    CivManagementPanel;
        [SerializeField] private UnitPaintingPanel     UnitPaintingPanel;
        [SerializeField] private CityPaintingPanel     CityPaintingPanel;
        [SerializeField] private TechManagementPanel   TechManagementPanel;
        [SerializeField] private ResourcePaintingPanel ResourcePaintingPanel;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<RectTransform>()
                .WithId("Map Editor Container")
                .FromInstance(MapEditorContainer);

            Container.Bind<List<RectTransform>>().WithId("Map Editor Default Panels").FromInstance(MapEditorDefaultPanels);

            Container.Bind<CivManagementPanel>   ().FromInstance(CivManagementPanel);
            Container.Bind<TerrainEditingPanel>  ().FromInstance(TerrainEditingPanel);
            Container.Bind<UnitPaintingPanel>    ().FromInstance(UnitPaintingPanel);
            Container.Bind<CityPaintingPanel>    ().FromInstance(CityPaintingPanel);
            Container.Bind<TechManagementPanel>  ().FromInstance(TechManagementPanel);
            Container.Bind<ResourcePaintingPanel>().FromInstance(ResourcePaintingPanel);
        }

        #endregion

        #endregion

    }

}
