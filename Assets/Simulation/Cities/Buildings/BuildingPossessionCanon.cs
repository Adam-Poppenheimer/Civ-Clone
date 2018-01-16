using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// the standard implementation for IBuildingPossessionCanon.
    /// </summary>
    public class BuildingPossessionCanon : PossessionRelationship<ICity, IBuilding> {

        #region constructors
        
        [Inject]
        public BuildingPossessionCanon(CitySignals signals) {
            signals.CityBeingDestroyedSignal.Subscribe(OnCityBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICity, IBuilding>

        protected override bool IsPossessionValid(IBuilding possession, ICity owner) {
            if(owner == null) {
                return true;
            }else {
                return !GetPossessionsOfOwner(owner).Contains(possession) && GetOwnerOfPossession(possession) == null;
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
