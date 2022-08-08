using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject go;

    private void Start()
    {
        for (int i = 0; i < 1000; i++)
        {
            var resourcePrefabPath = BattleConstant.playerCharacterPath;
            GameObject character = Resources.Load<GameObject>(resourcePrefabPath) as GameObject;
            GameObject go = Instantiate(character, new Vector3(Random.insideUnitCircle.x * 10, 0, Random.insideUnitCircle.y * 10), Quaternion.identity);
            AnimatedMeshAnimator animator = go.GetComponentInChildren<AnimatedMeshAnimator>();
            animator.Play("WK_heavy_infantry_08_attack_B", 0f);
        }
    }
}