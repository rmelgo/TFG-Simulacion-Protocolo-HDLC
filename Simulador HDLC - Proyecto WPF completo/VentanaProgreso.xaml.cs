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
    /// Lógica de interacción para VentanaProgreso.xaml
    /// </summary>
    public partial class VentanaProgreso : Window
    {
        public bool Resultado {get; set;}

        public VentanaProgreso() //Constructor sin argumentos que asigna por defecto el valor falso al booleano Resultado que indica si la ventana se cierra con éxito, o se cierra debido a una cancelación
        {
            InitializeComponent();
            Resultado = true;
            this.Loaded += Window_Loaded; //Se registra el evenot Loaded para desplazar la ventana de progreso una vez ha sido cargada
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Se desplaza la ventana 100 pixeles a la derecha y 260 pixeles hacia el fondo con respecto a la posicion de la ventana propietaria de la instacia de la ventana de progreso
            Left = Owner.Left + 100;
            Top = Owner.Top + 260;

            //Se ajusta la anchura y altura mínima y máxima al valor actual de la ventana para evitar el redimensinado de la ventana por parte del usuario
            this.MinWidth = this.MaxWidth = this.Width;
            this.MinHeight = this.MaxHeight = this.Height;
        }

        private void BotonCancelar_Click(object sender, RoutedEventArgs e)
        {
            Resultado = false; //Si el usuario pulsa el botín de cancelar, se asgina un valor falso al booleano Resultado que indica que la vetana se cierra debido a una cancelación
            this.Close(); //Se utiliza el método Close() para cerrar la instancia de la ventana de progreso
        }
    }
}
