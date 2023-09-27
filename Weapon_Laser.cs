using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/*---------------------------------------------------------------------------*/
/// ����� : ���� ��
/// �T�v�@ :���[�U�[(�A���e�B���b�g2)
/// �i���@ : ����(��X���X�N���v�g�Ƃ̂��荇�킹�Œ����̉\���A��)
/*---------------------------------------------------------------------------*/

public class Weapon_Laser : Weapons
{

    [SerializeField, Header("SE")]
    private AudioClip _SE = default;

    private AudioSource _audio = default;

    [SerializeField, Range(0.00f, 0.50f), Header("�U������̏o��Ԋu")]
    private float _intervalOfAttackArea = 0.25f;

    private int _countOfAttackAreaEnabled = 0;                    //�U������̔����񐔃J�E���g�p

    private int _maxCountOfAttackAreaEnabled = default;           //�U������̍ő唭����

    [SerializeField, Range(0.0f, 5.0f), Header("�U������̏o��܂ł̃f�B���C")]
    private float _delayOfAttackArea = 1.0f;

    [SerializeField, Header("�U������̏o��܂ł̃f�B���C�v���p�t���O")]
    private bool _isTimerOverDelayOfAttackArea = false;           //�U������̏o��܂ł̃f�B���C�v���p�t���O

    [SerializeField, Range(0.0f, 5.0f), Header("�U������̏o�Ă��鎞��")]
    private float _timeOfAttackArea = 3.0f;

    [SerializeField, Header("�U������̏o�Ă��鎞�Ԍv���p�t���O")]
    private bool _isTimerOverAttackAreaTime = false;              //�U������̏o�Ă��鎞�Ԍv���p�t���O

    [SerializeField, Range(0.0f, 5.0f), Header("�v���C���[���ړ��\�ɂȂ�܂ł̎���")]
    private float _timeOfinactionable = 2.0f;

    [SerializeField, Header("�v���C���[���ړ��\�ɂȂ�܂ł̎��Ԍv���p�t���O")]
    private bool _isTimerOverOfinactionableTime = false;          //�v���C���[���ړ��\�ɂȂ�܂ł̎��Ԍv���p�t���O

    [SerializeField, Header("�v���C���[(�X�l)")]
    private GameObject _player = default;

    private Transform _playerTransform = default;           //�v���C���[��Transform

    private Unit unit = default;                            //�v���C���[��Unit�R���|�[�l���g

    private Rigidbody _playerRb = default;                  //�v���C���[��RigidBody

    [SerializeField, Range(0, 100), Header("�΋��_�_���[�W�̌������[�g(%)")]
    private int _attenuationRateOfAnti_BaseDamage = 30;

    [SerializeField, Header("���˒n�_�̃v���C���[�Ƃ̑��΍��W")]
    private Vector3 _ofset = default;                       

    [SerializeField, Header("SphreCast�̔��a")]
    private float _radius = 20;

    [SerializeField, Header("SphreCast�̏Ǝˋ���")]
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



    /* �p������Weapons�ɂ��邯�ǒʂ������Ȃ������B��return;�ŏ㏑��
     * 
     * Acceleration()       : ��������
     * FixedUpdate()        : �ǔ�������Ȃǂ̕��������n
     * Kaboom()             : ��������
     */
    #region �ʂ��Ȃ������B

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

        _playerRb = _player.GetComponent<Rigidbody>();                                      //�v���C���[��RigidBody�擾

        _child = this.gameObject.transform.GetChild(0).gameObject;                          //�q�I�u�W�F�N�g���擾

        _audio = _player.GetComponent<AudioSource>();                                       //�v���C���[��AudioSource���擾

