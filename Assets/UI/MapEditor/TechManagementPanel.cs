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

        [SerializeField] private TechnologyRecord TechRecordPrefab;

        [SerializeField] private RectTransform TechRecordContainer;

        private ICivilization ActiveCivilization;

        private List<TechnologyRecord> InstantiatedTechRecords = new List<TechnologyRecord>();



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
                civilizationOptions.Add(new Dropdown.OptionData(civilization.Name));
            }

            CivilizationDropdown.AddOptions(civilizationOptions);
            SetActiveCivilization(CivilizationDropdown.value);
        }

        private void PopulateTechList() {
            for(int i = InstantiatedTechRecords.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedTechRecords[i].gameObject);
            }
            InstantiatedTechRecords.Clear();

            foreach(var technology in TechCanon.AllTechs) {
                var cachedTech = technology;

                var newRecord = Instantiate(TechRecordPrefab);
                
                newRecord.gameObject.SetActive(true);
                newRecord.transform.SetParent(TechRecordContainer);

                ConfigureRecord(newRecord, cachedTech);

                newRecord.RecordClicked += TechRecord_ToggleClicked;

                InstantiatedTechRecords.Add(newRecord);
            }
        }

        private void ConfigureRecord(TechnologyRecord record, ITechDefinition techToDisplay) {
            record.TechToDisplay = techToDisplay;

            if(TechCanon.IsTechDiscoveredByCiv(techToDisplay, ActiveCivilization)) {
                record.TechStatus = TechnologyRecord.StatusType.Discovered;

            }else if(TechCanon.IsTechAvailableToCiv(techToDisplay, ActiveCivilization)) {
                record.TechStatus = TechnologyRecord.StatusType.Available;

            }else {
                record.TechStatus = TechnologyRecord.StatusType.Unavailable;
            }

            record.Refresh();
        }

        private void TechRecord_ToggleClicked(object sender, EventArgs e) {
            var clickedRecord = sender as TechnologyRecord;
            var tech = clickedRecord.TechToDisplay;

            if(TechCanon.IsTechDiscoveredByCiv(tech, ActiveCivilization)) {
                TechCanon.SetTechAsUndiscoveredForCiv(tech, ActiveCivilization);                

            }else if(TechCanon.IsTechAvailableToCiv(tech, ActiveCivilization)) {
                TechCanon.SetTechAsDiscoveredForCiv(tech, ActiveCivilization);

            }

            foreach(var record in InstantiatedTechRecords) {
                ConfigureRecord(record, record.TechToDisplay);
            }
        }

        #endregion

    }

}
