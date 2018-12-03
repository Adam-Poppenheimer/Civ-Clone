using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Diplomacy {

    public class ResourceOngoingDiplomaticExchange : IOngoingDiplomaticExchange {

        #region instance fields and properties

        public ExchangeType Type {
            get { return ExchangeType.Resource; }
        }

        public ICivilization Sender   { get; set; }
        public ICivilization Receiver { get; set; }

        public int IntegerInput { get; set; }

        public IResourceDefinition ResourceInput { get; set; }

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
            ActiveTransfer = ResourceTransferCanon.ExportCopiesOfResource(ResourceInput, IntegerInput, Sender, Receiver);

            CancellationSubscription = CivSignals.ResourceTransferCancelled.Subscribe(OnResourceTransferCanceled);
        }

        public void End() {
            ResourceTransferCanon.CancelTransfer(ActiveTransfer);

            CancellationSubscription.Dispose();
        }

        public string GetSummary() {
            return string.Format("{0}: {1}", ResourceInput.name, IntegerInput);
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
