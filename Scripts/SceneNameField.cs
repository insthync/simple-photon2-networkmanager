using UnityEngine;

[System.Serializable]
public class SceneNameField
{
    [SerializeField]
    private Object sceneAsset;
    [SerializeField]
    private string sceneName = "";

    public string SceneName
    {
        get { return sceneName; }
        set { sceneName = value; }
    }

    // makes it work with the existing Unity methods (LoadLevel/LoadScene)
    public static implicit operator string(SceneNameField sceneNameField)
    {
        return sceneNameField.SceneName;
    }

    public bool IsSet()
    {
        return !string.IsNullOrEmpty(sceneName);
    }
}