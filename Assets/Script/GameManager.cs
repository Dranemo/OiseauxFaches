using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;


    [SerializeField] List<GameObject> birdPrefabList;
    [SerializeField] List<int> IndexBirdsLevel;

    [SerializeField] Vector2 firstBirdPos;
    [SerializeField] Vector2 birdDistance;


    public int birdIndex = 0;
    private int birdOnSpringIndex = -1;
    public bool canSpring = false;

    Coroutine movingBirdsCoroutine;

    void Start()
    {
        _instance = this;

        foreach (int index in IndexBirdsLevel)
        {
            GameObject bird = Instantiate(birdPrefabList[index], firstBirdPos, Quaternion.identity);
            firstBirdPos += birdDistance;
        }
    }




    public IEnumerator MoveBirds(Vector2 start, Vector2 end, float height, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            float x = Mathf.Lerp(start.x, end.x, t);
            float y = Mathf.Lerp(start.y, end.y, t) + height * Mathf.Sin(t * Mathf.PI);
            transform.position = new Vector2(x, y);

            time += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
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
