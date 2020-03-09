using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCtrl : MonoBehaviour
{

    [SerializeField]
    private GameObject go_SlotsParent;
    [SerializeField]
    private IventorySlot[] slots;

    void Start()
    {
        go_SlotsParent = transform.gameObject;
        slots = go_SlotsParent.GetComponentsInChildren<IventorySlot>();
    }

    void Update()
    {
        
    }

    public void AcquireItem(Itemgem _item, int _count = 1)
    {

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemname.Equals(string.Empty))
            {
                slots[i].AddItem(_item, _count);
                return;
            }
            else
            {
                if (slots[i].itemname == _item.itemName)
                {
                    slots[i].SetSlotCount(_count);
                    return;
                }
            }
        }
    }
}
