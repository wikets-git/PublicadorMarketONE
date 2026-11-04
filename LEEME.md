# Publicador MarketONE v1.00

Aplicación VB.NET (WinForms, .NET Framework 4.7.2, Visual Studio 2022) que **envía al SII, de forma automática, los documentos electrónicos que MarketONE deja pendientes de envío**.

Tiene el formato y el diseño de Publicador Supermercado (repositorio `PublicadorSupermercado`). Usa el modelo de datos de MarketONE y su módulo DTE (repositorio `MarketONE`): el mismo `MarketONEDataContext` y el mismo `DTE.vb`. Los documentos se leen y se envían como en el módulo **Control DTE Emitidos** de MarketONE.

## Qué hace

- **Es automático: no tiene menú ni login.** Al abrir muestra el módulo **Publicador DTE** y cuenta **5, 4, 3, 2, 1** (en *Restante*, en *Procesando* y en la barra de estado). Luego inicia con el usuario **SYS** y parte el primer ciclo.
- **Ejecutar** procesa de inmediato si no hay un proceso en curso. Si está en la cuenta regresiva o en un reintento, inicia el sistema en ese momento.
- **Detener** detiene el envío sin salir del programa:
  - si hay un proceso en curso, termina el documento que se está enviando;
  - si está esperando el próximo ciclo, en la cuenta regresiva o en un reintento, lo cancela.
  
  El envío queda detenido, sin ciclo automático, hasta presionar **Ejecutar**.
- **Cerrar la ventana** (X, o *Salir* en el menú de la bandeja) pregunta *"¿Desea cerrar el Publicador [Empresa]?"*. Si se confirma, detiene el proceso y sale. Si hay un ciclo en curso, termina el documento que se está enviando y se cierra solo.
- **Envío automático, sin filtros.** Toma todos los locales y todos los tipos de documento electrónicos, desde el primer día del mes anterior hasta el fin del mes en curso. En cada ciclo:
  1. lista los pendientes de envío de `DocumentosG`;
  2. los timbra con `DTE.GenerarTED`, que los deja en `STATUS_DTE = 3`;
  3. los envía con `DTE.EmitirDTE`, que usa `EmitirBoleta` o `EmitirDocumentoExportacion` según el tipo;
  4. deja los estados de MarketONE, igual que Control DTE Emitidos.
- **No hay traspaso de POS.** Los POS de MarketONE escriben directo en `DocumentosG` con `STATUS_DTE` pendiente. Tampoco hay conversiones de local ni de número de caja; eso es solo de Supermercado.
- **Registros por lote** (cuadro *Registros*, junto al intervalo). El valor inicial es `RegistrosPorCiclo`. El intervalo y los registros se pueden cambiar en pantalla y rigen desde el ciclo siguiente.
  - **0**: en cada ciclo envía todo lo pendiente.
  - **N** (por ejemplo, 10): en cada ciclo lee hasta N pendientes, los envía y espera el intervalo. Lo que falte queda para los ciclos siguientes.
  - En cada lote van primero los documentos nuevos (`STATUS_DTE = 0`) y al final los que ya se intentaron (`STATUS_DTE = 3`). Así, los que fallan siempre no ocupan todo el lote.
- **Bandeja de notificaciones.** El programa deja un ícono en la bandeja de Windows.
  - Al minimizar, la ventana se oculta y queda solo el ícono. Doble clic (o *Abrir* en el menú del botón derecho) la vuelve a mostrar.
  - El menú también tiene *Ejecutar ahora* y *Salir*.
  - Si se abre el `.exe` otra vez mientras ya corre para la misma base, se muestra la ventana del que está corriendo.
- **Alerta de errores.** Si un ciclo termina con errores, el ícono se pone rojo, parpadea mientras la ventana está oculta y aparece el aviso *"Publicador [Empresa]: Errores en envío de datos"*. El ícono vuelve a la normalidad cuando un ciclo termina sin errores.
- **Aviso de envío detenido.** Con *Detener*, el ícono muestra un círculo ámbar y el texto *"Publicador [Empresa]: Envío detenido"*. Con la ventana minimizada aparece un aviso, que se repite cada 30 minutos. Los errores tienen prioridad: si hay errores, el ícono se ve rojo.
- **Nunca muestra ventanas de error.** Es un proceso automático:
  - el motivo de cada documento con problema queda en la columna **Observación**;
  - los errores generales quedan en la barra de operación;
  - todo queda en los **logs**.
  
  `MsgError`, `Mensaje` e `Informacion` de MarketONE no abren ventanas en este programa. La única ventana es la confirmación al cerrarlo.
