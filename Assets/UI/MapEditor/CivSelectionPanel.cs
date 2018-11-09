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

    public class CivSelectionPanel : MonoBehaviour {

        #region instance fields and properties

        public Action<ICivilization> SelectedCivChangedAction { get; set; }

        [SerializeField] private Dropdown CivilizationDropdown;

        private ICivilization ActiveCivilization;



        private ICivilizationFactory CivilizationFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICivilizationFactory civilizationFactory) {
            CivilizationFactory = civilizationFactory;
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

            if(SelectedCivChangedAction != null) {
                SelectedCivChangedAction(ActiveCivilization);
            }
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

        #endregion

    }

}
