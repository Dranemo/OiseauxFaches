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
    bool powerUsed = false;




    public List<Vector2> listPosBird;
    private Coroutine moveCoroutine;
    private delegate void PowerDelegate();
    private PowerDelegate powerDelegate;


    // variables environnement
    private float gravity = 9.81f;
    private float k = 10;
    private float f2;
    private float reboundFactorY = .5f;
    private float reboundFactorX = .9f;
    [SerializeField] private float floorY = -3.5f;


    // Liste des positions de la trajectoire
    public int pointsCount = 100;
    public float timeStep = 0.1f;
    public Vector2[] tempListPosBird;
    private float lambdaX = 0;
    private float lambdaY = 0;
    private float t_impact = .1f;




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
        if(powerUsed)
            return;
        powerUsed = true;

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


	        float comparison = timeStep;
            if (i == listPosBird.Count - 2) 
                comparison = t_impact;

            while (elapsedTime < comparison)
            {
                transform.position = Vector2.Lerp(startPos, endPos, elapsedTime / comparison);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;
        }

        TrajectoryContinuity(transform.position);
        Launch();

        moveCoroutine = null;
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


    public void CalculateTrajectorySpring_recur(Vector2 startPosition, float angle, float vitesseInitiale)
    {

        float radianAngle = (angle + 180f) * Mathf.Deg2Rad;

        lambdaX = vitesseInitiale * Mathf.Cos(radianAngle);
        lambdaY = vitesseInitiale * Mathf.Sin(radianAngle);

        Recurrence(startPosition);
    }

    public void TrajectoryContinuity(Vector2 startPosition, bool jump = false)
    {
        if (jump)
        {
            lambdaY = Mathf.Abs(lambdaY);
        }

        

        Recurrence(startPosition);
    }


    private void Recurrence(Vector2 startPos)
    {
        Vector2[] temp = new Vector2[pointsCount];
        tempListPosBird = new Vector2[pointsCount];

        float x = startPos.x;
        float y = startPos.y;
        temp[0] = new Vector2(x, y);


        for (int i = 1; i < pointsCount; i++)
        {
            x += lambdaX * timeStep;
            y += lambdaY * timeStep;

            if (y <= floorY)
            {

                Vector2 previousPoint = temp[i-1];


                float t = (floorY - previousPoint.y) / (y - previousPoint.y);
                float intersectX = previousPoint.x + t * (x - previousPoint.x);

                t_impact = (t) * timeStep;

                Vector2 intersectionPoint = new Vector2(intersectX, floorY);


                temp[i] = intersectionPoint;

                lambdaX += -f2 * lambdaX * timeStep;
                lambdaY += -(gravity + f2 * lambdaY) * timeStep;


                lambdaY = Mathf.Abs(lambdaY) * reboundFactorY;
                lambdaX *= reboundFactorX;

                tempListPosBird = new Vector2[i+1];
                
                for (int j = 0; j <= i; j++)
                {
                    tempListPosBird[j] = temp[j];
                }

                break;
            }

            temp[i] = new Vector2(x, y);

            lambdaX += -f2 * lambdaX * timeStep;
            lambdaY += -(gravity + f2 * lambdaY) * timeStep;
        }
    }


    private void DoubleJump()
    {
        TrajectoryContinuity(transform.position, true);
        Launch();
    }
}