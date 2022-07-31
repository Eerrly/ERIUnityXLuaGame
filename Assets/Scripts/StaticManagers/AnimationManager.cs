using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager
{

    private static Dictionary<string, int> _animNameHashDic = new Dictionary<string, int>();

    public static void CrossFadeInFixedTime(
        Animator animator, 
        string animation, 
        float fixedTransitionDuration, 
        int layer, 
        float fixedTimeOffset, 
        float normalizedTransitionTime)
    {
        if (animator && !string.IsNullOrEmpty(animation))
        {
            int animHash;
            if(!_animNameHashDic.TryGetValue(animation, out animHash))
            {
                animHash = Animator.StringToHash(animation);
                _animNameHashDic.Add(animation, animHash);
            }
            animator.Update(0);
            animator.CrossFadeInFixedTime(animHash, fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
        }
    }

    public static void CrossFadeInFixedTime(
        Animator animator,
        string animation,
        float fixedTransitionDuration) 
    {
        CrossFadeInFixedTime(animator, animation, fixedTransitionDuration, -1, 0.0f, 0.0f);
    }

}
