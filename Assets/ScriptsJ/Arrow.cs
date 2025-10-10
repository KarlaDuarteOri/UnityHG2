using UnityEngine;

namespace ScriptsJ
{
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private float velocity;
        private int arrowDamage;
        private string targetTodamage = "Enemy";
        /*Apenas empieza y para que no vea muchos objeto en escena lo borramos despues de 5 segundos, ya que se desplazara
         *casi infinitamente o hasta que choque con un enemigo
         */
        private void Start()
        {
            Destroy(gameObject, 5f);
        }

        private void Update()
        {
            MoveArrow();
        }
        /*Inicializamos la flecha asignandole un nuevo daño y un objetivo*/
        public void InitArrow(int damage, string target)
        {
            arrowDamage = damage;
            targetTodamage = target;
        }
        /*Desplaza la flecha segun su direccion dada al ser creado*/
        public void MoveArrow()
        {
            Vector3 direction = transform.forward * velocity * Time.deltaTime;
            transform.Translate(direction);
        }
        /*Detectamos al enemigo mediante un tag="Enemy", asu vez para evitar errores corroboramos que tenga el script Health
         *para quitarle vida
         */
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(targetTodamage))
            {
                Health healthTarget = other.GetComponent<Health>();
                if (healthTarget != null)
                {
                    healthTarget.DecrementHealth(arrowDamage);
                    Debug.Log("Damage: " + arrowDamage + " with arrow");
                    Destroy(gameObject);
                }
            }
        }
    }
}
