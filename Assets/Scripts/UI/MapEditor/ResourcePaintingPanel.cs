using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class ResourcePaintingPanel : MonoBehaviour {

        #region instance fields and properties

        private IResourceDefinition ActiveResource;

        private int ActiveCopies {
            get { return _activeCopies; }
            set {
                _activeCopies = value;
                CopiesField.text = _activeCopies.ToString();
                CopiesSlider.value = value;
            }
        }
        private int _activeCopies;

        private bool IsDeleting = false;

        [SerializeField] private RectTransform CopiesSection  = null;
        [SerializeField] private Text          CopiesField    = null;
        [SerializeField] private Slider        CopiesSlider   = null;

        [SerializeField] private RectTransform ResourceRecordPrefab    = null;
        [SerializeField] private RectTransform ResourceRecordContainer = null;
        [SerializeField] private RectTransform ResourceSection         = null;

        private IDisposable CellClickedSubscription;




        private IResourceNodeFactory                             ResourceNodeFactory;
        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;
        private List<IResourceDefinition>                        AvailableResources;
        private HexCellSignals                                   CellSignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IResourceNodeFactory resourceNodeFactory, IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources, HexCellSignals cellSignals
        ){
            ResourceNodeFactory = resourceNodeFactory;
            NodePositionCanon   = nodePositionCanon;
            AvailableResources  = new List<IResourceDefinition>(availableResources);
            CellSignals         = cellSignals;
        }

        #region Unity messages

        private void Start() {
            BuildResourceRecords();
        }

        private void OnEnable() {
            CellClickedSubscription = CellSignals.Clicked.Subscribe(OnCellClicked);
        }
        
        private void OnDisable() {
            CellClickedSubscription.Dispose();
        }

        #endregion

        public void SetActiveCopies(float newValue) {
            ActiveCopies = Mathf.RoundToInt(newValue);
        }

        public void SetIsDeleting(bool newValue) {
            IsDeleting = newValue;

            CopiesSection  .gameObject.SetActive(!IsDeleting);
            ResourceSection.gameObject.SetActive(!IsDeleting);
        }

        public void SetActiveResource(int index) {
            ActiveResource = AvailableResources[index];

            var shouldHaveCopies = ActiveResource.Type == ResourceType.Strategic;

            CopiesSection.gameObject.SetActive(shouldHaveCopies);

            ActiveCopies = shouldHaveCopies ? ActiveCopies : 1;
        }

        private void BuildResourceRecords() {
            AvailableResources.Sort(ResourceSorter);

            for(int i = 0; i < AvailableResources.Count; i++) {
                var cachedIndex = i;

                var newRecord = Instantiate(ResourceRecordPrefab);

                var recordText   = newRecord.GetComponentInChildren<Text>();
                var recordToggle = newRecord.GetComponentInChildren<Toggle>();

                recordText.text = AvailableResources[i].name;
                recordToggle.onValueChanged.AddListener(delegate(bool isOn) {
                    if(isOn) {
                        SetActiveResource(cachedIndex);
                    }
                });

                if(ActiveResource == null && recordToggle.isOn) {
                    SetActiveResource(cachedIndex);
                }

                newRecord.gameObject.SetActive(true);
                newRecord.transform.SetParent(ResourceRecordContainer, false);
            }
        }

        private void OnCellClicked(Tuple<IHexCell, PointerEventData> data) {
            var cell = data.Item1;

            if(IsDeleting) {
                var nodeOnCell = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
                if(nodeOnCell != null) {
                    nodeOnCell.Destroy();
                }
            }else if(ActiveResource != null && ResourceNodeFactory.CanBuildNode(cell, ActiveResource)){
                ResourceNodeFactory.BuildNode(cell, ActiveResource, ActiveCopies);
            }
        }

        private int ResourceSorter(IResourceDefinition a, IResourceDefinition b) {
            return a.name.CompareTo(b.name);
        }

        #endregion

    }

}
