# Aplicación para la simulación del protocolo HDLC

![Inicial](https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/cbbe1d24-2982-4f5b-b33f-d2c34d43b0cc)

# - Introducción

Proyecto realizado en la asignatura de Trabajo de Fin de Grado del grado de Ingenieria Informática de la Universidad de Salamanca. El enunciado del proyecto se encuentra subido en el repositorio en un archivo pdf llamado *Popuesta TFG.pdf*.   
  
El principal objetivo de este proyecto es la realización de una aplicación de escritorio que implemente el protocolo de control de enlace de datos de alto nivel (HDLC).
El desarrrollo de esta herramienta esta principalmente enfocada en su uso en la asignatura de Redes de Computadores I del grado de Ingeniería Informática con el propósito de ser utilizada con fines didácticos.
Para el desarrollo de este proyecto, se permite total libertad a la hora de elegir el entorno de desarrollo en el que se construirá el proyecto.

Al tratarse de un Trabajo de Fin de Grado, el desarrollo no se basará exclusivamente en la cosntrucción de un producto software, si no que se realizaran exhautivos trabajos de investigación, análisis, diseño, documentación, planificación, etc.

# - Comentarios sobre el entorno de desarrollo

Para ejecutar este programa, se requerirá de una distribución del Sistema Operativo **Windows**.    

Para la elaboración y compilación de este simulador, se ha utilizado el framework **Visual Studio 2019**. 

En el repositorio se adjuntará un fichero ejecutable, de manera que no será necesario disponer de ninguna herramienta para la ejecución del simulador. Simplemente será necerario hacer uso del Sistema Operativo **Windows**. 

# Comentarios sobre el material adjuntado

El material adjuntado para la realización de este proyecto es tan amplia, que se va a clasificar en 2 secciones:

## Material teórico

En esta sección del proyecto, se adjuntarán una serie de documentos con distintos aspectos relevantes para la construcción del simulador del protocolo HDLC. Estos documentos se encuentran dentro de la carpeta de **Documentación** que se encuentra en el repositorio. Esta carpeta contiene los siguientes documentos:

- Un documento llamado ***Memoria TFG.pdf*** en el que se establece una *memoria general* del proyecto desarrollada. Esta memoria incluye:

  - Los objetivos a cumplir en el desarrollo del proyecto
  - Los conceptos teóricos fundamentales del protocolo HDLC
  - Trabajos relacionados
  - Métodologias, técnicas y herramientas utilizadas
  - Aspectos relevantes del desarrollo del proyecto (ciclo de vida)
  - Conclusiones y líneas futuras
    
- Un documento llamado ***Anexo I_ Plan de proyecto software.pdf*** en el que se realiza una *planificación* del proyecto. Esta planificación consta de:

  - Una estimación del esfuerzo del proyecto
  - Una planificación temporal del proyecto junto con un ánalisis de la planificación temporal (Diagrama de Gantt)

- Un documento llamado ***Anexo II.a_ Especificación de requisitos software.pdf*** en el que se realizan tareas relacionadas con la disciplina de *Requisitos*. Estas tareas son:

  - Una elicitación de requisitos la definición formal de los objetivos del sistema y elaboración del diagrama de casos de uso
  - Actividades relacionadas con el Desarrollo Centrado en el Usuario
 
- Un documento llamado ***Anexo II.b_ Análisis de requisitos.pdf*** en el que se realizan tareas relacionadas con la disciplina de *Análisis*. Estas tareas son:  
 
  - Un análisis de los requisitos previamente identificados y elaboración del diagrama de clases del sistema
  - Un análisis de tareas realizadas por el usuario en el futuro sistema
 
- Un documento llamado ***Anexo III_ Especificación del diseño.pdf*** en el que se realizan tareas relacionadas con la disciplina de *Diseño*. Estas tareas son:  

  - Realización del diseño de la estructura del sistema y la relación de los casos de uso previamente identificados a nivel de diseño
  - Realización de actividades de diseño de la interfaz de la aplicación.

