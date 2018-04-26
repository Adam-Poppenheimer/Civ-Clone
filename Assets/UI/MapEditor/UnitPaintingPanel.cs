using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.UI.MapEditor {

    public class UnitPaintingPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown CivilizationDropdown;

        [SerializeField] private RectTransform UnitRecordPrefab;
        [SerializeField] private RectTransform UnitRecordContainer;

        [SerializeField] private Toggle IsAddingToggle;
        [SerializeField] private Toggle IsRemovingToggle;

        private bool IsAdding = true;

        private IUnitTemplate ActiveTemplate;
        private ICivilization ActiveCivilization;

        private List<RectTransform> InstantiatedRecords = new List<RectTransform>();        

        private IDisposable UnitClickedSubscription;




        private ICivilizationFactory CivilizationFactory;

        private IUnitFactory UnitFactory;

        private HexCellSignals CellSignals;

        private List<IUnitTemplate> AvailableTemplates;

        private IUnitPositionCanon UnitPositionCanon;

        private UnitSignals UnitSignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICivilizationFactory civilizationFactory, IUnitFactory unitFactory, HexCellSignals cellSignals,
            [Inject(Id = "Available Unit Templates")] IEnumerable<IUnitTemplate> availableTemplates,
            IUnitPositionCanon unitPositionCanon, UnitSignals unitSignals
        ){
            CivilizationFactory = civilizationFactory;
            UnitFactory         = unitFactory;
            CellSignals         = cellSignals;
            AvailableTemplates  = availableTemplates.ToList();
            UnitPositionCanon   = unitPositionCanon;
            UnitSignals         = unitSignals;
        }

        #region Unity messages

        private void OnEnable() {
            PopulateCivilizationDropdown();
            PopulateUnitList();

            CellSignals.ClickedSignal.Listen(OnCellClicked);

            UnitClickedSubscription = UnitSignals.ClickedSignal.Subscribe(OnUnitClicked);

            IsAddingToggle.onValueChanged.AddListener(isOn => IsAdding = isOn);
            IsRemovingToggle.onValueChanged.AddListener(isOn => IsAdding = !isOn);
        }

        private void OnDisable() {
            for(int i = InstantiatedRecords.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedRecords[i].gameObject);
            }

            InstantiatedRecords.Clear();

            CellSignals.ClickedSignal.Unlisten(OnCellClicked);

            UnitClickedSubscription.Dispose();

            IsAddingToggle.onValueChanged.RemoveAllListeners();
            IsRemovingToggle.onValueChanged.RemoveAllListeners();
        }

        #endregion

        public void SetActiveCivilization(int index) {
            ActiveCivilization = CivilizationFactory.AllCivilizations[index];
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

        private void PopulateUnitList() {
            foreach(var unitTemplate in AvailableTemplates) {
                var cachedUnitTemplate = unitTemplate;

                var newRecord = Instantiate(UnitRecordPrefab);

                newRecord.gameObject.SetActive(true);
                newRecord.transform.SetParent(UnitRecordContainer, false);

                newRecord.GetComponentInChildren<Text>().text = unitTemplate.name;

                var recordToggle = newRecord.GetComponentInChildren<Toggle>();

                recordToggle.onValueChanged.AddListener(delegate(bool isOn) {
                    if(isOn) {
                        ActiveTemplate = cachedUnitTemplate;
                    }
                });

                if(recordToggle.isOn) {
                    ActiveTemplate = unitTemplate;
                }

                InstantiatedRecords.Add(newRecord);
            }
        }

        private void OnCellClicked(IHexCell cell, Vector3 location) {
            if(IsAdding) {
                TryAddUnit(cell);
            }else {
                TryRemoveUnit(cell);
            }
        }

        private void TryAddUnit(IHexCell location) {
            if( ActiveCivilization != null && ActiveTemplate != null &&
                UnitPositionCanon.CanPlaceUnitTemplateAtLocation(ActiveTemplate, location, false)
            ){
                UnitFactory.Create(location, ActiveTemplate, ActiveCivilization);
            }
        }

        private void TryRemoveUnit(IHexCell location) {
            foreach(var unit in new List<IUnit>(UnitPositionCanon.GetPossessionsOfOwner(location))) {
                unit.Destroy();
            }
        }

        private void OnUnitClicked(IUnit unit) {
            if(!IsAdding) {
                unit.Destroy();
            }
        }

        #endregion

    }

}
