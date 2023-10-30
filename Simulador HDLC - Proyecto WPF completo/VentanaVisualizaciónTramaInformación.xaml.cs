
using System;
using System.Collections.Generic;
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
    /// Lógica de interacción para VentanaVisualizacionTramaInformación.xaml
    /// </summary>
    public partial class VentanaVisualizaciónTramaInformación : Window
    {
        public String direccionEstacion { get; set; }

        public String infoBitCommandRequest { get; set; }

        public int infoBitSondeo { get; set; }

        public int numeroSecuencia { get; set; }

        public int numeroTramaEsperada { get; set; }

        public String infoTrama { get; set; }

        public bool retransmision { get; set; }

        private String nombreEstacionActual;
        private Protocolo protocolo;
        private bool envio_automatico;
        private Canvas canvas_visible;

        //Constructor que recibe como parámetros el tipo de trama, la dirección de la estación, información del bit C/R, información del bit P/F, el número de secuencia (NS) y de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo y un booleano que nos indica si la trama de información es reenviada
        public VentanaVisualizaciónTramaInformación(String tipoTrama, String direccionEstacion, String infoBitCommandRequest, int infoBitSondeo, int numeroSecuencia, int numeroTramaEsperada, String infoTrama, String nombreEstacionActual, Protocolo protocolo, bool retransmision, bool envio_automatico)
        {
            InitializeComponent();

            //Se almacena la información del nombre de la estación, el protocolo e información sobre el booleano que nos indica si la trama de información es reenviada
            this.nombreEstacionActual = nombreEstacionActual;
            this.protocolo = protocolo;
            this.retransmision = retransmision;

            //Se muestran los valores asociados a la trama a enviar en la ventana
            mostrarValores(tipoTrama, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroSecuencia, numeroTramaEsperada, infoTrama);
            cargarValores(tipoTrama, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroSecuencia, numeroTramaEsperada, infoTrama);

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

        private void mostrarValores(String tipoTrama, String direccionEstacion, String infoBitCommandRequest, int infoBitSondeo, int numeroSecuencia, int numeroTramaEsperada, String infoTrama)
        {
            //Se extrae el tipo de trama a representar y se coloca en la caja correspodiente
            CajaTipoTrama.Text = tipoTrama;

            //Se extrae la direccion de la estación
            CajaDireccionEstacion.Text = direccionEstacion;

            //Se extrae la información básica de la trama (NS, NR, bit C/R y bit P/F) y se colocan en las cajas correspondientes
            CajaBitCommandRequest.Text = infoBitCommandRequest;
            CajaNumeroSecuencia.Text = numeroSecuencia.ToString();
            CajaBitSondeo.Text = infoBitSondeo.ToString();
            CajaNumeroTramaEsperada.Text = numeroTramaEsperada.ToString();

            //Se extrae la información contenida en la propia trama de información
            CajaInfoTrama.Text = infoTrama;

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

            if (retransmision) //Si la trama a enviar se trata de una retransmisión, se desactivan todas las cajas correspodientes y se coloca en el campo de información de la trama el mensaje "RETRANSMISIÓN" que indica claramente que la trama a enviar es una retransmisión
            {
                DesactivarComboBox(CajaBitCommandRequest, BordeCajaBitCommandRequest, Color.FromRgb(235, 235, 235));
                DesactivarComboBox(CajaNumeroSecuencia, BordeCajaNumeroSecuencia, Color.FromRgb(235, 235, 235));
                DesactivarComboBox(CajaBitSondeo, BordeCajaBitSondeo, Color.FromRgb(235, 235, 235));
                DesactivarComboBox(CajaNumeroTramaEsperada, BordeCajaNumeroTramaEsperada, Color.FromRgb(235, 235, 235));

                CajaInfoTrama.Text = "RETRANSMISIÓN";
            }
        }

        private void DesactivarComboBox(ComboBox comboBox, Border bordeComboBox, Color color)
        {
            //Se desactiva el combobox recibido como parámetro y se cambia el color del borde de la caja al color pasado como parámetro
            comboBox.IsEnabled = false;
            bordeComboBox.Background = new SolidColorBrush(color);
            //comboBox.IsEditable = false;
        }

        public void cargarValores(String tipoTrama, String direccionEstacion, String infoBitCommandRequest, int infoBitSondeo, int numeroSecuencia, int numeroTramaEsperada, String infoTrama)
        {
            //Se cargan los valores de la información básica de la trama
            this.direccionEstacion = direccionEstacion;
            this.infoBitCommandRequest = infoBitCommandRequest;
            this.infoBitSondeo = infoBitSondeo;
            this.numeroSecuencia = numeroSecuencia;
            this.numeroTramaEsperada = numeroTramaEsperada;
            this.infoTrama = infoTrama;
        }

        private void CajaBitCommandRequest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Si el valor del bit C/R cambia, se debera actualizar el valor de la dirección de la estación y comprobar que la configuración de la trama actual es válida para alertar al usuario en el caso de que no sea válida dicha configuración
            if (e.AddedItems[0].ToString() == "R")
            {
                //Si el valor del bit C/R cambia a R, se coloca en el campo de dirección, la dirección de la estación que realiza la respuesta
                CajaDireccionEstacion.Text = nombreEstacionActual;

                //Se muestra el icono de advertencia y la descripción ya que una trama I no puede ser nunca una respuesta
                ImagenAdvertenciaTrama.Visibility = Visibility.Visible;
                ImagenAdvertenciaTrama.ToolTip = generarTooltip("No es posible enviar una trama de información (I) a modo de respuesta.\nLas tramas de información (I) siempre son comandos.");
            }
            else
            {
                //Si el valor del bit C/R cambia a C, se coloca en el campo de dirección, la dirección de la estación a la que va dirigido el comando
                CajaDireccionEstacion.Text = direccionEstacion;

                //Se oculta el icono de advertencia ya que una trama I siempre es un comando
                ImagenAdvertenciaTrama.Visibility = Visibility.Hidden;
            }
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
            //Si el usuario desea enviar la trama con la configuracion elegida, se guardan los valores correspondientes de la trama en los atributos correspodientes. 
            //Posteriormente, la ventana principal accederá a dichos valores para generar y enviar la trama con la configuración elegida por el usuario
            direccionEstacion = CajaDireccionEstacion.Text;
            infoBitCommandRequest = CajaBitCommandRequest.Text;
            infoBitSondeo = Int32.Parse(CajaBitSondeo.Text);
            numeroSecuencia = Int32.Parse(CajaNumeroSecuencia.Text);
            numeroTramaEsperada = Int32.Parse(CajaNumeroTramaEsperada.Text);
            infoTrama = CajaInfoTrama.Text;

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