- **Si no puede iniciar** (falta configuración, la base no responde), muestra el motivo y **reintenta cada 1 minuto**.
- **Una sola instancia por base de datos** (`Servidor` + `Base` de la configuración) en todo el equipo, aunque se abra desde otra carpeta o con otro usuario de Windows. Así no se envía dos veces el mismo documento.
- En la tabla `Auditoria` (estructura de MarketONE) queda el ingreso y la salida (`Login`, usuario `SYS`), junto con cada ciclo de envío.

## Qué documentos envía

Solo los **electrónicos pendientes de envío**. Un documento se envía si cumple las cuatro condiciones:

| Condición | Detalle |
|---|---|
| Tipo de documento | FV, BV, BE, FE, NC, ND, GD, FX, NX, DX (mismo orden de Control DTE Emitidos), y que en `TipoDoc` tenga `Electronica = 1` y `Cod_SII > 0`. |
| Local | `Locales.FElectronica = 1` y el local emite ese tipo electrónico (`FV_Elect`, `BV_Elect`, `GD_Elect`, etc.). |
| `STATUS_DTE` | **0** (No enviado) o **3** (Timbrado: tiene TED pero el envío no se aprobó, por ejemplo por un corte de conexión). |
| Fecha | Desde el primer día del mes anterior hasta el fin del mes en curso. |

No se envían automáticamente los estados 1 (Aprobado), 2 (Rechazado), 5 (Error de timbre), 8 (Envío anulado) ni los demás. Esos se revisan en MarketONE, en Control DTE Emitidos.

Antes de enviar, se vuelve a leer el documento en la base: si otro proceso ya lo envió, se salta. También se revisa lo que el módulo DTE rechaza sin mensaje, para dejar el motivo en la observación: total 0, sin detalle o cliente inexistente.

Las Facturas de Exportación en dólares usan el valor del dólar de la CMF, que se consulta al inicio de cada ciclo (Control DTE Emitidos lo consulta al abrir). Si no se puede obtener, esos documentos quedan para el ciclo siguiente.

## Requisitos

- Visual Studio 2022 (17.14) con el desarrollo de escritorio .NET, y .NET Framework 4.7.2.
- **No necesita Crystal Reports**: no imprime ni genera PDF.
- Acceso a la base MarketONE de la empresa y al servicio DTEBox de cada local (`Locales.IP_DTE` / `Llave`).
- Acceso a Internet (api.cmfchile.cl) para el valor del dólar, solo si hay exportaciones en USD.
- Librerías (carpeta `Lib`, las mismas de MarketONE): DTEBOXClienteNET20, DTEDocumentoNET20, DTETimbreNET20, DTELibroCVNET20, DTELibroGDNET20, BOLDocumentoNET20, BOLFoliosNET20, BOLLibroNET20, LiquidTechnologies.Runtime.Net20, BouncyCastle.Crypto, idautomation.pdf417, Newtonsoft.Json, WinDeco y Msg.

## Compilar

Abrir `PublicadorMarketONE.sln` (proyecto `PublicadorMarketONE\PublicadorMarketONE.vbproj`) y compilar. El ejecutable se llama **`PublicadorMarketONE.exe`** y el espacio de nombres es `PublicadorMarketONE`.

## Archivos que vienen de MarketONE

Si cambian en MarketONE, se deben copiar de nuevo aquí:

| Archivo aquí | Origen en MarketONE | Cambios |
|---|---|---|
| `Formularios\DTE\DTE.vb` | `Formularios\DTE\DTE.vb` | Solo los `Imports` (se quitan Crystal, OpenXml, Excel y WPF; `MarketONE.Funciones` → `PublicadorMarketONE.Funciones`), en `EmitirBoleta` el error al actualizar el estado también queda en `MensajeDTE`, y `Parametros_DTE` solo crea la carpeta TED (no CAF, PDF ni TXT, que el Publicador no usa). |
| `Datos\MarketONE.dbml`, `Datos\MarketONE1.designer.vb` | `MarketONE.dbml`, `MarketONE1.designer.vb` | Sin cambios. Del `.dbml` se quitó la cadena de conexión de desarrollo (con usuario y clave). |
| `Objetos\Validacion.vb` | `Objetos\Validacion.vb` | Se quita un `Imports` de OpenXml. |
| `Datos\ConstantesMarketONE.vb` | Estructuras de `Funciones.vb` (`TiposDoc`, `EstadosSII`, `FormasPago`, `MotivosDocumento`, etc.) | Sin cambios. |
| `Objetos\ClienteREST.vb` | `ObtenerDolar` de `Objetos\ClienteREST.vb` y `Objetos\Dolar.vb` | Sin cambios. |

