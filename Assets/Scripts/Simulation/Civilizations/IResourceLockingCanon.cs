using System;

using Assets.Simulation.MapResources;

namespace Assets.Simulation.Civilizations {

    public interface IResourceLockingCanon {

        #region methods

        int GetLockedCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ);

        bool CanLockCopyOfResourceForCiv(IResourceDefinition resource, ICivilization civ);
        void LockCopyOfResourceForCiv   (IResourceDefinition resource, ICivilization civ);

        bool CanUnlockCopyOfResourceForCiv(IResourceDefinition resource, ICivilization civ);        
        void UnlockCopyOfResourceForCiv   (IResourceDefinition resource, ICivilization civ);

        #endregion
    }

}