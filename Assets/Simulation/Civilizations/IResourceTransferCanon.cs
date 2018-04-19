using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Civilizations {

    public interface IResourceTransferCanon {

        #region instance methods

        int GetExportedCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);

        int GetImportedCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);

        int GetTradeableCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);

        IEnumerable<ResourceTransfer> GetAllExportTransfersFromCiv(ICivilization civ);
        IEnumerable<ResourceTransfer> GetAllImportTransfersFromCiv(ICivilization civ);

        bool             CanExportCopiesOfResource(ISpecialtyResourceDefinition resource, int copies, ICivilization fromCiv, ICivilization toCiv);
        ResourceTransfer ExportCopiesOfResource   (ISpecialtyResourceDefinition resource, int copies, ICivilization fromCiv, ICivilization toCiv);

        void CancelTransfer(ResourceTransfer transfer);

        void SynchronizeResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);

        #endregion

    }

}
