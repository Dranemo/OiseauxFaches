using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;


    [SerializeField] List<GameObject> birdPrefabList;
    [SerializeField] List<int> IndexBirdsLevel;
    List<GameObject> birdsList = new List<GameObject>();

    [SerializeField] Vector2 firstBirdPos;
    [SerializeField] Vector2 birdDistance;
    [SerializeField] Vector2 springPos = new Vector2(-.61f, -.67f);


    public int birdIndex = 0;
    private int birdOnSpringIndex = -1;
    public bool canSpring = false;

    Coroutine movingBirdsCoroutine;



    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        foreach (int index in IndexBirdsLevel)
        {
            GameObject bird = Instantiate(birdPrefabList[index], firstBirdPos, Quaternion.identity);
            firstBirdPos += birdDistance;
            birdsList.Add(bird);
        }


        if(birdsList.Count > 0)
        {
            movingBirdsCoroutine = StartCoroutine(MoveBirds(springPos, 3, 1, .2f));
        }
    }




    private IEnumerator MoveBirds(Vector2 end, float height, float durationParabol, float durationLittle)
    {
        birdOnSpringIndex += 1;
        Player.Instance().SetBird(birdsList[birdOnSpringIndex].GetComponent<Bird>());
        Vector2 previousBirdPos = birdsList[birdOnSpringIndex].transform.position;

        float time = 0f;
        while (time < durationParabol)
        {
            float t = time / durationParabol;
            float x = Mathf.Lerp(previousBirdPos.x, end.x, t);
            float y = Mathf.Lerp(previousBirdPos.y, end.y, t) + height * Mathf.Sin(t * Mathf.PI);
            birdsList[birdOnSpringIndex].transform.position = new Vector2(x, y);

            time += Time.deltaTime;
            yield return null;
        }
        birdsList[birdOnSpringIndex].transform.position = end;



        // Déplacer les oiseaux suivants
        for (int i = birdOnSpringIndex + 1; i < birdsList.Count; i++)
        {
            Vector2 targetPos = previousBirdPos; 
            previousBirdPos = birdsList[i].transform.position;
            Vector2 startPos = birdsList[i].transform.position;
            time = 0f;
            while (time < durationLittle)
            {
                float t = time / durationLittle;
                birdsList[i].transform.position = Vector2.Lerp(startPos, targetPos, t);
                time += Time.deltaTime;
                yield return null;
            }
            birdsList[i].transform.position = targetPos;
        }

        canSpring = true;
        movingBirdsCoroutine = null;
    }




    public void NextBird()
    {
        if (movingBirdsCoroutine != null)
            return;


        Debug.Log("Next Bird GM");

        canSpring = false;
        if (birdOnSpringIndex < birdsList.Count - 1)
        {
            movingBirdsCoroutine = StartCoroutine(MoveBirds(springPos, 3, 1, .2f));
        }
    }





    static public GameManager GetManager()
    {
        if (_instance == null)
        {
            _instance = new GameManager();
        }
        return _instance;
    }
}
