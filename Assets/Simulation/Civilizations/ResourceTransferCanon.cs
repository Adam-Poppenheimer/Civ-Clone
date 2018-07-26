﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.MapResources;

namespace Assets.Simulation.Civilizations {

    public class ResourceTransferCanon : IResourceTransferCanon {

        #region instance fields and properties

        private List<ResourceTransfer> AllActiveTransfers = new List<ResourceTransfer>();



        private IResourceExtractionLogic ExtractionLogic;
        private IResourceLockingCanon    LockingCanon;
        private CivilizationSignals      CivSignals;

        #endregion

        #region constructors

        [Inject]
        public ResourceTransferCanon(
            IResourceExtractionLogic extractionLogic, IResourceLockingCanon lockingCanon,
            CivilizationSignals civSignals
        ){
            ExtractionLogic = extractionLogic;
            LockingCanon    = lockingCanon;
            CivSignals      = civSignals;
        }

        #endregion

        #region instance methods

        #region from IResourceAssignmentCanon

        public int GetExportedCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ) {
            return AllActiveTransfers
                .Where(transfer => transfer.Exporter == civ && transfer.Resource == resource)
                .Sum(transfer => transfer.Copies);
        }

        public int GetImportedCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ) {
            return AllActiveTransfers
                .Where(transfer => transfer.Importer == civ && transfer.Resource == resource)
                .Sum(transfer => transfer.Copies);
        }

        public int GetTradeableCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ) {
            int extractedCopies = ExtractionLogic.GetExtractedCopiesOfResourceForCiv(resource, civ);
            int exportedCopies  = GetExportedCopiesOfResourceForCiv                 (resource, civ);
            int lockedCopies    = LockingCanon.GetLockedCopiesOfResourceForCiv      (resource, civ);

            return extractedCopies - exportedCopies - lockedCopies;
        }

        public IEnumerable<ResourceTransfer> GetAllExportTransfersFromCiv(ICivilization civ) {
            return AllActiveTransfers.Where(transfer => transfer.Exporter == civ);
        }

        public IEnumerable<ResourceTransfer> GetAllImportTransfersFromCiv(ICivilization civ) {
            return AllActiveTransfers.Where(transfer => transfer.Importer == civ);
        }

        public bool CanExportCopiesOfResource(
            IResourceDefinition resource, int copies,
            ICivilization exporter, ICivilization importer
        ){
            return GetTradeableCopiesOfResourceForCiv(resource, exporter) >= copies;
        }

        public ResourceTransfer ExportCopiesOfResource(
            IResourceDefinition resource, int copies,
            ICivilization exporter, ICivilization importer
        ){
            if(!CanExportCopiesOfResource(resource, copies, exporter, importer)) {
                throw new InvalidOperationException("CanExportCopiesOfResource must return true on the given arguments");
            }

            var newTransfer = new ResourceTransfer(exporter, importer, resource, copies);

            AllActiveTransfers.Add(newTransfer);

            return newTransfer;
        }

        public void CancelTransfer(ResourceTransfer transfer) {
            AllActiveTransfers.Remove(transfer);
            CivSignals.ResourceTransferCanceledSignal.OnNext(transfer);
        }

        #endregion

        public void SynchronizeResourceForCiv(IResourceDefinition resource, ICivilization civ) {
            int tradeDeficit = GetTradeableCopiesOfResourceForCiv(resource, civ);

            if(tradeDeficit < 0) {
                foreach(var exportOfResource in GetAllExportTransfersFromCiv(civ).Where(transfer => transfer.Resource == resource).ToList()) {
                    CancelTransfer(exportOfResource);
                    tradeDeficit += exportOfResource.Copies;

                    if(tradeDeficit >= 0) {
                        break;
                    }
                }
            }
        }

        #endregion

    }

}
