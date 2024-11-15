using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    // Esto del update se hace porque cuando pasamos de nivel el GameManager pierde el scoreBoardText y el playerUsernameField
    // Entonces en caso de que necesite actualizarse cuando le decimos, volvera a cargar esas variables y actualizar el score board
    private void Update()
    {
        if (!GameManager.Instance.getNeedUpdate())
            return;
        
        GameManager.Instance.UpdateTop5Score();
    }
}