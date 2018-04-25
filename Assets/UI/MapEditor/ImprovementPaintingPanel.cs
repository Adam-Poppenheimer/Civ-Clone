using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.UI.MapEditor {

    public class ImprovementPaintingPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private RectTransform ImprovementRecordPrefab;
        [SerializeField] private RectTransform ImprovementRecordContainer;

        [SerializeField] private Toggle IsAddingToggle;
        [SerializeField] private Toggle IsRemovingToggle;
        [SerializeField] private Toggle IsPillagingToggle;

        private bool IsAdding;
        private bool IsRemoving;
        private bool IsPillaging;

        private IImprovementTemplate SelectedTemplate;

        private List<RectTransform> InstantiatedRecords = new List<RectTransform>();  




        private IImprovementValidityLogic         ImprovementValidityLogic;
        private IImprovementLocationCanon         ImprovementLocationCanon;
        private IImprovementFactory               ImprovementFactory;
        private HexCellSignals                    CellSignals;
        private IEnumerable<IImprovementTemplate> AvailableImprovementTemplates;

        #endregion

        #region instance methods

        #region Unity messages

        [Inject]
        public void InjectDependencies(
            IImprovementValidityLogic improvementValidityLogic,
            IImprovementLocationCanon improvementLocationCanon,
            IImprovementFactory improvementFactory, HexCellSignals cellSignals,
            [Inject(Id = "Available Improvement Templates")] IEnumerable<IImprovementTemplate> availableImprovementTemplates
        ){
            ImprovementValidityLogic      = improvementValidityLogic;
            ImprovementLocationCanon      = improvementLocationCanon;
            ImprovementFactory            = improvementFactory;
            CellSignals                   = cellSignals;
            AvailableImprovementTemplates = availableImprovementTemplates;
        }

        private void OnEnable() {
            PopulateImprovementList();

            CellSignals.ClickedSignal.Listen(OnCellClicked);

            RefreshMode();
        }

        private void OnDisable() {
            for(int i = InstantiatedRecords.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedRecords[i].gameObject);
            }

            InstantiatedRecords.Clear();

            CellSignals.ClickedSignal.Unlisten(OnCellClicked);
        }

        #endregion

        private void PopulateImprovementList() {
            foreach(var improvementTemplate in AvailableImprovementTemplates) {
                var newRecord = Instantiate(ImprovementRecordPrefab);

                newRecord.gameObject.SetActive(true);
                newRecord.SetParent(ImprovementRecordContainer, false);

                newRecord.GetComponentInChildren<Text>().text = improvementTemplate.name;

                var recordToggle = newRecord.GetComponentInChildren<Toggle>();

                var cachedTemplate = improvementTemplate;
                recordToggle.onValueChanged.AddListener(delegate(bool isOn) {
                    if(isOn) {
                        SelectedTemplate = cachedTemplate;
                    }
                });

                if(recordToggle.isOn) {
                    SelectedTemplate = improvementTemplate;
                }

                InstantiatedRecords.Add(newRecord);
            }
        }

        private void OnCellClicked(IHexCell cell, Vector3 location) {
            if(IsAdding && SelectedTemplate != null) {
                TryAddImprovementTo(SelectedTemplate, cell);
            }else if(IsRemoving) {
                TryRemoveImprovementsFrom(cell);
            }else if(IsPillaging) {
                TryPillageImprovementsOn(cell);
            }
        }

        private void TryAddImprovementTo(IImprovementTemplate template, IHexCell location) {
            if( ImprovementValidityLogic.IsTemplateValidForCell(template, location) &&
                ImprovementLocationCanon.CanPlaceImprovementOfTemplateAtLocation(template, location)
            ) {
                ImprovementFactory.BuildImprovement(template, location, 0, true, false);
            }
        }

        private void TryRemoveImprovementsFrom(IHexCell location) {
            foreach(var improvement in new List<IImprovement>(ImprovementLocationCanon.GetPossessionsOfOwner(location))) {
                Destroy(improvement.gameObject);
            }
        }

        private void TryPillageImprovementsOn(IHexCell location) {
            foreach(var improvement in new List<IImprovement>(ImprovementLocationCanon.GetPossessionsOfOwner(location))) {
                if(!improvement.IsPillaged) {
                    improvement.Pillage();
                }
            }
        }

        public void RefreshMode() {
            IsAdding    = IsAddingToggle   .isOn;
            IsRemoving  = IsRemovingToggle .isOn;
            IsPillaging = IsPillagingToggle.isOn;
        }

        #endregion

    }

}
