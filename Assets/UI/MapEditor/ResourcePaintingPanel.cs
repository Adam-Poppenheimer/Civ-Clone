using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class ResourcePaintingPanel : MonoBehaviour {

        #region instance fields and properties

        private ISpecialtyResourceDefinition ActiveResource;

        private int ActiveCopies;

        private bool IsDeleting = false;

        [SerializeField] private RectTransform ResourceRecordPrefab;

        [SerializeField] private RectTransform ResourceRecordContainer;



        private IResourceNodeFactory ResourceNodeFactory;

        private IEnumerable<ISpecialtyResourceDefinition> AvailableResources;

        private HexCellSignals CellSignals;

        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IResourceNodeFactory resourceNodeFactory,
            [Inject(Id = "Available Specialty Resources")] IEnumerable<ISpecialtyResourceDefinition> availableResources,
            HexCellSignals cellSignals, IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon
        ){
            ResourceNodeFactory = resourceNodeFactory;
            AvailableResources        = availableResources;
            CellSignals         = cellSignals;
            NodePositionCanon   = nodePositionCanon;
        }

        #region Unity messages

        private void Start() {
            BuildResourceRecords();
        }

        private void OnEnable() {
            CellSignals.ClickedSignal.Listen(OnCellClicked);
        }
        
        private void OnDisable() {
            CellSignals.ClickedSignal.Unlisten(OnCellClicked);
        }

        #endregion

        public void SetActiveCopies(string newValue) {
            ActiveCopies = Int32.Parse(newValue);
        }

        public void SetIsDeleting(bool newValue) {
            IsDeleting = newValue;
        }

        private void BuildResourceRecords() {
            var sortedResources = new List<ISpecialtyResourceDefinition>(AvailableResources);
            sortedResources.Sort(ResourceSorter);

            foreach(var resource in sortedResources) {
                var cachedResource = resource;

                var newRecord = Instantiate(ResourceRecordPrefab);

                var recordText   = newRecord.GetComponentInChildren<Text>();
                var recordToggle = newRecord.GetComponentInChildren<Toggle>();

                recordText.text = cachedResource.name;
                recordToggle.onValueChanged.AddListener(delegate(bool isOn) {
                    if(isOn) {
                        ActiveResource = cachedResource;
                    }
                });

                if(ActiveResource == null && recordToggle.isOn) {
                    ActiveResource = cachedResource;
                }

                newRecord.gameObject.SetActive(true);
                newRecord.transform.SetParent(ResourceRecordContainer);
            }
        }

        private void OnCellClicked(IHexCell cell, Vector3 position) {
            if(IsDeleting) {
                var nodeOnCell = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
                if(nodeOnCell != null) {
                    Destroy(nodeOnCell.gameObject);
                }
            }else if(ActiveResource != null && ResourceNodeFactory.CanBuildNode(cell, ActiveResource)){
                ResourceNodeFactory.BuildNode(cell, ActiveResource, ActiveCopies);
            }

            cell.RefreshSelfOnly();
        }

        private int ResourceSorter(ISpecialtyResourceDefinition a, ISpecialtyResourceDefinition b) {
            return a.name.CompareTo(b.name);
        }

        #endregion

    }

}
