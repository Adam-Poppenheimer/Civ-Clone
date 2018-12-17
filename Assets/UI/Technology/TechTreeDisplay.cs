using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI.Extensions;

using Zenject;

using Assets.Simulation.Technology;
using Assets.Simulation.Civilizations;

using Assets.UI.Civilizations;

namespace Assets.UI.Technology {

    public class TechTreeDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        public TechSelectionMode SelectionMode { get; set; }

        [SerializeField] private TechnologyRecord TechRecordPrefab;

        [SerializeField] private UILineRenderer PrerequisiteLines;

        [SerializeField] private TableLayoutGroup TechTable;
        [SerializeField] private int TechTableRowCount;
        [SerializeField] private RectTransform TechTableCellPrefab;

        private List<TechnologyRecord> TechRecords;




        private ITechCanon              TechCanon;
        private ICivilizationYieldLogic YieldLogic;
        private DiContainer             Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ITechCanon techCanon, ICivilizationYieldLogic yieldLogic,
            DiContainer container
        ){
            TechCanon  = techCanon;
            YieldLogic = yieldLogic;
            Container  = container;
        }

        #region from CivilizationDisplayBase

        public override void Refresh() {
            if(TechRecords == null) {
                SetUpTechTable();
                SetUpTechTree();
            }

            foreach(var techRecord in TechRecords) {
                var recordedTech = techRecord.TechToDisplay;

                techRecord.CurrentProgress = TechCanon.GetProgressOnTechByCiv(recordedTech, ObjectToDisplay);

                if(TechCanon.IsTechDiscoveredByCiv(techRecord.TechToDisplay, ObjectToDisplay)) {

                    techRecord.Status = TechnologyRecord.TechStatus.Discovered;

                }else if(ObjectToDisplay.TechQueue.Contains(techRecord.TechToDisplay)) {

                    if(ObjectToDisplay.TechQueue.Peek() == techRecord.TechToDisplay) {
                        techRecord.Status = TechnologyRecord.TechStatus.BeingResearched;
                    }else {
                        techRecord.Status = TechnologyRecord.TechStatus.InQueue;
                    }

                }else if(
                    SelectionMode != TechSelectionMode.ResearchFreeTech ||
                    TechCanon.IsTechAvailableToCiv(recordedTech, ObjectToDisplay)
                ){
                    techRecord.Status = TechnologyRecord.TechStatus.Available;

                }else {
                    techRecord.Status = TechnologyRecord.TechStatus.Unavailable;
                }

                techRecord.CanBeClicked = SelectionMode != TechSelectionMode.SetTechQueue
                                       || !TechCanon.IsTechDiscoveredByCiv(recordedTech, ObjectToDisplay);

                float sciencePerTurn = YieldLogic.GetYieldOfCivilization(ObjectToDisplay)[Simulation.YieldType.Science];

                if(sciencePerTurn > 0) {
                    techRecord.TurnsToResearch = (int)Math.Ceiling(
                        (techRecord.TechToDisplay.Cost - techRecord.CurrentProgress) / sciencePerTurn
                    );
                }else {
                    techRecord.TurnsToResearch = -1;
                }

                techRecord.Refresh();
            }
        }

        #endregion

        private void SetUpTechTable() {
            for(int rowIndex = 0; rowIndex < TechTableRowCount; rowIndex++) {
                for(int columnIndex = 0; columnIndex < TechTable.ColumnWidths.Length; columnIndex++) {
                    var tableCell = Instantiate(TechTableCellPrefab);

                    tableCell.gameObject.SetActive(true);
                    tableCell.transform.SetParent(TechTable.transform, false);
                    tableCell.name = string.Format("Cell [{0}, {1}]", rowIndex, columnIndex);
                }
            }
        }

        private void SetUpTechTree() {
            TechRecords = new List<TechnologyRecord>();

            foreach(var technology in TechCanon.AvailableTechs) {
                var newTechRecord = Container.InstantiatePrefabForComponent<TechnologyRecord>(TechRecordPrefab);

                newTechRecord.name = string.Format("{0} Record", technology.Name);
                newTechRecord.gameObject.SetActive(true);

                var cellToPlaceInto = TechTable.transform.GetChild(
                    technology.TechTableColumn + technology.TechTableRow * TechTable.ColumnWidths.Length
                );

                newTechRecord.transform.SetParent(cellToPlaceInto, false);

                newTechRecord.TechToDisplay = technology;
                newTechRecord.SelectionButton.onClick.AddListener(() => OnTechRecordClicked(newTechRecord));
                newTechRecord.Refresh();

                TechRecords.Add(newTechRecord);
            }

            StartCoroutine(DrawPrerequisiteLinesCoroutine());
        }

        private IEnumerator DrawPrerequisiteLinesCoroutine() {
            yield return new WaitForEndOfFrame();

            var lineList = new List<Vector2>();

            foreach(var recordOfCurrent in TechRecords) {
                var currentCellTransform = recordOfCurrent.transform.parent.GetComponent<RectTransform>();

                var currentConnectionPoint = currentCellTransform.localPosition
                    - new Vector3(currentCellTransform.rect.width / 2f, 0f, 0f);

                foreach(var prerequisite in recordOfCurrent.TechToDisplay.Prerequisites) {
                    var recordOfPrerequisite = TechRecords.Where(record => record.TechToDisplay == prerequisite).First();

                    var prerequisiteCellTransform = recordOfPrerequisite.transform.parent.GetComponent<RectTransform>();
                    var prerequisiteConnectionPoint = prerequisiteCellTransform.localPosition
                        + new Vector3(prerequisiteCellTransform.rect.width / 2f, 0f, 0f);

                    lineList.Add(prerequisiteConnectionPoint);
                    lineList.Add(currentConnectionPoint);
                }
            }

            PrerequisiteLines.Points = lineList.ToArray();
        }

        private void OnTechRecordClicked(TechnologyRecord techRecord) {
            switch(SelectionMode) {
                case TechSelectionMode.SetTechQueue:       OnTechRecordClicked_SetTechQueue      (techRecord); break;
                case TechSelectionMode.SetDiscoveredTechs: OnTechRecordClicked_SetDiscoveredTechs(techRecord); break;
                case TechSelectionMode.ResearchFreeTech:   OnTechRecordClicked_ResearchFreeTech  (techRecord); break;
                default: throw new NotImplementedException();
            }
        }

        private void OnTechRecordClicked_SetTechQueue(TechnologyRecord techRecord) {
            if(!Input.GetButton("Shift")) {
                ObjectToDisplay.TechQueue.Clear();
            }

            foreach(var tech in TechCanon.GetPrerequisiteChainToResearchTech(techRecord.TechToDisplay, ObjectToDisplay)) {
                if(!ObjectToDisplay.TechQueue.Contains(tech)) {
                    ObjectToDisplay.TechQueue.Enqueue(tech);
                }
            }

            Refresh();
        }

        private void OnTechRecordClicked_SetDiscoveredTechs(TechnologyRecord techRecord) {
            var techClicked = techRecord.TechToDisplay;

            if(TechCanon.IsTechDiscoveredByCiv(techClicked, ObjectToDisplay)) {
                foreach(var postrequisiteTech in TechCanon.GetDiscoveredPostrequisiteTechs(techClicked, ObjectToDisplay)) {
                    TechCanon.SetTechAsUndiscoveredForCiv(postrequisiteTech, ObjectToDisplay);
                }

                TechCanon.SetTechAsUndiscoveredForCiv(techClicked, ObjectToDisplay);

            }else {
                foreach(var tech in TechCanon.GetPrerequisiteChainToResearchTech(techClicked, ObjectToDisplay)) {
                    TechCanon.SetTechAsDiscoveredForCiv(tech, ObjectToDisplay);
                }
            }

            Refresh();
        }

        private void OnTechRecordClicked_ResearchFreeTech(TechnologyRecord techRecord) {
            if( TechCanon.IsTechAvailableToCiv(techRecord.TechToDisplay, ObjectToDisplay) &&
                TechCanon.GetFreeTechsForCiv(ObjectToDisplay) > 0
            ) {
                TechCanon.SetTechAsDiscoveredForCiv(techRecord.TechToDisplay, ObjectToDisplay);

                TechCanon.RemoveFreeTechFromCiv(ObjectToDisplay);

                Refresh();
            }
        }

        #endregion

    }

}
