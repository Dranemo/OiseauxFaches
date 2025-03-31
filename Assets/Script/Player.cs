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
    private bool isLaunched = false;

    private LineRenderer lineRenderer;

    [SerializeField] private Bird bird;
    static private Player _instance;
    private GameManager gameManager;

    bool tooNear = false;
    [SerializeField] private float distanceMin = 0.2f;
    [SerializeField] private float distanceMax = 2f;


    [SerializeField] private AudioClip stretch;
    [SerializeField] private AudioClip release;
    [SerializeField] private AudioClip cancel;




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

        AudioSource.PlayClipAtPoint(stretch, Camera.main.transform.position);
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

        tooNear = false;
        if(direction.magnitude < distanceMin)
        {
            tooNear = true;
            Debug.Log("Too Near");
        }

        else if (direction.magnitude > distanceMax)
        {
            direction = direction.normalized * distanceMax;
            end_pos = start_pos + direction;
        }



        bird.CalculateTrajectorySpring_recur(start_pos, angle, bird.VitesseInitiale(Vector2.Distance(end_pos, start_pos), angle));
        DrawLine();
        bird.transform.position = end_pos;
    }

    // Fin du drag + Launch de l'oiseau
    public void OnEndDrag(PointerEventData eventData)
    {
        if(tooNear)
        {
            Cancel();
        }


        if (canceled)
        {
            canceled = false;
            return;
        }



        end_pos = Camera.main.ScreenToWorldPoint(eventData.position);



        Vector2 direction = end_pos - start_pos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (direction.magnitude > distanceMax)
        {
            direction = direction.normalized * distanceMax;
            end_pos = start_pos + direction;
        }

        float distance = Vector2.Distance(end_pos, start_pos);

        bird.CalculateTrajectorySpring_recur(start_pos, angle, bird.VitesseInitiale(distance, angle));
        RemoveLine();


        bird.Launch();
        gameManager.canSpring = false;
        gameManager.camFollowBird = true;

        isLaunched = true;

        AudioSource.PlayClipAtPoint(release, Camera.main.transform.position);
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

        AudioSource.PlayClipAtPoint(cancel, Camera.main.transform.position);
    }

    public void NextBird()
    {
        bird.Stop(true);

        gameManager.NextBird();
        isLaunched = false;
    }


    public void SetBird(Bird _bird)
    {
        bird = _bird;
    }
}
