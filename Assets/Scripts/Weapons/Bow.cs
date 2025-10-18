using UnityEngine;

namespace Scripts
{
    /*El Bow hereda de Weapon, las caracteristicas principales*/
    public class Bow : Weapon
    {
        [SerializeField] private Arrow arrowPrefab;
        [SerializeField] private Transform pointer;
        private float maxDistance = 500;
        
        /*Se sobre escribe el metodo atacar para realizar el disparo*/
        public override void Attack()
        {
            base.Attack();
            CreateArrow();
        }

        /*Crea y configura la flecha, desde la posición y la rotacion al puntero que se tiene como referencia a donde se quiere disparar*/
        public void CreateArrow()
        {
            Vector3 direction = pointer.position - transform.position;
            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            GameObject newTarget = Instantiate(new GameObject("TargetObject"), transform.position + direction * maxDistance, Quaternion.identity);
            Debug.DrawLine(transform.position, newTarget.transform.position, Color.red, 5f);
            Arrow newArrow = Instantiate(arrowPrefab, transform.position, targetRotation);
            newArrow.InitArrow(damage, target, newTarget);
        }
    }
}
