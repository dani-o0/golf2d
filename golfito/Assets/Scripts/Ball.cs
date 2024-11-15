using UnityEngine;

public class BallController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CircleCollider2D cl;
    private Vector2 startPoint;
    public Vector2 forceDirection;
    private float forceMultiplier = 10f;
    private float minVelocity = 2.5f;
    private float maxVelocity = 15f;
    private float bounceDamping = 0.95f;

    private LineRenderer lineRenderer; // Línea de estiramiento
    private LineRenderer previewLineRenderer; // Línea de previsualización

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cl = GetComponent<CircleCollider2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D no encontrado en el GameObject.");
            return;
        }

        // Configurar línea de estiramiento
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        ConfigureLineRenderer(lineRenderer, Color.green, Color.red);

        // Crear un GameObject vacío como hijo para la línea de previsualización
        GameObject previewLineObject = new GameObject("PreviewLine");
        previewLineObject.transform.SetParent(transform); // Hacerlo hijo del objeto actual

        // Configurar línea de previsualización
        previewLineRenderer = previewLineObject.AddComponent<LineRenderer>();
        ConfigureLineRenderer(previewLineRenderer, new Color(255, 255, 255, 150), new Color(255, 255, 255, 150));
        previewLineRenderer.enabled = false; // Desactiva la previsualización al inicio
    }

    private void ConfigureLineRenderer(LineRenderer lr, Color startColor, Color endColor)
    {
        // Aumentar el grosor de la línea
        lr.startWidth = 0.1f; // Grosor inicial
        lr.endWidth = 0.1f; // Grosor final
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = startColor;
        lr.endColor = endColor;
        lr.positionCount = 2;
    }

    private void Update()
    {
        if (lineRenderer.enabled)
        {
            // Calcular el punto final basado en la dirección y la intensidad
            Vector2 endPoint = (Vector2)transform.position + (forceDirection.normalized * Mathf.Min(forceDirection.magnitude, maxVelocity / forceMultiplier));
            lineRenderer.SetPosition(0, transform.position); // Punto inicial (centro de la bola)
            lineRenderer.SetPosition(1, endPoint); // Punto final basado en la dirección

            // Actualizar la línea de previsualización
            Vector2 previewEndPoint = (Vector2)transform.position + (forceDirection.normalized * (maxVelocity / forceMultiplier));
            previewLineRenderer.SetPosition(0, transform.position); // Punto inicial (centro de la bola)
            previewLineRenderer.SetPosition(1, previewEndPoint); // Punto final basado en el límite de estiramiento
        }

        // Lógica para reducir la velocidad
        if (rb.velocity.magnitude < minVelocity && rb.velocity.magnitude > 0.1f)
        {
            rb.velocity *= bounceDamping;

            if (rb.velocity.magnitude < 0.1f)
            {
                rb.velocity = Vector2.zero;
            }
        }

        // Limitar la velocidad máxima
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }
    }

    private void OnMouseDown()
    {
        if (rb == null || rb.velocity != Vector2.zero)
            return;

        startPoint = transform.position;
        lineRenderer.enabled = true; // Activa el LineRenderer al comenzar a estirar
        previewLineRenderer.enabled = true; // Activa la línea de previsualización
    }

    private void OnMouseDrag()
    {
        if (rb == null || rb.velocity != Vector2.zero)
            return;

        // Actualiza la dirección mientras se arrastra
        Vector2 currentEndPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        forceDirection = startPoint - currentEndPoint; // Calcular la dirección
    }

    private void OnMouseUp()
    {
        if (rb == null || rb.velocity != Vector2.zero)
            return;

        // Obtener el punto final al soltar
        Vector2 endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        forceDirection = startPoint - endPoint;

        Vector2 force = forceDirection * forceMultiplier;

        // Limitar la fuerza máxima
        if (force.magnitude > maxVelocity)
        {
            force = force.normalized * maxVelocity;
        }

        // Aplicar fuerza a la pelota
        rb.AddForce(force, ForceMode2D.Impulse);
        lineRenderer.enabled = false; // Desactiva el LineRenderer después de soltar
        previewLineRenderer.enabled = false; // Desactiva la línea de previsualización
        
        // Decimos al GameManager que la bola ha sido lanzada
        GameManager.Instance.ThrowedBall();
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Porteria"))
        {
            // Obtener el radio de la portería y de la bola
            float porteriaRadius = collider.bounds.extents.x; // Asumiendo que es circular
            float ballRadius = cl.bounds.extents.x; // Radio de la bola

            // Calcular la distancia entre los centros
            float distanceBetweenCenters = Vector2.Distance(collider.bounds.center, cl.bounds.center);

            // Verificar si la bola está completamente dentro
            if (distanceBetweenCenters + ballRadius <= porteriaRadius)
            {
                rb.velocity = Vector2.zero; // Detener la bola
                GameManager.Instance.LoadNextLevel();
            }
        }
    }

}
