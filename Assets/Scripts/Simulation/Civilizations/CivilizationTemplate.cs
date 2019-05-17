using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    [CreateAssetMenu(menuName = "Civ Clone/Civilizations/Civ")]
    public class CivilizationTemplate : ScriptableObject, ICivilizationTemplate {

        #region instance fields and properties

        #region from ICivilizationTemplate

        public string Name {
            get { return name; }
        }

        public Color Color {
            get { return _color; }
        }
        [SerializeField] private Color _color = Color.clear;

        public bool IsBarbaric {
            get { return _isBarbaric; }
        }
        [SerializeField] private bool _isBarbaric = false;

        #endregion

        [SerializeField] private List<string> NameList = null;

        #endregion

        #region instance methods

        #region from ICivilizationTemplate

        public string GetNextName(IEnumerable<ICity> existingCities) {
            int cityVersionNumber = 1;
            string nameSuffix = "";

            while(cityVersionNumber < 1000) {
                foreach(var name in NameList) {
                    var nameToUse = name + nameSuffix;

                    if(!existingCities.Any(city => city.Name.Equals(nameToUse))) {
                        return nameToUse;
                    }
                }

                cityVersionNumber++;
                nameSuffix = string.Format(" {0}", cityVersionNumber);
            }

            return "City_Name";
        }

        #endregion

        #endregion
        
    }

}
