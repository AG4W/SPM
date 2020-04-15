using UnityEngine;
using UnityEditor;

public class EditorCameraFollow : MonoBehaviour
{
    [SerializeField]GameObject target;

#if UNITY_EDITOR
    void Update()
    {
        Vector3 pos = target.transform.position;
        pos.y += 5;

        SceneView.lastActiveSceneView.camera.orthographic = true;
        SceneView.lastActiveSceneView.pivot = pos;
        SceneView.lastActiveSceneView.Repaint();
    }
#endif
}
