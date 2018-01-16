using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class CityPaintingPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown CivilizationDropdown;

        [SerializeField] private Toggle IsAddingToggle;
        [SerializeField] private Toggle IsRemovingToggle;

        private ICivilization ActiveCivilization;

        private bool IsAdding = true;

        private IDisposable CityClickedSubscription;


        private ICityFactory CityFactory;

        private ICivilizationFactory CivilizationFactory;

        private ICityValidityLogic CityValidityLogic;

        private HexCellSignals CellSignals;

        private CitySignals CitySignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICityFactory cityFactory, ICivilizationFactory civilizationFactory,
            ICityValidityLogic cityValidityLogic, HexCellSignals cellSignals, CitySignals citySignals
        ){
            CityFactory         = cityFactory;
            CivilizationFactory = civilizationFactory;
            CityValidityLogic   = cityValidityLogic;
            CellSignals         = cellSignals;
            CitySignals         = citySignals;
        }

        #region Unity messages

        private void OnEnable() {
            PopulateCivilizationDropdown();

            CellSignals.ClickedSignal.Listen(OnCellClicked);

            CityClickedSubscription = CitySignals.CityClickedSignal.Subscribe(OnCityClicked);

            IsAddingToggle.onValueChanged.AddListener(isOn => IsAdding = isOn);
            IsRemovingToggle.onValueChanged.AddListener(isOn => IsAdding = !isOn);
        }

        private void OnDisable() {
            CellSignals.ClickedSignal.Unlisten(OnCellClicked);

            CityClickedSubscription.Dispose();

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

        private void OnCellClicked(IHexCell cell, Vector3 location) {
            if(IsAdding) {
                if(CityValidityLogic.IsCellValidForCity(cell)) {
                    CityFactory.Create(cell, ActiveCivilization);
                }
            }else {
                var cityAtLocation = CityFactory.AllCities.Where(city => city.Location == cell).FirstOrDefault();
                if(cityAtLocation != null) {
                    Destroy(cityAtLocation.transform.gameObject);
                }
            }
        }

        private void OnCityClicked(ICity city) {
            if(!IsAdding) {
                Destroy(city.transform.gameObject);
            }
        }

        #endregion

    }

}
