using UnityEngine;

public class SpeedPlat : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField] private float speedBoost = 10f; // Intensidad del aumento de velocidad
    [SerializeField] private Direction impulseDirection; // Selección de dirección desde el Inspector

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Bola"))
        {
            Rigidbody2D ballRb = collider.GetComponent<Rigidbody2D>();

            if (ballRb != null)
            {
                // Determinar la dirección del impulso basada en el valor seleccionado en el Dropdown
                Vector2 direction = GetImpulseDirection(impulseDirection);
                
                // Aumentar la velocidad de la bola en la dirección específica
                ballRb.velocity += direction * speedBoost * Time.deltaTime;
            }
        }
    }

    private Vector2 GetImpulseDirection(Direction selectedDirection)
    {
        switch (selectedDirection)
        {
            case Direction.Up:
                return new Vector2(0, 1);
            case Direction.Down:
                return new Vector2(0, -1);
            case Direction.Left:
                return new Vector2(-1, 0);
            case Direction.Right:
                return new Vector2(1, 0);
            default:
                return Vector2.zero; // Por defecto, sin impulso
        }
    }
}