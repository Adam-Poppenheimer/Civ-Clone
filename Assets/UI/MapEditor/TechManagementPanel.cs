using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Technology;
using Assets.Simulation.Civilizations;

using Assets.UI.Technology;

namespace Assets.UI.MapEditor {

    public class TechManagementPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown CivilizationDropdown;

        [SerializeField] private MapEditorTechRecord TechRecordPrefab;

        [SerializeField] private RectTransform TechRecordContainer;

        private ICivilization ActiveCivilization;

        private List<MapEditorTechRecord> InstantiatedTechRecords = new List<MapEditorTechRecord>();



        private ICivilizationFactory CivilizationFactory;

        private ITechCanon TechCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICivilizationFactory civilizationFactory, ITechCanon techCanon) {
            CivilizationFactory = civilizationFactory;
            TechCanon           = techCanon;
        }

        #region Unity messages

        private void OnEnable() {
            PopulateCivilizationDropdown();
        }

        private void OnDisable() {
            ActiveCivilization = null;
        }

        #endregion

        public void SetActiveCivilization(int index) {
            ActiveCivilization = CivilizationFactory.AllCivilizations[index];
            PopulateTechList();
        }

        private void PopulateCivilizationDropdown() {
            CivilizationDropdown.ClearOptions();

            List<Dropdown.OptionData> civilizationOptions = new List<Dropdown.OptionData>();
            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                civilizationOptions.Add(new Dropdown.OptionData(civilization.Template.Name));
            }

            CivilizationDropdown.AddOptions(civilizationOptions);
            SetActiveCivilization(CivilizationDropdown.value);
        }

        private void PopulateTechList() {
            for(int i = InstantiatedTechRecords.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedTechRecords[i].gameObject);
            }
            InstantiatedTechRecords.Clear();

            foreach(var technology in TechCanon.AvailableTechs) {
                var cachedTech = technology;

                var newRecord = Instantiate(TechRecordPrefab);
                
                newRecord.gameObject.SetActive(true);
                newRecord.transform.SetParent(TechRecordContainer, false);

                ConfigureRecord(newRecord, cachedTech);

                newRecord.RecordClicked += TechRecord_ToggleClicked;

                InstantiatedTechRecords.Add(newRecord);
            }
        }

        private void ConfigureRecord(MapEditorTechRecord record, ITechDefinition techToDisplay) {
            record.TechToDisplay = techToDisplay;

            if(TechCanon.IsTechDiscoveredByCiv(techToDisplay, ActiveCivilization)) {
                record.TechStatus = MapEditorTechRecord.StatusType.Discovered;

            }else if(TechCanon.IsTechAvailableToCiv(techToDisplay, ActiveCivilization)) {
                record.TechStatus = MapEditorTechRecord.StatusType.Available;

            }else {
                record.TechStatus = MapEditorTechRecord.StatusType.Unavailable;
            }

            record.Refresh();
        }

        private void TechRecord_ToggleClicked(object sender, EventArgs e) {
            var clickedRecord = sender as MapEditorTechRecord;
            var tech = clickedRecord.TechToDisplay;

            if(TechCanon.IsTechDiscoveredByCiv(tech, ActiveCivilization)) {

                var discoveredTechs = new List<ITechDefinition>(TechCanon.GetTechsDiscoveredByCiv(ActiveCivilization));
                UndiscoverTechAndDescendants(tech, ActiveCivilization, discoveredTechs);

            }else if(TechCanon.IsTechAvailableToCiv(tech, ActiveCivilization)) {

                TechCanon.SetTechAsDiscoveredForCiv(tech, ActiveCivilization);

            }

            foreach(var record in InstantiatedTechRecords) {
                ConfigureRecord(record, record.TechToDisplay);
            }
        }

        private void UndiscoverTechAndDescendants(ITechDefinition techToRemove, ICivilization civ, IEnumerable<ITechDefinition> discoveredTechs) {
            foreach(var postRequisite in discoveredTechs.Where(tech => tech.Prerequisites.Contains(techToRemove))){
                UndiscoverTechAndDescendants(postRequisite, civ, discoveredTechs);
            }

            TechCanon.SetTechAsUndiscoveredForCiv(techToRemove, civ);
        }

        #endregion

    }

}
