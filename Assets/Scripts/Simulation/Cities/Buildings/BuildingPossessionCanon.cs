using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Cities.Buildings {

    public class BuildingPossessionCanon : PossessionRelationship<ICity, IBuilding> {

        #region instance fields and properties

        private CitySignals                                   CitySignals;
        private IResourceLockingCanon                         ResourceLockingCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ITechCanon                                    TechCanon;

        #endregion

        #region constructors

        [Inject]
        public BuildingPossessionCanon(
            CitySignals citySignals, IResourceLockingCanon resourceLockingCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ITechCanon techCanon
        ) {
            CitySignals          = citySignals;
            ResourceLockingCanon = resourceLockingCanon;
            CityPossessionCanon  = cityPossessionCanon;
            TechCanon            = techCanon;
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

            if(building.Template.ProvidesFreeTech) {
                TechCanon.AddFreeTechToCiv(cityOwner);
            }

            CitySignals.GainedBuilding.OnNext(new Tuple<ICity, IBuilding>(newOwner, building));
        }

        protected override void DoOnPossessionBroken(IBuilding building, ICity oldOwner) {
            if(oldOwner == null) {
                return;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(oldOwner);

            foreach(var resource in building.Template.ResourcesConsumed) {
                ResourceLockingCanon.UnlockCopyOfResourceForCiv(resource, cityOwner);
            }

            CitySignals.LostBuilding.OnNext(new Tuple<ICity, IBuilding>(oldOwner, building));
        }

        #endregion

        #endregion

    }

}
