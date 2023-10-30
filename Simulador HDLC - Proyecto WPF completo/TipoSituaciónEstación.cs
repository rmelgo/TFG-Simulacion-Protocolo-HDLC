using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulador_HDLC
{
    class TipoSituaciónEstación
    {
        //Se definen los distintos tipos de situaciones para la estación, utilizando constantes
        public const string Desconectado = "Desconectado";
        public const string Inicio_conexión = "Inicio conexión";
        public const string Conectado = "Conectado";
        public const string Inicio_desconexión = "Inicio desconexión";
        public const string Excepción = "Excepción";
    }
}
