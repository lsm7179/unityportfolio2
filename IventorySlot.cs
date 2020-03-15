using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class IventorySlot : MonoBehaviour
{

    public string itemname;//아이템의 이름
    public int itemCount; //획득한 아이템의 개수
    public Image itemImage;//아이템의 이미지

    [SerializeField]
    private Text text_Count;

    /// <summary>
    /// 이미지 투명도 조절
    /// </summary>
    /// <param name="_alpha"></param>
    private void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    /// <summary>
    /// 아이템 획득
    /// </summary>
    /// <param name="_item"></param>
    /// <param name="_count"></param>
    public void AddItem(Itemgem _item,int _count = 1)
    {
        itemname = _item.itemName;
        itemImage.sprite = _item.itemIcon;
        itemCount += _count;
        text_Count.text = itemCount.ToString();
        SetColor(1);
        
    }

    /// <summary>
    /// 아이템 갯수조정
    /// </summary>
    /// <param name="_count"></param>
    public void SetSlotCount(int _count)
    {
        itemCount += _count;
        text_Count.text = itemCount.ToString();

        if (itemCount <= 0)
        {
            ClearSlot();
        }
        
    }

    /// <summary>
    /// 슬롯초기화
    /// </summary>
    private void ClearSlot()
    {
        itemname = string.Empty;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(0);

        text_Count.text = "0";
    }
    
}
