using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : SingletonMonobehavior<EnemyManager> {

    [SerializeField]
    private Transform[] points;
    [SerializeField]
    private GameObject slimePre;
    [SerializeField]
    private GameObject turtleShellPre;
    [SerializeField]
    private GameObject goblinPre;
    [SerializeField]
    private float createTime = 0f;
    public bool isGameOver = false;
    public int slimeCount = 0;
    public int turtleShellCount = 0;
    public int goblinCount = 0;
    int pointCount =0;
    public List<GameObject> slimePool = new List<GameObject>();
    public List<GameObject> turtleShellPool = new List<GameObject>();
    public List<GameObject> goblinPool = new List<GameObject>();


    protected override void Awake()
    {
        //하이라키 상에 SpawnPoint 오브젝트 명을 찾은 다음 하위 오브젝트의 트랜스폼 컴퍼넌트를 배열에 대입
        pointCount = this.transform.childCount;
        slimeCount = pointCount / 3;
        turtleShellCount = pointCount / 3;
        goblinCount = pointCount / 3;
        points = this.GetComponentsInChildren<Transform>();
        slimePre = Resources.Load<GameObject>("Prefabs/Enemy/Slime");
        turtleShellPre = Resources.Load<GameObject>("Prefabs/Enemy/TurtleShell");
        goblinPre = Resources.Load<GameObject>("Prefabs/Enemy/Goblin");
        for (int i = 0; i < slimeCount; i++)
        {
            GameObject slime_ = (GameObject)Instantiate(slimePre);
            slime_.name = "slime_" + (i + 1).ToString();
            slime_.SetActive(false);
            slimePool.Add(slime_);
        }
        for (int i = slimeCount; i < slimeCount+turtleShellCount; i++)
        {
            GameObject turtleShell_ = (GameObject)Instantiate(turtleShellPre);
            turtleShell_.name = "turtleShell_" + (i+1).ToString();
            turtleShell_.SetActive(false);
            turtleShellPool.Add(turtleShell_);
        }
        for (int i = slimeCount+ turtleShellCount; i < slimeCount+turtleShellCount+goblinCount; i++)
        {
            GameObject goblin_ = (GameObject)Instantiate(goblinPre);
            goblin_.name = "goblin_" + (i+1).ToString();
            goblin_.SetActive(false);
            goblinPool.Add(goblin_);
        }
        StartCoroutine(CreateSlime());
        StartCoroutine(CreateturtleShell());
        StartCoroutine(Creategoblin());
        StartCoroutine(CheckTime());
        
    }

    IEnumerator CreateSlime()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(createTime);
            if (isGameOver) yield break;
            foreach (GameObject _slime in slimePool)
            {
                if (!_slime.activeSelf)
                {
                    _slime.transform.position = points[int.Parse(_slime.name.Split('_')[1])].position;
                    _slime.SetActive(true);
                    break;
                }
            }
        }
    }

    IEnumerator CreateturtleShell()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(createTime);
            if (isGameOver) yield break;
            foreach (GameObject _turtleShell in turtleShellPool)
            {
                if (!_turtleShell.activeSelf)
                {
                    _turtleShell.transform.position = points[int.Parse(_turtleShell.name.Split('_')[1])].position;
                    _turtleShell.SetActive(true);
                    break;
                }
            }
        }
    }

    IEnumerator Creategoblin()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(createTime);
            if (isGameOver) yield break;
            foreach (GameObject _goblin in goblinPool)
            {
                if (!_goblin.activeSelf)
                {
                    _goblin.transform.position = points[int.Parse(_goblin.name.Split('_')[1])].position;
                    _goblin.SetActive(true);
                    break;
                }
            }
        }
    }

    IEnumerator CheckTime()
    {
        yield return new WaitForSeconds(15f);
        createTime = 30f;
    }

}
