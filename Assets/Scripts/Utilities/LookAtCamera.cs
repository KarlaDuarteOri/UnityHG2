using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private new Camera camera;

    private void Start()
    {
        if(camera == null)
        {
            camera = Camera.main;
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
        if (camera != null)
        {
            Vector3 targetPosition = camera.transform.position;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }
    }
}