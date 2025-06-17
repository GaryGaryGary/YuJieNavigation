using YuJie.Navigation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class JPSPlusDemo : MonoBehaviour
{
    [SerializeField]
    private Button m_btnStart;

    [SerializeField]
    private JPSPlusBakedMap m_bakedMap;

    private GameObject m_start;
    private GameObject m_end;
    private JPSPlusRunner m_runner;


    private void Awake()
    {
        if (m_btnStart)
            m_btnStart.onClick.AddListener(OnBtnStartClickHandler);

        if(m_start == null)
        {
            m_start = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_start.name = "start";
            Material redMaterial = new Material(Shader.Find("Unlit/Color"));
            redMaterial.color = Color.red;
            m_start.GetComponent<Renderer>().material = redMaterial;
        }
        m_start.SetActive(false);
        if (m_end == null)
        {
            m_end = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_end.name = "end";
            Material blueMaterial = new Material(Shader.Find("Unlit/Color"));
            blueMaterial.color = Color.blue;
            m_end.GetComponent<Renderer>().material = blueMaterial;
        }
        m_end.SetActive(false);
    }

    private void Start()
    {
        m_runner = new JPSPlusRunner(m_bakedMap);
    }

    private void OnBtnStartClickHandler()
    {
        if (m_inMove)
            return;
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        StartNavi();
        sw.Stop();
        Debug.Log("navigation cost:" + sw.ElapsedMilliseconds);
    }

    private void StartNavi()
    {
        if (m_start == null || !m_start.activeSelf)
        {
            Debug.LogWarning("起点未设置");
            return;
        }

        if (m_end == null || !m_end.activeSelf)
        {
            Debug.LogWarning("终点未设置");
            return;
        }
        bool succ = m_runner.StepAll();
        if(succ)
        {
            m_pathPoints = m_runner.GetPaths();
            if(m_pathPoints.Count < 2)
            {
                Debug.LogWarning("路径点生成错误");
                return;
            }
            Debug.Log("寻路成功");
            StartMove();
        }
    }

    private void SetStart()
    {
        m_start.transform.position = GetMousePosition();
        if (!m_runner.SetStart(m_start.transform.position))
        {
            m_start.SetActive(false);
            return;
        }

        m_start.SetActive (true);
    }

    private void SetEnd()
    {
        m_end.transform.position = GetMousePosition();
        if (!m_runner.SetTarget(m_end.transform.position))
        {
            m_end.SetActive(false);
            return;
        }

        m_end.SetActive(true);
    }

    private Vector3 GetMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10f;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return new Vector3(worldPosition.x, 0, worldPosition.z);
    }

    private void Update()
    {
        if (m_inMove)
        {
            Move();
            return;
        }
        if(Input.GetMouseButtonUp(0) && !IsPointerOverUI())
        {
            SetStart();
        }

        if (Input.GetMouseButtonUp(1) && !IsPointerOverUI())
        {
            SetEnd();
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    #region 移动
    private bool m_inMove = false;
    private IReadOnlyList<JPSPlusNode> m_pathPoints;
    private int m_pointIndex;
    private JPSPlusNode m_curNode;
    [SerializeField]private float m_speed = 2;

    private void StartMove()
    {
        //去除起点节点,从下一个点开始
        m_pointIndex = 1;
        m_curNode = m_pathPoints[m_pointIndex];
        m_inMove = true;

    }

    private void EndMove()
    {
        m_inMove = false;
        m_end.SetActive(false);
    }

    private void Move()
    {
        float dis = Vector3.Distance(m_start.transform.position, m_curNode.WorldPos);
        if (Math.Abs(dis) < float.Epsilon)
        {
            m_pointIndex++;
            if(m_pointIndex >= m_pathPoints.Count)
            {
                EndMove();
                return;
            }
            m_curNode = m_pathPoints[m_pointIndex];
        }
        m_start.transform.position = Vector3.MoveTowards(m_start.transform.position,
                                               m_curNode.WorldPos,
                                               m_speed * Time.deltaTime);
    }
    #endregion 移动
}
