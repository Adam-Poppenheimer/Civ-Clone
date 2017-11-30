using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation {

    public interface IPossessionRelationship<TOwner, TPossession>
        where TOwner : class where TPossession : class {

        #region methods

        IEnumerable<TPossession> GetPossessionsOfOwner(TOwner owner);

        TOwner GetOwnerOfPossession(TPossession possession);

        bool CanChangeOwnerOfPossession(TPossession possession, TOwner newOwner);

        void ChangeOwnerOfPossession(TPossession possession, TOwner newOwner);

        #endregion

    }

}
