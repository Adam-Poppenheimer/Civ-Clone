using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Diplomacy {

    public class ResourceOngoingDiplomaticExchange : IOngoingDiplomaticExchange {

        #region instance fields and properties

        public ISpecialtyResourceDefinition ResourceToTransfer { get; set; }

        public int CopiesToTransfer { get; set; }

        public ICivilization Exporter { get; set; }

        public ICivilization Importer { get; set; }

        private ResourceTransfer ActiveTransfer;

        private IDisposable CancellationSubscription;



        private IResourceTransferCanon ResourceTransferCanon;
        private CivilizationSignals    CivSignals;

        #endregion

        #region events

        #region from IOngoingDiplomaticExchange

        public event EventHandler<EventArgs> TerminationRequested;

        #endregion

        #endregion

        #region constructors

        [Inject]
        public ResourceOngoingDiplomaticExchange(
            IResourceTransferCanon resourceTransferCanon, CivilizationSignals civSignals
        ){
            ResourceTransferCanon = resourceTransferCanon;
            CivSignals            = civSignals;
        }

        #endregion

        #region instance methods

        #region from IOngoingDiplomaticExchange

        public void Start() {
            ActiveTransfer = ResourceTransferCanon.ExportCopiesOfResource(ResourceToTransfer, CopiesToTransfer, Exporter, Importer);

            CancellationSubscription = CivSignals.ResourceTransferCanceledSignal.Subscribe(OnResourceTransferCanceled);
        }

        public void End() {
            ResourceTransferCanon.CancelTransfer(ActiveTransfer);

            CancellationSubscription.Dispose();
        }

        public string GetSummary() {
            return string.Format("{0}: {1}", ResourceToTransfer.name, CopiesToTransfer);
        }

        #endregion

        private void OnResourceTransferCanceled(ResourceTransfer transfer) {
            if(transfer.Equals(ActiveTransfer)) {
                if(TerminationRequested != null) {
                    TerminationRequested(this, EventArgs.Empty);
                }
            }
        }

        #endregion

    }

}
