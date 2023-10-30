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
    /// Lógica de interacción para VentanaVisualizaciónDetalleTramaInformación.xaml
    /// </summary>
    public partial class VentanaVisualizaciónDetalleTramaInformación : Window
    {
        private Trama trama;
        private Canvas canvas_visible;

        public VentanaVisualizaciónDetalleTramaInformación(Trama trama) //Constructor que recibe como parámetro la trama la cual se desea mostrar el detalle
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
            CajaNumeroSecuencia.Text = trama.NumeroSecuencia.ToString();
            CajaBitSondeo.Text = trama.InfoBitSondeoTabla;
            CajaNumeroTramaEsperada.Text = trama.NumeroTramaEsperada.ToString();

            //Se extrae la información general de los distintos campos de la trama (campo de dirección, campo de control, campo de información, CRC y flags) y se colocan en las cajas correspodientes
            CajaFlagInicial.Text = "01111110";
            CajaDireccion.Text = trama.CampoEstacionByte;
            CajaControl.Text = trama.CampoControl;
            CajaInfoTrama.Text = trama.InfoTrama;
            CajaCRC.Text = trama.CRC;
            CajaFlagFinal.Text = "01111110";

            //Se extrae la información específica del campo de control de la trama (NS, NR, bit C/R y bit P/F) y se colocan en las cajas correspodientes (ahora se coloca en binario)
            CajaBit1.Text = "0";
            CajaNumeroSecuenciaControl.Text = Convert.ToString(trama.NumeroSecuencia, 2).PadLeft(3, '0');
            CajaBitSondeoControl.Text = trama.InfoBitSondeo.ToString();
            CajaNumeroTramaEsperadaControl.Text = Convert.ToString(trama.NumeroTramaEsperada, 2).PadLeft(3, '0');

            if (trama.TramaIncorrecta) //si la trama a representar es errónea, se colorea de color rojo el borde del campo de control y se muestra de manera clara que la trama representada es errónea
            {
                BordeCajaControl.BorderBrush = Brushes.Red;
                TextoTramaErrónea.Visibility = Visibility.Visible;
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
            DialogResult = true; //Cuando el usuario pulse el botón de aceptar, se cerrara la instancia de la ventana de visualización de la trama de información devolviendo un valor true
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
