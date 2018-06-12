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

        [SerializeField] private CivManagementPanel       CivManagementPanel;
        [SerializeField] private UnitPaintingPanel        UnitPaintingPanel;
        [SerializeField] private CityPaintingPanel        CityPaintingPanel;
        [SerializeField] private TechManagementPanel      TechManagementPanel;
        [SerializeField] private ResourcePaintingPanel    ResourcePaintingPanel;
        [SerializeField] private ImprovementPaintingPanel ImprovementPaintingPanel;
        [SerializeField] private UnitEditingPanel         UnitEditingPanel;
        [SerializeField] private CivEditingPanel          CivEditingPanel;
        [SerializeField] private BrushPanel               BrushPanel;
        [SerializeField] private RiverPaintingPanel       RiverPaintingPanel;

        [SerializeField] private CellPaintingPanelBase TerrainPaintingPanel;
        [SerializeField] private CellPaintingPanelBase ShapePaintingPanel;
        [SerializeField] private CellPaintingPanelBase FeaturePaintingPanel;
        
        [SerializeField] private CellPaintingPanelBase RoadPaintingPanel;

        [SerializeField] private GameObject SaveMapDisplay;
        [SerializeField] private GameObject EditTypeSelectionPanel;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<RectTransform>().WithId("Map Editor Container").FromInstance(MapEditorContainer);

            Container.Bind<List<RectTransform>>().WithId("Map Editor Default Panels").FromInstance(MapEditorDefaultPanels);

            Container.Bind<CivManagementPanel>      ().FromInstance(CivManagementPanel);
            Container.Bind<UnitPaintingPanel>       ().FromInstance(UnitPaintingPanel);
            Container.Bind<CityPaintingPanel>       ().FromInstance(CityPaintingPanel);
            Container.Bind<TechManagementPanel>     ().FromInstance(TechManagementPanel);
            Container.Bind<ResourcePaintingPanel>   ().FromInstance(ResourcePaintingPanel);
            Container.Bind<ImprovementPaintingPanel>().FromInstance(ImprovementPaintingPanel);
            Container.Bind<UnitEditingPanel>        ().FromInstance(UnitEditingPanel);
            Container.Bind<CivEditingPanel>         ().FromInstance(CivEditingPanel);
            Container.Bind<BrushPanel>              ().FromInstance(BrushPanel);
            Container.Bind<RiverPaintingPanel>      ().FromInstance(RiverPaintingPanel);

            Container.Bind<CellPaintingPanelBase>().WithId("Terrain Painting Panel").FromInstance(TerrainPaintingPanel);
            Container.Bind<CellPaintingPanelBase>().WithId("Shape Painting Panel")  .FromInstance(ShapePaintingPanel);
            Container.Bind<CellPaintingPanelBase>().WithId("Feature Painting Panel").FromInstance(FeaturePaintingPanel);
            Container.Bind<CellPaintingPanelBase>().WithId("Road Painting Panel")   .FromInstance(RoadPaintingPanel);

            Container.Bind<GameObject>().WithId("Save Map Display")         .FromInstance(SaveMapDisplay);
            Container.Bind<GameObject>().WithId("Edit Type Selection Panel").FromInstance(EditTypeSelectionPanel);
        }

        #endregion

        #endregion

    }

}
