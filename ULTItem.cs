using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ULTItem : MonoBehaviour
{

    private Animator anim = default;

    [SerializeField]
    private float _respawnTime = default;

    [SerializeField]
    private GameObject _ball = default;

    private bool _isBallActive = true;
    public enum ItemType
    {
        SMALL,
        BIG,
    }

    public ItemType itemType;


    private void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        try
        {
            anim.SetTrigger("Respawn");
        }
        catch
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "BluePlayer" || other.tag == "RedPlayer")
        {
            switch (itemType)
            {

                case ItemType.SMALL:

                    other.GetComponent<UltimateMeter>().GetULTItem((int)ItemType.SMALL);
                    break;

                case ItemType.BIG:

                    other.GetComponent<UltimateMeter>().GetULTItem((int)ItemType.BIG);
                    break;
            }

            _ball.SetActive(false);
            StartCoroutine(waitingForRespawn());
        }
    }

    private IEnumerator waitingForRespawn()
    {

        yield return new WaitForSeconds(_respawnTime);

            _ball.SetActive(true);

    }
}
