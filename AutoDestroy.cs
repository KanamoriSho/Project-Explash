using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetDisable());
    }

    private IEnumerator SetDisable()
    {
        yield return new WaitForSeconds(10.0f);
        this.gameObject.SetActive(false);
    }
}
