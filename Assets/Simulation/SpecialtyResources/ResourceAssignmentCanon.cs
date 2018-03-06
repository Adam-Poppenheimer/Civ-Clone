using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.SpecialtyResources {

    public class ResourceAssignmentCanon : IResourceAssignmentCanon {

        #region instance fields and properties

        private Dictionary<ICivilization, Dictionary<ISpecialtyResourceDefinition, int>> CopiesOfResourceReservedByCiv = 
            new Dictionary<ICivilization, Dictionary<ISpecialtyResourceDefinition, int>>();




        private ISpecialtyResourcePossessionLogic ResourcePossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public ResourceAssignmentCanon(
            ISpecialtyResourcePossessionLogic resourcePossessionCanon,
            CivilizationSignals civSignals
        ){
            ResourcePossessionCanon = resourcePossessionCanon;

            civSignals.CivilizationBeingDestroyedSignal.Subscribe(OnCivilizationBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from ICityResourceAssignmentCanon

        public int GetFreeCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ) {
            var freeCopies = ResourcePossessionCanon.GetCopiesOfResourceBelongingToCiv(resource, civ);

            freeCopies -= CopiesOfResourceReservedByCiv.GetNestedDict(civ, resource);

            return freeCopies;
        }

        public bool CanReserveCopyOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ) {
            return true;
        }

        public void ReserveCopyOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ) {
            if(!CanReserveCopyOfResourceForCiv(resource, civ)) {
                throw new InvalidOperationException("CanReserveCopyOfResourceForCiv must return true on the given arguments");
            }
            CopiesOfResourceReservedByCiv.SetNestedDict(civ, resource, CopiesOfResourceReservedByCiv.GetNestedDict(civ, resource) + 1);
        }

        public bool CanUnreserveCopyOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ) {
            return CopiesOfResourceReservedByCiv.GetNestedDict(civ, resource) > 0;
        }

        public void UnreserveCopyOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ) {
            if(!CanUnreserveCopyOfResourceForCiv(resource, civ)) {
                throw new InvalidOperationException("CanUnreserveCopyOfResourceForCiv must return true on the given arguments");
            }
            CopiesOfResourceReservedByCiv.SetNestedDict(civ, resource, CopiesOfResourceReservedByCiv.GetNestedDict(civ, resource) - 1);
        }

        #endregion

        private void OnCivilizationBeingDestroyed(ICivilization civ) {
            CopiesOfResourceReservedByCiv.Remove(civ);
        }

        #endregion

    }

}
