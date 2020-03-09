using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUI : MonoBehaviour
{
    [SerializeField]
    private RectTransform _skillPad;
    public RectTransform _skillLine;
    public RectTransform _skillRange;

    private Vector3 _startPos;//스킬이미지의 시작지점
    private float _dragRadius = 30f;//스킬이미지가 원으로 움직이는 반지름
    private Vector3 _skillPos;//스킬의 발사위치

    [SerializeField]
    private PlayerCtrl _player;
    private bool _pressbutton;

    /* 터치 입력중에 방향 컨트롤의 영역안에 있는 
    * 입력을 구분하기 위해서 사용한 변수
    * 원밖에 터지했는지 확인하기 위한 변수 */
    private int _toucld = -1;

    void Start()
    {
        _skillPad = GetComponent<RectTransform>();
        _startPos = _skillLine.position;//초기화
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerCtrl>();
    }

    void FixedUpdate()
    {
        //현재 플랫폼이 안드로이드 플랫폼과 같다면
        if (Application.platform == RuntimePlatform.Android)
        {
            HandleTouchInput(); //모바일 터치용 함수
        }
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            if((Screen.width/2)< Input.mousePosition.x) { 
            HandleInput(Input.mousePosition);//pc게임용 마우스를 사용하는 함수
            }
        }
    }

    /// <summary>
    /// 스킬을 눌렀을때
    /// </summary>
    public void _ButtonDown()
    {
        _pressbutton = true;
    }

    /// <summary>
    /// 스킬을 떼었을때
    /// </summary>
    public void _ButtonUp()
    {
        _pressbutton = false;
        _skillRange.gameObject.SetActive(false);
        _skillLine.gameObject.SetActive(false);
        _player.OnSkill();
        //_player.OnSkill(_skillPos);
    }

    void HandleTouchInput()
    {
        int i = 0;
        //터치 단일변수 자동으로 배열에 담는다.
        if (Input.touchCount > 0)//한번이라도 터치했다면
        {

            int index = 0;
            foreach (Touch touch in Input.touches)
            {
                if ((Screen.width / 2) < touch.position.x)
                {
                    i++;//touchid 번호를 1증가.
                    Vector3 TouchPos = new Vector3(touch.position.x, touch.position.y);
                    //터치 요형 터치를 막시작한 터치유형이라면
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (touch.position.x <= (_startPos.x + _dragRadius))
                        {
                            _toucld = i;
                        }
                        if (touch.position.y <= (_startPos.y + _dragRadius))
                        {
                            _toucld = i;
                        }
                    }
                    //터치 유형이 움직이거나 멈춰있다면.
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        if (_toucld == i)
                        {
                            HandleInput(TouchPos);
                        }
                    }
                    if (touch.phase == TouchPhase.Ended)
                    {
                        if (_toucld == i)
                        {
                            _toucld = -1;
                        }
                    }
                    index++;
                }
            }
        }
    }

    void HandleInput(Vector3 _input)
    {
        if (_pressbutton)
        {
            _skillRange.gameObject.SetActive(true);
            _skillLine.gameObject.SetActive(true);
            Vector3 diffVector = (_input - _startPos);
            // 터치지점에서 시작 포지션을 빼면  거리가 구해진다.
            if (diffVector.sqrMagnitude > _dragRadius * _dragRadius)
            {               //3d개체 크기  입력지점과 기준좌표의 거리를 비교 한다. 만약 최대치보다 크다면
                diffVector.Normalize();// 방향을 유지한채 방향벡터의 거리를 1로 만든다.
                _skillLine.position = _startPos + diffVector * _dragRadius;
                //그리고 방향컨트롤러는 최대치 만큼 움직이게 한다.
            }
            else
            {
                _skillLine.position = _input;
                _skillRange.gameObject.SetActive(false);
                _skillLine.gameObject.SetActive(false);
            }
            //방향키와 기준지점의 차이를 구한다.
            Vector2 diff = _skillLine.position - _startPos;

            //방향키의 방향을 유지한채로 거리를 나누어 방향만 구한다.
            Vector3 normDiff = new Vector3(diff.x / _dragRadius, diff.y / _dragRadius);
            if (_player != null)
            {
                //방향만 전달한다.
                //_player.OnSkillMark(normDiff);
                _skillPos = normDiff;
            }
        }
    }
}

