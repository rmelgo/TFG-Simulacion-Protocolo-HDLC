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
    /// Lógica de interacción para Configuración.xaml
    /// </summary>
    public partial class Configuración : Window
    {
        public Protocolo protocolo { get; set; }
        public ModoDeTrabajo modoTrabajo { get; set; }
        public CanalTransmisión canalTransmision { get; set; }

        private String tempModoTrabajo;
        private Canvas canvas_visible;

        public Configuración(Protocolo protocolo, ModoDeTrabajo modoTrabajo, CanalTransmisión canalTransmision) //Constructor que recibe como parámetros, el protocolo de la estación, el modo de trabajo de la estación y el canal de transmisión
        {
            InitializeComponent();
            ajustarVentana();

            //Se recibe por parte de la ventana principal, el valor actual de la configuracion del protocolo, del modo de trabajo de la estación y del canal de transmisión
            this.protocolo = protocolo;
            this.modoTrabajo = modoTrabajo;
            this.canalTransmision = canalTransmision;

            cargarValoresConfiguracion(); //Cargamos los valores de las distintas secciones de la configuración, en la ventana de configuración
        }

        private void ajustarVentana()
        {
            this.Width = 516;

            //Se ajusta la anchura y altura mínima y máxima al valor actual de la ventana para evitar el redimensinado de la ventana por parte del usuario
            this.MinWidth = this.MaxWidth = this.Width;
            this.MinHeight = this.MaxHeight = this.Height;
        }

        private void cargarValoresConfiguracion()
        {
            //Cargamos en las cajas correspodientes los valores configurados por defecto para el protocolo
            CajaTimeoutCommand.Text = protocolo.TimeoutCommand.ToString();
            CajaTimeoutTramaI.Text = protocolo.TimeoutTramaI.ToString();
            CajaTimeoutRequest.Text = protocolo.TimeoutRequest.ToString();
            CajaTamañoVentana.Text = protocolo.TamañoVentana.ToString();
            CajaTramasErroneasConsecutivasPermitidas.Text = protocolo.NumeroMaximoTramasErroneasConsecutivasPermitidas.ToString();

            //Colocamos los posibles valores para el tamaño de la ventana y el número máximo de tramas erróneas consecutivas permitidas
            for (int i = 1; i < 8; i++)
            {
                CajaTamañoVentana.Items.Add(i);
                CajaTramasErroneasConsecutivasPermitidas.Items.Add(i);
            }

            tempModoTrabajo = modoTrabajo.TipoModoDeTrabajo; 

            //Se colorea de color rojo el boton del modo de trabajo de la estación seleccionado por defecto
            if (modoTrabajo.TipoModoDeTrabajo == "Manual")
            {
                ActualizarColorBordeBoton(BotonModoManual, BordeBotonModoManual, Color.FromRgb(216, 75, 75), 6);
            }
            else
            {
                ActualizarColorBordeBoton(BotonModoSemiautomático, BordeBotonModoSemiautomático, Color.FromRgb(216, 75, 75), 6);
            }

            //Cargamos en las cajas correspodientes los valores configurados por defecto para el canal de transmisión
            CajaRetardo.Text = canalTransmision.RetardoCanal.ToString();
            CajaTasaError.Text = canalTransmision.TasaError.ToString();

            SliderTasaError.Value = (double)Math.Round(float.Parse(CajaTasaError.Text), 2); //Configuramos el valor del slider de la tasa de error
        }
      
        private void mostrarVentanaAdvertencia(String mensaje, bool advertencia, String titulo) 
        {
            //Se crea una nueva instacia de la ventana de advertencia donde se pasa como parámetro el mensaje a mostrar en la ventana de advertencia. También se pasa un booleano como parámetro para indicar si se trata de una advertencia o de un mensaje informativo.
            VentanaAdvertencia va = new VentanaAdvertencia(mensaje, advertencia);
            va.Owner = this;
            va.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            va.Title = titulo;
            va.ShowDialog();
        }

        private void ActualizarColorBordeBoton(Button boton, Border bordeBoton, Color color, int tamaño_borde)
        {
            bordeBoton.BorderBrush = new SolidColorBrush(color); //Se actualiza el color del borde del botón pasado como parámetro al color pasado como parámetro
            bordeBoton.BorderThickness = new Thickness(tamaño_borde); //Se actualiza el grosor del borde del botón pasado como parámetro al valor pasado como parámetro
        }

        private void ActualizarColorBordeCaja(Border bordeBoton, Color color)
        {
            bordeBoton.BorderBrush = new SolidColorBrush(color); //Se actualiza el color del borde de la caja pasada como parámetro al color pasado como parámetro
        }

        private void CajaTimeoutCommand_TextChanged(object sender, TextChangedEventArgs e)
        {
            try //Cuando el contenido de la caja de timeout ante command cambia, se comprueba si el contenido de la caja es un número realizando la conversión del contenido a entero
            {
                Int32.Parse(CajaTimeoutCommand.Text); //Se realiza la conversión de la caja de timeout ante command a entero
                ActualizarColorBordeCaja(BordeCajaTimeoutCommand, Color.FromRgb(40, 40, 40)); //Si se ha podido hacer la conversión correctamente, entonces se mantiene/cambia el color del borde de la caja a su valor original
            }
            catch (Exception) //Si no se ha podido hacer la conversión correctamente, entonces se cambia el color del borde de la caja a rojo para alertar al usuario que el valor introducido no es válido
            {
                BordeCajaTimeoutCommand.BorderBrush = Brushes.Red;
            }
        }

        private void CajaTimeoutTramaI_TextChanged(object sender, TextChangedEventArgs e)
        {
            try //Cuando el contenido de la caja de timeout ante trama I cambia, se comprueba si el contenido de la caja es un número realizando la conversión del contenido a entero
            {
                Int32.Parse(CajaTimeoutTramaI.Text); //Se realiza la conversión de la caja de timeout ante trama I a entero
                ActualizarColorBordeCaja(BordeCajaTimeoutTramaI, Color.FromRgb(40, 40, 40)); //Si se ha podido hacer la conversión correctamente, entonces se mantiene/cambia el color del borde de la caja a su valor original
            }
            catch (Exception) //Si no se ha podido hacer la conversión correctamente, entonces se cambia el color del borde de la caja a rojo para alertar al usuario que el valor introducido no es válido
            {
                BordeCajaTimeoutTramaI.BorderBrush = Brushes.Red;
            }
        }

        private void CajaTimeoutRequest_TextChanged(object sender, TextChangedEventArgs e)
        {
            try //Cuando el contenido de la caja de timeout ante request cambia, se comprueba si el contenido de la caja es un número realizando la conversión del contenido a entero
            {
                Int32.Parse(CajaTimeoutRequest.Text); //Se realiza la conversión de la caja de timeout ante request a entero
                ActualizarColorBordeCaja(BordeCajaTimeoutRequest, Color.FromRgb(40, 40, 40)); //Si se ha podido hacer la conversión correctamente, entonces se mantiene/cambia el color del borde de la caja a su valor original
            }
            catch (Exception) //Si no se ha podido hacer la conversión correctamente, entonces se cambia el color del borde de la caja a rojo para alertar al usuario que el valor introducido no es válido
            {
                BordeCajaTimeoutRequest.BorderBrush = Brushes.Red;
            }
        }

        private void BotonModoManual_Click(object sender, RoutedEventArgs e)
        {
            //Se actualiza el valor de modo de trabajo a manual
            tempModoTrabajo = "Manual";

            //Se activa el borde rojo del botÓn del modo de trabajo manual y se desactiva el borde del botÓn del modo de trabajo semiautomático
            ActualizarColorBordeBoton(BotonModoManual, BordeBotonModoManual, Color.FromRgb(216, 75, 75), 6);
            ActualizarColorBordeBoton(BotonModoSemiautomático, BordeBotonModoSemiautomático, Color.FromRgb(112, 112, 112), 1);

            //Se desactiva el slider de la tasa de error, ya que carece de sentido su uso en el modo de trabajo manual
            SliderTasaError.Value = 0;
            SliderTasaError.IsEnabled = false;
        }

        private void BotonModoSemiautomático_Click(object sender, RoutedEventArgs e)
        {
            //Se actualiza el valor de modo de trabajo a semiautomático
            tempModoTrabajo = "Semiautomático";

            //Se activa el borde rojo del botón del modo de trabajo semiautomático y se desactiva el borde del botón del modo de trabajo manual
            ActualizarColorBordeBoton(BotonModoSemiautomático, BordeBotonModoSemiautomático, Color.FromRgb(216, 75, 75), 6);
            ActualizarColorBordeBoton(BotonModoManual, BordeBotonModoManual, Color.FromRgb(112, 112, 112), 1);

            //Se activa el slider de la tasa de error, ya que tiene sentido su uso en el modo de trabajo semiautomático
            SliderTasaError.IsEnabled = true;
        }

        private void CajaRetardo_TextChanged(object sender, TextChangedEventArgs e)
        {
            try //Cuando el contenido de la caja de retardo cambia, se comprueba si el contenido de la caja es un número realizando la conversión del contenido a entero
            {
                Int32.Parse(CajaRetardo.Text); //Se realiza la conversión de la caja de retardo a entero
                ActualizarColorBordeCaja(BordeCajaRetardo, Color.FromRgb(40, 40, 40)); //Si se ha podido hacer la conversión correctamente, entonces se mantiene/cambia el color del borde de la caja a su valor original
            }
            catch (Exception) //Si no se ha podido hacer la conversión correctamente, entonces se cambia el color del borde de la caja a rojo para alertar al usuario que el valor introducido no es válido
            {
                BordeCajaRetardo.BorderBrush = Brushes.Red;
            }
        }

        private void SliderTasaError_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CajaTasaError.Text = SliderTasaError.Value.ToString(); //Cuando el usuario modifica el valor del slider de la tasa de error, se translada dicho valor modificado a la caja de la tasa de error
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

        private void BotonAyudaTimeoutCommand_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaTimeoutCommand); //Si se pulsa el botón de ayuda del timeout ante command, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaTimeoutTramaI_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaTimeoutTramaI); //Si se pulsa el botón de ayuda del timeout ante trama I, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaTimeoutRequest_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaTimeoutRequest); //Si se pulsa el botón de ayuda del timeout ante request, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaTamañoVentana_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaTamañoVentana); //Si se pulsa el botón de ayuda del tamaño de la ventana, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaTramasErroneasConsecutivasPermitidas_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaTramasErroneasConsecutivasPermitidas); //Si se pulsa el botón de ayuda del número de tramas erróneas consecutivas permitidas, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaModoManual_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaModoManual); //Si se pulsa el botón de ayuda del modo manual, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaModoSemiautomático_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaModoSemiautomatico); //Si se pulsa el botón de ayuda del modo semiautomático, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaRetardo_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaRetardo); //Si se pulsa el botón de ayuda del retardo, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaTasaError_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaTasaError); //Si se pulsa el botón de ayuda de la tasa de error, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAceptarConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Convertimos los valores númericos de los parámetros del protocolo a enteros y los almacenamos en la instacia de Protocolo correspondiente
                protocolo.TimeoutCommand = Int32.Parse(CajaTimeoutCommand.Text);
                protocolo.TimeoutTramaI = Int32.Parse(CajaTimeoutTramaI.Text);
                protocolo.TimeoutRequest = Int32.Parse(CajaTimeoutRequest.Text);
                protocolo.TamañoVentana = Int32.Parse(CajaTamañoVentana.Text);
                protocolo.NumeroMaximoTramasErroneasConsecutivasPermitidas = Int32.Parse(CajaTramasErroneasConsecutivasPermitidas.Text);

                //Almacenamos el valor del modo de trabajo seleccionado por el usuario
                modoTrabajo.TipoModoDeTrabajo = tempModoTrabajo;

                //Convertimos los valores númericos de los parámetros del canal de transmisión a enteros y los almacenamos en la instacia de CanalTransmisión correspondiente
                canalTransmision.RetardoCanal = Int32.Parse(CajaRetardo.Text);
                canalTransmision.TasaError = float.Parse(CajaTasaError.Text);

                //La instancia de la ventana devuelve verdadero ya que se desea guardar la configuración
                DialogResult = true;
            }
            catch (Exception) //Si en la conversión de los valores del protocolo o del canal de transmisión a enteros, se produce algun error, se muestra un mensaje de advertencia
            {
                mostrarVentanaAdvertencia("Los parametros de la configuración no se han introducido correctamente.\nRevisa los parametros de configuración introducidos.", true, "Visual_HDLC");
            }
        }

        private void BotonCancelarConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            //Si el usuario no quiere guardar la configuración y desea cancelar los cambios, la instancia de la ventana devuelve falso
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
