using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NewBehaviourScript : MonoBehaviour
{
    public GameObject root;
    private Vector3 m_normal = Vector3.up;
    private Quaternion m_rot;

    private void Awake()
    {
        m_rot = Quaternion.Euler(m_normal);
    }

    private void OnDrawGizmos()
    {
        if (true)
        {
            DrawCube();
        }
    }

    private void DrawCube()
    {
        if (root == null)
            return;
        Gizmos.matrix = Matrix4x4.TRS(root.transform.position, m_rot, Vector3.one);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(root.transform.position, new Vector3(1,0,1));
    }
}
