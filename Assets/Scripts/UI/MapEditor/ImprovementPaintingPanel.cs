﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.UI.MapEditor {

    public class ImprovementPaintingPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private RectTransform ImprovementRecordPrefab    = null;
        [SerializeField] private RectTransform ImprovementRecordContainer = null;
        [SerializeField] private RectTransform ImprovementRecordSection   = null;

        [SerializeField] private Toggle IsAddingToggle    = null;
        [SerializeField] private Toggle IsRemovingToggle  = null;
        [SerializeField] private Toggle IsPillagingToggle = null;

        private bool IsAdding;
        private bool IsRemoving;
        private bool IsPillaging;

        private IImprovementTemplate SelectedTemplate;

        private List<RectTransform> InstantiatedRecords = new List<RectTransform>();

        private IDisposable CellClickedSubscription;



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

            CellClickedSubscription = CellSignals.Clicked.Subscribe(OnCellClicked);

            RefreshMode();
        }

        private void OnDisable() {
            for(int i = InstantiatedRecords.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedRecords[i].gameObject);
            }

            InstantiatedRecords.Clear();

            CellClickedSubscription.Dispose();
        }

        #endregion

        public void RefreshMode() {
            IsAdding    = IsAddingToggle   .isOn;
            IsRemoving  = IsRemovingToggle .isOn;
            IsPillaging = IsPillagingToggle.isOn;

            ImprovementRecordSection.gameObject.SetActive(IsAdding);
        }

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

                if(SelectedTemplate == improvementTemplate) {
                    recordToggle.isOn = true;
                }

                InstantiatedRecords.Add(newRecord);
            }
        }

        private void OnCellClicked(Tuple<IHexCell, PointerEventData> data) {
            var cell = data.Item1;

            if(IsAdding && SelectedTemplate != null) {
                TryAddImprovementTo(SelectedTemplate, cell);
            }else if(IsRemoving) {
                TryRemoveImprovementsFrom(cell);
            }else if(IsPillaging) {
                TryPillageImprovementsOn(cell);
            }
        }

        private void TryAddImprovementTo(IImprovementTemplate template, IHexCell location) {
            if( ImprovementValidityLogic.IsTemplateValidForCell(template, location, true) &&
                ImprovementLocationCanon.CanPlaceImprovementOfTemplateAtLocation(template, location)
            ) {
                ImprovementFactory.BuildImprovement(template, location, 0, true, false);
            }
        }

        private void TryRemoveImprovementsFrom(IHexCell location) {
            foreach(var improvement in new List<IImprovement>(ImprovementLocationCanon.GetPossessionsOfOwner(location))) {
                improvement.Destroy();
            }
        }

        private void TryPillageImprovementsOn(IHexCell location) {
            foreach(var improvement in new List<IImprovement>(ImprovementLocationCanon.GetPossessionsOfOwner(location))) {
                if(!improvement.IsPillaged) {
                    improvement.Pillage();
                }
            }
        }

        #endregion

    }

}
