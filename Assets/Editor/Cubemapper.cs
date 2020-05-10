using UnityEngine;
using UnityEditor;
using ICSharpCode.NRefactory.Ast;

public class Cubemapper : ScriptableWizard
{
    [SerializeField]Camera camera;
    [SerializeField]int resolution = 1024;

    void OnWizardUpdate()
    {
        this.helpString = "Select camera position to render from";
        this.isValid = (camera != null);
    }
    void OnWizardCreate()
    {
        Cubemap cm = new Cubemap(resolution, TextureFormat.ARGB32, false);
        camera.RenderToCubemap(cm);
        AssetDatabase.CreateAsset(cm, $"Assets/Content/Textures/Cubemaps/{camera.name}.cubemap");
    }

    [MenuItem("Tools/Cubemapper")]
    static void CreateCubemap()
    {
        DisplayWizard<Cubemapper>("Create Cubemap", "Render");
    }
}
