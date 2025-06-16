# 🏦 Microservicio Bancario - Sistema Distribuido con .NET 9, Kafka y Docker

Este repositorio contiene una arquitectura basada en microservicios para simular un sistema bancario distribuido. Incluye validación antifraude y persistencia de transacciones mediante eventos con Kafka.

---

## 📦 Estructura del Proyecto

- **Transaction.API**: expone el endpoint para crear transacciones.
- **AntiFraud.Worker**: consume eventos para validación antifraude.
- **Transaction.Worker**: escucha eventos de estado y actualiza las transacciones.
- **Transaction.Domain**: entidades y contratos de dominio.
- **Transaction.Infrastructure**: EF Core, contexto y repositorios.
- **Shared.Infrastructure**: configuración de Kafka y Polly.
- **Contracts**: eventos compartidos.

---

## 🚀 Tecnologías

- .NET 9
- Docker & Docker Compose
- Apache Kafka & Zookeeper (Bitnami)
- SQL Server 2022
- Kafka UI
- Polly (resiliencia)
- EF Core

---
## 🐳 Cómo ejecutar

```bash
docker-compose build
docker-compose up
```

> ⚠ Asegúrate de que el puerto 1433 (SQL Server) y 8080 (Kafka UI) estén libres en tu host.

---

## 🧪 Pruebas de API

**1. Crear una transacción**

Puedes usar Postman, Thunder Client o curl para probar:

**📥 Endpoint**
```
POST http://localhost:5001/api/transactions
```

**🧾 JSON de ejemplo**
```json
{
  "sourceAccountId": "11111111-1121-1111-1111-111111111111",
  "targetAccountId": "22222222-2222-2222-2222-222222222222",
  "transferTypeId": 1,
  "value": 500
}
```

**🔄 Resultado esperado**
- Se guarda la transacción con estado `Pending`.
- Se publica el evento `transaction-created` a Kafka.
- El microservicio antifraude evalúa la transacción.
- Se responde con `transaction-status`.
- El `Transaction.Worker` actualiza el estado final.

**✔ Verifica**
- En Kafka UI (`http://localhost:8080`) los tópicos `transaction-created` y `transaction-status`.
- En la base de datos `TransactionDb`, la tabla `Transactions` debe reflejar el nuevo estado.

---

## 📂 Accesos útiles

- **Kafka UI**: http://localhost:8080
- **SQL Server**: conectar con `localhost,1433` usuario `sa`, contraseña `YourStrong@Passw0rd`
---

## ✍ Autor

Desarrollado por [Juan Felipe Osorio Uribe]

---