- Un documento llamado ***Anexo IV_ Documentación técnica de programación.pdf*** en el que se incluye información relevante sobre la programación del simulador. Esta información es la siguiente:

  - Bibliotecas externas utilizadas
  - Estructura del código fuente de la aplicación
  - Colección de métodos utilizado (incluyendo nombre, parámetros, valores de retorno, clase en la que se encuentran y una breve descripción)

- Un documento llamado ***Anexo V_ Manual de usuario.pdf*** en el que se incluye un manual del usuario donde se explican las distintas funcionalidades que incluye el simulador así como el aspecto visual del simulador en disintos contextos.

## Implementación

En esta sección del proyecto, se adjuntarán una serie de ficheros que forman parte de la implementación del simulador. Estos ficheros se encuentran dentro de la carpeta de **Implementación** que se encuentra en el repositorio. Esta carpeta contiene los siguientes ficheros:

- Una carpeta llamada ***Simulador HDLC - Ejecutable y librerias necesarias*** que contiene el material mínimo para ejecutar el simulador HDLC y hacer uso de todas las funcionalidades. Esta carpeta incluye los siguientes ficheros:

  - Un fichero llamado ***Simulador HDLC.exe** el cual se trata de un fichero ejecutable que contiene la implementación del simulador HDLC.
  - Un fichero llamado ***Simulador HDLC.exe.config** que contiene una serie de configuraciones necesarias para ejecutar correctamente el simulador.
  - Un fichero llamado ***Simulador HDLC.pdb*** que contiene información de depuración sobre la ejeución del simulador.
  - Un fichero llamado ***Newtonsoft.Json.dll*** el cual se trata de una libreria de enlazado dinámico con funciones necesarias para el manejo de objetos JSON, los cuales son utilizados por el simulador.
  - Un fichero llamado ***Newtonsoft.Json.xml*** que contiene información sobre el contenido de la libreria de enlazado dinámico ***Newtonsoft.Json.dll***.
  - Un fichero llamado ***Imagen_Captura_Tráfico.png*** con un ejemplo de captura de tráfico guardado como imagen.
  - Un par de ficheros ***prueba_captura.txt*** y ***prueba_captura.txt (2)*** que contiene información sobre una captura de tráfico desde el punto de vista de las 2 máquinas involucradas en el enlace.
 
- Una carpeta llamada ***Simulador HDLC - Proyecto WPF completo.zip*** que contiene el proyecto WPF completo con la implementación del simulador del protocolo HDLC con todas las clases involucradas.

# - Estructura de la aplicación

Al realizar la implementación del simulador en el entorno de WPF, se van a tener 3 tipos de ficheros:

- Ficheros ***.xaml*** con la representación visual de las distintas ventanas del simulador.
- Ficheros ***.xaml.cs*** con la lógica interna del simulador.
- Ficheros ***.cs*** que actuan de modelos y almacenan los datos del simulador.

Estos ficheros se van a agrupar en distintas clases. El diagrama de clases del proyecto es la siguiente:

![Estructura](https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/0091fe1a-4de7-444b-8897-68196df8ed94)

En este diagrama, existen 3 tipos de clases:

- **Clases del sistema**: Son las que se encuentran en la primera fila
- **Clases ventana**: Incluyen los ficheros *.xaml* y *.xaml.cs*. Se encuentran en la segunda, tercera y cuarta fila.
- **Clases modelo**: Incluyen los ficheros *.cd*. Se encuentran en las últimas 2 filas.

# - Funcionalidades de la aplicación

Todos los datos que genera o utiliza la aplicación se encuentran en un una carpeta llamada *"LigFemBal"* que debe situarse obligatoriamente en el escritorio. En caso contrario, la aplicación no funcionará correctamente.

En el arranque de la aplicación, se comprobará si existen datos previamente guardados dentro de la subcarpeta *"binarios"* dentro de la carpeta *"LigFemBal"*. Si existen datos guardados, se cargaran dichos datos en la aplicación. Logicamente, la primera vez que se ejecute la aplicación, la carpeta *"binarios"* estará vacia.

