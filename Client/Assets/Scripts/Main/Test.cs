using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Threading;

public class Test : MonoBehaviour
{
    [SerializeField] Transform m_trans = null;
    [SerializeField] float m_nV0 = 10;
    [SerializeField] Vector3 m_v3Dir = Vector3.zero;

    [SerializeField] float m_nGForce = -9;  // 重力加速度
    [SerializeField] float m_nBackForce = -9;   // 前进阻力减速度

    private float m_nV0z = 0;
    private float m_nV0y = 0;

    private float m_nLastZ = 0;
    private bool m_isPlay = false;
    private float m_nTime = 0;

    [SerializeField]
    LineRenderer render;
    [SerializeField]
    GameObject sphere;

    private int i = 0;

    private void Start()
    {
    }

    private void ReadyData()
    {
        // 向量斜边长
        Vector3 v3Dir = m_v3Dir.normalized;
        float z = Mathf.Abs(v3Dir.z);
        float y = Mathf.Abs(v3Dir.y);
        float nCrossLen = Mathf.Sqrt(z * z + y * y);

        // 初速度分量;
        m_nV0y = y * m_nV0 / nCrossLen;
        m_nV0z = z * m_nV0 / nCrossLen;
    }

    private Vector3 MathPos(float nTime)
    {
        Vector3 result = Vector3.zero;
        result.y = m_nV0y * nTime + (m_nGForce * nTime * nTime) * 0.5f;
        float nZPos = m_nV0z * nTime + (m_nBackForce * nTime * nTime) * 0.5f;
        if (nZPos < m_nLastZ)
        {
            nZPos = m_nLastZ;
        }
        m_nLastZ = result.z = nZPos;
        return result;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ReadyData();
            m_isPlay = true;
            m_nTime = 0;
            m_nLastZ = 0;
            m_trans.localPosition = Vector3.zero;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            m_isPlay = false;
        }
        if (m_isPlay)
        {
            m_trans.localPosition = MathPos(m_nTime);
            m_nTime += Time.deltaTime;
        }
    }


}
