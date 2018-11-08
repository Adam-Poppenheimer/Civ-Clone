using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units.Promotions;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// the standard implementation for IBuildingPossessionCanon.
    /// </summary>
    public class BuildingPossessionCanon : PossessionRelationship<ICity, IBuilding> {

        #region instance fields and properties

        private CitySignals                                   CitySignals;
        private IResourceLockingCanon                         ResourceLockingCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IGlobalPromotionCanon                         GlobalPromotionCanon;

        #endregion

        #region constructors

        [Inject]
        public BuildingPossessionCanon(
            CitySignals citySignals, IResourceLockingCanon resourceLockingCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IGlobalPromotionCanon globalPromotionCanon
        ) {
            CitySignals          = citySignals;
            ResourceLockingCanon = resourceLockingCanon;
            CityPossessionCanon  = cityPossessionCanon;
            GlobalPromotionCanon = globalPromotionCanon;
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICity, IBuilding>

        protected override void DoOnPossessionEstablished(IBuilding building, ICity newOwner) {
            if(newOwner == null) {
                return;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(newOwner);

            foreach(var resource in building.Template.ResourcesConsumed) {
                ResourceLockingCanon.LockCopyOfResourceForCiv(resource, cityOwner);
            }

            foreach(var promotion in building.Template.GlobalPromotions) {
                GlobalPromotionCanon.AddGlobalPromotionToCiv(promotion, cityOwner);
            }

            CitySignals.CityGainedBuildingSignal.OnNext(new UniRx.Tuple<ICity, IBuilding>(newOwner, building));
        }

        protected override void DoOnPossessionBroken(IBuilding building, ICity oldOwner) {
            if(oldOwner == null) {
                return;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(oldOwner);

            foreach(var resource in building.Template.ResourcesConsumed) {
                ResourceLockingCanon.UnlockCopyOfResourceForCiv(resource, cityOwner);
            }

            foreach(var promotion in building.Template.GlobalPromotions) {
                GlobalPromotionCanon.RemoveGlobalPromotionFromCiv(promotion, cityOwner);
            }

            CitySignals.CityLostBuildingSignal.OnNext(new UniRx.Tuple<ICity, IBuilding>(oldOwner, building));
        }

        #endregion

        #endregion

    }

}
