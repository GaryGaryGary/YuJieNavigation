using YuJie.Navigation;
using UnityEngine;
using UnityEngine.UI;

public class JPSPlusDemo : MonoBehaviour
{
    [SerializeField]
    private Button m_start;

    [SerializeField]
    private JPSPlusBakedMap m_bakedMap;

    private void Awake()
    {
        if (m_start)
            m_start.onClick.AddListener(OnBtnStartClickHandler);
    }

    private void Start()
    {
    }

    private void OnBtnStartClickHandler()
    {
        Debug.Log("开始寻路");
    }

    private void StartNavi(bool[,] obst)
    {
        JPSPlusRunner runner = new JPSPlusRunner(obst);
    }

    private void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            Debug.Log("左键");
        }

        if (Input.GetMouseButtonUp(1))
        {
            Debug.Log("右键");
        }
    }
}
