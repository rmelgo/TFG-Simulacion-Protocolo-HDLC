using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Simulador_HDLC
{
    /// <summary>
    /// Lógica de interacción para VentanaVisualizaciónTrama.xaml
    /// </summary>
    public partial class VentanaVisualizaciónTrama : Window
    {
        public String direccionEstacion { get; set; }

        public String infoBitCommandRequest { get; set; }

        public int infoBitSondeo { get; set; }

        public int numeroTramaEsperada { get; set; }

        private String nombreEstacionActual;
        private Protocolo protocolo;
        private ObservableCollection<Trama> listaTramasEnviadas;
        private ObservableCollection<Trama> listaTramasRecibidas;
        private bool envio_automatico;
        private Canvas canvas_visible;

        //Constructor que recibe como parámetros el tipo de trama, la dirección de la estación, información del bit C/R, información del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo y la lista de tramas enviadas y recibidas por la estación propietaria de la instancia de la ventana 
        public VentanaVisualizaciónTrama(String tipoTrama, String direccionEstacion, String infoBitCommandRequest, int infoBitSondeo, int numeroTramaEsperada, String nombreEstacionActual, Protocolo protocolo, ObservableCollection<Trama> listaTramasEnviadas, ObservableCollection<Trama> listaTramasRecibidas, bool envio_automatico)
        {
            InitializeComponent();

            //Se almacena la información del nombre de la estación, el protocolo y la lista de tramas enviadas y recibidas por la estación propietaria de la instancia de la ventana 
            this.nombreEstacionActual = nombreEstacionActual;
            this.protocolo = protocolo;
            this.listaTramasEnviadas = listaTramasEnviadas;
            this.listaTramasRecibidas = listaTramasRecibidas;

            //Se muestran los valores asociados a la trama a enviar en la ventana
            mostrarValores(tipoTrama, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada);
            cargarValores(tipoTrama, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada);

            //Se ajusta la anchura y altura mínima y máxima al valor actual de la ventana para evitar el redimensinado de la ventana por parte del usuario
            this.MinWidth = this.MaxWidth = this.Width;
            this.MinHeight = this.MaxHeight = this.Height;

            this.envio_automatico = envio_automatico;
            Loaded += MainWindow_Loaded; //Se registra una manejadora para el evento Loaded de la ventana, en la que se cerrará la ventana si la envio_automatico es true
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (envio_automatico) //Cuando se cargue la ventana, si se desea realizar un envío automático, se cerrará la ventana devolviendo true
            {
                DialogResult = true;
            }
        }

        private void mostrarValores(String tipoTrama, String direccionEstacion, String infoBitCommandRequest, int infoBitSondeo, int numeroTramaEsperada)
        {
            //Se extrae el tipo de trama a representar y se coloca en la caja correspodiente
            CajaTipoTrama.Text = tipoTrama;

            //Se extrae la dirección de la estación en función de si la trama es un comando o es una respuesta
            if (infoBitCommandRequest == "C") //Si la trama es un comando, se coloca como dirección, la dirección de la estación a la que va dirigida la trama
            {
                CajaDireccionEstacion.Text = direccionEstacion;
            }
            else //Si la trama es una respuesta, se coloca como dirección, la dirección de la estación que origina la trama
            {
                CajaDireccionEstacion.Text = nombreEstacionActual;
            }

            //Se extrae la información básica de la trama (NR, bit C/R y bit P/F) y se colocan en las cajas correspondientes
            CajaBitCommandRequest.Text = infoBitCommandRequest;
            DesactivarComboBox(CajaNumeroSecuencia, BordeCajaNumeroSecuencia, Color.FromRgb(235, 235, 235)); 
            CajaBitSondeo.Text = infoBitSondeo.ToString(); 
            if (numeroTramaEsperada < 0) //Si el numero de trama esperada no tiene un valor válido, se desactiva la caja del número de trama esperada
            {
                DesactivarComboBox(CajaNumeroTramaEsperada, BordeCajaNumeroTramaEsperada, Color.FromRgb(235, 235, 235));
            }
            else //Si el numero de trama esperada tiene un valor válido, se muestra dicho valor en la caja correspodiente
            {
                CajaNumeroTramaEsperada.Text = numeroTramaEsperada.ToString();
            }

            if (tipoTrama == TipoTrama.Solicitud_conexion || tipoTrama == TipoTrama.Solicitud_desconexion)
            {
                CajaBitSondeo.IsEnabled = false;
            }

            //Se incluyen en el combobox, la lista de posibles valores para el bit C/R
            CajaBitCommandRequest.Items.Add("C");
            CajaBitCommandRequest.Items.Add("R");

            //Se incluyen en el combobox, la lista de posibles valores para el bit P/F
            CajaBitSondeo.Items.Add(0);
            CajaBitSondeo.Items.Add(1);

            for (int i = 0; i <= 7; i++) //Se incluyen en el combobox, la lista de posibles valores para el número de secuencia (NS)
            {
                CajaNumeroSecuencia.Items.Add(i);
            }

            for (int i = 0; i <= 7; i++) //Se incluyen en el combobox, la lista de posibles valores para el número de trama esperada (NR)
            {
                CajaNumeroTramaEsperada.Items.Add(i);
            }
        }

        private void DesactivarComboBox(ComboBox comboBox, Border bordeComboBox, Color color)
        {
            //Se desactiva el combobox recibido como parámetro y se cambia el color del borde de la caja al color pasado como parámetro
            comboBox.IsEnabled = false;
            bordeComboBox.Background = new SolidColorBrush(color);
            comboBox.IsEditable = false;
        }

        public void cargarValores(String tipoTrama, String direccionEstacion, String infoBitCommandRequest, int infoBitSondeo, int numeroTramaEsperada)
        {
            //Se cargan los valores de la información básica de la trama
            this.direccionEstacion = direccionEstacion;
            this.infoBitCommandRequest = infoBitCommandRequest;
            this.infoBitSondeo = infoBitSondeo;
            this.numeroTramaEsperada = numeroTramaEsperada;
        }

        private void CajaBitCommandRequest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Si el valor del bit C/R cambia, se debera actualizar el valor de la dirección de la estación y comprobar que la configuración de la trama actual es válida para alertar al usuario en el caso de que no sea válida dicha configuración
            if (e.AddedItems[0].ToString() == "R") //Si el valor del bit C/R cambia a R
            {
                //Si el valor del bit C/R cambia a R, se coloca en el campo de dirección, la dirección de la estación que realiza la respuesta
                CajaDireccionEstacion.Text = nombreEstacionActual;

                if (CajaTipoTrama.Text == TipoTrama.Solicitud_conexion) //Si la trama a enviar es de solicitud de conexión (SABM), se muestra el icono de advertencia y la descripción ya que una trama SABM no puede ser nunca una respuesta
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;          
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de solicitud de conexión (SABM) a modo de respuesta.\nLas tramas de solicitud de conexión (SABM) siempre son comandos.");
                }
                else if (CajaTipoTrama.Text == TipoTrama.Solicitud_desconexion) //Si la trama a enviar es de solicitud de desconexión (DISC), se muestra el icono de advertencia y la descripción ya que una trama DISC no puede ser nunca una respuesta
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de solicitud de desconexión (DISC) a modo de respuesta.\nLas tramas de solicitud de desconexión (DISC) siempre son comandos.");
                }
                else if (CajaTipoTrama.Text == TipoTrama.Asentimiento_no_numerado) //Si la trama a enviar es de asentimiento no numerado (UA), se oculta el icono de advertencia ya que una trama UA siempre es una respuesta
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
                else if (CajaTipoTrama.Text == TipoTrama.Modo_desconectado) //Si la trama a enviar es de modo desconectado (DM), se oculta el icono de advertencia ya que una trama DM siempre es una respuesta
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
                else if (CajaTipoTrama.Text == TipoTrama.Rechazo_trama) //Si la trama a enviar es de rechazo de trama (FRMR), se oculta el icono de advertencia ya que una trama FRMR siempre es una respuesta
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
                else if (CajaTipoTrama.Text == TipoTrama.Receptor_preparado) //Si la trama a enviar es de receptor preparado (RR), se muestra el icono de advertencia y la descripción siempre y cuando la trama RR no sea una respuesta a un comando con el bit P/F activado
                {
                    if (!ExisteComandoSinResponder()) //Si no existe ningún comando que este sin responder, entonces se mostrará el icono de advertencia y la descripción ya que no hay ningún comando al cual responder
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                        ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor preparado (RR) a modo de respuesta, ya que no existe ningun comando con el bit P/F al que responder");
                    }
                    else //Si existe algún comando que este sin responder
                    {
                        if (CajaBitSondeo.Text == "0") //Si el bit de sondeo esta a 0, entonces se mostrará el icono de advertencia y la descripción ya que no puede existir una respuesta sin el bit de sondeo activado
                        {
                            ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                            ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor preparado (RR) a modo de respuesta sin el bit P/F activado");
                        }
                        else //Si el bit de sondeo esta a 1, entonces se ocultará el icono de advertencia ya que la trama esta respondiendo a un comando anterior
                        {
                            ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                        }
                    }
                }
                else if (CajaTipoTrama.Text == TipoTrama.Receptor_no_preparado) //Si la trama a enviar es de receptor no preparado (RNR), se muestra el icono de advertencia y la descripción siempre y cuando la trama RNR no sea una respuesta a un comando con el bit P/F activado
                {
                    if (!ExisteComandoSinResponder()) //Si no existe ningún comando que este sin responder, entonces se mostrará el icono de advertencia y la descripción ya que no hay ningún comando al cual responder
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                        ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor no preparado (RNR) a modo de respuesta, ya que no existe ningun comando con el bit P/F al que responder");
                    }
                    else //Si existe algún comando que este sin responder
                    {
                        if (CajaBitSondeo.Text == "0") //Si el bit de sondeo esta a 0, entonces se mostrará el icono de advertencia y la descripción ya que no puede existir una respuesta sin el bit de sondeo activado
                        {
                            ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                            ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor no preparado (RNR) a modo de respuesta sin el bit P/F activado");
                        }
                        else //Si el bit de sondeo esta a 1, entonces se ocultará el icono de advertencia ya que la trama esta respondiendo a un comando anterior
                        {
                            ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                        }
                    }
                }
                else if (CajaTipoTrama.Text == TipoTrama.Rechazo) //Si la trama a enviar es de rechazo (REJ), se muestra el icono de advertencia y la descripción siempre y cuando la trama REJ no sea una respuesta a un comando con el bit P/F activado
                {
                    if (!ExisteComandoSinResponder()) //Si no existe ningún comando que este sin responder, entonces se mostrará el icono de advertencia y la descripción ya que no hay ningún comando al cual responder
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                        ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo (REJ) a modo de respuesta, ya que no existe ningun comando con el bit P/F al que responder");
                    }
                    else //Si existe algún comando que este sin responder
                    {
                        if (CajaBitSondeo.Text == "0") //Si el bit de sondeo esta a 0, entonces se mostrará el icono de advertencia y la descripción ya que no puede existir una respuesta sin el bit de sondeo activado
                        {
                            ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                            ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo (REJ) a modo de respuesta sin el bit P/F activado");
                        }
                        else //Si el bit de sondeo esta a 1, entonces se ocultará el icono de advertencia ya que la trama esta respondiendo a un comando anterior
                        {
                            ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                        }
                    }
                }
                else if (CajaTipoTrama.Text == TipoTrama.Rechazo_selectivo) //Si la trama a enviar es de rechazo selectivo (SREJ), se muestra el icono de advertencia y la descripción siempre y cuando la trama SREJ no sea una respuesta a un comando con el bit P/F activado
                {
                    if (!ExisteComandoSinResponder()) //Si no existe ningún comando que este sin responder, entonces se mostrará el icono de advertencia y la descripción ya que no hay ningún comando al cual responder
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                        ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo selectivo (SREJ) a modo de respuesta, ya que no existe ningun comando con el bit P/F al que responder");
                    }
                    else //Si existe algún comando que este sin responder
                    {
                        if (CajaBitSondeo.Text == "0") //Si el bit de sondeo esta a 0, entonces se mostrará el icono de advertencia y la descripción ya que no puede existir una respuesta sin el bit de sondeo activado
                        {
                            ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                            ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo selectivo (SREJ) a modo de respuesta sin el bit P/F activado");
                        }
                        else //Si el bit de sondeo esta a 1, entonces se ocultará el icono de advertencia ya que la trama esta respondiendo a un comando anterior
                        {
                            ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
            else //Si el valor del bit C/R cambia a C
            {
                //Si el valor del bit C/R cambia a C, se coloca en el campo de dirección, la dirección de la estación a la que va dirigido el comando
                CajaDireccionEstacion.Text = direccionEstacion;

                if (CajaTipoTrama.Text == TipoTrama.Solicitud_conexion) //Si la trama a enviar es de solicitud de conexión (SABM), se oculta el icono de advertencia ya que una trama SABM siempre es un comando
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
                else if (CajaTipoTrama.Text == TipoTrama.Solicitud_desconexion) //Si la trama a enviar es de solicitud de desconexión (DISC), se oculta el icono de advertencia ya que una trama DISC siempre es un comando
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
                else if (CajaTipoTrama.Text == TipoTrama.Asentimiento_no_numerado) //Si la trama a enviar es de asentimiento no numerado (UA), se muestra el icono de advertencia y la descripción ya que una trama UA no puede ser nunca un comando
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de asentimiento no numerado (UA) a modo de comando.\nLas tramas de asentimiento no numerado (UA) siempre son respuestas a una solicitud o comando.");
                }
                else if (CajaTipoTrama.Text == TipoTrama.Modo_desconectado) //Si la trama a enviar es de modo desconectado (DM), se muestra el icono de advertencia y la descripción ya que una trama DM no puede ser nunca un comando
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de modo desconectado (DM) a modo de comando.\nLas tramas de modo desconectado (DM) siempre son respuestas a una solicitud o comando.");
                }
                else if (CajaTipoTrama.Text == TipoTrama.Rechazo_trama) //Si la trama a enviar es de rechazo de trama (FRMR), se muestra el icono de advertencia y la descripción ya que una trama FRMR no puede ser nunca un comando
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo de trama (FRMR) a modo de comando.\nLas tramas de rechazo (FRMR) siempre son respuestas a una solicitud o comando.");
                }
                else if (CajaTipoTrama.Text == TipoTrama.Receptor_preparado) //Si la trama a enviar es de receptor preparado (RR), se oculta el icono de advertencia siempre y cuando no se tenga previamente un comando con el bit P/F activado
                {
                    if (ExisteComandoSinResponder() && CajaBitSondeo.Text != "0") //Si existe un comando previo sin responder y el bit de sondeo esta activado, entonces se mostrará el icono de advertencia y la descripción ya que no se debe enviar otro comando del mismo tipo hasta que no se reciba una respuesta al anterior
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                        ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor preparado (RR)P a modo de comando, ya que existe un comando con el bit P/F al que responder");
                    }
                    else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                    }
                }
                else if (CajaTipoTrama.Text == TipoTrama.Receptor_no_preparado) //Si la trama a enviar es de receptor no preparado (RNR), se oculta el icono de advertencia siempre y cuando no se tenga previamente un comando con el bit P/F activado
                {
                    if (ExisteComandoSinResponder() && CajaBitSondeo.Text != "0") //Si existe un comando previo sin responder y el bit de sondeo esta activado, entonces se mostrará el icono de advertencia y la descripción ya que no se debe enviar otro comando del mismo tipo hasta que no se reciba una respuesta al anterior
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                        ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor no preparado (RNR)P a modo de comando, ya que existe un comando con el bit P/F al que responder");
                    }
                    else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                    }
                }
                else if (CajaTipoTrama.Text == TipoTrama.Rechazo) //Si la trama a enviar es de rechazo (REJ), se oculta el icono de advertencia siempre y cuando no se tenga previamente un comando con el bit P/F activado
                {
                    if (ExisteComandoSinResponder() && CajaBitSondeo.Text != "0") //Si existe un comando previo sin responder y el bit de sondeo esta activado, entonces se mostrará el icono de advertencia y la descripción ya que no se debe enviar otro comando del mismo tipo hasta que no se reciba una respuesta al anterior
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                        ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo (REJ)P a modo de comando, ya que existe un comando con el bit P/F al que responder");
                    }
                    else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                    }
                }
                else if (CajaTipoTrama.Text == TipoTrama.Rechazo_selectivo) //Si la trama a enviar es de rechazo selectivo (SREJ), se oculta el icono de advertencia siempre y cuando no se tenga previamente un comando con el bit P/F activado
                {
                    if (ExisteComandoSinResponder() && CajaBitSondeo.Text != "0") //Si existe un comando previo sin responder y el bit de sondeo esta activado, entonces se mostrará el icono de advertencia y la descripción ya que no se debe enviar otro comando del mismo tipo hasta que no se reciba una respuesta al anterior
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                        ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo selectivo (SREJ)P a modo de comando, ya que existe un comando con el bit P/F al que responder");
                    }
                    else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                    {
                        ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void CajaBitSondeo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Si el valor del bit P/F cambia, se debera comprobar que la configuración de la trama actual es válida para alertar al usuario en el caso de que no sea válida dicha configuración
            if (CajaTipoTrama.Text == TipoTrama.Asentimiento_no_numerado) //Si la trama que se desea enviar es de asentimiento no numerado (UA)
            {
                if (ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo != Int32.Parse(e.AddedItems[0].ToString())) //Si la última trama recibida tiene un estado del bit de sondeo diferente al configurado por el usuario, entonces se mostrará el icono de advertencia y la descripción ya que una trama UA debe tener el mismo valor para el bit de sondeo que el comando al que responder
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de asentimiento no numerado (UA) con el bit de sondeo con estado distinto al comando SABM o DISC al que se responde.");
                }
                else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
            }
            else if (CajaTipoTrama.Text == TipoTrama.Modo_desconectado) //Si la trama que se desea enviar es de modo desconectado (DM)
            {
                if (ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo != Int32.Parse(e.AddedItems[0].ToString())) //Si la última trama recibida tiene un estado del bit de sondeo diferente al configurado por el usuario, entonces se mostrará el icono de advertencia y la descripción ya que una trama DM debe tener el mismo valor para el bit de sondeo que el comando al que responder
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de modo desconectado (DM) con el bit de sondeo con estado distinto al comando DISC al que se responde.");
                }
                else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
            }
            else if (CajaTipoTrama.Text == TipoTrama.Rechazo_trama) //Si la trama que se desea enviar es de rechazo de trama (FRMR)
            {
                //Si la trama configurada tiene el bit de sondeo activado pero no existe ningun comando previo sin respuesta, entonces se mostrará el icono de advertencia y la descripción ya que una trama FRMR solo puede tener el bit de sondeo activado si esta respodiendo a un comando anterior
                if (Int32.Parse(e.AddedItems[0].ToString()) == 1 && (ObtenerInformacionUltimaTrama().InfoBitCommandRequest != "C" || ObtenerInformacionUltimaTrama().DireccionEstacion != nombreEstacionActual))
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo de trama (FRMR) con el bit de sondeo con estado distinto al comando al que se responde.");
                }
                else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
            }
            else if (CajaTipoTrama.Text == TipoTrama.Receptor_preparado) //Si la trama que se desea enviar es de receptor preparado (RR)
            {
                if (Int32.Parse(e.AddedItems[0].ToString()) != 0 && CajaBitCommandRequest.Text == "C" && ExisteComandoSinResponder()) //Si el usuario configura la trama como una comando con el bit de sondeo activado y existe un comando previo sin responder, entonces se mostrará el icono de advertencia y la descripción ya que existe un comando con el bit P/F activado sin responder y debe responderlo antes
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor preparado (RR) a modo de comando, ya que existe un comando con el bit P/F al que responder");
                }
                else if (Int32.Parse(e.AddedItems[0].ToString()) == 0 && CajaBitCommandRequest.Text == "R") //Si el usuario configura la trama como una respuesta con el bit de sondeo desactivado, entonces se mostrará el icono de advertencia y la descripción ya que no es posible enviar una respuesta sin el bit P/F activado
                { 
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor preparado (RR) a modo de respuesta sin el bit P/F activado");
                }
                else if (CajaBitCommandRequest.Text == "R" && !ExisteComandoSinResponder()) //Si el usuario configura la trama como una respuesta y no existe comando previo al que responder, entonces se mostrará el icono de advertencia y la descripción ya que no es posible enviar una respuesta si no existe un comando previo al que responder
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor preparado (RR) a modo de respuesta, ya que no existe un comando con el bit P/F al que responder");
                }
                else  //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
            }
            else if (CajaTipoTrama.Text == TipoTrama.Receptor_no_preparado) //Si la trama que se desea enviar es de receptor no preparado (RNR)
            {
                if (Int32.Parse(e.AddedItems[0].ToString()) != 0 && CajaBitCommandRequest.Text == "C" && ExisteComandoSinResponder()) //Si el usuario configura la trama como una comando con el bit de sondeo activado y existe un comando previo sin responder, entonces se mostrará el icono de advertencia y la descripción ya que existe un comando con el bit P/F activado sin responder y debe responderlo antes
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor no preparado (RNR) a modo de comando, ya que existe un comando con el bit P/F al que responder");
                }
                else if (Int32.Parse(e.AddedItems[0].ToString()) == 0 && CajaBitCommandRequest.Text == "R") //Si el usuario configura la trama como una respuesta con el bit de sondeo desactivado, entonces se mostrará el icono de advertencia y la descripción ya que no es posible enviar una respuesta sin el bit P/F activado
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor no preparado (RNR) a modo de respuesta sin el bit P/F activado");
                }
                else if (CajaBitCommandRequest.Text == "R" && !ExisteComandoSinResponder()) //Si el usuario configura la trama como una respuesta y no existe comando previo al que responder, entonces se mostrará el icono de advertencia y la descripción ya que no es posible enviar una respuesta si no existe un comando previo al que responder
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de receptor no preparado (RNR) a modo de respuesta, ya que no existe un comando con el bit P/F al que responder");
                }
                else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
            }
            else if (CajaTipoTrama.Text == TipoTrama.Rechazo) //Si la trama que se desea enviar es de rechazo (REJ)
            {
                if (Int32.Parse(e.AddedItems[0].ToString()) != 0 && CajaBitCommandRequest.Text == "C" && ExisteComandoSinResponder()) //Si el usuario configura la trama como una comando con el bit de sondeo activado y existe un comando previo sin responder, entonces se mostrará el icono de advertencia y la descripción ya que existe un comando con el bit P/F activado sin responder y debe responderlo antes
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo (REJ) a modo de comando, ya que existe un comando con el bit P/F al que responder");
                }
                else if (Int32.Parse(e.AddedItems[0].ToString()) == 0 && CajaBitCommandRequest.Text == "R") //Si el usuario configura la trama como una respuesta con el bit de sondeo desactivado, entonces se mostrará el icono de advertencia y la descripción ya que no es posible enviar una respuesta sin el bit P/F activado
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo (REJ) a modo de respuesta sin el bit P/F activado");
                }
                else if (CajaBitCommandRequest.Text == "R" && !ExisteComandoSinResponder()) //Si el usuario configura la trama como una respuesta y no existe comando previo al que responder, entonces se mostrará el icono de advertencia y la descripción ya que no es posible enviar una respuesta si no existe un comando previo al que responder
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo (REJ) a modo de respuesta, ya que no existe un comando con el bit P/F al que responder");
                }
                else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
            }
            else if (CajaTipoTrama.Text == TipoTrama.Rechazo_selectivo) //Si la trama que se desea enviar es de rechazo selectivo (SREJ)
            {
                if (Int32.Parse(e.AddedItems[0].ToString()) != 0 && CajaBitCommandRequest.Text == "C" && ExisteComandoSinResponder()) //Si el usuario configura la trama como una comando con el bit de sondeo activado y existe un comando previo sin responder, entonces se mostrará el icono de advertencia y la descripción ya que existe un comando con el bit P/F activado sin responder y debe responderlo antes
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo selectivo (SREJ) a modo de comando, ya que existe un comando con el bit P/F al que responder");
                }
                else if (Int32.Parse(e.AddedItems[0].ToString()) == 0 && CajaBitCommandRequest.Text == "R") //Si el usuario configura la trama como una respuesta con el bit de sondeo desactivado, entonces se mostrará el icono de advertencia y la descripción ya que no es posible enviar una respuesta sin el bit P/F activado
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo selectivo (SREJ) a modo de respuesta sin el bit P/F activado");
                }
                else if (CajaBitCommandRequest.Text == "R" && !ExisteComandoSinResponder()) //Si el usuario configura la trama como una respuesta y no existe comando previo al que responder, entonces se mostrará el icono de advertencia y la descripción ya que no es posible enviar una respuesta si no existe un comando previo al que responder
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                    ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de rechazo selectivo (SREJ) a modo de respuesta, ya que no existe un comando con el bit P/F al que responder");
                }
                else //En cualquier otro caso, se ocultará el icono de advertencia (en el caso de que estuviera visible anteriormente)
                {
                    ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
                }
            }
        }

        private bool ExisteComandoSinResponder()
        {
            int indice = -1;
            for (int i = listaTramasRecibidas.Count - 1; i >= 0; i--) //Se recorre la lista de tramas recibidas desde el final en busca de comandos con el bit P/F activado
            {
                if (listaTramasRecibidas[i].InfoBitSondeo == 1 && listaTramasRecibidas[i].InfoBitCommandRequest == "C" && !listaTramasRecibidas[i].TramaIncorrecta) //Si en el recorrido, se encuentra un comando con el bit P/F activado y no es una trama errónea, se obtiene el índice o posición en el que se encuentra dicho comando y finaliza el recorrido
                {
                    indice = i;
                    break;
                }
            }

            if (indice == -1) //Si el indice vale -1, significa que no existe ningún comando con el bit P/F activado, por lo que no existe ningín comando sin responder y se devuelve false
            {
                return false;
            }

            for (int i = indice; i < listaTramasEnviadas.Count; i++) //Se recorre la lista de tramas enviadas desde el comando previamente encontrado en busca de respuestas con el bit P/F activado
            {
                if (listaTramasEnviadas[i].InfoBitSondeo == 1 && listaTramasEnviadas[i].InfoBitCommandRequest == "R") //Si en el recorrido, se encuentra una respuesta con el bit P/F activado, se devuelve true ya que existe una respuesta al comando previamente realizado
                {
                    return false;
                }
            }
            return true; //Si finaliza el recorrido sin encontrar una respuesta con el bit P/F activado, se devuelve false ya que no existe una respuesta al comando previamente realizado
        }

        private ToolTip generarTooltip(String mensaje) 
        {
            ToolTip toolTip = new ToolTip(); //Se crea una instancia de un toolTip cuyo cuerpo es el mensaje que se pasa como parámetro

            toolTip.BorderBrush = Brushes.Black; //Se define el color del borde del ToolTip de color negro
            toolTip.Content = mensaje; //Se define el contenido del ToolTip en función del mensaje recibido como parámetro
            toolTip.Foreground = Brushes.Black; //Se define el color de la fuente del texto del ToolTip de color negro
            toolTip.Background = Brushes.White; //Se define el color del fondo del ToolTip de color blanco

            return toolTip; //Se devuelve el ToolTip creado
        }

        private Trama ObtenerInformacionUltimaTramaRecibida()
        {
            //Recorremos la lista de tramas recibidas desde el final, hasta encontrar una trama no vacia la cual se tratará de la última trama recibida (Nota: Las tramas vacias tiene el bit de sondeo con un valor de -1)
            for (int i = listaTramasRecibidas.Count - 1; i >= 0; i--)
            {
                if (listaTramasRecibidas[i].InfoBitSondeo >= 0)
                {
                    return listaTramasRecibidas[i];
                }
            }

            //En el caso de que la estación no haya recibido ninguna trama, se generará y devovlerá una trama automática la cual será un comando (bit C/R a C) y tendrá el bit de sondeo desactivado
            Trama tr = new Trama();
            tr.InfoBitSondeo = 0;
            tr.InfoBitCommandRequest = "C";

            return tr;
        }

        private Trama ObtenerInformacionUltimaTrama()
        {
            int indice_ultima_trama_enviada = 0;
            int indice_ultima_trama_recibida = 0;

            //Recorremos la lista de tramas enviadas desde el final, hasta encontrar una trama no vacia la cual se tratará de la última trama enviada (Nota: Las tramas vacias tiene el bit de sondeo con un valor de -1)
            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--)
            {
                if (listaTramasEnviadas[i].InfoBitSondeo >= 0) //Cuando se encuentre la última trama enviada, se almacenará el índice de dicha trama en una variable
                {
                    indice_ultima_trama_enviada = i;
                    break;
                }
            }

            //Recorremos la lista de tramas recibidas desde el final, hasta encontrar una trama no vacia la cual se tratará de la última trama recibida (Nota: Las tramas vacias tiene el bit de sondeo con un valor de -1)
            for (int i = listaTramasRecibidas.Count - 1; i >= 0; i--)
            {
                if (listaTramasRecibidas[i].InfoBitSondeo >= 0) //Cuando se encuentre la última trama recibida, se almacenará el índice de dicha trama en una variable
                {
                    indice_ultima_trama_recibida = i;
                    break;
                }
            }

            if (indice_ultima_trama_enviada > indice_ultima_trama_recibida) //Si el índice de la última trama enviada es superior al índice de la última trama recibida, se devuelve la última trama enviada
            {
                return listaTramasEnviadas[indice_ultima_trama_enviada];
            }
            else //En caso contrario, se devuelve la última trama recibida
            {
                return listaTramasRecibidas[indice_ultima_trama_recibida];
            }
        }

        private void GestionarVisibilidadAyuda(Canvas canvas_ayuda)
        {
            //Si la visibilidad del canvas de ayuda esta oculta, se cambia su visibilidad a visible. 
            if (canvas_ayuda.Visibility == Visibility.Hidden)
            {
                canvas_ayuda.Visibility = Visibility.Visible;
                canvas_visible = canvas_ayuda; //Se almacena la referencia del último canvas que se ha hecho visible
            }
            else //Si la visibilidad del canvas de ayuda esta visible, se cambia su visibilidad a oculta.
            {
                canvas_ayuda.Visibility = Visibility.Hidden;
            }
        }

        private void BotonAyudaTipoTrama_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaTipoTrama); //Si se pulsa el botón de ayuda del tipo de trama, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaSeccionInformacionTrama_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaSeccionInformacionTrama); //Si se pulsa el botón de ayuda de la sección de la información sobre la trama, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonEnviar_Click(object sender, RoutedEventArgs e)
        {
            //Si el usuario desea enviar la trama con la configuracion elegida, se guardan los valores correspodientes de la trama en los atributos correspodientes. 
            //Posteriormente, la ventana principal accederá a dichos valores para generar y enviar la trama con la configuración elegida por el usuario
            direccionEstacion = CajaDireccionEstacion.Text;
            infoBitCommandRequest = CajaBitCommandRequest.Text;
            infoBitSondeo = Int32.Parse(CajaBitSondeo.Text);
            if (CajaNumeroTramaEsperada.IsEnabled)
            {
                numeroTramaEsperada = Int32.Parse(CajaNumeroTramaEsperada.Text);
            }

            //La instancia de la ventana devuelve verdadero ya que se desea enviar la trama configurada
            DialogResult = true;
        }

        private void BotonCancelar_Click(object sender, RoutedEventArgs e)
        {
            //Si el usuario no quiere enviar la trama configurada y desea cancelar los cambios, la instancia de la ventana devuelve falso
            DialogResult = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas_visible != null) //Si la referencia al último canvas visible no es nula y se ha pulsado cualquier parte de la pantalla, se ocultará dicho canvas
            {
                canvas_visible.Visibility = Visibility.Hidden;
            }
        }    
    }
}
