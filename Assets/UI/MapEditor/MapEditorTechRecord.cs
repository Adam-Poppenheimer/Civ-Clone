using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.Technology;

namespace Assets.UI.MapEditor {

    public class MapEditorTechRecord : MonoBehaviour {

        #region internal types

        public enum StatusType {
            Available, Unavailable, Discovered
        }

        #endregion

        #region instance fields and properties

        public ITechDefinition TechToDisplay { get; set; }

        public StatusType TechStatus { get; set; }

        [SerializeField] private Color DiscoveredColor;
        [SerializeField] private Color AvailableColor;
        [SerializeField] private Color UnavailableColor;

        [SerializeField] private Button StatusButton;

        [SerializeField] private Text NameField;

        #endregion

        #region events

        public event EventHandler<EventArgs> RecordClicked;

        #endregion

        #region instance methods

        public void Refresh() {
            if(TechToDisplay != null) {
                NameField.text = TechToDisplay.Name;

                if(TechStatus == StatusType.Available) {
                    StatusButton.image.color = AvailableColor;
                    StatusButton.interactable = true;

                }else if(TechStatus == StatusType.Unavailable) {
                    StatusButton.image.color = UnavailableColor;
                    StatusButton.interactable = false;

                }else if(TechStatus == StatusType.Discovered) {
                    StatusButton.image.color = DiscoveredColor;
                    StatusButton.interactable = true;
                }
            }
        }

        public void OnButtonClicked() {
            if(RecordClicked != null) {
                RecordClicked(this, EventArgs.Empty);
            }
        }

        #endregion

    }

}
