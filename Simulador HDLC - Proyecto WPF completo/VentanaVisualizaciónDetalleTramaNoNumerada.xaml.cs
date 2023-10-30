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
    /// Lógica de interacción para VentanaVisualizaciónDetalleTramaNoNumerada.xaml
    /// </summary>
    public partial class VentanaVisualizaciónDetalleTramaNoNumerada : Window
    {
        private Trama trama;
        private Canvas canvas_visible;

        public VentanaVisualizaciónDetalleTramaNoNumerada(Trama trama) //Constructor que recibe como parámetro la trama la cual se desea mostrar el detalle
        {
            InitializeComponent();
            ajustarVentana();
            this.trama = trama; //Se almacena la instancia de la trama recibida como parámetro en el atributo local trama
            cargarValores();
        }

        private void ajustarVentana()
        {
            this.Height = 405;
            this.Width = 868;

            //Se ajusta la anchura y altura mínima y máxima al valor actual de la ventana para evitar el redimensinado de la ventana por parte del usuario
            this.MinWidth = this.MaxWidth = this.Width;
            this.MinHeight = this.MaxHeight = this.Height;
        }

        private void cargarValores()
        {
            //Se extrae el tipo de trama a representar y se coloca en la caja correspodiente
            CajaTipoTrama.Text = trama.TipoDeTrama;

            //Se extrae la información básica de la trama (NS, NR, bit C/R y bit P/F) y se colocan en las cajas correspondientes
            CajaBitCommandRequest.Text = trama.InfoBitCommandRequest;
            DesactivarCampo(CajaNumeroSecuencia, BordeCajaNumeroSecuencia, Color.FromRgb(205, 206, 207));
            CajaBitSondeo.Text = trama.InfoBitSondeoTabla;
            DesactivarCampo(CajaNumeroTramaEsperada, BordeCajaNumeroTramaEsperada, Color.FromRgb(205, 206, 207));

            //Se extrae la información general de los distintos campos de la trama (campo de dirección, campo de control, campo de información, CRC y flags) y se colocan en las cajas correspodientes
            CajaFlagInicial.Text = "01111110";
            CajaDireccion.Text = trama.CampoEstacionByte;
            CajaControl.Text = trama.CampoControl;
            CajaCRC.Text = trama.CRC;
            CajaFlagFinal.Text = "01111110";

            //Se extrae la información específica del campo de control de la trama (NS, NR, bit C/R y bit P/F) y se colocan en las cajas correspodientes (ahora se coloca en binario)
            CajaBit1.Text = "1";
            CajaBit2.Text = "1";
            CajaBitSondeoControl.Text = trama.InfoBitSondeo + "";
            
            if (trama.TramaIncorrecta) //Cada tipo de trama tiene un valor distinto para M3M4M6M7M8
            {
                //Si la trama es erronea se configura M3M4M6M7M8 con un valor "imposible"
                CajaM3M4.Text = "11";
                CajaM6M7M8.Text = "111";

                //Si la trama representada es incorrecta, se colorea de color rojo el borde de la caja del campo de control
                BordeCajaControl.BorderBrush = Brushes.Red;
                TextoTramaErrónea.Visibility = Visibility.Visible;
            }
            else if (trama.TipoDeTrama == TipoTrama.Solicitud_conexion)
            {
                CajaM3M4.Text = "11";
                CajaM6M7M8.Text = "100";
            }
            else if (trama.TipoDeTrama == TipoTrama.Asentimiento_no_numerado)
            {
                CajaM3M4.Text = "00";
                CajaM6M7M8.Text = "110";
            }
            else if (trama.TipoDeTrama == TipoTrama.Solicitud_desconexion)
            {
                CajaM3M4.Text = "00";
                CajaM6M7M8.Text = "010";
            }
            else if (trama.TipoDeTrama == TipoTrama.Modo_desconectado)
            {
                CajaM3M4.Text = "11";
                CajaM6M7M8.Text = "000";
            }
            else if (trama.TipoDeTrama == TipoTrama.Rechazo_trama)
            {
                CajaM3M4.Text = "10";
                CajaM6M7M8.Text = "001";
            }
        }

        private void DesactivarCampo(TextBox caja, Border borde, Color color)
        {
            caja.Background = new SolidColorBrush(color); //Se actualiza el color del fondo de la caja pasada como parámetro al color pasado como parámetro
            borde.Background = new SolidColorBrush(color); //Se actualiza el color del borde de la caja pasada como parámetro al color pasado como parámetro
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

        private void BotonAyudaSeccionInformacionBasicaTrama_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaSeccionInformacionBasicaTrama); //Si se pulsa el botón de ayuda de la sección de información básica de la trama, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaSeccionInformacionGeneralTrama_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaSeccionInformacionGeneralTrama); //Si se pulsa el botón de ayuda de la sección de información general de la trama, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaSeccionInformacionEspecificaCampoControlTrama_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaSeccionInformacionEspecificaCampoControlTrama); //Si se pulsa el botón de ayuda de la sección de información específica del campo de control de la trama, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAceptar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; //Cuando el usuario pulse el botón de aceptar, se cerrara la instancia de la ventana de visualización de la trama no numerada devolviendo un valor true
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
