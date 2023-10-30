using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulador_HDLC
{
    class TipoTramaControl
    {
        //Se definen los distintos tipos de tramas en función de la estructura del campo de control, utilizando constantes
        public const string Informacion = "Información";
        public const string Supervision = "Supervisión";
        public const string No_numerada = "No numerada";
    }
}
