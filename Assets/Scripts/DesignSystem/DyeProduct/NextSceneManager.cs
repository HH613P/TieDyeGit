using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景跳转管理器，负责处理从当前场景跳转到Cloth场景的逻辑
/// </summary>
public class NextSceneManager : MonoBehaviour
{
    /// <summary>
    /// Cloth场景的路径
    /// </summary>
    [SerializeField]
    private string clothScenePath = "Assets/Scenes/Design/Cloth.unity";
    
    /// <summary>
    /// 初始化时添加点击事件监听
    /// </summary>
    private void Awake()
    {
        // 尝试获取Button组件，如果没有则添加BoxCollider以支持点击检测
        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnNextSceneClicked);
        }
        else
        {
            // 如果不是UI Button，确保有碰撞体用于射线检测
            if (!GetComponent<Collider>())
            {
                BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
            }
        }
    }
    
    /// <summary>
    /// 当nextscene被点击时触发场景跳转
    /// </summary>
    public void OnNextSceneClicked()
    {
        Debug.Log("NextScene clicked, attempting to load scene: " + clothScenePath);
        LoadClothScene();
    }
    
    /// <summary>
    /// 加载Cloth场景
    /// </summary>
    private void LoadClothScene()
    {
        try
        {
            // 直接使用场景路径加载
            SceneManager.LoadScene(clothScenePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load Cloth scene: " + e.Message);
        }
    }
    
    /// <summary>
    /// 当物体被鼠标点击时（非UI元素）
    /// </summary>
    private void OnMouseDown()
    {
        OnNextSceneClicked();
    }
}