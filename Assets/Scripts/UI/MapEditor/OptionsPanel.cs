using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Core;
using Assets.Simulation.Visibility;
using Assets.Simulation.Players;

using UnityCustomUtilities.Extensions;

namespace Assets.UI.MapEditor {

    public class OptionsPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown CellVisibilityModeDropdown;
        [SerializeField] private Dropdown ResourceVisibilityModeDropdown;
        [SerializeField] private Dropdown ExplorationModeDropdown;
        [SerializeField] private Dropdown ActivePlayerDropdown;




        private IVisibilityCanon  VisibilityCanon;
        private IExplorationCanon ExplorationCanon;
        private IPlayerFactory    PlayerFactory;
        private IGameCore         GameCore;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IVisibilityCanon visibilityCanon, IExplorationCanon explorationCanon,
            IPlayerFactory playerFactory, IGameCore gameCore
        ) {
            VisibilityCanon  = visibilityCanon;
            ExplorationCanon = explorationCanon;
            PlayerFactory    = playerFactory;
            GameCore         = gameCore;
        }

        #region Unity messages

        private void Start() {
            InitializeEnumDropdown(CellVisibilityModeDropdown,     VisibilityCanon .CellVisibilityMode);
            InitializeEnumDropdown(ResourceVisibilityModeDropdown, VisibilityCanon .ResourceVisibilityMode);
            InitializeEnumDropdown(ExplorationModeDropdown,        ExplorationCanon.ExplorationMode);
        }

        private void OnEnable() {
            SetUpActivePlayerDropdown();
        }

        #endregion

        public void UpdateSelectedCellVisibilityMode(int optionIndex) {
            var modeName = CellVisibilityModeDropdown.options[optionIndex].text;

            var newMode = EnumUtil.GetValues<CellVisibilityMode>().Where(
                mode => mode.ToString().Equals(modeName)
            ).FirstOrDefault();

            VisibilityCanon.CellVisibilityMode = newMode;
        }

        public void UpdateSelectedResourceVisibilityMode(int optionIndex) {
            var modeName = ResourceVisibilityModeDropdown.options[optionIndex].text;

            var newMode = EnumUtil.GetValues<ResourceVisibilityMode>().Where(
                mode => mode.ToString().Equals(modeName)
            ).FirstOrDefault();

            VisibilityCanon.ResourceVisibilityMode = newMode;
        }

        public void UpdateSelectedExplorationMode(int optionIndex) {
            var modeName = ExplorationModeDropdown.options[optionIndex].text;

            var newMode = EnumUtil.GetValues<CellExplorationMode>().Where(
                mode => mode.ToString().Equals(modeName)
            ).FirstOrDefault();

            ExplorationCanon.ExplorationMode = newMode;
        }

        public void UpdateActivePlayer(int optionIndex) {
            var civName = ActivePlayerDropdown.options[optionIndex].text;

            var newActivePlayer = PlayerFactory.AllPlayers.Where(player => player.Name.Equals(civName)).FirstOrDefault();

            GameCore.ActivePlayer = newActivePlayer;
        }

        private void InitializeEnumDropdown<TEnum>(Dropdown dropdown, TEnum selectedOption) {
            dropdown.ClearOptions();

            List<Dropdown.OptionData> options = EnumUtil.GetValues<TEnum>().Select(
                mode => new Dropdown.OptionData(mode.ToString())
            ).ToList();

            dropdown.AddOptions(options);

            Dropdown.OptionData currentOption = dropdown.options.Where(
                option => option.text.Equals(selectedOption.ToString()
            )).FirstOrDefault();

            dropdown.value = dropdown.options.IndexOf(currentOption);
        }

        private void SetUpActivePlayerDropdown() {
            ActivePlayerDropdown.ClearOptions();

            List<Dropdown.OptionData> civOptions = PlayerFactory.AllPlayers.Select(
                player => new Dropdown.OptionData(player.Name)
            ).ToList();

            ActivePlayerDropdown.AddOptions(civOptions);

            var activePlayerOption = ActivePlayerDropdown.options.Where(
                option => GameCore.ActivePlayer != null ? option.text.Equals(GameCore.ActivePlayer.Name) : false
            ).FirstOrDefault();

            if(activePlayerOption != null) {
                ActivePlayerDropdown.value = ActivePlayerDropdown.options.IndexOf(activePlayerOption);
            }
        }

        #endregion

    }

}
