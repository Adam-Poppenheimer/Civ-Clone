using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Core;
using Assets.Simulation.GameMap;

namespace Assets.UI.GameMap {

    public class MapTileDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Button CreateCityButton;

        [SerializeField] private CityBuilder CityBuilder;

        private IMapTile SelectedTile { get; set; }

        private ICityValidityLogic CityValidityLogic;
        private IObservable<Unit> DeselectedSignal;
        private IDisposable DeselectSubscription;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            MapTileSignals signals, ICityValidityLogic cityValidityLogic,
            [Inject(Id = "MapTileDisplay Deselected")] IObservable<Unit> deselectedSignal
        ){
            signals.ClickedSignal.AsObservable.Subscribe(OnTileClicked);
            CityValidityLogic = cityValidityLogic;
            DeselectedSignal = deselectedSignal;
        }

        #region Unity message methods

        private void OnEnable() {
            if(SelectedTile != null) {
                CreateCityButton.onClick.AddListener(() => CityBuilder.BuildFullCityOnTile(SelectedTile));               
            }
            DeselectSubscription = DeselectedSignal.Subscribe(OnDeselected);
        }

        private void OnDisable() {
            CreateCityButton.onClick.RemoveAllListeners();
            DeselectSubscription.Dispose();
            DeselectSubscription = null;
        }

        #endregion

        #region signal responses

        private void OnTileClicked(Tuple<IMapTile, PointerEventData> dataTuple) {
            SelectedTile = dataTuple.Item1;

            if(CityValidityLogic.IsTileValidForCity(SelectedTile)) {
                CreateCityButton.interactable = true;
            }else {
                CreateCityButton.interactable = false;
            }

            gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        private void OnDeselected(Unit unit) {
            SelectedTile = null;
            gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
