using UnityEngine;

namespace ScriptsJ
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;

        private void Start()
        {
            if(mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            ViewCamera();
        }
        /*Rota el objeto en el eje Y contal de ver y esta apuntando a la camara
         *Util para la barra de vida que siempre se muestre al jugador
         */
        private void ViewCamera()
        {
            if (mainCamera != null)
            {
                Vector3 targetPosition = mainCamera.transform.position;
                targetPosition.y = transform.position.y;
                transform.LookAt(targetPosition);
            }
        }
    }
}
