# Aplicación para la simulación del protocolo HDLC

![Inicial](https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/cbbe1d24-2982-4f5b-b33f-d2c34d43b0cc)

# - Introducción

Proyecto realizado en la asignatura de Trabajo de Fin de Grado del grado de Ingenieria Informática de la Universidad de Salamanca. El enunciado del proyecto se encuentra subido en el repositorio en un archivo pdf llamado <a href="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/blob/main/Propuesta%20TFG.pdf" target="_blank">*Propuesta TFG.pdf*</a>. 
  
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

# - Funcionalidades del simulador

## Pantalla principal

Al abrir el fichero ***Simulador HDLC.exe***, inicialmente se presentará una ventana similar a la que se presenta en la siguiente imagen:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/fabc26a8-64c1-4b69-ae1d-633d069cdada">
</p>

La ventana principal se agrupa en 3 secciones principales:

- Una sección que contiene **información básica** sobre la estación. En concreto se muestra la siguiente información:
  
  - Nombre de la estación
  - Modo de funcionamiento (Semiautomático o manual)
  - Número de secuencia (VS)
  - Número de trama esperada (VR)
    
- Una sección que contiene 2 tablas en las que se **representa información sobre las tramas enviadas y recibidas por la estación**. En concreto, se muestra la siguiente información sobre cada trama:
  
  - Instante en el que se envío/recibío la trama en la estación
  - Tipo de trama
  - Dirección a la que va dirigida la trama (si es un comando) o dirección de origen de la trama (si es una respuesta)
  - Número de secuencia de la trama (NS)
  - Estado del bit de poll (P/F)
  - Número de trama esperada (NR)

- Una sección en la que se **representa graficamente información sobre las tramas enviadas y recibidas por la estación**. En concreto, se muestra la siguiente información sobre cada trama:

  - Dirección a la que va dirigida la trama (si es un comando) o dirección de origen de la trama (si es una respuesta)
  - Tipo de trama
  - Estado del bit de poll (P/F)
  - Número de secuencia de la trama (NS)
  - Número de trama esperada (NR)

## Configuración

Cada ventana de simulador corresponde a una estación. Cada estación tiene asociada una **configuración**. 

Dentro de la **configuración de la estación**, existen 3 secciones:

- Protocolo
- Modo de trabajo
- Canal

Para acceder a la configuración, se debe hacer click a un botón con una rueda dentada que se encuentra en la esquina superior derecha. 

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/359b8ee3-b0a5-45a1-b85d-754d9155c8dd">
</p>

### Configuración del protocolo de la estación

Al acceder a la configuración de la estación, se desplegará una ventana en la que se mostrará por defecto la sección de la configuración del protocolo de la estación. Esta sección tiene el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/c280d274-4db5-4a59-95bb-1b8a8806b6cf">
</p>

Dentro de la sección de la configuración del protocolo de la estación, se identifican 2 subsecciones:

- Una sección destinada a la configuración de ***timeouts***. En esta sección, se pueden configurar 3 tipos de timeouts:
  
  - **Timeout ante Command**: Este timeout hace referencia al tiempo máximo que una estación espera una respuesta cuando esta envía un comando con el bit de sondeo activado. 
  - **Timeout ante trama I**: Este timeout hace referencia al tiempo máximo que una estación espera reconocimiento de una trama de información anteriormente enviada. 
  - **Timeout ante Request**: Este timeout hace referencia al tiempo máximo que una estación espera la recepción de un comando tras realizar el envío de una respuesta.

**Nota**: Para obtener mas información del funcionamiento o significado de los timeouts, se recomienda utilizar el manual de usuario del simulador o utilizar los botones de ayuda con el símbolo "?".

- Una sección destinada a la configuración del ***control de flujo***. Esta sección cuenta con 2 párametros:

  - **Tamaño de la ventana**: Este parámetro hace referencia al número máximo de tramas que en un determinado momento pueden estar pendientes de confirmación. 
  - **Número de tramas erróneas consecutivas**: Este parámetro hace referencia al número máximo de tramas erróneas consecutivas permitidas que una estación puede enviar antes de desconectar el enlace físico con la otra estación conectada. 