`Funciones.PrimeraLinea` y la auditoría (`IngresarAuditoria`) también son las de MarketONE.

## Configuración (`PublicadorMarketONE_Settings.json`, junto al .exe)

Usa el **formato simple de Publicador Supermercado**: conexión directa a la base, sin `ServerConfig` ni `ServidoresEmpresas` de MarketONE. Se puede partir de `PublicadorMarketONE_Settings.ejemplo.json`. Si el archivo no existe, el programa lo crea vacío, avisa y reintenta cada minuto.

| Clave | Uso |
|---|---|
| `Empresa` | Nombre de la empresa (título, bandeja y `status.json`). |
| `Servidor`, `Base` | Conexión SQL a la base MarketONE de la empresa. El usuario y la clave **no van en el archivo**: están fijos en el código (`Objetos\Configuracion.vb`), los mismos de MarketONE. Si un archivo antiguo trae `UsuarioBD` o `ClaveBD`, se ignoran. |
| `TiempoConexion` | Segundos de espera para conectar. |
| `RutaArchivos` | Carpeta base donde se crea la subcarpeta TED (vacío = carpeta del programa). |
| `MinutosEnvioAutomatico` | Minutos entre ciclos (por defecto 5). |
| `RegistrosPorCiclo` | Documentos por lote (0 = todos de una vez). Se puede cambiar en pantalla, en el cuadro *Registros*. |
| `LocalPublicador` | Local con que trabaja el usuario SYS. Con 0 se usa el primer local con facturación electrónica. Se procesan los documentos de todos los locales, cada uno con sus propios parámetros DTE. |

El archivo ya no contiene credenciales. Igual queda en `.gitignore`, porque cada instalación tiene su propio servidor y base.

## Archivo `status.json` (monitoreo)

Junto al `.exe` el Publicador escribe `status.json`, para que otro programa avise por correo si hay errores o si dejó de publicar:

```json
{
  "Empresa": "Nombre Empresa",
  "Publicacion": "2026-11-03 11:50:00",
  "Status": "Normal"
}
```

- **Empresa:** valor de `Empresa` en la configuración (vacío si no está configurada).
- **Publicacion:** fecha y hora (`aaaa-mm-dd hh:mm:ss`) de la última publicación terminada, sea por el ciclo automático o por el botón *Ejecutar*.
- **Status:**
  - `Normal` si el ciclo terminó sin errores.
  - `Error` si al menos un documento quedó con error o el ciclo falló (consulta, conexión).
  - Si el programa no puede iniciar, también se escribe `Error` en cada reintento (cada 1 minuto).
- Se reescribe al terminar cada ciclo. Si el ciclo se detiene con *Detener*, no se actualiza: la fecha deja de avanzar y el monitor puede detectar que no está publicando.
- Se escribe en un archivo temporal y luego se reemplaza, para que el monitor nunca lea un archivo a medio escribir. Codificación UTF-8 sin BOM.

## Logs

En la carpeta `Logs`, junto al programa:

- `Logs\DTE\Log_ErrorEnvioDTE_aaaammdd.txt`: cada documento que quedó con error y el motivo.
- `Logs\DTE\Log_RepuestaEnvioDTE_…` y `Log_GenerarTimbreDTE_…`: respuestas de DTEBox y timbres, los mismos que escribe MarketONE.
- `Logs\ErrorMsg\`: errores generales (lo que en MarketONE sería una ventana de error).
- `Logs\LogEx_…`: excepciones.

## Varias empresas en el mismo equipo

Pueden correr varios Publicadores a la vez, uno por empresa. Cada uno va en su propia carpeta, con su `PublicadorMarketONE_Settings.json` (otro `Servidor`/`Base` y otra `Empresa`). Cada copia tiene su ícono en la bandeja (el texto muestra la empresa), sus propios `Logs` y su carpeta TED.

Dos copias que apunten a la **misma** base no pueden correr juntas: la segunda muestra la ventana de la que ya corre y se cierra.

## Puesta en marcha

- Para que parta con Windows, deje un acceso directo a `PublicadorMarketONE.exe` en la carpeta Inicio del usuario del servidor, o cree una tarea programada "Al iniciar sesión".
- No ejecute al mismo tiempo el *Envío Automático* de Control DTE Emitidos de MarketONE contra la misma base.
