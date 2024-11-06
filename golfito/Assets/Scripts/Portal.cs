using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    
    [SerializeField] private GameObject linkedPortalObject; // El otro portal al que teletransportará
    [SerializeField] private float cooldownTime = 1f; // Tiempo de espera entre teletransportes (en segundos)
    [SerializeField] private Direction impulseDirection; // Selección de dirección desde el Inspector
    private bool isOnCooldown = false; // Indica si el portal está en cooldown

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (isOnCooldown && collider.CompareTag("Bola"))
        {
            StartCoroutine(CooldownCoroutine());
        }
        
        if (isOnCooldown || !collider.CompareTag("Bola"))
            return;

        // Teletransporta la bola
        Rigidbody2D ballRb = collider.GetComponent<Rigidbody2D>();
        if (ballRb != null && linkedPortalObject != null)
        {
            TeleportBall(ballRb);
        }
    }

    private void TeleportBall(Rigidbody2D ballRb)
    {
        ballRb.position = linkedPortalObject.transform.position; // Teletransporta la bola
        Vector2 direction = GetImpulseDirection(impulseDirection);
        ballRb.velocity += direction * Time.deltaTime;
        linkedPortalObject.GetComponent<Portal>().isOnCooldown = true; // Activa el cooldown
    }

    private IEnumerator CooldownCoroutine()
    {
        // Espera el tiempo del cooldown
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false; // Termina el cooldown
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