**Nota**: Para obtener mas información del funcionamiento o significado de estos parámetros, se recomienda utilizar el manual de usuario del simulador o utilizar los botones de ayuda con el símbolo "?".

### Configuración del modo de trabajo de la estación

La sección de la configuración del modo de trabajo de la estación tiene el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/8aab81bd-0ad8-4afa-8079-b4d49d402a9c">
</p>

La estación tiene 2 modos de trabajo en los que la estación puede funcionar:

  - **Modo semiautomático**: La estación responde de manera automática en algunas situaciones. En el manual de usuario y en los sistemas de ayuda se detallan en que situciones se producen respuestas automáticas. También se aplicarán los timeouts configurados en la sección de protocolo y se permitira el envío de tramas erróneas.
  - **Modo manual**: La estación no responde de manera automática en ninguna situación. Tampoco se aplicarán los timeouts configurados en la sección de protocolo y ni se permitira el envío de tramas erróneas.

### Configuración del canal de transmisión de la estación

La sección de la configuración del canal de transmisión de la estación tiene el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/bd9ca946-11ad-4382-8db1-c64249ad2dbd">
</p>

Dentro de la sección de la configuración del canal de transmisión de la estación, se identifican 2 parámetros fundamentales:

  - **Retardo**: El retardo hace referencia al tiempo transcurrido desde que se produce el envío de la trama desde la estación de origen hasta que se produce la recepción de la trama en la estación de destino. 
  - **Modo manual**: La tasa de error hace referencia a la probabilidad de que una trama enviada sea recibida de manera errónea debido a una alteración en el contenido de la trama.

**Nota**: Para obtener mas información del funcionamiento o significado de estos parámetros, se recomienda utilizar el manual de usuario del simulador o utilizar los botones de ayuda con el símbolo "?".

### Guardar una configuración

Para guardar la configuración de la estación, se debe pulsar el botón de "*Aceptar*" y si se quieren desechar los cambios realizados sobre la configuración se debe pulsar el botón de "*Cancelar*".

Si en alguno de los parámetros de configuración, se ha introducido un valor sintácticamente erróneo, el recuadro de dicho parámetro se marcará en rojo como se muestra en la siguiente imagen:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/530ba847-0732-44bd-8044-22229bbae6b5">
</p>

Si se intenta guardar una configuración con algún valor sintácticamente erróneo, se cancelará el guardado de dicha configuración y se mostrará la siguiente ventana:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/ae4a2864-f3d7-4e9c-b333-41f20f754cf7">
</p>

## Establecimiento de la conexión

El principal objetivo del simulador es establecer una conexión física entre 2 estaciones de forma que estas estaciones puedan intercambiar tramas siguiendo el protocolo HDLC. 

De esta manera, para establecer una conexión física deben ejecutarse 2 estaciones y se deben seguir los siguientes pasos:

- **Paso 1**: Configurar los nombres de las estaciones que se van a conectar fisicamente de manera que las estaciones que se vayan a conectar tengan un nombre de estación distinto.

  Si las 2 estaciones que se van a conectar tienen el mismo nombre se producirá un error y se desplegará una ventana como la que se muestra en la siguiente imagen:

  <p align="center">
    <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/d35f1758-176a-414e-8072-72b8b30791e7">
  </p>

  Para configurar el nombre de la estación, se utilizará el recuadro situado en la esquina superior izquierda de la ventana principal.

  <p align="center">
    <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/a3eae5f8-1194-4a25-b82f-7ce0374ed600">
  </p>

- **Paso 2**: Pulsar el botón de "*Inicializar*" situado en la parte inferior de la ventana principal en la primera estación. Al realizar esta acción, se desplegará la siguiente ventana:

  <p align="center">
    <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/df572b81-b1e0-4fee-ace6-a8ce76fd8faf">
  </p>

  En esta ventana, se indica que la estación esta buscando otra estación con la cual realizar el establecimiento de la conexión física.

