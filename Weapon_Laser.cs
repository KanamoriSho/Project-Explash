using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/*---------------------------------------------------------------------------*/
/// 製作者 : 金盛 翔
/// 概要　 :レーザー(アルティメット2)
/// 進捗　 : 完成(後々他スクリプトとのすり合わせで調整の可能性アリ)
/*---------------------------------------------------------------------------*/

public class Weapon_Laser : Weapons
{

    [SerializeField, Header("SE")]
    private AudioClip _SE = default;

    private AudioSource _audio = default;

    [SerializeField, Range(0.00f, 0.50f), Header("攻撃判定の出る間隔")]
    private float _intervalOfAttackArea = 0.25f;

    private int _countOfAttackAreaEnabled = 0;                    //攻撃判定の発生回数カウント用

    private int _maxCountOfAttackAreaEnabled = default;           //攻撃判定の最大発生回数

    [SerializeField, Range(0.0f, 5.0f), Header("攻撃判定の出るまでのディレイ")]
    private float _delayOfAttackArea = 1.0f;

    [SerializeField, Header("攻撃判定の出るまでのディレイ計測用フラグ")]
    private bool _isTimerOverDelayOfAttackArea = false;           //攻撃判定の出るまでのディレイ計測用フラグ

    [SerializeField, Range(0.0f, 5.0f), Header("攻撃判定の出ている時間")]
    private float _timeOfAttackArea = 3.0f;

    [SerializeField, Header("攻撃判定の出ている時間計測用フラグ")]
    private bool _isTimerOverAttackAreaTime = false;              //攻撃判定の出ている時間計測用フラグ

    [SerializeField, Range(0.0f, 5.0f), Header("プレイヤーが移動可能になるまでの時間")]
    private float _timeOfinactionable = 2.0f;

    [SerializeField, Header("プレイヤーが移動可能になるまでの時間計測用フラグ")]
    private bool _isTimerOverOfinactionableTime = false;          //プレイヤーが移動可能になるまでの時間計測用フラグ

    [SerializeField, Header("プレイヤー(個々人)")]
    private GameObject _player = default;

    private Transform _playerTransform = default;           //プレイヤーのTransform

    private Unit unit = default;                            //プレイヤーのUnitコンポーネント

    private Rigidbody _playerRb = default;                  //プレイヤーのRigidBody

    [SerializeField, Range(0, 100), Header("対拠点ダメージの減衰レート(%)")]
    private int _attenuationRateOfAnti_BaseDamage = 30;

    [SerializeField, Header("発射地点のプレイヤーとの相対座標")]
    private Vector3 _ofset = default;                       

    [SerializeField, Header("SphreCastの半径")]
    private float _radius = 20;

    [SerializeField, Header("SphreCastの照射距離")]
    private float _direction = 250;

    [SerializeField]
    private GameObject _dangerZone = default;

    [SerializeField]
    private GameObject _laserTube = default;

    [SerializeField]
    private Material _dangerAreaColor = default;


    [SerializeField]
    List<GameObject> enemiesInLaser = default;

    private GameObject _child = default;

    [SerializeField]
    private LayerMask _enemyLayer = default;



    /* 継承元のWeaponsにあるけど通したくない処理達をreturn;で上書き
     * 
     * Acceleration()       : 加速処理
     * FixedUpdate()        : 追尾や加速などの物理挙動系
     * Kaboom()             : 自爆処理
     */
    #region 通さない処理達

    protected override void Start()
    {
        return;
    }

    protected override void Acceleration()
    {
        return;
    }
    protected override void FixedUpdate()
    {
        return;
    }

    protected override void Kaboom()
    {
        return;
    }

    #endregion

    protected override void Awake()
    {
        _player = transform.root.gameObject;

        _playerRb = _player.GetComponent<Rigidbody>();                                      //プレイヤーのRigidBody取得

        _child = this.gameObject.transform.GetChild(0).gameObject;                          //子オブジェクトを取得

        _audio = _player.GetComponent<AudioSource>();                                       //プレイヤーのAudioSourceを取得

        _maxCountOfAttackAreaEnabled = (int)(_timeOfAttackArea / _intervalOfAttackArea);    //攻撃判定の最大発生回数を定義
    }

