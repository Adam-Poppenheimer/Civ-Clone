using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.MapManagement;

namespace Assets.UI.Common {

    public class GameMapFileRecord : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text NameField         = null;
        [SerializeField] private Text DateModifiedField = null;
        [SerializeField] private Text TimeModifiedField = null;

        [SerializeField] private Toggle SelectionToggle = null;

        private MapFileData DataToDisplay;

        #endregion

        #region instance methods

        public void Refresh(MapFileData fileData) {
            DataToDisplay = fileData;

            NameField.text = fileData.FileName;

            DateModifiedField.text = fileData.LastModified.ToString("M/d/yy");
            TimeModifiedField.text = fileData.LastModified.ToString("h:m tt");
        }

        public void BindSelectionToggle(ToggleGroup group, Action<MapFileData> callback) {
            if(SelectionToggle == null) {
                return;
            }

            SelectionToggle.isOn = false;

            group.RegisterToggle(SelectionToggle);
            SelectionToggle.group = group;

            SelectionToggle.onValueChanged.AddListener(delegate(bool isOn) {
                if(isOn) {
                    callback(DataToDisplay);
                }
            });
        }

        #endregion

    }

}
