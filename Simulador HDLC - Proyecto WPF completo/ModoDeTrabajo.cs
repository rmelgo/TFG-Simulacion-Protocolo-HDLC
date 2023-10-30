using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulador_HDLC
{
    public class ModoDeTrabajo
    { 
        private String tipoModoTrabajo;

        public ModoDeTrabajo() //Constructor sin argumentos que da valores por defecto al tipo de modo de trabajo
        {
            tipoModoTrabajo = "Semiautomático";
        }

        public String TipoModoDeTrabajo //Propiedad que permite obtener y definir el valor del tipo de modo de trabajo
        {
            get
            {
                return tipoModoTrabajo;
            }
            set
            {
                tipoModoTrabajo = value;
            }
        }  
    }
}
