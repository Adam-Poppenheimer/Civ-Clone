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

        [SerializeField] private RectTransform MapEditorContainer = null;

        [SerializeField] private List<RectTransform> MapEditorDefaultPanels = null;

        [SerializeField] private PlayerManagementPanel    CivManagementPanel       = null;
        [SerializeField] private UnitPaintingPanel        UnitPaintingPanel        = null;
        [SerializeField] private CityPaintingPanel        CityPaintingPanel        = null;
        [SerializeField] private CivSelectionPanel        CivSelectionPanel        = null;
        [SerializeField] private ResourcePaintingPanel    ResourcePaintingPanel    = null;
        [SerializeField] private ImprovementPaintingPanel ImprovementPaintingPanel = null;
        [SerializeField] private UnitEditingPanel         UnitEditingPanel         = null;
        [SerializeField] private CivEditingPanel          CivEditingPanel          = null;
        [SerializeField] private BrushPanel               BrushPanel               = null;
        [SerializeField] private RiverPaintingPanel       RiverPaintingPanel       = null;
        [SerializeField] private OptionsPanel             OptionsPanel             = null;

        [SerializeField] private CellPaintingPanelBase TerrainPaintingPanel    = null;
        [SerializeField] private CellPaintingPanelBase ShapePaintingPanel      = null;
        [SerializeField] private CellPaintingPanelBase VegetationPaintingPanel = null;
        [SerializeField] private CellPaintingPanelBase FeaturePaintingPanel    = null;
        [SerializeField] private CellPaintingPanelBase RoadPaintingPanel       = null;
        [SerializeField] private CellPaintingPanelBase EncampmentPaintingPanel = null;

        [SerializeField] private GameObject SaveMapDisplay         = null;
        [SerializeField] private GameObject EditTypeSelectionPanel = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<RectTransform>().WithId("Map Editor Container").FromInstance(MapEditorContainer);

            Container.Bind<List<RectTransform>>().WithId("Map Editor Default Panels").FromInstance(MapEditorDefaultPanels);

            Container.Bind<PlayerManagementPanel>   ().FromInstance(CivManagementPanel);
            Container.Bind<UnitPaintingPanel>       ().FromInstance(UnitPaintingPanel);
            Container.Bind<CityPaintingPanel>       ().FromInstance(CityPaintingPanel);
            Container.Bind<CivSelectionPanel>       ().FromInstance(CivSelectionPanel);
            Container.Bind<ResourcePaintingPanel>   ().FromInstance(ResourcePaintingPanel);
            Container.Bind<ImprovementPaintingPanel>().FromInstance(ImprovementPaintingPanel);
            Container.Bind<UnitEditingPanel>        ().FromInstance(UnitEditingPanel);
            Container.Bind<CivEditingPanel>         ().FromInstance(CivEditingPanel);
            Container.Bind<BrushPanel>              ().FromInstance(BrushPanel);
            Container.Bind<RiverPaintingPanel>      ().FromInstance(RiverPaintingPanel);
            Container.Bind<OptionsPanel>            ().FromInstance(OptionsPanel);

            Container.Bind<CellPaintingPanelBase>().WithId("Terrain Painting Panel")   .FromInstance(TerrainPaintingPanel);
            Container.Bind<CellPaintingPanelBase>().WithId("Shape Painting Panel")     .FromInstance(ShapePaintingPanel);
            Container.Bind<CellPaintingPanelBase>().WithId("Vegetation Painting Panel").FromInstance(VegetationPaintingPanel);
            Container.Bind<CellPaintingPanelBase>().WithId("Feature Painting Panel")   .FromInstance(FeaturePaintingPanel);
            Container.Bind<CellPaintingPanelBase>().WithId("Road Painting Panel")      .FromInstance(RoadPaintingPanel);
            Container.Bind<CellPaintingPanelBase>().WithId("Encampment Painting Panel").FromInstance(EncampmentPaintingPanel);

            Container.Bind<GameObject>().WithId("Save Map Display")         .FromInstance(SaveMapDisplay);
            Container.Bind<GameObject>().WithId("Edit Type Selection Panel").FromInstance(EditTypeSelectionPanel);
        }

        #endregion

        #endregion

    }

}
