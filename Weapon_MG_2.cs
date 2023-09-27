using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*---------------------------------------------------------------------------*/
/// 製作者 : 金盛 翔
/// 概要　 : 機銃挙動その2
/// 進捗　 : 完成
/*---------------------------------------------------------------------------*/

public class Weapon_MG_2 : Weapons
{
    /// <summary>
    /// ﾍﾋﾞｰﾏｼﾝｶﾞﾝ! のスクリプトです。
    /// 基本的に"各種パラメータ"の中に格納されている変数をInspectorで変えるだけで動くようになってます。
    /// なので基本的にスクリプト側は弄らなくてよろしくってよ。
    /// </summary>



    //これ本体
    private GameObject MGBullet;

    #region 各種パラメータ

    [Header("速度")]
    [SerializeField]
    [Range(0, 1000)]
    private float bulletPower = 100;
    #endregion

    //速度
    private Vector3 velocity;
    //発射するときの初期位置
    private Vector3 position;

    protected override void Acceleration()
    {
        //消滅まで
        if (time <= timeToKaboom)
        {
            RB.velocity = transform.forward * bulletPower;          //加速処理
        }
        else        //消滅
        {
            Kaboom();
        }
    }


}
