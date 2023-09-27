using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UltimateMeter : MonoBehaviour
{
    [SerializeField, Header("アルティメットゲージ最大値")]
    private float _maxUltimatePoint = 100f;

    [SerializeField, Header("現在のポイント")]
    private float _ultimatePoint = 0;

    [SerializeField, Range(0.0f, 10.0f), Header("ポイント自然上昇量")]
    private float _increasePoint = 1.0f;

    [SerializeField, Range(1.0f, 2.5f), Header("ポイント自然上昇量間隔")]
    private float _pointIncreasePerSec = 1.0f;

    [SerializeField, Header("ポイント回復可能状態か")]
    private bool _canPointIncrease = false;

    private bool _isPointIncrease = false;

    private bool _isWaiting = false;

    private bool _isCalledOnce = false;

    [SerializeField, Header("メーター")]
    Image meter = default;

    Unit selfUnit = default;    //自身のunit

    [SerializeField, Header("GameController")]
    private GameController GC;

    [SerializeField, Header("MainUIのAnimator")]
    private Animator UIanimator;


    public int intWeaponType = default;

    [SerializeField, Header("各武器の命中時加算ポイント")]
    private int[] weaponsIncreasePoints = default;

    [SerializeField, Header("敵の撃破時加算ポイント")]
    private int　eKIAIncreasePoint = default;

    [SerializeField, Header("各基地の破壊時加算ポイント")]
    private int[] destroyBasesIncreasePoints = default;

    [SerializeField, Header("アイテム取得時加算ポイント")]
    private int[] getItemIncreasePoints = default;

    [SerializeField, Range(0.00f, 1.00f), Header("死亡時のペナルティレート")]
    private float PointPenaltyRates = 0.75f;

    [SerializeField, Header("発動後ポイント加算不可能時間")]
    private int[] pointIncreaseInterval = default;

    [SerializeField]
    private Sprite defaultIcon = default;

    [SerializeField]
    private Sprite ult1Icon = default;

    [SerializeField]
    private Sprite ult2Icon = default;

    [SerializeField]
    private Image ultIcon = default;

    private bool isMeterNeedsReset = false;

    private float _duration = 1;

    private AudioSource audio = default;


    [SerializeField, Header("メーター最大時SE")]
    private AudioClip _soundMax = default;

    private void Start()
    {
        _isCalledOnce = false;

        audio = this.GetComponent<AudioSource>();

        selfUnit = this.GetComponent<Unit>();
    }

    private void OnDisable()
    {
        //死んだら所定の割合減点
        _ultimatePoint = Mathf.Floor(_ultimatePoint * PointPenaltyRates);

        //プレイヤーだったら
        if (this.name == "Player" || this.name == "Player2")
        {
            UIanimator.SetBool("ULTmeter_Idle", false);
        }
    }

    private void OnEnable()
    {
        _isCalledOnce = false;
        //プレイヤーだったら
        if (this.name == "Player" || this.name == "Player2")
        {
            UIanimator.SetBool("ULTmeter_MAX", false);
            UIanimator.SetTrigger("ULTmeter_Reset");
            //メーターUIに数値を表示
            meter.fillAmount = _ultimatePoint / _maxUltimatePoint;
        }
    }
    private void Update()
    {
        //ゲーム中でないなら
        if (!GC._inGame())
        {
            return;
        }

        //プレイヤーだったら
        if (this.name == "Player" || this.name == "Player2")
        {
            //メーターUIに数値を表示
            meter.fillAmount = _ultimatePoint / _maxUltimatePoint;
        }

        //現在のポイントが最大値未満なら
        if (_ultimatePoint < _maxUltimatePoint)
        {
            //ポイント加算中ではない かつポイント加算不可能なら
            if (!_isPointIncrease && !_canPointIncrease &&!_isWaiting)
            {
                //加算可能フラグをtrueに
                _canPointIncrease = true;

                //ポイント加算
                StartCoroutine(PointIncrease());

                //加算中フラグをtrueに
                _isPointIncrease = true;

            }
        }
        else
        {
            //プレイヤーだったら
            if (this.name == "Player" || this.name == "Player2")
            {
                if (!_isCalledOnce)
                {
                    UIanimator.SetBool("ULTmeter_MAX", true);

                    selfUnit.UltimateReadyToUse();
                    if (selfUnit.isUlt1Activated)
                    {
                        ultIcon.sprite = ult1Icon;
                    }
                    else
                    {
                        ultIcon.sprite = ult2Icon;
                    }



                    _isCalledOnce = true;
                }
            }

                _ultimatePoint = _maxUltimatePoint;
        }
    }

    private void FixedUpdate()
    {


        //アルティメットポイントが回復可能状態なら
        if (_canPointIncrease)
        {
            //デバッグモード判定
            if (!selfUnit.debugMode)
            {
                if (!_isPointIncrease)
                {
                    //ポイント加算
                    StartCoroutine(PointIncrease());
                    _isPointIncrease = true;
                }
                
            }
            else
            {
                //ポイントを常に最大に
                _ultimatePoint = _maxUltimatePoint;
                selfUnit.UltimateReadyToUse();
                _canPointIncrease = false;
            }

        }
    }

    private void WaitingPointIncreaseInterbal()
    {
        _isWaiting = false;
           _canPointIncrease = true;
    }

    IEnumerator PointIncrease()
    {
        while (true)
        {
            if (_ultimatePoint >= _maxUltimatePoint)
            {
                //ポイントを最大値にし、加算不可能にする。
                SetPointToLimit();
                break;
            }

            //ポイントを加算
            _ultimatePoint = _ultimatePoint + _increasePoint;
            

            yield return new WaitForSeconds(_pointIncreasePerSec);
        }
    }

    /// <summary>
    /// アルティメットを使用した場合
    /// </summary>
    public void UltimateUsed(int typeOfUltimate)
    {
        //プレイヤーだったら
        if (this.name == "Player" || this.name == "Player2")
        {
            UIanimator.SetTrigger("ULTmeter_Fired");
            UIanimator.SetBool("ULTmeter_MAX", false);
        }

        _isWaiting = true;

        _isCalledOnce = false;

        _ultimatePoint = 0;

        ultIcon.sprite = defaultIcon;

        Invoke(nameof(WaitingPointIncreaseInterbal), pointIncreaseInterval[typeOfUltimate]);
    }

    public void AddPointByDamageToEnemy(int setWeaponType)
    {
        if(_canPointIncrease)
        {
            if (_ultimatePoint >= _maxUltimatePoint)
            {
                //ポイントを最大値にし、加算不可能にする。
                SetPointToLimit();
            }
            _ultimatePoint = _ultimatePoint + weaponsIncreasePoints[setWeaponType];
        }
        
    }

    /// <summary>
    /// <para>AddPointByDestroyEnemy</para>
    /// <para>敵撃破時のポイント加算</para>
    /// </summary>
    public void AddPointByDestroyEnemy()
    {
        if (_canPointIncrease)
        {
            _ultimatePoint = _ultimatePoint + eKIAIncreasePoint;
            if (_ultimatePoint >= _maxUltimatePoint)
            {
                //ポイントを最大値にし、加算不可能にする。
                SetPointToLimit();
            }
        }
    }

    /// <summary>
    /// <para>AddPointByDestroyEnemyBase</para>
    /// <para>拠点破壊時のポイント加算</para>
    /// </summary>
    /// <param name="setBaseType">拠点のタイプ SMALL = 0, MIDDLE = 1, BIG = 2</param>
    public void AddPointByDestroyEnemyBase(int setBaseType)
    {
        if (_canPointIncrease)
        {
            _ultimatePoint = _ultimatePoint + destroyBasesIncreasePoints[setBaseType];
        }
    }

    /// <summary>
    /// <para>GetULTItem</para>
    /// <para>アイテムゲット時のポイント加算</para>
    /// </summary>
    /// <param name="itemType">アイテムのタイプ SMALL = 0, BIG = 1</param>
    public void GetULTItem(int itemType)
    {
        if (_canPointIncrease)
        {
            _ultimatePoint = _ultimatePoint + getItemIncreasePoints[itemType];
            if (_ultimatePoint >= _maxUltimatePoint)
            {
                //ポイントを最大値にし、加算不可能にする。
                SetPointToLimit();
            }
        }
    }

    private void SetPointToLimit()
    {
        //音を鳴らす処理
        audio.PlayOneShot(_soundMax);


        //ポイントを最大値にし、加算不可能にする。
        _ultimatePoint = _maxUltimatePoint;
        _canPointIncrease = false;
        _isPointIncrease = false;
    }

    /// <summary>
    /// <para>GameStart</para>
    /// <para>マッチ開始時に自然回復を起動する</para>
    /// </summary>
    public void GameStart()
    {
        _canPointIncrease = true;
    }
}

