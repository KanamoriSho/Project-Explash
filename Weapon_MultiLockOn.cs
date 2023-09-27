using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*---------------------------------------------------------------------------*/
/// ����� : ���� ��
/// �T�v�@ : �}���`���b�N�I���~�T�C������(�A���e�B���b�g1)
/// �i���@ : ����(��X���X�N���v�g�Ƃ̂��荇�킹�Œ����̉\���A��)
/*---------------------------------------------------------------------------*/

public class Weapon_MultiLockOn : Weapons
{

    /**
     * �}���`���b�N�I���~�T�C���̃X�N���v�g
     * ��{�I��"�e��p�����[�^"�̒��Ɋi�[����Ă���ϐ���Inspector�ŕς��邾���œ����悤�ɂȂ��Ă��܂��B
     * �Ȃ̂Ŋ�{�I�ɃX�N���v�g���͘M��Ȃ��đ��v�ł��B
     * 
     * <�e�����>
     * �ETimeTodisappear    : ��������܂ł̎��Ԃł��B�@�����M��΃~�T�C���̂������Ǝ˒�������ς����܂��B
     * �EMissileDerayTime   : ������������J�n�܂ł̃f�B���C�𒲐��ł��܂��B ���l����ŋ�����퓬�@�̃~�T�C���̂悤�ȋ����ɂł��܂��B
     * �EHomingDerayTime    : ��������ǔ��J�n�܂ł̃f�B���C�𒲐��ł��܂��B ���l���傫���قǋߋ����Ɏキ�Ȃ�܂��B
     * �EMissilePower       : �~�T�C���̉����͂ł��B�@Power is Power
     * �EMissileMaxSpeed    : �~�T�C���̍ō����ł��B�@�グ��قǑ����Ȃ�܂��B���̕����x��������̂ő��p�����[�^�������K�{�ł��B
     * �ERigidBodyDrag      : Rigidbody��Drag�𒲐��ł��܂��B �l�������قǏ����͌����܂����x���A�Ƃ������������d���Ȃ�܂��B
    **/

    //����{��
    private GameObject TurretMissile;
    //�~�T�C���̃��W�b�g�{�f�B
    #region �e��p�����[�^
    /// <summary>
    /// �^�[�Q�b�g���Z�b�g����
    /// Target => ���˂����L�����N�^�[�̃X�N���v�g����^�[�Q�b�g���擾����
    /// target => public�ϐ�Target����^�[�Q�b�g���擾����
    /// </summary>

    [Header("\n ��������~�T�C���̃p�����[�^ \n")]


    [SerializeField, Range(0.0f, 1.0f), Header("���˂܂ł̎��� (�f�t�H���g:0)")]
    private float _missileDerayTime = default;

    [SerializeField, Range(0.0f, 3.0f), Header("���ւ̃��[�v�܂ł̎��� (�f�t�H���g:0.5)")]
    private float _warpToTargetsAbove = default;

    [SerializeField, Range(30.0f, 200.0f), Header("��󃏁[�v�n�_���� (�f�t�H���g:100)")]
    private float _warpPointHeight = default;

    [SerializeField, Range(0.0f, 1.0f), Header("���[�v����ĉ����܂ł̃f�B���C")]
    private float _missileWarpedDerayTime = default;


    [SerializeField, Range(0, 100), Header("���i�� (�f�t�H���g:20)")]
    private float MissilePower = default;


    [SerializeField, Range(10, 50), Header("�ō��� (�f�t�H���g:30)")]
    private float MissileMaxSpeed = default;


    [SerializeField, Range(10, 50), Header("�Œᑬ�x (�f�t�H���g:30)")]
    private float MissileMinSpeed = default;

    [SerializeField, Range(0, 5), Header("��R (�f�t�H���g:1)")]
    private float RigidBodyDrag = default;

    #endregion

    //���x
    private Vector3 velocity;
    //���˂���Ƃ��̏����ʒu
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

    [SerializeField, Range(0, 100), Header("�^�[�Q�b�g���X�g�܂ł̎���")]
    private float targetMissLimit = 0;

    private float targetMissTime = 0;

    [SerializeField, Header("�����͈͕\���I�u�W�F�N�g")]
    private GameObject explosionAreaMarker = default;

    [SerializeField]
    private Animator explosionAreaMarkerAnim = default;

    [SerializeField, Header("�����͈̓I�u�W�F�N�g")]
    private GameObject explosionArea = default;

    [SerializeField, Header("�����͈͂̎q�I�u�W�F�N�g")]
    private GameObject[] explosionObjects = default;

    [SerializeField, Header("�������S���I�u�W�F�N�g")]
    private GameObject explosionCenter = default;

    [SerializeField, Header("�����O�����I�u�W�F�N�g")]
    private GameObject explosionOutside = default;

    // �������S����Weapons
    private Weapons weaponsScriptCenter = default;

    // �����O������Weapons
    private Weapons weaponsScriptOutside = default;

    protected override void OnEnable()
    {
        _isBoomed = false;

        explosionArea.SetActive(false);

        explosionAreaMarker.SetActive(false);

        //�����I�u�W�F�N�g��Weapons���擾
        weaponsScriptCenter = explosionCenter.GetComponent<Weapons>();
        weaponsScriptOutside = explosionOutside.GetComponent<Weapons>();

        _isCalledOnce = false;

        base.OnEnable();
    }

    private void LateUpdate()
    {
        //�ڕW���ǔ�����null�ɂȂ��� or �ڕW�̃^�O��Obstacle��������
        if (target && !target.activeSelf | target && target.tag == "Obstacle")
        {
            //�ڕW��null��
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
            ///����(����)��������܂ł̃C���^�[�o���ȍ~ �` ������b�O��
            /// ������ʂ�ԉ�������
            /// </summary>
            if (!isWarping)
            {
                Acceleration();
            }

            if (time > _warpToTargetsAbove && time < _warpToTargetsAbove + _missileWarpedDerayTime)
            {
                //���[�v���Ă���Ĕ��˖��ɑ��x���������A�p�x��ݒ肷��B

                explosionAreaMarker.SetActive(true);

                explosionAreaMarker.gameObject.transform.parent = null;

                explosionAreaMarker.transform.position = target.transform.position;

                explosionAreaMarker.transform.rotation = new Quaternion(0, 0, 0, 0);

                //���[�v���t���Otrue
                isWarping = true;
                //���x������
                RB.velocity = new Vector3(0, 0, 0);
                //���W���i�[
                transform.position = new Vector3(target.transform.position.x, _warpPointHeight, target.transform.position.z);
                //�^�[�Q�b�g�̕�������������
                transform.rotation = Quaternion.LookRotation(target.transform.position - this.transform.position);

            }
            else if (time > _warpToTargetsAbove + _missileWarpedDerayTime)
            {
                //�Ĕ���

                //���[�v���t���Ofalse
                isWarping = false;
            }
        }
        else   //����
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

        //�e��p�����[�^������
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

        //�ō������~�b�^�[
        if (RB.velocity.magnitude >= MissileMaxSpeed)
        {
            RB.velocity = RB.velocity.normalized * MissileMaxSpeed;
        }

        //�Œᑬ���~�b�^�[
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
