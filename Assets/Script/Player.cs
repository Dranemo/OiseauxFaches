using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Player : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{


    // --------------------------------------------------------------------------------------------- //
    // --------------------------------------------------------------------------------------------- //
    // ----------------------------------------- Variables ----------------------------------------- //
    // --------------------------------------------------------------------------------------------- //
    // --------------------------------------------------------------------------------------------- //
    // Positions spring
    private Vector2 start_pos;
    private Vector2 end_pos;


    // variables environnement
    private float gravity = 9.81f;
    private float k = 10;
    private float f2;


    // Liste des positions de la trajectoire
    public int pointsCount = 100;
    public float timeStep = 0.1f;
    Vector2[] listPosBird;


    // State
    private bool canceled = false;

    private LineRenderer lineRenderer;

    [SerializeField] private Bird bird;
    static private Player _instance;
    private GameManager gameManager;




    // --------------------------------------------------------------------------------------------- //
    // --------------------------------------------------------------------------------------------- //
    // ----------------------------------------- Fonctions ----------------------------------------- //
    // --------------------------------------------------------------------------------------------- //
    // --------------------------------------------------------------------------------------------- //



    public static Player Instance()
    {
        if(_instance == null)
        {
            _instance = new Player();
        }
        return _instance;
    }


    // ----------------------------------------------------------------------------------------------------------------- //
    // --------------------------------------------------- Unity ------------------------------------------------------- //
    // ----------------------------------------------------------------------------------------------------------------- //
    // Start is called before the first frame update

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    void Start()
    {
        gameManager = GameManager.GetManager();

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = pointsCount;


        f2 = 0.2f / bird.mass;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Power();
        }
        if (Input.GetKeyDown(KeyCode.Q) && gameManager.canSpring)
        {
            Cancel();
        }
        if (Input.GetKeyDown(KeyCode.N) && !gameManager.canSpring)
        {
            NextBird();
        }
    }


    // ------------------------------------------------------------------------------------------------------------------ //
    // ---------------------------------------------------- Drag -------------------------------------------------------- //
    // ------------------------------------------------------------------------------------------------------------------ //
    
    // Begin du drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!gameManager.canSpring)
            canceled = true;

        start_pos = transform.position;
        end_pos = start_pos;
    }

    // Calcul de l'angle, position et vitesse initiale + drawline + deplacement spring
    public void OnDrag(PointerEventData eventData)
    {
        if (canceled)
        {
            return;
        }


        end_pos = Camera.main.ScreenToWorldPoint(eventData.position);



        Vector2 direction = end_pos - start_pos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        CalculateTrajectorySpring(start_pos, angle, VitesseInitiale(Vector2.Distance(end_pos, start_pos), angle));
        DrawLine();

        bird.transform.position = end_pos;
    }

    // Fin du drag + Launch de l'oiseau
    public void OnEndDrag(PointerEventData eventData)
    {
        if (canceled)
        {
            canceled = false;
            return;
        }



        end_pos = Camera.main.ScreenToWorldPoint(eventData.position);



        Vector2 direction = end_pos - start_pos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        CalculateTrajectorySpring(start_pos, angle, VitesseInitiale(Vector2.Distance(end_pos, start_pos), angle));
        RemoveLine();


        bird.Launch(listPosBird.ToList<Vector2>());
        gameManager.canSpring = false;
        gameManager.camFollowBird = true;
    }




    // ----------------------------------------------------------------------------------------------------------------- //
    // ---------------------------------------------------- Calculs ---------------------------------------------------- //
    // ----------------------------------------------------------------------------------------------------------------- //


    // Calculate vitesse initiale
    private float VitesseInitiale(float distance, float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        float vitesse = distance * Mathf.Sqrt(k / bird.mass) * Mathf.Sqrt(1 - Mathf.Pow((bird.mass * gravity) / (distance * k) * Mathf.Sin(angleRad), 2));
        return vitesse * 2;
    }


    void CalculateTrajectorySpring(Vector2 startPosition, float angle, float vitesseInitiale)
    {
        listPosBird = new Vector2[pointsCount];
        float radianAngle = (angle + 180f) * Mathf.Deg2Rad;

        float lambdaX = vitesseInitiale * Mathf.Cos(radianAngle);
        float lambdaY = vitesseInitiale * Mathf.Sin(radianAngle) + gravity / f2;

        for (int i = 0; i < pointsCount; i++)
        {
            float t = i * timeStep;
            float x = startPosition.x + lambdaX / f2 * (1 - Mathf.Exp(-f2 *t));
            float y = startPosition.y + lambdaY / f2 * (1 - Mathf.Exp(-f2 * t)) - gravity/f2 * t;

            listPosBird[i] = new Vector2(x, y);
        }
    }



    
    // ------------------------------------------------------------------------------------------------------------------ //
    // ------------------------------------------------------ Render ---------------------------------------------------- //
    // ------------------------------------------------------------------------------------------------------------------ //

    // Draw trajectoire
    private void DrawLine()
    {
        lineRenderer.enabled = true;

        lineRenderer.positionCount = listPosBird.Length;
        Vector3[] positions = System.Array.ConvertAll(listPosBird, p => (Vector3)p);
        lineRenderer.SetPositions(positions);
    }

    // Remove trajectoire
    private void RemoveLine()
    {
        lineRenderer.enabled = false;
    }
    











    private void Power()
    {
        Debug.Log("Double Jump");
        bird.Power();
    }

    private void Cancel()
    {
        Debug.Log("Cancel");
        bird.transform.position = start_pos;
        RemoveLine();
        canceled = true;
    }

    private void NextBird()
    {
        Debug.Log("Next Bird");

        gameManager.NextBird();
    }


    public void SetBird(Bird _bird)
    {
        bird = _bird;
    }
}
