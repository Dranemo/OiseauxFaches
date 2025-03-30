using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public enum PowerType
{
    DoubleJump,
}


public class Bird : MonoBehaviour
{
    // Variables de l'oiseau
    public float mass = .8f;
    [SerializeField] private PowerType powerType;




    List<Vector2> listPosBird;
    float duration = .1f; // Durée de l'interpolation
    private Coroutine moveCoroutine;




    private void Start()
    {
        listPosBird = new List<Vector2>();
        GetComponent<Rigidbody2D>().mass = mass;
    }











    public void Power()
    {

    }


    public void Launch(List<Vector2> _listPosBird)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }


        listPosBird.Clear();
        listPosBird = _listPosBird;
        moveCoroutine = StartCoroutine(MoveBird());
    }

    private IEnumerator MoveBird()
    {
        for (int i = 0; i < listPosBird.Count - 1; i++)
        {
            Vector2 startPos = listPosBird[i];
            Vector2 endPos = listPosBird[i + 1];
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                transform.position = Vector2.Lerp(startPos, endPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;
        }
    }
}