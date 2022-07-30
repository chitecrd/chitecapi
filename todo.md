# Controladores Chitec API

## Recepcion_Orden_Compra

### Tablas

- [ ] recepcion_orden_compra
  - id identity int pk
  - fecha datetime2 not null
  - id_orden_compra varchar(100) fk (PurchaseOrderHeader) not null
  - fecha_registro datetime2 not null d: currentdate
  - id_contenedor varachar(100) null
  - numero_factura varchar(100) null
  - comentario varchar(max) null
  - id_usuario int null fk (usuarios)
- [ ] detalle_recepcion_orden_compra
  - id identity int pk
  - id_recepcion (recepciones_ordenes) fk not null
  - no_articulo varchar(max) not null
  - cantidad float not null
  - id_ubicacion varchar(10) not null fk (ubicaciones)

### Endpoints

- [x] Post
  - [x] Crear
    - json:
      ```json
      [
        {
          "fecha": "2021-09-02",
          "id_orden_compra": "id_orden_compra",
          "id_contenedor": "id_contenedor",
          "numero_factura": "numero_factura",
          "comentario": "comentario",
          "id_usuario": "1",
          "detalles": {
            "no_articulo": "no_articulo",
            "cantidad": "4.0",
            "id_ubicacion": "id_ubicacion"
          }
        }
      ]
      ```
    - Verificar cantidades e ítemes
    - Verificación de cantidad (colocar estados)
      - Cambiar estados en orden de compran
        - faltan: backorder
        - sobra: excedente
        - completo: completada
- [x] Get
  - [x] Buscar  
         (por: numero_orden, numero_recepcion, numero_articulo, ubicacion, rango fecha) dinámico

## Transferencia_Localizacion

### Tablas

- [ ] transferencia_localizacion
  - id int identity pk not null
  - fecha datetime2 not null
  - id_usuario fk (usuarios) null
  - fecha_registro datetime2 not null default:currentdate
- [ ] detalle_transferencia_localizacion
  - id int pk not null identity
  - id_transferencia int not null fk(transferencia_localizacion)
  - ubicacion_inicial varchar(10) not null fk(ubicaciones)
  - ubicacion_final varchar(10) not null fk(ubicaciones)
  - no_articulo varchar(max) not null
  - cantidad float not null

### Endpoints

- [ ] Post
  - [ ] Crear
    - json:
      ```json
      [
        {
          "fecha": "2021-09-01",
          "id_usuario": "5",
          "detalles": {
            "ubicacion_inicial": "ubicacion_inicial",
            "ubicacion_final": "ubicacion_final",
            "no_articulo": "no_articulo",
            "cantidad": "4.0"
          }
        }
      ]
      ```
- [ ] Get
  - [ ] Buscar  
         (por: fecha, no_articulo, ubicacion_inicial, ubicacion_final) dinámico

## Transferencia_Almacen

### Tablas

- [ ] transferencia_almacen
  - id int identity pk not null
  - fecha datetime2 not null
  - id_usuario fk (usuarios) null
  - fecha_registro not null currentdate datetime2
  - status not null varchar(50) defecto pendiente
  - id_almacen_inicial int not null fk (almacen)
  - id_almacen_final int not null fk (almacen)
- [ ] detalle_transferencia_almacen
  - id int pk not null identity
  - id_transferencia_almacen fk int not null
  - no_articulo varchar(max) not null
  - cantidad float not null

### Endpoints

- [ ] Post
  - [ ] Crear
    - json:
      ```json
      [
        {
          "fecha": "2021-09-01",
          "id_usuario": "5",
          "id_almacen_inicial": "1",
          "id_almacen_final": "2",
          "detalles": {
            "no_articulo": "no_articulo",
            "cantidad": "4.0"
          }
        }
      ]
      ```
- [ ] Get
  - [ ] Buscar  
         (por: fecha, no_articulo, id_almacen_inicial, id_almacen_final, status, id_transferencia_almacen)dinámico

## Picking_Transferencia_Almacen

### Tablas

- [ ] picking_transferencia_almacen
  - id identity int pk
  - fecha datetime2 not null
  - id_transferencia_almacen int fk (tranfrencia_almacen) not null
  - fecha_registro datetime2 not null d: currentdate
  - id_usuario int null fk (usuarios)
- [ ] detalle_picking_transferencia_almacen
  - id identity int pk
  - id_picking_transferencia_almacen fk(picking_transferencia_almacen) not null
  - no_articulo varchar(max) not null
  - cantidad float not null
  - id_ubicacion varchar(10) not null fk (ubicaciones)

### Endpoints

- [ ] Post
  - [ ] Crear
    - json:
      ```json
      [
        {
          "fecha": "2021-09-02",
          "id_transferencia_almacen": "1",
          "id_usuario": "1",
          "detalles": {
            "no_articulo": "no_articulo",
            "cantidad": "4.0",
            "id_ubicacion": "id_ubicacion"
          }
        }
      ]
      ```
    - Verificar ítemes
- [ ] Get
  - [ ] Buscar  
         (por id_pedido, fecha, ubicacion, no_articulo) dinámico

## Shipping_Transferencia_Almacen

### Tablas

- [ ] shipping_transferencia_almacen

  - id identity int pk
  - fecha datetime2 not null
  - id_tranferencia_almacen int fk (tranfrencia_almacen) not null
  - fecha_registro datetime2 not null default: currentdate
  - id_usuario int null fk (usuarios)
  - id_contenedor varchar(100) null
  - comentario varchar(max) null

