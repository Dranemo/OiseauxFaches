using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public enum PowerType
{
    DoubleJump,
    Speed
}


public class Bird : MonoBehaviour
{
    // Variables de l'oiseau
    public float mass = .8f;
    [SerializeField] private PowerType powerType;
    bool powerUsed = false;
    float velocity = 0;




    public List<Vector2> listPosBird;
    private Coroutine moveCoroutine;
    private delegate void PowerDelegate();
    private PowerDelegate powerDelegate;


    // variables environnement
    private float gravity = 9.81f;
    private float k = 10;
    private float f2;
    Vector2 reboundFactor = new Vector2(.9f, .5f);
    [SerializeField] private float floorY = -3.5f;


    // Liste des positions de la trajectoire
    public int pointsCount = 100;
    public float timeStep = 0.1f;
    public Vector2[] tempListPosBird;
    private Vector2[] lambdasValues;
    Vector2 currentLambda = new Vector2(0, 0);
    private float t_impact = .1f;
    private int rebondCount = 0;

    public bool stopped = false;
    Vector2 acceleration = new Vector2(1, 1);




    [SerializeField] private AudioClip power;
    CanvasMain canvasMain;



    private void Start()
    {
        canvasMain = CanvasMain.GetInstance();

        listPosBird = new List<Vector2>();
        GetComponent<Rigidbody2D>().mass = mass;

        f2 = 0.2f / mass;


        switch
            (powerType)
        {
            case PowerType.DoubleJump:
                powerDelegate = DoubleJump;
                break;
            case PowerType.Speed:
                powerDelegate = Speed;
                break;
        }
    }




    public void Power()
    {
        if(powerUsed)
            return;
        powerUsed = true;
        
        canvasMain.DisplayPower(false);

        powerDelegate();
        AudioSource.PlayClipAtPoint(power, transform.position);
    }


    public void Launch()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        listPosBird.Clear();
        canvasMain.DisplayPower(!powerUsed, powerType.ToString());

        if (stopped)
        {
            return;
        }

        listPosBird = tempListPosBird.ToList<Vector2>();
        moveCoroutine = StartCoroutine(MoveBird());
    }

    

    private IEnumerator MoveBird()
    {
        for (int i = 0; i < listPosBird.Count - 1; i++)
        {
            currentLambda = lambdasValues[i];

            Vector2 startPos = listPosBird[i];
            Vector2 endPos = listPosBird[i + 1];
            float elapsedTime = 0;


	        float comparison = timeStep;
            if (i == listPosBird.Count - 2) 
                comparison = t_impact;

            while (elapsedTime < comparison)
            {
                Vector2 newPosition = Vector2.Lerp(startPos, endPos, elapsedTime / comparison);
                if (float.IsNaN(newPosition.x) || float.IsNaN(newPosition.y))
                {
                    Stop();
                    Debug.LogError("NaN detected in MoveBird");
                    yield break;
                }
                transform.position = newPosition;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            if (float.IsNaN(endPos.x) || float.IsNaN(endPos.y))
            {
                Stop();
                Debug.LogError("NaN detected in MoveBird");
                yield break;
            }
            transform.position = endPos;

            CalculateVelocity();
        }

        Rebond();
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


    public void CalculateTrajectorySpring_recur(Vector2 startPosition, float angle, float vitesseInitiale)
    {

        float radianAngle = (angle + 180f) * Mathf.Deg2Rad;

        currentLambda.x = vitesseInitiale * Mathf.Cos(radianAngle);
        currentLambda.y = vitesseInitiale * Mathf.Sin(radianAngle);

        Recurrence(startPosition);
    }

    public void TrajectoryContinuity(Vector2 startPosition, bool jump = false)
    {
        if (jump)
        {
            currentLambda.y = Mathf.Abs(currentLambda.y);
        }

        

        Recurrence(startPosition);
    }


    private void Recurrence(Vector2 startPos)
    {
        Vector2[] temp = new Vector2[pointsCount];
        Vector2[] lambdaTemp = new Vector2[pointsCount];

        float x = startPos.x;
        float y = startPos.y;
        temp[0] = new Vector2(x, y);


        for (int i = 1; i < pointsCount; i++)
        {
            x += currentLambda.x * timeStep;
            y += currentLambda.y * timeStep;

            if (y <= floorY)
            {
                Vector2 previousPoint = temp[i-1];


                float t = (floorY - previousPoint.y) / (y - previousPoint.y);
                float intersectX = previousPoint.x + t * (x - previousPoint.x);

                t_impact = (t) * timeStep;

                Vector2 intersectionPoint = new Vector2(intersectX, floorY);


                temp[i] = intersectionPoint;


                currentLambda.x += (-f2 * currentLambda.x * timeStep);
                currentLambda.y += (-(gravity + f2 * currentLambda.y) * timeStep);

                lambdaTemp[i] = new Vector2(currentLambda.x, currentLambda.y);



                tempListPosBird = new Vector2[i+1];
                lambdasValues = new Vector2[i + 1];

                for (int j = 0; j <= i; j++)
                {
                    tempListPosBird[j] = temp[j];
                    lambdasValues[j] = lambdaTemp[j];
                }

                return;
            }
                
            
            temp[i] = new Vector2(x, y);


            currentLambda.x += (-f2 * currentLambda.x * timeStep);
            currentLambda.y += (-(gravity + f2 * currentLambda.y) * timeStep);

            lambdaTemp[i] = new Vector2(currentLambda.x, currentLambda.y);
        }

        tempListPosBird = temp;
        lambdasValues = lambdaTemp;
    }



    void CalculateVelocity()
    {
        velocity = currentLambda.magnitude;
    }



    void InverseDirectionY()
    {
        currentLambda.y = -currentLambda.y;
    }
    void InverseDirectionX()
    {
        currentLambda.x = -currentLambda.x;
    }

    void Rebond()
    {
        powerUsed = true;
        InverseDirectionY();

        if(rebondCount * .05f < reboundFactor.y)
            rebondCount++;

        currentLambda.x *= reboundFactor.x;
        currentLambda.y = currentLambda.y * (reboundFactor.y - (rebondCount * 0.05f));

        if(currentLambda.y <= 1 && currentLambda.x <= 1)
        {
            Stop();
        }
    }



    public void BlockImpactHorizontal()
    {
        InverseDirectionX();
    }

    public void BlockImpactVertical()
    {
        InverseDirectionY();
    }




    public void Stop(bool fromPlayerClass = false)
    {
        if(stopped)
            return;

        canvasMain.DisplayPower(false);

        Debug.Log("Stopped");
        stopped = true;

        currentLambda = new Vector2(0, 0);
        GameManager.GetManager().StopWaitingCorout();

        if(moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = null;

        if(!fromPlayerClass)
            Player.Instance().NextBird();
    }




    private void DoubleJump()
    {
        TrajectoryContinuity(transform.position, true);
        Launch();
    }

    private void Speed()
    {
        acceleration = new Vector2(3, 2);

        currentLambda *= acceleration;


        TrajectoryContinuity(transform.position);
        Launch();
    }
}