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
    private bool isOnCooldown = false; // Indica si el portal está en cooldow

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Rigidbody2D ballRb = collider.GetComponent<Rigidbody2D>();

        if (isOnCooldown && collider.CompareTag("Bola"))
        {
            StartCoroutine(CooldownCoroutine());
        }
        
        if (isOnCooldown || !collider.CompareTag("Bola"))
            return;

        // Teletransporta la bola
        if (ballRb != null && linkedPortalObject != null)
        {
            TeleportBall(ballRb);
        }
    }

    private void TeleportBall(Rigidbody2D ballRb)
    {
        ballRb.position = linkedPortalObject.transform.position; // Teletransporta la bola
        Portal linkedPortal = linkedPortalObject.GetComponent<Portal>();
        linkedPortal.isOnCooldown = true; // Activa el cooldown
    }

    private IEnumerator CooldownCoroutine()
    {
        // Espera el tiempo del cooldown
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false; // Termina el cooldown
    }
}