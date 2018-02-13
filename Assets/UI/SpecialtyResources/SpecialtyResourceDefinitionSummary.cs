using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.SpecialtyResources;

namespace Assets.UI.SpecialtyResources {

    public class SpecialtyResourceDefinitionSummary : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text NameField;
        [SerializeField] private Text FreeCopiesField;
        [SerializeField] private Text TotalCopiesField;

        #endregion

        #region instance methods

        public void Initialize(ISpecialtyResourceDefinition resource, int freeCopies, int totalCopies) {
            NameField       .text = resource.name;
            FreeCopiesField .text = freeCopies .ToString();
            TotalCopiesField.text = totalCopies.ToString();
        }

        #endregion

    }

}
