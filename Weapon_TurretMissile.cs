using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*---------------------------------------------------------------------------*/
/// 製作者 : 金盛 翔
/// 概要　 : ミサイル挙動
/// 進捗　 : 完成(後々他スクリプトとのすり合わせで調整の可能性アリ)
/*---------------------------------------------------------------------------*/

public class Weapon_TurretMissile : Weapons
{

    /**
     * 誘導・無誘導ミサイルのスクリプトですわ。
     * 基本的に"各種パラメータ"の中に格納されている変数をInspectorで変えるだけで動くようになっています。
     * なので基本的にスクリプト側は弄らなく大丈夫です。
     * 
     * <各種説明>
     * ・TimeTodisappear    : 自爆するまでの時間です。　これを弄ればミサイルのしつこさと射程距離を変えられます。
     * ・MissileDerayTime   : 生成から加速開始までのディレイを調整できます。 数値次第で魚雷や戦闘機のミサイルのような挙動にできます。
     * ・HomingDerayTime    : 生成から追尾開始までのディレイを調整できます。 数値が大きいほど近距離に弱くなります。
     * ・HomingCorrectDeray : ターゲットの方向を向く間隔を変えられます。　数値が大きいほど精度が上がります。
     * ・MissileTime        : 生成からの経過時間です。　この時間の一秒前に加速終了、一秒後に自爆します。
     * ・MissilePower       : ミサイルの加速力です。　Power is Power
     * ・MissileMaxSpeed    : ミサイルの最高速です。　上げるほど速くなります。その分精度も落ちるので他パラメータも調整必須です。
     * ・RigidBodyDrag      : RigidbodyのDragを調整できます。 値が高いほど小回りは効きますが遅く、というか動きが重くなります。
    **/ 

    //これ本体
    private GameObject TurretMissile;
    //ミサイルのリジットボディ
    #region 各種パラメータ
    /// <summary>
    /// ターゲットをセットする
    /// Target => 発射したキャラクターのスクリプトからターゲットを取得する
    /// target => public変数Targetからターゲットを取得する
    /// </summary>
    
    [Header("\n ここからミサイルのパラメータ \n")]

    
    [SerializeField, Range(0.0f, 1.0f), Header("発射までの時間 (デフォルト:0)")]
    private float _missileDerayTime = default;

    
    [SerializeField, Range(0.0f, 3.0f), Header("追尾開始までの時間 (デフォルト:0.5)")]
    private float _homingDerayTime = default;

    
    [SerializeField, Range(0.0f, 90.0f), Header("追尾用角度補間 (デフォルト:2.0f)")]
    private float _homingCorrectDeray = default;

    [SerializeField, Range(0, 100), Header("推進力 (デフォルト:20)")]
    private float _missilePower = default;

    [SerializeField, Range(10, 50), Header("最高速 (デフォルト:30)")]
    private float _missileMaxSpeed = default;

    [SerializeField, Range(10, 50), Header("最低速度 (デフォルト:30)")]
    private float _missileMinSpeed = default;

    [SerializeField, Range(0, 5), Header("抵抗 (デフォルト:1)")]
    private float _rigidBodyDrag = default;

    #endregion

    //速度
    private Vector3 _velocity;
    //発射するときの初期位置
    private Vector3 position;

    //ローカル速度
    private Vector3 localVelocity;

    //目標との距離
    private float targetDistance;

    //直前の目標との距離
    private float preTargetDistance = 0;

    [Header("ターゲットロストまでの時間")]
    [SerializeField,Range(0, 100)]
    private float targetMissLimit = 0;

    private float targetMissTime = 0;


    private void LateUpdate()
    {
        //目標が追尾中にnullになった or 目標のタグがObstacleだったら
        if (target && !target.activeSelf | target && target.tag == "Obstacle")
        {
            //目標をnullに
            target = null;
        }

        //目標がnullでないとき
        if (target)
        {
            //目標との距離を計測
            targetDistance = Vector3.Distance(this.transform.position, target.transform.position);
        }
        else
        {
            //以前の目標との距離を0に
            preTargetDistance = 0;
        }

        //追尾中、かつ目標との距離が離れていっている場合
        if (preTargetDistance != 0 && targetDistance > preTargetDistance && time >= _homingDerayTime)
        {
            //見失うまでのタイマーを加算
            targetMissTime += Time.deltaTime;

            //タイマーが一定の数値を超えたら
            if(targetMissTime >= targetMissLimit)
            {
                //目標をnullに
                target = null;
            }
        }
        else if (target)
        {
            targetMissTime = 0;
            preTargetDistance = targetDistance;
        }
    }
    protected override void FixedUpdate()
    {
        if (time < timeToKaboom)
        {
            ///<summary>
            ///発射(生成)から加速までのインターバル以降 ～ 自爆一秒前か
            /// ここを通る間加速する
            /// </summary>
            Acceleration();

            //追尾までのディレイを超えたら
            if (time > _homingDerayTime)
            {
                //旋回・追尾開始
                Rotation();
            }
        }
        else if (time > 0.1f)   //自爆
        {
            Kaboom();
        }

        
    }


    protected override void Kaboom()
    {
        //各種パラメータ初期化
        targetMissTime = 0;
        preTargetDistance = 0;

        //weaponsの同名メソッド呼び出し
        base.Kaboom();
    }


    protected override void Acceleration()
    {
        //前進する力を与え続ける(ローカルZ軸)
        RB.AddRelativeForce(0, 0, _missilePower);
        
        //localVelocityを格納する
        localVelocity = transform.InverseTransformDirection(RB.velocity);

        //常に0.9倍する(加速終了時の減速用)
        localVelocity.x *= 0.9f;

        //計算したlocalVelocityを戻す
        RB.velocity = transform.TransformDirection(localVelocity);

        //最高速リミッター
        if (RB.velocity.magnitude >= _missileMaxSpeed)
        {
            //現在の速度が最高速を超えていたら現在の速度を最高速に設定
            RB.velocity = RB.velocity.normalized * _missileMaxSpeed;
        }

        //最低速リミッター
        if (RB.velocity.magnitude <= _missileMinSpeed)
        {
            //現在の速度が最低速を下回っていたら現在の速度を最低速に設定
            RB.velocity = RB.velocity.normalized * _missileMinSpeed;
        }
    }

    /// <summary>
    /// 旋回用メソッド
    /// 
    /// Sleapを使用して目標の方向を向かせる
    /// </summary>
    private void Rotation()
    {
        if (target != null)
        {
            //自身～目標間の方向ベクトル取得
            Vector3 relativePos = target.transform.position - this.transform.position;

            // 方向を回転情報に変換
            Quaternion rotation = Quaternion.LookRotation(relativePos);
            // 現在の回転情報と、ターゲット方向の回転情報を補完する
            transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, _homingCorrectDeray * Time.deltaTime);

        }
    }


}
