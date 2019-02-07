/**
 * Author:  David Asmuth
 * Contact: piranha771@gmail.com
 * License: Public domain
 * 
 * Converts the .fbx model
 * from Blender orientation system (Z is up, Y is forward)
 * to the Unity3D orientation system (Y is up, Z is forward)
 */
using System.IO;
using UnityEngine;
using UnityEditor;


public class FixFbxModelPostprocessor : AssetPostprocessor
{
    public void OnPostprocessModel(GameObject obj)
    {
        ModelImporter importer = assetImporter as ModelImporter;
        if (Path.GetExtension(importer.assetPath) == ".fbx")
        {
            FixObject(obj.transform);
            Debug.Log("[Fix FBX] Finished for " + Path.GetFileName(importer.assetPath));
        }
    }

    private void FixObject(Transform obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();

        /*
         * First we need to undo the stupid -90 dregree rotation on the X axis that Unity does
         * on every game object with a mesh
         */
        if (meshFilter)
        {
            obj.localRotation *= Quaternion.Euler(90, 0, 0);
        }

        /*
         * Translate rotation into Unity's coordinate system
         */
        Quaternion lc = obj.transform.localRotation;
        obj.transform.localRotation = new Quaternion(-lc.x, lc.y, -lc.z, lc.w);

        /*
         * Translate position into Unity's coordinate system
         */
        Vector3 currentPos = obj.transform.localPosition;
        obj.transform.localPosition = new Vector3(-currentPos.x, currentPos.y, -currentPos.z);

        /*
         * Translate mesh into Unity's coordinate system
         */
        if (meshFilter)
        {
            FixMesh(meshFilter.sharedMesh);
        }

        /*
         * Repeat for all sub objects
         */
        foreach (Transform child in obj)
        {
            FixObject(child);
        }
    }

    private void FixMesh(Mesh mesh)
    {
        /*
         * We fix the vertices by flipping Z axis with Y axis
         * Odly enough X has to be inverted. When we store a positive X in Blender somehow it gets inverted in the *.fbx format O.o
         */
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(-vertices[i].x, vertices[i].z, vertices[i].y);
        }
        mesh.vertices = vertices;

        /*
         * Same goes for normals
         */
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = new Vector3(-normals[i].x, normals[i].z, normals[i].y);
        }
        mesh.normals = normals;

        /*
         * Vertex positions have changed, so recalc bounds
         */
        mesh.RecalculateBounds();
    }
}