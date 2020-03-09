using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchPad : MonoBehaviour {

    [SerializeField]
    private RectTransform _touchPad;

    private int _toucld = -1;
    /* 터치 입력중에 방향 컨트롤의 영역안에 있는 
    * 입력을 구분하기 위해서 사용한 변수
    * 원밖에 터지했는지 확인하기 위한 변수
    */
    private Vector3 _startPos;//터치패드의 시작지점
    private float _dragRadius=90f;//방향 컨트롤러가 원으로 움직이는 반지름
    [SerializeField]
    private PlayerCtrl _player;
    public bool _pressbutton;

    void Start () {
        _touchPad = GetComponent<RectTransform>();
        _startPos = _touchPad.position;//초기화
        _player = GameObject.FindWithTag("Player").GetComponent <PlayerCtrl>();
        //Hierarchy에 Player라는 Tag를 가진 Object 안에 PlayerMovement
	}
	
    public void _ButtonDown()
    {
        _pressbutton = true;
    }

    public void _ButtonUp()
    {
        _pressbutton = false;
        HandleInput(_startPos);
    }

    /*고정프레임 업데이트
     * 그냥 update Method 는 일정하지 않는다.
     * FiexedUpdate Method는 일정하다.
    */
     
	void FixedUpdate () {
        //현재 플랫폼이 안드로이드 플랫폼과 같다면
        if (Application.platform == RuntimePlatform.Android)
        {
        HandleTouchInput(); //모바일 터치용 함수
        }
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            if ((Screen.width / 2) >= Input.mousePosition.x)
            {
                HandleInput(Input.mousePosition);//pc게임용 마우스를 사용하는 함수
            }
        }

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
                if ((Screen.width / 2) >= touch.position.x)
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
            Vector3 diffVector = (_input - _startPos);
            // 터치지점에서 시작 포지션을 빼면  거리가 구해진다.
            if (diffVector.sqrMagnitude > _dragRadius * _dragRadius)
            {               //3d개체 크기  입력지점과 기준좌표의 거리를 비교 한다. 만약 최대치보다 크다면
                diffVector.Normalize();// 방향을 유지한채 방향벡터의 거리를 1로 만든다.
                _touchPad.position = _startPos + diffVector * _dragRadius;
                //그리고 방향컨트롤러는 최대치 만큼 움직이게 한다.
            }
            else
            {
                _touchPad.position = _input;
            }
        }
        else //누르지 않았다면
        {
            _touchPad.position = _startPos;

            if (_player != null)
            {
                //방향만 전달한다.
                _player.OnStickChanged(new Vector3(0f,0f,0f));
                return;
            }
        }
        //방향키와 기준지점의 차이를 구한다.
        Vector2 diff = _touchPad.position - _startPos;
        //방향키의 방향을 유지한채로 거리를 나누어 방향만 구한다.
        Vector3 normDiff = new Vector3(diff.x / _dragRadius, diff.y / _dragRadius);
        if (_player != null)
        {
            //방향만 전달한다.
            _player.OnStickChanged(normDiff);
        }
    }

}