        _maxCountOfAttackAreaEnabled = (int)(_timeOfAttackArea / _intervalOfAttackArea);    //�U������̍ő唭���񐔂��`
    }

    protected override void OnEnable()
    {

        //�t���O�ƕϐ��̏�����

        this.gameObject.transform.parent = _player.transform;                               //�e�I�u�W�F�N�g���v���C���[�Ɏw��

        this.gameObject.transform.position = _player.transform.position;                    //�ʒu���v���C���[�̍��W�ɏC��

        this.gameObject.transform.rotation = _player.transform.rotation;                    //�������v���C���[�̊p�x�ɏC��

        time = 0;                                                    //�^�C�}�[�����Z�b�g

        _countOfAttackAreaEnabled = 0;                               //�U������̔����񐔃J�E���g�����Z�b�g

        _isTimerOverDelayOfAttackArea = false;                       //�U������̏o��܂ł̃f�B���C�̎��Ԍv���p�t���O

        _isTimerOverAttackAreaTime = false;                          //�U������̏o�Ă��鎞�Ԍv���p�t���O

        _isTimerOverOfinactionableTime = false;                      //�v���C���[���s���\�ɂȂ�܂ł̎��Ԍv���p�t���O

        _audio.PlayOneShot(_SE);
    }

    private void OnDisable()
    {

        //�t���O�ƕϐ��̏�����

        time = 0;                                                    //�^�C�}�[�����Z�b�g

        _countOfAttackAreaEnabled = 0;                               //�U������̔����񐔃J�E���g�����Z�b�g

        _isTimerOverDelayOfAttackArea = false;                       //�U������̏o��܂ł̃f�B���C�̎��Ԍv���p�t���O

        _isTimerOverAttackAreaTime = false;                          //�U������̏o�Ă��鎞�Ԍv���p�t���O

        _isTimerOverOfinactionableTime = false;                      //�v���C���[�������\�ɂȂ�܂ł̎��Ԍv���p�t���O

        if(gameObject.transform.parent == _player.transform)
        {
            this.gameObject.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if(_isTimerOverOfinactionableTime)                          //�ړ��s�\��ԂȂ�
        {
            _playerRb.velocity = new Vector3(0, 0, 0);              //���x��0�ŏ㏑��
        }

        //�f�B���C�ҋ@�I���t���O��false�Ȃ�
        if (!_isTimerOverDelayOfAttackArea)
        {
            _isTimerOverOfinactionableTime = true;                  //�ړ��s�\�t���O��true��

            _dangerZone.SetActive(true);                             //���[�U�[�̌����ڂ�true��

            //�^�C�}�[�����ˑO�f�B���C�𒴂��Ă�����
            if (time >= _delayOfAttackArea)
            {
                _isTimerOverDelayOfAttackArea = true;               //�f�B���C�ҋ@�I���t���O��true��
                time = 0;                                           //�^�C�}�[�����Z�b�g
            }
        }
        //���ˑO�f�B���C���I�����Ă�����
        else
        {

            this.gameObject.transform.parent = null;                //���[�U�[���v���C���[���番��
                _dangerZone.SetActive(false);                         //���[�U�[(������)������

            _laserTube.SetActive(true);                         //���[�U�[�̐F��ύX

            enemiesInLaser =                                        //SphereCastAll�Ŕ͈͓��̓G��S�Ď擾
                    Physics.SphereCastAll
                    (this.transform.position + _ofset, _radius, this.transform.forward,�@_direction,
                                _enemyLayer).Select(hits => hits.collider.gameObject).ToList();
            
            if(time > _timeOfinactionable - _delayOfAttackArea)
            {
                _isTimerOverOfinactionableTime = false;             //�ړ��s�\�t���O��false��
            }


            if(time > _intervalOfAttackArea && !_isTimerOverAttackAreaTime)
            {
                DamagedAreaOccurrence();                            //�͈͓��̓G�Ƀ_���[�W��^����
                time = 0;                                           //�^�C�}�[�����Z�b�g
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
        //���X�g���̑S�Ă̓G���I�u�W�F�N�g�Ƀ_���[�W��^����
        for (int lengthOfEnemiesInLaser = 0; lengthOfEnemiesInLaser < enemiesInLaser.Count; lengthOfEnemiesInLaser++)
        {

            //�ΏۃI�u�W�F�N�g��Unit�����邩
            if (enemiesInLaser[lengthOfEnemiesInLaser].TryGetComponent(out Unit unit))
            {
                unit.Damage(damage, _child);            //Unit������Ȃ�Unit�̃_���[�W�������N��
            }
            else
            {
                int fixedDamage = damage / _attenuationRateOfAnti_BaseDamage;

                //�������Castle_HP�ɂ���_���[�W�������N��
                enemiesInLaser[lengthOfEnemiesInLaser].GetComponent<Castle_HP>().
                                        Damage(_shooterNum, _shooter, fixedDamage, this.gameObject);
            }

        }

            Debug.Log("���߁[��");

        _countOfAttackAreaEnabled++;                            //�U������̔����񐔂����Z

        //�U���񐔂̍ő吔�ɒB������
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