    protected override void OnEnable()
    {

        //フラグと変数の初期化

        this.gameObject.transform.parent = _player.transform;                               //親オブジェクトをプレイヤーに指定

        this.gameObject.transform.position = _player.transform.position;                    //位置をプレイヤーの座標に修正

        this.gameObject.transform.rotation = _player.transform.rotation;                    //向きをプレイヤーの角度に修正

        time = 0;                                                    //タイマーをリセット

        _countOfAttackAreaEnabled = 0;                               //攻撃判定の発生回数カウントをリセット

        _isTimerOverDelayOfAttackArea = false;                       //攻撃判定の出るまでのディレイの時間計測用フラグ

        _isTimerOverAttackAreaTime = false;                          //攻撃判定の出ている時間計測用フラグ

        _isTimerOverOfinactionableTime = false;                      //プレイヤーが行動可能になるまでの時間計測用フラグ

        _audio.PlayOneShot(_SE);
    }

    private void OnDisable()
    {

        //フラグと変数の初期化

        time = 0;                                                    //タイマーをリセット

        _countOfAttackAreaEnabled = 0;                               //攻撃判定の発生回数カウントをリセット

        _isTimerOverDelayOfAttackArea = false;                       //攻撃判定の出るまでのディレイの時間計測用フラグ

        _isTimerOverAttackAreaTime = false;                          //攻撃判定の出ている時間計測用フラグ

        _isTimerOverOfinactionableTime = false;                      //プレイヤーが公道可能になるまでの時間計測用フラグ

        if(gameObject.transform.parent == _player.transform)
        {
            this.gameObject.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if(_isTimerOverOfinactionableTime)                          //移動不可能状態なら
        {
            _playerRb.velocity = new Vector3(0, 0, 0);              //速度を0で上書き
        }

        //ディレイ待機終了フラグがfalseなら
        if (!_isTimerOverDelayOfAttackArea)
        {
            _isTimerOverOfinactionableTime = true;                  //移動不可能フラグをtrueに

            _dangerZone.SetActive(true);                             //レーザーの見た目をtrueに

            //タイマーが発射前ディレイを超えていたら
            if (time >= _delayOfAttackArea)
            {
                _isTimerOverDelayOfAttackArea = true;               //ディレイ待機終了フラグをtrueに
                time = 0;                                           //タイマーをリセット
            }
        }
        //発射前ディレイが終了していたら
        else
        {

            this.gameObject.transform.parent = null;                //レーザーをプレイヤーから分離
                _dangerZone.SetActive(false);                         //レーザー(見た目)を消す

            _laserTube.SetActive(true);                         //レーザーの色を変更

            enemiesInLaser =                                        //SphereCastAllで範囲内の敵を全て取得
                    Physics.SphereCastAll
                    (this.transform.position + _ofset, _radius, this.transform.forward,　_direction,
                                _enemyLayer).Select(hits => hits.collider.gameObject).ToList();
            
            if(time > _timeOfinactionable - _delayOfAttackArea)
            {
                _isTimerOverOfinactionableTime = false;             //移動不可能フラグをfalseに
            }


            if(time > _intervalOfAttackArea && !_isTimerOverAttackAreaTime)
            {
                DamagedAreaOccurrence();                            //範囲内の敵にダメージを与える
                time = 0;                                           //タイマーをリセット
            }

            if(_isTimerOverAttackAreaTime)
            {
                _laserTube.SetActive(false);
                this.gameObject.transform.parent = _player.transform;
                this.gameObject.SetActive(false);
            }
        }
    }

    private void DamagedAreaOccurrence()
    {
        //リスト内の全ての敵性オブジェクトにダメージを与える
        for (int lengthOfEnemiesInLaser = 0; lengthOfEnemiesInLaser < enemiesInLaser.Count; lengthOfEnemiesInLaser++)
        {

            //対象オブジェクトにUnitがあるか
            if (enemiesInLaser[lengthOfEnemiesInLaser].TryGetComponent(out Unit unit))
            {
                unit.Damage(damage, _child);            //UnitがあるならUnitのダメージ処理を起動
            }
            else
            {
                int fixedDamage = damage / _attenuationRateOfAnti_BaseDamage;

                //無ければCastle_HPにあるダメージ処理を起動
                enemiesInLaser[lengthOfEnemiesInLaser].GetComponent<Castle_HP>().
                                        Damage(_shooterNum, _shooter, fixedDamage, this.gameObject);
            }

        }

            Debug.Log("だめーじ");

        _countOfAttackAreaEnabled++;                            //攻撃判定の発生回数を加算

        //攻撃回数の最大数に達したら
        if (_countOfAttackAreaEnabled < _maxCountOfAttackAreaEnabled)
        {
            _isTimerOverAttackAreaTime = false;
        }
        else
        {
            _isTimerOverAttackAreaTime = true;
        }
    }

    public void SetDamage(int loc_damage)
    {
        damage = loc_damage;
    }
}
