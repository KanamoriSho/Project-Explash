using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{

    private Animator anim = default;

    [SerializeField, Range(0, 10), Header("�~�T�C���񕜐�")]
    private int _additionalMissile = default;

    [SerializeField, Range(0, 10), Header("���R���ł܂ł̎���")]
    private float _timeToDisable = 10;

    private float _currentTime = default;

    private void OnEnable()
    {
        _currentTime = 0;
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;

        if(_currentTime >= _additionalMissile)
        {
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "BluePlayer" || other.tag == "RedPlayer")
        {
            other.GetComponent<Unit>().GetDropItem(_additionalMissile);

            this.gameObject.SetActive(false);
        }
    }
}
