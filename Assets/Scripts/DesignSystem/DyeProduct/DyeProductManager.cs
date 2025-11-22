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
        [SerializeField] private GameObject hammerImagePrefab; // HammerImage预制体引用
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pourWaterClip;
        [SerializeField] private AudioClip beatClip; // 锤子音效
        [SerializeField] private AudioClip finishClip; // 完成音效
        [SerializeField] private GameObject bucketEmptyObject; // 空桶对象引用
        [SerializeField] private GameObject bucketMixObject; // 混合桶对象引用
        [SerializeField] private GameObject bucketFrothObject; // 泡沫桶对象引用
        [SerializeField] private GameObject banlanObject; // Banlan对象引用
        [SerializeField] private GameObject limeObject; // Lime对象引用
        [SerializeField] private GameObject hammerObject; // Hammer对象引用
        [SerializeField] private GameObject bubbleObject; // Bubble对象引用
        [SerializeField] private GameObject rawmaterialsIntroduce; // 原材料介绍对象引用
        [SerializeField] private GameObject finishObject; // Finish对象引用

        [Header("Settings")]
        [SerializeField] private float totalEffectDuration = 1.0f; // 总效果持续时间
        [SerializeField] private float fadeInPercentage = 0.5f; // 渐显占总时间的比例
        [SerializeField] private float audioPlayDuration = 2.0f; // 音频播放时长
        [SerializeField] private float bucketFadeOutDuration = 1.0f; // 桶透明度过渡持续时间
        [SerializeField] private float moveDuration = 1.0f; // 移动动画持续时间
        [SerializeField] private float hammerDelay = 0.5f; // Hammer开始移动的延迟时间
        [SerializeField] private float banlanLimeMoveDistance = 500f; // Banlan和Lime向左移动距离
        [SerializeField] private float hammerMoveDistance = 400f; // Hammer向右移动距离

        private float fadeInDuration;
        private float fadeOutDuration;
        private Coroutine audioPlayCoroutine;
        private Coroutine bucketFadeCoroutine;
        private Coroutine bucketMixFadeCoroutine; // 混合桶渐变协程引用
        private Coroutine bucketFrothFadeCoroutine; // 泡沫桶渐变协程引用
        private Coroutine banlanMoveCoroutine;
        private Coroutine limeMoveCoroutine;
        private Coroutine hammerMoveCoroutine;
        private Coroutine bubbleMoveCoroutine; // 气泡移动协程引用
        private Coroutine rawmaterialsIntroduceMoveCoroutine; // 原材料介绍移动协程引用
        private Coroutine finishMoveCoroutine; // Finish移动协程引用
        private int nextButtonClickCount = 0; // 下一个按钮的点击次数计数
        private Coroutine hammerAnimationCoroutine; // 锤子动画协程引用

        private void Awake()
        {
            // 初始化协程引用
            hammerAnimationCoroutine = null;
            bucketFadeCoroutine = null;
            bucketMixFadeCoroutine = null;
            bucketFrothFadeCoroutine = null;
            audioPlayCoroutine = null;
            banlanMoveCoroutine = null;
            limeMoveCoroutine = null;
            hammerMoveCoroutine = null;
            bubbleMoveCoroutine = null;
            rawmaterialsIntroduceMoveCoroutine = null;
            
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
            
            // 验证桶对象引用
            ValidateBucketReference();
            
            // 验证移动对象引用
            ValidateMoveObjectReferences();
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
        {    // 移除按钮点击事件监听
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(OnNextButtonClicked);
            }
            
            // 停止所有协程
            StopAllCoroutines();
            
            // 重置协程引用
            hammerAnimationCoroutine = null;
            bucketFadeCoroutine = null;
            bucketMixFadeCoroutine = null;
            audioPlayCoroutine = null;
            banlanMoveCoroutine = null;
            limeMoveCoroutine = null;
            hammerMoveCoroutine = null;
            bubbleMoveCoroutine = null;
            rawmaterialsIntroduceMoveCoroutine = null;
            finishMoveCoroutine = null;
            audioPlayCoroutine = null;
            
            // 重置点击计数
            nextButtonClickCount = 0;
        }

        private void OnNextButtonClicked()
        {    // 增加点击次数
            nextButtonClickCount++;
            Debug.Log($"NextButton clicked {nextButtonClickCount} times");
            
            // 播放倒水音效
            PlayPourWaterSound();
            
            // 开始桶的透明度过渡
            StartBucketFadeOut();
            
            // 根据点击次数执行不同的操作
            if (nextButtonClickCount == 1)
            {    // 第一次点击 - 原有的逻辑
                // 实例化预制体并添加渐显渐隐效果
                StartCoroutine(ShowAndHidePrefabs());
                
                // 开始Banlan和Lime的移动
                StartBanlanLimeMovement();
                
                // 延迟后开始Hammer的移动
                StartCoroutine(StartHammerMovementWithDelay());
            }
            else if (nextButtonClickCount == 2)
            {    // 第二次点击 - 新的锤子动画逻辑
                StartHammerAnimationSequence();
            }
            else if (nextButtonClickCount == 3)
            {    // 第三次点击 - Bubble向左平移1000单位
                StartBubbleLeftMovement();
                
                // 同时启动Finish对象的延迟移动（0.5秒后向右平移900单位）
                StartFinishMovementWithDelay();
                
                // 播放完成音效
                PlayFinishSound();
                
                // 将Bucket_froth的透明度调为0，持续1秒
                FadeBucketFrothToZero();
            }
        }
        
        private void StartHammerAnimationSequence()
        {    // 停止现有的锤子动画协程
            if (hammerAnimationCoroutine != null)
            {    StopCoroutine(hammerAnimationCoroutine);
                hammerAnimationCoroutine = null;
            }
            
            // 播放锤子音效（持续2秒）
            PlayBeatSound();
            
            // 在第二次点击时，将Bucket_mix的透明度调为0，持续1秒
            FadeBucketMixToZero();
            
            // 锤子向左平移1000单位，持续1秒
            if (hammerObject != null)
            {
                Debug.Log("Starting hammer movement to the left by 1000 units over 1 second");
                // 停止现有的锤子移动协程
                if (hammerMoveCoroutine != null)
                {
                    StopCoroutine(hammerMoveCoroutine);
                    hammerMoveCoroutine = null;
                }
                
                // 向左平移1000单位（负值表示向左）
                float hammerMoveDuration = 1.0f; // 持续1秒
                hammerMoveCoroutine = StartCoroutine(MoveObject(hammerObject, new Vector3(-1000f, 0, 0), hammerMoveDuration));
            }
            
            // 0.5秒延迟后气泡向右平移800单位，持续1秒
            if (bubbleObject != null)
            {
                Debug.Log("Starting bubble movement after 0.5s delay, moving right by 900 units over 1 second");
                // 停止现有的气泡移动协程
                if (bubbleMoveCoroutine != null)
                {
                    StopCoroutine(bubbleMoveCoroutine);
                    bubbleMoveCoroutine = null;
                }
                
                float delay = 0.5f; // 延迟0.5秒
                float bubbleMoveDuration = 1.0f; // 持续1秒
                bubbleMoveCoroutine = StartCoroutine(DelayedBubbleMovement(delay, bubbleMoveDuration));
            }
            
            // 原材料介绍向左平移1000单位，持续1秒
            if (rawmaterialsIntroduce != null)
            {
                Debug.Log("Starting rawmaterialsIntroduce movement, moving left by 1000 units over 1 second");
                // 停止现有的原材料介绍移动协程
                if (rawmaterialsIntroduceMoveCoroutine != null)
                {
                    StopCoroutine(rawmaterialsIntroduceMoveCoroutine);
                    rawmaterialsIntroduceMoveCoroutine = null;
                }
                
                float delay = 0f; // 无延迟，立即开始
                float rawmaterialsIntroduceMoveDuration = 1.0f; // 持续1秒
                rawmaterialsIntroduceMoveCoroutine = StartCoroutine(DelayedRawmaterialsIntroduceMovement(delay, rawmaterialsIntroduceMoveDuration));
            }
            
            // 开始新的锤子动画协程
            hammerAnimationCoroutine = StartCoroutine(ShowHammerPrefab());
        }
        
        private void FadeBucketMixToZero()
        {    Debug.Log("Starting to fade Bucket_mix to zero opacity for 1 second");
        StartCoroutine(FadeBucketMixToZeroCoroutine());
        }
        
        private void FadeBucketFrothToZero()
        {    Debug.Log("Starting to fade Bucket_froth to zero opacity for 1 second");
        StartCoroutine(FadeBucketFrothToZeroCoroutine());
        }
        
        // 延迟后气泡移动协程
        private IEnumerator DelayedBubbleMovement(float delay, float duration)
        {    // 等待指定的延迟时间
            yield return new WaitForSeconds(delay);
            
            // 验证气泡对象引用
            if (bubbleObject != null)
            {    Debug.Log("Bubble movement started after delay");
                // 向右平移800单位（正值表示向右）
                yield return StartCoroutine(MoveObject(bubbleObject, new Vector3(900f, 0, 0), duration));
            }
            else
            {    Debug.LogWarning("Bubble object reference is null, cannot move.");
            }
        }
        
        private IEnumerator DelayedRawmaterialsIntroduceMovement(float delay, float duration)
        {
            // 等待指定的延迟时间
            yield return new WaitForSeconds(delay);
            
            // 验证原材料介绍对象引用
            if (rawmaterialsIntroduce != null)
            {
                Debug.Log("RawmaterialsIntroduce movement started after delay: moving left by 1000 units");
                // 向左平移1000单位（负值表示向左）
                yield return StartCoroutine(MoveObject(rawmaterialsIntroduce, new Vector3(-1000f, 0, 0), duration));
            }
            else
            {
                Debug.LogWarning("RawmaterialsIntroduce object reference is null, cannot move.");
            }
        }
        
        private void StartBubbleLeftMovement()
        {
            Debug.Log("Starting Bubble left movement, moving left by 1000 units over 1 second");
            
            // 停止现有的气泡移动协程
            if (bubbleMoveCoroutine != null)
            {
                StopCoroutine(bubbleMoveCoroutine);
                bubbleMoveCoroutine = null;
            }
            
            // 启动新的气泡向左移动协程
            float bubbleMoveDuration = 1.0f; // 持续1秒
            bubbleMoveCoroutine = StartCoroutine(DelayedBubbleLeftMovement(bubbleMoveDuration));
        }
        
        private IEnumerator DelayedBubbleLeftMovement(float duration)
        {
            // 验证气泡对象引用
            if (bubbleObject != null)
            {
                Debug.Log("Bubble left movement started: moving left by 1000 units");
                // 向左平移1000单位（负值表示向左）
                yield return StartCoroutine(MoveObject(bubbleObject, new Vector3(-1000f, 0, 0), duration));
                Debug.Log("Bubble left movement completed");
            }
            else
            {
                Debug.LogWarning("Bubble object reference is null, cannot move.");
            }
        }
        
        private void StartFinishMovementWithDelay()
        {
            Debug.Log("Starting Finish movement with 0.5s delay, moving right by 900 units over 1 second");
            
            // 停止现有的Finish移动协程
            if (finishMoveCoroutine != null)
            {
                StopCoroutine(finishMoveCoroutine);
                finishMoveCoroutine = null;
            }
            
            // 启动新的Finish延迟移动协程（0.5秒延迟，1秒持续时间）
            float finishMoveDelay = 0.5f; // 0.5秒延迟
            float finishMoveDuration = 1.0f; // 持续1秒
            finishMoveCoroutine = StartCoroutine(DelayedFinishMovement(finishMoveDelay, finishMoveDuration));
        }
        
        private IEnumerator DelayedFinishMovement(float delay, float duration)
        {
            // 等待指定的延迟时间（0.5秒）
            yield return new WaitForSeconds(delay);
            
            // 验证Finish对象引用
            if (finishObject != null)
            {
                Debug.Log("Finish movement started after delay: moving right by 900 units");
                // 向右平移900单位（正值表示向右）
                yield return StartCoroutine(MoveObject(finishObject, new Vector3(900f, 0, 0), duration));
                Debug.Log("Finish movement completed");
            }
            else
            {
                Debug.LogWarning("Finish object reference is null, cannot move.");
            }
        }
        
        private IEnumerator FadeBucketMixToZeroCoroutine()
        {    if (bucketMixObject == null)
            {    Debug.LogWarning("Cannot fade bucket mix - bucketMixObject reference is null!");
                yield break;
            }
            
            // 保存初始透明度
            float startAlpha = 1.0f;
            SpriteRenderer[] renderers = bucketMixObject.GetComponentsInChildren<SpriteRenderer>(true);
            Image[] images = bucketMixObject.GetComponentsInChildren<Image>(true);
            
            // 检查是否有可设置透明度的组件
            if (renderers.Length == 0 && images.Length == 0)
            {    Debug.LogWarning("No SpriteRenderer or Image components found on BucketMixObject! Cannot apply fade effect.");
                yield break;
            }
            
            // 记录找到的组件数量用于调试
            Debug.Log($"Starting bucket mix fade effect: {renderers.Length} SpriteRenderers, {images.Length} Images found");
            
            float fadeDuration = 1.0f; // 持续1秒
            float elapsedTime = 0f;
            
            // 执行透明度渐变到0
            while (elapsedTime < fadeDuration)
            {    float t = elapsedTime / fadeDuration;
                float currentAlpha = Mathf.Lerp(startAlpha, 0f, t);
                
                // 设置所有SpriteRenderer的透明度
                foreach (SpriteRenderer renderer in renderers)
                {    if (renderer != null)
                    {    Color color = renderer.color;
                        color.a = currentAlpha;
                        renderer.color = color;
                    }
                }
                
                // 设置所有Image的透明度
                foreach (Image image in images)
                {    if (image != null)
                    {    Color color = image.color;
                        color.a = currentAlpha;
                        image.color = color;
                    }
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保最终透明度为0
            foreach (SpriteRenderer renderer in renderers)
            {    if (renderer != null)
                {    Color color = renderer.color;
                    color.a = 0f;
                    renderer.color = color;
                }
            }
            
            foreach (Image image in images)
            {    if (image != null)
                {    Color color = image.color;
                    color.a = 0f;
                    image.color = color;
                }
            }
            
            Debug.Log("Bucket mix fade effect to zero opacity completed successfully");
        }
        
        private IEnumerator FadeBucketFrothToZeroCoroutine()
        {    if (bucketFrothObject == null)
            {    Debug.LogWarning("Cannot fade bucket froth - bucketFrothObject reference is null!");
                yield break;
            }
            
            // 保存初始透明度
            float startAlpha = 1.0f;
            SpriteRenderer[] renderers = bucketFrothObject.GetComponentsInChildren<SpriteRenderer>(true);
            Image[] images = bucketFrothObject.GetComponentsInChildren<Image>(true);
            
            // 检查是否有可设置透明度的组件
            if (renderers.Length == 0 && images.Length == 0)
            {    Debug.LogWarning("No SpriteRenderer or Image components found on BucketFrothObject! Cannot apply fade effect.");
                yield break;
            }
            
            // 记录找到的组件数量用于调试
            Debug.Log($"Starting bucket froth fade effect: {renderers.Length} SpriteRenderers, {images.Length} Images found");
            
            float fadeDuration = 1.0f; // 持续1秒
            float elapsedTime = 0f;
            
            // 执行透明度渐变到0
            while (elapsedTime < fadeDuration)
            {    float t = elapsedTime / fadeDuration;
                float currentAlpha = Mathf.Lerp(startAlpha, 0f, t);
                
                // 设置所有SpriteRenderer的透明度
                foreach (SpriteRenderer renderer in renderers)
                {    if (renderer != null)
                    {    Color color = renderer.color;
                        color.a = currentAlpha;
                        renderer.color = color;
                    }
                }
                
                // 设置所有Image的透明度
                foreach (Image image in images)
                {    if (image != null)
                    {    Color color = image.color;
                        color.a = currentAlpha;
                        image.color = color;
                    }
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保最终透明度为0
            foreach (SpriteRenderer renderer in renderers)
            {    if (renderer != null)
                {    Color color = renderer.color;
                    color.a = 0f;
                    renderer.color = color;
                }
            }
            
            foreach (Image image in images)
            {    if (image != null)
                {    Color color = image.color;
                    color.a = 0f;
                    image.color = color;
                }
            }
            
            Debug.Log("Bucket froth fade effect to zero opacity completed successfully");
        }
        
        private void PlayBeatSound()
        {    // 如果有正在播放的音频协程，停止它
            if (audioPlayCoroutine != null)
            {    StopCoroutine(audioPlayCoroutine);
            }
            
            // 启动新的音频播放协程，播放beat.wav
            audioPlayCoroutine = StartCoroutine(PlayBeatAudioForDuration());
        }
        
        private void PlayFinishSound()
        {    // 如果有正在播放的音频协程，停止它
            if (audioPlayCoroutine != null)
            {    StopCoroutine(audioPlayCoroutine);
            }
            
            // 启动新的音频播放协程，播放finish.wav
            audioPlayCoroutine = StartCoroutine(PlayFinishAudioForDuration());
        }
        
        private IEnumerator PlayFinishAudioForDuration()
        {    // 如果没有音频剪辑，尝试加载
            if (finishClip == null)
            {    // 尝试从Resources文件夹加载
                finishClip = Resources.Load<AudioClip>("finish");
                
                // 如果Resources中也没有，尝试直接从Assets/Audio文件夹加载（仅编辑器模式）
                #if UNITY_EDITOR
                if (finishClip == null)
                {    string audioPath = "Assets/Audio/finish.wav";
                    Debug.Log("Attempting to load audio clip from: " + audioPath);
                    UnityEditor.AssetDatabase.Refresh();
                    finishClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                }
                #endif
                
                if (finishClip == null)
                {    Debug.LogError("Failed to load finish audio clip!");
                    yield break;
                }
            }
            
            // 设置音频剪辑并播放
            audioSource.clip = finishClip;
            audioSource.loop = false;
            audioSource.Play();
            
            // 等待指定的播放时长（2秒）
            float duration = 2.0f; // 硬性设置为2秒
            yield return new WaitForSeconds(duration);
            
            // 停止播放
            if (audioSource.isPlaying)
            {    audioSource.Stop();
            }
            
            audioPlayCoroutine = null;
        }
        
        private IEnumerator PlayBeatAudioForDuration()
        {    // 如果没有音频剪辑，尝试加载
            if (beatClip == null)
            {    // 尝试从Resources文件夹加载
                beatClip = Resources.Load<AudioClip>("beat");
                
                // 如果Resources中也没有，尝试直接从Assets/Audio文件夹加载（仅编辑器模式）
                #if UNITY_EDITOR
                if (beatClip == null)
                {    string audioPath = "Assets/Audio/beat.wav";
                    Debug.Log("Attempting to load audio clip from: " + audioPath);
                    UnityEditor.AssetDatabase.Refresh();
                    beatClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                }
                #endif
                
                if (beatClip == null)
                {    Debug.LogError("Failed to load beat audio clip!");
                    yield break;
                }
            }
            
            // 设置音频剪辑并播放
            audioSource.clip = beatClip;
            audioSource.loop = false;
            audioSource.Play();
            
            // 等待指定的播放时长（2秒）
            float duration = 2.0f; // 硬性设置为2秒，符合需求
            yield return new WaitForSeconds(duration);
            
            // 停止播放
            if (audioSource.isPlaying)
            {    audioSource.Stop();
            }
            
            audioPlayCoroutine = null;
        }
        
        private IEnumerator ShowHammerPrefab()
        {    Debug.Log("Starting hammer animation sequence for second button click");
            
            // 实例化Hammer预制体
            GameObject hammerInstance = null;
            RectTransform rectTransform = null;
            bool initialized = false;
            Quaternion startRotation = Quaternion.identity;
            Quaternion rotation90 = Quaternion.identity;
            float rotationDuration = 0f;
            
            // 参数验证和初始化 - 非yield部分
            try
            {    if (hammerImagePrefab != null)
                {    Debug.Log("Creating HammerImagePrefab instance from inspector reference");
                    hammerInstance = Instantiate(hammerImagePrefab, canvasTransform);
                    hammerInstance.transform.localPosition = Vector3.zero;
                    hammerInstance.transform.localScale = Vector3.one;
                    hammerInstance.transform.localRotation = Quaternion.identity;
                }
                else
                {    Debug.LogWarning("HammerImagePrefab reference is missing, attempting to load from resources");
                    hammerInstance = CreatePrefabInstance("HammerImage");
                }
                
                if (hammerInstance == null)
                {    Debug.LogError("Failed to create HammerImage instance: both inspector reference and resource loading failed");
                }
                else
                {    Debug.Log($"Hammer instance created successfully: {hammerInstance.name}");
                    
                    // 获取RectTransform组件
                    rectTransform = hammerInstance.GetComponent<RectTransform>();
                    if (rectTransform == null)
                    {    Debug.LogWarning("Hammer instance does not have RectTransform component, using Transform instead");
                    }
                    else
                    {    // 验证动画参数
                        if (fadeInDuration <= 0)
                        {    Debug.LogWarning("FadeInDuration is invalid, setting to default 0.3s");
                            fadeInDuration = 0.3f;
                        }
                        
                        if (fadeOutDuration <= 0)
                        {    Debug.LogWarning("FadeOutDuration is invalid, setting to default 0.3s");
                            fadeOutDuration = 0.3f;
                        }
                        
                        // 设置初始透明度为0
                        FadeEffect.SetAlphaRecursive(hammerInstance, 0f);
                        Debug.Log("Initial alpha set to 0, preparing for fade-in");
                        
                        // 初始化旋转参数
                        startRotation = rectTransform.localRotation;
                        rotation90 = Quaternion.Euler(0, 0, 90);
                        rotationDuration = 1f / 4f; // 每次旋转1/4秒
                        
                        initialized = true;
                    }
                }
            }
            catch (System.Exception e)
            {    Debug.LogError($"Exception occurred during initialization: {e.Message}\n{e.StackTrace}");
            }
            
            // 如果初始化失败，销毁实例并退出
            if (!initialized || hammerInstance == null || rectTransform == null)
            {    if (hammerInstance != null)
                {    Debug.Log("Cleaning up hammer instance due to initialization failure");
                    Destroy(hammerInstance);
                }
                hammerAnimationCoroutine = null;
                Debug.Log("Hammer animation coroutine reference cleared");
                yield break;
            }
            
            // 渐显效果 - 现在在try-catch外执行yield
            Debug.Log($"Starting fade-in effect with duration: {fadeInDuration}s");
            yield return StartCoroutine(FadeEffect.Fade(hammerInstance, 0f, 1f, fadeInDuration));
            Debug.Log("Fade-in effect completed");
            
            // 实现两次往复旋转90度（持续1秒）
            Debug.Log("Starting rotation sequence (total 1 second duration)");
            
            // 第一次旋转90度
            Debug.Log("First rotation: 0° -> 90°");
            yield return StartCoroutine(RotateObject(rectTransform, startRotation, rotation90, rotationDuration));
            
            // 第二次旋转回原位
            Debug.Log("Second rotation: 90° -> 0°");
            yield return StartCoroutine(RotateObject(rectTransform, rotation90, startRotation, rotationDuration));
            
            // 第三次旋转90度
            Debug.Log("Third rotation: 0° -> 90°");
            yield return StartCoroutine(RotateObject(rectTransform, startRotation, rotation90, rotationDuration));
            
            // 第四次旋转回原位
            Debug.Log("Fourth rotation: 90° -> 0°");
            yield return StartCoroutine(RotateObject(rectTransform, rotation90, startRotation, rotationDuration));
            
            Debug.Log("Rotation sequence completed");
            
            // 渐隐效果
            Debug.Log($"Starting fade-out effect with duration: {fadeOutDuration}s");
            yield return StartCoroutine(FadeEffect.Fade(hammerInstance, 1f, 0f, fadeOutDuration));
            Debug.Log("Fade-out effect completed");
            
            // 延迟一帧再销毁，确保视觉效果完成
            yield return null;
            
            // 销毁实例
            if (hammerInstance != null)
            {    Debug.Log($"Destroying hammer instance: {hammerInstance.name}");
                Destroy(hammerInstance);
            }
            
            hammerAnimationCoroutine = null;
            Debug.Log("Hammer animation coroutine reference cleared");
        }
        
        // 旋转对象的协程
        private IEnumerator RotateObject(RectTransform target, Quaternion startRotation, Quaternion endRotation, float duration)
        {
            if (target == null)
            {
                Debug.LogWarning("Cannot rotate null RectTransform");
                yield break;
            }
            
            if (duration <= 0)
            {
                target.localRotation = endRotation;
                yield break;
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float progress = elapsedTime / duration;
                target.localRotation = Quaternion.Slerp(startRotation, endRotation, progress);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            target.localRotation = endRotation;
        }
        
        private void StartBanlanLimeMovement()
        {    Debug.Log("Starting Banlan and Lime movement: " + banlanLimeMoveDistance + " units left over " + moveDuration + " seconds");
            
            // 停止现有的移动协程
            if (banlanMoveCoroutine != null)
            {    StopCoroutine(banlanMoveCoroutine);
                banlanMoveCoroutine = null;
            }
            if (limeMoveCoroutine != null)
            {    StopCoroutine(limeMoveCoroutine);
                limeMoveCoroutine = null;
            }
            
            // 开始新的移动协程
            if (banlanObject != null)
            {    Debug.Log("Starting move for Banlan");
                banlanMoveCoroutine = StartCoroutine(MoveObject(banlanObject, new Vector3(-banlanLimeMoveDistance, 0, 0), moveDuration));
            }
            else
            {    Debug.LogWarning("Banlan object is null, skipping movement");
            }
            
            if (limeObject != null)
            {    Debug.Log("Starting move for Lime");
                limeMoveCoroutine = StartCoroutine(MoveObject(limeObject, new Vector3(-banlanLimeMoveDistance, 0, 0), moveDuration));
            }
            else
            {    Debug.LogWarning("Lime object is null, skipping movement");
            }
        }
        
        private IEnumerator StartHammerMovementWithDelay()
        {
            Debug.Log("Starting Hammer movement with " + hammerDelay + " seconds delay");
            yield return new WaitForSeconds(hammerDelay);
            
            // 停止现有的锤子移动协程
            if (hammerMoveCoroutine != null)
            {
                StopCoroutine(hammerMoveCoroutine);
                hammerMoveCoroutine = null;
            }
            
            // 开始锤子移动协程
            if (hammerObject != null)
            {
                Debug.Log("Starting move for Hammer: " + hammerMoveDistance + " units right over " + moveDuration + " seconds");
                hammerMoveCoroutine = StartCoroutine(MoveObject(hammerObject, new Vector3(hammerMoveDistance, 0, 0), moveDuration));
            }
            else
            {
                Debug.LogWarning("Hammer object is null, skipping movement");
            }
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
        {    // 检查音频播放时长
            if (audioPlayDuration <= 0)
            {                Debug.LogWarning("Audio play duration must be positive. Setting to default 2.0s.");
                audioPlayDuration = 2.0f;
            }
            
            // 在编辑器模式下提示用户可以在Inspector中设置音频引用
            #if UNITY_EDITOR
            if (pourWaterClip == null)
            {                Debug.Log("Pour water audio clip not set. You can assign it in the Inspector or it will be loaded at runtime from Assets/Audio/pour water.mp3.");
            }
            #endif
        }
        
        private void ValidateBucketReference()
        {    // 确保桶对象引用有效
            if (bucketEmptyObject == null)
            {                Debug.LogWarning("BucketEmptyObject reference is missing! Please assign it in the Inspector.");
            }
            
            if (bucketMixObject == null)
            {                Debug.LogWarning("BucketMixObject reference is missing! Please assign it in the Inspector.");
            }
            
            // 确保桶透明度过渡时间有效
            if (bucketFadeOutDuration <= 0)
            {                Debug.LogWarning("Bucket fade out duration must be positive. Setting to default 1.0s.");
                bucketFadeOutDuration = 1.0f;
            }
        }
        
        private void StartBucketFadeOut()
        {    // 如果有正在运行的桶透明度协程，停止它
            if (bucketFadeCoroutine != null)
            {                StopCoroutine(bucketFadeCoroutine);
            }
            
            // 启动新的透明度渐变协程
            bucketFadeCoroutine = StartCoroutine(FadeBucketToZero());
        }
        
        private IEnumerator FadeBucketToZero()
        {    if (bucketEmptyObject == null)
            {                Debug.LogWarning("Cannot fade bucket - bucketEmptyObject reference is null!");
                yield break;
            }
            
            // 保存初始透明度
            float startAlpha = 1.0f;
            SpriteRenderer[] renderers = bucketEmptyObject.GetComponentsInChildren<SpriteRenderer>(true);
            Image[] images = bucketEmptyObject.GetComponentsInChildren<Image>(true);
            
            // 检查是否有可设置透明度的组件
            if (renderers.Length == 0 && images.Length == 0)
            {                Debug.LogWarning("No SpriteRenderer or Image components found on BucketEmptyObject! Cannot apply fade effect.");
                yield break;
            }
            
            // 记录找到的组件数量用于调试
            Debug.Log($"Starting bucket fade effect: {renderers.Length} SpriteRenderers, {images.Length} Images found");
            
            float elapsedTime = 0f;
            
            // 执行透明度渐变
            while (elapsedTime < bucketFadeOutDuration)
            {                float t = elapsedTime / bucketFadeOutDuration;
                float currentAlpha = Mathf.Lerp(startAlpha, 0f, t);
                
                // 设置所有SpriteRenderer的透明度
                foreach (SpriteRenderer renderer in renderers)
                {                    if (renderer != null)
                    {                        Color color = renderer.color;
                        color.a = currentAlpha;
                        renderer.color = color;
                    }
                }
                
                // 设置所有Image的透明度
                foreach (Image image in images)
                {                    if (image != null)
                    {                        Color color = image.color;
                        color.a = currentAlpha;
                        image.color = color;
                    }
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保最终透明度为0
            foreach (SpriteRenderer renderer in renderers)
            {                if (renderer != null)
                {                    Color color = renderer.color;
                    color.a = 0f;
                    renderer.color = color;
                }
            }
            
            foreach (Image image in images)
            {                if (image != null)
                {                    Color color = image.color;
                    color.a = 0f;
                    image.color = color;
                }
            }
            
            Debug.Log("Bucket fade effect completed successfully");
            bucketFadeCoroutine = null;
        }
        
        private IEnumerator MoveObject(GameObject targetObject, Vector3 targetOffset, float duration)
        {    if (targetObject == null)
            {    Debug.LogWarning("Cannot move null object!");
                yield break;
            }
            
            RectTransform rectTransform = targetObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {    Debug.LogWarning("Object does not have RectTransform component: " + targetObject.name);
                yield break;
            }
            
            // 记录起始位置
            Vector3 startPosition = rectTransform.anchoredPosition3D;
            // 计算目标位置
            Vector3 targetPosition = startPosition + targetOffset;
            
            float elapsedTime = 0f;
            
            // 平滑移动对象
            while (elapsedTime < duration)
            {    // 使用线性插值计算当前位置
                float progress = elapsedTime / duration;
                rectTransform.anchoredPosition3D = Vector3.Lerp(startPosition, targetPosition, progress);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保对象精确到达目标位置
            rectTransform.anchoredPosition3D = targetPosition;
            
            Debug.Log("Object movement completed for: " + targetObject.name + ". Final position: " + targetPosition);
        }
        
        private void ValidateMoveObjectReferences()
        {
            // 验证Banlan对象引用
            if (banlanObject == null)
            {
                Debug.LogWarning("BanlanObject reference is missing! Please assign it in the Inspector.");
            }
            
            // 验证Lime对象引用
            if (limeObject == null)
            {
                Debug.LogWarning("LimeObject reference is missing! Please assign it in the Inspector.");
            }
            
            // 验证Hammer对象引用
            if (hammerObject == null)
            {
                Debug.LogWarning("HammerObject reference is missing! Please assign it in the Inspector.");
            }
            
            // 验证Bubble对象引用
            if (bubbleObject == null)
            {
                Debug.LogWarning("BubbleObject reference is missing! Please assign it in the Inspector.");
            }
            
            // 验证原材料介绍对象引用
            if (rawmaterialsIntroduce == null)
            {
                Debug.LogWarning("RawmaterialsIntroduce reference is missing! Please assign it in the Inspector.");
            }
            
            // 验证Finish对象引用
            if (finishObject == null)
            {
                Debug.LogWarning("FinishObject reference is missing! Please assign it in the Inspector.");
            }
            
            // 确保移动参数有效
            if (moveDuration <= 0)
            {
                Debug.LogWarning("Move duration must be positive. Setting to default 1.0s.");
                moveDuration = 1.0f;
            }
            
            if (hammerDelay < 0)
            {
                Debug.LogWarning("Hammer delay cannot be negative. Setting to default 0.5s.");
                hammerDelay = 0.5f;
            }
        }
    }
}