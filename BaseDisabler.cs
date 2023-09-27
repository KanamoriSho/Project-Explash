using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDisabler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            this.gameObject.SetActive(false);
        }
    }
}
