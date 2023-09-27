using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropping : MonoBehaviour
{
    [SerializeField, Header("ドロップアイテム")]
    private GameObject _item = default;

    [SerializeField, Header("ドロップアイテムオブジェクトプール")]
    private GameObject _itemPool = default;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(_item, _itemPool.transform); 
    }
    private void OnDisable()
    {
        try
        {
            //オブジェクトプールを検索
            for (int j = 0; j < _itemPool.transform.childCount; j++)
            {
                //アクティブでない弾があったとき
                if (_itemPool.transform.GetChild(j).gameObject.activeSelf == false)
                {
                    //弾をnewWeaponに格納
                    GameObject item = _itemPool.transform.GetChild(j).gameObject;

                    item.transform.position = this.gameObject.transform.position;

                    item.SetActive(true);
                    return;
                }
            }
            return;
        }
        catch
        {

        }
    }
}
