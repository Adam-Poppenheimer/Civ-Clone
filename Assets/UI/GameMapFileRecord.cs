using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.MapManagement;

namespace Assets.UI {

    public class GameMapFileRecord : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text NameField;
        [SerializeField] private Text DateModifiedField;
        [SerializeField] private Text TimeModifiedField;

        #endregion

        #region instance methods

        public void Refresh(MapFileData fileData) {
            NameField.text = fileData.FileName;

            DateModifiedField.text = fileData.LastModified.ToString("M/d/yy");
            TimeModifiedField.text = fileData.LastModified.ToString("h:m tt");
        }

        #endregion

    }

}
