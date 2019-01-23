using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Barbarians {

    public class EncampmentClearingResponder {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IEncampmentLocationCanon                      EncampmentLocationCanon;
        private IEncampmentFactory                            EncampmentFactory;
        private IBarbarianConfig                              BarbarianConfig;
        private ICivModifiers                                 CivModifiers;

        #endregion

        #region constructors

        [Inject]
        public EncampmentClearingResponder(
            UnitSignals unitSignals, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IEncampmentLocationCanon encampmentLocationCanon, IEncampmentFactory encampmentFactory,
            IBarbarianConfig barbarianConfig, ICivModifiers civModifiers
        ) {
            unitSignals.EnteredLocation.Subscribe(OnUnitEnteredLocation);

            UnitPossessionCanon     = unitPossessionCanon;
            EncampmentLocationCanon = encampmentLocationCanon;
            EncampmentFactory       = encampmentFactory;
            BarbarianConfig         = barbarianConfig;
            CivModifiers            = civModifiers;
        }

        #endregion

        #region instance methods

        private void OnUnitEnteredLocation(Tuple<IUnit, IHexCell> data) {
            var unit        = data.Item1;
            var newLocation = data.Item2;

            var encampmentAtLocation = EncampmentLocationCanon.GetPossessionsOfOwner(newLocation).FirstOrDefault();

            if(encampmentAtLocation != null) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                if(!unitOwner.Template.IsBarbaric) {
                    float modifier = CivModifiers.GoldBountyFromEncampments.GetValueForCiv(unitOwner);

                    unitOwner.GoldStockpile += Mathf.RoundToInt(BarbarianConfig.EncampmentBounty * modifier);

                    EncampmentFactory.DestroyEncampment(encampmentAtLocation);
                }
            }            
        }

        #endregion

    }

}
