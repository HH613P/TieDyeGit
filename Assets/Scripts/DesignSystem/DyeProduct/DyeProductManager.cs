using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DesignSystem.DyeProduct
{
    public class DyeProductManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button nextButton;
        [SerializeField] private GameObject banlanImagePrefab;
        [SerializeField] private GameObject limeImagePrefab;
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pourWaterClip;

        [Header("Settings")]
        [SerializeField] private float totalEffectDuration = 1.0f; // 总效果持续时间
        [SerializeField] private float fadeInPercentage = 0.5f; // 渐显占总时间的比例
        [SerializeField] private float audioPlayDuration = 2.0f; // 音频播放时长

        private float fadeInDuration;
        private float fadeOutDuration;
        private Coroutine audioPlayCoroutine;

        private void Awake()
        {
            // 计算渐显渐隐持续时间
            fadeInDuration = totalEffectDuration * fadeInPercentage;
            fadeOutDuration = totalEffectDuration - fadeInDuration;

            // 确保引用正确设置
            if (nextButton == null)
            {
                Debug.LogError("NextButton reference is missing!");
            }
            
            if (canvasTransform == null)
            {
                // 尝试查找Canvas组件
                canvasTransform = FindObjectOfType<Canvas>().transform;
                if (canvasTransform == null)
                {
                    Debug.LogError("Canvas not found in the scene!");
                }
            }

            // 初始化音频源
            InitializeAudioSource();

            // 验证预制体路径
            ValidatePrefabReferences();
        }

        private void InitializeAudioSource()
        {
            // 如果没有设置音频源，尝试查找或添加一个
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            // 确保音频播放时长有效
            if (audioPlayDuration <= 0)
            {
                audioPlayDuration = 2.0f; // 默认2秒
                Debug.LogWarning("Audio play duration must be positive. Setting to default 2.0s.");
            }
        }

        private void PlayPourWaterSound()
        {
            // 如果有正在播放的音频协程，停止它
            if (audioPlayCoroutine != null)
            {
                StopCoroutine(audioPlayCoroutine);
            }

            // 启动新的音频播放协程
            audioPlayCoroutine = StartCoroutine(PlayAudioForDuration());
        }

        private IEnumerator PlayAudioForDuration()
        {
            // 如果没有音频剪辑，尝试加载
            if (pourWaterClip == null)
            {
                // 尝试从Resources文件夹加载
                pourWaterClip = Resources.Load<AudioClip>("pour water");
                
                // 如果Resources中也没有，尝试直接从Assets/Audio文件夹加载（仅编辑器模式）
                #if UNITY_EDITOR
                if (pourWaterClip == null)
                {
                    string audioPath = "Assets/Audio/pour water.mp3";
                    Debug.Log("Attempting to load audio clip from: " + audioPath);
                    UnityEditor.AssetDatabase.Refresh();
                    pourWaterClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                }
                #endif

                if (pourWaterClip == null)
                {
                    Debug.LogError("Failed to load pour water audio clip!");
                    yield break;
                }
            }

            // 设置音频剪辑并播放
            audioSource.clip = pourWaterClip;
            audioSource.loop = false;
            audioSource.Play();

            // 等待指定的播放时长
            yield return new WaitForSeconds(audioPlayDuration);

            // 停止播放
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioPlayCoroutine = null;
        }

        private void OnEnable()
        {
            // 添加按钮点击事件监听
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextButtonClicked);
            }
        }

        private void OnDisable()
        {
            // 移除按钮点击事件监听
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(OnNextButtonClicked);
            }
        }

        private void OnNextButtonClicked()
        {
            // 播放倒水音效
            PlayPourWaterSound();
            
            // 实例化预制体并添加渐显渐隐效果
            StartCoroutine(ShowAndHidePrefabs());
        }

        private IEnumerator ShowAndHidePrefabs()
        {
            // 实例化两个预制体
            List<GameObject> instances = new List<GameObject>();
            
            GameObject banlanInstance = CreatePrefabInstance("BanlanImage");
            GameObject limeInstance = CreatePrefabInstance("LimeImage");
            
            if (banlanInstance != null) instances.Add(banlanInstance);
            if (limeInstance != null) instances.Add(limeInstance);

            if (instances.Count == 0)
            {
                Debug.LogWarning("No prefab instances were created.");
                yield break;
            }

            // 为每个实例应用渐显渐隐效果
            List<Coroutine> fadeCoroutines = new List<Coroutine>();
            
            foreach (GameObject instance in instances)
            {
                // 设置初始透明度为0
                FadeEffect.SetAlphaRecursive(instance, 0f);
                
                // 启动渐显渐隐协程
                fadeCoroutines.Add(StartCoroutine(FadeEffect.FadeInAndOut(instance, fadeInDuration, fadeOutDuration)));
            }

            // 等待所有渐隐效果完成
            foreach (Coroutine coroutine in fadeCoroutines)
            {
                yield return coroutine;
            }

            // 延迟一帧再销毁，确保视觉效果完成
            yield return null;
            
            // 销毁所有实例
            foreach (GameObject instance in instances)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
        }

        private GameObject CreatePrefabInstance(string prefabName)
        {
            GameObject prefab = null;
            GameObject instance = null;

            // 根据名称选择正确的预制体引用
            if (prefabName == "BanlanImage" && banlanImagePrefab != null)
            {
                prefab = banlanImagePrefab;
            }
            else if (prefabName == "LimeImage" && limeImagePrefab != null)
            {
                prefab = limeImagePrefab;
            }

            // 如果没有设置预制体引用，尝试直接加载
            if (prefab == null)
            {
                // 尝试多种可能的预制体路径
                string[] possiblePaths = {
                    "Prefab/" + prefabName,
                    "Prefabs/" + prefabName,
                    prefabName
                };

                #if UNITY_EDITOR
                // 在编辑器模式下尝试不同路径
                foreach (string path in possiblePaths)
                {
                    string fullPath = "Assets/" + path + ".prefab";
                    Debug.Log("Attempting to load prefab from: " + fullPath);
                    UnityEditor.AssetDatabase.Refresh();
                    prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                    if (prefab != null)
                    {
                        Debug.Log("Successfully loaded prefab from: " + fullPath);
                        break;
                    }
                }
                #else
                // 运行时尝试从Resources加载
                foreach (string path in possiblePaths)
                {
                    Debug.Log("Attempting to load prefab from Resources: " + path);
                    prefab = Resources.Load<GameObject>(path);
                    if (prefab != null)
                    {
                        Debug.Log("Successfully loaded prefab from Resources: " + path);
                        break;
                    }
                }
                #endif
                
                if (prefab == null)
                {
                    Debug.LogError("Failed to load prefab: " + prefabName + ". Please ensure the prefab is in Prefab folder and named correctly.");
                    return null;
                }
            }

            // 实例化预制体
            if (prefab != null && canvasTransform != null)
            {
                instance = Instantiate(prefab, canvasTransform);
                
                // 设置合适的位置和大小
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localScale = Vector3.one;
            }

            return instance;
        }

        private void ValidatePrefabReferences()
        {
            // 验证BanlanImage预制体
            if (banlanImagePrefab == null)
            {
                Debug.LogWarning("BanlanImagePrefab reference is missing, will attempt to load at runtime.");
            }
            
            // 验证LimeImage预制体
            if (limeImagePrefab == null)
            {
                Debug.LogWarning("LimeImagePrefab reference is missing, will attempt to load at runtime.");
            }

            // 确保时间设置合理
            if (totalEffectDuration <= 0)
            {
                Debug.LogWarning("Total effect duration must be positive. Setting to default 1.0s.");
                totalEffectDuration = 1.0f;
                fadeInDuration = 0.5f;
                fadeOutDuration = 0.5f;
            }
            
            if (fadeInPercentage <= 0 || fadeInPercentage >= 1)
            {
                Debug.LogWarning("Fade in percentage must be between 0 and 1. Setting to default 0.5.");
                fadeInPercentage = 0.5f;
                fadeInDuration = totalEffectDuration * fadeInPercentage;
                fadeOutDuration = totalEffectDuration - fadeInDuration;
            }
            
            // 验证音频设置
            ValidateAudioSettings();
        }
        
        private void ValidateAudioSettings()
        {
            // 检查音频播放时长
            if (audioPlayDuration <= 0)
            {
                Debug.LogWarning("Audio play duration must be positive. Setting to default 2.0s.");
                audioPlayDuration = 2.0f;
            }
            
            // 在编辑器模式下提示用户可以在Inspector中设置音频引用
            #if UNITY_EDITOR
            if (pourWaterClip == null)
            {
                Debug.Log("Pour water audio clip not set. You can assign it in the Inspector or it will be loaded at runtime from Assets/Audio/pour water.mp3.");
            }
            #endif
        }
    }
}