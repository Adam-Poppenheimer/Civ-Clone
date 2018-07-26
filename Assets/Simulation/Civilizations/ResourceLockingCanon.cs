using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Civilizations {

    public class ResourceLockingCanon : IResourceLockingCanon {

        #region instance fields and properties

        private Dictionary<ICivilization, Dictionary<IResourceDefinition, int>> LockedResourcesDict = 
            new Dictionary<ICivilization, Dictionary<IResourceDefinition, int>>();

        #endregion

        #region constructors

        public ResourceLockingCanon() { }

        #endregion

        #region instance methods

        #region from IResourceLockingCanon

        public int GetLockedCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ) {
            return LockedResourcesDict.GetNestedDict(civ, resource);
        }

        public bool CanLockCopyOfResourceForCiv(IResourceDefinition resource, ICivilization civ) {
            return true;
        }

        public void LockCopyOfResourceForCiv (IResourceDefinition resource, ICivilization civ) {
            if(!CanLockCopyOfResourceForCiv(resource, civ)) {
                throw new InvalidOperationException("CanLockCopyOfResourceForCiv must return true on the given arguments");
            }

            var newLockedCopies = LockedResourcesDict.GetNestedDict(civ, resource) + 1;
            LockedResourcesDict.SetNestedDict(civ, resource, newLockedCopies);
        }

        public bool CanUnlockCopyOfResourceForCiv(IResourceDefinition resource, ICivilization civ) {
            return GetLockedCopiesOfResourceForCiv(resource, civ) > 0;
        }

        public void UnlockCopyOfResourceForCiv(IResourceDefinition resource, ICivilization civ) {
            if(!CanUnlockCopyOfResourceForCiv(resource, civ)) {
                throw new InvalidOperationException("CanUnlockCopyOfResourceForCiv must return true on the given arguments");
            }

            var newLockedCopies = LockedResourcesDict.GetNestedDict(civ, resource) - 1;
            LockedResourcesDict.SetNestedDict(civ, resource, newLockedCopies);
        }

        #endregion

        #endregion

    }

}
