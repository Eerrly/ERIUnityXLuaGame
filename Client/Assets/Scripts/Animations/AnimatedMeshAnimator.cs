using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimatedMeshAnimator : MonoBehaviour
{
    [SerializeField]
    List<AnimationFrameInfo> FrameInformations;
    [SerializeField]
    MaterialPropertyBlockController PropertyBlockController;
    
    public float NormalizedTime { get; private set; }
    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        _timer = 0;
        _frameCount = 1;
        NormalizedTime = 0;
        IsPlaying = false;
        loop = false;
    }

    public void Setup(List<AnimationFrameInfo> frameInformations, MaterialPropertyBlockController propertyBlockController)
    {
        FrameInformations = frameInformations;
        PropertyBlockController = propertyBlockController;
    }

    public void Play(string animationName, float offsetSeconds, bool loop = true)
    {
        this.loop = loop;
        if (IsPlaying) Stop();

        var frameInformation = FrameInformations.First(x => x.Name == animationName);

        PropertyBlockController.SetFloat("_OffsetSeconds", offsetSeconds);
        PropertyBlockController.SetFloat("_StartFrame", frameInformation.StartFrame);
        PropertyBlockController.SetFloat("_EndFrame", frameInformation.EndFrame);
        PropertyBlockController.SetFloat("_FrameCount", frameInformation.FrameCount);
        PropertyBlockController.Apply();

        IsPlaying = true;
        _frameCount = frameInformation.FrameCount;
    }

    public void Stop()
    {
        PropertyBlockController.SetFloat("_StartFrame", 0);
        PropertyBlockController.SetFloat("_EndFrame", 0);
        PropertyBlockController.SetFloat("_FrameCount", 1);
        PropertyBlockController.Apply();

        IsPlaying = false;
        _timer = 0;
        NormalizedTime = 0;
    }

    private float _timer;
    private int _frameCount;
    private bool loop;
    private void Update()
    {
        if (IsPlaying && !loop)
        {
            if(NormalizedTime < 1)
            {
                NormalizedTime = Mathf.Min(1.0f, (_timer += Time.deltaTime) * BattleConstant.FrameInterval / _frameCount);
            }
            else
            {
                Stop();
            }
        }
    }

}