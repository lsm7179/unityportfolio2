using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


//직렬화 하여 아이템을 json에 저장 불러오기 
[Serializable]
public class ItemDataList
{
    public ItemData[] itemDatas;
}
[Serializable]
public class ItemData
{
    public string itemname;
    public int itemcount;
    public int itemorder;

    public ItemData(string _itemname, int _itemcount, int _itemorder)
    {
        this.itemname = _itemname;
        this.itemcount = _itemcount;
        this.itemorder = _itemorder;
    }
}

public class InventoryCtrl : SingletonMonobehavior<InventoryCtrl>
{

    [SerializeField]
    private GameObject go_SlotsParent;
    [SerializeField]
    private IventorySlot[] slots;

    private string jsonString;
    [SerializeField]
    private List<ItemData> itemDatas;
    [SerializeField]
    private ItemDataList itemDataList;

    

    void Start()
    {
        slots = go_SlotsParent.GetComponentsInChildren<IventorySlot>();
        if (Application.platform == RuntimePlatform.Android)
        {
            jsonString = File.ReadAllText(Application.persistentDataPath + "/Resources/Json/itemdata.json");
        }
        else
        {
            jsonString = File.ReadAllText(Application.dataPath + "/Resources/Json/itemdata.json");
        }
        itemDataList = JsonUtility.FromJson< ItemDataList>(jsonString);
        if (itemDataList == null)
        {
            itemDataList = new ItemDataList();
            itemDatas = new List<ItemData>();
        }
        else
        {
            itemDatas = new List<ItemData>(itemDataList.itemDatas);
            foreach (ItemData itemData in itemDataList.itemDatas)
            {
                Itemgem _item = Resources.Load<GameObject>("Prefabs/Gems/" + itemData.itemname).GetComponent<Itemgem>();
                slots[itemData.itemorder].AddItem(_item, itemData.itemcount);
            }
        }
        
    }



    /// <summary>
    /// 인벤토리에 넣는 방법
    /// </summary>
    /// <param name="_item"></param>
    /// <param name="_count"></param>
    public void AcquireItem(Itemgem _item, int _count = 1)
    {

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemname.Equals(string.Empty))
            {
                slots[i].AddItem(_item, _count);
                //여기에 추가하는 로직 구현
                ItemData itemData = new ItemData(_item.itemName, slots[i].itemCount, i);
                itemDatas.Add(itemData);
                itemDataList.itemDatas = itemDatas.ToArray();
                if (Application.platform == RuntimePlatform.Android) File.WriteAllText(Application.persistentDataPath + "/Resources/Json/itemdata.json", JsonUtility.ToJson(itemDataList));
                else  File.WriteAllText(Application.dataPath + "/Resources/Json/itemdata.json", JsonUtility.ToJson(itemDataList)); 
                    
                return;
            }
            else
            {
                if (slots[i].itemname == _item.itemName)
                {
                    slots[i].SetSlotCount(_count);
                    //여기에 수정해서 저장하는 로직 구현
                    foreach(var itemdata in itemDatas)
                    {
                        if (itemdata.itemname.Equals(_item.itemName))
                        {
                            itemdata.itemcount += _count;
                            break;
                        }
                    }
                    itemDataList.itemDatas = itemDatas.ToArray();
                    if (Application.platform == RuntimePlatform.Android) File.WriteAllText(Application.persistentDataPath + "/Resources/Json/itemdata.json", JsonUtility.ToJson(itemDataList));
                    else File.WriteAllText(Application.dataPath + "/Resources/Json/itemdata.json", JsonUtility.ToJson(itemDataList));
                    return;
                }
            }
        }
    }
}
