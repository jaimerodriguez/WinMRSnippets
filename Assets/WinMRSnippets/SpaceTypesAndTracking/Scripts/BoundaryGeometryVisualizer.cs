using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.XR;

namespace WinMRSnippets
{
    /// <summary>
    /// Draws the Geometry of a boundary 
    /// </summary>
    public class BoundaryGeometryVisualizer : MonoBehaviour
    {

#pragma warning disable CS0649 
        [SerializeField]
        [Tooltip("Material for rendering the floor boundary")]
        private Material material;

        [SerializeField]
        [Tooltip("Boundary to draw. Note: Playarea does not seem to work")]
        private Boundary.Type boundaryType = Boundary.Type.TrackedArea;

        [SerializeField]
        [Tooltip("Only draws boundary when in Room Scale. In stationary mode the floor is too high so you should not draw boundary.")]
        private bool drawOnlyInRoomScale = true;

#pragma warning restore  

        void Start()
        {
            //TODO: We only check on start. That feels appropriate for most scenarios. 
            // Our demo (test scene) might need extra handling, but better there than in this generic script 
            if (drawOnlyInRoomScale)
            {
                var spaceType = XRDevice.GetTrackingSpaceType();
                if (spaceType != TrackingSpaceType.RoomScale)
                    return;
            }

            List<Vector3> geometry = new List<Vector3>();
            bool retVal = UnityEngine.Experimental.XR.Boundary.TryGetGeometry(geometry, boundaryType);
            if (retVal)
            {
                MeshFilter mf = gameObject.AddComponent<MeshFilter>();
                Vector3[] poly = geometry.ToArray();

                if (CreateFloorMesh(mf, poly))
                {

                    Renderer rend = gameObject.AddComponent<MeshRenderer>();
                    rend.material = material;
                }
            }
#if TRACING_VERBOSE
        else
            Debug.Log("BoundaryGeometryVisualized Failed to retrieve Geometry. Are you in the Editor??"); 
#endif
        }

        /// <summary>
        ///  Creates a Mesh for the floor. 
        ///  The very simplistic drawing algorithm used to create mesh comes from https://answers.unity.com/questions/546473/create-a-plane-from-points.html 
        /// </summary>
        /// <param name="mf"></param>
        /// <param name="poly"></param>
        bool CreateFloorMesh(MeshFilter mf, Vector3[] poly)
        {
            Debug.Assert(mf != null);
            Debug.Assert(poly != null);
            if (mf == null || poly == null)
                return false;

            Mesh mesh = new Mesh();
            mf.mesh = mesh;

            Vector3 center = FindCenter(poly);

            Vector3[] vertices = new Vector3[poly.Length + 1];
            vertices[0] = Vector3.zero;

            for (int i = 0; i < poly.Length; i++)
            {
                vertices[i + 1] = poly[i] - center;
            }

            mesh.vertices = vertices;

            int[] triangles = new int[poly.Length * 3];

            for (int i = 0; i < poly.Length - 1; i++)
            {
                triangles[i * 3] = i + 2;
                triangles[i * 3 + 2] = 0;
                triangles[i * 3 + 1] = i + 1;
            }

            triangles[(poly.Length - 1) * 3] = 1;
            triangles[(poly.Length - 1) * 3 + 2] = 0;
            triangles[(poly.Length - 1) * 3 + 1] = poly.Length;

            mesh.triangles = triangles;
            mesh.uv = BuildUVs(vertices);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return true;
        }

        Vector3 FindCenter(Vector3[] poly)
        {
            // Use the center as the origin 
            return Vector3.zero;

            //here is a center approximation, but origin is better. 
            //Vector3 center = Vector3.zero;
            //foreach (Vector3 v3 in poly)
            //{
            //    center += v3;
            //}
            //return center / poly.Length;
        }

        Vector2[] BuildUVs(Vector3[] vertices)
        {

            float xMin = Mathf.Infinity;
            float yMin = Mathf.Infinity;
            float xMax = -Mathf.Infinity;
            float yMax = -Mathf.Infinity;

            foreach (Vector3 v3 in vertices)
            {
                if (v3.x < xMin)
                    xMin = v3.x;
                if (v3.y < yMin)
                    yMin = v3.y;
                if (v3.x > xMax)
                    xMax = v3.x;
                if (v3.y > yMax)
                    yMax = v3.y;
            }

            float xRange = xMax - xMin;
            float yRange = yMax - yMin;

            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i].x = (vertices[i].x - xMin) / xRange;
                uvs[i].y = (vertices[i].y - yMin) / yRange;

            }
            return uvs;
        }
    }
} 