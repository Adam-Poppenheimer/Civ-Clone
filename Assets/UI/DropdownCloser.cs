using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.UI {

    public class DropdownCloser : MonoBehaviour {

        #region instance methods

        #region Unity messages

        private void OnDisable() {
            var dropdownList = transform.Find("Dropdown List");

            if(dropdownList != null) {
                Destroy(dropdownList.gameObject);
            }
        }

        #endregion

        #endregion

    }

}
