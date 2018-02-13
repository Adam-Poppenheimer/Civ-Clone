using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.SpecialtyResources;

namespace Assets.UI.SpecialtyResources {

    public class ResourceNodeDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text ResourceNameField;
        [SerializeField] private Text CopiesField;

        #endregion

        #region instance methods

        public void DisplayNode(IResourceNode node) {
            ResourceNameField.text = node.Resource.name;
            CopiesField.text = node.Copies.ToString();
        }

        #endregion

    }

}
