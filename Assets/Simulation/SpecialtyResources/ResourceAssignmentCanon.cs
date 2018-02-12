using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.SpecialtyResources {

    public class ResourceAssignmentCanon : IResourceAssignmentCanon {

        #region instance fields and properties

        private DictionaryOfLists<ICity, ISpecialtyResourceDefinition> ResourcesAssignedToCity =
            new DictionaryOfLists<ICity, ISpecialtyResourceDefinition>();

        private Dictionary<ICivilization, Dictionary<ISpecialtyResourceDefinition, int>> CopiesOfResourceReservedByCiv = 
            new Dictionary<ICivilization, Dictionary<ISpecialtyResourceDefinition, int>>();




        private ISpecialtyResourcePossessionCanon ResourcePossessionCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public ResourceAssignmentCanon(
            ISpecialtyResourcePossessionCanon resourcePossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ){
            ResourcePossessionCanon = resourcePossessionCanon;
            CityPossessionCanon     = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ICityResourceAssignmentCanon

        public bool HasResourceBeenAssignedToCity(ISpecialtyResourceDefinition resource, ICity city) {
            return ResourcesAssignedToCity[city].Contains(resource);
        }

        public IEnumerable<ISpecialtyResourceDefinition> GetAllResourcesAssignedToCity(ICity city) {
            return ResourcesAssignedToCity[city];
        }

        public int GetFreeCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ) {
            var freeCopies = ResourcePossessionCanon.GetCopiesOfResourceBelongingToCiv(resource, civ);

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                if(HasResourceBeenAssignedToCity(resource, city)) {
                    freeCopies--;
                }
            }

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

        public bool CanAssignResourceToCity(ISpecialtyResourceDefinition resource, ICity city) {
            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            var alreadyAssigned = HasResourceBeenAssignedToCity(resource, city);
            var hasFreeCopies = GetFreeCopiesOfResourceForCiv(resource, cityOwner) > 0;

            return !alreadyAssigned && hasFreeCopies;
        }

        public void AssignResourceToCity(ISpecialtyResourceDefinition resource, ICity city) {
            if(!CanAssignResourceToCity(resource, city)) {
                throw new InvalidOperationException("CanAssignResourceToCity must return true on the arguments");
            }

            ResourcesAssignedToCity[city].Add(resource);
        }

        public bool CanUnassignResourceFromCity(ISpecialtyResourceDefinition resource, ICity city) {
            return HasResourceBeenAssignedToCity(resource, city);
        }

        public void UnassignResourceFromCity(ISpecialtyResourceDefinition resource, ICity city) {
            if(!CanUnassignResourceFromCity(resource, city)) {
                throw new InvalidOperationException("CanUnassignResourceFromCity must return true on the arguments");
            }
            ResourcesAssignedToCity[city].Remove(resource);
        }

        public void UnassignAllResourcesFromCity(ICity city) {
            ResourcesAssignedToCity[city].Clear();
        }

        #endregion

        #endregion

    }

}