- **Paso 3**: Pulsar el botón de "*Inicializar*" situado en la parte inferior de la ventana principal en la segunda estación. Al realizar esta acción, se habrá realizado la conexión física de ambas estaciones correctamente (si los nombres de las estaciones no coinciden) y se desplegará la siguiente ventana:

  <p align="center">
    <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/653972b6-9a79-4eb1-a168-8086f3a1f67f">
  </p>

## Envío de tramas

Una vez se ha establecido una conexión física entre 2 estaciones, el siguiente paso es intercambiar tramas entre las 2 estacionnes. Para ello, se deben seguir los siguientes pasos:

- **Paso 1**: Elegir el tipo de trama que se desea enviar.

  En la parte superior de la ventana principal existe una serie de botones con los diferentes tipos de trama que pueden enviarse. Estos botones solo se activan cuando se ha establecido una conexión física con otra estación.

  <p align="center">
    <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/01d90398-6055-46e1-b3df-4e7d16c81733">
  </p>

- **Paso 2**: Pulsar el botón relacionado con el tipo de trama que se desea enviar.

  Para enviar una trama, el usuario simplemente tiene que pulsar el botón correspondiente al tipo de trama que desee enviar. Al realizar esto, se presentará una ventana en la que se mostrará información básica de la trama que se desea enviar y que tiene el siguiente     aspecto:

  <p align="center">
    <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/f59741b5-4c12-4555-906f-4d6b78328552">
  </p>

  Si la trama que se desea enviar es una trama de información, la ventana en la que se mostrará información básica de la trama que se desea enviar contará con 2 elementos adicionales y tendrá el siguiente aspecto:

  <p align="center">
    <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/5985451a-4e19-4197-86ec-f454625bb077">
  </p>

- **Paso 3**: Configurar los parámetros de la trama que se desea enviar. En concreto, se podrá modificar la siguiente información de la trama:
  
  - Información sobre el bit P/F. 
  - Información sobre el número de secuencia (NS) en tramas que cuenten con número de secuencia. 
  - Información sobre el número de trama esperada (NR) en tramas que cuenten con número de trama esperada.
 
  Si la trama a enviar se trata de una trama de información, se podran configurar adicionalmente:

  - La información que se envía en la trama.
  - La posibilidad de enviar una trama errónea.
 
- **Paso 4**: Enviar la trama. Para ello, se deberá pulsar el botón “*Enviar*” para enviar la trama a la estación situada en el otro extremo. 
 
## Representación de las tramas intercambiadas

Una vez se han intercambiado una serie de tramas entre las 2 estaciones fisicamente conectadas, se representará en la ventana de la estación correspondiente la información de las tramas intercambiadas.

### Sección de tablas

Cada vez que una estación **envía** una trama, se actualiza la ***tabla de las tramas enviadas*** con la información de la trama enviada.  
En la siguiente imagen, se muestra el contenido de la tabla de las tramas enviadas tras enviarse una trama:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/8f53c67b-7b15-43c9-b71a-50ba33ea62dd">
</p>

Cada vez que una estación **recibe** una trama, se actualiza la ***tabla de las tramas recibidas*** con la información de la trama recibida.  
En la siguiente imagen, se muestra el contenido de la tabla de las tramas recibidas tras recibirse una trama:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/e5a3bd6c-d987-4655-ab71-4914ea98121e">
</p>

**Nota**: Las tramas enviadas se representan de color negro y las tramas recibidas se representan de color azul.

En las **tablas**, se muestra la siguiente información sobre las tramas:

- Instante temporal en el que se ha enviado la trama. 
- Tipo de trama enviada. 
- Dirección de la estación a la que va dirigida la trama o que responde en el caso de que la trama sea una respuesta. 
- Número de secuencia (NS) de la trama enviada. 
- Estado del bit de sondeo. 
- Número de trama esperada (NR) por la estación. 
  
### Sección gráfica

Cada vez que una estación **envía** o **recibe** una trama, se actualizará la sección gráfica de la ventana de la estación con información de la trama enviada/recibida.  

