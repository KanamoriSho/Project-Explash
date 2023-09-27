using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*---------------------------------------------------------------------------*/
/// 製作者 : 金盛 翔
/// 概要　 : マルチロックオンミサイル挙動(アルティメット1)
/// 進捗　 : 完成(後々他スクリプトとのすり合わせで調整の可能性アリ)
/*---------------------------------------------------------------------------*/

public class Weapon_MultiLockOn : Weapons
{

    /**
     * マルチロックオンミサイルのスクリプト
     * 基本的に"各種パラメータ"の中に格納されている変数をInspectorで変えるだけで動くようになっています。
     * なので基本的にスクリプト側は弄らなくて大丈夫です。
     * 
     * <各種説明>
     * ・TimeTodisappear    : 自爆するまでの時間です。　これを弄ればミサイルのしつこさと射程距離を変えられます。
     * ・MissileDerayTime   : 生成から加速開始までのディレイを調整できます。 数値次第で魚雷や戦闘機のミサイルのような挙動にできます。
     * ・HomingDerayTime    : 生成から追尾開始までのディレイを調整できます。 数値が大きいほど近距離に弱くなります。
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

    [SerializeField, Range(0.0f, 3.0f), Header("上空へのワープまでの時間 (デフォルト:0.5)")]
    private float _warpToTargetsAbove = default;

    [SerializeField, Range(30.0f, 200.0f), Header("上空ワープ地点高さ (デフォルト:100)")]
    private float _warpPointHeight = default;

    [SerializeField, Range(0.0f, 1.0f), Header("ワープから再加速までのディレイ")]
    private float _missileWarpedDerayTime = default;


    [SerializeField, Range(0, 100), Header("推進力 (デフォルト:20)")]
    private float MissilePower = default;


    [SerializeField, Range(10, 50), Header("最高速 (デフォルト:30)")]
    private float MissileMaxSpeed = default;


    [SerializeField, Range(10, 50), Header("最低速度 (デフォルト:30)")]
    private float MissileMinSpeed = default;

    [SerializeField, Range(0, 5), Header("抵抗 (デフォルト:1)")]
    private float RigidBodyDrag = default;

    #endregion

    //速度
    private Vector3 velocity;
    //発射するときの初期位置
    private Vector3 position;

    private Vector3 localVelocity;

    private Vector3 localTargetPos;

    private float targetDistance;

    private float preTargetDistance = 0;

    private bool isWarped = false;

    private bool isWarping = false;

    private bool _isCalledOnce = false;

    private bool _isBoomed = false;

    private Transform _targetTransform = default;

    private Transform _targetPos = default;

    [SerializeField, Range(0, 100), Header("ターゲットロストまでの時間")]
    private float targetMissLimit = 0;

    private float targetMissTime = 0;

    [SerializeField, Header("爆発範囲表示オブジェクト")]
    private GameObject explosionAreaMarker = default;

    [SerializeField]
    private Animator explosionAreaMarkerAnim = default;

    [SerializeField, Header("爆発範囲オブジェクト")]
    private GameObject explosionArea = default;

    [SerializeField, Header("爆発範囲の子オブジェクト")]
    private GameObject[] explosionObjects = default;

    [SerializeField, Header("爆発中心部オブジェクト")]
    private GameObject explosionCenter = default;

    [SerializeField, Header("爆発外周部オブジェクト")]
    private GameObject explosionOutside = default;

    // 爆発中心部のWeapons
    private Weapons weaponsScriptCenter = default;

    // 爆発外周部のWeapons
    private Weapons weaponsScriptOutside = default;

    protected override void OnEnable()
    {
        _isBoomed = false;

        explosionArea.SetActive(false);

        explosionAreaMarker.SetActive(false);

        //爆発オブジェクトのWeaponsを取得
        weaponsScriptCenter = explosionCenter.GetComponent<Weapons>();
        weaponsScriptOutside = explosionOutside.GetComponent<Weapons>();

        _isCalledOnce = false;

        base.OnEnable();
    }

    private void LateUpdate()
    {
        //目標が追尾中にnullになった or 目標のタグがObstacleだったら
        if (target && !target.activeSelf | target && target.tag == "Obstacle")
        {
            //目標をnullに
            target = null;
        }

        if (this.transform.position.y <= 0 || !target.activeSelf)
        {
            Kaboom();
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
            if (!isWarping)
            {
                Acceleration();
            }

            if (time > _warpToTargetsAbove && time < _warpToTargetsAbove + _missileWarpedDerayTime)
            {
                //ワープしてから再発射迄に速度を初期化、角度を設定する。

                explosionAreaMarker.SetActive(true);

                explosionAreaMarker.gameObject.transform.parent = null;

                explosionAreaMarker.transform.position = target.transform.position;

                explosionAreaMarker.transform.rotation = new Quaternion(0, 0, 0, 0);

                //ワープ中フラグtrue
                isWarping = true;
                //速度初期化
                RB.velocity = new Vector3(0, 0, 0);
                //座標を格納
                transform.position = new Vector3(target.transform.position.x, _warpPointHeight, target.transform.position.z);
                //ターゲットの方向を向かせる
                transform.rotation = Quaternion.LookRotation(target.transform.position - this.transform.position);

            }
            else if (time > _warpToTargetsAbove + _missileWarpedDerayTime)
            {
                //再発射

                //ワープ中フラグfalse
                isWarping = false;
            }
        }
        else   //自爆
        {
            if (!_isCalledOnce)
            {
                Kaboom();
                _isCalledOnce = true;
            }
            
        }


    }


    protected override void Kaboom()
    {
        if(_isBoomed)
        {
            return;
        }

        _isBoomed = true;

        //各種パラメータ初期化
        targetMissTime = 0;
        preTargetDistance = 0;
        isWarped = false;

        explosionArea.SetActive(true);

        weaponsScriptCenter.SetShooter(GetShooter(), GetShooterNum(), this.gameObject.tag);
        weaponsScriptOutside.SetShooter(GetShooter(), GetShooterNum(), this.gameObject.tag);

        explosionAreaMarker.gameObject.transform.parent = this.gameObject.transform;


        Invoke(nameof(Wait), 0.1f);
    }


    protected override void Acceleration()
    {
        RB.AddRelativeForce(0, 0, MissilePower);


        localVelocity = transform.InverseTransformDirection(RB.velocity);

        localVelocity.x *= 0.9f;

        RB.velocity = transform.TransformDirection(localVelocity);

        //最高速リミッター
        if (RB.velocity.magnitude >= MissileMaxSpeed)
        {
            RB.velocity = RB.velocity.normalized * MissileMaxSpeed;
        }

        //最低速リミッター
        if (RB.velocity.magnitude <= MissileMinSpeed)
        {
            RB.velocity = RB.velocity.normalized * MissileMinSpeed;
        }
    }

    private void Wait()
    {
        base.Kaboom();
    }
}
