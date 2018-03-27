﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.Core;

namespace Assets.UI.Diplomacy {

    public class DeclareWarRecord : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text CivNameField;
        
        public ICivilization CivilizationToRecord { get; set; }



        private IWarCanon WarCanon;

        private IGameCore GameCore;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IWarCanon warCanon, IGameCore gameCore){
            WarCanon = warCanon;
            GameCore = gameCore;
        }

        #region Unity messages

        private void OnEnable() {
            if(CivilizationToRecord == null) {
                return;
            }

            CivNameField.text = CivilizationToRecord.Name;
        }

        #endregion

        public void TryDeclareWar() {
            if(CivilizationToRecord != null && WarCanon.CanDeclareWar(GameCore.ActiveCivilization, CivilizationToRecord)) {
                WarCanon.DeclareWar(GameCore.ActiveCivilization, CivilizationToRecord);
            }
        }

        #endregion

    }

}
