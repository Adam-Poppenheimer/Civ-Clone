using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.Units;

namespace Assets.UI.Units {

    public class UnitSummaryDisplay : UnitDisplayBase {

        #region instance fields and properties

        private IUnitConfig Config;

        [SerializeField] private Text NameField;
        [SerializeField] private Text CurrentMovementField;
        [SerializeField] private Text MaxMovementField;
        [SerializeField] private Text TypeField;

        [SerializeField] private Slider HealthSlider;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IUnitConfig config,
            [InjectOptional(Id = "Name Field"            )] Text nameField,
            [InjectOptional(Id = "Current Movement Field")] Text currentMovementField,
            [InjectOptional(Id = "Max Movement Field"    )] Text maxMovementField,
            [InjectOptional(Id = "Type Field"            )] Text typeField,
            [InjectOptional(Id = "Health Slider"         )] Slider healthSlider
        ) {
            Config = config;

            if(nameField            != null) { NameField            = nameField;            }
            if(currentMovementField != null) { CurrentMovementField = currentMovementField; }
            if(maxMovementField     != null) { MaxMovementField     = maxMovementField;     }
            if(typeField            != null) { TypeField            = typeField;            }

            if(healthSlider != null) { HealthSlider = healthSlider; }
        }

        #region from UnitDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            NameField           .text = ObjectToDisplay.Name;
            MaxMovementField    .text = ObjectToDisplay.MaxMovement.ToString();
            TypeField           .text = ObjectToDisplay.Type       .ToString();

            CurrentMovementField.text = ObjectToDisplay.CurrentMovement.ToString();

            HealthSlider.minValue = 0;
            HealthSlider.maxValue = Config.MaxHealth;
            HealthSlider.value    = ObjectToDisplay.Health;
        }

        #endregion

        #endregion

    }

}
