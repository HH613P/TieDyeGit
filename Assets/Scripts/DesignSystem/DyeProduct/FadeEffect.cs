using UnityEngine;
using System.Collections;

namespace DesignSystem.DyeProduct
{
    public static class FadeEffect
    {
        /// <summary>
        /// 为游戏对象添加渐显渐隐效果
        /// </summary>
        /// <param name="target">目标游戏对象</param>
        /// <param name="fadeInDuration">渐显持续时间</param>
        /// <param name="fadeOutDuration">渐隐持续时间</param>
        /// <param name="onComplete">效果完成后的回调</param>
        public static IEnumerator FadeInAndOut(GameObject target, float fadeInDuration, float fadeOutDuration, System.Action onComplete = null)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            // 保存初始状态
            Color[] initialColors = SaveInitialColors(target);
            Component[] coloredComponents = GetColoredComponents(target);

            // 渐显
            yield return Fade(target, 0f, 1f, fadeInDuration);

            // 渐隐
            yield return Fade(target, 1f, 0f, fadeOutDuration);

            // 恢复初始状态（可选）
            // RestoreInitialColors(coloredComponents, initialColors);

            // 调用完成回调
            onComplete?.Invoke();
        }

        /// <summary>
        /// 执行淡入淡出效果
        /// </summary>
        /// <param name="target">目标游戏对象</param>
        /// <param name="startAlpha">起始透明度</param>
        /// <param name="endAlpha">结束透明度</param>
        /// <param name="duration">持续时间</param>
        public static IEnumerator Fade(GameObject target, float startAlpha, float endAlpha, float duration)
        {
            if (target == null || duration <= 0f)
                yield break;

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                SetAlphaRecursive(target, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 确保设置为最终值
            SetAlphaRecursive(target, endAlpha);
        }

        /// <summary>
        /// 递归设置游戏对象及其子对象的透明度
        /// </summary>
        /// <param name="gameObject">目标游戏对象</param>
        /// <param name="alpha">目标透明度</param>
        public static void SetAlphaRecursive(GameObject gameObject, float alpha)
        {
            if (gameObject == null)
                return;

            // 设置所有图形组件
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                foreach (Material material in materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = alpha;
                        material.color = color;
                    }
                    else if (material.HasProperty("_TintColor")) // 对于UI/粒子着色器
                    {
                        Color color = material.GetColor("_TintColor");
                        color.a = alpha;
                        material.SetColor("_TintColor", color);
                    }
                }
            }

            // 设置所有UI图像组件
            UnityEngine.UI.Image[] images = gameObject.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach (UnityEngine.UI.Image image in images)
            {
                Color color = image.color;
                color.a = alpha;
                image.color = color;
            }

            // 设置所有文本组件
            UnityEngine.UI.Text[] texts = gameObject.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (UnityEngine.UI.Text text in texts)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }

            // 设置所有SpriteRenderer组件
            SpriteRenderer[] sprites = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sprite in sprites)
            {
                Color color = sprite.color;
                color.a = alpha;
                sprite.color = color;
            }
        }

        // 保存组件的初始颜色（可选功能）
        private static Color[] SaveInitialColors(GameObject target)
        {
            Component[] components = GetColoredComponents(target);
            Color[] colors = new Color[components.Length];

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is UnityEngine.UI.Image image)
                    colors[i] = image.color;
                else if (components[i] is UnityEngine.UI.Text text)
                    colors[i] = text.color;
                else if (components[i] is SpriteRenderer sprite)
                    colors[i] = sprite.color;
                else if (components[i] is Renderer renderer)
                    colors[i] = renderer.material.color;
            }

            return colors;
        }

        // 获取所有可着色的组件
        private static Component[] GetColoredComponents(GameObject target)
        {
            return target.GetComponentsInChildren<Component>(true);
        }

        // 恢复组件的初始颜色（可选功能）
        private static void RestoreInitialColors(Component[] components, Color[] initialColors)
        {
            if (components.Length != initialColors.Length)
                return;

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is UnityEngine.UI.Image image)
                    image.color = initialColors[i];
                else if (components[i] is UnityEngine.UI.Text text)
                    text.color = initialColors[i];
                else if (components[i] is SpriteRenderer sprite)
                    sprite.color = initialColors[i];
                else if (components[i] is Renderer renderer)
                    renderer.material.color = initialColors[i];
            }
        }
    }
}