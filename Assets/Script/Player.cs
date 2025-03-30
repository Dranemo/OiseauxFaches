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
        lineRenderer.positionCount = bird.pointsCount;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(isLaunched)
            {
                Power();
            }
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

        bird.CalculateTrajectorySpring(start_pos, angle, bird.VitesseInitiale(Vector2.Distance(end_pos, start_pos), angle));
        DrawLine();

        Debug.Log("Angle : " + angle);
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
        bird.distance = Vector2.Distance(end_pos, start_pos);

        bird.CalculateTrajectorySpring(start_pos, angle, bird.VitesseInitiale(bird.distance, angle));
        RemoveLine();


        bird.Launch();
        gameManager.canSpring = false;
        gameManager.camFollowBird = true;

        isLaunched = true;
    }




    
    // ------------------------------------------------------------------------------------------------------------------ //
    // ------------------------------------------------------ Render ---------------------------------------------------- //
    // ------------------------------------------------------------------------------------------------------------------ //

    // Draw trajectoire
    private void DrawLine()
    {
        lineRenderer.enabled = true;

        lineRenderer.positionCount = bird.tempListPosBird.Length;
        Vector3[] positions = System.Array.ConvertAll(bird.tempListPosBird, p => (Vector3)p);
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
