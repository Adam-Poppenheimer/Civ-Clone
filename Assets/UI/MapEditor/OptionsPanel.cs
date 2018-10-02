using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Core;
using Assets.Simulation.Visibility;
using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.UI.MapEditor {

    public class OptionsPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown VisibilityModeDropdown;
        [SerializeField] private Dropdown ActiveCivDropdown;




        private IVisibilityCanon     VisibilityCanon;
        private ICivilizationFactory CivFactory;
        private IGameCore            GameCore;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IVisibilityCanon visibilityCanon, ICivilizationFactory civFactory, IGameCore gameCore
        ) {
            VisibilityCanon = visibilityCanon;
            CivFactory      = civFactory;
            GameCore        = gameCore;
        }

        #region Unity messages

        private void Start() {
            InitializeVisibilityModeDropdown();
        }

        private void OnEnable() {
            SetUpActiveCivDropdown();
        }

        #endregion

        public void UpdateSelectedVisibilityMode(int optionIndex) {
            var modeName = VisibilityModeDropdown.options[optionIndex].text;

            var newMode = EnumUtil.GetValues<CellVisibilityMode>().Where(
                mode => mode.ToString().Equals(modeName)
            ).FirstOrDefault();

            VisibilityCanon.CellVisibilityMode = newMode;
        }

        public void UpdateActiveCiv(int optionIndex) {
            var civName = ActiveCivDropdown.options[optionIndex].text;

            var newActiveCiv = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(civName)).FirstOrDefault();

            GameCore.ActiveCivilization = newActiveCiv;
        }

        private void InitializeVisibilityModeDropdown() {
            VisibilityModeDropdown.ClearOptions();

            List<Dropdown.OptionData> modeOptions = EnumUtil.GetValues<CellVisibilityMode>().Select(
                mode => new Dropdown.OptionData(mode.ToString())
            ).ToList();

            VisibilityModeDropdown.AddOptions(modeOptions);

            Dropdown.OptionData currentModeOption = VisibilityModeDropdown.options.Where(
                option => option.text.Equals(VisibilityCanon.CellVisibilityMode.ToString()
            )).FirstOrDefault();

            VisibilityModeDropdown.value = VisibilityModeDropdown.options.IndexOf(currentModeOption);
        }

        private void SetUpActiveCivDropdown() {
            ActiveCivDropdown.ClearOptions();

            List<Dropdown.OptionData> civOptions = CivFactory.AllCivilizations.Select(
                civ => new Dropdown.OptionData(civ.Name)
            ).ToList();

            ActiveCivDropdown.AddOptions(civOptions);

            var activeCivOption = ActiveCivDropdown.options.Where(
                option => GameCore.ActiveCivilization != null ? option.text.Equals(GameCore.ActiveCivilization.Name) : false
            ).FirstOrDefault();

            if(activeCivOption != null) {
                ActiveCivDropdown.value = ActiveCivDropdown.options.IndexOf(activeCivOption);
            }
        }

        #endregion

    }

}
