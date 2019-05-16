using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Cities {

    public class LocalPromotionLogic : ILocalPromotionLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public LocalPromotionLogic(
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ) {
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ILocalPromotionLogic

        public IEnumerable<IPromotion> GetLocalPromotionsForCity(ICity city) {
            return BuildingPossessionCanon.GetPossessionsOfOwner(city).SelectMany(
                building => building.Template.LocalPromotions
            );
        }

        #endregion

        #endregion
        
    }

}
