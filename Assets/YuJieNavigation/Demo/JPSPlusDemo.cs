using YuJie.Navigation;
using UnityEngine;

public class JPSPlusDemo : MonoBehaviour
{

    private void Bake(bool[,] obst)
    {
        JPSPlusRunner runner = new JPSPlusRunner(obst);
    }

}
