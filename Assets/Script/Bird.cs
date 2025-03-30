using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public enum PowerType
{
    DoubleJump,
}


public class Bird : MonoBehaviour
{
    // Variables de l'oiseau
    public float mass = .8f;
    [SerializeField] private PowerType powerType;




    public List<Vector2> listPosBird;
    float duration = .1f; // Durée de l'interpolation
    private Coroutine moveCoroutine;
    private delegate void PowerDelegate();
    private PowerDelegate powerDelegate;


    // variables environnement
    private float gravity = 9.81f;
    private float k = 10;
    private float f2;


    // Liste des positions de la trajectoire
    public int pointsCount = 100;
    public float timeStep = 0.1f;
    public float distance;
    public Vector2[] tempListPosBird;




    private void Start()
    {
        listPosBird = new List<Vector2>();
        GetComponent<Rigidbody2D>().mass = mass;

        f2 = 0.2f / mass;

        if (powerType == PowerType.DoubleJump)
        {
            powerDelegate = DoubleJump;
        }
    }




    public void Power()
    {
        powerDelegate();
    }


    public void Launch()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }



        listPosBird.Clear();
        listPosBird = tempListPosBird.ToList<Vector2>();
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


    // ----------------------------------------------------------------------------------------------------------------- //
    // ---------------------------------------------------- Calculs ---------------------------------------------------- //
    // ----------------------------------------------------------------------------------------------------------------- //


    // Calculate vitesse initiale
    public float VitesseInitiale(float distance, float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        float vitesse = distance * Mathf.Sqrt(k / mass) * Mathf.Sqrt(1 - Mathf.Pow((mass * gravity) / (distance * k) * Mathf.Sin(angleRad), 2));
        return vitesse * 2;
    }


    public void CalculateTrajectorySpring(Vector2 startPosition, float angle, float vitesseInitiale)
    {
        tempListPosBird = new Vector2[pointsCount];
        float radianAngle = (angle + 180f) * Mathf.Deg2Rad;

        float lambdaX = vitesseInitiale * Mathf.Cos(radianAngle);
        float lambdaY = vitesseInitiale * Mathf.Sin(radianAngle) + gravity / f2;

        for (int i = 0; i < pointsCount; i++)
        {
            float t = i * timeStep;
            float x = startPosition.x + lambdaX / f2 * (1 - Mathf.Exp(-f2 * t));
            float y = startPosition.y + lambdaY / f2 * (1 - Mathf.Exp(-f2 * t)) - gravity / f2 * t;

            tempListPosBird[i] = new Vector2(x, y);
        }
    }


    private void DoubleJump()
    {
       CalculateTrajectorySpring(transform.position, -135, VitesseInitiale(distance, -135));
       Launch();
    }
}