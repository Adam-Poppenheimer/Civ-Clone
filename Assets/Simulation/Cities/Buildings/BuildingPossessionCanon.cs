using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.SpecialtyResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// the standard implementation for IBuildingPossessionCanon.
    /// </summary>
    public class BuildingPossessionCanon : PossessionRelationship<ICity, IBuilding> {

        #region instance fields and properties

        private IResourceAssignmentCanon ResourceAssignmentCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;        

        #endregion

        #region constructors

        [Inject]
        public BuildingPossessionCanon(
            CitySignals signals, IResourceAssignmentCanon resourceAssignmentCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
            ){
            signals.CityBeingDestroyedSignal.Subscribe(OnCityBeingDestroyed);

            ResourceAssignmentCanon = resourceAssignmentCanon;
            CityPossessionCanon     = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICity, IBuilding>

        protected override void DoOnPossessionEstablished(IBuilding building, ICity newOwner) {
            if(newOwner == null) {
                return;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(newOwner);

            foreach(var resource in building.Template.RequiredResources) {
                ResourceAssignmentCanon.ReserveCopyOfResourceForCiv(resource, cityOwner);
            }
        }

        protected override void DoOnPossessionBroken(IBuilding building, ICity oldOwner) {
            if(oldOwner == null) {
                return;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(oldOwner);

            foreach(var resource in building.Template.RequiredResources) {
                ResourceAssignmentCanon.UnreserveCopyOfResourceForCiv(resource, cityOwner);
            }
        }

        #endregion

        private void OnCityBeingDestroyed(ICity city) {
            foreach(var building in new List<IBuilding>(GetPossessionsOfOwner(city))) {
                ChangeOwnerOfPossession(building, null);
            }
        }

        #endregion

    }

}
