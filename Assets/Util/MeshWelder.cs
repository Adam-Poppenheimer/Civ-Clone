using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

/*
	Originally created by Bunny83.
	Was streamlined and modified a bit by Terrev to better fit the needs of this project,
	then overhauled by grappigegovert for major speed improvements.
	Bunny83's original version:
	https://www.dropbox.com/s/u0wfq42441pkoat/MeshWelder.cs?dl=0
	Which was posted here:
	http://answers.unity3d.com/questions/1382854/welding-vertices-at-runtime.html
    Copied into this project from: 
    https://github.com/Terrev/3DXML-to-OBJ/blob/master/Assets/Scripts/MeshWelder.cs
*/

namespace Assets.Util {

    [Flags]
    public enum EVertexAttribute {
        Position = 1,
        Normal   = 2,
        UV       = 4,
        UV2      = 8,
        UV3      = 16,
        UV4      = 32,
        Color    = 64,
    }

    public class Vertex {

        #region instance fields and properties

        public Vector3 Position;
        public Vector4 UV;
        public Vector4 UV2;
        public Vector4 UV3;
        public Vector4 UV4;
        public Color   Color;

        #endregion

        #region constructors

        public Vertex(Vector3 position) {
            Position = position;
        }

        #endregion

        #region instance methods

        #region from Object

        public override bool Equals(object obj) {
            Vertex other = obj as Vertex;

            if(other != null) {
                return other.Position.Equals(Position)
                    && other.UV      .Equals(UV)
                    && other.UV2     .Equals(UV2)
                    && other.UV3     .Equals(UV3)
                    && other.UV4     .Equals(UV4);
            }else {
                return false;
            }
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = Position.x.GetHashCode();
                hashCode = (hashCode * 397) ^ Position.y.GetHashCode();
                hashCode = (hashCode * 397) ^ Position.z.GetHashCode();

                return hashCode;
            }
        }

        #endregion

