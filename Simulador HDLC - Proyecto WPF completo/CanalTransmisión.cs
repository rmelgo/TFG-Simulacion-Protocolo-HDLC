using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulador_HDLC
{
    public class CanalTransmisión
    {
        private int retardoCanal;
        private float tasaError;

        public CanalTransmisión() //Constructor sin argumentos que da valores por defecto al retardo del canal y a la tasa de error
        {
            retardoCanal = 1000;
            tasaError = 0;
        }

        public int RetardoCanal //Propiedad que permite obtener y definir el valor del retardo del canal
        {
            get
            {
                return retardoCanal;
            }
            set
            {
                retardoCanal = value;
            }
        }

        public float TasaError //Propiedad que permite obtener y definir el valor de la tasa de error
        {
            get
            {
                return tasaError;
            }
            set
            {
                tasaError = value;
            }
        }
    }
}
