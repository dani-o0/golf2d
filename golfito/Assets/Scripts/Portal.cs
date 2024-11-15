using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameObject linkedPortalObject; // El otro portal al que teletransportará
    [SerializeField] private float cooldownTime = 1f; // Tiempo de espera entre teletransportes (en segundos)
    private bool isOnCooldown = false; // Indica si el portal está en cooldow
    
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Obtenemos el objeto que ha colisionado con el portal
        Rigidbody2D ballRb = collider.GetComponent<Rigidbody2D>();
        
        // Comprobacion del coldown para dar un marjen para que salga la bola del portal y que no se teletransporte otra vez
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

    // Metodo para teletransportar la bola hacia el portal
    private void TeleportBall(Rigidbody2D ballRb)
    {
        // Cambiamos la posicion de la bola a la del portal
        ballRb.position = linkedPortalObject.transform.position;
        Portal linkedPortal = linkedPortalObject.GetComponent<Portal>();
        // Activamos el cooldown
        linkedPortal.isOnCooldown = true;
    }

    private IEnumerator CooldownCoroutine()
    {
        // Espera el tiempo del cooldown
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false; // Termina el cooldown
    }
}