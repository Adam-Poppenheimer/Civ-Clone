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

    public class MapTileDisplay : TileDisplayBase {

        #region instance fields and properties

        [SerializeField] private Button CreateCityButton;

        [SerializeField] private CityBuilder CityBuilder;

        private ICityValidityLogic CityValidityLogic;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(ICityValidityLogic cityValidityLogic){
            CityValidityLogic = cityValidityLogic;
        }

        #region Unity message methods

        private void Start() {
            CreateCityButton.onClick.AddListener(() => CityBuilder.BuildFullCityOnTile(ObjectToDisplay));               
        }

        #endregion

        #region signal responses

        public override void Refresh() {
            if(CityValidityLogic.IsTileValidForCity(ObjectToDisplay)) {
                CreateCityButton.interactable = true;
            }else {
                CreateCityButton.interactable = false;
            }
        }

        #endregion

        #endregion

    }

}
