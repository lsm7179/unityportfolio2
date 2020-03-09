using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//싱글톤으로 만들어주는 스크립트
public class SingletonMonobehavior<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;
    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T));
                if (_instance == null)
                {
                    var _newGameObject = new GameObject(typeof(T).ToString());
                    _instance = _newGameObject.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    //가상함수 Awake
    //부모 클래스에서 virtual 키워드를 사용하여 함수를 만들면, 자식 클래스에서 이 함수를 재정의 할 수 있도록 허용하겠다는 의미입니다.
    protected virtual void Awake()
    {
        if(_instance == null)
        {
            _instance = this as T;
        }
        //씬이 변경되어도 사라지지 않는 객체
        //가급적이면 트랜스폼에 루트게임오브젝트를 파라메터로 넘겨주는것이 좋다.
        //DontDestroyOnLoad(gameObject);
    }
 
}
