using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// 设计界面动画控制器
/// 控制PatternSelectionPanel、Toolbar、DesignArea和IntroduceArea的平移动画
/// 支持第一次和第二次点击nextstepbutton的不同动画效果
/// </summary>
public class DesignUIAnimation : MonoBehaviour
{
    [Header("组件引用")]
    [Tooltip("要向左移动的PatternSelectionPanel")]
    public RectTransform patternSelectionPanel;
    
    [Tooltip("要向右移动的Toolbar")]
    public RectTransform toolbar;
    
    [Tooltip("要向左移动的DesignArea")]
    public RectTransform designArea;
    
    [Tooltip("要向左移动的IntroduceArea")]
    public RectTransform introduceArea;
    
    [Tooltip("第一次点击时出现的IntroduceText1")]
    public RectTransform introduceText1;
    
    [Tooltip("第二次点击时出现的IntroduceText2")]
    public RectTransform introduceText2;
    
    [Tooltip("第二次点击时向下移动出现的Plastic组件")]
    public RectTransform plasticComponent;
    
    [Header("动画设置")]
    [Tooltip("动画持续时间（秒）")]
    public float animationDuration = 1.0f;
    
    [Tooltip("PatternSelectionPanel向左移动的单位数")]
    public float panelMoveDistance = 50f;
    
    [Tooltip("Toolbar向右移动的单位数")]
    public float toolbarMoveDistance = 200f;
    
    [Tooltip("DesignArea向左移动的单位数")]
    public float designAreaMoveDistance = 50f;
    
    [Tooltip("IntroduceArea向左移动的单位数")]
    public float introduceAreaMoveDistance = 200f;
    
    [Tooltip("IntroduceText缩放动画的持续时间")]
    public float introduceTextScaleDuration = 0.5f;
    
    [Tooltip("Plastic组件向下移动的单位数")]
    public float plasticMoveDistance = 400f;
    
    [Tooltip("Plastic组件移动动画的持续时间")]
    public float plasticMoveDuration = 0.8f;
    
    // 点击次数计数器
    private int clickCount = 0;
    
    private void Start()
    {
        // 尝试从按钮组件获取自身
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnNextStepButtonClicked);
        }
        
        // 初始化introduceText2为不可见（缩放为0）
        if (introduceText2 != null)
        {
            introduceText2.localScale = Vector3.zero;
        }
        
        // 初始化clickCount
        clickCount = 0;
    }
    
    /// <summary>
    /// 当NextStepButton被点击时调用
    /// 根据点击次数执行不同的动画序列
    /// </summary>
    public void OnNextStepButtonClicked()
    {
        // 增加点击计数
        clickCount++;
        Debug.Log($"NextStepButton第{clickCount}次被点击，开始执行动画序列");
        
        // 检查各组件引用
        Debug.Log($"组件引用检查：\n- patternSelectionPanel: {(patternSelectionPanel != null ? "已设置" : "未设置")}\n- toolbar: {(toolbar != null ? "已设置" : "未设置")}\n- designArea: {(designArea != null ? "已设置" : "未设置")}\n- introduceArea: {(introduceArea != null ? "已设置" : "未设置")}\n- introduceText1: {(introduceText1 != null ? "已设置" : "未设置")}\n- introduceText2: {(introduceText2 != null ? "已设置" : "未设置")}\n- plasticComponent: {(plasticComponent != null ? "已设置" : "未设置")}");
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        // 根据点击次数执行不同的动画序列
        if (clickCount == 1)
        {
            // 第一次点击：执行初始动画序列
            if (patternSelectionPanel != null)
            {
                float startX = patternSelectionPanel.localPosition.x;
                float targetX = startX - panelMoveDistance;
                sequence.Join(patternSelectionPanel.DOLocalMoveX(targetX, animationDuration)
                    .SetEase(Ease.InOutQuad));
            }
            
            if (toolbar != null)
            {
                float startX = toolbar.localPosition.x;
                float targetX = startX + toolbarMoveDistance;
                sequence.Join(toolbar.DOLocalMoveX(targetX, animationDuration)
                    .SetEase(Ease.InOutQuad));
            }
            
            if (designArea != null)
            {
                float startX = designArea.localPosition.x;
                float targetX = startX - designAreaMoveDistance;
                sequence.Join(designArea.DOLocalMoveX(targetX, animationDuration)
                    .SetEase(Ease.InOutQuad));
            }
            
            // 第二步：间隔0.1秒后，让IntroduceArea向左移动
            sequence.AppendInterval(0.1f);
            
            if (introduceArea != null)
            {
                float startX = introduceArea.localPosition.x;
                float targetX = startX - introduceAreaMoveDistance;
                sequence.Append(introduceArea.DOLocalMoveX(targetX, 0.8f)
                    .SetEase(Ease.InOutQuad));
            }
            
            // 第三步：间隔0.3秒后，让introduceText1从中心缩放出现
            sequence.AppendInterval(0.3f);
            
            if (introduceText1 != null)
            {
                introduceText1.localScale = Vector3.zero;
                sequence.Append(introduceText1.DOScale(Vector3.one, introduceTextScaleDuration)
                    .SetEase(Ease.OutQuad));
            }
        }
        else if (clickCount == 2)
        {
            // 第二次点击：执行特定的动画序列
            
            // 第一步：introduceText1缩放消失
            if (introduceText1 != null)
            {
                sequence.Append(introduceText1.DOScale(Vector3.zero, introduceTextScaleDuration)
                    .SetEase(Ease.InQuad));
            }
            
            // 第二步：introduceText2缩放出现
            sequence.AppendCallback(() =>
            {
                if (introduceText2 != null)
                {
                    sequence.Join(introduceText2.DOScale(Vector3.one, introduceTextScaleDuration)
                        .SetEase(Ease.OutQuad));
                }
            });
            
            // 第三步：Plastic组件向下移动400个单位出现
            sequence.AppendCallback(() =>
            {
                if (plasticComponent != null)
                {
                    float startY = plasticComponent.localPosition.y;
                    float targetY = startY - plasticMoveDistance;
                    sequence.Join(plasticComponent.DOLocalMoveY(targetY, plasticMoveDuration)
                        .SetEase(Ease.OutQuad));
                }
            });
        }
        
        else if (clickCount == 3)
        {
            // 第三次点击：切换到DyeProduct场景
            Debug.Log("第三次点击，切换到DyeProduct场景");
            SceneManager.LoadScene("Assets/Scenes/Design/DyeProduct.unity");
        }
        
        // 设置动画完成回调
        sequence.OnComplete(() =>
        {
            Debug.Log($"第{clickCount}次点击的所有动画序列执行完成");
        });
    }
    
    private void OnDestroy()
    {
        // 清理动画，防止内存泄漏
        DOTween.Kill(patternSelectionPanel);
        DOTween.Kill(toolbar);
        DOTween.Kill(designArea);
        DOTween.Kill(introduceArea);
        DOTween.Kill(introduceText1);
        DOTween.Kill(introduceText2);
        DOTween.Kill(plasticComponent);
    }
}