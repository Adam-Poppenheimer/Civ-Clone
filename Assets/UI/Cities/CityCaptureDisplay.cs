using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public class CityCaptureDisplay : MonoBehaviour, ICityCaptureDisplay {

        #region instance fields and properties

        public ICity TargetedCity { get; set; }

        [SerializeField] private Text DescriptionField;

        private string DescriptionFormat;


        private ICityRazer CityRazer;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityRazer cityRazer) {
            CityRazer = cityRazer;
        }

        #region Unity messages

        private void Awake() {
            DescriptionFormat = DescriptionField.text;
        }

        private void OnEnable() {
            if(TargetedCity != null) {
                Refresh();
            }
        }

        #endregion

        public void RazeCity() {
            CityRazer.RazeCity(TargetedCity);
        }

        private void Refresh() {
            DescriptionField.text = string.Format(
                DescriptionFormat, TargetedCity.Name, TargetedCity.Population
            );
        }

        #endregion

    }

}
