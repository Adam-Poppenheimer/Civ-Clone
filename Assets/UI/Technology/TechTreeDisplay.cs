using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

using Zenject;

using Assets.Simulation.Technology;

using Assets.UI.Civilizations;

namespace Assets.UI.Technology {

    public class TechTreeDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        [SerializeField] private TechnologyRecord TechRecordPrefab;
        
        [SerializeField] private RectTransform TechRecordContainer;

        [SerializeField] private UILineRenderer PrerequisiteLines;

        private List<TechnologyRecord> TechRecords = new List<TechnologyRecord>();


        private ITechCanon TechCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITechCanon techCanon) {
            TechCanon = techCanon;
        }

        #region Unity messages

        private void Start() {
            SetUpTechTree();
        }

        #endregion

        #region from CivilizationDisplayBase

        public override void Refresh() {
            foreach(var techRecord in TechRecords) {
                if(TechCanon.IsTechDiscoveredByCiv(techRecord.TechToDisplay, ObjectToDisplay)) {
                    techRecord.Status = TechnologyRecord.TechStatus.Discovered;

                }else if(ObjectToDisplay.TechQueue.Contains(techRecord.TechToDisplay)) {
                    if(ObjectToDisplay.TechQueue.Peek() == techRecord.TechToDisplay) {
                        techRecord.Status = TechnologyRecord.TechStatus.BeingResearched;
                    }else {
                        techRecord.Status = TechnologyRecord.TechStatus.InQueue;
                    }
                }else {
                    techRecord.Status = TechnologyRecord.TechStatus.Available;
                }

                techRecord.Refresh();
            }
        }

        #endregion

        private void SetUpTechTree() {
            foreach(var technology in TechCanon.AllTechs) {
                var newTechRecord = Instantiate(TechRecordPrefab);

                newTechRecord.gameObject.SetActive(true);
                newTechRecord.transform.SetParent(TechRecordContainer, false);
                newTechRecord.transform.localPosition = technology.TechScreenPosition;

                newTechRecord.TechToDisplay = technology;
                newTechRecord.SelectionButton.onClick.AddListener(() => OnTechRecordClicked(newTechRecord));
                newTechRecord.Refresh();

                TechRecords.Add(newTechRecord);
            }

            var lineList = new List<Vector2>();

            foreach(var recordOfCurrent in TechRecords) {
                foreach(var prerequisite in recordOfCurrent.TechToDisplay.Prerequisites) {
                    var recordOfPrerequisite = TechRecords.Where(record => record.TechToDisplay == prerequisite).First();

                    lineList.Add(recordOfPrerequisite.ForwardConnectionPoint);
                    lineList.Add(recordOfCurrent.BackwardConnectionPoint);
                }
            }

            PrerequisiteLines.Points = lineList.ToArray();
        }

        private void OnTechRecordClicked(TechnologyRecord techRecord) {
            if(!Input.GetButton("Shift")) {
                ObjectToDisplay.TechQueue.Clear();
            }

            foreach(var tech in TechCanon.GetPrerequisiteChainToResearchTech(techRecord.TechToDisplay, ObjectToDisplay)) {
                ObjectToDisplay.TechQueue.Enqueue(tech);
            }

            Refresh();
        }

        #endregion

    }

}