        #endregion

    }

    public class MeshWelder {

        #region  instance fields and properties

        public Mesh Mesh;

        private int[] OriginalTriangles;

        private Vertex[] Vertices;
        Dictionary<Vertex, List<int>> NewVertices;
        int[] Map;

        private EVertexAttribute Attributes;

        #endregion

        #region instance methods

        public void Weld() {
            OriginalTriangles = Mesh.triangles;

            CreateVertexList();
            RemoveDuplicates();
            AssignNewVertexArrays();
            RemapTriangles();
        }

        private bool HasAttribute(EVertexAttribute attribute) {
            return (Attributes & attribute) != 0;
        }

        private void CreateVertexList() {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals   = new List<Vector3>();
            List<Vector4> uvs       = new List<Vector4>();
            List<Vector4> uv2s      = new List<Vector4>();
            List<Vector4> uv3s      = new List<Vector4>();
            List<Vector4> uv4s      = new List<Vector4>();
            List<Color> colors      = new List<Color>();

            Mesh.GetVertices(positions);
            Mesh.GetNormals (normals);
            Mesh.GetUVs     (0, uvs);
            Mesh.GetUVs     (1, uv2s);
            Mesh.GetUVs     (2, uv3s);
            Mesh.GetUVs     (3, uv4s);
            Mesh.GetColors  (colors);

            Attributes = EVertexAttribute.Position;

            if(normals != null && normals.Count > 0) { Attributes |= EVertexAttribute.Normal; }
            if(uvs     != null && uvs    .Count > 0) { Attributes |= EVertexAttribute.UV;     }
            if(uv2s    != null && uv2s   .Count > 0) { Attributes |= EVertexAttribute.UV2;    }
            if(uv3s    != null && uv3s   .Count > 0) { Attributes |= EVertexAttribute.UV3;    }
            if(uv4s    != null && uv4s   .Count > 0) { Attributes |= EVertexAttribute.UV4;    }
            if(colors  != null && colors .Count > 0) { Attributes |= EVertexAttribute.Color;  }

            Vertices = new Vertex[positions.Count];

            for(int i = 0; i < positions.Count; i++) {
                var vertex = new Vertex(positions[i]);

                if(HasAttribute(EVertexAttribute.UV    )) { vertex.UV     = uvs    [i]; }
                if(HasAttribute(EVertexAttribute.UV2   )) { vertex.UV2    = uv2s   [i]; }
                if(HasAttribute(EVertexAttribute.UV3   )) { vertex.UV3    = uv3s   [i]; }
                if(HasAttribute(EVertexAttribute.UV4   )) { vertex.UV4    = uv4s   [i]; }
                if(HasAttribute(EVertexAttribute.Color )) { vertex.Color  = colors [i]; }

                Vertices[i] = vertex;
            }
        }

        private void RemoveDuplicates() {
            NewVertices = new Dictionary<Vertex, List<int>>(Vertices.Length);

            for(int i = 0; i < Vertices.Length; i++) {
                Vertex vertex = Vertices[i];

                List<int> originals;
                if(NewVertices.TryGetValue(vertex, out originals)) {
                    originals.Add(i);
                }else {
                    NewVertices.Add(vertex, new List<int> { i });
                }
            }
        }

        private void AssignNewVertexArrays() {
            Map = new int[Vertices.Length];

            Vector3[] newVertices = new Vector3[NewVertices.Count];

            Vector3[] newNormals = HasAttribute(EVertexAttribute.Normal) ? new Vector3[NewVertices.Count] : new Vector3[0];
            Vector4[] newUVs     = HasAttribute(EVertexAttribute.UV    ) ? new Vector4[NewVertices.Count] : new Vector4[0];
            Vector4[] newUV2s    = HasAttribute(EVertexAttribute.UV2   ) ? new Vector4[NewVertices.Count] : new Vector4[0];
            Vector4[] newUV3s    = HasAttribute(EVertexAttribute.UV3   ) ? new Vector4[NewVertices.Count] : new Vector4[0];
            Vector4[] newUV4s    = HasAttribute(EVertexAttribute.UV4   ) ? new Vector4[NewVertices.Count] : new Vector4[0];
            Color  [] newColors  = HasAttribute(EVertexAttribute.Color ) ? new Color  [NewVertices.Count] : new Color  [0];

            int i = 0;
            foreach(KeyValuePair<Vertex, List<int>> keyValue in NewVertices) {
                foreach(var vertexIndex in keyValue.Value) {
                    Map[vertexIndex] = i;
                }

                newVertices[i] = keyValue.Key.Position;

                if(HasAttribute(EVertexAttribute.UV    )) { newUVs    [i] = keyValue.Key.UV;     }
                if(HasAttribute(EVertexAttribute.UV2   )) { newUV2s   [i] = keyValue.Key.UV2;    }
                if(HasAttribute(EVertexAttribute.UV3   )) { newUV3s   [i] = keyValue.Key.UV3;    }
                if(HasAttribute(EVertexAttribute.UV4   )) { newUV4s   [i] = keyValue.Key.UV4;    }
                if(HasAttribute(EVertexAttribute.Color )) { newColors [i] = keyValue.Key.Color;  }

                i++;
            }

            Mesh.Clear();

            Mesh.vertices = newVertices;
            
            if(HasAttribute(EVertexAttribute.Normal)) { Mesh.SetNormals(newNormals.ToList()); }
            if(HasAttribute(EVertexAttribute.UV   ))  { Mesh.SetUVs    (0, newUVs .ToList()); }
            if(HasAttribute(EVertexAttribute.UV2   )) { Mesh.SetUVs    (1, newUV2s.ToList()); }
            if(HasAttribute(EVertexAttribute.UV3   )) { Mesh.SetUVs    (2, newUV3s.ToList()); }
            if(HasAttribute(EVertexAttribute.UV4   )) { Mesh.SetUVs    (3, newUV4s.ToList()); }
            if(HasAttribute(EVertexAttribute.Color )) { Mesh.SetColors (newColors .ToList()); }
        }

        private void RemapTriangles() {
            int[] triangles = OriginalTriangles;

            for(int i = 0; i < triangles.Length; i++) {
                triangles[i] = Map[triangles[i]];
            }

            Mesh.triangles = triangles;
        }

        #endregion

    }

}
