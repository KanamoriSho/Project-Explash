using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropping : MonoBehaviour
{
    [SerializeField, Header("�h���b�v�A�C�e��")]
    private GameObject _item = default;

    [SerializeField, Header("�h���b�v�A�C�e���I�u�W�F�N�g�v�[��")]
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
            //�I�u�W�F�N�g�v�[��������
            for (int j = 0; j < _itemPool.transform.childCount; j++)
            {
                //�A�N�e�B�u�łȂ��e���������Ƃ�
                if (_itemPool.transform.GetChild(j).gameObject.activeSelf == false)
                {
                    //�e��newWeapon�Ɋi�[
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
