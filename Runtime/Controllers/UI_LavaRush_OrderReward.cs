using System;
using System.Collections;
using System.Collections.Generic;
using ActionFit.LavaRush.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Explicit order/merge reward flight effect; no product order or global-manager lookup.</summary>
public class UI_LavaRush_OrderReward : MonoBehaviour
{
    [Serializable]
    public class Refs
    {
        public TextMeshProUGUI txtReward;
        public Image imgEffect;
    }

    [Serializable]
    public sealed class Settings
    {
        [SerializeField, Min(0f)] private float duration = 0.6f;
        [SerializeField, Min(0f)] private float stagger = 0.08f;
        [SerializeField] private float arcHeight = 200f;
        [SerializeField, Min(1f)] private float maxScale = 1.5f;

        public float Duration => duration;
        public float Stagger => stagger;
        public float ArcHeight => arcHeight;
        public float MaxScale => maxScale;
    }

    private static readonly List<UI_LavaRush_OrderReward> DisplayInstances = new();
    private static readonly List<UI_LavaRush_OrderReward> MergeEffectInstances = new();
    private readonly List<RectTransform> _activeFlights = new();

    [SerializeField] public Refs refs = new();
    [SerializeField] private Settings settings = new();

    private Func<int> _amount;
    private Func<bool> _visible;
    private Sprite _mergeSprite;
    private Action _mergeArrived;
    private ILavaRushAudio _audio = NullLavaRushAudio.Instance;
    private bool _displayRegistered;
    private bool _mergeEffectRegistered;

    public static void RefreshAll()
    {
        for (int index = DisplayInstances.Count - 1; index >= 0; index--)
        {
            if (DisplayInstances[index] == null)
                DisplayInstances.RemoveAt(index);
            else
                DisplayInstances[index].Refresh();
        }
    }

    public void Configure(Func<int> amount, Func<bool> visible)
    {
        _amount = amount;
        _visible = visible;
        EnsureDisplayRegistered();
        Refresh();
    }

    public void ConfigureAudio(ILavaRushAudio audio)
    {
        _audio = audio ?? NullLavaRushAudio.Instance;
    }

    public void ConfigureMergeEffect(
        Sprite sprite,
        ILavaRushAudio audio = null,
        Action onArrive = null)
    {
        _mergeSprite = sprite;
        _mergeArrived = onArrive;
        _audio = audio ?? NullLavaRushAudio.Instance;
        EnsureMergeEffectRegistered();
    }

    public void Refresh()
    {
        EnsureDisplayRegistered();
        bool visible = _visible?.Invoke() ?? false;
        int amount = Mathf.Max(0, _amount?.Invoke() ?? 0);
        if (refs?.txtReward != null)
            refs.txtReward.text = amount.ToString();
        gameObject.SetActive(visible && amount > 0);
    }

    public void PlayEffect(int progressAmount)
    {
        RectTransform target = UI_LavaRush_Cell.Primary?.TargetProgress?.RectTransform;
        if (refs?.imgEffect != null)
            PlayEffect(refs.imgEffect, target, progressAmount);
    }

    public void PlayEffect(
        Image source,
        RectTransform target,
        int progressAmount,
        Action onSpawn = null,
        Action onArrive = null,
        Transform parent = null)
    {
        if (source == null || target == null || progressAmount <= 0)
            return;

        Canvas canvas = source.canvas?.rootCanvas;
        if (canvas == null)
            return;

        StartFlights(
            source.sprite,
            source.color,
            source.rectTransform.rect.size,
            source.rectTransform.position,
            target,
            progressAmount,
            onSpawn,
            onArrive,
            parent != null ? parent : canvas.transform,
            "LavaRushRewardFlight");
    }

    public void PlayEffect(
        Sprite sprite,
        Vector3 startWorldPosition,
        RectTransform target,
        int progressAmount,
        Action onSpawn = null,
        Action onArrive = null,
        Transform parent = null)
    {
        if (sprite == null || target == null || progressAmount <= 0)
            return;

        Canvas canvas = target.GetComponentInParent<Canvas>()?.rootCanvas;
        if (canvas == null)
            return;

        StartFlights(
            sprite,
            Color.white,
            new Vector2(sprite.rect.width, sprite.rect.height),
            startWorldPosition,
            target,
            progressAmount,
            onSpawn,
            onArrive,
            parent != null ? parent : canvas.transform,
            "LavaRushMergeFly");
    }