En la siguiente imagen, se muestra el contenido de la sección gráfica tras realizarse el intercambio de una trama desde el punto de vista de las 2 estaciones:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/17fab50b-1bfd-4fe8-8a57-50cc779bcd5f">
</p>

**Nota**: Las tramas enviadas se representan de color azul y apuntan hacia la derecha mientras que las tramas recibidas se representan de color verde y apuntan a la izquierda.

En la **sección gráfica**, se muestra la siguiente información sobre las tramas:

- Dirección de la estación a la que va dirigida la trama o que responde en el caso de que la trama sea una respuesta. 
- Tipo de trama enviada. 
- Estado del bit de sondeo (solo se muestra si está activo). 
- Número de secuencia de la trama enviada (solo en tramas que tienen número de secuencia). 
- Número de trama esperada por la estación situada en el otro extremo (solo en tramas que tienen número de trama esperada).

## Visualización del detalle de una trama 

Es posible ver el detalle de la composición de cada una de las tramas intercambiadas por una estación. Para acceder a la composición detallada de una trama se tienen 2 vías:

- **Vía 1**: A través de la ***tabla de tramas enviadas/recibidas de la estación***, haciendo click en la trama correspondiente.
- **Vía 2**: A través de la ***sección gráfica de la estación***, haciendo click en el sobre asociado a la trama correspondiente.

Al acceder a la composición detallada de una trama, se desplegará una nueva ventana en el que se mostrará la información detallada de la trama seleccionada. Existen 3 tipos de tramas en función de su composición:

- Tramas no numerada
- Tramas de información
- Tramas de supervisión

De esta manera, en función del tipo de trama, se desplegará una ventana diferente para representar la composición detallada de la trama. 

### Visualización del detalle de una trama no numerada

La composición detallada de una trama no numerada, se mostrará en una ventana con el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/5b5cfd7d-87ae-4c59-bde1-a38f1c83ff92">
</p>

En esta ventana, se muesta la siguiente información:

- **Tipo de trama**
- **Información básica sobre la trama**
  
  - El bit C/R 
  - El número de secuencia (NS) 
  - El bit P/F 
  - El número de trama esperada (NR)
    
- **Información sobre los campos genéricos de la trama**
  
  - Flag inicial 
  - Campo de dirección 
  - Campo de control 
  - Campo de información 
  - Código de redundancia cíclica (CRC) 
  - Flag final
 
- **Información sobre el campo de control de la trama**

  - Primer bit con valor 1 por defecto 
  - Segundo bit con valor 1 por defecto 
  - Tipo de trama no numerada (bits 3, 4, 6, 7, 8) 
  - Estado del bit P/F (bit 5) 

### Visualización del detalle de una trama de información

La composición detallada de una trama de información, se mostrará en una ventana con el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/4639a29d-161d-458e-893d-bdb248d66a70">
</p>

En esta ventana, se muesta la siguiente información:

- **Tipo de trama**
- **Información básica sobre la trama**
  
  - El bit C/R 
  - El número de secuencia (NS) 
  - El bit P/F 
  - El número de trama esperada (NR)
    
- **Información sobre los campos genéricos de la trama**
  
  - Flag inicial 
  - Campo de dirección 
  - Campo de control 
  - Campo de información 
  - Código de redundancia cíclica (CRC) 
  - Flag final
 
- **Información sobre el campo de control de la trama**

  - Primer bit con valor 0 por defecto 
  - Número de secuencia (bits 2, 3, 4) 
  - Estado del bit P/F (bit 5) 
  - Número de trama esperada (bits 6, 7, 8)
 
### Visualización del detalle de una trama de supervisión

La composición detallada de una trama de supervisión, se mostrará en una ventana con el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/fc0d296d-e021-4ffd-81f9-a942eee110ee">
</p>

En esta ventana, se muesta la siguiente información:

- **Tipo de trama**
- **Información básica sobre la trama**
  
  - El bit C/R 
  - El número de secuencia (NS) 
  - El bit P/F 
  - El número de trama esperada (NR)
    
