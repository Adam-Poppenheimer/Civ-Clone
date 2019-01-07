using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Players;

namespace Assets.UI.MapEditor {

    public class PlayerManagementPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown               CivTemplateDropdown;
        [SerializeField] private Button                 CreatePlayerButton;
        [SerializeField] private PlayerManagementRecord RecordPrefab;
        [SerializeField] private RectTransform          RecordContainer;

        private List<PlayerManagementRecord> InstantiatedRecords = new List<PlayerManagementRecord>();

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();

        private ICivilizationTemplate SelectedTemplate;




        
        private ICivilizationFactory                      CivilizationFactory;
        private IPlayerFactory                            PlayerFactory;
        private CivilizationSignals                       CivSignals;
        private ReadOnlyCollection<ICivilizationTemplate> CivTemplates;
        
        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICivilizationFactory civilizationFactory, IPlayerFactory playerFactory,
            CivilizationSignals civSignals, ReadOnlyCollection<ICivilizationTemplate> civTemplates
        ){
            CivilizationFactory = civilizationFactory;
            PlayerFactory       = playerFactory;
            CivSignals          = civSignals;
            CivTemplates        = civTemplates;
        }

        #region Unity messages

        private void OnEnable() {
            Refresh();

            SignalSubscriptions.Add(CivSignals.CivBeingDestroyed.Subscribe(civ => Refresh()));
        }

        private void OnDisable() {
            Clear();

            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
            SignalSubscriptions.Clear();
        }

        #endregion

        public void CreateNewPlayer() {
            var newCiv = CivilizationFactory.Create(SelectedTemplate);

            PlayerFactory.CreatePlayer(newCiv, PlayerFactory.HumanBrain);

            Refresh();
        }

        public void UpdateSelectedCivTemplate(int optionIndex) {
            var templateName = CivTemplateDropdown.options[optionIndex].text;

            SelectedTemplate = CivTemplates.Where(template => template.Name.Equals(templateName)).FirstOrDefault();
        }

        private void Clear() {
            for(int i = InstantiatedRecords.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedRecords[i].gameObject);
            }

            InstantiatedRecords.Clear();
        }

        private void Refresh() {
            Clear();

            foreach(var player in PlayerFactory.AllPlayers.Where(player => !player.ControlledCiv.Template.IsBarbaric)) {
                PlayerManagementRecord newRecord = Instantiate(RecordPrefab);

                newRecord.gameObject.SetActive(true);
                newRecord.transform.SetParent(RecordContainer, false);
                newRecord.NameField.text = player.Name;

                var cachedPlayer = player;
                newRecord.EditButton   .onClick.AddListener(() => CivSignals.CivSelected.OnNext(cachedPlayer.ControlledCiv));
                newRecord.DestroyButton.onClick.AddListener(() => DestroyPlayer(cachedPlayer));

                InstantiatedRecords.Add(newRecord);
            }

            CreatePlayerButton.transform.SetAsLastSibling();

            RefreshCivTemplateDropdown();
        }

        private void RefreshCivTemplateDropdown() {
            CivTemplateDropdown.ClearOptions();

            List<Dropdown.OptionData> templateOptions = CivTemplates
                .Where (template => !CivilizationFactory.AllCivilizations.Any(civ => civ.Template == template))
                .Select(template => new Dropdown.OptionData(template.Name))
                .ToList();

            CivTemplateDropdown.AddOptions(templateOptions);

            if(templateOptions.Count > 0) {
                CivTemplateDropdown.value = 0;

                UpdateSelectedCivTemplate(0);

                CreatePlayerButton.interactable = true;
            }else {
                CreatePlayerButton.interactable = false;
            }
        }

        private void DestroyPlayer(IPlayer player) {
            var civToDestroy = player.ControlledCiv;

            PlayerFactory.DestroyPlayer(player);
            civToDestroy.Destroy();
        }

        #endregion

    }

}
