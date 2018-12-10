using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.UI.MapEditor {

    public class CivManagementPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown            CivTemplateDropdown;
        [SerializeField] private Button              CreateCivilizationButton;
        [SerializeField] private CivManagementRecord RecordPrefab;
        [SerializeField] private RectTransform       RecordContainer;

        private List<CivManagementRecord> InstantiatedRecords = new List<CivManagementRecord>();

        private ICivilizationTemplate SelectedTemplate;





        private ICivilizationFactory                      CivilizationFactory;
        private CivilizationSignals                       CivSignals;
        private ReadOnlyCollection<ICivilizationTemplate> CivTemplates;
        
        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICivilizationFactory civilizationFactory, CivilizationSignals civSignals,
            ReadOnlyCollection<ICivilizationTemplate> civTemplates
        ){
            CivilizationFactory = civilizationFactory;
            CivSignals          = civSignals;
            CivTemplates        = civTemplates;
        }

        #region Unity messages

        private void OnEnable() {
            Refresh();
        }

        private void OnDisable() {
            Clear();
        }

        #endregion

        public void CreateNewCivilization() {
            CivilizationFactory.Create(SelectedTemplate);

            Refresh();
        }

        public void UpdateSelectedCivTemplate(int optionIndex) {
            var templateName = CivTemplateDropdown.options[optionIndex].text;

            SelectedTemplate = CivTemplates.Where(template => template.Name.Equals(templateName)).FirstOrDefault();
        }

        private void Clear() {
            for(int i = InstantiatedRecords.Count - 1; i >=0; i--) {
                Destroy(InstantiatedRecords[i].gameObject);
            }

            InstantiatedRecords.Clear();
        }

        private void Refresh() {
            Clear();

            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                CivManagementRecord newRecord = Instantiate(RecordPrefab);

                newRecord.gameObject.SetActive(true);
                newRecord.transform.SetParent(RecordContainer, false);
                newRecord.NameField.text = civilization.Template.Name;

                var cachedCiv = civilization;
                newRecord.EditButton.onClick.AddListener(() => CivSignals.CivSelected.OnNext(cachedCiv));

                InstantiatedRecords.Add(newRecord);
            }

            CreateCivilizationButton.transform.SetAsLastSibling();

            RefreshCivTemplateDropdown();
        }

        private void RefreshCivTemplateDropdown() {
            CivTemplateDropdown.ClearOptions();

            List<Dropdown.OptionData> templateOptions = CivTemplates
                .Where (template => !CivilizationFactory.AllCivilizations.Any(civ => civ.Template == template))
                .Select(template => new Dropdown.OptionData(template.Name))
                .ToList();

            CivTemplateDropdown.AddOptions(templateOptions);

            CivTemplateDropdown.value = 0;

            UpdateSelectedCivTemplate(0);
        }

        #endregion

    }

}