- **Gestión de la temporada** 
  
  - **Iniciar temporada**: Se introduce el nombre de la temporada de la cual se desean introducir datos.
  - **Cargar jornadas**: Se cargarán los datos relativos a todas las jornadas de la liga. Estos datos se encuentran en un fichero llamado ***datosjornadas.txt*** dentro de la carpeta *"LigFemBal"*. Este fichero se trata de un fichero CSV con el separador "+" para separar los atributos de cada jornada, el separador "#" para separar los distintos partidos de una jornadas y el separador "$" para separar los datos de cada partido de la jornada.
  - **Cargar equipos**: Se cargarán los datos relativos a todos los equipos de la liga. Estos datos se encuentran en un fichero llamado ***datosequipos.txt*** dentro de la carpeta *"LigFemBal"*. Este fichero se trata de un fichero CSV con el separador "#" para separar los distintos atributos de cada equipo.
  - **Cargar jugadoras**: Se cargarán los datos relativos a las jugadoras de cada equipo de la liga. En la subcarpeta ***jugadoras*** dentro de la carpeta *"LigFemBal"* existe un fichero por cada equipo de la liga (con el nombre correspondiente al equipo), donde cada uno de estos ficheros contiene las jugadoras del equipo correspondiente. Estos ficheros se tratan de ficheros CSV con el separador "\t" para separar los distintos atributos de cada jugadora. Es posible que algunos atributos de una jugadora esten vacios. En ese caso, habrá 2 tabuladores juntos.
 
(***Nota***: Cada una de esta funcionalidades solo puede ejecutarse una vez y debe ejecutarse de manera secuencial)
 
- **Gestión de jugadoras**
  - **Modificar datos de una jugadora**: Se pedirá el nombre de la jugadora cuyos datos se desean modificar y posteriormente se introducen los datos deseados. Si la jugadora no existe, se producirá un error y se alertará al usuario de la situación.
  - **Eliminar jugadora de un equipo**: Se pedirá el nombre de la jugadora que se desea eliminar y se elimina la jugadora del equipo al que pertenece. Si la jugadora no existe, se producirá un error y se alertará al usuario de la situación.
  - **Añadir jugadora a un equipo**: Se pedirán los datos de la jugadora que se desea añadir y al equipo al que pertenecerá. Si el equipo no existe, se producirá un error y se alertará al usuario de la situación.
 
- **Gestión de la jornada**
  - **Leer resultados de la jornada**: En la subcarpeta ***resul_jornadas*** dentro de la carpeta *"LigFemBal"* existe un fichero por cada jornada donde cada fichero contiene los resultados de los partidos de dicha jornada. De esta manera, se pedirá el número de la jornada de la cual se desean leer los resultados y la apliación lee los resultados de los partidos de dicha jornada introducida. Al realizar la carga de los resultados de una jornada, tambien se calculará la clasificación de dicha jornada, teniendo en cuenta los datos de jornadas anteriores.
  - **Modificar fecha de la jornada**: Se pedirá el número de la jornada que la cual se desea modificar su fecha y se introduce la nueva fecha.
  - **Modificar fecha u hora de un partido**: Se pedirá el número de la jornada y el nombre de uno de los equipos que participan en el partido del cual se desea modificar su fecha u hora y se introduce la nueva fecha u hora.
  - **Mostrar los resutados de la jornada**: Se pedirá el número de la jornada y se muestra los resultados de los partidos de dicha jornada en un listado.
  - **Mostrar la clasificación de una jornada**: Se pedirá el número de la jornada y se muestra la clasifiación de lo equipos de la liga en dicha jornada en un listado.
 
