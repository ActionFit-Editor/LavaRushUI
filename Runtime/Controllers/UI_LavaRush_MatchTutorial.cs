using System;
using System.Collections.Generic;
using ActionFit.LavaRush.UI;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialFocusSprite
{
    Default = 0,
    Circle = 1,
}

/// <summary>
/// Preserved match-guide controller identity. This is intentionally a plain controller helper,
/// as in the original production implementation; the package-owned tutorial prefab uses
/// <see cref="LavaRushTutorialView"/>.
/// </summary>
public class UI_LavaRush_MatchTutorial
{
    private readonly UI_LavaRush_Match _match;
    private readonly UI_Rect[] _steps;
    private readonly List<UI_Image>[] _stepFocuses;
    private int _currentStep;
    private bool _isActive;
    private bool _locked;
    private bool _willStart;

    public bool IsActive => _isActive;
    public Action<int> OnStepShown;
    public Action OnGuideCompleted;

    public UI_LavaRush_MatchTutorial(UI_LavaRush_Match match)
    {
        _match = match ?? throw new ArgumentNullException(nameof(match));
        _steps = new[]
        {
            match.refs.tutorial1,
            match.refs.tutorial2,
            match.refs.tutorial3,
        };
        _stepFocuses = new List<UI_Image>[_steps.Length];
        for (int index = 0; index < _stepFocuses.Length; index++)
            _stepFocuses[index] = new List<UI_Image>();
    }

    public void PrepareGuideRoot()
    {
        HideAll();
        ClearAllStepFocuses();
        _willStart = _match.IsInTutorial
            && _match.refs.rectMatchTutorial != null
            && _steps[0] != null
            && _steps[1] != null
            && _steps[2] != null;

        if (_willStart)
            _match.refs.rectMatchTutorial.gameObject.SetActive(true);
    }

    public bool StartGuide()
    {
        if (!_willStart)
            return false;

        _currentStep = 0;
        _isActive = true;
        _locked = false;
        ShowStep(0);
        OnStepShown?.Invoke(0);
        return true;
    }

    public void UpdateClick() => UpdateClick(Input.GetMouseButtonUp(0));

    internal void UpdateClick(bool pointerReleased)
    {
        if (!_isActive || _locked || !pointerReleased)
            return;

        _match.Owner?.PlayAudio(LavaRushAudioCue.TutorialStep);
        int lastStep = _steps.Length - 1;
        if (_currentStep >= lastStep)
        {
            Complete();
            return;
        }

        int nextStep = _currentStep + 1;
        SwapStep(_currentStep, nextStep);
        OnStepShown?.Invoke(nextStep);
    }

    public void SetLocked(bool locked)
    {
        if (_isActive)
            _locked = locked;
    }

    public void ForceStop()
    {
        if (!_isActive)
            return;

        _isActive = false;
        _locked = false;
        HideAll();
    }

    public UI_Image AddStepFocus(
        int step,
        RectTransform target,
        Sprite sprite,
        Vector2 size,
        Image.Type? imageType = null)
    {
        if (step < 0 || step >= _steps.Length)
        {
            Debug.LogError($"[UI_LavaRush_MatchTutorial] step out of range: {step}");
            return null;
        }
        if (_steps[step] == null)
        {
            Debug.LogError($"[UI_LavaRush_MatchTutorial] _steps[{step}] is null");
            return null;
        }
        UI_Image instance = _match.CreateTutorialFocus(
            _steps[step].RectTransform,
            sprite,
            size,
            imageType);
        if (instance == null)
            return null;

        if (target != null)
        {
            instance.RectTransform.pivot = target.pivot;
            instance.RectTransform.position = target.position;
        }

        Vector3 lossyScale = _steps[step].RectTransform.lossyScale;
        float scaleX = Mathf.Abs(lossyScale.x) > 1e-4f ? lossyScale.x : 1f;
        float scaleY = Mathf.Abs(lossyScale.y) > 1e-4f ? lossyScale.y : 1f;
        instance.RectTransform.sizeDelta = new Vector2(size.x / scaleX, size.y / scaleY);

        _stepFocuses[step].Add(instance);
        return instance;
    }

    public UI_Image AddStepFocus(
        int step,
        RectTransform target,
        TutorialFocusSprite spriteType,
        Vector2 size,
        Image.Type? imageType = null)
    {
        if (step < 0 || step >= _steps.Length || _steps[step] == null)
            return AddStepFocus(
                step,
                target,
                ResolveFocusSprite(spriteType),
                size,
                imageType);

        Vector3 lossyScale = _steps[step].RectTransform.lossyScale;
        float scaleX = Mathf.Abs(lossyScale.x) > 1e-4f ? lossyScale.x : 1f;
        float scaleY = Mathf.Abs(lossyScale.y) > 1e-4f ? lossyScale.y : 1f;
        return AddStepFocus(
            step,
            target,
            ResolveFocusSprite(spriteType),
            new Vector2(size.x * scaleX, size.y * scaleY),
            imageType);
    }

    public void ClearStepFocuses(int step)
    {
        if (step < 0 || step >= _stepFocuses.Length)
            return;

        List<UI_Image> entries = _stepFocuses[step];
        for (int index = 0; index < entries.Count; index++)
            if (entries[index] != null)
                UnityEngine.Object.Destroy(entries[index].gameObject);
        entries.Clear();
    }

    public void ClearAllStepFocuses()
    {
        for (int index = 0; index < _stepFocuses.Length; index++)
            ClearStepFocuses(index);
    }

    private Sprite ResolveFocusSprite(TutorialFocusSprite spriteType) =>
        _match.ResolveTutorialSprite(spriteType);

    private void ShowStep(int index)
    {
        if (_steps[index] != null)
            _steps[index].gameObject.SetActive(true);
    }

    private void SwapStep(int fromIndex, int toIndex)
    {
        if (_steps[fromIndex] != null)
            _steps[fromIndex].gameObject.SetActive(false);
        _currentStep = toIndex;
        if (_steps[toIndex] != null)
            _steps[toIndex].gameObject.SetActive(true);
    }

    private void Complete()
    {
        _isActive = false;
        _locked = false;
        HideAll();
        OnGuideCompleted?.Invoke();
    }

    private void HideAll()
    {
        if (_match.refs.rectMatchTutorial != null)
            _match.refs.rectMatchTutorial.gameObject.SetActive(false);

        for (int index = 0; index < _steps.Length; index++)
            if (_steps[index] != null)
                _steps[index].gameObject.SetActive(false);
    }
}