- **Información sobre los campos genéricos de la trama**
  
  - Flag inicial 
  - Campo de dirección 
  - Campo de control 
  - Campo de información 
  - Código de redundancia cíclica (CRC) 
  - Flag final
 
- **Información sobre el campo de control de la trama**

  - Primer bit con valor 1 por defecto 
  - Segundo bit con valor 0 por defecto 
  - Tipo de trama de supervisión (bits 3, 4) 
  - Estado del bit P/F (bit 5) 
  - Número de trama esperada (bits 6, 7, 8)

## Fin del establecimiento de la conexión

Una vez se ha realizado el intercambio correspondiente de tramas entre 2 estaciones fisicamente conectadas y se desea poner fin a dicha conexión física, se deberá pulsar el botón de "*Finalizar*" situado en la parte inferior de la ventana principal. 
Al realizar esta acción, se desplegará la siguiente ventana:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/fdf76a4f-ac0a-496a-a5b6-fe487b8fdba1">
</p>

## Guardar captura de tráfico de las tramas intercambiadas

Al realizar un intercambio de tramas entre 2 estaciones fisicamente conectadas, es posible guardar la información de dicho intercambio de tramas (*captura de tráfico*) en un fichero para posteriormente ser cargado y utilizado de nuevo. 

Para ello, se debe hacer click a un botón con un disquete que se encuentra en la esquina superior derecha.

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/3da64864-4a84-4942-9ce9-d4d8d28b7513">
</p>

Al pulsar el botón, se desplegará una ventana de exploración en la que se podrá editar el nombre de la captura de tráfico que se desea guardar así como su ubicación. Esta ventana tiene el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/add121cd-51a4-4283-aadf-4b77a0a070a9">
</p>

Una vez se ha establecido el nombre de la captura de tráfico así como la ubicación en la que se desea guardar la captura, se deberá pulsar el botón de "*Guardar*" situado en la parte inferior de la ventana. 
Al realizar esta acción, se desplegará la siguiente ventana:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/f8cb56e8-59cf-4d1d-abf1-3ba5a0c3ff88">
</p>

Al realizar el guardado de una captura de tráfico, se generarán 2 ficheros:

- Un fichero llamado ***nombre-captura.txt*** que contiene la información del intercambio de tramas desde la perspectiva de la estación desde la cual se solicito el guardado de la captura de tráfico.
- Un fichero llamado ***nombre-captura.txt (2)*** que contiene la información del intercambio de tramas desde la perspectiva de la estación contraria situada al otro extremo de la conexión.

Estos 2 ficheros son necesarios e indispensables para posteriormente poder cargar la captura de tráfico guardada. Debido a esto, si posteriormente se desea cargar la captura de tráfico, es imprescindible realizar el guardado de la captura de tráfico mientras las estaciones se encuentran **fisicamente conectadas**. 

Es posible guardar la captura de tráfico después de finalizar la conexión física. Sin embargo, en este caso solo se generará el primer fichero y por tanto, no se podrá realizar posteriormente el cargado de dicha captura de tráfico.

## Cargar captura de tráfico de las tramas intercambiadas

En el simulador, es posible cargar una *captura de tráfico* con información del intercambio de tramas entre 2 estaciones, previamente guardado a través de la aplicación.

Para ello, se debe hacer click a un botón con un fichero que se encuentra en la esquina superior derecha.

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/dd6d88fa-5545-49de-8469-cddcd9c4c417">
</p>

Al pulsar el botón, se desplegará una ventana de exploración en la que se podrá navegar y seleccionar la captura de tráfico que se desea cargar en el simulador. Esta ventana tiene el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/0483034b-cbb2-4ab5-ae8f-236425e45ca0">
</p>

Una vez se ha seleccionado la captura de tráfico que se desea cargar, se deberá pulsar el botón de "*Abrir*" situado en la parte inferior de la ventana. 
Al hacer esto, el simulador automaticamente representará la información de las tramas que formaban parte de la captura de tráfico.

**Restricciones en la carga de una captura de tráfico**

