using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UltimateMeter : MonoBehaviour
{
    [SerializeField, Header("�A���e�B���b�g�Q�[�W�ő�l")]
    private float _maxUltimatePoint = 100f;

    [SerializeField, Header("���݂̃|�C���g")]
    private float _ultimatePoint = 0;

    [SerializeField, Range(0.0f, 10.0f), Header("�|�C���g���R�㏸��")]
    private float _increasePoint = 1.0f;

    [SerializeField, Range(1.0f, 2.5f), Header("�|�C���g���R�㏸�ʊԊu")]
    private float _pointIncreasePerSec = 1.0f;

    [SerializeField, Header("�|�C���g�񕜉\��Ԃ�")]
    private bool _canPointIncrease = false;

    private bool _isPointIncrease = false;

    private bool _isWaiting = false;

    private bool _isCalledOnce = false;

    [SerializeField, Header("���[�^�[")]
    Image meter = default;

    Unit selfUnit = default;    //���g��unit

    [SerializeField, Header("GameController")]
    private GameController GC;

    [SerializeField, Header("MainUI��Animator")]
    private Animator UIanimator;


    public int intWeaponType = default;

    [SerializeField, Header("�e����̖��������Z�|�C���g")]
    private int[] weaponsIncreasePoints = default;

    [SerializeField, Header("�G�̌��j�����Z�|�C���g")]
    private int�@eKIAIncreasePoint = default;

    [SerializeField, Header("�e��n�̔j�󎞉��Z�|�C���g")]
    private int[] destroyBasesIncreasePoints = default;

    [SerializeField, Header("�A�C�e���擾�����Z�|�C���g")]
    private int[] getItemIncreasePoints = default;

    [SerializeField, Range(0.00f, 1.00f), Header("���S���̃y�i���e�B���[�g")]
    private float PointPenaltyRates = 0.75f;

    [SerializeField, Header("������|�C���g���Z�s�\����")]
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


    [SerializeField, Header("���[�^�[�ő厞SE")]
    private AudioClip _soundMax = default;

    private void Start()
    {
        _isCalledOnce = false;

        audio = this.GetComponent<AudioSource>();

        selfUnit = this.GetComponent<Unit>();
    }

    private void OnDisable()
    {
        //���񂾂珊��̊������_
        _ultimatePoint = Mathf.Floor(_ultimatePoint * PointPenaltyRates);

        //�v���C���[��������
        if (this.name == "Player" || this.name == "Player2")
        {
            UIanimator.SetBool("ULTmeter_Idle", false);
        }
    }

    private void OnEnable()
    {
        _isCalledOnce = false;
        //�v���C���[��������
        if (this.name == "Player" || this.name == "Player2")
        {
            UIanimator.SetBool("ULTmeter_MAX", false);
            UIanimator.SetTrigger("ULTmeter_Reset");
            //���[�^�[UI�ɐ��l��\��
            meter.fillAmount = _ultimatePoint / _maxUltimatePoint;
        }
    }
    private void Update()
    {
        //�Q�[�����łȂ��Ȃ�
        if (!GC._inGame())
        {
            return;
        }

        //�v���C���[��������
        if (this.name == "Player" || this.name == "Player2")
        {
            //���[�^�[UI�ɐ��l��\��
            meter.fillAmount = _ultimatePoint / _maxUltimatePoint;
        }

        //���݂̃|�C���g���ő�l�����Ȃ�
        if (_ultimatePoint < _maxUltimatePoint)
        {
            //�|�C���g���Z���ł͂Ȃ� ���|�C���g���Z�s�\�Ȃ�
            if (!_isPointIncrease && !_canPointIncrease &&!_isWaiting)
            {
                //���Z�\�t���O��true��
                _canPointIncrease = true;

                //�|�C���g���Z
                StartCoroutine(PointIncrease());

                //���Z���t���O��true��
                _isPointIncrease = true;

            }
        }
        else
        {
            //�v���C���[��������
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


        //�A���e�B���b�g�|�C���g���񕜉\��ԂȂ�
        if (_canPointIncrease)
        {
            //�f�o�b�O���[�h����
            if (!selfUnit.debugMode)
            {
                if (!_isPointIncrease)
                {
                    //�|�C���g���Z
                    StartCoroutine(PointIncrease());
                    _isPointIncrease = true;
                }
                
            }
            else
            {
                //�|�C���g����ɍő��
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
                //�|�C���g���ő�l�ɂ��A���Z�s�\�ɂ���B
                SetPointToLimit();
                break;
            }

            //�|�C���g�����Z
            _ultimatePoint = _ultimatePoint + _increasePoint;
            

            yield return new WaitForSeconds(_pointIncreasePerSec);
        }
    }

    /// <summary>
    /// �A���e�B���b�g���g�p�����ꍇ
    /// </summary>
    public void UltimateUsed(int typeOfUltimate)
    {
        //�v���C���[��������
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
                //�|�C���g���ő�l�ɂ��A���Z�s�\�ɂ���B
                SetPointToLimit();
            }
            _ultimatePoint = _ultimatePoint + weaponsIncreasePoints[setWeaponType];
        }
        
    }

    /// <summary>
    /// <para>AddPointByDestroyEnemy</para>
    /// <para>�G���j���̃|�C���g���Z</para>
    /// </summary>
    public void AddPointByDestroyEnemy()
    {
        if (_canPointIncrease)
        {
            _ultimatePoint = _ultimatePoint + eKIAIncreasePoint;
            if (_ultimatePoint >= _maxUltimatePoint)
            {
                //�|�C���g���ő�l�ɂ��A���Z�s�\�ɂ���B
                SetPointToLimit();
            }
        }
    }

    /// <summary>
    /// <para>AddPointByDestroyEnemyBase</para>
    /// <para>���_�j�󎞂̃|�C���g���Z</para>
    /// </summary>
    /// <param name="setBaseType">���_�̃^�C�v SMALL = 0, MIDDLE = 1, BIG = 2</param>
    public void AddPointByDestroyEnemyBase(int setBaseType)
    {
        if (_canPointIncrease)
        {
            _ultimatePoint = _ultimatePoint + destroyBasesIncreasePoints[setBaseType];
        }
    }

    /// <summary>
    /// <para>GetULTItem</para>
    /// <para>�A�C�e���Q�b�g���̃|�C���g���Z</para>
    /// </summary>
    /// <param name="itemType">�A�C�e���̃^�C�v SMALL = 0, BIG = 1</param>
    public void GetULTItem(int itemType)
    {
        if (_canPointIncrease)
        {
            _ultimatePoint = _ultimatePoint + getItemIncreasePoints[itemType];
            if (_ultimatePoint >= _maxUltimatePoint)
            {
                //�|�C���g���ő�l�ɂ��A���Z�s�\�ɂ���B
                SetPointToLimit();
            }
        }
    }

    private void SetPointToLimit()
    {
        //����炷����
        audio.PlayOneShot(_soundMax);


        //�|�C���g���ő�l�ɂ��A���Z�s�\�ɂ���B
        _ultimatePoint = _maxUltimatePoint;
        _canPointIncrease = false;
        _isPointIncrease = false;
    }

    /// <summary>
    /// <para>GameStart</para>
    /// <para>�}�b�`�J�n���Ɏ��R�񕜂��N������</para>
    /// </summary>
    public void GameStart()
    {
        _canPointIncrease = true;
    }
}