- **Visualización de resultados**
  - **Jugadoras de un equipo**: Se pedirá el nombre del equipo y se mostrará un listado con las jugadoras que pertenecen a dicho equipo ordenado por posición y altura. Si el equipo no existe, se producirá un error y se alertará al usuario de la situación.
  - **Relación de equipos**: Se mostrará un listado con la información de los distintos equipos de la liga ordenados por el número de telefono.
  - **Relación de jugadoras**: Se pedirá la letra inicial del nombre y se mostrará un listado con las jugadoras cuyo nombre empieza por esa letra ordenadas por fecha de nacimiento.
 
- **Almacenamiento de resultados**
  - **Jugadoras de un equipo**: Se pedirá el nombre del equipo y se generará un fichero encolumnado con el nombre del equipo y extensión .enc, que almacenará la información de las distintias jugadoras que pertenecen a dicho equipo. Si el equipo no existe, se producirá un error y se alertará al usuario de la situación.
  - **Relación de equipos**: Se generará un fichero encolumnado llamado ***equipos.enc***, que almacenará la información de los distintos equipos de la liga (nombre, telefono, web y email).
  - **Clasificación de una jornada**: Se pedirá el número de la jornada y se generará un fichero html que almacenará la clasificación de la liga en la jornada introducida. El nombre de dicho fichero será n.hmtl siendo n el número de la jornada introducida.
 
(***Nota***: Los ficheros generados se almacenan en la subcarpeta ***fichsalida*** dentro de la carpeta *"LigFemBal"*)

- **Salida de la aplicación**  
Cuando el usuario salga de la aplicación, automaticamente se guardaran todos los datos de la aplicación en un fichero con formato binario llamado ***binario.txt*** dentro de la la subcarpeta ***binarios*** dentro de la carpeta *"LigFemBal"*.
  
# - Ejemplo de ejecución

En las siguientes imagenes, se muestra un ejemplo del uso y funcionamiento de la aplicación:    

## Gestión de la temporada

![Ejemplo ejecucion 1](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/22b2a4d7-c394-4383-80e3-736855fbb07f)
![Ejemplo ejecucion 2](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/764891f5-f22d-42d8-8640-6b66cd4edcb9)
![Ejemplo ejecucion 3](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/a8d9f6cc-a0b0-49f5-9409-4061e46b244c)

## Visualización de resultados

### Visualización de las jugadoras de un equipo:

![Ejemplo ejecucion 4](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/64d3164a-e6d9-4517-9b37-b4e8122d94de)

### Visualización de los datos de los equipos de la liga:

![Ejemplo ejecucion 5](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/3402ac2f-e9b7-4478-afb9-b10580194ab3)

### Visualización de las jugadoras cuyo nombre empieza con la incial introducida:

![Ejemplo ejecucion 6](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/faf29d1f-96ca-45e6-bc9b-4ff6963fbd54)

## Gestión de jugadoras

### Modificar y eliminar una jugadora de un equipo:

![Ejemplo ejecucion 7](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/ed772b33-0932-4ddd-8295-6c9aabf460dd)

### Añadir una jugadora de un equipo:

![Ejemplo ejecucion 8](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/71a11903-ac50-432d-bfd1-4f63edfdb23f)

### Representación de los cambios realizados:

![Ejemplo ejecucion 9](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/1e912882-ab18-4db8-9d1f-d0f704fc00d2)

## Gestión de jornada

### Cargar y mostrar los resultados de una jornada:

![Ejemplo ejecucion 10](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/15160db4-bbd7-4887-a141-0cbbeb8b2d32)

### Mostrar la clasificación de una jornada:

![Ejemplo ejecucion 11](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/6e08ee73-ef75-461d-9e11-9d4349f87814)

## Almacenamiento de resultados

### Almacenar la clasificación de una jornada

![Ejemplo ejecucion 12](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/3494c560-a763-4305-b79a-ae554bf05078)
![Ejemplo ejecucion 13](https://github.com/rmelgo/PROG-III-Aplicacion-Liga-Baloncesto/assets/145989723/457a3699-f640-4e27-941b-3078e65f07c3)
