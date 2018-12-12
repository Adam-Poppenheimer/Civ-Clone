using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation;
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

        private IDisposable CellClickedSubscription;
        private IDisposable CityClickedSubscription;





        private ICityFactory                                  CityFactory;
        private ICivilizationFactory                          CivilizationFactory;
        private ICityValidityLogic                            CityValidityLogic;
        private HexCellSignals                                CellSignals;
        private CitySignals                                   CitySignals;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICivilizationFactory civilizationFactory, ICityValidityLogic cityValidityLogic,
            HexCellSignals cellSignals, CitySignals citySignals, ICityFactory cityFactory,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ){
            CivilizationFactory = civilizationFactory;
            CityValidityLogic   = cityValidityLogic;
            CellSignals         = cellSignals;
            CitySignals         = citySignals;
            CityFactory         = cityFactory;
            CityLocationCanon   = cityLocationCanon;
            CityPossessionCanon = cityPossessionCanon;
        }

        #region Unity messages

        private void OnEnable() {
            PopulateCivilizationDropdown();

            CellClickedSubscription = CellSignals.ClickedSignal       .Subscribe(OnCellClicked);
            CityClickedSubscription = CitySignals.PointerClickedSignal.Subscribe(OnCityClicked);

            IsAddingToggle  .onValueChanged.AddListener(isOn => UpdatePaintingState( isOn));
            IsRemovingToggle.onValueChanged.AddListener(isOn => UpdatePaintingState(!isOn));
        }

        private void OnDisable() {
            CellClickedSubscription.Dispose();
            CityClickedSubscription.Dispose();

            IsAddingToggle  .onValueChanged.RemoveAllListeners();
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

        private void OnCellClicked(Tuple<IHexCell, PointerEventData> data) {
            var cell = data.Item1;

            if(IsAdding) {
                if(CityValidityLogic.IsCellValidForCity(cell)) {
                    var citiesOfCiv = CityPossessionCanon.GetPossessionsOfOwner(ActiveCivilization);

                    CityFactory.Create(cell, ActiveCivilization, ActiveCivilization.Template.GetNextName(citiesOfCiv));
                }
            }else {
                var cityAtLocation = CityLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
                if(cityAtLocation != null) {
                    cityAtLocation.Destroy();
                }
            }
        }

        private void OnCityClicked(ICity city) {
            if(!IsAdding) {
                city.Destroy();
            }
        }

        private void UpdatePaintingState(bool isAdding) {
            IsAdding = isAdding;

            CivilizationDropdown.gameObject.SetActive(isAdding);
        }

        #endregion

    }

}
