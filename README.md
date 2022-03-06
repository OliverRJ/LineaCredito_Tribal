# LineaCredito_Tribal
Ejemplo de Servicio para calcular la línea de crédito de clientes.

## Acerca del Proyecto
El proyecto es WebApi hecho con Net Core y que incluye lo siguiente:
- Implementa CORS para limitar el llamado malicioso de orígenes desconocidos.
- JWT para que generación de token de seguridad.
- Incluye swagger como herramienta de documentación.
- Incluye Ratelimit para el control de peticiones(se deshabilitó para usar otra lógica)
- Incluye pruebas unitarias con Xunit.

## ¿Cómo usar el proyecto?

Se debe usar el Ide de Visual Studio para ejecutar las fuentes y Postman para la prueba de los servicios.

1. Ejecutar localmente el proyecto con nombre TribalCreditoWebApi en visual studio.
2. Ingresar a postman y ejecutar el servicio que genera el token a usar para el llamado de los demás métodos:
- Método get: http://localhost:2971/loginuser/requesttoken
- Copiar el token generado.
![Image text](https://github.com/OliverRJ/LineaCredito_Tribal/blob/main/guia_token.jpg)
3. Abrir otro request en postman, pero esta vez del tipo post, el cual debe usar "Autorización" del tipo *Bearer Token* y el Body del tipo *Raw - JSON*
![Image text](https://github.com/OliverRJ/LineaCredito_Tribal/blob/main/guia_token2.jpg)
- Método POST: http://localhost:2971/api/Credit/SolicitarCredito
- Ejemplo Body: 
```json
{
    "FoundingType": "Startup",
    "CashBalance" : 120,
    "MontlyRevenue" : 1000,
    "RequestCreditLine": 200
}
    
