using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.UI.MapEditor {

    public class CivEditingPanel : MonoBehaviour {

        #region instance fields and properties

        public ICivilization CivToEdit { get; set; }

        [SerializeField] private Text NameField = null;

        [SerializeField] private InputField GoldStockpileField    = null;
        [SerializeField] private InputField CultureStockpileField = null;

        #endregion

        #region instance methods

        #region Unity messages

        private void OnEnable() {
            Refresh();
        }

        #endregion

        public void Refresh() {
            if(CivToEdit == null) {
                return;
            }

            NameField.text = CivToEdit.Template.Name;

            GoldStockpileField.text    = CivToEdit.GoldStockpile   .ToString();
            CultureStockpileField.text = CivToEdit.CultureStockpile.ToString();
        }

        public void UpdateGoldStockpile(string newValue) {
            if(CivToEdit == null) {
                return;
            }

            CivToEdit.GoldStockpile = int.Parse(newValue);

            Refresh();
        }

        public void UpdateCultureStockpile(string newValue) {
            if(CivToEdit == null) {
                return;
            }

            CivToEdit.CultureStockpile = int.Parse(newValue);

            Refresh();
        }

        #endregion

    }
}
