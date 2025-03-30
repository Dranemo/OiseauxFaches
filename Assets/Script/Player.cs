using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Player : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 mouseInitialPos;
    private Vector2 mouseLastPos;

    Vector2[] listPosBird;
    private LineRenderer lineRenderer;
    public int pointsCount = 100; 
    public float timeStep = 0.1f;
    private bool isLaunched = false;

    public float gravity = 9.81f;
    private float m = 0.8f;
    private float k = 10;
    private float f2;

    [SerializeField] private Bird bird;


    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = pointsCount;
        f2 = 0.2f / m;

        bird.StartValues(m);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            DoubleJump();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        mouseInitialPos = Camera.main.ScreenToWorldPoint(eventData.position); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        lineRenderer.enabled = true;

        mouseLastPos = Camera.main.ScreenToWorldPoint(eventData.position);

        Vector2 direction = mouseLastPos - mouseInitialPos;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        DrawTrajectory(mouseInitialPos, angle, VitesseInitiale(Vector2.Distance(mouseLastPos, mouseInitialPos), angle));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        mouseLastPos = Camera.main.ScreenToWorldPoint(eventData.position);

        Vector2 direction = mouseLastPos - mouseInitialPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        DrawTrajectory(mouseInitialPos, angle, VitesseInitiale(Vector2.Distance(mouseLastPos, mouseInitialPos), angle));

        bird.Launch(listPosBird.ToList<Vector2>());

    }

    void DrawTrajectory(Vector2 startPosition, float angle, float vitesseInitiale)
    {
        lineRenderer.enabled = true;

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

        lineRenderer.positionCount = listPosBird.Length;
        Vector3[] positions = System.Array.ConvertAll(listPosBird, p => (Vector3)p);
        lineRenderer.SetPositions(positions);
    }

    private float VitesseInitiale(float distance, float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        float vitesse = distance * Mathf.Sqrt(k / m) * Mathf.Sqrt(1 - Mathf.Pow((m * gravity) / (distance * k) * Mathf.Sin(angleRad),2));
        return vitesse * 2;
    }

    private void DoubleJump()
    {
        Debug.Log("Double Jump");
    }
}
