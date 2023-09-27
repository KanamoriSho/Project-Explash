using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/*--------------------------------------*/
//  制作者: かなもり
//  概要: 建物の破壊の起動・無効化
//  進捗: 仕様変更中
/*--------------------------------------*/

public class Destory_Bases : MonoBehaviour
{
    [Header("分割してないオブジェクト"), SerializeField]
    private GameObject[] BilldingParts;

    [Header("分割後のオブジェクトの親(○○_root)"), SerializeField]
    private GameObject[] DestroyBilldingParts_root;


    [Header("Rayfire Rigid(分割後のオブジェクトを入れる)"), SerializeField]
    private RayFire.RayfireRigid[] RfRbs;

    [Header("カメラ"), SerializeField]
    private GameObject explosionCam;

    [Header("通常カメラ"), SerializeField]
    private GameObject cam;


    private GameObject BaseObject;


    private enum TypeOfBases
    {
        MainHead,
        Main,
        Other
    }

    [SerializeField]
    private TypeOfBases _type = default;

    private int count = 0;

    [SerializeField]
    private VibrationController _vC = default;

    //このメソッドを呼び出すと崩れる。エフェクトもここで呼び出したり??
    public void Collapse()
    {
        if (_type == TypeOfBases.Main)
        {
            _vC.MainBaseDestroy();
            _vC.MainBaseDestroy2();
            cam.SetActive(false);
            explosionCam.SetActive(true);
        }

        for (count = 0; count < BilldingParts.Length; count++)
        {
            //非分割オブジェクトを無効化
            BilldingParts[count].SetActive(false);
            //分割オブジェクトを有効化
            DestroyBilldingParts_root[count].SetActive(true);
            //崩す
            RfRbs[count].Demolish();
        }
    }
    //このメソッドを呼び出すと崩れる。エフェクトもここで呼び出したり??
    public void Collapse2()
    {
        for (count = 0; count < BilldingParts.Length; count++)
        {
            //非分割オブジェクトを無効化
            BilldingParts[count].SetActive(false);
            //場所を分割前オブジェクトの場所・角度に
            DestroyBilldingParts_root[count].transform.position = BilldingParts[count].transform.position;

            DestroyBilldingParts_root[count].transform.rotation = BilldingParts[count].transform.rotation;
            //分割オブジェクトを有効化
            DestroyBilldingParts_root[count].SetActive(true);
            //崩す
            RfRbs[count].Demolish();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (_type)
        {
            case TypeOfBases.MainHead:

                if (collision.gameObject.tag == ("Ground"))
                {
                    Collapse2();
                }

                break;

            case TypeOfBases.Other:

                break;
        }
    }

}
