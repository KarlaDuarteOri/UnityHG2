using UnityEngine;

namespace ScriptsJ
{
    /*El Bow hereda de Weapon, las caracteristicas principales*/
    public class Bow : Weapon
    {
        [SerializeField] private Arrow arrowPrefab;
        [SerializeField] private Transform pointer;
        /*Se sobre escribe el metodo atacar para realizar el disparo*/
        public override void Attack()
        {
            base.Attack();
            CreateArrow();
        }

        /*Crea y configura la flecha, desde la posición y la rotacion al puntero que se tiene como referencia a donde se quiere disparar*/
        public void CreateArrow()
        {
            Arrow newArrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            newArrow.transform.LookAt(pointer);
            newArrow.InitArrow(damage, target);
        }
    }
}

