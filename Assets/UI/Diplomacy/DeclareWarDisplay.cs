using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.Core;

namespace Assets.UI.Diplomacy {

    public class DeclareWarDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private DeclareWarRecord DeclareWarRecordPrefab;
        [SerializeField] private RectTransform    DeclareWarRecordContainer;

        private List<DeclareWarRecord> InstantiatedRecords = new List<DeclareWarRecord>();



        private IWarCanon            WarCanon;
        private ICivilizationFactory CivilizationFactory;
        private IGameCore            GameCore;
        private DiContainer          Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IWarCanon warCanon, ICivilizationFactory civilizationFactory,
            IGameCore gameCore, DiContainer container
        ){
            WarCanon            = warCanon;
            CivilizationFactory = civilizationFactory;
            GameCore            = gameCore;
            Container           = container;
        }

        #region Unity messages

        private void OnEnable() {
            Clear();
            BuildRecords();
        }

        #endregion

        private void BuildRecords() {
            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                if(!WarCanon.CanDeclareWar(GameCore.ActiveCiv, civilization)) {
                    continue;
                }

                var newRecord = Container.InstantiatePrefabForComponent<DeclareWarRecord>(DeclareWarRecordPrefab);

                newRecord.CivilizationToRecord = civilization;

                newRecord.transform.SetParent(DeclareWarRecordContainer, false);
                newRecord.gameObject.SetActive(true);

                InstantiatedRecords.Add(newRecord);
            }
        }

        private void Clear() {
            foreach(var record in InstantiatedRecords) {
                Destroy(record.gameObject);
            }

            InstantiatedRecords.Clear();
        }

        #endregion

    }

}
