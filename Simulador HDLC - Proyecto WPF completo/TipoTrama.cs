using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulador_HDLC
{
    class TipoTrama
    {
        //Se definen los distintos tipos de tramas, utilizando constantes
        public const string Informacion = "Información (I)";
        public const string Receptor_preparado = "Receptor preparado (RR)";
        public const string Receptor_no_preparado = "Receptor no preparado (RNR)";
        public const string Rechazo = "Rechazo (REJ)";
        public const string Rechazo_selectivo = "Rechazo selectivo (SREJ)";
        public const string Solicitud_conexion = "Solicitud de conexión (SABM)";
        public const string Solicitud_desconexion = "Solicitud de desconexión (DISC)";
        public const string Asentimiento_no_numerado = "Asentimiento no numerado (UA)";
        public const string Modo_desconectado = "Modo desconectado (DM)";
        public const string Rechazo_trama = "Rechazo de trama (FRMR)";
    }
}
