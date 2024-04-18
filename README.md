# PaymentProcessorApp 🚀

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![.NET Version](https://img.shields.io/badge/.NET-6-blue)
![Docker](https://img.shields.io/badge/docker-supported-blue)

## 📝 Descripción
¡Bienvenido al proyecto PaymentProcessorApp! Este proyecto es mi propuesta de solución para el challenge de la empresa GeoPagos. La solución consta de dos APIs y un conjunto de pruebas (Test). Una de las APIs es completamente pública y tiene Swagger disponible para facilitar su uso. La otra API está protegida con JWT y, por razones de seguridad, no dispone de Swagger en los entornos de Testing y Producción. Todo el proyecto está dockerizado e incluye PostgreSQL como sistema de gestión de base de datos y RabbitMQ como sistema de cola de mensajes.

## 📋 Índice

- [🚀 Cómo Empezar](#cómo-empezar)
- [📊 Contexto/Reglas de Negocio](#contexto-reglas-de-negocio)
- [🛠 Uso](#uso)
- [📬 Contacto](#contacto)

### 📦 Prerrequisitos
- ![.NET](https://img.shields.io/badge/.NET-6-blue)
- ![Docker](https://img.shields.io/badge/docker-supported-blue)
- pgAdmin o DBeaver (para la gestión visual de la base de datos)

## 🚀 Cómo Empezar

Para poner en marcha el proyecto PaymentProcessorApp, sigue estos pasos:
1. **Clona el repositorio**  
   Usa el siguiente comando para clonar el repositorio desde la rama master:
   ```bash
   git clone https://github.com/juanlotito/GeoPagosChallenge.git
2. Luego, pegá el appsettings.json en el directorio de la API pública
    ```bash
    (./PaymentProcessorApp/PublicApi)
3. Desde la consola, dirigite a la carpeta en donde se clonó el proyecto:
    ```bash
    cd PaymentProcessorApp
4. Para ejecutar la solución, realizá un docker compose (no te olvides del --build!)
    ```bash
    docker-compose up -d --build
5. Si el proyecto corrió correctamente, deberías ver 4 elementos si ejecutas el comando *docker ps*
    - 1: RabbitMQ
    - 2: PublicApi
    - 3: PaymentProcessor
    - 4: db

## 📊 Contexto/Reglas de Negocio

Como mencioné anteriormente, la solución incluye una **API pública** accesible libremente. Esta API ofrece el método `/authorize`, que es el principal del proyecto.

### Método `/authorize`
- **Objetivo**: Recibir la solicitud de autorización y responder según la respuesta del procesador de pago.
- **Comportamiento**:
  - **Aprobación**: Si la solicitud es con un número entero.
  - **Rechazo**: Si la solicitud es con un número decimal.

### Procesador de Pago
El procesador de pago es una **API privada** que requiere autorización JWT para ejecutarse. Se necesita usuario y contraseña para acceder.

### Tipos de Cliente
Existen dos tipos de solicitudes de cliente:
- **Primero**: Solicitud de autorización.
- **Segundo**: Solicitud de autorización más su posterior confirmación.

### Manejo de Solicitudes del Segundo Tipo
Para las solicitudes del tipo "Segundo", se implementó un sistema de reversa en caso de que el procesador de pago no responda en 5 minutos. Este proceso se gestiona con RabbitMQ, que:
- Recibe la solicitud de tipo segundo.
- Verifica cada 15 segundos si el procesador de pago aprobó la solicitud.

### Registro Asíncrono
Además, se implementó un registro asíncrono para reportería:
- **Función**: Encolar una tarea cuando el proceso finaliza exitosamente.
- **Almacenamiento**: Guarda los registros exitosos en una tabla específica.

Este esquema asegura que la API pública funcione de manera efectiva y segura, mientras que las operaciones críticas son manejadas de manera confiable y eficiente.

## 🛠 Uso

Para realizar el flujo esperado según el enunciado, seguí estos pasos detallados para autorizarte y hacer solicitudes al sistema:

### Autorización y Autenticación
1. **Inicio de Sesión**
   Utiliza Postman o una herramienta similar para consumir el endpoint POST:
    ```bash
    http://localhost:4000/Auth/login
2. **Copiate el token que te devuelve el procesador**
3. **Vía postman/swagger, enviamos una solicitud a la PublicApi, al endpoint POST**
    ```bash
    http://localhost:5000/payments/authorize
4. **Ese POST recibe por body un JSON asi:**
    ```json
    { 
        "customerId": 1, 
        "amount": 100.00 
    }
5. **Vamos a recibir una respuesta parecida a esta:**
    ```json
    {
      "success": true,
      "message": "Request is being processed.",
      "data": {
        "success": true,
        "paymentRequestId": 54,
        "approved": false
      }
    }
    ```
6. Una vez recibida la respuesta, podemos copiarnos el paymentRequestId e ir a pegarle al endpoint
    ```bash
    GET http://localhost:5000/payments/status/{paymentRequestId}
   ```
   Y recibiremos un JSON como este:
   ```json
   {
      "success": true,
      "message": "Search done correctly",
      "data": {
        "customerId": 2,
        "amount": 10,
        "requestDate": "2024-04-18T12:11:35.601718Z",
        "statusId": 3,
        "paymentTypesId": 0,
        "isConfirmed": false,
        "requiresConfirmation": false
      }
    }
    ```
    Que son básicamente todos los campos de la base de datos. IsConfirmed es el campo que se usa para el procesador de pagos, si el mismo lo aprueba, ese campo será TRUE. StatusId es el campo que nos indica en qué estado esta la solicitud, los status son los siguientes:
     * 1: Approved
     * 2: Denied
     * 3: Pending
     * 4: Reverted
     
    Si vemos IsConfirmed en TRUE y StatusId en 1, significa que la solicitud entró al sistema, se envió de manera asincrónica al procesador de pagos, se aprobó desde el procesador de pagos, se chequeó asíncronamente ese "aprobado", se actualizó el estado y se guardó en la tabla de registros exitosos.



Con eso cubriríamos el flujo principal de este ejercicio, aunque la API tiene más métodos disponibles que no fueron solicitados por el ejercicio:
 - ##### GET `/payments/authorized` : No recibe parámetros y devuelve todos los PaymentRequest con StatusId = 1 (aprobado).
 - ##### POST `/payment/confirm/{PaymentRequestId}`: Recibe por path el PaymentRequestId y genera la confirmación manual.
 - ##### POST `/payment/reverse/{PaymentRequestId}`: Recibe por path el PaymentRequestId y genera la reversa manual del pago.
 
## 📬 Contacto

Con eso culminaríamos, muchas gracias por leer! Te dejo mis redes:

- **Email**: [juanilotito@gmail.com](mailto:juanilotito@gmail.com)
- **GitHub**: [juanlotito](https://github.com/juanlotito)
- **LinkedIn**: [Juan Ignacio Lotito](https://www.linkedin.com/in/juan-ignacio-lotito-601157195/)

Gracias! :)

