using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Civilizations {

    public class FreeResourcesLogic : IFreeResourcesLogic {

        #region instance fields and properties

        private IResourceExtractionLogic ExtractionLogic;
        private IResourceLockingCanon    LockingCanon;
        private IResourceTransferCanon   TransferCanon;

        #endregion

        #region constructors

        [Inject]
        public FreeResourcesLogic(
            IResourceExtractionLogic extractionLogic, IResourceLockingCanon lockingCanon,
            IResourceTransferCanon transferCanon
        ){
            ExtractionLogic = extractionLogic;
            LockingCanon    = lockingCanon;
            TransferCanon   = transferCanon;
        }

        #endregion

        #region instance methods

        #region from IFreeResourceLogic

        public int GetFreeCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ) {
            return
                ExtractionLogic.GetExtractedCopiesOfResourceForCiv(resource, civ) + 
                TransferCanon  .GetImportedCopiesOfResourceForCiv (resource, civ) -
                TransferCanon  .GetExportedCopiesOfResourceForCiv (resource, civ) -
                LockingCanon   .GetLockedCopiesOfResourceForCiv   (resource, civ);
        }

        #endregion

        #endregion
        
    }

}
