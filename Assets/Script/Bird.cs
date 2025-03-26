using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Bird : MonoBehaviour
{
    List<Vector2> listPosBird;
    float duration = .1f; // Durée de l'interpolation
    private Coroutine moveCoroutine;



    public void StartValues(float _mass)
    {
        listPosBird = new List<Vector2>();
        GetComponent<Rigidbody2D>().mass = _mass;
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