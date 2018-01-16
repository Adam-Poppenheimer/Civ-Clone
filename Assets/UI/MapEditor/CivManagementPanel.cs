using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.UI.MapEditor {

    public class CivManagementPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Button CreateCivilizationButton;

        [SerializeField] private CivManagementRecord RecordPrefab;

        [SerializeField] private List<Color> Colors;

        private List<CivManagementRecord> InstantiatedRecords = new List<CivManagementRecord>();

        private ICivilizationFactory CivilizationFactory;
        
        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICivilizationFactory civilizationFactory) {
            CivilizationFactory = civilizationFactory;
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
            ICivilization newCivilization = CivilizationFactory.Create("New Civilization", Color.gray);

            Refresh();
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
                newRecord.transform.SetParent(this.transform, false);

                newRecord.NameField.text = civilization.Name;
                newRecord.NameField.onEndEdit.AddListener(text => civilization.Name = text);

                newRecord.ColorDropdown.value = Colors.FindIndex(color => color == civilization.Color);
                newRecord.ColorDropdown.onValueChanged.AddListener(newColorIndex => civilization.Color = Colors[newColorIndex]);

                InstantiatedRecords.Add(newRecord);
            }

            CreateCivilizationButton.transform.SetAsLastSibling();
        }

        #endregion

    }

}
