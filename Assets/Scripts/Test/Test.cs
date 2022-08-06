using UnityEngine;

public class Test : MonoBehaviour
{
    private AnimatedMeshAnimator[] enemyAnimator;

    private void Start()
    {
        enemyAnimator = FindObjectsOfType<AnimatedMeshAnimator>();
        for (int i = 0; i < enemyAnimator.Length; i++)
        {
            enemyAnimator[i].Play("Orc_wolfrider_03_run", 0f);
        }
    }
}