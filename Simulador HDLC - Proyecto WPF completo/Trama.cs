using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Simulador_HDLC
{
    public class Trama
    {
        private float instanteTemporal;
        private String tipoTrama;
        private String direccionEstacion;
        private int numeroSecuencia;
        private int numeroTramaEsperada;
        private int infoBitSondeo;
        private String infoBitCommandRequest;
        private String infoTrama;
        private String crc;

        private String tipoTramaEstructuraControl;
        private String campoControl;
        private bool tramaIncorrecta;
        private bool tramaReenviada;
        private bool tramaConfirmada;

        public Trama() //Constructor sin argumentos que da valores por defecto (false) a los booleanos que marcan las tramas incorrectas, las tramas reenviadas y las tramas confirmadas
        {
            tramaIncorrecta = false;
            tramaReenviada = false;
            tramaConfirmada = false;
        }

        public Trama(String tipoTramaEstructuraControl) //Constructor que recibe como parámetro el tipo de trama (en función de estructura de control)
        {
            //Se da uan valor falso por defecto a los booleanos que marcan las tramas incorrectas, las tramas reenviadas y las tramas confirmadas
            tramaIncorrecta = false;
            tramaReenviada = false;
            tramaConfirmada = false;
            this.tipoTramaEstructuraControl = tipoTramaEstructuraControl; //Se asgina al atributo del tipo de trama estructura de control, el valor del tipo de trama estructura de control recibido como parámetro
        }

        public Trama(bool vacio) //Constructor que recibe como parámetro un booleano que define si la trama a crear debe estar vacia
        {
            if (vacio) //Si vacio es verdadera, significa que debera construirse una trama vacia. Una trama vacia tendrá atributos como el número de secuencia, número de trama esperada o el bit de sondeo a un valor imposible (-1)
            {
                NumeroSecuencia = -1;
                InfoBitSondeo = -1;
                NumeroTramaEsperada = -1;
                InstanteTemporal = -1;
            } //Con estos valores negativos, se puede diferenciar facilmente una trama normal de una trama vacia (el uso de tramas vacías es fundamental dentro del simulador)
        }

        public void generarCampoControl() //Función que genera el campo de control de la trama a partir de la información disponible sobre la trama
        {
            campoControl = "";

            switch (tipoTramaEstructuraControl) {
                case TipoTramaControl.Informacion: //Si la trama cuyo campo de control se desea calcular se trata de una trama de información
                    campoControl = campoControl + "0"; //Todas las tramas de información tienen el primer bit del campo de control a 0
                    string NS = Convert.ToString(numeroSecuencia, 2).PadLeft(3, '0'); //Se obtiene el valor del número de secuencia (NS) de la trama, se convierte a binario y se añaden los 0 necesarios a la izquierda hasta tener un número binario de 3 bits
                    campoControl = campoControl + NS; //Se añade el número de secuencia (NS) de la trama al campo de control (bits 2, 3 y 4)
                    string BitPF = Convert.ToString(infoBitSondeo, 2); //Se obtiene el valor del bit P/F, el cual se convierte a un valor binario
                    campoControl = campoControl + BitPF; //Se añade el valor del bit P/F de la trama al campo de control (bit 5)
                    string NR = Convert.ToString(numeroTramaEsperada, 2).PadLeft(3, '0'); //Se obtiene el valor del número de trama esperada (NR) de la trama, se convierte a binario y se añaden los 0 necesarios a la izquierda hasta tener un número binario de 3 bits
                    campoControl = campoControl + NR; //Se añade el número de trama esperada (NR) de la trama al campo de control (bits 6, 7 y 8)
                    break;
                case TipoTramaControl.Supervision: //Si la trama cuyo campo de control se desea calcular se trata de una trama de supervisión
                    campoControl = campoControl + "10"; //Todas las tramas de supervisión tienen el primer bit del campo de control a 1 y el segundo bit a 0

                    switch (tipoTrama)
                    {
                        case TipoTrama.Receptor_preparado: //Si la trama cuyo campo de control se desea calcular se trata de una trama de receptor preparado (RR)
                            campoControl = campoControl + "00"; //Se añade 00 al bit 3 y 4 del campo de control de la trama (00 hace referencia a la trama de receptor preparado (RR))
                            break;
                        case TipoTrama.Receptor_no_preparado: //Si la trama cuyo campo de control se desea calcular se trata de una trama de receptor no preparado (RNR)
                            campoControl = campoControl + "10"; //Se añade 10 al bit 3 y 4 del campo de control de la trama (10 hace referencia a la trama de receptor no preparado (RNR))
                            break;
                        case TipoTrama.Rechazo: //Si la trama cuyo campo de control se desea calcular se trata de una trama de rechazo (REJ)
                            campoControl = campoControl + "01"; //Se añade 01 al bit 3 y 4 del campo de control de la trama (01 hace referencia a la trama de rechazo (REJ))
                            break;
                        case TipoTrama.Rechazo_selectivo: //Si la trama cuyo campo de control se desea calcular se trata de una trama de rechazo selectivo (SREJ)
                            campoControl = campoControl + "11"; //Se añade 11 al bit 3 y 4 del campo de control de la trama (11 hace referencia a la trama de rechazo selectivo (SREJ))
                            break;
                    }

                    BitPF = Convert.ToString(infoBitSondeo, 2); //Se obtiene el valor del bit P/F, el cual se convierte a un valor binario
                    campoControl = campoControl + BitPF; //Se añade el valor del bit P/F de la trama al campo de control (bit 5)
                    NR = Convert.ToString(numeroTramaEsperada, 2).PadLeft(3, '0'); //Se obtiene el valor del número de trama esperada (NR) de la trama, se convierte a binario y se añaden los 0 necesarios a la izquierda hasta tener un número binario de 3 bits
                    campoControl = campoControl + NR; //Se añade el número de trama esperada (NR) de la trama al campo de control (bits 6, 7 y 8)
                    break;
                case TipoTramaControl.No_numerada: //Si la trama cuyo campo de control se desea calcular se trata de una trama no numerada
                    campoControl = campoControl + "11"; //Todas las tramas no numeradas tienen el primer bit del campo de control a 1 y el segundo bit a 1

                    switch (tipoTrama)
                    {
                        case TipoTrama.Solicitud_conexion: //Si la trama cuyo campo de control se desea calcular se trata de una trama de solicitud de conexión (SABM)
                            campoControl = campoControl + "11"; 
                            BitPF = Convert.ToString(infoBitSondeo, 2); //Se obtiene el valor del bit P/F, el cual se convierte a un valor binario
                            campoControl = campoControl + BitPF; //Se añade el valor del bit P/F de la trama al campo de control (bit 5)
                            campoControl = campoControl + "100"; //Se añade 11100 al bit 3, 4, 6, 7 y 8 del campo de control de la trama (11100 hace referencia a la trama de solicitud de conexión (SABM))
                            break;
                        case TipoTrama.Solicitud_desconexion: //Si la trama cuyo campo de control se desea calcular se trata de una trama de solicitud de desconexión (DISC)
                            campoControl = campoControl + "00";
                            BitPF = Convert.ToString(infoBitSondeo, 2); //Se obtiene el valor del bit P/F, el cual se convierte a un valor binario
                            campoControl = campoControl + BitPF; //Se añade el valor del bit P/F de la trama al campo de control (bit 5)
                            campoControl = campoControl + "010"; //Se añade 00010 al bit 3, 4, 6, 7 y 8 del campo de control de la trama (00010 hace referencia a la trama de solicitud de desconexión (DISC))
                            break;
                        case TipoTrama.Asentimiento_no_numerado: //Si la trama cuyo campo de control se desea calcular se trata de una trama de solicitud de conexión (UA)
                            campoControl = campoControl + "00";
                            BitPF = Convert.ToString(infoBitSondeo, 2); //Se obtiene el valor del bit P/F, el cual se convierte a un valor binario
                            campoControl = campoControl + BitPF; //Se añade el valor del bit P/F de la trama al campo de control (bit 5)
                            campoControl = campoControl + "110"; //Se añade 00110 al bit 3, 4, 6, 7 y 8 del campo de control de la trama (00110 hace referencia a la trama de asentimiento no numerado (UA))
                            break;
                        case TipoTrama.Modo_desconectado: //Si la trama cuyo campo de control se desea calcular se trata de una trama de solicitud de conexión (DM)
                            campoControl = campoControl + "11";
                            BitPF = Convert.ToString(infoBitSondeo, 2); //Se obtiene el valor del bit P/F, el cual se convierte a un valor binario
                            campoControl = campoControl + BitPF; //Se añade el valor del bit P/F de la trama al campo de control (bit 5)
                            campoControl = campoControl + "000"; //Se añade 11000 al bit 3, 4, 6, 7 y 8 del campo de control de la trama (11000 hace referencia a la trama de modo desconectado (DM))
                            break;
                        case TipoTrama.Rechazo_trama: //Si la trama cuyo campo de control se desea calcular se trata de una trama de rechazo de trama (FRMR)
                            campoControl = campoControl + "10";
                            BitPF = Convert.ToString(infoBitSondeo, 2); //Se obtiene el valor del bit P/F, el cual se convierte a un valor binario
                            campoControl = campoControl + BitPF; //Se añade el valor del bit P/F de la trama al campo de control (bit 5)
                            campoControl = campoControl + "001"; //Se añade 10001 al bit 3, 4, 6, 7 y 8 del campo de control de la trama (10001 hace referencia a la trama de rechazo de trama (FRMR))
                            break;
                    }
                    break;               
            }          
        }

        public float InstanteTemporal //Propiedad que permite obtener y definir el valor del instante temporal en el que se envía/recibe la trama
        {
            get
            {
                return instanteTemporal;
            }
            set
            {
                instanteTemporal = value;
            }
        }

        public String InstanteTemporalTabla //Propiedad que permite obtener el valor del instante temporal en el que se envía/recibe la trama para su represetación en la tabla correspondiente (nos interesa que en las tramas vacias, este campo se devuelva vacío)
        {
            get
            {
                if (instanteTemporal < 0) //Si el instante temporal en el que se envia/recibe la trama es negativo, entonces no se devuelve nada
                {
                    return "";
                }
                else //En caso contrario, se devuelve el instante temporal en el que se envío/recibío la trama
                {
                    return instanteTemporal.ToString("0.000"); //Se representa el instante temporal con un formato de 3 decimales
                }
            }
        }

        public String TipoDeTrama //Propiedad que permite obtener y definir el valor del tipo de trama
        {
            get
            {
                return tipoTrama;
            }
            set
            {
                tipoTrama = value;
            }
        }

        public String TipoDeTramaTabla //Propiedad que permite obtener el valor del tipo de trama para su represetación en la tabla correspondiente (nos interesa que en las tramas vacias, este campo se devuelva vacío)
        {
            get
            {
                if (tramaIncorrecta) //Si la trama es incorrecta, en vez de devolver el tipo de trama, se devuelve ERR
                {
                    return "ERR";
                }
                else if (tipoTrama != null) //Si la trama tiene asociado algun tipo, se obtiene dicho tipo (en formato abreviado) y se devuelve dicho tipo
                { 
                    String[] temp = tipoTrama.Split('(');
                    String tipoTramaTabla = temp[1].Remove(temp[1].Length - 1, 1);

                    return tipoTramaTabla;
                }
                else  //Si no se cumple ninguno de los casos anteriores, se asume que la trama esta vacia por lo que no se devuelve nada
                {
                    return "";
                }
            }
        }

        public String DireccionEstacion //Propiedad que permite obtener y definir el valor de la dirección de la estación de la trama
        {
            get
            {
                return direccionEstacion;
            }
            set
            {
                direccionEstacion = value;
            }
        }

        public int NumeroSecuencia //Propiedad que permite obtener y definir el valor del número de secuencia (NS) de la trama 
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

        public String NumeroSecuenciaTabla //Propiedad que permite obtener el valor del número de secuencia (NS) de la trama para su represetación en la tabla correspondiente (nos interesa que en las tramas vacias, este campo se devuelva vacío)
        {
            get
            {
                if (numeroSecuencia < 0 || tramaIncorrecta) //Si el número de secuencia (NS) de la trama es negativo o la trama es incorrecta, entonces no se devuelve nada
                {
                    return "";
                }
                else //En caso contrario, se devuelve el número de secuencia (NS) de la trama
                {
                    return numeroSecuencia.ToString();
                }            
            }
        }

        public int NumeroTramaEsperada //Propiedad que permite obtener y definir el valor del número de trama esperada (NR) de la trama 
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

        public String NumeroTramaEsperadaTabla //Propiedad que permite obtener el valor del número de trama esperada (NR) de la trama para su represetación en la tabla correspondiente (nos interesa que en las tramas vacias, este campo se devuelva vacío)
        {
            get
            {
                if (numeroTramaEsperada < 0 || tramaIncorrecta) //Si el número de trama esperada (NR) de la trama es negativo o la trama es incorrecta, entonces no se devuelve nada
                {
                    return "";
                }
                else //En caso contrario, se devuelve el número de trama esperada (NR) de la trama
                {
                    return numeroTramaEsperada.ToString();
                }
            }
        }


        public int InfoBitSondeo //Propiedad que permite obtener y definir el valor del bit P/F de la trama 
        {
            get
            {
                return infoBitSondeo;
            }
            set
            {
                infoBitSondeo = value;
            }
        }

        public String InfoBitSondeoTabla //Propiedad que permite obtener el valor del bit P/F de la trama para su represetación en la tabla correspondiente (nos interesa que en las tramas vacias, este campo se devuelva vacío)
        {
            get
            {
                if (infoBitSondeo < 0 || tramaIncorrecta) //Si el bit P/F de la trama es negativo o la trama es incorrecta, entonces no se devuelve nada
                {
                    return "";
                }
                else //En caso contrario, se devuelve el número de trama esperada (NR) de la trama
                {
                    if (infoBitSondeo == 1) //Si el bit P/F de la trama esta activado
                    {
                        if (infoBitCommandRequest == "C") //Si la trama es un comando (C), se devuelve una P
                        {
                            return "P";
                        }
                        else //Si la trama es una respuesta (R), se devuelve una F
                        {
                            return "F";
                        }
                    }
                    else //Si el bit P/F de la trama no esta activado, no se devuelve nada
                    {
                        return "";
                    }
                }
            }
        }

        public String InfoBitCommandRequest //Propiedad que permite obtener y definir el valor del bit C/R de la trama 
        {
            get
            {
                return infoBitCommandRequest;
            }
            set
            {
                infoBitCommandRequest = value;
            }
        }

        public String InfoTrama //Propiedad que permite obtener y definir el valor de la información contenida en la propia trama
        {
            get
            {
                return infoTrama;
            }
            set
            {
                infoTrama = value;
            }
        }

        public String CRC //Propiedad que permite obtener y definir el valor del Código de Redundancia Cíclica (CRC) de la trama
        {
            get
            {
                return crc;
            }
            set
            {
                crc = value;
            }
        }

        public String TipoTramaEstructuraControl //Propiedad que permite obtener y definir el valor del tipo de trama en función de su estructura de control
        {
            get
            {
                return tipoTramaEstructuraControl;
            }
            set
            {
                tipoTramaEstructuraControl = value;
            }
        }

        public bool TramaIncorrecta //Propiedad que permite obtener y definir el valor de la variable tramaIncorrecta (esta variable se utiliza para marcar las tramas erróneas)
        {
            get
            {
                return tramaIncorrecta;
            }
            set
            {
                tramaIncorrecta = value;
            }
        }

        public bool TramaReenviada //Propiedad que permite obtener y definir el valor de la variable tramaReenviada (esta variable se utiliza para marcar las tramas reenviadas)
        {
            get
            {
                return tramaReenviada;
            }
            set
            {
                tramaReenviada = value;
            }
        }

        public bool TramaConfirmada //Propiedad que permite obtener y definir el valor de la variable tramaConfirmada (esta variable se utiliza para marcar las tramas de información confirmadas)
        {
            get
            {
                return tramaConfirmada;
            }
            set
            {
                tramaConfirmada = value;
            }
        }

        public String CampoControl //Propiedad que permite obtener y definir el valor del campo de control de la trama
        {
            get
            {
                return campoControl;
            }
            set
            {
                campoControl = value;
            }
        }

        public String CampoEstacionByte //Propiedad que permite obtener el valor de la dirección de la estación de la trama en binario para su represetación en la ventana correspondiente de visualización de detalle de la trama
        {
            get
            {
                if (direccionEstacion != null) //Si la dirección de la estación no es un valor nulo
                {
                    //Si la trama tiene en el campo de dirección la estación A, el valor binario de 8 bits asociado sera de 1 (00000001) y si la trama tiene en el campo de dirección la estación B, el valor binario de 8 bits asociado sera de 2 (00000010)
                    if (direccionEstacion.EndsWith("A"))
                    {
                        return Convert.ToString(1, 2).PadLeft(8, '0'); //Se convierte el número 1 a binario y posteriormente se añadiran los 0 necesarios a la izquierda para tener así un número binario de 8 bits
                    }
                    else
                    {
                        return Convert.ToString(2, 2).PadLeft(8, '0'); //Se convierte el número 2 a binario y posteriormente se añadiran los 0 necesarios a la izquierda para tener así un número binario de 8 bits
                    }
                }
                else //Si la dirección de la estación es un valor nulo, se devuelve null
                {
                    return null;
                }
            }
        }
    }
}
