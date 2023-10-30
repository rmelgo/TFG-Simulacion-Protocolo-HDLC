using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Simulador_HDLC
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private bool conexionServidor;
        private bool segundaEstacion;
        private NamedPipeServer pipeServidor;
        private NamedPipeClient pipeConexion;
        private NamedPipeServer pipeServidor2;
        private NamedPipeClient pipeConexion2;

        private VentanaProgreso vp;
        private DateTime fechaActual;

        private Estación estacion;
        private Protocolo protocolo;
        private ModoDeTrabajo modoTrabajo;
        private CanalTransmisión canalTransmision;

        private VentanaVisualizaciónTrama vvt;
        private VentanaVisualizaciónTramaInformación vvti;

        private ObservableCollection<Trama> listaTramasEnviadas;
        private ObservableCollection<Trama> listaTramasRecibidas;
        private ObservableCollection<Trama> listaTramasPendientesRetransmision;

        private int tamaño_ventana_actual = 0;
        private int numero_tramas_erroneas_consecutivas_recibidas = 0;
        private int numero_tramas_dibujadas = 0;
        private int tramas_enviadas = 0;
        private int tramas_recibidas = 0;

        private CancellationTokenSource cancellationTokenSourceTimeoutRequest = new CancellationTokenSource();
        private CancellationTokenSource cancellationTokenSourceTimeoutCommand = new CancellationTokenSource();
        private CancellationTokenSource cancellationTokenSourceTimeoutTramaI = new CancellationTokenSource();
        private bool vistaGrafica = true;

        private bool tareaTimeoutTramaIActivada = false;

        private bool encolarRecepcionTrama = false;
        List<String> listaMensajes = new List<String>();

        private bool turno = true;
        private bool no_corregir = false;

        private DispatcherTimer timer;
        private int segundos = 0;
        private int minutos = 0;
        private Canvas canvas_visible;
        private Line EjeEstacionLocal;
        private Line EjeEstacionContraria;

        public MainWindow()
        {
            InitializeComponent();
            ajustarVentana();
            inicializarValores();
            cargarValoresVentanaEstacion();
        }

        private void ajustarVentana()
        {
            this.Height = 548;
            this.Width = 968;

            //Se ajusta la anchura y altura mínima y máxima al valor actual de la ventana para evitar el redimensinado de la ventana por parte del usuario
            this.MinWidth = this.MaxWidth = this.Width;
            this.MinHeight = this.MaxHeight = this.Height;
        }

        private void inicializarValores()
        {
            estacion = new Estación(); //Se crea una nueva instacia de la clase Estación que almacena el nombre, la situacion y los numeros de secuencia y trama esperada de la estación
            protocolo = new Protocolo(); //Se crea una nueva instacia de la clase Protocolo que almacena la información correspondiente del protocolo asociada a la estación actual
            modoTrabajo = new ModoDeTrabajo(); //Se crea una nueva instacia de la clase ModoDeTrabajo que almacena la información correspondiente del modo de trabajo asociada a la estación actual
            canalTransmision = new CanalTransmisión(); //Se crea una nueva instacia de la clase CanalTransmisión que almacena la información correspondiente del canal de transmisión asociada a la estación actual

            conexionServidor = false;

            listaTramasEnviadas = new ObservableCollection<Trama>(); //Se crea una colección que almacenará las tramas enviadas por la estación actual
            listaTramasRecibidas = new ObservableCollection<Trama>(); //Se crea una colección que almacenará las tramas recibidas por la estación actual
            listaTramasPendientesRetransmision = new ObservableCollection<Trama>(); //Se crea una colección que almacenará las tramas pendientes de enviar o retransmitir por la estación actual

            TablaTramasEnviadas.ItemsSource = listaTramasEnviadas; //Se establece el origen de items de la tabla de tramas enviadas en la lista que almacena las tramas enviadas
            TablaTramasRecibidas.ItemsSource = listaTramasRecibidas; //Se establece el origen de items de la tabla de tramas recibidas en la lista que almacena las tramas recibidas

            listaTramasEnviadas.CollectionChanged += ListaTramasEnviadas_CollectionChanged; //Se registra el evento CollectionChanged de la colección de tramas enviadas, ya que es conveniente actualizar la vista de la tabla de tramas enviadas cuando se produzcan cambios en la colección de tramas enviadas
            listaTramasRecibidas.CollectionChanged += ListaTramasRecibidas_CollectionChanged; //Se registra el evento CollectionChanged de la colección de tramas recibidas, ya que es conveniente actualizar la vista de la tabla de tramas recibidas cuando se produzcan cambios en la colección de tramas recibidas
            listaTramasPendientesRetransmision.CollectionChanged += ListaTramasPendientesRetransmision_CollectionChanged; //Se registra el evento CollectionChanged de la colección de tramas pendientes de retransmisión, ya que es conveniente indicar el numero de tramas pendientes por reenviar en el caso de que las haya

            segundaEstacion = false;
            EstadoBotonesEnvioTramas(false); //Se desactivan todos los botones relacionados con el envio de tramas ya que la estación por defecto no se encuentra fisicamente conectada con ninguna otra estación

            //Se generan los tooltip o mensajes de ayuda auxiliares para indicar que timeouts se encuentran activos en cada momento en la estación
            SimboloTimeoutCommand.ToolTip = generarTooltip("El timeout ante Command se encuentra activo en este momento");
            SimboloTimeoutTramaI.ToolTip = generarTooltip("El timeout ante Trama I se encuentra activo en este momento");
            SimboloTimeoutRequest.ToolTip = generarTooltip("El timeout ante Request se encuentra activo en este momento");

            //Se generan los tooltip o mensajes de ayuda auxiliares para indicar las funciones de los botones de la botonera superior
            BotonEnvioTramaInformacion.ToolTip = generarTooltip("Este botón permite enviar una trama de información (I)");
            BotonEnvioTramaReceptorPreparado.ToolTip = generarTooltip("Este botón permite enviar una trama de receptor preparado (RR)");
            BotonEnvioTramaReceptorNoPreparado.ToolTip = generarTooltip("Este botón permite enviar una trama de receptor no preparado (RNR)");
            BotonEnvioTramaRechazo.ToolTip = generarTooltip("Este botón permite enviar una trama de rechazo (REJ)");
            BotonEnvioTramaRechazoSelectivo.ToolTip = generarTooltip("Este botón permite enviar una trama de rechazo selectivo (SREJ)");
            BotonEnvioTramaSolicitudConexion.ToolTip = generarTooltip("Este botón permite enviar una trama de solicitud de conexión (SABM)");
            BotonEnvioTramaSolicitudDesconexion.ToolTip = generarTooltip("Este botón permite enviar una trama de solicitud de desconexión (DISC)");
            BotonEnvioTramaAsentimientoNoNumerado.ToolTip = generarTooltip("Este botón permite enviar una trama de asentimiento no numerado (UA)");
            BotonEnvioTramaModoDesconectado.ToolTip = generarTooltip("Este botón permite enviar una trama de modo desconectado (DM)");
            BotonEnvioTramaRechazoTrama.ToolTip = generarTooltip("Este botón permite enviar una trama de rechazo de trama (FRMR)");

            //Se generan los tooltip o mensajes de ayuda auxiliares para indicar las funciones de los botones de la botonera de opciones
            BotonConfiguracion.ToolTip = generarTooltip("Este botón permite acceder a la configuración de la estación");
            BotonGuardarCapturaTrafico.ToolTip = generarTooltip("Este botón permite guardar el intercambio de tramas realizado");
            BotonCargarCapturaTrafico.ToolTip = generarTooltip("Este botón permite cargar el intercambio de tramas realizado");
        }

        private void ListaTramasPendientesRetransmision_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) 
        {
            if (listaTramasPendientesRetransmision.Count > 0) //Si la lista de tramas pendientes de retransmisión no esta vacia, se muestra un mensaje alertando al usaurio del número de tramas pendientes de retransmisión
            {
                TramasPendientes.Text = "Tramas pendientes de retransmisión: " + listaTramasPendientesRetransmision.Count;
            }
            else //Si la lista de tramas pendientes de retransmisión esta vacia, no se muestra nada
            {
                TramasPendientes.Text = "";
            }
        }

        private void cargarValoresVentanaEstacion()
        {
            //Se asigna por defecto el nombre de la direccion de la estación a "Estación A"
            estacion.NombreEstacion = "Estación A";

            //Se asignan al combobox los posibles valores para el nombre de la dirección de la estación
            CajaNombreEstacion.Items.Add("Estación A");
            CajaNombreEstacion.Items.Add("Estación B");

            //Se asignan la direccion de la estación y los numeros de secuencia (NS) y de trama esperada (NR) de la estación
            CajaNombreEstacion.Text = estacion.NombreEstacion;
            CajaNumeroSecuencia.Text = estacion.NumeroSecuencia.ToString();
            CajaNumeroTramaEsperada.Text = estacion.NumeroTramaEsperada.ToString();

            //Se desactiva por defecto el boton para la finalización de la conexión física ya que por defecto la estación no esta conectada con ninguna otra
            DesactivarBoton(BotonFinalizarConexionFisica, BordeBotonFinalizarConexionFisica, Color.FromRgb(244, 244, 244));
        }

        private void BotonConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            //Se crea una nueva instancia de la ventana de configuración, a la cual se le pasa como parametros el protocolo, el modo de trabajo y el estado de canal de transmisión que tiene actualmente configurados la estación
            Configuración vai = new Configuración(protocolo, modoTrabajo, canalTransmision);
            vai.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            vai.Owner = this;

            vai.ShowDialog();
            if (vai.DialogResult == true) //Si el usuario ha realizado modificaciones sobre la configuración de la estación y ha pulsado el botón de guardar, la ventana de configración devolverá true y se guardaran los nuevos valores para el protocolo, para el modo de trabajo y para el canal de transmisión
            {
                protocolo = vai.protocolo;
                modoTrabajo = vai.modoTrabajo;
                canalTransmision = vai.canalTransmision;

                if (modoTrabajo.TipoModoDeTrabajo == "Semiautomático") //Si la estación se encuentra en el modo de trabajo semiautomático, se hace visible el icono del modo semiautomático y se oculta el icono del modo manual
                {
                    ImagenModoSemiautomatico.Visibility = Visibility.Visible;
                    ImagenModoManual.Visibility = Visibility.Hidden;
                }
                if (modoTrabajo.TipoModoDeTrabajo == "Manual") //Si la estación se encuentra en el modo de trabajo manual, se hace visible el icono del modo manual y se oculta el icono del modo semiautomático
                {
                    ImagenModoManual.Visibility = Visibility.Visible;
                    ImagenModoSemiautomatico.Visibility = Visibility.Hidden;
                }
            }
        }

        private async void BotonInicializarConexionFisica_Click(object sender, RoutedEventArgs e)
        {
            //Desactivamos el boton de inicializar la conexión física de la interfaz ya que se acaba de solicitar la inicialización de la conexión física
            DesactivarBoton(BotonInicializarConexionFisica, BordeBotonInicializarConexionFisica, Color.FromRgb(244, 244, 244));

            //Se obtiene el nombre de la estación actual
            String nombreEstacion = estacion.NombreEstacion;

            //Se crea una instancia de ventanaProgreso que indica el progreso realizado por la estaciín para establecer una conexión física con otra estación
            vp = new VentanaProgreso();
            vp.Owner = this;
            vp.Closed += VentanaProgreso_Closed; //Se registra el evento Closed de la ventana de progreso, para gestionar la cancelacion de la conexión física en el caso de que el usuario pulse el boton de cancelar de la ventana de progreso
            vp.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            vp.Show();

            //Creamos una instancia de la clase NamePipeClient en la cual se creará un tubería cliente con el nombre pasado como parámetro y que tendrá un comportamiento de entrada y salida
            pipeConexion = new NamedPipeClient("tuberiaServidor" + estacion.NombreEstacionContraria);
            pipeConexion.MessageReceived += Server_MessageReceived; //Registramos el evento MessageReceived de la clase NamedPipeClient a la instancia de la clase NamePipeClient creada
            conexionServidor = await pipeConexion.ConectarCliente(500); //Realizamos una llamada asíncrona a ConectarCliente() donde se ejecutará el comando ConnectAsync() con la instancia de la tubería del cliente creada

            //Si el valor devuelto por ConectarCliente() es verdadero, existe una tubería servidor escuchando en el otro extremo con la cual se ha establecido una conexión, por lo que se procederá a crear una tubería servidor a la cual se conectará el otro extremo a través de una tubería cliente
            if (conexionServidor)
            {
                //Creamos una instancia de la clase NamePipeServer en la cual se creará un tubería servidor con el nombre pasado como parámetro y que tendrá un comportamiento de entrada y salida
                pipeServidor2 = new NamedPipeServer("tuberiaServidor" + nombreEstacion);
                pipeServidor2.MessageReceived += Server_MessageReceived; //Registramos el evento MessageReceived de la clase NamedPipeServer a la instancia de la clase NamePipeServer creada
                await Task.Run(() => pipeServidor2.ConectarServidor()); //Realizamos una llamada asíncrona a ConectarServidor() donde se ejecutará el comando WaitForConnection() de manera bloqueante hasta que una tubería cliente se conecte
                //Como la llamada a WaitForConnection() es bloqueante y eso impide al proceso principal realizar otras tareas como refrescar la ventana de progreso, se ha creado un hilo o tarea específico que realice la espera bloqueante y así no bloquear el proceso principal
                //Aunque el hilo principal no se encuentre bloqueado, esperará a que finalice dicha tarea asíncrona antes de continuar con la ejecución del código

                segundaEstacion = true; //Variable que nos permite distinguir el primer proceso que solicitó la inicialización de la conexión física del segundo proceso que solicitó la inicialización de la conexión física
                //En este caso, como la estación ha podido conectarse con la tubería servidor de otra estación, entonces esta estación se considerará como segunda estación
            }
            else //Si el valor devuelto por ConectarCliente() es falso, no existe una tubería servidor escuchando en el otro extremo con la cual se haya podido establecer una conexión, por lo que se procederá a crear una tubería servidor a la cual se conectará el otro extremo a través de una tubería cliente
            {
                //Creamos una instancia de la clase NamePipeServer en la cual se creará un tubería servidor con el nombre pasado como parámetro y que tendrá un comportamiento de entrada y salida
                pipeServidor = new NamedPipeServer("tuberiaServidor" + nombreEstacion);

                if (pipeServidor.Tuberia == null) //Si la tubería servidor con el nombre de la estacion actual no se ha podido crear, significa que ya existe un tuberia con ese nombre en uso y que ambas estaciones tienen el mismo nombre
                {
                    //Se cierra la ventana de progreso 
                    vp.Close();

                    //Creamos una tubería cliente con el objetivo de sacar a la tubería servidor correspondiente del bloqueo de espera y poder cerrar todas las instancias de tuberias abiertas
                    NamedPipeClientStream pipeConexion = new NamedPipeClientStream(".", "tuberiaServidor" + estacion.NombreEstacion, PipeDirection.InOut);

                    //Se activa de nuevo el botón de Inicializar de la estación correspondiente
                    ActivarBoton(BotonInicializarConexionFisica, BordeBotonInicializarConexionFisica, Color.FromRgb(15, 216, 158));

                    try
                    {
                        //Nos conectamos a la tubería servidora para sacarla de la espera bloqueada (WaitForConnnection()) y posteriormente cerrarla
                        pipeConexion.Connect();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al establecer la conexión: {0}", ex.Message);
                    }

                    //Cerramos la instacia de la tuberia del cliente que se acaba de crear
                    pipeConexion.Close();

                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido realizar la conexión física
                    mostrarVentanaAdvertencia("Conexión fallida.\nLos nombres de las estaciones coinciden.", true, "Advertencia");

                    return; //Salimos de la función ya que el proceso de establecimiento de la conexión física ha finalizado con fallo
                }
                else //Si la tubería con el nombre de la estación actual también se ha creado sin problemas, significa que las estaciones tienen distinto nombre
                {
                    pipeServidor.MessageReceived += Server_MessageReceived; //Registramos el evento MessageReceived de la clase NamedPipeServer a la instancia de la clase NamePipeServer creada
                    await Task.Run(() => pipeServidor.ConectarServidor()); //Realizamos una llamada asíncrona a ConectarServidor() donde se ejecutará el comando WaitForConnection() de manera bloqueante hasta que una tubería cliente se conecte

                    //Creamos una tubería cliente para conectarnos con la tubería servidor de la estación situada en el otro extremo
                    pipeConexion2 = new NamedPipeClient("tuberiaServidor" + estacion.NombreEstacionContraria); //Creamos una instancia de la clase NamePipeClient en la cual se creará un tubería cliente con el nombre pasado como parámetro y que tendrá un comportamiento de entrada y salida
                    pipeConexion2.MessageReceived += Server_MessageReceived; //Registramos el evento MessageReceived de la clase NamedPipeClient a la instancia de la clase NamePipeClient creada
                    conexionServidor = await pipeConexion2.ConectarCliente(500); //Realizamos una llamada asíncrona a ConectarCliente() donde se ejecutará el comando ConnectAsync() con la instancia de la tubería del cliente creada

                    if (!conexionServidor) //Si no se ha podido conectar la tubería cliente con la tubería servidor situada en la estación del otro extremo, eso significa que las estaciines tienen el mismo nombre por lo que no se puede realizar la conexión
                    {
                        //Se cierra la ventana de progreso 
                        vp.Close();

                        //Se activa de nuevo el botón de Inicializar de la estación correspondiente
                        ActivarBoton(BotonInicializarConexionFisica, BordeBotonInicializarConexionFisica, Color.FromRgb(15, 216, 158));

                        //Cerramos la tubería del servidor ya que los nombres de las estaciones coinciden (sabemos que coinciden ya que la conexión del cliente con el servidor de la estacion contraria no se ha podido realizar)
                        pipeServidor.Close();

                        return; //Salimos de la función ya que el proceso de establecimiento de la conexión física ha finalizado con fallo
                    }
                }
            }

            vp.Close(); //Una vez se han conectado ambas tuberías, se cierra la ventana de progreso

            //Si la operacion de establecimiento de la conexión física no se ha cancelado, el primer proceso que solicito dicho establecimiento de la conexión física ejecutará el siguiente código
            if (segundaEstacion == false && vp.Resultado == true)
            {
                StreamWriter sw = new StreamWriter(pipeServidor.Tuberia);
                sw.AutoFlush = true;

                //Se obtiene el instante de tiempo en el cual ha empezado la conexión física y se envia este tiempo a la otra estación para mantener una referencia de tiempo común
                fechaActual = DateTime.UtcNow;
                sw.WriteLine(fechaActual.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                //Se crea un timer el cual generará ticks con una frecuencia de 0.1 segundos.
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer.Tick += Timer_Tick; //Se define una manejadora del evento tick del timer, la cual se encargará de actualizar en pantalla la referencia temporal
                timer.Start(); //Se inicia el timer

                CanvasSeccionGrafica.Height = 270; //Se restablece la altura del canvas de la sección gráfica

                //Se establece la situación de la estación correspondiente en Desconectado
                ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Desconectado);

                //Activamos el boton de finalizar la conexión física de la interfaz ya que se acaba de establecer la conexión física
                ActivarBoton(BotonFinalizarConexionFisica, BordeBotonFinalizarConexionFisica, Color.FromRgb(255, 123, 123));

                //Activamos los botones de envío de tramas y desactivamos los botones de configuración (no nos interesa modificar la configuración si se ha establecido una conexión física con otra estación)
                EstadoBotonesEnvioTramas(true);
                EstadoBotonConfiguracion(false);

                //Se limpian las listas de tramas enviadas y recibidas así como la representacion gráfica y animaciones asociadas
                LimpiarInformaciónTramasEstación();

                //Se cargan los valores del número de secuencia y número de trama esperada
                CajaNumeroSecuencia.Text = estacion.NumeroSecuencia.ToString();
                CajaNumeroTramaEsperada.Text = estacion.NumeroTramaEsperada.ToString();

                //Colocamos los nombres de las estaciones conectadas en la sección gráfica
                NombreEstaciónOrigen.Content = nombreEstacion.LastOrDefault();
                NombreEstaciónDestino.Content = estacion.NombreEstacionContraria.LastOrDefault();

                //Se imprime un mensaje indicando que la conexión física se ha realizado correctamente
                mostrarVentanaAdvertencia("Conexión realizada correctamente.", false, "Información");

                //Ejecutamos el metodo EscucharTuberia() de la tuberia servidor del proceso correspondiente
                //Este método esperará de manera asíncrona la recepción de mensajes por la tuberia servidora, lo que permite al proceso ejecutar otras tareas mientras se escucha por la tubería
                pipeServidor.EscucharTuberia();
            }
            else if (vp.Resultado == true)  //Si la operación de establecimiento de la conexión física no se ha cancelado, el segundo proceso que solicito dicho establecimiento de la conexión física ejecutará el siguiente código
            {
                StreamReader sr = new StreamReader(pipeConexion.Tuberia);

                //Se recibe el instante de tiempo en el cual ha empezado la conexión física, la cual es enviada por la otra estación, para mantener una referencia de tiempo común
                String temp = sr.ReadLine();
                fechaActual = DateTime.ParseExact(temp, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                //Se crea un timer el cual generará ticks con una frecuencia de 0.1 segundos.
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer.Tick += Timer_Tick; //Se define una manejadora del evento tick del timer, la cual se encargará de actualizar en pantalla la referencia temporal
                timer.Start(); //Se inicia el timer

                CanvasSeccionGrafica.Height = 270; //Se restablece la altura del canvas de la sección gráfica

                //Se establece la situación de la estación correspondiente en Desconectado
                ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Desconectado);

                //Activamos el boton de finalizar la conexión física de la interfaz ya que se acaba de establecer la conexión física
                ActivarBoton(BotonFinalizarConexionFisica, BordeBotonFinalizarConexionFisica, Color.FromRgb(255, 123, 123));

                //Activamos los botones de envío de tramas y desactivamos los botones de configuración (no nos interesa modificar la configuración si se ha establecido una conexión física con otra estación)
                EstadoBotonesEnvioTramas(true);
                EstadoBotonConfiguracion(false);

                //Se limpian las listas de tramas enviadas y recibidas así como la representación gráfica y animaciones asociadas
                LimpiarInformaciónTramasEstación();

                //Se cargan los valores del número de secuencia y número de trama esperada
                CajaNumeroSecuencia.Text = estacion.NumeroSecuencia.ToString();
                CajaNumeroTramaEsperada.Text = estacion.NumeroTramaEsperada.ToString();

                //Colocamos los nombres de las estaciones conectadas en la sección gráfica
                NombreEstaciónOrigen.Content = nombreEstacion.LastOrDefault();
                NombreEstaciónDestino.Content = estacion.NombreEstacionContraria.LastOrDefault();

                //Ejecutamos el metodo EscucharTuberia() de la tubería servidor del proceso correspondiente
                //Este método esperará de manera asíncrona la recepción de mensaje por la tubería servidora, lo que permite al proceso ejecutar otras tareas mientras se escucha por la tubería
                pipeServidor2.EscucharTuberia();
            }
        }

        private void VentanaProgreso_Closed(object sender, EventArgs e)
        {
            //Se evalua el valor devuelto por la ventana de progreso
            if (!vp.Resultado) //Si la ventana de progreso devuelve un valor falso, esto quiere decir que el usuario ha solicitado cancelar el establecimiento de la conexión por lo que se tomarán las medidas correspondientes
            {
                //Se activa de nuevo el botón de Inicializar de la estación correspondiente
                ActivarBoton(BotonInicializarConexionFisica, BordeBotonInicializarConexionFisica, Color.FromRgb(15, 216, 158));

                //Desactivamos el botón de finalizar la conexión física de la interfaz ya que se acaba de cancelar el establecimiento de la conexión física
                DesactivarBoton(BotonFinalizarConexionFisica, BordeBotonFinalizarConexionFisica, Color.FromRgb(244, 244, 244));

                //Desactivamos los botones de envío de tramas y activamos los botones de configuración
                EstadoBotonesEnvioTramas(false);
                EstadoBotonConfiguracion(true);

                //Creamos una tubería cliente con el objetivo de sacar a la tubería servidor correspondiente del bloqueo de espera y poder cerrar todas las instancias de tuberias abiertas
                NamedPipeClientStream pipeConexion = new NamedPipeClientStream(".", "tuberiaServidor" + estacion.NombreEstacion, PipeDirection.InOut);

                try
                {
                    //Nos conectamos a la tuberia servidora para sacarla de la espera bloqueada (WaitForConnnection()) y posteriormente cerrarla
                    pipeConexion.Connect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al establecer la conexión: {0}", ex.Message);
                }

                //Cerramos la instacia de la tubería servidor que ha sido sacada del bloqueo y de la instacia de la tubería del cliente que se acaba de crear
                pipeServidor.Close();
                pipeConexion.Close();
            }
        }

        private void BotonFinalizarConexionFisica_Click(object sender, RoutedEventArgs e)
        {
            //Se establece la situación de la estación correspondiente en Desconectado y se elimina la situación de la estación de la caja correspondiente
            estacion.SituacionEstacion = TipoSituaciónEstación.Desconectado;
            CajaSituacionEstacion.Text = "";

            //Se activa de nuevo el botón de Inicializar de la estacion correspondiente
            ActivarBoton(BotonInicializarConexionFisica, BordeBotonInicializarConexionFisica, Color.FromRgb(15, 216, 158));

            //Desactivamos el botón de finalizar la conexión física de la interfaz ya que el establecimiento de la conexión física ha finalizado para la estación correspondiente
            DesactivarBoton(BotonFinalizarConexionFisica, BordeBotonFinalizarConexionFisica, Color.FromRgb(244, 244, 244));

            //Desactivamos los botones de envío de tramas y activamos los botones de configuración
            EstadoBotonesEnvioTramas(false);
            EstadoBotonConfiguracion(true);

            //Se resetean los valores del número de secuencia y número de trama esperada
            estacion.NumeroSecuencia = 0;
            CajaNumeroSecuencia.Text = "";

            estacion.NumeroTramaEsperada = 0;
            CajaNumeroTramaEsperada.Text = "";

            //Se cancelan o finalizan los timeouts de manera que si alguno de ellos estaba activo, sea desactivado en el momento en el que finaliza la conexión física con la otra estación
            cancellationTokenSourceTimeoutRequest.Cancel();
            cancellationTokenSourceTimeoutRequest = new CancellationTokenSource();

            cancellationTokenSourceTimeoutTramaI.Cancel();
            cancellationTokenSourceTimeoutTramaI = new CancellationTokenSource();

            cancellationTokenSourceTimeoutCommand.Cancel();
            cancellationTokenSourceTimeoutCommand = new CancellationTokenSource();

            listaTramasPendientesRetransmision.Clear(); //Se limpia la lista de tramas pendientes de retransmisión
           
            //Se resetean valores del protocolo
            tamaño_ventana_actual = 0;
            numero_tramas_erroneas_consecutivas_recibidas = 0;

            //Se resetea el valor de las variables relacionadas con la gestion de los timeouts
            tareaTimeoutTramaIActivada = false;
            encolarRecepcionTrama = false;

            //Se resetean otras variables relacionadas con el funcionamiento del simulador
            turno = true;
            no_corregir = false;

            //Se para el timer y se restablece la referencia temporal
            timer.Stop(); 
            ReferenciaTemporal.Content = "00:00";

            //Se resetean las variables aspciadas a la referencia temporal
            segundos = 0;
            minutos = 0;
           
            //Se comprueba qué proceso que ha solicitado el fin del establecimiento de la conexión con el objetivo de que libere los recursos correspondientes
            try
            {
                if (segundaEstacion == false)
                {
                    if (sender.ToString() != "FIN") //Si el remitente de este método no es FIN, se enviará un mensaje de terminación (FIN) a la estación situada en el otro extremo para que finalice también la conexión física y libere los recursos correspondientes
                    {
                        StreamWriter sw = new StreamWriter(pipeConexion2.Tuberia);
                        sw.AutoFlush = true;
                        sw.WriteLine("FIN");
                    }

                    //Se cierran las instancias de tuberías creadas por el proceso correspodiente
                    pipeServidor.Close();
                    pipeConexion2.Close();
                }
                else
                {
                    if (sender.ToString() != "FIN") //Si el remitente de este método no es FIN, se enviará un mensaje de terminación (FIN) a la estación situada en el otro extremo para que finalice también la conexión física y libere los recursos correspondientes
                    {
                        StreamWriter sw = new StreamWriter(pipeConexion.Tuberia);
                        sw.AutoFlush = true;
                        sw.WriteLine("FIN");
                    }

                    //Se cierran las instancias de tuberías creadas por el proceso correspodiente
                    pipeServidor2.Close();
                    pipeConexion.Close();
                }

                if (sender.ToString() != "FIN") //Si el remitente de este método no es FIN, se motrará un mensaje indicando el correcto fin de la conexión física
                {
                    //Se imprime un mensaje indicando que la conexión física ha terminado correctamente
                    mostrarVentanaAdvertencia("Conexión finalizada correctamente.", false, "Información");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }

            segundaEstacion = false;
        }

        private void DesactivarBoton(Button boton, Border bordeBoton, Color color) 
        {
            boton.IsEnabled = false; //Se desactiva el botón pasado como parámetro, modificando la propiedad isEnabled del botón correspodiente
            bordeBoton.Background = new SolidColorBrush(color); //Se modifica el color de fondo del botón para que tenga el mismo color que el resto del botón
        }

        private void ActivarBoton(Button boton, Border bordeBoton, Color color) 
        {
            boton.IsEnabled = true; //Se activa el botón pasado como parámetro, modificando la propiedad isEnabled del botón correspodiente
            bordeBoton.Background = new SolidColorBrush(color); //Se modifica el color de fondo del botón para que tenga el mismo color que el resto del botón
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

        private void ActualizarSituaciónConexiónEstación(String tipoSituaciónEstación)
        {
            estacion.SituacionEstacion = tipoSituaciónEstación; //Se actualiza el valor de la situación de la estación con el valor recibido como parámetro
            CajaSituacionEstacion.Text = tipoSituaciónEstación; //Se actualiza la caja de la situación de la estación con el valor recibido como parámetro

            if (tipoSituaciónEstación == TipoSituaciónEstación.Excepción) //Si la situación de la estación cambia a Excepción
            {
                //Se resetean los valores del número de secuencia y número de trama esperada
                estacion.NumeroSecuencia = 0;
                estacion.NumeroTramaEsperada = 0;
                CajaNumeroSecuencia.Text = "0";
                CajaNumeroTramaEsperada.Text = "0";

                //Se cancelan o finalizan los timeouts (excepto el timeout ante request debido al envio de una trama FRMR) de manera que si alguno de ellos estaba activo, sea desactivado en el momento en el que finaliza la conexión con la otra estación
                cancellationTokenSourceTimeoutTramaI.Cancel();
                cancellationTokenSourceTimeoutTramaI = new CancellationTokenSource();

                cancellationTokenSourceTimeoutCommand.Cancel();
                cancellationTokenSourceTimeoutCommand = new CancellationTokenSource();
            }
            else if (tipoSituaciónEstación == TipoSituaciónEstación.Desconectado) //Si la situación de la estación cambia a Desconectado
            {
                //Se resetean los valores del número de secuencia y número de trama esperada
                estacion.NumeroSecuencia = 0;
                estacion.NumeroTramaEsperada = 0;
                CajaNumeroSecuencia.Text = "0";
                CajaNumeroTramaEsperada.Text = "0";

                //Se cancelan o finalizan los timeouts de manera que si alguno de ellos estaba activo, sea desactivado en el momento en el que finaliza la conexión con la otra estación
                cancellationTokenSourceTimeoutRequest.Cancel();
                cancellationTokenSourceTimeoutRequest = new CancellationTokenSource();

                cancellationTokenSourceTimeoutTramaI.Cancel();
                cancellationTokenSourceTimeoutTramaI = new CancellationTokenSource();

                cancellationTokenSourceTimeoutCommand.Cancel();
                cancellationTokenSourceTimeoutCommand = new CancellationTokenSource();
            }
            else if (tipoSituaciónEstación == TipoSituaciónEstación.Conectado) //Si la situación de la estación cambia a Conectado
            {
                //Se resetea el valor del tamaño de la ventana actual y el número de tramas errróneas consecutivas recibidas ya que se ha establecido una nueva conexión
                tamaño_ventana_actual = 0;
                numero_tramas_erroneas_consecutivas_recibidas = 0;

                //Se resetea el valor de las variables relacionadas con la gestion de los timeouts
                tareaTimeoutTramaIActivada = false;
                encolarRecepcionTrama = false;

                //Se resetean otras variables relacionadas con el funcionamiento del simulador
                turno = true;
                no_corregir = false;
            }
        }

        private void LimpiarInformaciónTramasEstación() 
        {
            listaTramasEnviadas.Clear(); //Se limpia la colección que almacena las tramas enviadas por la estación actual
            listaTramasRecibidas.Clear(); //Se limpia la colección que almacena las tramas recibias por la estación actual
            CanvasSeccionGrafica.Children.Clear(); //Se limpia el canvas en el que se representan gráficamente las tramas intercambiadas por la estación

            //Se vuelve a crear el eje de la estacion local
            EjeEstacionLocal = CrearLinea(57, 0, 57, 270, Brushes.Black, 2);
            CanvasSeccionGrafica.Children.Add(EjeEstacionLocal);

            //Se vuelve a crear el eje de la estacion contraria
            EjeEstacionContraria = CrearLinea(232, 0, 232, 270, Brushes.Black, 2);
            CanvasSeccionGrafica.Children.Add(EjeEstacionContraria);

            for (int i = 0; i < numero_tramas_dibujadas; i++) //Se desregistran las animaciones asociadas a cada una de las tramas representadas gráficamente 
            {
                UnregisterName("FlechaTrama" + i);
                UnregisterName("SobreTrama" + i);
            }

            //Se resetea el número de tramas enviadas, recibidas y dibujadas
            numero_tramas_dibujadas = 0;
            tramas_enviadas = 0;
            tramas_recibidas = 0;
        }

        private Line CrearLinea(int X1, int Y1, int X2, int Y2, Brush color, int strokeThickness)
        {
            //Se crea una línea con las coordenadas, el color y el grosor especificados como parámetros
            Line linea = new Line();
            linea.X1 = X1;
            linea.Y1 = Y1;
            linea.X2 = X2;
            linea.Y2 = Y2;
            linea.Stroke = color;
            linea.StrokeThickness = strokeThickness;

            return linea;
        }

        void EstadoBotonesEnvioTramas(bool estado)
        {
            //Se activa/desactiva cada uno de los botones que permiten el envío de tramas, modificando su propiedad IsEnabled
            BotonEnvioTramaInformacion.IsEnabled = estado;
            BotonEnvioTramaReceptorPreparado.IsEnabled = estado;
            BotonEnvioTramaReceptorNoPreparado.IsEnabled = estado;
            BotonEnvioTramaRechazo.IsEnabled = estado;
            BotonEnvioTramaRechazoSelectivo.IsEnabled = estado;
            BotonEnvioTramaSolicitudConexion.IsEnabled = estado;
            BotonEnvioTramaAsentimientoNoNumerado.IsEnabled = estado;
            BotonEnvioTramaSolicitudDesconexion.IsEnabled = estado;
            BotonEnvioTramaModoDesconectado.IsEnabled = estado;
            BotonEnvioTramaRechazoTrama.IsEnabled = estado;

            if (!estado) //Si se desactivan los botones para el envío de tramas, se modifica el color del fondo de los botones para que tengan un color gris
            {
                Color colorBorde = Color.FromRgb(244, 244, 244);
                BordeBotonEnvioTramaInformacion.Background = new SolidColorBrush(colorBorde);
            }
            else //Si se activan los botones para el envío de tramas, se modifica el color del fondo de los botones para que tengan el color original
            {
                Color colorBorde = Color.FromRgb(255, 219, 219);
                BordeBotonEnvioTramaInformacion.Background = new SolidColorBrush(colorBorde);
            }
        }

        void EstadoBotonConfiguracion(bool estado)
        {
            //Se activa/desactiva el botón de configuración, modificando su propiedad IsEnabled
            BotonConfiguracion.IsEnabled = estado;

            if (!estado) //Si se desactiva el botón de configuración, se modifica el color del fondo del botón para que tenga un color gris
            {
                Color colorBorde = Color.FromRgb(244, 244, 244);
                BordeBotonConfiguracion.Background = new SolidColorBrush(colorBorde);
            }
            else //Si se activa el botón de configuración, se modifica el color del fondo del botón para que tenga el color original
            {
                Color colorBorde = Color.FromRgb(207, 211, 255);
                BordeBotonConfiguracion.Background = new SolidColorBrush(colorBorde);
            }
        }

        private void CajaNombreEstacion_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            if (CajaNombreEstacion.SelectedItem != null)
            {
                estacion.NombreEstacion = CajaNombreEstacion.SelectedItem.ToString(); //Se actualiza el nombre de la estación con el nombre de estación elegido por el usuario en el ComboBox correspondiente
            }
        }

        private async void BotonEnvioTramaSolicitudConexion_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "C"; //Definimos por defecto el estado del bit C/R en C ya que una trama de solicitud de conexión (SABM) siempre es un comando
            int infoBitSondeo = 1; //Definimos por defecto el estado del bit P/F a 1 ya que una trama de solicitud de conexión (SABM) suele tener el bit de sondeo activado
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual
          
            if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Solicitud_conexion, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, true);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama 
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Solicitud_conexion, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, false);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            
            vvt.ShowDialog();
            if (vvt.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                //Comprobar la direccion de la estacion de la trama
                if (vvt.infoBitCommandRequest == "R") //Si el usuario configura la trama de solicitud de conexión (SABM) como una respuesta, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                else if (!turno) //Si la estación no tiene el turno, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El turno corresponde a la otra estación.", true, "Advertencia");
                }
                else if (estacion.SituacionEstacion != TipoSituaciónEstación.Desconectado && estacion.SituacionEstacion != TipoSituaciónEstación.Excepción) //Si la estación no se encuentra en una situación de Desconexión o Excepción, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de solicitud de conexión (SABM) solo puede enviarse cuando la situación de la estación sea de \"Desconectado\" o de \"Excepción\"", true, "Advertencia");
                }
                else
                {
                    //Se genera y envia la trama de solicitud de conexión (SABM) con la configuración elegida a la estación situada en el otro extremo
                    Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.No_numerada, TipoTrama.Solicitud_conexion);

                    if (!tr.TramaIncorrecta) //Si la trama SABM enviada no es incorrecta
                    {
                        turno = false; //Se establece el turno a falso ya que el turno corresponde a la otra estación hasta que responda a la solicitud de conexión

                        //Se actualiza la situación de la estación a Inicio Conexión
                        ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Inicio_conexión);
                    }
                }
            }
        }

        private async void BotonEnvioTramaAsentimientoNoNumerado_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "R"; //Definimos por defecto el estado del bit C/R en R ya que una trama de asentimiento no numerado (UA) siempre es una respuesta
            int infoBitSondeo = 0; //Definimos por defecto el estado del bit P/F a 0 ya que una trama de asentimiento no numerado (UA) suele tener el bit de sondeo desactivado
            if (ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo == 1 && ObtenerInformacionUltimaTramaRecibida().InfoBitCommandRequest == "C") //Si la ultima trama recibida tiene el bit de sondeo activado y es un comando, se definirá el estado del bit P/F a 1 con el objetivo de responder a la trama anterior
            {
                infoBitSondeo = 1;
            }
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual
         
            if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Asentimiento_no_numerado, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, true);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Asentimiento_no_numerado, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, false);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }

            vvt.ShowDialog();
            if (vvt.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                //Comprobar la direccion de la estacion de la trama
                if (vvt.infoBitCommandRequest == "C") //Si el usuario configura la trama de asentimiento no numerado (UA) como un comando, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                else if (estacion.SituacionEstacion != "Inicio conexión" && estacion.SituacionEstacion != "Inicio desconexión") //Si la estación no se encuentra en una situación de Inicio conexión o Inicio desconexión, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de asentimiento no numerado (UA) solo puede enviarse en respuesta a una solicitud de conexión (SABM) o una solicitud de desconexión (DISC)", true, "Advertencia");
                }
                else if (!turno) //Si la estación no tiene el turno, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El turno corresponde a la otra estación.", true, "Advertencia");
                }
                else if (ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo != vvt.infoBitSondeo) //Se revisa la correspondencia entre el bit P/F de la solicitud de conexion (SABM) y del asentimiento no numerado (UA)
                {
                    if (ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo == 1) //Si trama de solicitud de conexión (SABM) anterior tiene el bit P/F activado pero la trama de asentimiento no numerado (UA) no tiene el bit P/F activado, se cancela el envío de la trama
                    {
                        //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                        mostrarVentanaAdvertencia("Despues de recibirse una trama SABM(P) debe enviarse una trama UA(P).", true, "Advertencia");
                    }
                    else //Si trama de solicitud de conexión (SABM) anterior no tiene el bit P/F activado pero la trama de asentimiento no numerado (UA) tiene el bit P/F activado, se cancela el envío de la trama
                    {
                        //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                        mostrarVentanaAdvertencia("No se puede enviar una respuesta con el bit P/F activado sin haber recibido previamente un comando con el bit P/F activado.", true, "Advertencia");
                    }
                }
                else
                {                 
                    //Se genera y envia la trama de asentimiento no numerado (UA) con la configuración elegida a la estación situada en el otro extremo
                    Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.No_numerada, TipoTrama.Asentimiento_no_numerado);

                    if (!tr.TramaIncorrecta) //Si la trama UA enviada no es incorrecta
                    {
                        if (ObtenerInformacionUltimaTramaRecibida().TipoDeTrama == TipoTrama.Solicitud_conexion) //Si la trama previamente recibida es una trama solicitud de conexión (SABM), se actualiza la situación de la estación a Conectado
                        {
                            ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Conectado);
                        }
                        else if (ObtenerInformacionUltimaTramaRecibida().TipoDeTrama == TipoTrama.Solicitud_desconexion) //Si la trama previamente recibida es una trama solicitud de desconexión (DISC), se actualiza la situación de la estación a Desconectado
                        {
                            ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Desconectado);
                        }
                    }
                }
            }
        }

        private async void BotonEnvioTramaSolicitudDesconexion_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "C"; //Definimos por defecto el estado del bit C/R en C ya que una trama de solicitud de desconexión (DISC) siempre es un comando
            int infoBitSondeo = 1; //Definimos por defecto el estado del bit P/F a 1 ya que una trama de solicitud de desconexión (DISC) suele tener el bit de sondeo activado
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual
        
            if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Solicitud_desconexion, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, true);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Solicitud_desconexion, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, false);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }

            vvt.ShowDialog();
            if (vvt.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                //Comprobar la direccion de la estacion de la trama
                if (vvt.infoBitCommandRequest == "R") //Si el usuario configura la trama de solicitud de desconexión (DISC) como una respuesta, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                else if (!turno) //Si la estación no tiene el turno, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El turno corresponde a la otra estación.", true, "Advertencia");
                }
                else if (estacion.SituacionEstacion != TipoSituaciónEstación.Conectado) //Si la estación no se encuentra en una situación de Conectado, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de solicitud de desconexión (DISC) solo puede enviarse cuando la situación de la estación sea de \"Conectado\"", true, "Advertencia");
                }
                else
                {
                    //Se genera y envia la trama de solicitud de desconexión (DISC) con la configuración elegida a la estación situada en el otro extremo
                    Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.No_numerada, TipoTrama.Solicitud_desconexion);

                    if (!tr.TramaIncorrecta) //Si la trama DISC enviada no es incorrecta
                    {
                        turno = false; //Se establece el turno a falso ya que el turno corresponde a la otra estación hasta que responda a la solicitud de desconexión

                        //Se actualiza la situación de la estación a Inicio Desconexión
                        ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Inicio_desconexión);
                    }
                }
            }
        }

        private async void BotonEnvioTramaModoDesconectado_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "R"; //Definimos por defecto el estado del bit C/R en R ya que una trama de modo desconectado (DM) siempre es una respuesta
            int infoBitSondeo = 0; //Definimos por defecto el estado del bit P/F a 0 ya que una trama de modo desconectado (DM) suele tener el bit de sondeo desactivado
            if (ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo == 1 && ObtenerInformacionUltimaTramaRecibida().InfoBitCommandRequest == "C") //Si la ultima trama recibida tiene el bit de sondeo activado y es un comando, se definirá el estado del bit P/F a 1 con el objetivo de responder a la trama anterior
            {
                infoBitSondeo = 1;
            }
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual
          
            if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Modo_desconectado, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, true);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Modo_desconectado, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, false);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }

            vvt.ShowDialog();
            if (vvt.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                //Comprobar la direccion de la estacion de la trama
                if (vvt.infoBitCommandRequest == "C") //Si el usuario configura la trama de modo desconectado (DM) como un comando, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");

                }
                else if (estacion.SituacionEstacion != "Inicio conexión" && estacion.SituacionEstacion != "Inicio desconexión") //Si la estación no se encuentra en una situación de Inicio conexión o Inicio desconexión, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de modo desconectado (DM) solo puede enviarse en respuesta a una solicitud de conexión (SABM) o una solicitud de desconexión (DISC)", true, "Advertencia");
                }
                else if (!turno) //Si la estación no tiene el turno, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El turno corresponde a la otra estación.", true, "Advertencia");
                }
                else if (ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo != vvt.infoBitSondeo) //Se revisa la correspodencia entre el bit P/F de la solicitud de desconexión (DISC) y del modo desconectado (DM)
                {
                    if (ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo == 1) //Si trama de solicitud de desconexión (DISC) anterior tiene el bit P/F activado pero la trama de modo desconectado (DM) no tiene el bit P/F activado, se cancela el envío de la trama
                    {
                        //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                        mostrarVentanaAdvertencia("Despues de recibirse una trama DISC(P) debe enviarse una trama DM(P).", true, "Advertencia");
                    }
                    else //Si trama de solicitud de desconexión (DISC) anterior no tiene el bit P/F activado pero la trama de modo desconectado (DM) tiene el bit P/F activado, se cancela el envío de la trama
                    {
                        //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                        mostrarVentanaAdvertencia("No se puede enviar una respuesta con el bit P/F activado sin haber recibido previamente un comando con el bit P/F activado.", true, "Advertencia");
                    }
                }
                else
                {
                    //Se genera y envia la trama de modo desconectado (DM) con la configuración elegida a la estación situada en el otro extremo
                    Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.No_numerada, TipoTrama.Modo_desconectado);

                    if (!tr.TramaIncorrecta) //Si la trama DISC enviada no es incorrecta
                    {
                        //Se actualiza la situación de la estación a Desconectado
                        ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Desconectado);
                    }
                }
            }
        }

        private async void BotonEnvioTramaRechazoTrama_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "R"; //Definimos por defecto el estado del bit C/R en R ya que una trama de rechazo de trama (FRMR) siempre es una respuesta
            int infoBitSondeo = 0; //Definimos por defecto el estado del bit P/F a 0 ya que una trama de rechazo de trama (FRMR) suele tener el bit de sondeo desactivado
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual
            if (ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo == 1 && ObtenerInformacionUltimaTramaRecibida().InfoBitCommandRequest == "C" && ObtenerInformacionUltimaTramaRecibida().DireccionEstacion == nombreEstacionActual) //Si la trama anterior tiene el bit de sondeo activado y es un comando, se definirá el estado del bit P/F a 1 con el objetivo de responder a la trama anterior
            {
                infoBitSondeo = 1;
            }
           
            if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Rechazo_trama, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, true);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Rechazo_trama, direccionEstacion, infoBitCommandRequest, infoBitSondeo, -1, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, false);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }

            vvt.ShowDialog();
            if (vvt.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                //Comprobar la direccion de la estacion de la trama
                if (vvt.infoBitCommandRequest == "C") //Si el usuario configura la trama de rechazo de trama (FRMR) como un comando, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                else if (!turno) //Si la estación no tiene el turno, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El turno corresponde a la otra estación.", true, "Advertencia");
                }
                else if (estacion.SituacionEstacion != "Conectado" && estacion.SituacionEstacion != TipoSituaciónEstación.Excepción) //Si la estación no se encuentra en una situación de Conectado o Excepción, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de rechazo de trama (FRMR) solo puede enviarse cuando la situación de la estación sea de \"Conectado\" o de \"Excepción\"", true, "Advertencia");
                }
                else if (vvt.infoBitSondeo == 1 && (ObtenerInformacionUltimaTramaRecibida().InfoBitCommandRequest != "C" || ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo != 1 || ObtenerInformacionUltimaTramaRecibida().DireccionEstacion != nombreEstacionActual)) //Si no se ha recibido previamente un comando con el bit P/F activado y se pretende enviar una trama de rechazo de trama (FRMR) con el bit P/F activado, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("No se puede enviar una respuesta con el bit P/F activado sin haber recibido previamente un comando con el bit P/F activado.", true, "Advertencia");
                }
                else
                {
                    //Se actualiza la situación de la estación a Excepción
                    ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Excepción);

                    //Se genera y envia la trama de rechazo de trama (FRMR) con la configuración elegida a la estación situada en el otro extremo
                    Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.No_numerada, TipoTrama.Rechazo_trama);

                    if (!tr.TramaIncorrecta) //Si la trama FRMR enviada no es incorrecta
                    {
                        turno = false; //Se establece el turno a falso ya que el turno corresponde a la otra estación hasta que responda al rechazo de trama

                        //Se actualiza la situación de la estación a Excepción
                        ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Excepción);
                    }

                    //Se recuperan las tramas de información previas no confirmadas enviadas por la propia estación que envia el FRMR
                    RecopilarTramasPendientesConfirmacionFRMR(0);
                }
            }
        }

        private async void BotonEnvioTramaInformacion_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "C"; //Definimos por defecto el estado del bit C/R en C ya que una trama de información (I) siempre es un comando
            int infoBitSondeo = 0; //Definimos por defecto el estado del bit P/F a 0 ya que una trama de información (I) suele tener el bit de sondeo desactivado
            if (tamaño_ventana_actual == protocolo.TamañoVentana - 1) //Si el tamaño de la ventana esta a punto de agotarse, se activa el bit de sondeo ya que se necesita una confirmación de tramas por parte de la otra estación para poder seguir enviando mas tramas
            {
                infoBitSondeo = 1;
            }
            int numeroSecuencia = estacion.NumeroSecuencia; //Obtenemos el número de secuencia (NS) de la estación actual
            int numeroTramaEsperada = estacion.NumeroTramaEsperada; //Obtenemos el número de trama esperada (NR) de la estación actual
            String infoTrama = "La inspiración viene del cielo"; //Generamos un mensaje de prueba para adjuntarlo en las tramas de información
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual

            if (listaTramasPendientesRetransmision.Count == 0) //Si no existen tramas pendientes por reenviar
            {               
                if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
                {
                    //Creamos una instancia de la ventana de visualización de la trama de información a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de secuencia (NS), el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y un booleano que indica si la trama es reenviada o no. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                    vvti = new VentanaVisualizaciónTramaInformación(TipoTrama.Informacion, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroSecuencia, numeroTramaEsperada, infoTrama, nombreEstacionActual, protocolo, false, true);
                    vvti.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    vvti.Owner = this;
                }
                else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
                {
                    //Creamos una instancia de la ventana de visualización de la trama de información a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de secuencia (NS), el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y un booleano que indica si la trama es reenviada o no. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                    vvti = new VentanaVisualizaciónTramaInformación(TipoTrama.Informacion, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroSecuencia, numeroTramaEsperada, infoTrama, nombreEstacionActual, protocolo, false, false);
                    vvti.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    vvti.Owner = this;
                }
            }
            else //Si existen tramas pendientes por reenviar
            {
                int numero_trama_reenviar = listaTramasPendientesRetransmision.Count - 1;

                if (listaTramasPendientesRetransmision[numero_trama_reenviar].NumeroSecuencia != estacion.NumeroSecuencia && no_corregir == false) //Si la trama a reenviar tiene un número de secuencia que no corresponde con el número de secuencia de la estación, se corrige el número de secuencia de la trama reenviada
                { //OJO: Si se produce un rechazo de tramas, no interesa corregir el numero de secuencia de la trama reenviada por lo que la variable booleana no_corregir valdrá true y no se realizará la corrección
                    listaTramasPendientesRetransmision[numero_trama_reenviar].NumeroSecuencia = estacion.NumeroSecuencia;
                }
                if (listaTramasPendientesRetransmision[numero_trama_reenviar].NumeroTramaEsperada != estacion.NumeroTramaEsperada && no_corregir == false) //Si la trama a reenviar tiene un número de trama esperada que no corresponde con el número de trama esperada de la estación, se corrige el número de trama esperada de la trama reenviada
                { //OJO: Si se produce un rechazo de tramas, no interesa corregir el numero de trama esperada de la trama reenviada por lo que la variable booleana no_corregir valdrá true y no se realizará la corrección
                    listaTramasPendientesRetransmision[numero_trama_reenviar].NumeroTramaEsperada = estacion.NumeroTramaEsperada;
                }
               
                if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
                {
                    //Creamos una instancia de la ventana de visualización de la trama de información a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de secuencia (NS), el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y un booleano que indica si la trama es reenviada o no. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                    vvti = new VentanaVisualizaciónTramaInformación(TipoTrama.Informacion, listaTramasPendientesRetransmision[numero_trama_reenviar].DireccionEstacion, listaTramasPendientesRetransmision[numero_trama_reenviar].InfoBitCommandRequest, listaTramasPendientesRetransmision[numero_trama_reenviar].InfoBitSondeo, listaTramasPendientesRetransmision[numero_trama_reenviar].NumeroSecuencia, listaTramasPendientesRetransmision[numero_trama_reenviar].NumeroTramaEsperada, infoTrama, nombreEstacionActual, protocolo, true, true);
                    vvti.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    vvti.Owner = this;
                }
                else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
                {
                    //Creamos una instancia de la ventana de visualización de la trama de información a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de secuencia (NS), el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y un booleano que indica si la trama es reenviada o no. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                    vvti = new VentanaVisualizaciónTramaInformación(TipoTrama.Informacion, listaTramasPendientesRetransmision[numero_trama_reenviar].DireccionEstacion, listaTramasPendientesRetransmision[numero_trama_reenviar].InfoBitCommandRequest, listaTramasPendientesRetransmision[numero_trama_reenviar].InfoBitSondeo, listaTramasPendientesRetransmision[numero_trama_reenviar].NumeroSecuencia, listaTramasPendientesRetransmision[numero_trama_reenviar].NumeroTramaEsperada, infoTrama, nombreEstacionActual, protocolo, true, false);
                    vvti.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    vvti.Owner = this;
                }
            }

            vvti.ShowDialog();
            if (vvti.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                //Comprobar la direccion de la estacion de la trama
                if (vvti.infoBitCommandRequest == "R" || (ExisteComandoSinResponder() && vvti.infoBitSondeo == 1)) //Si el usuario configura la trama de información (I) como una respuesta, se cancelará el envío de la trama. Tambien se cancela el envio de la trama si la trama tiene el bit P/F activado y existe un comando pendiente de responder
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                else if (estacion.SituacionEstacion != TipoSituaciónEstación.Conectado) //Si la estación no se encuentra en una situación de Conectado, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de información (I) solo puede enviarse cuando la situación de la estación sea de \"Conectado\"", true, "Advertencia");
                }
                else
                {
                    if (!EstaPreparadoReceptor()) //Se comprueba si el receptor esta preparado para recibir tramas (se buscan tramas RNR recibidas previamente para determinar si el receptor esta preparado o no). Si el receptor no esta preparado para recibir tramas, se cancelará el envío de la trama
                    {
                        //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                        mostrarVentanaAdvertencia("No es posible enviar una trama de información por que el otro extremo no esta listo para recibir mas tramas", true, "Advertencia");
                    }
                    else
                    {
                        //Se obtiene el tamaño de la ventana
                        int tamaño_ventana = protocolo.TamañoVentana;

                        if (tamaño_ventana_actual >= tamaño_ventana) //Si se excede el tamaño de la ventana fijado en el protocolo, se cancelará el envío de la trama
                        {
                            //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                            mostrarVentanaAdvertencia("No es posible enviar mas tramas de información por que se ha agotado la ventana", true, "Advertencia");
                        }
                        else
                        {
                            //Se incrementa el número de secuencia (NS) de la estación, siempre y cuando la trama de información no sea reenviada debido a un rechazo previo (no_corregir seria true en ese caso)
                            if (!no_corregir) { 
                                incrementarNumeroSecuencia();
                            }

                            if (listaTramasPendientesRetransmision.Count > 0) //Si la lista de tramas pendientes no esta vacia, se elimina de la lista la ultima trama ya que va a ser enviada a continuación
                            {
                                listaTramasPendientesRetransmision.RemoveAt(listaTramasPendientesRetransmision.Count - 1);
                                if (listaTramasPendientesRetransmision.Count == 0) //Si la lista de tramas pendientes se vacia después de este envío, se pone a falso la variable booleana no_corregir ya que finalizo el envío de tramas pendientes después del rechazo
                                {
                                    no_corregir = false;
                                }
                            }

                            //Se genera y envia la trama de información (I) con la configuración elegida a la estación situada en el otro extremo
                            Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.Informacion, TipoTrama.Informacion);                           
                        }
                    }
                }
            }
            else //Si la trama a enviar se cancela, se borra la instancia de la ventana para evitar que otras tramas accedan al estado del bot´pn de CRC erróneo
            {
                vvti = null;
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

            if (indice == -1) //Si el indice vale -1, significa que no existe ningún comando con el bit P/F activado, por lo que no existe ningún comando sin responder y se devuelve false
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

        private async void BotonEnvioTramaReceptorPreparado_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "C"; //Definimos por defecto el estado del bit C/R en C ya que una trama de receptor preparado (RR) suele ser un comando
            int infoBitSondeo = 0; //Definimos por defecto el estado del bit P/F a 0 ya que una trama de receptor preparado (RR) suele tener el bit de sondeo desactivado
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual
            if (ExisteComandoSinResponder()) //Si existe un comando anterior con el bit P/F activado sin responder, se definirá el estado del bit P/F a 1 y el bit C/R a R con el objetivo de responder a la trama anterior
            {
                infoBitSondeo = 1;
                infoBitCommandRequest = "R";
            }
            int numeroTramaEsperada = estacion.NumeroTramaEsperada; //Obtenemos el número de trama esperada (NR) de la estación actual
           
            if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Receptor_preparado, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, true);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Receptor_preparado, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, false);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }

            vvt.ShowDialog();
            if (vvt.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                if (estacion.SituacionEstacion != TipoSituaciónEstación.Conectado) //Si la estación no se encuentra en una situación de Conectado, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de receptor preparado (RR) solo puede enviarse cuando la situación de la estación sea de \"Conectado\"", true, "Advertencia");
                }
                //Si el bit C/R de la trama vale C, y existe un comando previo sin responder, se cancelará el envío de la trama. //Se permite enviar un comando RR sin el bit de poll activado pues no es imprescindible responder inmediatamente al sondeo
                else if (vvt.infoBitCommandRequest == "C" && vvt.infoBitSondeo != 0 && ExisteComandoSinResponder())
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                //Si el bit C/R de la trama vale R, y no existe ningun comando sin responder o bien el bit de sondeo vale 0, se cancelará el envío de la trama
                else if (vvt.infoBitCommandRequest == "R" && (!ExisteComandoSinResponder() || vvt.infoBitSondeo == 0))
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                else
                {
                    //Se genera y envia la trama de receptor preparado (RR) con la configuración elegida a la estación situada en el otro extremo
                    Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }
            }
        }

        private async void BotonEnvioTramaReceptorNoPreparado_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "C"; //Definimos por defecto el estado del bit C/R en C ya que una trama de receptor no preparado (RNR) suele ser un comando
            int infoBitSondeo = 0; //Definimos por defecto el estado del bit P/F a 0 ya que una trama de receptor no preparado (RNR) suele tener el bit de sondeo desactivado
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual
            if (ExisteComandoSinResponder()) //Si existe un comando anterior con el bit P/F activado sin responder, se definirá el estado del bit P/F a 1 y el bit C/R a R con el objetivo de responder a la trama anterior
            {
                infoBitSondeo = 1;
                infoBitCommandRequest = "R";
            }
            int numeroTramaEsperada = estacion.NumeroTramaEsperada; //Obtenemos el número de trama esperada (NR) de la estación actual
            
            if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Receptor_no_preparado, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, true);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Receptor_no_preparado, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, false);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }

            vvt.ShowDialog();
            if (vvt.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                if (estacion.SituacionEstacion != TipoSituaciónEstación.Conectado) //Si la estación no se encuentra en una situación de Conectado, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de receptor no preparado (RNR) solo puede enviarse cuando la situación de la estación sea de \"Conectado\"", true, "Advertencia");
                }
                //Si el bit C/R de la trama vale C, y existe un comando previo sin responder, se cancelará el envío de la trama. //Se permite enviar un comando RNR sin el bit de poll activado pues no es imprescindible responder inmediatamente al sondeo
                else if (vvt.infoBitCommandRequest == "C" && vvt.infoBitSondeo != 0 && ExisteComandoSinResponder())
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                //Si el bit C/R de la trama vale R, y no existe ningun comando sin responder o bien el bit de sondeo vale 0, se cancelará el envío de la trama
                else if (vvt.infoBitCommandRequest == "R" && (!ExisteComandoSinResponder() || vvt.infoBitSondeo == 0))
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                else
                {
                    //Se genera y envia la trama de receptor no preparado (RNR) con la configuración elegida a la estación situada en el otro extremo
                    Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_no_preparado);
                }
            }
        }

        private async void BotonEnvioTramaRechazo_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "C"; //Definimos por defecto el estado del bit C/R en C ya que una trama de rechazo (REJ) suele ser un comando
            int infoBitSondeo = 0; //Definimos por defecto el estado del bit P/F a 0 ya que una trama de rechazo (REJ) suele tener el bit de sondeo desactivado
            if (ExisteComandoSinResponder()) //Si existe un comando anterior con el bit P/F activado sin responder, se definirá el estado del bit P/F a 1 y el bit C/R a R con el objetivo de responder a la trama anterior
            {
                infoBitSondeo = 1;
                infoBitCommandRequest = "R";
            }
            int numeroTramaEsperada = estacion.NumeroTramaEsperada; //Obtenemos el número de trama esperada (NR) de la estación actual
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual

            if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama 
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Rechazo, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, true);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Rechazo, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, false);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }

            vvt.ShowDialog();
            if (vvt.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                if (estacion.SituacionEstacion != TipoSituaciónEstación.Conectado) //Si la estación no se encuentra en una situación de Conectado, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de rechazo (REJ) solo puede enviarse cuando la situación de la estación sea de \"Conectado\"", true, "Advertencia");
                }
                //Si el bit C/R de la trama vale C, y existe un comando previo sin responder, se cancelará el envío de la trama. //Se permite enviar un comando REJ sin el bit de poll activado pues no es imprescindible responder inmediatamente al sondeo
                else if (vvt.infoBitCommandRequest == "C" && vvt.infoBitSondeo != 0 && ExisteComandoSinResponder())
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                //Si el bit C/R de la trama vale R, y no existe ningun comando sin responder o bien el bit de sondeo vale 0, se cancelará el envío de la trama
                else if (vvt.infoBitCommandRequest == "R" && (!ExisteComandoSinResponder() || vvt.infoBitSondeo == 0))
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                else
                {
                    //Se genera y envia la trama de rechazo (REJ) con la configuración elegida a la estación situada en el otro extremo
                    Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.Supervision, TipoTrama.Rechazo);
                }
            }
        }

        private async void BotonEnvioTramaRechazoSelectivo_Click(object sender, RoutedEventArgs e)
        {
            String direccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Obtenemos la dirección de la estación situada en el extremo contrario
            String infoBitCommandRequest = "C"; //Definimos por defecto el estado del bit C/R en C ya que una trama de rechazo selectivo (SREJ) suele ser un comando
            int infoBitSondeo = 0; //Definimos por defecto el estado del bit P/F a 0 ya que una trama de rechazo selectivo (SREJ) suele tener el bit de sondeo desactivado
            if (ExisteComandoSinResponder()) //Si existe un comando anterior con el bit P/F activado sin responder, se definirá el estado del bit P/F a 1 y el bit C/R a R con el objetivo de responder a la trama anterior
            {
                infoBitSondeo = 1;
                infoBitCommandRequest = "R";
            }
            int numeroTramaEsperada = estacion.NumeroTramaEsperada; //Obtenemos el número de trama esperada (NR) de la estación actual
            String nombreEstacionActual = estacion.NombreEstacion.LastOrDefault().ToString(); //Obtenemos el nombre la estación actual
         
            if (sender.ToString() == "ENVIO_DIRECTO") //Si el remitente de este método es ENVIO_DIRECTO, se cerrará la ventana de visualización de la trama nada mas abrirse para enviar de manera directa la trama correspondiente
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Rechazo_selectivo, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, true);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }
            else //En caso contrario, se mostrará de manera normal la ventana de visualización de la trama
            {
                //Creamos una instancia de la ventana de visualización de la trama a la cual le pasamos como parámetros el tipo de trama, la dirección de la estación de la trama, el estado del bit C/R, el estado del bit P/F, el número de trama esperada (NR), el nombre de la estación actual, información sobre el protocolo de la estación y la colección de tramas enviadas y recibidas por la estación. También se le pasa un boolenao para indicar si se desea o no un envío automático de la trama
                vvt = new VentanaVisualizaciónTrama(TipoTrama.Rechazo_selectivo, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroTramaEsperada, nombreEstacionActual, protocolo, listaTramasEnviadas, listaTramasRecibidas, false);
                vvt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvt.Owner = this;
            }

            vvt.ShowDialog();
            if (vvt.DialogResult == true) //Si el usuario confirma el deseo de enviar la trama con la configuración elegida
            {
                if (estacion.SituacionEstacion != TipoSituaciónEstación.Conectado) //Si la estación no se encuentra en una situación de Conectado, se cancelará el envío de la trama
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("Trama no válida en este contexto.\nLa trama de rechazo (REJ) solo puede enviarse cuando la situación de la estación sea de \"Conectado\"", true, "Advertencia");
                }
                //Si el bit C/R de la trama vale C, y existe un comando previo sin responder, se cancelará el envío de la trama. //Se permite enviar un comando SREJ sin el bit de poll activado pues no es imprescindible responder inmediatamente al sondeo
                else if (vvt.infoBitCommandRequest == "C" && vvt.infoBitSondeo != 0 && ExisteComandoSinResponder())
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                //Si el bit C/R de la trama vale R, y no existe ningun comando sin responder o bien el bit de sondeo vale 0, se cancelará el envío de la trama
                else if (vvt.infoBitCommandRequest == "R" && (!ExisteComandoSinResponder() || vvt.infoBitSondeo == 0))
                {
                    //Se imprime un mensaje indicando el motivo por el cual no se ha podido enviar la trama
                    mostrarVentanaAdvertencia("El campo address introducido no se corresponde con el tipo de trama a enviar.", true, "Advertencia");
                }
                else
                {
                    //Se genera y envia la trama de rechazo selectivo (SREJ) con la configuración elegida a la estación situada en el otro extremo
                    Trama tr = await GeneracionyEnvioTrama(TipoTramaControl.Supervision, TipoTrama.Rechazo_selectivo);
                }
            }
        }

        private async Task<Trama> GeneracionyEnvioTrama(String tipoTramaControl, String tipoTrama)
        {
            //Bloquear y encolar la recepción de tramas hasta que se termine de generar la trama (generación atómica de tramas)
            encolarRecepcionTrama = true;

            //Obtenemos el valor del retardo del canal y de la tasa de error
            int retardoCanal = canalTransmision.RetardoCanal;
            float tasaError = canalTransmision.TasaError;

            //Generamos una trama vacía que se represente en la tabla de tramas recibidas
            Trama tr = new Trama(true);
            listaTramasRecibidas.Add(tr);

            //Generamos la trama con los valores configurados por en la ventana de visualización de trama
            tr = generarTramaManual(tipoTramaControl, tipoTrama);

            //Obtenemos la referencia de tiempo y se lo asignamos a la trama
            DateTime fechaTrama = DateTime.UtcNow;
            TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
            tr.InstanteTemporal = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

            //Incrementamos el valor de la ventana actual en una unidad si la trama a enviar se trata de una trama de información y no es una trama reenviada
            if (tr.TipoDeTrama == TipoTrama.Informacion && tr.TramaReenviada == false)
            {
                tamaño_ventana_actual = tamaño_ventana_actual + 1;
            }

            //Si se trata de una trama de información, revisamos si el check box de trama errónea esta activo para generar una trama errónea en el caso que este activado
            if (vvti != null) //Primero se revisa si la instancia de la ventana de visualización de la trama de información es nula. Si es nula, en ningún caso podemos revisar el estado del check box
            {
                if (vvti.BotonCRCErroneo.IsChecked == true) //Si el check box de trama errónea esta activo, se eleva el valor de la tasa de error a 1, lo que nos producirá una trama errónea con total seguridad
                {
                    tasaError = 1;
                    vvti = null; //Eliminamos la instancia de la ventana para que futuras tramas no evaluen el estado pasado del botón de CRC erróneo
                }
            }
            tr = generarTramaErronea(tr, tasaError); //Aplicamos la tasa de error del canal para enviar las tramas con la probabilidad de error especificada

            //Si la trama generada no es incorrecta, aplicamos los timeouts correspodientes en el caso de que el envío de la trama actual desencadene algún timeout
            if (!tr.TramaIncorrecta)
            {
                AplicarTimeouts(tr);
            }

            //Convertimos la trama a un objeto JSON de manera que pueda ser enviada de manera limpia y directa por la tubería a la estación situada en el otro extremo
            String json = JsonConvert.SerializeObject(tr);

            //Añadimos la trama generada a la lista de tramas enviadas
            listaTramasEnviadas.Add(tr);

            //Permitir de nuevo la recepción de mensajes, pues la generación de la trama ya ha finalizado
            encolarRecepcionTrama = false;

            //Atender las recepciones de mensajes producidas (si se dieron) durante el bloqueo de recepción de mensajes
            AtenderRecepcionMensajePospuesta();

            //Aplicamos el retardo especificado para el canal (se crea un hilo asíncrono específico para realizar este cometido para evitar bloquear el proceso principal)
            await Task.Delay(retardoCanal);

            //Se envía la trama en formato JSON por la tubería correspodiente (en función de la estación que envia la trama se utilizará una tubería u otra)
            EnviarTrama(json);

            return tr; //Devolvemos la trama generada
        }

        private async void AplicarTimeouts(Trama tr)
        {          
            //Si la trama a enviar, no es una trama de asentimiento no numerado (UA), una trama de modo desconectado (DM) o una trama de rechazo de trama (FRMR) (ya que estas solo pueden ser respuestas)
            if (tr.TipoDeTrama != TipoTrama.Asentimiento_no_numerado && tr.TipoDeTrama != TipoTrama.Modo_desconectado && tr.TipoDeTrama != TipoTrama.Rechazo_trama) 
            {
                //Si la estación se encuentra en el modo de trabajo semiautomático y la trama a enviar además tiene el bit de sondeo activado y se trata de un comando, entonces se activa el timeout ante Command
                if (modoTrabajo.TipoModoDeTrabajo == "Semiautomático" && tr.InfoBitSondeo == 1 && tr.InfoBitCommandRequest == "C")
                {
                    SimboloTimeoutCommand.Visibility = Visibility.Visible; //Se hace visible un símbolo que indica que el timeout ante Command de la estación se encuentra activo
                      
                    await Task.Run(() => ImplementarTimeoutCommand(protocolo.TimeoutCommand, tr)); //Se crea un hilo asíncrono que se encargará de implementar el timeout ante Command de la trama correspondiente
                }
            }
            //Si la trama a enviar se trata de una trama de información (I) y no tiene el bit P/F activado (si lo tiene se aplica el timeout ante command)
            if (tr.TipoDeTrama == TipoTrama.Informacion && tr.InfoBitSondeo != 1)
            {
                //Si la estación se encuentra en el modo de trabajo semiautomático
                if (modoTrabajo.TipoModoDeTrabajo == "Semiautomático")
                {
                    //Se comprueba si ya existe un timeout ante Trama I activo en la estación, y en el caso de que así sea se impide lanzar otro timeout
                    if (!tareaTimeoutTramaIActivada) 
                    {
                        tareaTimeoutTramaIActivada = true; //Se marca como "activo" el timeout ante trama I
                        SimboloTimeoutTramaI.Visibility = Visibility.Visible; //Se hace visible un símbolo que indica que el timeout ante Trama I de la estación se encuentra activo

                        await Task.Run(() => ImplementarTimeoutTramaI(protocolo.TimeoutTramaI)); //Se crea un hilo asíncrono que se encargará de implementar el timeout ante Trama I de la trama correspondiente
                        //Cuando se envie la primera trama de RR debido al agotamiento del timeout se pasa al timeout ante command
                    }
                }
            }
            //Si la trama a enviar se trata de una trama de rechazo de trama (FRMR) y no tiene el bit P/F activado (si lo tiene se aplica el timeout ante command)
            if (tr.TipoDeTrama == TipoTrama.Rechazo_trama && tr.InfoBitSondeo != 1) 
            {
                //Si la estación se encuentra en el modo de trabajo semiautomático
                if (modoTrabajo.TipoModoDeTrabajo == "Semiautomático")
                {
                    SimboloTimeoutRequest.Visibility = Visibility.Visible; //Se hace visible un símbolo que indica que el timeout ante Request de la estación se encuentra activo

                    await Task.Run(() => ImplementarTimeoutRequest(protocolo.TimeoutRequest)); //Se crea un hilo asíncrono que se encargará de implementar el timeout ante Request de la trama correspondiente
                }
            }
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

        private Trama ObtenerInformacionUltimaTramaEnviada()
        {
            //Recorremos la lista de tramas enviadas desde el final, hasta encontrar una trama no vacia la cual se tratará de la última trama enviada (Nota: Las tramas vacias tiene el bit de sondeo con un valor de -1)
            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--)
            {
                if (listaTramasEnviadas[i].InfoBitSondeo >= 0)
                {
                    return listaTramasEnviadas[i];
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
            int indice_ultima_trama_enviada = -1;
            int indice_ultima_trama_recibida = -1;

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

        private bool EstaPreparadoReceptor()
        {
            //Recorremos la lista de tramas recibidas desde el final, en busca de tramas RR y RNR que nos permitan determinar si el receptor esta preparado o no para recibir tramas
            for (int i = listaTramasRecibidas.Count - 1; i >= 0; i--)
            {
                if (listaTramasRecibidas[i].TipoDeTrama == TipoTrama.Receptor_preparado) //Si encontramos primero una trama RR, significa que la estación situada en el otro extremo esta preparada para recibir tramas
                {
                    return true;
                }
                if (listaTramasRecibidas[i].TipoDeTrama == TipoTrama.Receptor_no_preparado) //Si encontramos primero una trama RNR, significa que la estación situada en el otro extremo no esta preparada para recibir tramas
                {
                    return false;
                }
                if (listaTramasRecibidas[i].TipoDeTrama == TipoTrama.Rechazo_trama || listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Rechazo_trama) //Si encontramos una trama FRMR en la lista de tramas enviadas o recibidas, no se tendra en cuenta el efecto de RNR anteriores por lo que finalizará la busqueda devolviendo true ya que no se encontró previamente ninguna RNR
                {
                    return true;
                }
            }

            //Si no encontramos tramas RR o RNR en la lista de tramas recibidas, se asume que la estación situada en el otro extremo esta preparada para recibir tramas
            return true;
        }

        private Trama generarTramaManual(String tipoTramaControl, String tipoTrama)
        {
            Trama tr = new Trama(tipoTramaControl); //Se crea una nueva trama (de información, de supervisión o no numerada en función de lo que se pase como parámetro)
            if (tipoTramaControl == TipoTramaControl.No_numerada) //Si la trama a generar es no numerada, los valores del número de secuencia (NS) y número de trama esperada (NR) no seran utilizados (-1)
            {
                tr.DireccionEstacion = vvt.direccionEstacion; //Se asignará al campo de dirección de la estación de la trama, el valor de la dirección de la estación almacenado en la ventana de visualización de la trama
                tr.InfoBitCommandRequest = vvt.infoBitCommandRequest; //Se asignará al bit C/R de la trama, el valor del bit C/R almacenado en la ventana de visualización de la trama
                tr.NumeroSecuencia = -1; //Se asignará al número de secuencia (NS) de la trama, el valor -1 pues las tramas no numeradas no tienen número de secuencia (NS)
                tr.InfoBitSondeo = vvt.infoBitSondeo; //Se asignará al bit P/F de la trama, el valor del bit P/F almacenado en la ventana de visualización de la trama
                tr.NumeroTramaEsperada = -1; //Se asignará al número de trama esperada (NR) de la trama, el valor -1 pues las tramas no numeradas no tienen número de trama esperada (NR)
                tr.TipoDeTrama = tipoTrama; //Se asignará al tipo de trama, el valor del tipo de trama recibido como parámetro 
                tr.generarCampoControl(); //Se generará el campo de control de la trama a partir de la información disponible sobre la trama
                tr.CRC = generarCRC(tr); //Se generará el CRC de la trama a partir de del campo de control, el campo de dirección y el campo de información de la trama
            }
            else if (tipoTramaControl == TipoTramaControl.Informacion) //Si la trama a generar es de información, se tendran en cuenta los valores del número de secuencia (NS) y número de trama esperada (NR)
            {
                tr.DireccionEstacion = vvti.direccionEstacion; //Se asignará al campo de dirección de la estación de la trama, el valor de la dirección de la estación almacenado en la ventana de visualización de la trama de información
                tr.InfoBitCommandRequest = vvti.infoBitCommandRequest; //Se asignará al bit C/R de la trama, el valor del bit C/R almacenado en la ventana de visualización de la trama de información
                tr.NumeroSecuencia = vvti.numeroSecuencia; //Se asignará al número de secuencia (NS) de la trama, el valor del número de secuencia (NS) almacenado en la ventana de visualización de la trama de información
                tr.InfoBitSondeo = vvti.infoBitSondeo; //Se asignará al bit P/F de la trama, el valor del bit P/F almacenado en la ventana de visualización de la trama de información
                tr.NumeroTramaEsperada = vvti.numeroTramaEsperada; //Se asignará al número de trama esperada (NR) de la trama, el valor del número de trama esperada (NR) almacenado en la ventana de visualización de la trama de información
                tr.TipoDeTrama = tipoTrama; //Se asignará al tipo de trama, el valor del tipo de trama recibido como parámetro 
                tr.InfoTrama = vvti.infoTrama; //Se asignará al campo de información de la trama, el valor de la información de la trama almacenado en la ventana de visualización de la trama de información
                tr.TramaReenviada = vvti.retransmision; //Si la trama generada es una retransmisión, se marcará la trama como reenviada (esto es importante porque las tramas reenviadas recibiran un tratamiento diferente)
                tr.generarCampoControl(); //Se generará el campo de control de la trama a partir de la información disponible sobre la trama
                tr.CRC = generarCRC(tr); //Se generará el CRC de la trama a partir de del campo de control, el campo de dirección y el campo de información de la trama

                //Si el tamaño actual de la ventana está a punto de agotarse y la estación se encuentra en el modo de trabajo semiautomático, entonces se activa el bit de sondeo con el objetivo de recibir confirmación de las tramas enviadas y poder ampliar así el tamaño de la ventana
                if (tamaño_ventana_actual == protocolo.TamañoVentana - 1 && modoTrabajo.TipoModoDeTrabajo == "Semiautomático") 
                {
                    tr.InfoBitSondeo = 1;
                }
            }
            else if (tipoTramaControl == TipoTramaControl.Supervision) //Si la trama a generar es de supervision, el valor del número de secuencia (NS) no sera utilizado (-1)
            {
                tr.DireccionEstacion = vvt.direccionEstacion; //Se asignará al campo de dirección de la estación de la trama, el valor de la dirección de la estación almacenado en la ventana de visualización de la trama
                tr.InfoBitCommandRequest = vvt.infoBitCommandRequest; //Se asignará al bit C/R de la trama, el valor del bit C/R almacenado en la ventana de visualización de la trama
                tr.NumeroSecuencia = -1; //Se asignará al número de secuencia (NS) de la trama, el valor -1 pues las tramas de supervisión no tienen número de secuencia (NS)
                tr.InfoBitSondeo = vvt.infoBitSondeo; //Se asignará al bit P/F de la trama, el valor del bit P/F almacenado en la ventana de visualización de la trama
                tr.NumeroTramaEsperada = vvt.numeroTramaEsperada; //Se asignará al número de trama esperada (NR) de la trama, el valor del número de trama esperada (NR) almacenado en la ventana de visualización de la trama
                tr.TipoDeTrama = tipoTrama; //Se asignará al tipo de trama, el valor del tipo de trama recibido como parámetro 
                tr.generarCampoControl(); //Se generará el campo de control de la trama a partir de la información disponible sobre la trama
                tr.CRC = generarCRC(tr); //Se generará el CRC de la trama a partir de del campo de control, el campo de dirección y el campo de información de la trama
            }

            return tr; //Se devuelve la trama generada
        }

        private Trama generarTramaErronea(Trama tr, float tasaError)
        {
            //Generamos un número aleatorio del 0 al 99 para decidir si la trama generada sera errónea o no
            Random random = new Random();
            int numero_aleatorio = random.Next(100);

            if (numero_aleatorio < tasaError * 100) //Si el número aleatorio generado es mas bajo que la probabilidad de error, la trama generada sera errónea
            {
                tr.CampoControl = "11111111"; //Introducimos un valor imposible para el campo de control de la trama
                tr.TramaIncorrecta = true; //Marcamos la trama enviada como incorrecta
            }

            return tr; //Devolvemos la trama errónea
        }

        private void EnviarTrama(String trama_json)
        {
            try //Se envia la trama convertida en un objeto JSON por la tuberia correspondiente
            {
                if (!segundaEstacion) //Si la estación que envía la trama, es la estación que solicitó en primer lugar la conexión física, se enviará el contenido de la trama a través de la tubería cliente creada por la propia estación (pipeConexion2)
                {
                    StreamWriter sw = new StreamWriter(pipeConexion2.Tuberia);
                    sw.AutoFlush = true;
                    sw.WriteLine(trama_json);
                }
                else //Si la estación que envía la trama, es la estación que solicitó en segundo lugar la conexión física, se enviará el contenido de la trama a través de la tubería cliente creada por la propia estación (pipeConexion)
                {
                    StreamWriter sw = new StreamWriter(pipeConexion.Tuberia);
                    sw.AutoFlush = true;
                    sw.WriteLine(trama_json);
                }
            }
            catch (Exception) //Si en el envío de la trama a través de la tubería correspondiente se produce algún error, se imprimirá un mensaje alertando de tal hecho
            {
                Console.WriteLine("ERROR -> Canalizacion cerrada");
            }
        }

        private String generarCRC(Trama trama)
        {
            //Obtenemos el valor del campo de la estación y del campo de control de la trama en formato binario (byte).
            byte campo_estacion = ObtenerByteDireccionEstacion(trama); //Para obtener el valor del campo de la estación en formato binario, utilizamos la función auxiliar ObtenerByteDireccionEstacion()
            byte campo_control = Convert.ToByte(trama.CampoControl, 2); //Para obtener el valor del campo de la estación en formato binario, utilizamos la función Convert.ToByte()

            //Añadimos el valor del campo de la estación y del campo de control de la trama a una lista de bytes
            List<Byte> data = new List<Byte>();
            data.Add(campo_estacion);
            data.Add(campo_control);

            //Si la trama es una trama de información, el campo de información de la trama puede contener información la cual será tenida en cuenta para calcular el CRC de la trama
            if (trama.InfoTrama != null)
            {
                byte[] campo_informacion_trama = Encoding.Default.GetBytes(trama.InfoTrama); //Para obtener el valor del campo de la información de la trama en formato binario, utilizamos la función Encoding.Default.GetBytes()

                for (int i = 0; i < campo_informacion_trama.Length; i++) //Reccorremos el array de bytes y vamos añadiendo el valor del campo de información de la trama a la lista de bytes
                {
                    data.Add(campo_informacion_trama[i]);
                }
            }

            //Calculamos el CRC de la trama proporcionando la información del campo de la estación y del campo de control (y del campo de información en el caso de que este no este vacio) en una lista
            string crc = CalcCRC16(data.ToArray());

            //Convertimos el CRC obtenido a formato binario (por defecto la funcion CalcCRC16 nos devuelve el valor del CRC en hexadecimal)
            crc = Convert.ToString(Convert.ToInt32(crc, 16), 2).PadLeft(16, '0');
            //Para realizar la conversión de hexadecimal a binario, primero se convierte el número hexadecimal en un numero entero a través del método Convert.ToInt32()
            //Despues, se convierte el número entero en un número binario utilizando el método Convert.ToString()
            //Por último, se ajusta la longitud del número binario a 16 bits añadiendo los 0s necesarios al final, utilizando la funcion PadLeft()

            return crc; //Se devuelve el CRC generado
        }

        public string CalcCRC16(byte[] data) 
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) > 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            return crc.ToString("X4");
        }

        private byte ObtenerByteDireccionEstacion(Trama tr)
        {
            //Si la trama tiene en el campo de dirección la estacion A, el valor binario de 8 bits asociado sera de 1 (00000001) y si la trama tiene en el campo de dirección la estacion B, el valor binario de 8 bits asociado sera de 2 (00000010)
            if (tr.DireccionEstacion.EndsWith("A"))
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        private void incrementarNumeroSecuencia()
        {
            if (estacion.NumeroSecuencia < 7) //Si el número de secuencia de la estación no supera el tamaño máximo de la numeración, se incrementa el valor del número de secuencia en una unidad
            {
                estacion.NumeroSecuencia = estacion.NumeroSecuencia + 1;
            }
            else //Si el número de secuencia de la estación supera el tamaño máximo de la numeración, el valor del número de secuencia pasa a valer 0
            {
                estacion.NumeroSecuencia = 0;
            }
            CajaNumeroSecuencia.Text = estacion.NumeroSecuencia.ToString(); //Se actualiza el valor de la caja del número de secuencia, mostrandose así el valor actualizado del número de secuencia de la estación
        }

        private void incrementarNumeroTramaEsperada()
        {
            if (estacion.NumeroTramaEsperada < 7) //Si el número de trama esperada de la estación no supera el tamaño máximo de la numeración, se incrementa el valor del número de trama esperada en una unidad
            {
                estacion.NumeroTramaEsperada = estacion.NumeroTramaEsperada + 1;
            }
            else //Si el número de trama esperada de la estación supera el tamaño máximo de la numeración, el valor del número de trama esperada pasa a valer 0
            {
                estacion.NumeroTramaEsperada = 0;
            }
            CajaNumeroTramaEsperada.Text = estacion.NumeroTramaEsperada.ToString(); //Se actualiza el valor de la caja del número de trama esperada, mostrandose así el valor actualizado del número de trama esperada de la estación
        }

        private void TablaTramasRecibidas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int indice = TablaTramasRecibidas.SelectedIndex; //Obtenemos el índice de la tabla que ha sido seleccionado por el usuario
            if (TablaTramasRecibidas.SelectedItem != null) //Comprobamos si efectivamente el usuario ha seleccioando un item de la tabla
            {
                MostrarDetalleTramaRecibida(indice); //Mostramos el detalle de la trama seleccionada por el usuario
            }
            TablaTramasRecibidas.SelectedItem = null; //Asignamos el valor null al elemento seleccionado para que así el elemento deje de estar seleccionado y el usuario pueda volver a seleccionar dicho elemento si así lo desea
        }

        private void TablaTramasEnviadas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int indice = TablaTramasEnviadas.SelectedIndex; //Obtenemos el índice de la tabla que ha sido seleccionado por el usuario
            if (TablaTramasEnviadas.SelectedItem != null) //Comprobamos si efectivamente el usuario ha seleccioando un item de la tabla
            {
                MostrarDetalleTramaEnviada(indice); //Mostramos el detalle de la trama seleccionada por el usuario
            }
            TablaTramasEnviadas.SelectedItem = null; //Asignamos el valor null al elemento seleccionado para que asi el elemento deje de estar seleccionado y el usuario pueda volver a seleccionar dicho elemento si así lo desea
        }

        private void CancelarTimeout(String tipoTimeout)
        {
            switch (tipoTimeout) //En función del tipo de timeout que se desee cancelar (pasado como parámetro) se cancelará el token asociado al timeout correspondiente
            {
                case "Command": //Si el timeout que se desea cancelar es el timeout ante Command, se cancela el token asociado al timeout ante command y despues se crea una nueva instancia del token
                    cancellationTokenSourceTimeoutCommand.Cancel();
                    break;
                case "TramaI": //Si el timeout que se desea cancelar es el timeout ante Trama I, se cancela el token asociado al timeout ante trama I y despues se crea una nueva instancia del token
                    cancellationTokenSourceTimeoutTramaI.Cancel();
                    break;
                case "Request": //Si el timeout que se desea cancelar es el timeout ante Request, se cancela el token asociado al timeout ante request y despues se crea una nueva instancia del token
                    cancellationTokenSourceTimeoutRequest.Cancel();
                    break;
            }
        }

        private void Server_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Si en el momento en el que se esta recibiendo el mensaje, la estación esta generando otra trama para su posterior envio, se guarda la trama recibida en una cola y posteriormente se realiza la recepción de la trama
            if (encolarRecepcionTrama) 
            {
                listaMensajes.Add(e.Message); //Añadimos la trama recibida en la cola, y posteriormente se realizaran las acciones necesarias para su correcta recepción
            }
            else
            {
                if (e.Message == "FIN") //Si el mensaje recibido es FIN, significa que el otro extremo de la conexión ha recibido la orden de finalizar la conexión física. De esta manera, al recibir este mensaje de terminación, la estación llama a BotonFinalizarConexionFisica_Click para finalizar la conexión y liberar las tuberias
                {
                    BotonFinalizarConexionFisica_Click("FIN", new RoutedEventArgs()); //Se llama a la función BotonFinalizarConexionFisica_Click donde el sender tiene el valor de FIN para distinguir el proceso que solicita el fin de la conexión del que lo recibe
                }
                else if (e.Message.StartsWith("GUARDAR_CAPTURA_TRAFICO")) //Si el mensaje empieza por GUARDAR_CAPTURA_TRAFICO, significa que el otro extremo de la conexión ha recibido la orden de guardar una captura de tráfico. De esta manera, al recibir este mensaje, la estación llama a BotonGuardarCapturaTrafico_Click para que el otro extremo tambien guarde la captura de tráfico
                {
                    BotonGuardarCapturaTrafico_Click(e.Message, new RoutedEventArgs()); //Se llama a la función BotonGuardarCapturaTrafico_Click donde el sender tiene el valor del mensaje recibido para distinguir el proceso que solicita el guardado del que lo recibe

                    EscucharTuberia(); //Una vez se ha terminado de realizar los procesamientos correspondientes a la recepción del mensaje, la estación vuelve a escuchar por la tubería correspondiente a la espera de recibir nuevos mensajes
                }
                else if (e.Message.StartsWith("CARGAR_CAPTURA_TRAFICO")) //Si el mensaje empieza por CARGAR_CAPTURA_TRAFICO, significa que el otro extremo de la conexión ha recibido la orden de cargar una captura de tráfico. De esta manera, al recibir este mensaje, la estación llama a BotonCargarCapturaTrafico_Click para que el otro extremo tambien cargue la captura de tráfico
                {
                    BotonCargarCapturaTrafico_Click(e.Message, new RoutedEventArgs()); //Se llama a la función BotonCargarCapturaTrafico_Click donde el sender tiene el valor del mensaje recibido para distinguir el proceso que solicita el cargado del que lo recibe

                    EscucharTuberia(); //Una vez se ha terminado de realizar los procesamientos correspondientes a la recepción del mensaje, la estación vuelve a escuchar por la tubería correspondiente a la espera de recibir nuevos mensajes
                }
                else 
                {
                    try
                    {
                        //Generamos una trama vacía que se represente en la tabla de tramas enviadas
                        Trama tr = new Trama(true);
                        listaTramasEnviadas.Add(tr);

                        //Se convierte el objeto JSON recibido a traves de la tubería de nuevo en un objeto Trama
                        tr = JsonConvert.DeserializeObject<Trama>(e.Message);

                        //Se calcula el instante temporal en el que se recibe la trama (el cual es distinto al del origen)
                        DateTime fechaTrama = DateTime.UtcNow;
                        TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
                        tr.InstanteTemporal = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

                        //Se añade la trama recibida a la lista de tramas recibidas
                        listaTramasRecibidas.Add(tr);

                        //Se calcula el CRC de la trama recibida y se compara con CRC recibido en la trama
                        if (generarCRC(tr) != tr.CRC) //Si el CRC de la trama es disinto al CRC calculado de la trama recibida, entonces la trama será incorrecta
                        {
                            //Se marca la trama como incorrecta y se asigna al campo de CRC de la trama el valor calculado del CRC
                            tr.TramaIncorrecta = true;
                            tr.CRC = generarCRC(tr);

                            numero_tramas_erroneas_consecutivas_recibidas = numero_tramas_erroneas_consecutivas_recibidas + 1; //Se incrementa en una unidad el número de tramas erróneas consecutivas recibidas

                            int numero_maximo_tramas_erroneas_consecutivas_recibidas = protocolo.NumeroMaximoTramasErroneasConsecutivasPermitidas; //Se obtiene el número máximo de tramas erróneas consecutivas permitido por la estación
                            if (numero_tramas_erroneas_consecutivas_recibidas == numero_maximo_tramas_erroneas_consecutivas_recibidas) //Se comprueba si se ha excedido el número máximo de tramas erróneas consecutivas recibidas
                            {
                                //En el caso de que se exceda el número máximo tramas erróneas consecutivas recibidas, se procede a finalizar la conexión física de ambas estaciones y mostrar un mensaje de advertencia indicando de la situación
                                BotonFinalizarConexionFisica_Click(sender, new RoutedEventArgs());

                                mostrarVentanaAdvertencia("Se ha excedido el número máximo de tramas erróneas consecutivas permitidas.\nDebido a esto, finaliza la conexión física de ambas estaciones.", true, "Advertencia");
                            }
                        }
                        else
                        {
                            //En el caso de que la trama recibida no sea errónea, se resetea el contador del número de tramas erróneas consecutivas recibidas
                            numero_tramas_erroneas_consecutivas_recibidas = 0;

                            switch (tr.TipoDeTrama) //En función del tipo de trama recibida, se realizarán las acciones correspondientes al tipo de trama recibida
                            {
                                case TipoTrama.Solicitud_conexion: //Si la trama recibida se trama de una trama de solicitud de conexion (SABM)
                                    RecibirTramaSolicitudConexion(tr);
                                    break;
                                case TipoTrama.Asentimiento_no_numerado: //Si la trama recibida se trama de una trama de asentimiento no numerado (UA)
                                    RecibirTramaAsentimientoNoNumerado(tr);
                                    break;
                                case TipoTrama.Solicitud_desconexion: //Si la trama recibida se trama de una trama de solicitud de desconexion (DISC)
                                    RecibirTramaSolicitudDesconexion(tr);
                                    break;
                                case TipoTrama.Modo_desconectado: //Si la trama recibida se trama de una trama de modo desconectado (DM)
                                    RecibirTramaModoDesconectado(tr);
                                    break;
                                case TipoTrama.Rechazo_trama: //Si la trama recibida se trama de una trama de rechazo de trama (FRMR)
                                    RecibirTramaRechazoTrama(tr);
                                    break;
                                case TipoTrama.Informacion: //Si la trama recibida se trama de una trama de información (I)
                                    RecibirTramaInformacion(tr);
                                    break;
                                case TipoTrama.Receptor_preparado: //Si la trama recibida se trama de una trama de receptor preparado (RR)
                                    RecibirTramaReceptorPreparado(tr);
                                    break;
                                case TipoTrama.Receptor_no_preparado: //Si la trama recibida se trama de una trama de receptor no preparado (RNR)
                                    RecibirTramaReceptorNoPreparado(tr);
                                    break;
                                case TipoTrama.Rechazo: //Si la trama recibida se trama de una trama de rechazo (REJ)
                                    RecibirTramaRechazo(tr);
                                    break;
                                case TipoTrama.Rechazo_selectivo: //Si la trama recibida se trama de una trama de rechazo selectivo (SREJ)
                                    RecibirTramaRechazoSelectivo(tr);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex) //Si en la recepción se produce cualquier tipo de error, se terminará el proceso de recepción y se imprimirá un mensaje de error
                    {
                        Console.WriteLine("ERROR: {0}", ex.Message);
                    }

                    EscucharTuberia(); //Una vez se ha terminado de realizar los procesamientos correspondientes a la recepción del mensaje, la estación vuelve a escuchar por la tubería correspondiente a la espera de recibir nuevos mensajes
                }
            }
        }

        private void EscucharTuberia()
        {
            if (segundaEstacion) //En función de la estación se escucha por una tubería servidora u otra con el objetivo de esperar la recepción de nuevos mensajes a través de la tubería
            {
                pipeServidor2.EscucharTuberia();
            }
            else
            {
                pipeServidor.EscucharTuberia();
            }
        }

        private void RecibirTramaSolicitudConexion(Trama tr)
        {
            //Se actualiza la situación de la estación receptora a Inicio conexión
            ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Inicio_conexión);

            //Si la última trama enviada por la estación es una trama de rechazo de trama, entonces se cancela el timeout ante request pues se ha recibido un comando (SABM) a la respuesta (FRMR) enviada previamente por la estación
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (ObtenerInformacionUltimaTramaEnviada().TipoDeTrama == TipoTrama.Rechazo_trama && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("Request");
            }

            //Si la estación se encuentra en el modo de trabajo semiautomático, esta reenviará automáticamente una trama de asentimiento no numerado
            if (modo_trabajo == "Semiautomático")
            {
                generarRespuestaAutomaticaTrama(TipoTramaControl.No_numerada, TipoTrama.Asentimiento_no_numerado);

                ReenviarTramasInformacionPendientes(); //Se reenviarán automáticamente las tramas pendientes por retransmitir (si hay)
            }

            turno = true; //La estación vuelve a recuperar el turno para enviar tramas (la estación perdío el turno previamente cuando envío una trama FRMR)
        }

        private void RecibirTramaAsentimientoNoNumerado(Trama tr)
        {
            //Se actualiza la situación de la estación receptora a Conectado o Desconectado dependiendo si la trama que precede a la trama de asentimiento numerado es de solicitud de conexión o solicitud de desconexión
            if (ObtenerInformacionUltimaTramaEnviada().TipoDeTrama == TipoTrama.Solicitud_conexion)
            {
                ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Conectado);
            }
            else if (ObtenerInformacionUltimaTramaEnviada().TipoDeTrama == TipoTrama.Solicitud_desconexion)
            {
                ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Desconectado);
            }

            //Si la trama recibida tiene el bit de sondeo activado y se trata de una respuesta, entonces se cancelará el timeout ante command pues se ha recibido una respuesta a un comando previo con el bit de poll activado
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (tr.InfoBitSondeo == 1 && tr.InfoBitCommandRequest == "R" && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("Command");
            }

            turno = true; //La estación vuelve a recuperar el turno para enviar tramas (la estación perdío el turno previamente cuando envío una trama SABM o DISC)
        }

        private void RecibirTramaSolicitudDesconexion(Trama tr)
        {
            //Se actualiza la situación de la estación receptora a Inicio desconexión
            ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Inicio_desconexión);

            //Si la estación se encuentra en el modo de trabajo semiautomático, esta reenviará automáticamente una trama de modo desconectado
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (modo_trabajo == "Semiautomático")
            {
                generarRespuestaAutomaticaTrama(TipoTramaControl.No_numerada, TipoTrama.Modo_desconectado);
            }
        }

        private void RecibirTramaModoDesconectado(Trama tr)
        {
            //Se actualiza la situación de la estación receptora a Desconectado
            ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Desconectado);

            //Si la trama recibida tiene el bit de sondeo activado y se trata de una respuesta, entonces se cancelará el timeout ante command pues se ha recibido una respuesta a un comando previo con el bit de poll activado
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (tr.InfoBitSondeo == 1 && tr.InfoBitCommandRequest == "R" && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("Command");
            }

            turno = true; //La estación vuelve a recuperar el turno para enviar tramas (la estación perdío el turno previamente cuando envío una trama DISC)
        }

        private void RecibirTramaRechazoTrama(Trama tr)
        {
            //Se actualiza la situación de la estación receptora a Excepción
            ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Excepción);

            //Recuperar tramas información previas no confirmadas enviadas por la propia estación que envia el FRMR
            RecopilarTramasPendientesConfirmacionFRMR(0);

            //Si la estación se encuentra en el modo de trabajo semiautomático, esta reenviará automáticamente una trama de solicitud de conexión
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (modo_trabajo == "Semiautomático")
            {
                generarRespuestaAutomaticaTrama(TipoTramaControl.No_numerada, TipoTrama.Solicitud_conexion);
            }
        }

        private void RecopilarTramasPendientesConfirmacionFRMR(int FRMR)
        {
            int contador_FRMR = FRMR;
            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Recorremos la lista de tramas enviadas desde el final
            {
                //Si en el recorrido encontramos una trama de información que no haya sido reenviada y este pendiente de confirmar, incluimos dicha trama a la lista de tramas pendientes de retransmisión
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].TramaReenviada == false && listaTramasEnviadas[i].TramaConfirmada == false)
                {
                    listaTramasPendientesRetransmision.Add(listaTramasEnviadas[i]);
                }
                //Si en el recorrido nos encontramos una trama de rechazo de trama por segunda vez (la primera FRMR es la que se acaba de producir) (ya bien sea enviada o recibida por la propia estación), finalizamos la busqueda ya que no se tienen en cuenta las tramas de información previas a un segundo restablecimiento del enlace anterior
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Rechazo_trama || listaTramasRecibidas[i].TipoDeTrama == TipoTrama.Rechazo_trama)
                {
                    contador_FRMR = contador_FRMR + 1;
                    if (contador_FRMR >= 2)
                    {
                        return;
                    }                
                }
            }
        }

        private void RecibirTramaInformacion(Trama tr)
        {
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (modo_trabajo == "Semiautomático" && tr.NumeroSecuencia != estacion.NumeroTramaEsperada && tr.TramaReenviada == false) //Si el número de secuencia de la trama (NS) no coincide con el número de trama esperada (NR) de la estación receptora, entonces se generará automáticamente un trama de rechazo solicitando la trama o tramas que faltan por recibirse
            {
                generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Rechazo);
            }
            else if (tr.NumeroTramaEsperada != estacion.NumeroSecuencia && !EsTramaAnteriorSinConfirmar(tr)) //Si el número de trama esperada (NR) de la trama no coincide con el número de secuencia (NS) de la estación receptora y no se trata de una confirmación de una trama anteriormente enviada, entonces se producirá un estado de excepción ya que se ha producido una pérdida irrecuperable de tramas
            {
                //Se actualiza la situación de la estación receptora a Excepción
                ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Excepción);

                //Si la estación se encuentra en el modo de trabajo semiautomático, esta reenviará automaticamente una trama de rechazo de trama para restablecer de nuevo el enlace
                if (modo_trabajo == "Semiautomático")
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.No_numerada, TipoTrama.Rechazo_trama);
                }
            }
            else if (tr.NumeroTramaEsperada != estacion.NumeroSecuencia && EsTramaAnteriorSinConfirmar(tr)) //Si el número de trama esperada (NR) de la trama no coincide con el número de secuencia (NS) de la estación receptora y se trata de una confirmación de una trama anteriormente enviada, entonces la recepción es correcta se aumentará el tamaño de la ventana en función del número de tramas confirmadas y se incrementará en una unidad el número de trama esperada de la estación receptora
            {
                //Se actualiza el tamaño de la ventana en función del valor actual y el número de tramas confirmadas a través de la recepción de la trama de información
                tamaño_ventana_actual = tamaño_ventana_actual - ConfirmarTramasInformaciónEnviadas(tr); //Se confirman las tramas pendientes de acuerdo con el número de trama esperada recibido en la trama de información

                incrementarNumeroTramaEsperada(); //Se incrementa en una unidad el número de trama esperada (NR) de la estación

                //Si la estación se encuentra en el modo de trabajo semiautomático y el bit de sondeo de la trama de información recibida esta activo, esta reenviará automáticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1)
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }
            }
            else if (tr.NumeroSecuencia == estacion.NumeroTramaEsperada && tr.NumeroTramaEsperada == estacion.NumeroSecuencia) //Si el número de secuencia de la trama (NS) coincide con el número de trama esperada (NR) de la estación receptora y el número de trama esperada de la trama coincide con el número de secuencia de la estación, la trama de información recibida a priori es correcta por lo que se incrementa en una unidad el número de trama esperada de la estación receptora
            {
                ConfirmarTramasInformaciónEnviadas(tr); //Se confirman todas las tramas pendientes por confirmar (ya que el número de trama esperada de la trama coincide con el número de secuencia de la estación)
                tamaño_ventana_actual = 0; //Se resetea el tamaño actual de la ventana, ya que al recibir una trama de información donde el número de trama esperada de la trama coincide con el número de secuencia de la estación se confirman todas las tramas anteriores

                incrementarNumeroTramaEsperada(); //Se incrementa en una unidad el número de trama esperada (NR) de la estación

                //Si la estación se encuentra en el modo de trabajo semiautomático y el bit de sondeo de la trama de información recibida esta activo, esta reenviará automáticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1)
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }
            }
                   
            //Si el número de trama esperada de la trama recibida coincide con el número de secuencia de la estación, entonces se cancelará el timeout ante trama I pues se han confirmado las tramas de información pendientes por confirmar
            if (tr.NumeroTramaEsperada == estacion.NumeroSecuencia && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("TramaI");
            }
        }

        private int ConfirmarTramasInformaciónEnviadas(Trama tr)
        {
            int indice = -1;
            int numero_tramas_confirmadas = 0;
            int numero_trama_esperada_anterior;

            if (tr.NumeroTramaEsperada == 0)
            {
                numero_trama_esperada_anterior = 7;
            }
            else
            {
                numero_trama_esperada_anterior = tr.NumeroTramaEsperada - 1;
            }

            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Recorremos la lista de tramas enviadas desde el final
            {               
                //Si en el recorrido nos encontramos una trama cuyo número de secuencia coincide con el número anterior de trama esperada de la trama recibida y dicha trama encontrada no esta confirmada, entonces obtiene el indice de la trama en la lista y finaliza el recorrido ya se ha encontrado una trama sin confirmar con dicho número de secuencia
                if (listaTramasEnviadas[i].NumeroSecuencia == numero_trama_esperada_anterior && listaTramasEnviadas[i].TramaConfirmada == false)
                {
                    indice = i;
                    break;
                }
                //Si la trama recibida es una SREJ hay que confirmar todas las tramas pendientes de confirmar menos la trama indicada en el número de trama esperada 
                //Con el resto de tramas, solo hay que confirmar las tramas anteriores al número de trama esperada de la trama que estén sin confirmar
                if (tr.TipoDeTrama == TipoTrama.Rechazo_selectivo && listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].TramaConfirmada == false)
                {
                    listaTramasEnviadas[i].TramaConfirmada = true;
                    numero_tramas_confirmadas = numero_tramas_confirmadas + 1;
                }
                //Si en el recorrido nos encontramos una trama de rechazo de trama (ya bien sea enviada o recibida por la propia estación), finalizamos la busqueda devolviendo 0 ya que no tiene sentido confirmar tramas de información previas al restablecimiento del enlace
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Rechazo_trama || listaTramasRecibidas[i].TipoDeTrama == TipoTrama.Rechazo_trama)
                {
                    return 0;
                }
            }

            if (indice == -1) //Si el índice vale -1, significa que no se ha encontrado la trama buscada, por lo que no se ha confirmado ninguna nueva trama
            {
                return 0;
            }


            for (int i = indice; i >= 0; i--) //Recorremos la lista de tramas enviadas desde el índice obtenido anteriormente
            {
                //Si en el recorrido encontramos una trama de información ya confirmada, entonces finaliza el recorrido ya que no quedan tramas pendientes de confirmación
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].TramaConfirmada == true)
                {
                    break;
                }
                //Si en el recorrido encontramos una trama de información pendiente de confirmar, confirmamos dicha trama e incrementamos en una unidad el numero de tramas confirmadas
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].TramaConfirmada == false) {
                    listaTramasEnviadas[i].TramaConfirmada = true;
                    numero_tramas_confirmadas = numero_tramas_confirmadas + 1;
                }              
                //Si en el recorrido nos encontramos una trama de rechazo de trama (ya bien sea enviada o recibida por la propia estación), finalizamos la busqueda devolviendo 0 ya que no tiene sentido confirmar tramas de información previas al restablecimiento del enlace
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Rechazo_trama || listaTramasRecibidas[i].TipoDeTrama == TipoTrama.Rechazo_trama)
                {
                    return 0;
                }
            }

            return numero_tramas_confirmadas; //Devolvemos el número de tramas confirmadas
        }

        private bool EsTramaAnteriorSinConfirmar(Trama tr)
        {
            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Recorremos la lista de tramas enviadas desde el final
            {
                //Si en el recorrido nos encontramos una trama cuyo número de secuencia coincide con el número de trama esperada de la trama recibida y dicha trama encontrada no esta confirmada, entonces se devuelve true ya se ha encontrado una trama sin confirmar con dicho número de secuencia
                if (listaTramasEnviadas[i].NumeroSecuencia == tr.NumeroTramaEsperada && listaTramasEnviadas[i].TramaConfirmada == false) 
                {
                    return true;
                }
                //Si en el recorrido nos encontramos una trama de rechazo de trama (ya bien sea enviada o recibida por la propia estación), finalizamos la busqueda devolviendo falso ya que no se tienen en cuenta las tramas de información previas al restablecimiento del enlace
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Rechazo_trama || listaTramasRecibidas[i].TipoDeTrama == TipoTrama.Rechazo_trama)
                {
                    return false;
                }
            }

            //Si el recorrido finaliza y no se ha encontrado la trama sin confirmar buscada, también se devuelve falso
            return false;
        }

        private void RecibirTramaReceptorPreparado(Trama tr)
        {
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (tr.NumeroTramaEsperada != estacion.NumeroSecuencia && !EsTramaAnteriorSinConfirmar(tr)) //Si el número de trama esperada (NR) de la trama no coincide con el número de secuencia (NS) de la estación receptora y no se trata de una confirmación de una trama anteriormente enviada, entonces se producirá un estado de excepción ya que se ha producido una pérdida irrecuperable de tramas
            {
                //Se actualiza la situación de la estación receptora a Excepción
                ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Excepción);

                //Si la estación se encuentra en el modo de trabajo semiautomático, esta reenviará automáticamente una trama de rechazo de trama para restablecer de nuevo el enlace
                if (modo_trabajo == "Semiautomático")
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.No_numerada, TipoTrama.Rechazo_trama);
                }
            }
            else if (tr.NumeroTramaEsperada != estacion.NumeroSecuencia && EsTramaAnteriorSinConfirmar(tr)) //Si el número de trama esperada (NR) de la trama no coincide con el número de secuencia (NS) de la estación receptora y se trata de una confirmación de una trama anteriormente enviada, entonces la recepción es correcta se aumentará el tamaño de la ventana en función del número de tramas confirmadas
            {
                //Se actualiza el tamaño de la ventana en función del valor actual y el número de tramas confirmadas a través de la recepción de la trama de receptor preparado
                tamaño_ventana_actual = tamaño_ventana_actual - ConfirmarTramasInformaciónEnviadas(tr); //Se confirman las tramas pendientes de acuerdo con el número de trama esperada recibido en la trama de receptor preparado

                //Si la estación se encuentra en el modo de trabajo semiautomático, el bit de sondeo de la trama RR esta activado y se trata de un comando, esta reenviará automáticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1 && tr.DireccionEstacion == estacion.NombreEstacion.LastOrDefault().ToString())
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }
            }           
            else if (tr.NumeroTramaEsperada == estacion.NumeroSecuencia) //Si el número de trama esperada (NR) coincide con el número de secuencia de la estación, entonces la recepción es correcta y se reseteará el tamaño de la ventana y confirmaran las tramas correspodientes
            {
                ConfirmarTramasInformaciónEnviadas(tr); //Se confirman todas las tramas pendientes por confirmar (ya que el número de trama esperada de la trama coincide con el número de secuencia de la estación)
                tamaño_ventana_actual = 0; //Se resetea el tamaño actual de la ventana, ya que al recibir una trama de receptor preparado donde el número de trama esperada de la trama coincide con el número de secuencia de la estación se confirman todas las tramas anteriores

                //Si la estación se encuentra en el modo de trabajo semiautomático, el bit de sondeo de la trama RR esta activado y se trata de un comando, esta reenviará automáticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1 && tr.DireccionEstacion == estacion.NombreEstacion.LastOrDefault().ToString())
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }
            }
                     
            //Si la trama recibida tiene el bit de sondeo activado y se trata de una respuesta, entonces se cancelará el timeout ante command pues se ha recibido una respuesta a un comando previo con el bit de poll activado
            if (tr.InfoBitSondeo == 1 && tr.InfoBitCommandRequest == "R" && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("Command");
            }
            //Si el número de trama esperada de la trama recibida coincide con el número de secuencia de la estación, entonces se cancelará el timeout ante trama I pues se han confirmado las tramas de información pendientes por confirmar
            if (tr.NumeroTramaEsperada == estacion.NumeroSecuencia && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("TramaI");
            }
        }

        private void RecibirTramaReceptorNoPreparado(Trama tr)
        {
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (tr.NumeroTramaEsperada != estacion.NumeroSecuencia && !EsTramaAnteriorSinConfirmar(tr)) //Si el número de trama esperada (NR) de la trama no coincide con el número de secuencia (NS) de la estación receptora y no se trata de una confirmación de una trama anteriormente enviada, entonces se producirá un estado de excepción ya que se ha producido una pérdida irrecuperable de tramas
            {
                //Se actualiza la situación de la estación receptora a Excepción
                ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Excepción);

                //Si la estación se encuentra en el modo de trabajo semiautomático, esta reenviará automáticamente una trama de rechazo de trama para restablecer de nuevo el enlace
                if (modo_trabajo == "Semiautomático")
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.No_numerada, TipoTrama.Rechazo_trama);
                }
            }
            else if (tr.NumeroTramaEsperada != estacion.NumeroSecuencia && EsTramaAnteriorSinConfirmar(tr)) //Si el número de trama esperada (NR) de la trama no coincide con el número de secuencia (NS) de la estación receptora y se trata de una confirmación de una trama anteriormente enviada, entonces la recepción es correcta se aumentará el tamaño de la ventana en función del número de tramas confirmadas
            {
                //Se actualiza el tamaño de la ventana en función del valor actual y el número de tramas confirmadas a través de la recepción de la trama de receptor no preparado
                tamaño_ventana_actual = tamaño_ventana_actual - ConfirmarTramasInformaciónEnviadas(tr); //Se confirman las tramas pendientes de acuerdo con el número de trama esperada recibido en la trama de receptor no preparado
                
                //Si la estación se encuentra en el modo de trabajo semiautomático, el bit de sondeo de la trama RNR esta activado y se trata de un comando, esta reenviará automáticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1 && tr.DireccionEstacion == estacion.NombreEstacion.LastOrDefault().ToString())
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }
            }
            else if (tr.NumeroTramaEsperada == estacion.NumeroSecuencia) //Si el número de trama esperada (NR) coincide con el número de secuencia de la estación, entonces la recepción es correcta y se reseteará el tamaño de la ventana y confirmaran las tramas correspodientes
            {
                ConfirmarTramasInformaciónEnviadas(tr); //Se confirman todas las tramas pendientes por confirmar (ya que el número de trama esperada de la trama coincide con el número de secuencia de la estación)
                tamaño_ventana_actual = 0; //Se resetea el tamaño actual de la ventana, ya que al recibir una trama de receptor no preparado donde el número de trama esperada de la trama coincide con el número de secuencia de la estación se confirman todas las tramas anteriores

                //Si la estación se encuentra en el modo de trabajo semiautomático, el bit de sondeo de la trama RNR esta activado y se trata de un comando, esta reenviará automáticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1 && tr.DireccionEstacion == estacion.NombreEstacion.LastOrDefault().ToString())
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }
            }

            //Si la trama recibida tiene el bit de sondeo activado y se trata de una respuesta, entonces se cancelará el timeout ante command pues se ha recibido una respuesta a un comando previo con el bit de poll activado
            if (tr.InfoBitSondeo == 1 && tr.InfoBitCommandRequest == "R" && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("Command");
            }
            //Si el número de trama esperada de la trama recibida coincide con el número de secuencia de la estación, entonces se cancelará el timeout ante trama I pues se han confirmado las tramas de información pendientes por confirmar
            if (tr.NumeroTramaEsperada == estacion.NumeroSecuencia && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("TramaI");
            }
        }

        private void RecibirTramaRechazo(Trama tr)
        {
            //Si el número de trama esperada de la trama coincide con el número de secuencia de la estación, entonces significa que las tramas enviadas por la estación pendientes de confirmar, quedan confirmadas 
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (tr.NumeroTramaEsperada == estacion.NumeroSecuencia)
            {
                ConfirmarTramasInformaciónEnviadas(tr); //Se confirman todas las tramas pendientes por confirmar (ya que el número de trama esperada de la trama coincide con el número de secuencia de la estación)
                tamaño_ventana_actual = 0; //Se resetea el tamaño actual de la ventana, ya que al recibir una trama de rechazo donde el número de trama esperada de la trama coincide con el número de secuencia de la estación se confirman las tramas anteriores

                //Si la estación se encuentra en el modo de trabajo semiautomático, el bit de sondeo de la trama REJ esta activado y se trata de un comando, esta reenviará automaticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1 && tr.DireccionEstacion == estacion.NombreEstacion.LastOrDefault().ToString())
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }
            }
            else if (modo_trabajo == "Semiautomático" && EsTramaAnterior(tr.NumeroTramaEsperada)) //Si la estación se encuentra en el modo de trabajo semiautomático y el número de trama esperada de la trama REJ es una trama enviada anteriormente por la estación la cual esta pendiente de confirmar, entonces la estación reenviará las tramas pendientes de confirmación a partir de la trama indicada en el REJ
            {
                //Se actualiza el tamaño de la ventana en función del valor actual y el número de tramas confirmadas a través de la recepción de la trama de rechazo
                tamaño_ventana_actual = tamaño_ventana_actual - ConfirmarTramasInformaciónEnviadas(tr); //Se confirman las tramas pendientes de acuerdo con el número de trama esperada recibido en la trama de rechazo

                //Si la estación se encuentra en el modo de trabajo semiautomático, el bit de sondeo de la trama REJ esta activado y se trata de un comando, esta reenviará automaticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1 && tr.DireccionEstacion == estacion.NombreEstacion.LastOrDefault().ToString())
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }

                ReenviarTramasInformacionNoConfirmadas(tr.NumeroTramaEsperada); //Se reenvian las tramas de información pendientes de confirmación a partir del número de trama indicado como parámetro
            }
            else if (EsTramaAnterior(tr.NumeroTramaEsperada)) //Si el número de trama esperada de la trama REJ es una trama enviada anteriormente por la estación la cual esta pendiente de confirmar, entonces la estación recopilará las tramas pendientes de confirmación a partir de la trama indicada en el REJ
            {
                no_corregir = true; //Activamos la variable no corregir ya que las tramas que deben ser reenviadas no necesitan ningún tipo de corrección

                //Se actualiza el tamaño de la ventana en función del valor actual y el número de tramas confirmadas a través de la recepción de la trama de rechazo
                tamaño_ventana_actual = tamaño_ventana_actual - ConfirmarTramasInformaciónEnviadas(tr); //Se confirman las tramas pendientes de acuerdo con el número de trama esperada recibido en la trama de rechazo

                //Recopilamos las tramas de información que quedan pendientes de confirmar a partir de la trama indicada en el REJ
                RecopilarTramasPendientesConfirmacion(tr);             
            }
            else if (!EsTramaAnterior(tr.NumeroTramaEsperada)) //Si el número de trama esperada de la trama REJ no es una trama enviada anteriormente por la estación la cual este pendiente de confirmar, entonces se producirá un estado de excepción ya que se ha producido una perdida irrecuperable de tramas
            {
                //Se actualiza la situación de la estacion receptora a Excepción
                ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Excepción);

                //Si la estación se encuentra en el modo de trabajo semiautomático, esta reenviará automáticamente una trama de rechazo de trama para restablecer de nuevo el enlace
                modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
                if (modo_trabajo == "Semiautomático")
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.No_numerada, TipoTrama.Rechazo_trama);
                }
            }

            //Si la trama recibida tiene el bit de sondeo activado y se trata de una respuesta, entonces se cancelará el timeout ante command pues se ha recibido una respuesta a un comando previo con el bit de poll activado
            if (tr.InfoBitSondeo == 1 && tr.InfoBitCommandRequest == "R" && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("Command");
            }
            //Si el número de trama esperada de la trama recibida coincide con el número de secuencia de la estación, entonces se cancelará el timeout ante trama I pues se han confirmado las tramas de información pendientes por confirmar
            if (tr.NumeroTramaEsperada == estacion.NumeroSecuencia && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("TramaI");
            }
        }

        private void RecopilarTramasPendientesConfirmacion(Trama tr)
        {
            int contador_FRMR = 0;
            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Se recorre la lista de tramas enviadas desde el final hasta encontrar las tramas rechazadas
            {
                //Si en el recorrido encontramos una trama de información que no haya sido reenviada y este pendiente de confirmar, incluimos dicha trama a la lista de tramas pendientes de retransmisión
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].TramaReenviada == false && listaTramasEnviadas[i].TramaConfirmada == false)
                {
                    listaTramasPendientesRetransmision.Add(listaTramasEnviadas[i]);
                }
                //Si la trama recorrida es de información y el número de secuencia de la trama coincide con el número de la trama rechazada, finaliza el recorrido ya que no quedan tramas rechazadas que deben ser reenviadas
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].NumeroSecuencia == tr.NumeroTramaEsperada) 
                {
                    break;
                }
                //Si en el recorrido nos encontramos una trama de rechazo de trama por segunda vez (la primera FRMR es la que se acaba de producir) (ya bien sea enviada o recibida por la propia estación), finalizamos la busqueda ya que no se tienen en cuenta las tramas de información previas a un segundo restablecimiento del enlace anterior
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Rechazo_trama || listaTramasRecibidas[i].TipoDeTrama == TipoTrama.Rechazo_trama)
                {
                    contador_FRMR = contador_FRMR + 1;
                    if (contador_FRMR >= 2)
                    {
                        return;
                    }
                }
            }
        }

        private void RecibirTramaRechazoSelectivo(Trama tr)
        {
            //Si el número de trama esperada de la trama coincide con el número de secuencia de la estación, entonces significa que las tramas enviadas por la estación pendientes de confirmar, quedan confirmadas 
            String modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
            if (tr.NumeroTramaEsperada == estacion.NumeroSecuencia)
            {
                ConfirmarTramasInformaciónEnviadas(tr); //Se confirman todas las tramas pendientes por confirmar (ya que el número de trama esperada de la trama coincide con el número de secuencia de la estación)
                tamaño_ventana_actual = 0; //Se resetea el tamaño actual de la ventana, ya que al recibir una trama de rechazo selectivo donde el número de trama esperada de la trama coincide con el número de secuencia de la estación se confirman las tramas anteriores

                //Si la estación se encuentra en el modo de trabajo semiautomático, el bit de sondeo de la trama SREJ esta activado y se trata de un comando, esta reenviará automaticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1 && tr.DireccionEstacion == estacion.NombreEstacion.LastOrDefault().ToString())
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }
            }
            else if (modo_trabajo == "Semiautomático" && EsTramaAnterior(tr.NumeroTramaEsperada)) //Si la estación se encuentra en el modo de trabajo semiautomático y el número de trama esperada de la trama SREJ es una trama enviada anteriormente por la estación la cual esta pendiente de confirmar, entonces la estación reenviará dicha trama pendiente de confirmación
            {
                //Se actualiza el tamaño de la ventana en función del valor actual y el número de tramas confirmadas a través de la recepción de la trama de rechazo selectivo
                tamaño_ventana_actual = tamaño_ventana_actual - ConfirmarTramasInformaciónEnviadas(tr); //Se confirman las tramas pendientes de acuerdo con el número de trama esperada recibido en la trama de rechazo selectivo

                //Si la estación se encuentra en el modo de trabajo semiautomático, el bit de sondeo de la trama SREJ esta activado y se trata de un comando, esta reenviará automaticamente una trama de receptor preparado
                if (modo_trabajo == "Semiautomático" && tr.InfoBitSondeo == 1 && tr.DireccionEstacion == estacion.NombreEstacion.LastOrDefault().ToString())
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.Supervision, TipoTrama.Receptor_preparado);
                }

                ReenviarTramaInformacionRechazada(tr.NumeroTramaEsperada); //Se reenvia la trama de información pendiente de confirmación a partir del número de trama indicado como parámetro
            }
            else if (EsTramaAnterior(tr.NumeroTramaEsperada)) //Si el número de trama esperada de la trama SREJ es una trama enviada anteriormente por la estación la cual esta pendiente de confirmar, entonces la estación recopilará la trama pendiente de confirmación a partir de la trama indicada en el SREJ
            {
                no_corregir = true; //Activamos la variable no corregir ya que la trama que debe ser reenviada no necesita ningún tipo de corrección

                //Se actualiza el tamaño de la ventana en función del valor actual y el número de tramas confirmadas a través de la recepción de la trama de rechazo selectivo
                tamaño_ventana_actual = tamaño_ventana_actual - ConfirmarTramasInformaciónEnviadas(tr); //Se confirman las tramas pendientes de acuerdo con el número de trama esperada recibido en la trama de rechazo selectivo

                //Se recupera la trama de infromación pendiente de confirmación y se añade a la lista de tramas pendientes de retransmisión
                for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Se recorre la lista de tramas enviadas desde el final hasta encontrar la trama rechazada
                {
                    if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].NumeroSecuencia == tr.NumeroTramaEsperada) //Si la trama recorrida es de información y el número de secuencia de la trama coincide con el número de la trama rechazada, se añade dicha trama a la lista de tramas pendientes de retransmisión
                    {
                        listaTramasPendientesRetransmision.Add(listaTramasEnviadas[i]);
                        break; 
                    }
                }
            }
            else if (!EsTramaAnterior(tr.NumeroTramaEsperada)) //Si el número de trama esperada de la trama SREJ no es una trama enviada anteriormente por la estación la cual este pendiente de confirmar, entonces se producirá un estado de excepción ya que se ha producido una perdida irrecuperable de tramas
            {
                //Se actualiza la situación de la estacion receptora a Excepción
                ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Excepción);

                //Si la estación se encuentra en el modo de trabajo semiautomático, esta reenviará automáticamente una trama de rechazo de trama para restablecer de nuevo el enlace
                modo_trabajo = modoTrabajo.TipoModoDeTrabajo;
                if (modo_trabajo == "Semiautomático")
                {
                    generarRespuestaAutomaticaTrama(TipoTramaControl.No_numerada, TipoTrama.Rechazo_trama);
                }
            }

            //Si la trama recibida tiene el bit de sondeo activado y se trata de una respuesta, entonces se cancelará el timeout ante command pues se ha recibido una respuesta a un comando previo con el bit de poll activado
            if (tr.InfoBitSondeo == 1 && tr.InfoBitCommandRequest == "R" && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("Command");
            }
            //Si el número de trama esperada de la trama recibida coincide con el número de secuencia de la estación, entonces se cancelará el timeout ante trama I pues se han confirmado las tramas de información pendientes por confirmar
            if (tr.NumeroTramaEsperada == estacion.NumeroSecuencia && modo_trabajo == "Semiautomático") //Para cancelar el timeout también revisar que el modo de trabajo de la estación es semiautomático, ya que en el caso de encontrarse en el modo manual no tendría sentido cancelar el timeout
            {
                CancelarTimeout("TramaI");
            }
        }

        private bool EsTramaAnterior(int numeroTrama)
        {
            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Recorremos la lista de tramas enviadas desde el final
            {
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].TramaConfirmada == false) //Si en el recorrido encontramos una trama de información no confirmada, comprobamos si el número de secuencia de dicha trama coincide con el número de trama que se esta buscando
                {
                    if (listaTramasEnviadas[i].NumeroSecuencia == numeroTrama) //Si el número de secuencia de dicha trama coincide con el número de trama que se esta buscando, se devuelve true ya que la trama que esta buscando existe
                    {
                        return true;
                    }
                }
            }
            return false; //Si no se ha encontrado una trama pendiente de confirmación con el número de secuencia pasado como parámetro, se devuelve false
        }

        private async void ReenviarTramasInformacionPendientes()
        {
            List<String> cola_tramas_reenviadas = new List<String>(); //Se crea una cola de tramas, con el objetivo de almacenar temporalmente todas las tramas a reenviar y aplicar el retardo del canal de manera conjunta y luego enviar las tramas de una en una

            //Se obtiene el valor del retardo del canal y la tasa de error
            int retardoCanal = canalTransmision.RetardoCanal;
            float tasaError = canalTransmision.TasaError;

            for (int i = listaTramasPendientesRetransmision.Count - 1; i >= 0; i--) //Recorremos la lista de tramas pendientes desde el final (para asi poder reenviar las tramas en orden)
            {
                //Bloquear y encolar la recepción de tramas hasta que se termine de generar la trama (generación atómica de tramas)
                encolarRecepcionTrama = true;

                //Generamos una trama vacia que se represente en la tabla de tramas recibidas
                Trama tr = new Trama(true);
                listaTramasRecibidas.Add(tr);

                //Generamos una nueva trama de información a la cual se le asignan los valores del número de secuencia y número de trama esperarada de la trama a reenviar
                tr = generarTramaAutomatica(TipoTramaControl.Informacion, TipoTrama.Informacion);
                tr.NumeroSecuencia = listaTramasPendientesRetransmision[i].NumeroSecuencia;
                tr.NumeroTramaEsperada = listaTramasPendientesRetransmision[i].NumeroTramaEsperada;
               
                //Se marca la trama como reenviada y se pone por defecto el bit de sondeo a 0 (por tratarse de un reenvio)
                tr.TramaReenviada = true;
                tr.InfoBitSondeo = 0;

                if (tr.NumeroSecuencia != estacion.NumeroSecuencia) //Si el número de secuencia de la trama a reenviar no coincide con el número de secuencia de la estación se corrige el número de secuencia de la trama reenviada
                {
                    tr.NumeroSecuencia = estacion.NumeroSecuencia;
                }
                if (tr.NumeroTramaEsperada != estacion.NumeroTramaEsperada) //Si el número de trama esperada de la trama a reenviar no coincide con el número de trama esperada de la estación se corrige el número de trama esperada de la trama reenviada
                {
                    tr.NumeroTramaEsperada = estacion.NumeroTramaEsperada;
                }

                tr.generarCampoControl(); //Se generá el campo de control de la trama a reenviar
                tr.CRC = generarCRC(tr); //Se generá el CRC de la trama a reenviar

                //Obtenemos la referencia de tiempo y se lo asignamos a la trama
                DateTime fechaTrama = DateTime.UtcNow;
                TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
                tr.InstanteTemporal = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

                tamaño_ventana_actual = tamaño_ventana_actual + 1; //Se incrementa el tamaño de la ventana ya que se esta reenviando una trama de información no confirmada
                incrementarNumeroSecuencia(); //Incrementamos el número de secuencia de la estación actual ya que se esta reenviando una trama de información no confirmada
                 
                tr = generarTramaErronea(tr, tasaError); //Aplicamos la tasa de error del canal para enviar las tramas con la probabilidad de error especificada

                //Si la trama generada no es incorrecta, aplicamos los timeouts correspodientes en el caso de que el envío de la trama actual desencadene algún timeout
                if (!tr.TramaIncorrecta)
                {
                    AplicarTimeouts(tr);
                }

                //Convertimos la trama a un objeto JSON de manera que pueda ser enviada de manera limpia y directa por la tubería a la estación situada en el otro extremo
                String json = JsonConvert.SerializeObject(tr); 

                //Añadimos la trama generada a la lista de tramas enviadas
                listaTramasEnviadas.Add(tr);

                //Permitir de nuevo la recepción de mensajes, pues la generación de la trama ya ha finalizado
                encolarRecepcionTrama = false;

                //Atender las recepciones de mensajes producidas (si se dieron) durante el bloqueo de recepción de mensajes
                AtenderRecepcionMensajePospuesta();

                //Añadimos la trama en formato json a la cola de tramas, poder aplicar el retardo a todas ellas de manera conjunta y luego enviarlas por separado
                cola_tramas_reenviadas.Add(json);
            }

            //Aplicamos el retardo especificado para el canal (se crea un hilo asíncrono específico para realizar este cometido para evitar bloquear el proceso principal)
            await Task.Delay(retardoCanal);

            for (int i = 0; i < cola_tramas_reenviadas.Count; i++) //Se recorre la cola de tramas para ir enviando las tramas de una en una
            {
                //Se envía la trama en formato JSON por la tubería correspodiente (en función de la estación que envia la trama se utilizará una tubería u otra)
                EnviarTrama(cola_tramas_reenviadas[i]);
            }

            listaTramasPendientesRetransmision.Clear(); //Al reenviar todas las tramas pendientes, se vacia la lista de tramas pendientes por retransmitir
        }

        private async void ReenviarTramaInformacionRechazada(int numeroTramaRechazada)
        {
            //Bloquear y encolar la recepción de tramas hasta que se termine de generar la trama (generación atómica de tramas)
            encolarRecepcionTrama = true;

            //Se obtiene el valor del retardo del canal y la tasa de error
            int retardoCanal = canalTransmision.RetardoCanal;
            float tasaError = canalTransmision.TasaError;

            //Generamos una trama vacia que se represente en la tabla de tramas recibidas
            Trama tr = new Trama(true);
            listaTramasRecibidas.Add(tr);

            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Recorremos la lista de tramas enviadas hasta encontrar la trama rechazada la cual se desea reenviar
            {
                if (listaTramasEnviadas[i].NumeroSecuencia == numeroTramaRechazada) //Si el número de secuencia de la trama coincide con el número de secuencia de la trama rechazada
                {
                    //Generamos una nueva trama de información a la cual se le asignan los valores del número de secuencia y número de trama esperarada de la trama rechazada a reenviar
                    tr = generarTramaAutomatica(TipoTramaControl.Informacion, TipoTrama.Informacion);
                    tr.NumeroSecuencia = listaTramasEnviadas[i].NumeroSecuencia;
                    tr.NumeroTramaEsperada = listaTramasEnviadas[i].NumeroTramaEsperada;
                    break;
                }
            }

            //Se marca la trama como reenviada y se pone por defecto el bit de sondeo a 0 (por tratarse de un reenvio)
            tr.TramaReenviada = true;
            tr.InfoBitSondeo = 0;

            tr.generarCampoControl(); //Se generá el campo de control de la trama a reenviar
            tr.CRC = generarCRC(tr); //Se generá el CRC de la trama a reenviar

            //Obtenemos la referencia de tiempo y se lo asignamos a la trama
            DateTime fechaTrama = DateTime.UtcNow;
            TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
            tr.InstanteTemporal = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

            tr = generarTramaErronea(tr, tasaError); //Aplicamos la tasa de error del canal para enviar las tramas con la probabilidad de error especificada

            //Convertimos la trama a un objeto JSON de manera que pueda ser enviada de manera limpia y directa por la tubería a la estación situada en el otro extremo
            String json = JsonConvert.SerializeObject(tr);

            //Añadimos la trama generada a la lista de tramas enviadas
            listaTramasEnviadas.Add(tr);

            //Permitir de nuevo la recepción de mensajes, pues la generación de la trama ya ha finalizado
            encolarRecepcionTrama = false;

            //Atender las recepciones de mensajes producidas (si se dieron) durante el bloqueo de recepción de mensajes
            AtenderRecepcionMensajePospuesta();

            //Aplicamos el retardo especificado para el canal (se crea un hilo asíncrono específico para realizar este cometido para evitar bloquear el proceso principal)
            await Task.Delay(retardoCanal);

            //Se envía la trama en formato JSON por la tubería correspodiente (en función de la estación que envia la trama se utilizará una tubería u otra)
            EnviarTrama(json);
        }

        private async void ReenviarTramasInformacionNoConfirmadas(int numeroTrama) //CORRECTA Y COMENTADA
        {
            List<String> cola_tramas_reenviadas = new List<String>(); //Se crea una cola de tramas, con el objetivo de almacenar temporalmente todas las tramas a reenviar y aplicar el retardo del canal de manera conjunta y luego enviar las tramas de una en una

            int numeroTramaReenviada = numeroTrama;

            //Bloquear y encolar la recepción de tramas hasta que se termine de generar la trama (generación atómica de tramas)
            encolarRecepcionTrama = true;

            //Se obtiene el valor del retardo del canal y la tasa de error
            int retardoCanal = canalTransmision.RetardoCanal;
            float tasaError = canalTransmision.TasaError;

            //Esta función recibe como parámetro el número de secuencia a partir del cual se deben reenviar las tramas
            //De esta manera, se va a reenviar las tramas cuyo número de secuencia se encuentre comprendido entre el número de trama recibido como parámetro y el número de secuencia de la estación
            while (numeroTramaReenviada != estacion.NumeroSecuencia) //Generamos y reenviamos las tramas necesarias hasta alcanzar una trama donde su número de secuencia coincida con el número de secuencia de la estación
            {
                //Generamos una trama vacia que se represente en la tabla de tramas recibidas
                Trama tr = new Trama(true);
                listaTramasRecibidas.Add(tr);

                //Generamos una nueva trama de información a la cual se le asigna el valor del número de secuencia de la trama no confirmada a reenviar
                tr = generarTramaAutomatica(TipoTramaControl.Informacion, TipoTrama.Informacion);
                tr.NumeroSecuencia = numeroTramaReenviada;

                //Se marca la trama como reenviada y se pone por defecto el bit de sondeo a 0 (por tratarse de un reenvio)
                tr.TramaReenviada = true;
                tr.InfoBitSondeo = 0;

                tr.generarCampoControl(); //Se generá el campo de control de la trama a reenviar
                tr.CRC = generarCRC(tr); //Se generá el CRC de la trama a reenviar

                //Obtenemos la referencia de tiempo y se lo asignamos a la trama
                DateTime fechaTrama = DateTime.UtcNow;
                TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
                tr.InstanteTemporal = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

                tr = generarTramaErronea(tr, tasaError); //Aplicamos la tasa de error del canal para enviar las tramas con la probabilidad de error especificada

                //Convertimos la trama a un objeto JSON de manera que pueda ser enviada de manera limpia y directa por la tubería a la estación situada en el otro extremo
                String json = JsonConvert.SerializeObject(tr);

                //Añadimos la trama generada a la lista de tramas enviadas
                listaTramasEnviadas.Add(tr);

                //Permitir de nuevo la recepción de mensajes, pues la generación de la trama ya ha finalizado
                encolarRecepcionTrama = false;

                //Atender las recepciones de mensajes producidas (si se dieron) durante el bloqueo de recepción de mensajes
                AtenderRecepcionMensajePospuesta();

                //Añadimos la trama en formato json a la cola de tramas, poder aplicar el retardo a todas ellas de manera conjunta y luego enviarlas por separado
                cola_tramas_reenviadas.Add(json);

                if (numeroTramaReenviada < 7) //Se incrementa el número de secuencia de la trama a reenviar. Si se desborda el tamaño máximo de la numeración, el número de secuencia de la trama a reenviar pasa a valer 0
                {
                    numeroTramaReenviada = numeroTramaReenviada + 1;
                }
                else
                {
                    numeroTramaReenviada = 0;
                }
            }

            //Aplicamos el retardo especificado para el canal (se crea un hilo asíncrono específico para realizar este cometido para evitar bloquear el proceso principal)
            await Task.Delay(retardoCanal);

            for (int i = 0; i < cola_tramas_reenviadas.Count; i++) //Se recorre la cola de tramas para ir enviando las tramas de una en una
            {
                //Se envía la trama en formato JSON por la tubería correspodiente (en función de la estación que envia la trama se utilizará una tubería u otra)
                EnviarTrama(cola_tramas_reenviadas[i]);
            }

            listaTramasPendientesRetransmision.Clear(); //Al reenviar todas las tramas pendientes, se vacia la lista de tramas pendientes por retransmitir
        }

        private async void generarRespuestaAutomaticaTrama(String tipoTramaControl, String tipoTrama)
        {         
            //Bloquear y encolar la recepción de tramas hasta que se termine de generar la trama (generación atómica de tramas)
            encolarRecepcionTrama = true;

            //Obtenemos el valor del retardo del canal y de la tasa de error
            int retardoCanal = canalTransmision.RetardoCanal;
            float tasaError = canalTransmision.TasaError;

            //Generamos la trama del tipo específicado
            Trama tr = generarTramaAutomatica(tipoTramaControl, tipoTrama);

            //Obtenemos la referencia de tiempo y se lo asignamos a la trama
            DateTime fechaTrama = DateTime.UtcNow;
            TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
            tr.InstanteTemporal = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

            tr = generarTramaErronea(tr, tasaError); //Aplicamos la tasa de error del canal para enviar las tramas con la probabilidad de error especificada

            //Si la trama generada no es incorrecta, aplicamos los timeouts correspodientes en el caso de que el envío de la trama actual desencadene algún timeout
            if (!tr.TramaIncorrecta)
            {
                AplicarTimeouts(tr);

                if (tipoTrama == TipoTrama.Rechazo_trama) //Si la trama a generar automáticamente es una trama de rechazo de trama (FRMR), se recuperan las tramas de información no confirmadas enviadas por la propia estación
                {
                    RecopilarTramasPendientesConfirmacionFRMR(1);
                }
                else if (tipoTrama == TipoTrama.Solicitud_conexion) //Si la trama a generar automáticamente es una trama de solicitud de conexión (SABM), se actualiza la situación de la estación a Inicio conexión y se pone el turno a falso
                {
                    ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Inicio_conexión); //Actualizamos la situación de la estación a Inicio Conexión
                    turno = false; //Establecemos el turno de la estación
                }
                else if (tipoTrama == TipoTrama.Asentimiento_no_numerado) //Si la trama a generar automáticamente es una trama de asentimiento no numerado (UA), se actualiza la situación de la estación a Conectado o Desconectado
                {
                    if (ObtenerInformacionUltimaTramaRecibida().TipoDeTrama == TipoTrama.Solicitud_conexion) //Si se responde automáticamente a una trama de solicitud de conexión (SABM), se actualiza la situación de la estación a Conectado
                    {
                        ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Conectado);
                    }
                    else //Si se responde automáticamente a una trama de solicitud de desconexión (DISC), se actualiza la situación de la estación a Desconectado
                    {
                        ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Desconectado);
                    }
                }
                else if (tipoTrama == TipoTrama.Modo_desconectado) //Si la trama a generar automáticamente es una trama de modo desconectado (DM), se actualiza la situación de la estación a Desconectado
                {
                    ActualizarSituaciónConexiónEstación(TipoSituaciónEstación.Desconectado);
                }
            }

            //Convertimos la trama a un objeto JSON de manera que pueda ser enviada de manera limpia y directa por la tubería a la estación situada en el otro extremo
            String json = JsonConvert.SerializeObject(tr);

            //Añadimos la trama generada a la lista de tramas enviadas
            listaTramasEnviadas.Add(tr);

            //Generamos una trama vacia que se represente en la tabla de tramas recibidas
            tr = new Trama(true);
            listaTramasRecibidas.Add(tr);

            //Permitir de nuevo la recepción de mensajes, pues la generación de la trama ya ha finalizado
            encolarRecepcionTrama = false;

            //Atender las recepciones de mensajes producidas (si se dieron) durante el bloqueo de recepción de mensajes
            AtenderRecepcionMensajePospuesta();

            //Aplicamos el retardo especificado para el canal (se crea un hilo asíncrono específico para realizar este cometido para evitar bloquear el proceso principal)
            await Task.Delay(retardoCanal);

            //Se envía la trama en formato JSON por la tubería correspodiente (en función de la estación que envia la trama se utilizará una tubería u otra)
            EnviarTrama(json);
        }

        private Trama generarTramaAutomatica(String tipoTramaControl, String tipoTrama)
        {
            Trama tr = new Trama(tipoTramaControl); //Se crea una nueva trama (de información, de supervisión o no numerada en función de lo que se pase como parámetro)
            if (tipoTramaControl == TipoTramaControl.No_numerada) //Si la trama a generar es no numerada, los valores del número de secuencia (NS) y número de trama esperada (NR) no seran utilizados (-1)
            {
                //Si la última trama recibida se trata de un comando con el bit de poll activado, se configura el bit C/R como respuesta (R) y como dirección de la estación, la dirección de la estación que responde a la trama anterior
                if (ObtenerInformacionUltimaTramaRecibida().InfoBitCommandRequest == "C" && ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo == 1)
                {
                    tr.InfoBitCommandRequest = "R";
                    tr.DireccionEstacion = CajaNombreEstacion.Text.LastOrDefault().ToString();
                }
                else //En caso contrario, se configura el bit C/R como comando (C) y como dirección de la estación, la dirección de la estación a la que va dirigida la trama
                {
                    tr.InfoBitCommandRequest = "C";
                    tr.DireccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString();
                }
                tr.NumeroSecuencia = -1; //Se asignará al número de secuencia (NS) de la trama, el valor -1 pues las tramas no numeradas no tienen número de secuencia (NS)
                if (tipoTrama == TipoTrama.Solicitud_conexion || tipoTrama == TipoTrama.Solicitud_desconexion) //Si la trama a generar es SABM o DISC, se asignará un 1 al bit P/F
                {
                    tr.InfoBitSondeo = 1;
                }
                else //En caso contrario, se asignará al bit P/F el valor del bit P/F de la ultima trama recibida por la estación
                {
                    tr.InfoBitSondeo = ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo;
                }
                tr.NumeroTramaEsperada = -1; //Se asignará al número de trama esperada (NR) de la trama, el valor -1 pues las tramas no numeradas no tienen número de trama esperada (NR)
                tr.TipoDeTrama = tipoTrama; //Se asignará al tipo de trama, el valor del tipo de trama recibido como parámetro 
                tr.generarCampoControl(); //Se generará el campo de control de la trama a partir de la información disponible sobre la trama
                tr.CRC = generarCRC(tr); //Se generará el CRC de la trama a partir de del campo de control, el campo de dirección y el campo de información de la trama
            }
            else if (tipoTramaControl == TipoTramaControl.Supervision) //Si la trama a generar es de supervision, el valor del número de secuencia (NS) no sera utilizado (-1)
            {
                //Si la última trama recibida se trata de un comando con el bit de poll activado, se configura el bit C/R como respuesta (R) y como dirección de la estación, la dirección de la estación que responde a la trama anterior
                if (ObtenerInformacionUltimaTramaRecibida().InfoBitCommandRequest == "C" && ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo == 1)
                {
                    tr.InfoBitCommandRequest = "R";
                    tr.DireccionEstacion = CajaNombreEstacion.Text.LastOrDefault().ToString();
                }
                else //En caso contrario, se configura el bit C/R como comando (C) y como dirección de la estación, la dirección de la estación a la que va dirigida la trama
                {
                    tr.InfoBitCommandRequest = "C";
                    tr.DireccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString();
                }
                tr.NumeroSecuencia = -1; //Se asignará al número de secuencia (NS) de la trama, el valor -1 pues las tramas de supervisión no tienen número de secuencia (NS)
                if (tipoTrama == TipoTrama.Receptor_preparado) //Si la trama a generar es RR, se asignará un 1 al bit P/F
                {
                    tr.InfoBitSondeo = 1;
                }
                else //En caso contrario, se asignará al bit P/F el valor del bit P/F de la ultima trama recibida por la estación
                {
                    tr.InfoBitSondeo = ObtenerInformacionUltimaTramaRecibida().InfoBitSondeo;
                }
                tr.NumeroTramaEsperada = estacion.NumeroTramaEsperada; //Se asignará al número de trama esperada (NR) de la trama, el valor del número de trama esperada de la estación
                tr.TipoDeTrama = tipoTrama; //Se asignará al tipo de trama, el valor del tipo de trama recibido como parámetro
                tr.generarCampoControl(); //Se generará el campo de control de la trama a partir de la información disponible sobre la trama
                tr.CRC = generarCRC(tr); //Se generará el CRC de la trama a partir de del campo de control, el campo de dirección y el campo de información de la trama
            }
            else if (tipoTramaControl == TipoTramaControl.Informacion) //Si la trama a generar es de información, se tendran en cuenta los valores del número de secuencia (NS) y número de trama esperada (NR)
            { 
                tr.InfoBitCommandRequest = "C"; //Se asignará al bit C/R de la trama, el valor C ya que las tramas de información siempre son comandos
                tr.DireccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Se asignará al campo de dirección de la estación de la trama, el valor de la dirección de la estación a la que va dirigida la trama (por tratarse de un comando)
                tr.NumeroSecuencia = estacion.NumeroSecuencia; //Se asignará al número de secuencia (NS) de la trama, el valor del número de secuencia de la estación
                tr.InfoBitSondeo = 0; //Se asignará al bit P/F de la trama, el valor 0
                tr.NumeroTramaEsperada = estacion.NumeroTramaEsperada; //Se asignará al número de trama esperada (NR) de la trama, el valor del número de trama esperada de la estación
                tr.TipoDeTrama = tipoTrama; //Se asignará al tipo de trama, el valor del tipo de trama recibido como parámetro
                tr.generarCampoControl(); //Se generará el campo de control de la trama a partir de la información disponible sobre la trama
                tr.CRC = generarCRC(tr); //Se generará el CRC de la trama a partir de del campo de control, el campo de dirección y el campo de información de la trama
            }
        
            return tr; //Se devuelve la trama generada
        }

        private Trama generarRespuestaFinTimeout(bool reenviarTrama)
        {
            Trama tr;
            if (reenviarTrama) //Si en el agotamiento del timeout se requiere reenviar la trama anterior, sencillamente se recupera la información de la última trama enviada para enviarla de nuevo
            {
                tr = ObtenerInformacionUltimaTramaEnviada();               
            }
            else //En caso contrario, se genera una trama de receptor preparado (RR) con el bit de poll activado para sondear a la otra estación en busca de una respuesta
            {
                tr = new Trama(TipoTramaControl.Supervision); //Se crea una nueva trama de supervisión (las tramas de receptor preparado (RR) son de supervisión)

                tr.InfoBitCommandRequest = "C"; //Se asignará al bit C/F el valor C ya que la trama será un comando
                tr.DireccionEstacion = estacion.NombreEstacionContraria.LastOrDefault().ToString(); //Se asignará al campo de dirección de la estación, la dirección de la estación a la que va dirigida la trama (por tratarse de un comando)
                tr.NumeroSecuencia = -1; //Se asignará al número de secuencia (NS) de la trama, el valor -1 pues las tramas de supervisión no tienen número de secuencia (NS)
                tr.InfoBitSondeo = 1; //Se asignará al bit P/F de la trama, el valor 1 con el objetivo de sondear de nuevo a la otra estación
                tr.NumeroTramaEsperada = estacion.NumeroTramaEsperada; //Se asignará al número de trama esperada (NR) de la trama, el valor del número de trama esperada de la estación
                tr.TipoDeTrama = TipoTrama.Receptor_preparado; //Se asignará al tipo de trama, el valor de receptor preparado (RR)
                tr.generarCampoControl(); //Se generará el campo de control de la trama a partir de la información disponible sobre la trama
                tr.CRC = generarCRC(tr); //Se generará el CRC de la trama a partir de del campo de control, el campo de dirección y el campo de información de la trama
            }

            return tr; //Se devuelve la trama generada
        }

        private async void ControlTimeoutCommand(bool timeoutExpirado, Trama t)
        {
            if (timeoutExpirado) //Si el timeout ante command ha expirado
            {
                //Obtenemos el valor del retardo del canal
                int retardoCanal = canalTransmision.RetardoCanal;

                //Generamos una trama vacia que se represente en la tabla de tramas recibidas
                Trama tr = new Trama(true);
                listaTramasRecibidas.Add(tr);

                if (t.TipoDeTrama == TipoTrama.Informacion) //Si la trama que desencadenó el timeout ante command es de información, se generará automaticamente una trama RR con el bit P/F activado
                {              
                    tr = generarRespuestaFinTimeout(false); //En este caso, pasamos el valor falso por lo que la trama generada de respuesta automática será una trama RR con el bit P/F activado
                }
                else //En caso contrario, se reenviará la trama que desencadenó el timeout ante command
                {
                    tr = t; //Obtenemos la trama que produjo el timeout ante command con el objetivo de reenviarla de nuevo a la otra estación

                    //Obtenemos la referencia de tiempo y se lo asignamos a la trama
                    DateTime fechaTrama = DateTime.UtcNow;
                    TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
                    tr.InstanteTemporal = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);
                }

                //Convertimos la trama a un objeto JSON de manera que pueda ser enviada de manera limpia y directa por la tubería a la estación situada en el otro extremo
                String json = JsonConvert.SerializeObject(tr);

                //Añadimos la trama generada a la lista de tramas enviadas
                listaTramasEnviadas.Add(tr);

                //Permitir de nuevo la recepción de mensajes
                encolarRecepcionTrama = false;

                //Atender las recepciones de mensajes producidas (si se dieron) durante el bloqueo de recepción de mensajes
                AtenderRecepcionMensajePospuesta();

                //Aplicamos el retardo especificado para el canal (se crea un hilo asíncrono específico para realizar este cometido para evitar bloquear el proceso principal)
                await Task.Delay(retardoCanal);

                //Se envía la trama en formato JSON por la tubería correspodiente (en función de la estación que envia la trama se utilizará una tubería u otra)
                EnviarTrama(json);

                //Se crea un hilo asíncrono que se encargará de implementar de nuevo el timeout ante Command de la trama correspondiente
                await Task.Run(() => ImplementarTimeoutCommand(protocolo.TimeoutCommand - retardoCanal, t)); //Observese cómo se resta el retardo del canal al tiempo del timeout ante command, ya que hasta que no se termine de aplicar el retardo no se producirá la llamada a esta función
            }
            else //Si el timeout ante command ha sido cancelado
            {
                SimboloTimeoutCommand.Visibility = Visibility.Hidden; //Se oculta el símbolo de timeout ante command, pues este tiemout ha sido cancelado
            }
        }

        private async void ImplementarTimeoutCommand(int timeoutCommand, Trama tr)
        {
            cancellationTokenSourceTimeoutCommand = new CancellationTokenSource(); //Se crea una nueva instancia del token de cancelación del timeout ante command
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeoutCommand / 1000), cancellationTokenSourceTimeoutCommand.Token); //Se ejecuta una tarea asíncrona en la que un hilo independiente duerme durante el tiempo especificado para el timeout ante command
                //Mientras el proceso principal se encarga de otras tareas

                //Si se agota el timeout ante command sin que haya ocurrido ningún evento que haya cancelado dicho timeout, se procederá a generar una respuesta automática

                //Se bloquea y encola la recepción de tramas hasta que se termine de enviar la trama
                encolarRecepcionTrama = true;

                //Se llama al método ControlTimeoutCommand() a través del dispatcher o proceso principal. Es necesario que el proceso principal ejecute esta función por que es propietario de algunos recursos a los que se acceden desde esta función
                Dispatcher.Invoke(() => ControlTimeoutCommand(true, tr)); //En este caso, se pasa como parámetro el valor true ya que el timeout ante command ha expirado
            }
            catch (TaskCanceledException) //Si ocurre algún evento que produce la cancelación del timeout ante command antes de que expire el tiempo especificado, se procederá a finalizar el timeout de manera ordenada
            {
                //Se llama al método ControlTimeoutCommand() a través del dispatcher o proceso principal. Es necesario que el proceso principal ejecute esta función por que es propietario de algunos recursos a los que se acceden desde esta función
                Dispatcher.Invoke(() => ControlTimeoutCommand(false, tr)); //En este caso, se pasa como parámetro el valor false ya que el timeout ante command ha sido cancelado
            }
        }

        private Trama ObtenerTramaInformaciónNoConfirmadaMasAntigua()
        {
            Trama tr = new Trama();
            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Se recorre la lista de tramas enviadas desde el final buscando la trama de información mas antigua sin confirmación
            {
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].TramaConfirmada == false) //Si en el recorrido, se encuentra una trama de información que no sea confirmada, se almacena temporalmente dicha trama
                {
                    tr = listaTramasEnviadas[i];
                }
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Rechazo_trama || listaTramasRecibidas[i].TipoDeTrama == TipoTrama.Rechazo_trama) //Si en el recorrido se encuentra una trama FRMR (tanto en el envio como en la recepción), finaliza la busqueda y se devuelve la trama almacenada temporalmente 
                { //No se tendran en cuenta tramas de información anteriores a un restablecimiento de la conexión
                    return tr;
                }
            }

            return tr; //Se devuelve la trama de información mas antigua sin confirmación
        }

        private async void ControlTimeoutTramaI(bool timeoutExpirado)
        {
            if (timeoutExpirado) //Si el timeout ante trama I ha expirado
            {
                //Se obtiene información sobre la trama de información mas antigua sin confirmación
                Trama tncma = ObtenerTramaInformaciónNoConfirmadaMasAntigua();

                //Se obtiene el instante de tiempo actual
                DateTime fechaTrama = DateTime.UtcNow;
                TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
                float instante_actual = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

                //Si la diferencia de tiempo entre el instante actual y el instante de la trama de información mas antigua sin confirmación es menor que el timeout ante trama I, se esperá el tiempo suficiente hasta que dicha diferencia supere el valor del timeout ante trama I 
                if ((instante_actual - tncma.InstanteTemporal) < (protocolo.TimeoutTramaI / 1000))
                {
                    try
                    {
                        //Permitir de nuevo la recepcion de mensajes
                        encolarRecepcionTrama = false;

                        //Atender las recepciones de mensajes producidas (si se dieron) durante el bloqueo de recepcion de mensajes
                        AtenderRecepcionMensajePospuesta();

                        //Se obtiene el tiempo adicional que se debe esperar hasta que la diferencia entre el isntante actual y el instante de la trama de información mas antigua sin confirmación sea superior al timeout ante trama I 
                        float tiempo_espera_adicional = (protocolo.TimeoutTramaI / 1000) - (instante_actual - tncma.InstanteTemporal);
                        await Task.Delay(TimeSpan.FromSeconds(tiempo_espera_adicional), cancellationTokenSourceTimeoutTramaI.Token); //Se ejecuta una tarea asíncrona en la que un hilo independiente duerme durante el tiempo de espera adicional especificado
                    }
                    catch (TaskCanceledException)
                    {
                        tareaTimeoutTramaIActivada = false; //Se marca como "no activo" el timeout ante trama I
                        SimboloTimeoutTramaI.Visibility = Visibility.Hidden; //Se oculta el símbolo de timeout ante trama I, pues este tiemout ha sido cancelado

                        return; //Finaliza la ejecución de la función, ya que se ha cancelado el timeout antes de que expirase
                    }
                }

                //Se bloquea y encola la recepción de tramas hasta que se termine de enviar la trama
                encolarRecepcionTrama = true;

                //Obtenemos el valor del retardo del canal
                int retardoCanal = canalTransmision.RetardoCanal;

                //Generamos una trama vacia que se represente en la tabla de tramas recibidas
                Trama tr = new Trama(true);
                listaTramasRecibidas.Add(tr);

                //Se genera una trama de respuesta debido a la expiración del timeout
                tr = generarRespuestaFinTimeout(false); //En este caso, pasamos el valor falso por lo que la trama generada de respuesta automática será una trama RR con el bit P/F activado

                //Obtenemos la referencia de tiempo y se lo asignamos a la trama
                fechaTrama = DateTime.UtcNow;
                tiempoTranscurrido = fechaTrama - fechaActual;
                tr.InstanteTemporal = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

                //Convertimos la trama a un objeto JSON de manera que pueda ser enviada de manera limpia y directa por la tubería a la estación situada en el otro extremo
                String json = JsonConvert.SerializeObject(tr);

                //Añadimos la trama generada a la lista de tramas enviadas
                listaTramasEnviadas.Add(tr);

                //Permitir de nuevo la recepcion de mensajes
                encolarRecepcionTrama = false;

                //Atender las recepciones de mensajes producidas (si se dieron) durante el bloqueo de recepcion de mensajes
                AtenderRecepcionMensajePospuesta();

                //Aplicamos el retardo especificado para el canal (se crea un hilo asíncrono específico para realizar este cometido para evitar bloquear el proceso principal)
                await Task.Delay(retardoCanal);

                //Se envía la trama en formato JSON por la tubería correspodiente (en función de la estación que envia la trama se utilizará una tubería u otra)
                EnviarTrama(json);

                //Se marca como "no activo" el timeout ante trama I
                tareaTimeoutTramaIActivada = false;

                //Se oculta el símbolo de timeout ante trama I y se muestra el símbolo de timeout ante command
                SimboloTimeoutTramaI.Visibility = Visibility.Hidden;
                SimboloTimeoutCommand.Visibility = Visibility.Visible;

                //Cuando se agota el timeout ante trama I, se envia automáticamente un comando RR con el bit de P/F activado lo que activa el timeout ante command. Para no mantener 2 timeouts abiertos, se cierra el timeout ante trama I

                //Se crea un hilo asíncrono que se encargará de implementar el timeout ante Command de la trama correspondiente
                await Task.Run(() => ImplementarTimeoutCommand(protocolo.TimeoutCommand - retardoCanal, tr)); //Observese cómo se resta el retardo del canal al tiempo del timeout ante command, ya que hasta que no se termine de aplicar el retardo no se producirá la llamada a esta función
            }
            else //Si el timeout ante trama I ha sido cancelado
            {
                tareaTimeoutTramaIActivada = false; //Se marca como "no activo" el timeout ante trama I
                SimboloTimeoutTramaI.Visibility = Visibility.Hidden; //Se oculta el símbolo de timeout ante trama I, pues este tiemout ha sido cancelado
            }
        }

        private async void ImplementarTimeoutTramaI(int timeoutTramaI)
        {
            cancellationTokenSourceTimeoutTramaI = new CancellationTokenSource(); //Se crea una nueva instancia del token de cancelación del timeout ante trama I
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeoutTramaI / 1000), cancellationTokenSourceTimeoutTramaI.Token); //Se ejecuta una tarea asíncrona en la que un hilo independiente duerme durante el tiempo especificado para el timeout ante trama I

                //Si se agota el timeout ante trama I sin que haya ocurrido ningún evento que haya cancelado dicho timeout, se procederá a generar una respuesta automática

                //Se bloquea y encola la recepción de tramas hasta que se termine de enviar la trama
                encolarRecepcionTrama = true;

                //Se llama al método ControlTimeoutTramaI() a través del dispatcher o proceso principal. Es necesario que el proceso principal ejecute esta función por que es propietario de algunos recursos a los que se acceden desde esta función
                Dispatcher.Invoke(() => ControlTimeoutTramaI(true)); //En este caso, se pasa como parámetro el valor true ya que el timeout ante trama I ha expirado
            }
            catch (TaskCanceledException) //Si ocurre algún evento que produce la cancelación del timeout ante trama I antes de que expire el tiempo especificado, se procederá a finalizar el timeout de manera ordenada
            {               
                //Se llama al método ControlTimeoutTramaI() a través del dispatcher o proceso principal. Es necesario que el proceso principal ejecute esta función por que es propietario de algunos recursos a los que se acceden desde esta función
                Dispatcher.Invoke(() => ControlTimeoutTramaI(false)); //En este caso, se pasa como parámetro el valor false ya que el timeout ante trama I ha sido cancelado
            }
        }

        private async void ControlTimeoutRequest(bool timeoutExpirado)
        {
            cancellationTokenSourceTimeoutRequest = new CancellationTokenSource(); //Se crea una nueva instancia del token de cancelación del timeout ante request
            if (timeoutExpirado) //Si el timeout ante request ha expirado
            {
                //Obtenemos el valor del retardo del canal
                int retardoCanal = canalTransmision.RetardoCanal;

                //Generamos una trama vacia que se represente en la tabla de tramas recibidas
                Trama tr = new Trama(true);
                listaTramasRecibidas.Add(tr);

                //Se genera una trama de respuesta debido a la expiración del timeout
                tr = generarRespuestaFinTimeout(true); //En este caso, pasamos el valor true por lo que la trama generada de respuesta automática será un reenvio de la trama anterior

                //Obtenemos la referencia de tiempo y se lo asignamos a la trama
                DateTime fechaTrama = DateTime.UtcNow;
                TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
                tr.InstanteTemporal = (float)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

                //Convertimos la trama a un objeto JSON de manera que pueda ser enviada de manera limpia y directa por la tubería a la estación situada en el otro extremo
                String json = JsonConvert.SerializeObject(tr);

                //Añadimos la trama generada a la lista de tramas enviadas
                listaTramasEnviadas.Add(tr);

                //Permitir de nuevo la recepcion de mensajes
                encolarRecepcionTrama = false;

                //Atender las recepciones de mensajes producidas (si se dieron) durante el bloqueo de recepcion de mensajes
                AtenderRecepcionMensajePospuesta();

                //Aplicamos el retardo especificado para el canal (se crea un hilo asíncrono específico para realizar este cometido para evitar bloquear el proceso principal)
                await Task.Delay(retardoCanal);

                //Se envía la trama en formato JSON por la tubería correspodiente (en función de la estación que envia la trama se utilizará una tubería u otra)
                EnviarTrama(json);

                //Se crea un hilo asíncrono que se encargará de implementar de nuevo el timeout ante Request de la trama correspondiente
                await Task.Run(() => ImplementarTimeoutRequest(protocolo.TimeoutRequest - retardoCanal)); //Observese cómo se resta el retardo del canal al tiempo del timeout ante request, ya que hasta que no se termine de aplicar el retardo no se producirá la llamada a esta función
            }
            else
            {
                SimboloTimeoutRequest.Visibility = Visibility.Hidden; //Se oculta el símbolo de timeout ante request, pues este tiemout ha sido cancelado
            }
        }

        private async void ImplementarTimeoutRequest(int timeoutRequest)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeoutRequest / 1000), cancellationTokenSourceTimeoutRequest.Token); //Se ejecuta una tarea asíncrona en la que un hilo independiente duerme durante el tiempo especificado para el timeout ante request

                //Si se agota el timeout ante request sin que haya ocurrido ningún evento que haya cancelado dicho timeout, se procederá a generar una respuesta automática

                //Se bloquea y encola la recepción de tramas hasta que se termine de enviar la trama
                encolarRecepcionTrama = true;

                //Se llama al método ControlTimeoutRequest() a través del dispatcher o proceso principal. Es necesario que el proceso principal ejecute esta función por que es propietario de algunos recursos a los que se acceden desde esta función
                Dispatcher.Invoke(() => ControlTimeoutRequest(true)); //En este caso, se pasa como parámetro el valor true ya que el timeout ante request ha expirado
            }
            catch (TaskCanceledException) //Si ocurre algún evento que produce la cancelación del timeout ante request antes de que expire el tiempo especificado, se procederá a finalizar el timeout de manera ordenada
            {               
                //Se llama al método ControlTimeoutRequest() a través del dispatcher o proceso principal. Es necesario que el proceso principal ejecute esta función por que es propietario de algunos recursos a los que se acceden desde esta función
                Dispatcher.Invoke(() => ControlTimeoutRequest(false)); //En este caso, se pasa como parámetro el valor false ya que el timeout ante request ha sido cancelado
            }
        }

        private void BotonGuardarCapturaTrafico_Click(object sender, RoutedEventArgs e)
        {         
            if (!sender.ToString().StartsWith("GUARDAR_CAPTURA_TRAFICO")) //Si el contenido del sender no empieza por GUARDAR_CAPTURA_TRAFICO, significa que el guardado no ha sido solicitado primero en el otro extremo de la conexión
            { 
                String nombre_fichero = System.String.Empty;
                SaveFileDialog saveFileDialog = new SaveFileDialog(); //Se crea una instancia de la ventana de exploración de Windows
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"; //Se ajustan los filtros de la ventana de exploración de Windows, con el objetivo de poder acceder a cualquier tipo de archivo
                if (saveFileDialog.ShowDialog() == true) //Si el usuario confirma su deseo de guardar la captura de tráfico en un fichero con un determinado nombre y una determinada ubicación
                {
                    nombre_fichero = saveFileDialog.FileName; //Se obtiene la ruta del fichero en el cual el usuario desea guardar la captura de tráfico

                    try
                    {
                        AlmacenarTramasFichero(nombre_fichero); //Se almacena la información de la captura de tráfico en la ruta del fichero indicado como párametro

                        //Enviamos una mensaje a la estación situada en el otro extremo para que esta tambien guarde la captura de tráfico
                        NotificarEstacionExtremoContrario("GUARDAR_CAPTURA_TRAFICO;" + nombre_fichero + " (2)"); //En el mensaje se incluye el nombre de la ruta del fichero donde la estación debe guardar dicha captura de tráfico
                    }
                    catch (Exception) //Si en el guardado de la captura de tráfico se produce algún tipo de error, se mostrará un mensaje al usuario alertando de tal hecho
                    {
                        mostrarVentanaAdvertencia("Error en el guardado de los datos de la captura de tráfico", true, "Advertencia");
                    }
                    finally //Si el guardado de la captura de tráfico se ha realizado de manera satisfactoria, se mostrará un mensaje al usuario alertando de tal hecho
                    {
                        mostrarVentanaAdvertencia("Datos de la captura de tráfico guardados con éxito", false, "Información");
                    }
                }
            }
            else //Si el contenido del sender empieza por GUARDAR_CAPTURA_TRAFICO, significa que el guardado ha sido solicitado primero en el otro extremo de la conexión
            {
                //En este caso, el sender contiene la ruta del fichero donde se debe guardar la captura de tráfico

                String[] temp = sender.ToString().Split(';'); //Se separa la cabecera del mensaje de la ruta del fichero
                String nombre_fichero = temp[1]; //Se obtiene la ruta del fichero

                AlmacenarTramasFichero(nombre_fichero); //Se almacena la información de la captura de tráfico en la ruta del fichero indicado como párametro
            }
        }

        private void NotificarEstacionExtremoContrario(String mensaje)
        {
            try 
            {
                if (!segundaEstacion) //Si la estación que envía la trama, es la estación que solicitó en primer lugar la conexión física, se enviará el mensaje a través de la tubería cliente creada por la propia estación (pipeConexion2)
                {
                    StreamWriter sw2 = new StreamWriter(pipeConexion2.Tuberia);
                    sw2.AutoFlush = true;
                    sw2.WriteLine(mensaje);
                }
                else //Si la estación que envía la trama, es la estación que solicitó en segundo lugar la conexión física, se enviará el mensaje a través de la tubería cliente creada por la propia estación (pipeConexion)
                {
                    StreamWriter sw2 = new StreamWriter(pipeConexion.Tuberia);
                    sw2.AutoFlush = true;
                    sw2.WriteLine(mensaje);
                }
            }
            catch (Exception) //Si en el envío del mensaje a través de la tubería correspondiente se produce algún error, se imprimirá un mensaje alertando de tal hecho
            {
                Console.WriteLine("ERROR -> Canalizacion cerrada");
            }
        }

        private void AlmacenarTramasFichero(String ruta_fichero)
        {
            StreamWriter sw = new StreamWriter(ruta_fichero); //Se crea un flujo de datos de escritura con destino la ruta del fichero en el cual se desea guardar la captura de tráfico

            //Se almacena información relevante sobre el estado actual de la estación (separada por ;)
            sw.WriteLine(estacion.NombreEstacion + ";" + estacion.SituacionEstacion + ";" + estacion.NumeroSecuencia + ";" + estacion.NumeroTramaEsperada + ";" + tamaño_ventana_actual + ";" + numero_tramas_erroneas_consecutivas_recibidas + ";" + turno);

            for (int i = 0; i < listaTramasEnviadas.Count; i++) //Se recorre la lista de tramas enviadas y recibidas desde el principio hasta el final
            {
                //Para cada trama enviada por la estación, se convierte dicha trama a un objeto JSON y posteriermente se almacena dicho objeto JSON en el fichero correspondiente
                Trama tr = listaTramasEnviadas[i];
                String json = JsonConvert.SerializeObject(tr);
                sw.WriteLine(json);

                //Para cada trama recibida por la estación, se convierte dicha trama a un objeto JSON y posteriermente se almacena dicho objeto JSON en el fichero correspondiente
                tr = listaTramasRecibidas[i];
                json = JsonConvert.SerializeObject(tr);
                sw.WriteLine(json);
            } //Observerse como se almacenan las tramas enviadas y recibidas por la estación de manera intercalada. Esto hay que tenerlo en cuenta a la hora de cargar una captura de tráfico, ya que hay que saber interpretar los datos almacenados

            sw.Close(); //Una vez se ha terminado de recorrer la lista de tramas enviadas y recibidas, se cierra el fichero donde se almacena la captura de tráfico
        }

        private void AtenderRecepcionMensajePospuesta()
        {
            if (listaMensajes.Count > 0) //Si la lista de mensajes pendientes por recibir, no esta vacia
            {
                for (int i = 0; i < listaMensajes.Count; i++) //Se recorre la lista de mensajes pendientes por recibir desde el principio
                {
                    //Para cada mensaje pendiente por recibir, se invoca la función de recepción Server_MessageReceived() pasandole como parámetros la referencia al objeto actual (sender) y el cuerpo del mensaje pendiente en un MessageReceivedEventArg 
                    MessageReceivedEventArgs mrea = new MessageReceivedEventArgs(listaMensajes[i]); 
                    Server_MessageReceived(this, mrea);
                }
                listaMensajes.Clear(); //Tras recorrer la lista de mensajes pendientes por recibir y realizar las recepciones de manera retardada, se limpia la lista de mensajes pendientes por recibir
            }
        }

        private void BotonCargarCapturaTrafico_Click(object sender, RoutedEventArgs e)
        {
            if (CajaSituacionEstacion.Text != "") //Si la estación se encuentra físcamente conectada con otra estación
            {
                //Se cancelan o finalizan los timeouts de manera que si alguno de ellos estaba activo, sea desactivado en el momento en el que se realiza el cargado de la captura de tráfico
                cancellationTokenSourceTimeoutRequest.Cancel();
                cancellationTokenSourceTimeoutRequest = new CancellationTokenSource();

                cancellationTokenSourceTimeoutTramaI.Cancel();
                cancellationTokenSourceTimeoutTramaI = new CancellationTokenSource();

                cancellationTokenSourceTimeoutCommand.Cancel();
                cancellationTokenSourceTimeoutCommand = new CancellationTokenSource();

                listaTramasPendientesRetransmision.Clear(); //Se limpia la lista de tramas pendientes de retransmisión

                //Se resetea el valor de las variables relacionadas con la gestión de los timeouts
                tareaTimeoutTramaIActivada = false;
                encolarRecepcionTrama = false;

                if (!sender.ToString().StartsWith("CARGAR_CAPTURA_TRAFICO")) //Si el contenido del sender no empieza por CARGAR_CAPTURA_TRAFICO, significa que el cargado no ha sido solicitado primero en el otro extremo de la conexión
                {
                    String nombre_fichero = System.String.Empty;
                    OpenFileDialog openFileDialog = new OpenFileDialog(); //Se crea una instancia de la ventana de exploración de Windows
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"; //Se ajustan los filtros de la ventana de exploración de Windows, con el objetivo de poder acceder a cualquier tipo de archivo
                    if (openFileDialog.ShowDialog() == true) //Si el usuario confirma su deseo de cargar la captura de tráfico almacenada en un fichero con un determinado nombre y una determinada ubicación
                    {
                        nombre_fichero = openFileDialog.FileName; //Se obtiene la ruta del fichero en el cual se encuentra almacenada la captura de tráfico que el usuario desea cargar

                        try
                        {
                            CargarTramasFichero(nombre_fichero); //Se carga la información de la captura de tráfico almacenada en la ruta del fichero indicado como párametro

                            try //Se envia la trama convertida en un objeto JSON por la tuberia correspondiente
                            {
                                //Se actualiza el valor de la referencia de tiempo de la estación en función del instante en el que se produjo la última trama de la captura de tráfico cargada
                                DateTime fechaProvisional = DateTime.UtcNow; //Se obtiene el instante de tiempo actual
 
                                double desplazamiento_ms = ObtenerInformacionUltimaTrama().InstanteTemporal * 1000; //Se calcula el desplazamiento a aplicar sobre el instante actual en función del instante en el que se produjo la última trama de la captura de tráfico cargada
                                fechaActual = fechaProvisional.AddMilliseconds(-desplazamiento_ms); //Se aplica dicho desplazamiento al instante de tiempo actual

                                segundos = Convert.ToInt32(desplazamiento_ms / 1000); //Se modifica el valor de los segundos de la referencia temporal al valor del desplazmiento

                                DetectarTimeoutsCargadoCapturaTrafico(); //Se recorre las tramas cargadas, en busca de timeouts que deban ser activados debido a la situación actual

                                //Enviamos una mensaje a la estación situada en el otro extremo para que esta tambien cargue la captura de tráfico
                                NotificarEstacionExtremoContrario("CARGAR_CAPTURA_TRAFICO;" + nombre_fichero + " (2)" + ";" + fechaActual.ToString("yyyy-MM-dd HH:mm:ss.fff")); //En el mensaje se incluye el nombre de la ruta del fichero donde la estación debe guardar dicha captura de tráfico y la nueva referencia de tiempo
                            }
                            catch (Exception) //Si en el envío de la trama a través de la tubería correspondiente se produce algún error, se imprimirá un mensaje alertando de tal hecho
                            {
                                Console.WriteLine("ERROR -> Canalizacion cerrada");
                            }
                        }
                        catch (Exception ex) //Si en el cargado de la captura de tráfico se produce algún tipo de error, se mostrará un mensaje al usuario alertando de tal hecho
                        {
                            if (ex.Message.StartsWith("Error en el cargado"))
                            {
                                mostrarVentanaAdvertencia(ex.Message, true, "Advertencia");
                            }
                            else
                            {
                                mostrarVentanaAdvertencia("Error en el cargado de los datos de la captura de tráfico.\nEl fichero no tiene el formato adecuado", true, "Advertencia");
                            }
                        }
                    }
                }
                else //Si el contenido del sender empieza por CARGAR_CAPTURA_TRAFICO, significa que el cargado ha sido solicitado primero en el otro extremo de la conexión
                {
                    try
                    {
                        //En este caso, el sender contiene la ruta del fichero donde se debe guardar la captura de tráfico
                        String[] temp = sender.ToString().Split(';'); //Se separa la cabecera del mensaje de la ruta del fichero
                        String nombre_fichero = temp[1]; //Se obtiene la ruta del fichero

                        CargarTramasFichero(nombre_fichero); //Se carga la información de la captura de tráfico almacenada en la ruta del fichero indicado como párametro

                        String nuevo_tiempo = temp[2]; //Se obtiene la nueva referencia de tiempo, calculada y enviada por la estación situada en el otro extremo. Seguimos mantiendo una referencia de tiempo común
                        fechaActual = DateTime.ParseExact(nuevo_tiempo, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture); //Actualizamos la referencia de tiempo
                        double desplazamiento_ms = ObtenerInformacionUltimaTrama().InstanteTemporal * 1000; //Se calcula el desplazamiento a aplicar sobre el instante actual en función del instante en el que se produjo la última trama de la captura de tráfico cargada
                        segundos = Convert.ToInt32(desplazamiento_ms / 1000); //Se modifica el valor de los segundos de la referencia temporal al valor del desplazmiento

                        DetectarTimeoutsCargadoCapturaTrafico(); //Se recorre las tramas cargadas, en busca de timeouts que deban ser activados debido a la situación actual
                    }
                    catch (Exception) //Si en el cargado de la captura de tráfico se produce algún tipo de error, se mostrará un mensaje al usuario alertando de tal hecho
                    {
                        mostrarVentanaAdvertencia("Error en el cargado de los datos de la captura de tráfico.\nEl fichero no tiene el formato adecuado", true, "Advertencia");
                    }
                }
            }
            else //Si la estación no se encuentra físcamente conectada con otra estación, no se podrá realizar el cargado de la captura de tráfico y se mostrará un mensaje de advertencia al usario alertando de tal hecho
            {
                mostrarVentanaAdvertencia("Error en el cargado de los datos de la captura de tráfico.\nNo se puede realizar un cargado de una captura de tráfico si las estaciones no se encuentran físicamente conectadas", true, "Advertencia");
            }
        }

        private void CargarTramasFichero(string ruta_fichero)
        {
            StreamReader sr = new StreamReader(ruta_fichero); //Se crea un flujo de datos de lectura con destino la ruta del fichero en el cual se desea cargar la captura de tráfico

            LimpiarInformaciónTramasEstación(); //Se limpian las listas de tramas enviadas y recibidas así como la representación gráfica y animaciones asociadas

            String line = sr.ReadLine(); //Se extrae la primera línea del fichero 

            String[] vector_variables_cargadas = line.Split(';'); //Se obtiene información relevante sobre el estado de la estación en el instante en el que se guardo la captura de tráfico (esta separada por ;)

            if (estacion.NombreEstacion != vector_variables_cargadas[0]) //Si la estación que realiza el cargado de la captura de tráfico no es la misma estación que guardo anteriormente dicha captura de tráfico, se lanzará una excepción y se mostrará una ventana de advertencia indicando porque no se ha podido realizar el cargado de la captura de tráfico
            {          
                throw new Exception("Error en el cargado de los datos de la captura de trafico.\nLa estación actual no es la estación que guardó la captura de tráfico previamente");
            }

            //Se cargan los valores relevantes sobre el estado de la estación en el instante en el que se guardo la captura de tráfico 
            estacion.NombreEstacion = vector_variables_cargadas[0];
            estacion.SituacionEstacion = vector_variables_cargadas[1];
            estacion.NumeroSecuencia = Int32.Parse(vector_variables_cargadas[2]);
            estacion.NumeroTramaEsperada = Int32.Parse(vector_variables_cargadas[3]);
            tamaño_ventana_actual = Int32.Parse(vector_variables_cargadas[4]);
            numero_tramas_erroneas_consecutivas_recibidas = Int32.Parse(vector_variables_cargadas[5]);
            turno = bool.Parse(vector_variables_cargadas[6]);

            //Se actualizan los valores de las cajas de la estación
            CajaSituacionEstacion.Text = estacion.SituacionEstacion;
            CajaNumeroSecuencia.Text = estacion.NumeroSecuencia.ToString();
            CajaNumeroTramaEsperada.Text = estacion.NumeroTramaEsperada.ToString();

            line = sr.ReadLine(); //Se extrae la segunda línea del fichero 

            while (line != null && line != "") //Se recorre todo el fichero hasta leer un linea vacia o nula (lo que indica el final del contenido del fichero)
            {
                //En primer lugar, se lee la trama enviada por la estación, la cual se transforma de un objeto JSON a objeto de tipo Trama. Posteriormente, se almacena dicha trama en la lista de tramas enviadas y se lee la siguiente línea del fichero
                Trama tr = JsonConvert.DeserializeObject<Trama>(line);
                listaTramasEnviadas.Add(tr);
                line = sr.ReadLine();

                //En segundo lugar, se lee la trama recibida por la estación, la cual se transforma de un objeto JSON a objeto de tipo Trama. Posteriormente, se almacena dicha trama en la lista de tramas recibidas y se lee la siguiente línea del fichero
                tr = JsonConvert.DeserializeObject<Trama>(line);
                listaTramasRecibidas.Add(tr);
                line = sr.ReadLine();
            } //Como se almacenaron las tramas enviadas y recibidas de manera intercalada, en el cargado se realiza el mismo procedimiento. Si no se tiene en cuenta esto, pueden mezclarse tramas en el cargado

            sr.Close(); //Una vez se ha terminado de recorrer el fichero, se cierra el fichero donde se almacena la captura de tráfico
        }

        private async void DetectarTimeoutsCargadoCapturaTrafico()
        {
            bool timeout_command_activado = false;

            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Se recorre la lista de tramas enviadas desde el final
            {
                if (i == listaTramasEnviadas.Count - 1) //Si recorremos la última trama
                {
                    if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Rechazo_trama) //Si la última trama enviada es una trama de rechazo de trama (FRMR), se activa el timeout ante request por no se ha recibido respuesta a la trama FRMR
                    {
                        SimboloTimeoutRequest.Visibility = Visibility.Visible; //Se hace visible un símbolo que indica que el timeout ante Request de la estación se encuentra activo

                        await Task.Run(() => ImplementarTimeoutRequest(protocolo.TimeoutRequest)); //Se crea un hilo asíncrono que se encargará de implementar el timeout ante Request de la trama correspondiente
                    }
                }
                if (listaTramasEnviadas[i].TipoDeTrama == TipoTrama.Informacion && listaTramasEnviadas[i].TramaConfirmada == false) //Si la trama enviada es de información y no esta confirmada, se activa el timeout ante trama I por no se ha recibido confirmación para dicha trama de información
                {
                    //Se comprueba si ya existe un timeout ante Trama I activo en la estación, y en el caso de que así sea se impide lanzar otro timeout
                    if (!tareaTimeoutTramaIActivada)
                    {
                        tareaTimeoutTramaIActivada = true; //Se marca como "activo" el timeout ante trama I
                        SimboloTimeoutTramaI.Visibility = Visibility.Visible; //Se hace visible un símbolo que indica que el timeout ante Trama I de la estación se encuentra activo

                        await Task.Run(() => ImplementarTimeoutTramaI(protocolo.TimeoutTramaI)); //Se crea un hilo asíncrono que se encargará de implementar el timeout ante Trama I de la trama correspondiente
                        //Cuando se envie la primera trama de RR debido al agotamiento del timeout se pasa al timeout ante command
                    }
                }
                if (!timeout_command_activado && listaTramasEnviadas[i].InfoBitCommandRequest == "C" && listaTramasEnviadas[i].InfoBitSondeo == 1 && ExisteComandoPendiente()) //Si la trama enviada es un comando con el bit P/F activado, la cual no ha sido respondida, se activa el timeout ante command por no se ha respuesta a un comando previo enviado por la estación
                {
                    timeout_command_activado = true; //Activamos esta variable booleana por que en este caso solo nos interesa mantener un timeout ante command
                    SimboloTimeoutCommand.Visibility = Visibility.Visible; //Se hace visible un símbolo que indica que el timeout ante Command de la estación se encuentra activo

                    await Task.Run(() => ImplementarTimeoutCommand(protocolo.TimeoutCommand, listaTramasEnviadas[i])); //Se crea un hilo asíncrono que se encargará de implementar el timeout ante Command de la trama correspondiente
                }
            }
        }

        private bool ExisteComandoPendiente()
        {
            int indice = -1;
            for (int i = listaTramasEnviadas.Count - 1; i >= 0; i--) //Se recorre la lista de tramas enviadas desde el final en busca de comandos con el bit P/F activado
            {
                if (listaTramasEnviadas[i].InfoBitSondeo == 1 && listaTramasEnviadas[i].InfoBitCommandRequest == "C") //Si en el recorrido, se encuentra un comando con el bit P/F activado, se obtiene el índice o posición en el que se encuentra dicho comando y finaliza el recorrido
                {
                    indice = i;
                    break;
                }
            }

            if (indice == -1) //Si el indice vale -1, significa que no existe ningun comando con el bit P/F activado, por lo que no existe ningun comando sin responder y se devuelve false
            {
                return false;
            }

            for (int i = indice; i < listaTramasRecibidas.Count; i++) //Se recorre la lista de tramas recibidas desde el comando previamente encontrado en busca de respuestas con el bit P/F activado
            {
                if (listaTramasRecibidas[i].InfoBitSondeo == 1 && listaTramasRecibidas[i].InfoBitCommandRequest == "R") //Si en el recorrido, se encuentra una respuesta con el bit P/F activado, se devuelve false ya que existe una respuesta al comando previamente realizado
                {
                    return false;
                }
            }
            return true; //Si finaliza el recorrido sin encontrar una respuesta con el bit P/F activado, se devuelve true ya que no existe una respuesta al comando previamente realizado
        }

        private void ListaTramasEnviadas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null) //Si se han añadido nuevas tramas a la lista de tramas enviadas
            {
                foreach (Trama tr in e.NewItems) //Se recorre la lista que contiene las nuevas tramas añadidas a la lista de tramas enviadas
                {
                    TablaTramasEnviadas.ScrollIntoView(tr); //Se desplaza el scroll de la tabla de tramas enviadas hacia la última trama enviada
                    if (tr.InfoBitSondeo >= 0) //Si la trama añadida no se encuentra vacía, se procederá a realizar la representación gráfica del envío de dicha trama (recuerdo que las tramas vacias tiene un valor del bit de sondeo negativo)
                    {
                        dibujarTrama(tr, false);
                    }
                }
            }
        }

        private void ListaTramasRecibidas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null) //Si se han añadido nuevas tramas a la lista de tramas recibidas
            {
                foreach (Trama tr in e.NewItems) //Se recorre la lista que contiene las nuevas tramas añadidas a la lista de tramas recibidas
                {
                    TablaTramasRecibidas.ScrollIntoView(tr); //Se desplaza el scroll de la tabla de tramas recibidas hacia la última trama recibida
                    if (tr.InfoBitSondeo >= 0) //Si la trama añadida no se encuentra vacía, se procederá a realizar la representación gráfica de la recepción de dicha trama (recuerdo que las tramas vacias tiene un valor del bit de sondeo negativo)
                    {
                        dibujarTrama(tr, true);
                    }
                }
            }
        }

        private Point CrearPuntoFlechaTrama(int coordenadaX, int coordenadaY)
        {
            Point punto = new Point(); //Se crea una nueva instancia de un punto
            punto.X = coordenadaX; //Se asigna al componente X del punto, el valor de la coordeanda X del punto recibida como parámetro
            punto.Y = coordenadaY; //Se asigna al componente Y del punto, el valor de la coordeanda Y del punto recibida como parámetro

            return punto; //Se devuelve el punto creado
        }

        private Label CrearEtiquetaTrama(String contenido, Brush color_fuente, int tamaño_texto, String tipo_fuente, FontWeight grosor_fuente, FontStyle estilo_fuente)
        {
            Label etiqueta = new Label(); //Se crea una instacia de una etiqueta
            etiqueta.Content = contenido; //Se asigna al contenido de la etiqueta, el mensaje recibido como parámetro 
            etiqueta.Foreground = color_fuente; //Se asigna al color de fuente de la etiqueta, el color de fuente recibido como parámetro
            etiqueta.FontSize = tamaño_texto; //Se asigna al tamaño del texto de la etiqueta, el tamaño recibido como parámetro
            FontFamily fuente = new FontFamily(tipo_fuente); 
            etiqueta.FontFamily = fuente; //Se asigna a la familia de fuente de la etiqueta, el tipo de familia recibido como parámetro
            etiqueta.FontWeight = grosor_fuente; //Se asigna al grosor de fuente de la etiqueta, el tipo de grosor recibido como parámetro
            etiqueta.FontStyle = estilo_fuente; //Se asigna al estilo de fuente de la etiqueta, el tipo de estilo recibido como parámetro

            return etiqueta; //Se devuelve la etiqueta creada
        }

        private PointCollection crearPuntosFlechaTrama(int coordenadaX_inicial, int coordenadaY_inicial, int coordenadaX_final, int coordenadaY_final, int separacion_tramas, bool tramaRecibida)
        {
            PointCollection puntos_fecha_trama = new PointCollection(); //Se crea una nueva colección de puntos

            puntos_fecha_trama.Add(CrearPuntoFlechaTrama(coordenadaX_inicial, coordenadaY_inicial + (numero_tramas_dibujadas * separacion_tramas))); //Se añade el punto inicial de la flecha a la colección (la coordenada Y del punto se define a partir del número de tramas dibujadas y la separación vertical entre tramas)
            puntos_fecha_trama.Add(CrearPuntoFlechaTrama(coordenadaX_final, coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas))); //Se añade el punto final de la flecha a la colección (la coordenada Y del punto se define a partir del número de tramas dibujadas y la separación vertical entre tramas)
            if (tramaRecibida) //Si la trama a dibujar se trata de una trama recibida
            {
                puntos_fecha_trama.Add(CrearPuntoFlechaTrama(coordenadaX_final + 11, coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas) - 10)); //Se añade el punto de la esquina derecha de la flecha a la colección (la coordenada Y del punto se define a partir del número de tramas dibujadas y la separación vertical entre tramas) (Se aplica un pequeño desplazamiento de la coordenada X e Y con respecto del punto final de la flecha de la trama)
                puntos_fecha_trama.Add(CrearPuntoFlechaTrama(coordenadaX_final, coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas))); //Se añade de nuevo el punto final de la flecha a la colección (la coordenada Y del punto se define a partir del número de tramas dibujadas y la separación vertical entre tramas)
                puntos_fecha_trama.Add(CrearPuntoFlechaTrama(coordenadaX_final + 11, coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas) + 9)); //Se añade el punto de la esquina izquierda de la flecha a la colección (la coordenada Y del punto se define a partir del número de tramas dibujadas y la separación vertical entre tramas) (Se aplica un pequeño desplazamiento de la coordenada X e Y con respecto del punto final de la flecha de la trama)
            }
            else //Si la trama a dibujar se trata de una trama enviada
            {
                puntos_fecha_trama.Add(CrearPuntoFlechaTrama(coordenadaX_final - 11, coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas) - 10)); //Se añade el punto de la esquina derecha de la flecha a la colección (la coordenada Y del punto se define a partir del número de tramas dibujadas y la separación vertical entre tramas) (Se aplica un pequeño desplazamiento de la coordenada X e Y con respecto del punto final de la flecha de la trama)
                puntos_fecha_trama.Add(CrearPuntoFlechaTrama(coordenadaX_final, coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas))); //Se añade de nuevo el punto final de la flecha a la colección (la coordenada Y del punto se define a partir del número de tramas dibujadas y la separación vertical entre tramas)
                puntos_fecha_trama.Add(CrearPuntoFlechaTrama(coordenadaX_final - 11, coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas) + 9)); //Se añade el punto de la esquina izquierda de la flecha a la colección (la coordenada Y del punto se define a partir del número de tramas dibujadas y la separación vertical entre tramas) (Se aplica un pequeño desplazamiento de la coordenada X e Y con respecto del punto final de la flecha de la trama)
            }
            puntos_fecha_trama.Add(CrearPuntoFlechaTrama(coordenadaX_final, coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas))); //Se añade de nuevo el punto final de la flecha a la colección (la coordenada Y del punto se define a partir del número de tramas dibujadas y la separación vertical entre tramas)

            return puntos_fecha_trama; //Se devuelve la colección de puntos que conforman la flecha de la trama a representar graficamente
        }

        private double ObtenerDesplazamientoEtiquetaSubindice(Trama tr)
        {
            //Se crea un diccionario con los desplazamientos que se deben aplicar a las etiquetas de los subindices en función del tipo de trama que se esta representando
            Dictionary<String, double> desplazamientos_etiqueta_superindice = new Dictionary<String, double>
            {
                { TipoTrama.Informacion, 23 },
                { TipoTrama.Receptor_preparado, 35 },
                { TipoTrama.Receptor_no_preparado, 40 },
                { TipoTrama.Rechazo, 37 },
                { TipoTrama.Rechazo_selectivo, 41 },
                { TipoTrama.Solicitud_conexion, 55 },
                { TipoTrama.Asentimiento_no_numerado, 46 },
                { TipoTrama.Solicitud_desconexion, 52 },
                { TipoTrama.Modo_desconectado, 48 },
                { TipoTrama.Rechazo_trama, 54 }
            };

            return desplazamientos_etiqueta_superindice[tr.TipoDeTrama]; //Se devuelve el desplazamiento correspodiente en función del tipo de trama pasado como parámetro
        }

        private void AñadirEtiquetaSuperindice(Trama tr, int coordenadaY_inicial, int separacion_tramas, Label texto_trama)
        {
            //Se crea una segunda etiqueta que contenga el estado del bit de sondeo (debe crearse una segunda etiqueta ya que no es posible definir superindices)
            Label texto_superindice = CrearEtiquetaTrama(tr.InfoBitSondeoTabla, Brushes.Black, 9, "Segoe UI", FontWeights.DemiBold, FontStyles.Italic);

            //Se define una anchura de 20 para la etiqueta de la trama y se centra horizontalmente el contenido de la etiqueta de la trama
            texto_superindice.Width = 20;
            texto_superindice.HorizontalContentAlignment = HorizontalAlignment.Center;

            //En función del tipo de trama que se representa graficamente, se aplica un determinado desplazamiento horizontal. Por otro lado, tambien se aplica un desplazamiento vertical calculado en función del número de tramas dibujadas y la separación vertical entre tramas
            Canvas.SetLeft(texto_superindice, ((CanvasSeccionGrafica.ActualWidth - texto_trama.Width) / 2) + ObtenerDesplazamientoEtiquetaSubindice(tr));
            Canvas.SetTop(texto_superindice, coordenadaY_inicial + (numero_tramas_dibujadas * separacion_tramas) - 22);

            CanvasSeccionGrafica.Children.Add(texto_superindice); //Se añade la etiqueta del superindice al canvas de la sección gráfica
        }

        private void dibujarTrama(Trama tr, bool tramaRecibida)
        {
            //Se definen variables relevantes para la representación gráfica de las tramas como las coordenadas X e Y de los puntos de inicio y fin de la flecha de la trama o la separación vertical entre 2 tramas
            int coordenadaX_inicial = 57;
            int coordenadaY_inicial = 20;
            int coordenadaX_final = 230;
            int coordenadaY_final = 30;
            int separacion_tramas = 30;

            Polyline flecha_trama = new Polyline(); //Se crea una nueva instacia de una polilínea, la cual representará la flecha de la trama representada graficamente         
            flecha_trama.StrokeThickness = 2; //Se le asigna a la polilínea un grosor de 2 puntos
            Color colorFlecha;
            if (tramaRecibida) //Si la trama a representar graficamente se trata de una trama recibida
            {
                //Se definen los puntos de la polilínea con el objetivo de que tengan forma de flecha (hay que tener en cuenta la orientación ya que se trata de una trama recibida)
                flecha_trama.Points = crearPuntosFlechaTrama(coordenadaX_final, coordenadaY_inicial, coordenadaX_inicial, coordenadaY_final, separacion_tramas, tramaRecibida);
                colorFlecha = Color.FromRgb(34, 168, 79); //Se asigna a la flecha un color verde ya que se trata de una trama recibida
                tramas_recibidas = tramas_recibidas + 1; //Se incrementa en una unidad el número de trama recibidas representadas graficamente
            }
            else //Si la trama a representar graficamente se trata de una trama enviada
            {
                //Se definen los puntos de la polilínea con el objetivo de que tengan forma de flecha (hay que tener en cuenta la orientación ya que se trata de una trama enviada)
                flecha_trama.Points = crearPuntosFlechaTrama(coordenadaX_inicial, coordenadaY_inicial, coordenadaX_final, coordenadaY_final, separacion_tramas, tramaRecibida);
                colorFlecha = Color.FromRgb(35, 94, 255); //Se asigna a la flecha un color azul ya que se trata de una trama enviada
                tramas_enviadas = tramas_enviadas + 1; //Se incrementa en una unidad el número de trama enviadas representadas graficamente
            }
            flecha_trama.Stroke = new SolidColorBrush(colorFlecha); //Se le asigna a la polilínea el color previamente elegido (varia en función de si se trata de una trama enviada o recibida)
            flecha_trama.StrokeLineJoin = PenLineJoin.Round; //Se definen las esquinas de la polilínea como redondeadas (para conseguir un mejor diseño para la flecha)

            AnimacionFlechaTrama(flecha_trama, numero_tramas_dibujadas); //Se aplica una animación sobre la flecha de la trama representada graficamente
            AnimacionSobreTrama(tramaRecibida, numero_tramas_dibujadas); //Se aplica una animación sobre el sobre de la trama representada graficamente

            //Se crea la etiqueta que contiene información básica sobre la trama representada graficamente
            Label texto_trama = CrearEtiquetaTrama(tr.DireccionEstacion + ", " + tr.TipoDeTramaTabla, Brushes.Black, 12, "Segoe UI", FontWeights.DemiBold, FontStyles.Italic);

            //Se define una anchura de 75 para la etiqueta de la trama y se centra horizontalmente el contenido de la etiqueta de la trama
            texto_trama.Width = 75;
            texto_trama.HorizontalContentAlignment = HorizontalAlignment.Center;

            //Se coloca la etiqueta de la trama justo encima de la flecha de la trama correspondiente
            Canvas.SetLeft(texto_trama, (CanvasSeccionGrafica.ActualWidth - texto_trama.Width) / 2);
            Canvas.SetTop(texto_trama, coordenadaY_inicial + (numero_tramas_dibujadas * separacion_tramas) - 20);

            if (tr.InfoBitSondeo == 1) //Si la trama a representar graficamente tiene el bit de sondeo activado
            {
                if (tr.TramaIncorrecta == false) //Si la trama a representar graficamente no se trata de una trama errónea
                {
                    //Se crea y añade una segunda etiqueta que contenga el estado del bit de sondeo (el estado del bit de sondeo se coloca como un superindice y por esto es necesario una segunda etiqueta)
                    AñadirEtiquetaSuperindice(tr, coordenadaY_inicial, separacion_tramas, texto_trama);
                }
            }

            if (tr.TramaIncorrecta == true) //Si la trama a representar graficamente se trata de una trama errónea
            {
                flecha_trama.Stroke = Brushes.Red; //Se asigna a la flecha un color rojo ya que se trata de una trama errónea
            }
            else if (tr.TipoTramaEstructuraControl == TipoTramaControl.Informacion) //Si la trama a representar graficamente se trata de una trama de información
            {
                //Al tratarse de una trama de información, se incluye entre parentesis el número de secuencia y el número de trama esperada de la trama
                texto_trama.Content = texto_trama.Content + "  (" + tr.NumeroSecuencia + ", " + tr.NumeroTramaEsperada + ")";
            }
            else if (tr.TipoTramaEstructuraControl == TipoTramaControl.Supervision) //Si la trama a representar graficamente se trata de una trama de supervisión
            {
                //Al tratarse de una trama de supervisión, se incluye entre paréntesis el número de trama esperada de la trama
                texto_trama.Content = texto_trama.Content + "  (" + tr.NumeroTramaEsperada + ")";
            }
          
            CanvasSeccionGrafica.Children.Add(texto_trama); //Se añade la etiqueta de la trama al canvas de la sección gráfica
            numero_tramas_dibujadas = numero_tramas_dibujadas + 1; //Se incrementa en una unidad el número de tramas dibujadas

            texto_trama.BringIntoView(); //Se desplaza el scroll de la sección gráfica hacia la última trama representada
            ScrollCanvasSeccionGrafica.ScrollToVerticalOffset(ScrollCanvasSeccionGrafica.VerticalOffset + 30); //Se despleza el scroll 30 píxeles mas para dejar margen

            //Si la trama dibujada "amenaza" con agotar la altura de canvas, se incrementa la altura del canvas de la sección gráfica y de los ejes para que se puedan representar y visualizar nuevas tramas
            if (coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas) > CanvasSeccionGrafica.Height)
            {
                CanvasSeccionGrafica.Height = CanvasSeccionGrafica.Height + separacion_tramas;
                EjeEstacionLocal.Y2 = EjeEstacionLocal.Y2 + separacion_tramas; 
                EjeEstacionContraria.Y2 = EjeEstacionContraria.Y2 + separacion_tramas;
            }
        }

        private void AnimacionFlechaTrama(Polyline flecha_trama, int numero_trama)
        {
            //Se crea un nombre para la animación de la flecha (en función del número de tramas dibujadas) y se registra el nombre de la animación de la flecha
            flecha_trama.Name = "FlechaTrama" + numero_trama; 
            this.RegisterName(flecha_trama.Name, flecha_trama);

            //Se crea la animación y se definen los parámetros fundamentales de la animación
            DoubleAnimation dobleAnimacion = new DoubleAnimation();
            dobleAnimacion.From = 0.0; //Se define el valor inicial para la animación
            dobleAnimacion.To = 1.0; //Se define el valor final para la animación
            dobleAnimacion.Duration = new Duration(TimeSpan.FromSeconds(1)); //Se define la duración de la animación a 1 segundo
            dobleAnimacion.AutoReverse = false; //Se define la animación para que no vuelve hacia atrás

            //Se crea el stortboard el cual se encargara de ejecutar la animación, donde se le asociará la animación correspodiente, el nombre y la propiedad del objeto a animar
            Storyboard storyboardAnimacionSobre = new Storyboard();
            storyboardAnimacionSobre.Children.Add(dobleAnimacion); //Se establece la animación creada como hijo del storyboard
            Storyboard.SetTargetName(dobleAnimacion, flecha_trama.Name); //Se asocia el nombre registrado para la animación a la propia animación
            Storyboard.SetTargetProperty(dobleAnimacion, new PropertyPath(Polyline.OpacityProperty)); //Se establece la propiedad la cual se desea animar (en este caso se desea animar la opacidad de la polilínea)

            //Se registra el evento Loaded de la imagen del sobre, para que se ejecute la animación cuando la flecha se cargue en el canvas
            flecha_trama.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                storyboardAnimacionSobre.Begin(this); //Cuando la flecha se dibuje en el canvas, se pondrá en marcha la animación correspodiente, utilizando el método begin del Storyboard creado anteriormente
            };

            //Se añade la flecha al canvas de la seccion gráfica
            CanvasSeccionGrafica.Children.Add(flecha_trama);
        }

        private void AnimacionSobreTrama(bool tramaRecibida, int numero_trama)
        {
            //Se definen variables relevantes para la representación gráfica del sobre como las coordenadas X e Y de los puntos de inicio y fin de la ruta que seguirá el sobre o la separación vertical entre 2 sobres
            int coordenadaX_inicial = 32;
            int coordenadaY_inicial = 20;
            int coordenadaX_final = 235;
            int coordenadaY_final = 30;
            int separacion_tramas = 30;

            if (tramaRecibida)
            {
                coordenadaX_inicial = 235;
                coordenadaX_final = 32;
            }

            //Se crea la imagen del sobre, con una anchura y altura de 20 píxeles
            Image imagenSobre = new Image();
            BitmapImage imagenSobreBitmap = new BitmapImage(new Uri("sobre.png", UriKind.Relative));
            imagenSobre.Source = imagenSobreBitmap;
            imagenSobre.Width = 20;
            imagenSobre.Height = 20;

            if (tramaRecibida) //Si la trama asociada al sobre es una trama recibida, se almacena en el tag de la imagen del sobre Recibida + el número de trama recibida
            {
                imagenSobre.Tag = "Recibida" + tramas_recibidas;
            }
            else //Si la trama asociada al sobre es una trama enviada, se almacena en el tag de la imagen del sobre Enviada + el número de trama recibida
            {
                imagenSobre.Tag = "Enviada" + tramas_enviadas;
            }
            //Esto tiene como objetivo permitir diferenciar univocamente los sobres para que así, cuando el usuario pulse a un sobre, se muestre información detallada sobre la trama asociada al sobre

            //Creamos una transformación que se utilizará para mover el sobre
            TranslateTransform transformacionSobre = new TranslateTransform();

            //Registramos el nombre de la transformación para que posteriormente pueda ser utilizada desde el storyboard
            this.RegisterName("SobreTrama" + numero_trama, transformacionSobre);

            imagenSobre.RenderTransform = transformacionSobre;

            //Añadimos el sobre al canvas de la sección gráfica
            CanvasSeccionGrafica.Children.Add(imagenSobre);

            //Creamos la ruta de animación por la cual se desplazará el sobre
            PathGeometry rutaSobre = new PathGeometry(); //Se crea un PathGeometry que contendrá la ruta por la cual se desplazará el sobre
            PathFigure pFigure = new PathFigure(); //Se crea un PathFigure que contendrá los puntos de la ruta 
            pFigure.StartPoint = new Point(coordenadaX_inicial, coordenadaY_inicial + (numero_tramas_dibujadas * separacion_tramas) - 10); //Se establece el punto inicial de la ruta por la cual se desplazará el sobre 
            LineSegment lineaSegment = new LineSegment(new Point(coordenadaX_final, coordenadaY_final + (numero_tramas_dibujadas * separacion_tramas) - 10), true); ///Se establece el punto  de la ruta por la cual se desplazará el sobre (teniendo en cuenta el numero de tramas dibujadas y la separación entre tramas) 
            pFigure.Segments.Add(lineaSegment); //Se añade el punto final de la ruta, al pathFigure
            rutaSobre.Figures.Add(pFigure); //Se añade la ruta con los puntos al PathGeometry

            //Se congela la ruta, para obtener un mejor rendimiento
            rutaSobre.Freeze();

            //Se crea un DoubleAnimationUsingPath para mover el sobre horizontalmente a traves de la ruta creada
            DoubleAnimationUsingPath animacionMovimientoX = new DoubleAnimationUsingPath();
            animacionMovimientoX.PathGeometry = rutaSobre; //Asignamos la ruta de animación
            animacionMovimientoX.Duration = TimeSpan.FromSeconds(2); //Definimos la duración de la animación en 2 segundos
            animacionMovimientoX.Source = PathAnimationSource.X; //Establecemos el origen de la animación con la propiedad X. Con esto, se generarán los valores de desplazamiento horizontal a partir de la información de la ruta.

            //Asignamos a la transformación de movimiento la animación de movimiento horizontal en el eje X
            Storyboard.SetTargetName(animacionMovimientoX, "SobreTrama" + numero_trama); //Se asocia el nombre registrado para la animación a la propia animación
            Storyboard.SetTargetProperty(animacionMovimientoX, new PropertyPath(TranslateTransform.XProperty)); //Se establece la propiedad la cual se desea animar (en este caso se desea animar el componente X del movimiento del sobre)

            //Se crea un DoubleAnimationUsingPath para mover el sobre verticalmente a traves de la ruta
            DoubleAnimationUsingPath animacionMovimientoY = new DoubleAnimationUsingPath();
            animacionMovimientoY.PathGeometry = rutaSobre; //Asignamos la ruta de animación
            animacionMovimientoY.Duration = TimeSpan.FromSeconds(2); //Definimos la duración de la animación en 2 segundos
            animacionMovimientoY.Source = PathAnimationSource.Y; //Establecemos el origen de la animación con la propiedad Y. Con esto, se generarán los valores de desplazamiento vertical a partir de la información de la ruta.

            //Asignamos a la transformacion de movimiento la animacion de movimiento vertical en el eje Y
            Storyboard.SetTargetName(animacionMovimientoY, "SobreTrama" + numero_trama); //Se asocia el nombre registrado para la animación a la propia animación
            Storyboard.SetTargetProperty(animacionMovimientoY, new PropertyPath(TranslateTransform.YProperty)); //Se establece la propiedad la cual se desea animar (en este caso se desea animar el componente Y del movimiento del sobre)

            //Se crea un storyboard que contiene y aplica las animaciones creadas anteriormente para mover la imagen del sobre por el eje X y eje Y de la ruta
            Storyboard storyboardAnimacionSobre = new Storyboard();
            storyboardAnimacionSobre.Children.Add(animacionMovimientoX);
            storyboardAnimacionSobre.Children.Add(animacionMovimientoY);

            //Se registra el evento Loaded de la imagen del sobre, para que se ejecute la animación cuando la flecha se cargue en el canvas
            imagenSobre.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                storyboardAnimacionSobre.Begin(this); //Cuando el sobre se dibuje en el canvas, se pondrá en marcha la animación correspodiente, utilizando el método begin del Storyboard creado anteriormente
            };

            imagenSobre.MouseLeftButtonDown += VerDetalleTramaSeleccionada; //Se registrar el evento MouseLeftButtonDown de la imagen del sobre, para que se recopile y se muestre la información detallada de la trama asociada a dicho sobre pulsado
        }

        private void VerDetalleTramaSeleccionada(object sender, MouseButtonEventArgs e)
        {
            //Obtenemos el remitente (imagen) el cual ha generado la llamada a esta función
            Image imagenSobre = (Image)sender;
            int indice = 0;

            if (imagenSobre.Tag.ToString().StartsWith("Enviada")) //Comprobamos el tag de la imagen que ha sido pulsada y desencadenado el evento, representa un trama enviada
            {
                //Recorremos la lista de tramas enviadas desde el principio
                for (int i = 0; i < listaTramasEnviadas.Count; i++)
                {
                    if (listaTramasEnviadas[i].InfoBitSondeo != -1) //Si la trama enviada recorrida no esta vacia (el bit de sondeo tiene un valro válido) se incrementa el valor del índice en una unidad
                    {
                        indice = indice + 1;
                    }
                    if (indice == Int32.Parse(imagenSobre.Tag.ToString().Substring(imagenSobre.Tag.ToString().Length - 1))) //Si el valor del índice coincide con el número de tag que incluye la imagen que ha sido pulsada, se actualiza el valor del índice y finaliza el recorrido
                    {
                        indice = i;
                        break;
                    }
                } //En este recorrido, se ha buscado encontrar la posición dentro de la lista de tramas enviadas en la que se encuentra la trama asociada a la imagen del sobre pulsada

                MostrarDetalleTramaEnviada(indice); //Se muestra el detalle de la trama asociada a la imagen del sobre pulsada
            }
            else if (imagenSobre.Tag.ToString().StartsWith("Recibida")) //Comprobamos el tag de la imagen que ha sido pulsada y desencadenado el evento, representa un trama recibida
            {
                //Recorremos la lista de tramas recibidas desde el principio
                for (int i = 0; i < listaTramasRecibidas.Count; i++)
                {
                    if (listaTramasRecibidas[i].InfoBitSondeo != -1) //Si la trama recibida recorrida no esta vacia (el bit de sondeo tiene un valro válido) se incrementa el valor del índice en una unidad
                    {
                        indice = indice + 1;
                    }
                    if (indice == Int32.Parse(imagenSobre.Tag.ToString().Substring(imagenSobre.Tag.ToString().Length - 1))) //Si el valor del índice coincide con el número de tag que incluye la imagen que ha sido pulsada, se actualiza el valor del índice y finaliza el recorrido
                    {
                        indice = i;
                        break;
                    }
                } //En este recorrido, se ha buscado encontrar la posición dentro de la lista de tramas recibidas en la que se encuentra la trama asociada a la imagen del sobre pulsada

                MostrarDetalleTramaRecibida(indice); //Se muestra el detalle de la trama asociada a la imagen del sobre pulsada
            }
        }

        private void MostrarDetalleTramaEnviada(int indice)
        {
            if (listaTramasEnviadas[indice].TipoTramaEstructuraControl == TipoTramaControl.No_numerada) //Si la trama a mostrar el detalle, se trata de una trama no numerada
            {
                //Se crea una instacia de la VentanaVisualizaciónDetalleTramaNoNumerada a la cual se le pasa como parámetro la trama enviada a representar
                VentanaVisualizaciónDetalleTramaNoNumerada vvdtnn = new VentanaVisualizaciónDetalleTramaNoNumerada(listaTramasEnviadas[indice]);
                vvdtnn.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvdtnn.Owner = this;

                vvdtnn.ShowDialog(); //Se muestra la ventana de visualización de la trama no numerada
            }
            else if (listaTramasEnviadas[indice].TipoTramaEstructuraControl == TipoTramaControl.Informacion) //Si la trama a mostrar el detalle, se trata de una trama de información
            {
                //Se crea una instacia de la VentanaVisualizaciónDetalleTramaInformación a la cual se le pasa como parámetro la trama enviada a representar
                VentanaVisualizaciónDetalleTramaInformación vvdti = new VentanaVisualizaciónDetalleTramaInformación(listaTramasEnviadas[indice]);
                vvdti.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvdti.Owner = this;

                vvdti.ShowDialog(); //Se muestra la ventana de visualización de la trama de información
            }
            else if (listaTramasEnviadas[indice].TipoTramaEstructuraControl == TipoTramaControl.Supervision) //Si la trama a mostrar el detalle, se trata de una trama de supervisión
            {
                //Se crea una instacia de la VentanaVisualizaciónDetalleTramaSupervisión a la cual se le pasa como parámetro la trama enviada a representar
                VentanaVisualizaciónDetalleTramaSupervisión vvdts = new VentanaVisualizaciónDetalleTramaSupervisión(listaTramasEnviadas[indice]);
                vvdts.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvdts.Owner = this;

                vvdts.ShowDialog(); //Se muestra la ventana de visualización de la trama de supervisión
            }
        }

        private void MostrarDetalleTramaRecibida(int indice)
        {
            if (listaTramasRecibidas[indice].TipoTramaEstructuraControl == TipoTramaControl.No_numerada) //Si la trama a mostrar el detalle, se trata de una trama no numerada
            {
                //Se crea una instacia de la VentanaVisualizaciónDetalleTramaNoNumerada a la cual se le pasa como parámetro la trama recibida a representar
                VentanaVisualizaciónDetalleTramaNoNumerada vvdtnn = new VentanaVisualizaciónDetalleTramaNoNumerada(listaTramasRecibidas[indice]);
                vvdtnn.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvdtnn.Owner = this;

                vvdtnn.ShowDialog(); //Se muestra la ventana de visualización de la trama no numerada
            }
            else if (listaTramasRecibidas[indice].TipoTramaEstructuraControl == TipoTramaControl.Informacion) //Si la trama a mostrar el detalle, se trata de una trama de información
            {
                //Se crea una instacia de la VentanaVisualizaciónDetalleTramaInformación a la cual se le pasa como parámetro la trama recibida a representar
                VentanaVisualizaciónDetalleTramaInformación vvdti = new VentanaVisualizaciónDetalleTramaInformación(listaTramasRecibidas[indice]);
                vvdti.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvdti.Owner = this;

                vvdti.ShowDialog(); //Se muestra la ventana de visualización de la trama de información
            }
            else if (listaTramasRecibidas[indice].TipoTramaEstructuraControl == TipoTramaControl.Supervision) //Si la trama a mostrar el detalle, se trata de una trama de supervisión
            {
                //Se crea una instacia de la VentanaVisualizaciónDetalleTramaSupervisión a la cual se le pasa como parámetro la trama recibida a representar
                VentanaVisualizaciónDetalleTramaSupervisión vvdts = new VentanaVisualizaciónDetalleTramaSupervisión(listaTramasRecibidas[indice]);
                vvdts.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                vvdts.Owner = this;

                vvdts.ShowDialog(); //Se muestra la ventana de visualización de la trama de supervisión
            }
        }
      
        private void BotonCambioVisualización_Click(object sender, RoutedEventArgs e)
        {
            if (vistaGrafica) //Si la estación esta configurada en el modo de vista gráfico
            {
                //Se quita momentaneamente el límite para la anchura y altura la ventana, para poder ajustar las nuevas dimensiones de la ventana
                this.MinWidth = this.MinHeight = 0;
                this.MaxWidth = this.MaxHeight = 2000;

                //Se ajusta la altura y anchura de la ventana
                this.Height = 648;
                this.Width = 680;

                //Se ajusta la anchura y altura mínima y máxima al valor actual de la ventana para evitar el redimensinado de la ventana por parte del usuario
                this.MinWidth = this.MaxWidth = this.Width;
                this.MinHeight = this.MaxHeight = this.Height;

                //Se recolocan los elementos principales de la ventana de la estación y se modifica la altura de los elementos relevantes
                AjustarComponentesVentanaEstación(100);

                //Se aplica una rotacion de 90 grados a la botonora de las opciones
                RotateTransform rotacion90 = new RotateTransform(90);
                CanvasBotoneraOpciones.RenderTransform = rotacion90;

                //Se aplica una rotacion de 270 a los iconos de la botonera de las opciones
                RotateTransform rotacion270 = new RotateTransform(270);
                ImagenConfiguracion.RenderTransform = rotacion270;
                ImagenGuardar.RenderTransform = rotacion270;
                ImagenCargar.RenderTransform = rotacion270;
                ImagenAyuda.RenderTransform = rotacion270;

                //Se recoloca la botonera de opciones en función de las nuevas dimensiones de la ventana
                Canvas.SetTop(CanvasBotoneraOpciones, 50);
                Canvas.SetLeft(CanvasBotoneraOpciones, Canvas.GetLeft(CanvasBotoneraOpciones) + 27);
                Canvas.SetLeft(SeccionGraficoIntercambioTramas, Canvas.GetLeft(SeccionGraficoIntercambioTramas) + 30);

                CanvasSeccionGrafica.Visibility = Visibility.Hidden; //Se oculta el canvas de la sección gráfica

                //Se actualiza la imagen del botón del modo de visualización de la ventana con la imagen de un gráfico
                Image imagen = new Image();
                BitmapImage imagenBitmap = new BitmapImage(new Uri("modo-grafico.png", UriKind.Relative));
                ImagenModoVisualizacion.Source = imagenBitmap;

                vistaGrafica = false; //Tras realizar todas estas modificaciones, la estación dejará de estar en el modo de visualización gráfico, por lo que se pondra la variable vistaGráfica a falso
            }
            else
            {
                //Se quita momentaneamente el límite para la anchura y altura la ventana, para poder ajustar las nuevas dimensiones de la ventana
                this.MinWidth = this.MinHeight = 0;
                this.MaxWidth = this.MaxHeight = 2000;

                //Se ajusta la altura y anchura de la ventana
                this.Height = 548;
                this.Width = 968;

                //Se ajusta la anchura y altura mínima y máxima al valor actual de la ventana para evitar el redimensinado de la ventana por parte del usuario
                this.MinWidth = this.MaxWidth = this.Width;
                this.MinHeight = this.MaxHeight = this.Height;

                //Se recolocan los elementos principales de la ventana de la estación y se modifica la altura de los elementos relevantes
                AjustarComponentesVentanaEstación(-100);

                //Se aplica una rotacion de 0 a los iconos de la botonera de las opciones así como la propia botonera de opciones
                RotateTransform rotacion0 = new RotateTransform(0);
                CanvasBotoneraOpciones.RenderTransform = rotacion0;
                ImagenConfiguracion.RenderTransform = rotacion0;
                ImagenGuardar.RenderTransform = rotacion0;
                ImagenCargar.RenderTransform = rotacion0;
                ImagenAyuda.RenderTransform = rotacion0;

                //Se recoloca la botonera de opciones en función de las nuevas dimensiones de la ventana
                Canvas.SetTop(CanvasBotoneraOpciones, 0);
                Canvas.SetLeft(CanvasBotoneraOpciones, Canvas.GetLeft(CanvasBotoneraOpciones) - 27);
                Canvas.SetLeft(SeccionGraficoIntercambioTramas, Canvas.GetLeft(SeccionGraficoIntercambioTramas) - 30);

                CanvasSeccionGrafica.Visibility = Visibility.Visible; //Se hace visible el canvas de la sección gráfica

                //Se actualiza la imagen del botón del modo de visualización de la ventana con la imagen de un gráfico
                Image imagen = new Image();
                BitmapImage imagenBitmap = new BitmapImage(new Uri("modo_lectura.png", UriKind.Relative));
                ImagenModoVisualizacion.Source = imagenBitmap;

                vistaGrafica = true; //Tras realizar todas estas modificaciones, la estación volverá de estar en el modo de visualización gráfico, por lo que se pondra la variable vistaGráfica a true
            }
        }

        private void AjustarComponentesVentanaEstación(int desplazamiento)
        {
            //Se recolocan los componentes de la ventana
            Canvas.SetTop(BordeBotonCambioVisualización, Canvas.GetTop(BordeBotonCambioVisualización) + desplazamiento);
            Canvas.SetTop(EtiquetaSituación, Canvas.GetTop(EtiquetaSituación) + desplazamiento);
            Canvas.SetTop(BordeCajaSituacionEstacion, Canvas.GetTop(BordeCajaSituacionEstacion) + desplazamiento);
            Canvas.SetTop(ReferenciaTemporal, Canvas.GetTop(ReferenciaTemporal) + desplazamiento);
            Canvas.SetTop(BordeBotonInicializarConexionFisica, Canvas.GetTop(BordeBotonInicializarConexionFisica) + desplazamiento);
            Canvas.SetTop(BordeBotonFinalizarConexionFisica, Canvas.GetTop(BordeBotonFinalizarConexionFisica) + desplazamiento);

            //Se modifica la altura de los componentes necesarios
            SeccionTablaIntercambioTramas.Height = SeccionTablaIntercambioTramas.Height + desplazamiento;
            Canvas.SetTop(BotonAyudaSeccionTablaIntercambioTramas, Canvas.GetTop(BotonAyudaSeccionTablaIntercambioTramas) + desplazamiento);
            TablaTramasEnviadas.Height = TablaTramasEnviadas.Height + desplazamiento;
            TablaTramasRecibidas.Height = TablaTramasRecibidas.Height + desplazamiento;
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

        private void BotonAyudaSeccionInformacionBasicaTrama_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaSeccionInformacionBasicaTrama); //Si se pulsa el botón de ayuda de la sección de información básica de la trama, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaSeccionTablaIntercambioTramas_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaSeccionTablaIntercambioTramas); //Si se pulsa el botón de ayuda de la sección de la tabla de intercambio de tramas, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void BotonAyudaSeccionGraficoIntercambioTramas_Click(object sender, RoutedEventArgs e)
        {
            GestionarVisibilidadAyuda(CanvasAyudaSeccionGraficoIntercambioTramas); //Si se pulsa el botón de ayuda de la sección gráfica de intercambio de tramas, se cambia la visibilidad del bocadillo de ayuda correspondiente
        }

        private void TablaTramasEnviadas_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollTablaTramasEnviadas = GetScrollViewer(TablaTramasEnviadas); //Se obtiene el scrollviewer asociado a la tabla de tramas enviadas
            ScrollViewer scrollTablaTramasRecibidas = GetScrollViewer(TablaTramasRecibidas); //Se obtiene el scrollviewer asociado a la tabla de tramas recibidas

            if (e.VerticalChange != 0) //Si el scroll de la tabla de tramas enviadas se ha desplazado de manera vertical, se ajusta el scroll de la tabla de trama recibidas con el mismo valor que el scroll de la tabla de tramas enviadas
            {
                scrollTablaTramasRecibidas.ScrollToVerticalOffset(scrollTablaTramasEnviadas.VerticalOffset); //Para ajustar el scroll de la tabla de trama recibidas con el mismo valor que el scroll de la tabla de tramas enviadas, se utiliza la función ScrollToVerticalOffset
            }
        }

        private void TablaTramasRecibidas_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollTablaTramasEnviadas = GetScrollViewer(TablaTramasEnviadas); //Se obtiene el scrollviewer asociado a la tabla de tramas enviadas
            ScrollViewer scrollTablaTramasRecibidas = GetScrollViewer(TablaTramasRecibidas); //Se obtiene el scrollviewer asociado a la tabla de tramas recibidas

            if (e.VerticalChange != 0) //Si el scroll de la tabla de tramas recibidas se ha desplazado de manera vertical, se ajusta el scroll de la tabla de trama enviadas con el mismo valor que el scroll de la tabla de tramas recibidas
            {
                scrollTablaTramasEnviadas.ScrollToVerticalOffset(scrollTablaTramasRecibidas.VerticalOffset); //Para ajustar el scroll de la tabla de trama enviadas con el mismo valor que el scroll de la tabla de tramas recibidas, se utiliza la función ScrollToVerticalOffset
            }
        }

        private static ScrollViewer GetScrollViewer(DependencyObject listView)
        {
            Decorator borde = VisualTreeHelper.GetChild(listView, 0) as Decorator; //Se obtiene la referencia al scrollviewer del listview pasado como parámetro el cual se encuentra almacenado como un hijo del listview
            if (borde != null)
            {
                return borde.Child as ScrollViewer;
            }
            return null;
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

        private void BotonAyudaTramas_Click(object sender, RoutedEventArgs e) 
        {
            //Se crea un vector de 10 canvas en el que se van a almacenar los 10 canvas que contienen los bocadillos de ayuda de cada uno de los botones de la botonera superior
            Canvas[] vector_canvas = new Canvas[10];

            //Se asignan al vector los distintos canvas que contienen los bocadillos de ayuda (no se puede hacer un bucle por se necesita la referencia directa de cada canvas)
            vector_canvas[0] = CanvasAyudaBotoneraSuperior1;
            vector_canvas[1] = CanvasAyudaBotoneraSuperior2;
            vector_canvas[2] = CanvasAyudaBotoneraSuperior3;
            vector_canvas[3] = CanvasAyudaBotoneraSuperior4;
            vector_canvas[4] = CanvasAyudaBotoneraSuperior5;
            vector_canvas[5] = CanvasAyudaBotoneraSuperior6;
            vector_canvas[6] = CanvasAyudaBotoneraSuperior7;
            vector_canvas[7] = CanvasAyudaBotoneraSuperior8;
            vector_canvas[8] = CanvasAyudaBotoneraSuperior9;
            vector_canvas[9] = CanvasAyudaBotoneraSuperior10;

            //Se recorre el vector de canvas desde el principio con el objetivo de detectar cual de los canvas se encuentra visible en este momento
            for (int i = 0; i < vector_canvas.Length; i++)
            {
                if (vector_canvas[i].Visibility == Visibility.Visible && i == vector_canvas.Length - 1) //Si el canvas recorrido se encuentra visible y es el último canvas del vector, se oculta dicho canvas y finaliza el recorrido
                {
                    vector_canvas[i].Visibility = Visibility.Hidden;
                    return;
                }
                if (vector_canvas[i].Visibility == Visibility.Visible) //Si el canvas recorrido se encuentra visible, se oculta dicho canvas, se hace visible el canvas siguiente y finaliza el recorrido
                {
                    vector_canvas[i].Visibility = Visibility.Hidden;
                    vector_canvas[i + 1].Visibility = Visibility.Visible;
                    return;
                }
            }

            vector_canvas[0].Visibility = Visibility.Visible; //Si en el recorrido no se ha encontrado ningún canvas visible, se hace visible el primer canvas del vector          
        }

        private void BotonAyudaOpciones_Click(object sender, RoutedEventArgs e)
        {
            //Se crea un vector de 3 canvas en el que se van a almacenar los 3 canvas que contienen los bocadillos de ayuda de cada uno de los botones de la botonera de opciones
            Canvas[] vector_canvas = new Canvas[3];

            //Se asignan al vector los distintos canvas que contienen los bocadillos de ayuda (no se puede hacer un bucle por se necesita la referencia directa de cada canvas)
            vector_canvas[0] = CanvasBotoneraOpciones1;
            vector_canvas[1] = CanvasBotoneraOpciones2;
            vector_canvas[2] = CanvasBotoneraOpciones3;

            //Se recorre el vector de canvas desde el principio con el objetivo de detectar cual de los canvas se encuentra visible en este momento
            for (int i = 0; i < vector_canvas.Length; i++)
            {
                if (vector_canvas[i].Visibility == Visibility.Visible && i == vector_canvas.Length - 1) //Si el canvas recorrido se encuentra visible y es el último canvas del vector, se oculta dicho canvas y finaliza el recorrido
                {
                    vector_canvas[i].Visibility = Visibility.Hidden;
                    return;
                }
                if (vector_canvas[i].Visibility == Visibility.Visible) //Si el canvas recorrido se encuentra visible, se oculta dicho canvas, se hace visible el canvas siguiente y finaliza el recorrido
                {
                    vector_canvas[i].Visibility = Visibility.Hidden;
                    vector_canvas[i + 1].Visibility = Visibility.Visible;
                    return;
                }
            }

            vector_canvas[0].Visibility = Visibility.Visible; //Si en el recorrido no se ha encontrado ningún canvas visible, se hace visible el primer canvas del vector
        }

        private void BotonEnvioTramaSolicitudConexion_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaSolicitudConexion_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama SABM, se llama al método BotonEnvioTramaSolicitudConexion_Click donde el remitente será ENVIO_DIRECTO
        }

        private void BotonEnvioTramaAsentimientoNoNumerado_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaAsentimientoNoNumerado_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama UA, se llama al método BotonEnvioTramaAsentimientoNoNumerado_Click donde el remitente será ENVIO_DIRECTO
        }

        private void BotonEnvioTramaSolicitudDesconexion_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaSolicitudDesconexion_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama DISC, se llama al método BotonEnvioTramaSolicitudDesconexion_Click donde el remitente será ENVIO_DIRECTO
        }

        private void BotonEnvioTramaModoDesconectado_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaModoDesconectado_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama DM, se llama al método BotonEnvioTramaModoDesconectado_Click donde el remitente será ENVIO_DIRECTO
        }

        private void BotonEnvioTramaRechazoTrama_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaRechazoTrama_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama FRMR, se llama al método BotonEnvioTramaRechazoTrama_Click donde el remitente será ENVIO_DIRECTO
        }

        private void BotonEnvioTramaInformacion_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaInformacion_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama I, se llama al método BotonEnvioTramaInformacion_Click donde el remitente será ENVIO_DIRECTO
        }

        private void BotonEnvioTramaReceptorPreparado_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaReceptorPreparado_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama RR, se llama al método BotonEnvioTramaReceptorPreparado_Click donde el remitente será ENVIO_DIRECTO
        }

        private void BotonEnvioTramaReceptorNoPreparado_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaReceptorNoPreparado_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama RNR, se llama al método BotonEnvioTramaReceptorNoPreparado_Click donde el remitente será ENVIO_DIRECTO
        }

        private void BotonEnvioTramaRechazo_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaRechazo_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama REJ, se llama al método BotonEnvioTramaRechazo_Click donde el remitente será ENVIO_DIRECTO
        }

        private void BotonEnvioTramaRechazoSelectivo_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BotonEnvioTramaRechazoSelectivo_Click("ENVIO_DIRECTO", new RoutedEventArgs()); //Si se hace click derecho sobre el botón de envío de una trama SREJ, se llama al método BotonEnvioTramaRechazoSelectivo_Click donde el remitente será ENVIO_DIRECTO
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            String min;
            String seg;

            //Obtenemos la referencia de tiempo y asignamos dicha referencia a la variable entera segundos
            DateTime fechaTrama = DateTime.UtcNow;
            TimeSpan tiempoTranscurrido = fechaTrama - fechaActual;
            segundos = (int)Math.Round(tiempoTranscurrido.TotalMilliseconds / 1000, 3);

            if (segundos >= 60) //Si el número de segundos es igual o superior a 60, se desgranará la  referencia temporal en minutos y segundos
            {
                minutos = segundos / 60; //El número de minutos sera la division entera del número de segundo entre 60
                segundos = segundos % 60; //El número de segundos será el resto de la división del número de segundos entre 60
            }

            //Se añaden 0s a la izquierda al número de segundos y minutos para su representación en la pantalla en un formato de 2 dígitos
            seg = Convert.ToString(segundos).PadLeft(2, '0');
            min = Convert.ToString(minutos).PadLeft(2, '0');

            //Se representa en el label correspondiente, la referencia temporal en el foramto minutos:segundos
            ReferenciaTemporal.Content = Convert.ToString(min + ":" + seg);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas_visible != null) //Si la referencia al último canvas visible no es nula y se ha pulsado cualquier parte de la pantalla, se ocultará dicho canvas
            {
                canvas_visible.Visibility = Visibility.Hidden;
            }
        }

        private void BotonCapturaImagen_Click(object sender, RoutedEventArgs e)
        {
            //Se crea un RenderTargetBitmap con las dimensiones del Canvas
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)CanvasSeccionGrafica.ActualWidth, (int)CanvasSeccionGrafica.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            //Se renderiza el contenido del Canvas en el RenderTargetBitmap
            renderBitmap.Render(CanvasSeccionGrafica);

            //Se crea un codificador de imágenes (por ejemplo, PNG)
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            SaveFileDialog saveFileDialog = new SaveFileDialog(); //Se crea una instancia de la ventana de exploración de Windows
            saveFileDialog.Filter = "Archivos PNG (*.png)|*.png"; //Se ajustan los filtros de la ventana de exploración de Windows, con el objetivo de poder acceder a cualquier tipo de archivo
            saveFileDialog.FileName = "Imagen_Captura_Tráfico"; //Se aigna un nombre por defecto a la imagen
            if (saveFileDialog.ShowDialog() == true) //Si el usuario confirma su deseo de guardar la captura de tráfico en un fichero con un determinado nombre y una determinada ubicación
            {
                using (FileStream file = new FileStream(saveFileDialog.FileName, FileMode.Create)) //Se guarda la imagen con el nombre y la ruta indicada
                {
                    encoder.Save(file);
                }
            }        
        }
    }
}