- A diferencia del guardado, para cargar una captura de tráfico es imprescindible que la estación en la que se realice el cargado de la captura de tráfico se encuentre ***fisicamente conectada*** con otra estación. Si no se cumple esta restricción, al realizar el cargado de la captura de tráfico se mostrará la siguiente ventana:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/1d43bd4e-3df6-42e2-93bb-db440ce9166c">
</p>

- También es imprescindible que el nombre de la estación que realizo previamente el guardado de la captura de tráfico debe ***coincidir*** con el nombre de la estación que realiza la carga de la captura de tráfico. Si no se cumple esta restricción, al realizar el cargado de la captura de tráfico se mostrará la siguiente ventana:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/f8b8d26f-e279-4afd-8cc0-4ea88093abdb">
</p>

## Envío y representación de tramas erróneas

En el simulador del protocolo HDLC, es posible enviar **tramas de información erróneas** y configurar el canal de comunicación que tenga una determinada tasa de error, tal como se ha visto anteriormente.

Para enviar una **trama de información errónea**, se debe activar el *CheckBox* de trama errónea en la ventana de configuración de la trama de información a enviar. Esto se muestra en la siguiente figura:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/961fdb58-2cb7-4a93-bab8-a2691e6e006a">
</p>

Cuando se produce el envío de una trama errónea, la representación de dicha trama será la que se muestra en la siguiente ventana:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/4997398e-6c58-445d-abaf-7df9617b602b">
</p>

Por un lado, en la ***tabla de tramas enviadas***, la trama errónea se respresenta de color rojo. Por otro lado, en la ***sección gráfica*** se representa la trama errónea enviada con una flecha de color rojo.

A diferencia del resto de tramas, tanto en las tablas de tramas enviadas/recibidas como en la sección gráfica solo se representa la siguiente información sobre las tramas erróneas:

- Tipo de trama
- Instante temporal en el que se ha producido el envío/recepción (solo en las tablas)

## Mecanismos de ayuda

El simulador, al tratar un tema complejo como es el protocolo HDLC, cuenta con una gran infraestructura de sistemas de ayuda que permitan al usuario comprender el uso de la herramienta y del protocolo HDLC.

Estos mecanismos de ayuda consisten en **botones azules** con el símbolo ***?***. Al pulsar sobre un botón de ayuda, se desplegará una **bocadillo** con una explicación de la sección, parámetro o funcionalidad al que pertenezca dicha ayuda. 

En la siguiente imagen, se muestra un ejemplo de ayuda:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/d9472c8a-ce8d-429b-9360-35e79a958fcc">
</p>

Para ocultar el bocadillo de ayuda, bastará con hacer click de nuevo en el botón de ayuda o en cualquier parte de la ventana fuera del bocadillo de ayuda.

## Funcionalidades adicionales

El simulador cuenta con una serie de funcionalidades adicionales, las cuales, a pesar de tener menos importancia, pueden tener un gran impacto y ser de gran ayuda para el usuario.

### Envío directo de tramas

El usuario puede enviar cualquier trama a la estación situada en el otro extremo de la conexión de **manera directa**, evitando la configuración previa de la trama. Para ello, se debe hacer ***click derecho*** sobre el botón de la trama que desea enviar.

De esta manera, se enviará directamente la trama elegida con una configuración automática por lo que el usuario no tendrá que configurarla y no se desplegará la ventana de configuración de la trama a enviar.

### Símbolos para los timeouts

Si la estación se encuentra en el modo de trabajo semiautomático, en la sección de **información básica** de la estación, se muestran con **círculos** de distintos colores los timeouts que estan activados en la estación en cada momento.

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/b1d8bee0-1413-49b3-92f1-dac91fe69b4b">
</p>

El significado de cada círculo es el siguiente:

- El **círculo azul** representa la activación del ***timeout ante COMMAND***. 
- El **círculo verde** representa la activación del ***timeout ante trama I***. 
- El **círculo rojo** representa la activación del ***timeout ante REQUEST***. 

### Símbolo para el modo de trabajo de la estación

