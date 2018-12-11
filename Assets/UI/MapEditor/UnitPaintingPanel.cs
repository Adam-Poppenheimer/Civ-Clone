using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        private IDisposable CellClickedSubscription;




        private ICivilizationFactory CivilizationFactory;
        private IUnitFactory         UnitFactory;
        private HexCellSignals       CellSignals;
        private List<IUnitTemplate>  AvailableTemplates;
        private IUnitPositionCanon   UnitPositionCanon;
        private UnitSignals          UnitSignals;

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

            UnitClickedSubscription = UnitSignals.ClickedSignal.Subscribe(OnUnitClicked);
            CellClickedSubscription = CellSignals.ClickedSignal.Subscribe(OnCellClicked);

            IsAddingToggle  .onValueChanged.AddListener(isOn => IsAdding = isOn);
            IsRemovingToggle.onValueChanged.AddListener(isOn => IsAdding = !isOn);
        }

        private void OnDisable() {
            for(int i = InstantiatedRecords.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedRecords[i].gameObject);
            }

            InstantiatedRecords.Clear();

            UnitClickedSubscription.Dispose();
            CellClickedSubscription.Dispose();

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
                civilizationOptions.Add(new Dropdown.OptionData(civilization.Template.Name));
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

                }else if(ActiveTemplate == unitTemplate) {
                    recordToggle.isOn = true;
                }

                InstantiatedRecords.Add(newRecord);
            }
        }

        private void OnCellClicked(Tuple<IHexCell, PointerEventData> data) {
            if(IsAdding) {
                TryAddUnit(data.Item1);
            }else {
                TryRemoveUnit(data.Item1);
            }
        }

        private void TryAddUnit(IHexCell location) {
            if( ActiveCivilization != null && ActiveTemplate != null &&
                UnitFactory.CanBuildUnit(location, ActiveTemplate, ActiveCivilization)
            ){
                UnitFactory.BuildUnit(location, ActiveTemplate, ActiveCivilization);
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
