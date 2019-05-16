using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Promotions {

    public class UnitPromotionLogic : IUnitPromotionLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IGlobalPromotionLogic                         GlobalPromotionLogic;

        #endregion

        #region constructors

        [Inject]
        public UnitPromotionLogic(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IGlobalPromotionLogic globalPromotionLogic
        ) {
            UnitPossessionCanon  = unitPossessionCanon;
            GlobalPromotionLogic = globalPromotionLogic;
        }

        #endregion

        #region instance methods

        #region from IUnitPromotionLogic

        public IEnumerable<IPromotion> GetPromotionsForUnit(IUnit unit) {
            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            return unit.Template.StartingPromotions
                                .Concat(GlobalPromotionLogic.GetGlobalPromotionsOfCiv(unitOwner))
                                .Concat(unit.PromotionTree.GetAllPromotions());
        }

        #endregion

        #endregion
        
    }

}
