using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.MapResources;

namespace Assets.UI.SpecialtyResources {

    public class SpecialtyResourceDefinitionSummary : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text  NameField        = null;
        [SerializeField] private Text  FreeCopiesField  = null;
        [SerializeField] private Text  TotalCopiesField = null;
        [SerializeField] private Image IconField        = null;

        #endregion

        #region instance methods

        public void Initialize(IResourceDefinition resource, int freeCopies, int totalCopies) {
            if(NameField != null) {
                NameField.text = resource.name;
            }

            if(FreeCopiesField != null) {
                FreeCopiesField .text = freeCopies .ToString();
            }

            if(TotalCopiesField != null) {
                TotalCopiesField.text = totalCopies.ToString();
            }

            if(IconField != null) {
                IconField.sprite = resource.Icon;
            }            
        }

        #endregion

    }

}
