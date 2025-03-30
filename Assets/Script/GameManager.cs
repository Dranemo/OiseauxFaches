using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

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
    public bool camFollowBird = false;

    Coroutine movingBirdsCoroutine;
    Coroutine watchCoroutine;

    Camera mainCamera;
    Vector3 mainCamPos;





    [SerializeField] private GameObject blocksPosObject;
    Vector2 blocksPos;


    private void Awake()
    {
        foreach (int index in IndexBirdsLevel)
        {
            GameObject bird = Instantiate(birdPrefabList[index], firstBirdPos, Quaternion.identity);
            firstBirdPos += birdDistance;
            birdsList.Add(bird);
        }

        _instance = this;
    }

    private void Start()
    {
        Player.Instance().SetBird(birdsList[birdOnSpringIndex + 1].GetComponent<Bird>());

        StartCoroutine(StartCoroutine());
    }

    private void Update()
    {
        if (camFollowBird && birdsList[birdOnSpringIndex].transform.position.x > mainCamPos.x && birdsList[birdOnSpringIndex].transform.position.x < blocksPos.x)
        {
            mainCamera.transform.position = new Vector3(birdsList[birdOnSpringIndex].transform.position.x, mainCamPos.y, mainCamPos.z);
        }
    }




    IEnumerator StartCoroutine()
    {
        mainCamera = Camera.main;
        mainCamPos = mainCamera.transform.position;

        blocksPos.x = blocksPosObject.transform.position.x;
        blocksPos.y = mainCamPos.y;


        yield return watchCoroutine = StartCoroutine(MoveToPos(blocksPos, 2f));
        yield return watchCoroutine = StartCoroutine(WaitDur(2f));
        yield return watchCoroutine = StartCoroutine(MoveToPos(mainCamPos, 2f));


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

    private IEnumerator MoveToPos(Vector2 endPos, float duration)
    {
        Vector3 startPos = mainCamera.transform.position;
        Vector3 end = new Vector3(endPos.x, endPos.y, startPos.z);
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            mainCamera.transform.position = Vector3.Lerp(startPos, end, t);
            time += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = end;
    }

    private IEnumerator WaitDur(float duration)
    {
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator NextBirdCorout( )
    {
        yield return MoveToPos(mainCamPos, 2);
        if (birdOnSpringIndex < birdsList.Count - 1)
        {
            movingBirdsCoroutine = StartCoroutine(MoveBirds(springPos, 3, 1, .2f));
        }
    }

    public void NextBird()
    {
        if (movingBirdsCoroutine != null)
            return;
        canSpring = false;
        camFollowBird = false;
        StartCoroutine(NextBirdCorout());
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
