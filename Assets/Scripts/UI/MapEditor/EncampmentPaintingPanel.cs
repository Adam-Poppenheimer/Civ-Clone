using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Barbarians;

namespace Assets.UI.MapEditor {

    public class EncampmentPaintingPanel : CellPaintingPanelBase {

        #region instance fields and properties

        private bool AddingOrRemoving;




        private IEncampmentLocationCanon EncampmentLocationCanon;
        private IEncampmentFactory       EncampmentFactory;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IEncampmentLocationCanon encampmentLocationCanon, IEncampmentFactory encampmentFactory
        ) {
            EncampmentLocationCanon = encampmentLocationCanon;
            EncampmentFactory       = encampmentFactory;
        }

        #region from CellPaintingPanelBase

        protected override void EditCell(IHexCell cell) {
            if(AddingOrRemoving) {
                if(EncampmentFactory.CanCreateEncampment(cell)) {
                    EncampmentFactory.CreateEncampment(cell);
                }

            }else {
                var encampmentOn = EncampmentLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

                if(encampmentOn != null) {
                    EncampmentFactory.DestroyEncampment(encampmentOn);
                }
            }
        }

        #endregion

        public void SetEncampmentMode(bool addingOrRemoving) {
            AddingOrRemoving = addingOrRemoving;
        }

        #endregion

    }

}
