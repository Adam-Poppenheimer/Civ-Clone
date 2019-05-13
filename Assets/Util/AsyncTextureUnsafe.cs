using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Unity.Collections;

namespace Assets.Util {

    public class AsyncTextureUnsafe<T> where T : struct {

        #region instance fields and properties

        public int Width;
        public int Height;

        public NativeArray<T> Raw;

        public Texture2D Original;

        #endregion

        #region constructors

        public AsyncTextureUnsafe(Texture2D original) {
            Width  = original.width;
            Height = original.height;

            Raw = original.GetRawTextureData<T>();

            Original = original;
        }

        #endregion

    }

}
