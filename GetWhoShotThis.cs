using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetWhoShotThis : MonoBehaviour
{

    [SerializeField]  //これを発射したプレイヤーを入れる
    private GameObject pWST;
    //上のプライベート変数
    public GameObject peasonWhoShotThis;

    void Start()
    {
        //撃ったプレイヤーを入れる(ダメージ判定とかキルカメラ用)
        pWST = peasonWhoShotThis;
    }

    private void OnEnable()
    {
        //撃ったプレイヤーを入れる(ダメージ判定とかキルカメラ用)
        pWST = peasonWhoShotThis;
    }

    void Update()
    {
        
    }
}