- [ ] detalle_shipping_transferencia_almacen
  - id identity int pk
  - id_shipping_transferencia_almacen int fk(shipping_transferencia_almacen) not null
  - no_articulo varchar(max) not null
  - cantidad float not null

### Endpoints

- [ ] Post
  - [ ] Crear
    - json:
      ```json
      [
        {
          "fecha": "2021-09-02",
          "id_transferencia_almacen": "1",
          "id_usuario": "1",
          "id_contenedor": "id_contenedor",
          "comentario": "comentario",
          "detalles": {
            "no_articulo": "no_articulo",
            "cantidad": "4.0"
          }
        }
      ]
      ```
    - Verificar ítemes
    - Verificación de cantidad (colocar estados)
      - Cambiar estados en orden de transferencia
        - faltan: envio parcial
        - completo: pendiente de recepcion
    - verificacion de la cantidad recogida
- [ ] Get
  - [ ] Buscar  
         (por id_transferencia, fecha, ubicacion, no_articulo) dinámico

## Recepcion_Transferencia_Almacen

### Tablas

- [ ] recepcion_transferencia_almacen
  - id identity int pk
  - fecha datetime2 not null
  - id_transferencia_almacen int fk not null
  - fecha_registro datetime2 not null default:currentdate
  - id_contenedor varachar(100) null
  - comentario varchar(max) null
  - id_usuario int null fk (usuarios)
- [ ] detalle_recepcion_transferencia_almacen
  - id identity int pk
  - id_recepcion_transferencia_almacen fk(recepcion_transferencia_almacen) not null
  - no_articulo varchar(max) not null
  - cantidad float not null
  - id_ubicacion varchar(10) not null fk (ubicaciones)

### Endpoints

- [ ] Post
  - [ ] Crear
    - json:
      ```json
      [
        {
          "fecha": "2021-09-02",
          "id_transferencia_almacen": "1",
          "id_contenedor": "id_contenedor",
          "comentario": "comentario",
          "usuario": "1",
          "detalles": {
            "no_articulo": "no_articulo",
            "cantidad": "4.0",
            "id_ubicacion": "id_ubicacion"
          }
        }
      ]
      ```
    - Verificar cantidades e ítemes
    - verificación de cantidad (colocar estados)
      - Cambiar estados en transferencia de almacen
        - faltan: recepcion parcial
        - completo: completada
- [ ] Get
  - [ ] Buscar  
         (por: id_transferencia_almacen, numero_recepcion, numero_articulo, ubicacion, rango fecha) dinámico

## Cliente

### Tablas

- [ ] cliente
  - id int identity pk not null
  - nombre varchar(100) not null

## Pedido

### Tablas

Guiarse de PurchaseOrder, cambiando cardName y NumAtCard por id_cliente

- [ ] pedido
- [ ] detalle_pedido

### Endpoints

- [ ] Post
  - [ ] Crear (guiarse de PurchaseOrder)
- [ ] Get
  - [ ] Buscar (guiarse de PurchaseOrder)

## Picking_Pedido

### Tablas

- [ ] picking_pedido
  - id identity int pk
  - fecha datetime2 not null
  - id_pedido int fk (pedido) not null
  - fecha_registro datetime2 not null default:currentdate
  - id_usuario int null fk (usuarios)
- [ ] detalle_picking_pedido
  - id identity int pk
  - id_picking_pedido int fk(picking_pedido) not null
  - no_articulo varchar(max) not null
  - cantidad float not null
  - id_ubicacion varchar(10) not null fk (ubicaciones)

### Endpoints

- [ ] Post
  - [ ] Crear
    - json:
      ```json
      [
        {
          "fecha": "2021-09-02",
          "id_pedido": "1",
          "id_usuario": "1",
          "detalles": {
            "no_articulo": "no_articulo",
            "cantidad": "4.0",
            "id_ubicacion": "id_ubicacion"
          }
        }
      ]
      ```
    - Verificar ítemes
- [ ] Get
  - [ ] Buscar  
         (por id_pedido, fecha, ubicacion, no_articulo) dinámico

## Shipping_Pedido

### Tablas

- [ ] shipping_pedido
  - id identity int pk
  - fecha datetime2 not null
  - id_pedido int fk (pedido) not null
  - fecha_registro datetime2 not null default:currentdate
  - id_usuario int null fk (usuarios)
  - id_contenedor varchar(100) null
  - comentario varchar(max) null
- [ ] detalle_shipping_pedido
  - id identity int pk
  - id_shipping_pedido int fk(shipping_pedido) not null
  - no_articulo varchar(max) not null
  - cantidad float not null

### Endpoints

- [ ] Post
  - [ ] Crear
    - json:
      ```json
      [
        {
          "fecha": "2021-09-02",
          "id_pedido": "1",
          "id_usuario": "1",
          "id_contenedor": "id_contenedor",
          "comentario": "comentario",
          "detalles": {
            "no_articulo": "no_articulo",
            "cantidad": "4.0"
          }
        }
      ]
      ```
    - Verificar ítemes
    - Verificación de cantidad (colocar estados)
      - Cambiar estados en orden de transferencia
        - faltan: envio parcial
        - completo: pendiente de recepcion
    - Verificación de la cantidad recogida
- [ ] Get
  - [ ] Buscar  
         (por id_pedido, fecha, ubicacion, no_articulo) dinámico

