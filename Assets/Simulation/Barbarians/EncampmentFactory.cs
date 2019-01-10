using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Barbarians {

    public class EncampmentFactory : IEncampmentFactory {

        #region instance fields and properties

        #region from IEncampmentFactory

        public ReadOnlyCollection<IEncampment> AllEncampments {
            get { return EncampmentList.AsReadOnly(); }
        }
        private List<IEncampment> EncampmentList = new List<IEncampment>();

        #endregion

        private IEncampmentLocationCanon EncampmentLocationCanon;
        private DiContainer              Container;

        #endregion

        #region constructors

        [Inject]
        public EncampmentFactory(
            IEncampmentLocationCanon encampmentLocationCanon, DiContainer container
        ) {
            EncampmentLocationCanon = encampmentLocationCanon;
            Container               = container;
        }

        #endregion

        #region instance methods

        #region from EncampmentFactory

        public bool CanCreateEncampment(IHexCell location) {
            return EncampmentLocationCanon.CanCellAcceptAnEncampment(location);
        }

        public IEncampment CreateEncampment(IHexCell location) {
            if(location == null) {
                throw new ArgumentNullException("location");
            }

            if(!CanCreateEncampment(location)) {
                throw new InvalidOperationException("CanCreateEncampment must return true on the arguments");
            }

            var newEncampment = Container.Instantiate<Encampment>();

            EncampmentLocationCanon.ChangeOwnerOfPossession(newEncampment, location);
            
            EncampmentList.Add(newEncampment);

            return newEncampment;
        }

        public void DestroyEncampment(IEncampment encampment) {
            if(encampment == null) {
                throw new ArgumentNullException("encampment");
            }

            EncampmentLocationCanon.ChangeOwnerOfPossession(encampment, null);

            EncampmentList.Remove(encampment);
        }

        #endregion

        #endregion
        
    }

}
