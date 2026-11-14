using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class HitscanImpactService : IDisposable
{
    private const float MARK_SIZE = 0.16f;
    private const float MARK_LIFETIME = 6f;
    private const float MARK_FADE_DURATION = 0.8f;
    private const float MARK_FADE_DELAY = MARK_LIFETIME - MARK_FADE_DURATION;
    private const float MARK_SURFACE_OFFSET = 0.01f;
    private const float FLASH_SIZE = 0.32f;
    private const float FLASH_START_SIZE = 0.16f;
    private const float FLASH_LIFETIME = 0.16f;
    private const float FLASH_SURFACE_OFFSET = 0.03f;

    private static readonly Color MARK_COLOR = new Color(0.05f, 0.05f, 0.05f, 0.85f);
    private static readonly Color FLASH_COLOR = new Color(1f, 0.85f, 0.35f, 1f);

    private readonly Sprite _markSprite;
    private readonly Sprite _flashSprite;
    private Camera _viewCamera;

    public HitscanImpactService(Texture2D markTexture, Texture2D flashTexture)
    {
        _markSprite = CreateSprite(markTexture);
        _flashSprite = CreateSprite(flashTexture != null ? flashTexture : markTexture);

        if (_markSprite == null)
        {
            Debug.LogWarning("Hitscan impact visuals disabled: assign an impact texture in GameplayInstaller.");
        }
    }

    public void Spawn(Vector3 point, Vector3 normal, Transform parent = null)
    {
        Vector3 direction = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;

        SpawnMark(point, direction, parent);
        SpawnFlash(point, direction, parent);
    }

    public void Dispose()
    {
        DestroyResource(_markSprite);
        DestroyResource(_flashSprite);
    }

    private void SpawnMark(Vector3 point, Vector3 normal, Transform parent)
    {
        if (_markSprite == null)
        {
            return;
        }

        SpriteRenderer mark = CreateEffect("HitscanMark", _markSprite, MARK_COLOR, MARK_SIZE, parent);
        mark.transform.position = point + normal * MARK_SURFACE_OFFSET;
        mark.transform.rotation = Quaternion.LookRotation(normal) * Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));

        Tween.Alpha(mark, MARK_COLOR.a, 0f, MARK_FADE_DURATION, startDelay: MARK_FADE_DELAY)
            .OnComplete(() => DestroyEffect(mark.gameObject), warnIfTargetDestroyed: false);
    }

    private void SpawnFlash(Vector3 point, Vector3 normal, Transform parent)
    {
        if (_flashSprite == null)
        {
            return;
        }

        if (_viewCamera == null)
        {
            _viewCamera = Camera.main;
        }

        Quaternion rotation = _viewCamera != null ? _viewCamera.transform.rotation : Quaternion.LookRotation(normal);

        SpriteRenderer flash = CreateEffect("HitscanFlash", _flashSprite, FLASH_COLOR, FLASH_START_SIZE, parent);
        flash.transform.position = point + normal * FLASH_SURFACE_OFFSET;
        flash.transform.rotation = rotation;

        Tween scaleTween = Tween.Scale(flash.transform, Vector3.one * FLASH_SIZE, FLASH_LIFETIME, Ease.OutQuad);
        Tween fadeTween = Tween.Alpha(flash, FLASH_COLOR.a, 0f, FLASH_LIFETIME);

        Sequence.Create(scaleTween)
            .Group(fadeTween)
            .OnComplete(() => DestroyEffect(flash.gameObject), warnIfTargetDestroyed: false);
    }

    private static SpriteRenderer CreateEffect(
        string name,
        Sprite sprite,
        Color color,
        float size,
        Transform parent)
    {
        var effect = new GameObject(name);
        effect.transform.localScale = Vector3.one * size;

        var spriteRenderer = effect.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.shadowCastingMode = ShadowCastingMode.Off;
        spriteRenderer.receiveShadows = false;
        spriteRenderer.lightProbeUsage = LightProbeUsage.Off;
        spriteRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        if (parent != null)
        {
            effect.transform.SetParent(parent, true);
        }

        return spriteRenderer;
    }

    private static Sprite CreateSprite(Texture2D texture)
    {
        if (texture == null)
        {
            return null;
        }

        float pixelsPerUnit = Mathf.Max(texture.width, texture.height);

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
    }

    private static void DestroyEffect(GameObject effect)
    {
        DestroyResource(effect);
    }

    private static void DestroyResource(UnityEngine.Object resource)
    {
        if (resource == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            UnityEngine.Object.Destroy(resource);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(resource);
        }
    }
}
