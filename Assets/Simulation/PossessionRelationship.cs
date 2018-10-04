﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation {

    public class PossessionRelationship<TOwner, TPossession> : IPossessionRelationship<TOwner, TPossession>
        where TOwner : class where TPossession : class {

        #region instance fields and properties

        private DictionaryOfLists<TOwner, TPossession> PossessionsOfOwner = new DictionaryOfLists<TOwner, TPossession>();

        private Dictionary<TPossession, TOwner> OwnerOfPossession = new Dictionary<TPossession, TOwner>();

        #endregion

        #region instance methods

        #region from IPossessionRelationship<TOwner, TPossession>

        public IEnumerable<TPossession> GetPossessionsOfOwner(TOwner owner) {
            if(owner == null) {
                throw new ArgumentNullException("owner");
            }

            return PossessionsOfOwner[owner];
        }

        public TOwner GetOwnerOfPossession(TPossession possession) {
            if(possession == null) {
                throw new ArgumentNullException("possession");
            }

            TOwner retval;
            OwnerOfPossession.TryGetValue(possession, out retval);
            return retval;
        }

        public bool CanChangeOwnerOfPossession(TPossession possession, TOwner newOwner) {
            if(possession == null) {
                throw new ArgumentNullException("possession");
            }

            if(GetOwnerOfPossession(possession) == newOwner) {
                return false;
            }else {
                return IsPossessionValid(possession, newOwner);
            }            
        }

        public void ChangeOwnerOfPossession(TPossession possession, TOwner newOwner) {
            if(possession == null) {
                throw new ArgumentNullException("possession");
            }else if(!CanChangeOwnerOfPossession(possession, newOwner)) {
                throw new InvalidOperationException("CanChangeOwnershipOf must be true on the given arguments");
            }

            var oldOwner = GetOwnerOfPossession(possession);
            if(oldOwner != null) {
                DoOnPossessionBeingBroken(possession, oldOwner);

                PossessionsOfOwner[oldOwner].Remove(possession);

                DoOnPossessionBroken(possession, oldOwner);
            }

            OwnerOfPossession[possession] = newOwner;

            if(newOwner != null) {
                DoOnPossessionBeingEstablished(possession, newOwner);

                PossessionsOfOwner[newOwner].Add(possession);

                DoOnPossessionEstablished(possession, newOwner);
            }
        }

        public void Clear(bool performPossessionUnassignment) {
            if(performPossessionUnassignment) {
                foreach(var possession in OwnerOfPossession.Keys) {
                    ChangeOwnerOfPossession(possession, null);
                }
            }

            OwnerOfPossession .Clear();
            PossessionsOfOwner.Clear();
        }

        #endregion

        protected virtual bool IsPossessionValid(TPossession possession, TOwner owner) { return true; }

        protected virtual void DoOnPossessionBeingEstablished(TPossession possession, TOwner newOwner) { }
        protected virtual void DoOnPossessionBeingBroken     (TPossession possession, TOwner oldOwner) { }

        protected virtual void DoOnPossessionEstablished(TPossession possession, TOwner newOwner) { }
        protected virtual void DoOnPossessionBroken     (TPossession possession, TOwner oldOwner) { }

        #endregion
        
    }

}
