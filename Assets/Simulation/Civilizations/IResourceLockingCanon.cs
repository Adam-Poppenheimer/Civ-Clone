using System;

using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Civilizations {

    public interface IResourceLockingCanon {

        #region methods

        int GetLockedCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);

        bool CanLockCopyOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);
        void LockCopyOfResourceForCiv   (ISpecialtyResourceDefinition resource, ICivilization civ);

        bool CanUnlockCopyOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);        
        void UnlockCopyOfResourceForCiv   (ISpecialtyResourceDefinition resource, ICivilization civ);

        #endregion
    }

}