    public static void PlayMergeEffect(Vector3 startWorldPosition, int progressAmount)
    {
        if (progressAmount <= 0)
            return;

        UI_LavaRush_OrderReward effect = FindMergeEffect();
        UI_Image targetProgress = UI_LavaRush_Cell.Primary?.TargetProgress;
        if (effect == null || effect._mergeSprite == null || targetProgress == null)
            return;

        Action arrived = effect._mergeArrived ?? (() =>
        {
            UI_LavaRush_Cell.Primary?.PlayRewardArrive();
            UI_LavaRush_Cell.NotifyProgressArrived();
        });
        effect.PlayEffect(
            effect._mergeSprite,
            startWorldPosition,
            targetProgress.RectTransform,
            progressAmount,
            onArrive: arrived);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        for (int index = _activeFlights.Count - 1; index >= 0; index--)
            if (_activeFlights[index] != null)
                DestroyRuntimeObject(_activeFlights[index].gameObject);
        _activeFlights.Clear();
    }

    private void OnDestroy()
    {
        DisplayInstances.Remove(this);
        MergeEffectInstances.Remove(this);
        _displayRegistered = false;
        _mergeEffectRegistered = false;
    }

    private void StartFlights(
        Sprite sprite,
        Color color,
        Vector2 size,
        Vector3 startWorldPosition,
        RectTransform target,
        int progressAmount,
        Action onSpawn,
        Action onArrive,
        Transform parent,
        string cloneName)
    {
        int count = Mathf.Clamp(Mathf.CeilToInt(progressAmount / 5f), 1, 8);
        _audio.Play(LavaRushAudioCue.RewardSpawn);
        onSpawn?.Invoke();
        float worldScale = Mathf.Max(0.0001f, parent.lossyScale.x);
        for (int index = 0; index < count; index++)
        {
            var imageObject = new GameObject(
                cloneName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            var rect = (RectTransform)imageObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.position = startWorldPosition + new Vector3(
                UnityEngine.Random.Range(-30f, 30f) * worldScale,
                UnityEngine.Random.Range(-20f, 20f) * worldScale,
                0f);
            rect.sizeDelta = size;
            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            _activeFlights.Add(rect);
            float stagger = settings != null ? settings.Stagger : 0.08f;
            StartCoroutine(Animate(rect, target, index * stagger, onArrive));
        }
    }

    private IEnumerator Animate(
        RectTransform flight,
        RectTransform target,
        float delay,
        Action onArrive)
    {
        float delayed = 0f;
        while (delayed < delay && flight != null && target != null)
        {
            delayed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (flight == null || target == null)
        {
            DestroyFlight(flight);
            yield break;
        }

        Vector3 start = flight.position;
        float duration = settings != null && settings.Duration > 0f ? settings.Duration : 0.6f;
        float arcHeight = settings != null && settings.ArcHeight > 0f ? settings.ArcHeight : 200f;
        float maxScale = settings != null && settings.MaxScale > 0f ? settings.MaxScale : 1.5f;
        float elapsed = 0f;
        while (elapsed < duration && flight != null && target != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            Vector3 end = target.position;
            float eased = normalized * normalized;
            Vector3 position = Vector3.Lerp(start, end, eased);
            position.y += 4f * arcHeight * normalized * (1f - normalized);
            flight.position = position;
            float scale = normalized < 0.4f
                ? Mathf.Lerp(1f, maxScale, normalized / 0.4f)
                : normalized < 0.7f
                    ? maxScale
                    : Mathf.Lerp(maxScale, 0f, (normalized - 0.7f) / 0.3f);
            flight.localScale = Vector3.one * scale;
            yield return null;
        }

        bool arrived = flight != null && target != null;
        DestroyFlight(flight);
        if (arrived)
            onArrive?.Invoke();
    }

    private void EnsureDisplayRegistered()
    {
        if (_displayRegistered)
            return;

        DisplayInstances.Add(this);
        _displayRegistered = true;
    }

    private void EnsureMergeEffectRegistered()
    {
        if (_mergeEffectRegistered)
            return;

        MergeEffectInstances.Add(this);
        _mergeEffectRegistered = true;
    }

    private void DestroyFlight(RectTransform flight)
    {
        _activeFlights.Remove(flight);
        if (flight != null)
            DestroyRuntimeObject(flight.gameObject);
    }

    private static void DestroyRuntimeObject(GameObject value)
    {
        if (value == null)
            return;

        if (Application.isPlaying)
            Destroy(value);
        else
            DestroyImmediate(value);
    }

    private static UI_LavaRush_OrderReward FindMergeEffect()
    {
        for (int index = MergeEffectInstances.Count - 1; index >= 0; index--)
        {
            UI_LavaRush_OrderReward instance = MergeEffectInstances[index];
            if (instance == null)
            {
                MergeEffectInstances.RemoveAt(index);
                continue;
            }
            if (instance.isActiveAndEnabled && instance._mergeSprite != null)
                return instance;
        }
        return null;
    }
}
