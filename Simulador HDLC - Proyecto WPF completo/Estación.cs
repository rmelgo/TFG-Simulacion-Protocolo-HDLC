using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulador_HDLC
{
    class Estación
    {
        private String nombreEstacion;
        private String situacionEstacion;
        private int numeroSecuencia;
        private int numeroTramaEsperada;

        public Estación() //Constructor sin argumentos que da valores por defecto al nombre de la estación (NS) y a los números de secuencia y tramas esperada (NR)
        {
            nombreEstacion = "A";
            numeroSecuencia = 0;
            numeroTramaEsperada = 0;
        }

        public String NombreEstacion //Propiedad que permite obtener y definir el valor del nombre de la estación
        {
            get
            {
                return nombreEstacion;
            }
            set
            {
                nombreEstacion = value;
            }
        }

        public String NombreEstacionContraria //Propiedad que permite obtener el valor del nombre de la estación situada en el extremo contrario
        {
            get
            {
                if (nombreEstacion.EndsWith("A"))
                {
                    return "Estación B";
                }
                else 
                {
                    return "Estación A";
                }
            }
        }

        public String SituacionEstacion //Propiedad que permite obtener y definir el valor de la situación de la estación
        {
            get
            {
                return situacionEstacion;
            }
            set
            {
                situacionEstacion = value;
            }
        }

        public int NumeroSecuencia //Propiedad que permite obtener y definir el valor del número de secuencia (NS)
        {
            get
            {
                return numeroSecuencia;
            }
            set
            {
                numeroSecuencia = value;
            }
        }

        public int NumeroTramaEsperada //Propiedad que permite obtener y definir el valor del número de trama esperada (NS)
        {
            get
            {
                return numeroTramaEsperada;
            }
            set
            {
                numeroTramaEsperada = value;
            }
        }
    }
}
