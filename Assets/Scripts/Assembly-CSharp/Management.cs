using UnityEngine.SceneManagement;

public class Management
{
    public static bool isLogo = true;

    // This source project runs beside Teamobi2026/SRC. The server listens on
    // 14445 locally and publishes its complete list after this first connection.
    public static string IpServer = "NRO:127.0.0.1:14445:0,0,0";

    public static string LinkWeb = "@godmobi";

    private static TabType[] tabTypes = new TabType[]
    {
        TabType.Tab1,
        TabType.Tab2
    };

    private static string[] SceneNames = new string[]
    {
         "NROL",
         "NROL1"
    };
    public static TabType tab;

    private static bool SceneLoad(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    public static void ChangeTab(int index)
    {
        tab = tabTypes[index];
        if (!SceneLoad(SceneNames[index]))
        {
            SceneManager.LoadScene(SceneNames[index], LoadSceneMode.Additive);
        }
    }
}

