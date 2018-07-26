using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Civilizations {

    public interface IResourceTransferCanon {

        #region instance methods

        int GetExportedCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ);

        int GetImportedCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ);

        int GetTradeableCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ);

        IEnumerable<ResourceTransfer> GetAllExportTransfersFromCiv(ICivilization civ);
        IEnumerable<ResourceTransfer> GetAllImportTransfersFromCiv(ICivilization civ);

        bool             CanExportCopiesOfResource(IResourceDefinition resource, int copies, ICivilization fromCiv, ICivilization toCiv);
        ResourceTransfer ExportCopiesOfResource   (IResourceDefinition resource, int copies, ICivilization fromCiv, ICivilization toCiv);

        void CancelTransfer(ResourceTransfer transfer);

        void SynchronizeResourceForCiv(IResourceDefinition resource, ICivilization civ);

        #endregion

    }

}
