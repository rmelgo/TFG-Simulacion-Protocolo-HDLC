using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulador_HDLC
{
    public class Protocolo
    {
        private int tamañoVentana;
        private int numeroMaximoTramasErroneasConsecutivasPermitidas;
        private int timeoutCommand;
        private int timeoutTramaI;
        private int timeoutRequest;

        public Protocolo() //Constructor sin argumentos que da valores por defecto al tamaño de la ventana, al número máximo de tramas erróneas consecutivas permitidas y a los distintos timeouts
        {
            tamañoVentana = 7;
            numeroMaximoTramasErroneasConsecutivasPermitidas = 3;
            timeoutCommand = 20000;
            timeoutTramaI = 20000;
            timeoutRequest = 20000;
        }

        public int TamañoVentana //Propiedad que permite obtener y definir el valor del tamaño de la ventana
        {
            get
            {
                return tamañoVentana;
            }
            set
            {
                tamañoVentana = value;
            }
        }

        public int NumeroMaximoTramasErroneasConsecutivasPermitidas //Propiedad que permite obtener y definir el valor del número máximo de tramas erróneas consecutivas permitidas
        {
            get
            {
                return numeroMaximoTramasErroneasConsecutivasPermitidas;
            }
            set
            {
                numeroMaximoTramasErroneasConsecutivasPermitidas = value;
            }
        }

        public int TimeoutCommand //Propiedad que permite obtener y definir el valor del timeout ante command
        {
            get
            {
                return timeoutCommand;
            }
            set
            {
                timeoutCommand = value;
            }
        }

        public int TimeoutTramaI //Propiedad que permite obtener y definir el valor del timeout ante trama I
        {
            get
            {
                return timeoutTramaI;
            }
            set
            {
                timeoutTramaI = value;
            }
        }

        public int TimeoutRequest //Propiedad que permite obtener y definir el valor del timeout ante request
        {
            get
            {
                return timeoutRequest;
            }
            set
            {
                timeoutRequest = value;
            }
        }

    }
}