En la sección de **información básica** de la estación, junto a los símbolos de los timeouts, se muestra a través de un símbolo el modo de trabajo de la estación.

Si la estacíon se encuentra en modo ***semiautomático***, se mostrará el siguiente símbolo:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/5aa3f211-eed6-4297-adfb-9796aaf666a0">
</p>

Si la estacíon se encuentra en modo ***manual***, se mostrará el siguiente símbolo:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/ed8d6c77-a104-4f09-b213-dd90645c8595">
</p>

### Guardar imagen del tráfico intercambiado entre estaciones

El usuario tiene la posibilidad de guardar la información de un intercambio de tramas entre 2 estaciones fisicamente conectadas en formato de **imagen**. 

Para ello, se debe pulsar una botón con una cámara situado en la esquina superior derecha de la sección gráfica de la ventana de la estación.

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/e4ca15e2-f73a-4f3f-8f53-e59aaf0d4f67">
</p>

Al pulsar el botón, se desplegará una ventana de exploración en la que se podrá editar el nombre la imagen de la captura de tráfico que se desea guardar así como su ubicación. Esta ventana tiene el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/720e0b4d-5cf6-465a-aa02-1e2b5f0c541c">
</p>

Una vez se ha establecido el nombre de la captura de tráfico así como la ubicación en la que se desea guardar la captura, se deberá pulsar el botón de "*Guardar*" situado en la parte inferior de la ventana. 

La imagen con el contenido de la captura de tráfico, tendrá el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/d1fa7eab-0bb2-4a93-b9c7-5b5998045266">
</p>

Como se puede observar, la imagen de la captura de tráfico es similar al contenido de la sección gráfica de la estación en el momento de realizar el guardado de la captura de tráfico.

### Situación de la estación (significado)

La situación de la estación hace referencia al "estado" en el que se encuentra la estación. Esta información se muestra en un recuadro en la parte inferior de la ventana de la estación.

Existen 5 tipos de situaciones para la estación:

- **Desconectado**: La estación no está conectada por lo que no puede intercambiar tramas de información con otra estación. Sin embargo, sí existe una conexión física entre ambas estaciones.
- **Conectado**: La estación está conectada por lo que puede intercambiar tramas de información con otra estación.  
- **Inicio conexión**: Se ha iniciado el proceso de establecimiento de conexión entre las tramas. Se ha enviado/recibido la solicitud de conexión (SABM) pero no se ha respondido a dicha solicitud de conexión. 
- **Inicio desconexión**: Se ha iniciado el proceso de establecimiento de desconexión entre las tramas. Se ha enviado/recibido la solicitud de desconexión (DISC) pero no se ha respondido a dicha solicitud de desconexión. 
- **Excepción**: Se ha producido un error irrecuperable en la transmisión y es necesario un restablecimiento del enlace a través de una trama FRMR.

### Modos de vista de la estación

La ventana de la estación cuenta con mucha información y abarca mucho espacio en la pantalla, por lo que se han diseñado 2 tipos de modos de vista para la estación, de forma que el usuario pueda elegir dicho modo de vista.

Existen 2 modos de vista:

- **Modo de vista gráfico**: Este modo incluye la sección gráfica donde se representa gráficamente las tramas intercambiadas por la estación. Este es el modo utilizado por defecto.
- **Modo de vista lectura**: Este modo oculta la sección gráfica y aumenta el tamaño de las tablas de tramas enviadas y recibidas.

Para cambiar de un modo de vista a otro, se debe pulsar una botón con forma de gráfico o libro (dependiendo del modo en el que se encuentre la estación) situado en la esquina inferior izquierda:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/754aad5e-109e-454c-9a87-fc73fa1322e4">
</p>

La ventana de la estación en el modo de vista gráfico tiene el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/aaa1b807-1b73-4e26-b956-4d912961e28b">
</p>

La ventana de la estación en el modo de vista lectura tiene el siguiente aspecto:

<p align="center">
  <img src="https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/e8e000f2-aaeb-40f3-b9a8-6f194f97c8c6">
</p>

