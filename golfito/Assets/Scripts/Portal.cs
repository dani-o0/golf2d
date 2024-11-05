using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Transform linkedPortal; // El otro portal al que teletransportará
    [SerializeField] private float cooldownTime = 1.0f; // Tiempo de espera para evitar bucles

    private bool isOnCooldown = false; // Indica si el portal está en cooldown

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Verifica si el objeto que entra en el portal es la bola
        if (collider.CompareTag("Bola") && !isOnCooldown)
        {
            // Teletransporta la bola al portal vinculado
            Rigidbody2D ballRb = collider.GetComponent<Rigidbody2D>();

            if (ballRb != null && linkedPortal != null)
            {
                StartCoroutine(TeleportBall(ballRb));
            }
        }
    }

    private IEnumerator TeleportBall(Rigidbody2D ballRb)
    {
        isOnCooldown = true; // Activa el cooldown para este portal
        ballRb.position = linkedPortal.position; // Teletransporta la bola al portal vinculado

        // Opcional: Ajusta la velocidad de la bola después de teletransportarse
        ballRb.velocity = ballRb.velocity * 0.8f; // Reduce un poco la velocidad al salir

        yield return new WaitForSeconds(cooldownTime);

        // Desactiva el cooldown para permitir nuevos teletransportes
        isOnCooldown = false;
    }
}