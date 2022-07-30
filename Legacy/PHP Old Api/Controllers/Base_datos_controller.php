<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;

class Base_datos_controller extends Controller
{
    public function get_bases_datos(Request $request)
    {
        $json = new stdClass();
        $bases_datos = DB::select("SHOW DATABASES");
        $bases_datos_wmsmobile = [];
        foreach ($bases_datos as $base_datos) 
        {   
            if (strpos($base_datos->Database, "wmsmobile_") !== false) 
            {
                $bases_datos_wmsmobile[] = $base_datos->Database;
            }
        }
        $json->bases_datos_wmsmobile = $bases_datos_wmsmobile;
        $json->error = 0;
        $json->error_type = 0;
        $json->error_message = 0;


        if ($json->error_type != 400)
        {
            return response()->json(
                $json,
                200
            );
        } else
        {
            throw new HttpException(400, $json->error_message);
        }
    }

    public function get_tablas(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request(['DB'], $json, $request);
        if ($es_request_valido)
        {
            $DB = ($request->input('DB') != NULL) ? $request->input('DB') : '';
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos)
            {
                $tablas = DB::select("SELECT table_name FROM information_schema.tables WHERE table_schema = ?", [
                    $DB
                ]);
                $tablas_enviar = [];
                foreach ($tablas as $tabla) 
                {
                    $tabla_enviar = new stdClass();
                    $tablas_enviar[] = $tabla->table_name;
                }
                $json->tablas = $tablas_enviar;
                $json->error = 0;
                $json->error_type = 0;
                $json->error_message = 0;
            }
            else
            {
                $json->error = 1;
                $json->error_type = 400;
                $json->error_message = "La base de datos: \"{$DB}\" no existe";
            }
        }

        if ($json->error_type != 400)
        {
            return response()->json(
                $json,
                200
            );
        } else
        {
            throw new HttpException(400, $json->error_message);
        }
    }

    public function crear(Request $request) 
    {
        //$request = json_decode(file_get_contents('php://input'), true);
        $json = new stdClass();
        if (isset($_GET['nombre_base_datos']))
        {
            $nombre_base_datos = $_GET['nombre_base_datos'];
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $nombre_base_datos
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if (!$existe_base_datos) 
            {
                $nombre_base_datos = mysql_escape($nombre_base_datos);
                DB::connection()->getpdo()->exec("CREATE DATABASE {$nombre_base_datos}");
                DB::connection()->getpdo()->exec("USE {$nombre_base_datos};");
                DB::connection()->getpdo()->exec("CREATE TABLE `articulos` (
                `id` int(11) NOT NULL PRIMARY KEY AUTO_INCREMENT,
                `no_articulo` text COLLATE utf8_unicode_ci NOT NULL,
                `codigo_barra` varchar(30) COLLATE utf8_unicode_ci DEFAULT NULL,
                `alterno1` varchar(20) COLLATE utf8_unicode_ci DEFAULT NULL,
                `alterno2` varchar(20) COLLATE utf8_unicode_ci DEFAULT NULL,
                `alterno3` varchar(20) COLLATE utf8_unicode_ci DEFAULT NULL,
                `descripcion` varchar(200) COLLATE utf8_unicode_ci NOT NULL,
                `existencia` double DEFAULT NULL,
                `unidad_medida` varchar(20) COLLATE utf8_unicode_ci DEFAULT NULL,
                `costo` double DEFAULT NULL,
                `precio` double DEFAULT NULL,
                `referencia` text COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
                `marca` text COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
                `familia` text COLLATE utf8_unicode_ci NOT NULL,
                `created_at` datetime NOT NULL DEFAULT current_timestamp(),
                `updated_at` datetime NOT NULL DEFAULT current_timestamp()
                ) ENGINE=MyISAM DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;");
                
                DB::connection()->getpdo()->exec("CREATE TABLE `detalle_inventario` (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `id_terminal` varchar(150) COLLATE utf8_unicode_ci NOT NULL,
                    `no_detalleInv` varchar(15) COLLATE utf8_unicode_ci NOT NULL,
                    `no_articulo` varchar(15) COLLATE utf8_unicode_ci NOT NULL,
                    `codigo_barra` varchar(30) COLLATE utf8_unicode_ci NOT NULL,
                    `alterno1` varchar(20) COLLATE utf8_unicode_ci NOT NULL,
                    `alterno2` varchar(20) COLLATE utf8_unicode_ci NOT NULL,
                    `alterno3` varchar(20) COLLATE utf8_unicode_ci NOT NULL,
                    `descripcion` varchar(200) COLLATE utf8_unicode_ci NOT NULL,
                    `cantidad` double NOT NULL,
                    `costo` double NOT NULL,
                    `costo_total` double NOT NULL,
                    `precio` double NOT NULL,
                    `precio_total` double NOT NULL,
                    `id_ubicacion` int(11) DEFAULT NULL,
                    `cod_alterno` varchar(10) COLLATE utf8_unicode_ci NOT NULL,
                    `fecha_registro` datetime NOT NULL,
                    `fecha_modificacion` datetime NOT NULL,
                    `id_usuario_registro` int(11) NOT NULL,
                    `id_usuario_modificacion` int(11) NOT NULL,
                    `id_auditor` int(11) NOT NULL DEFAULT 0,
                    `id_tipo_auditoria` int(11) NOT NULL DEFAULT 0,
                    `pre_conteo` double NOT NULL DEFAULT 0,
                    `cantidad_auditada` double NOT NULL DEFAULT 0,
                    `diferencia` double NOT NULL DEFAULT 0,
                    `porcentaje_diferencia` double NOT NULL DEFAULT 0,
                    `id_tipo_error` int(11) NOT NULL DEFAULT 0,
                    `notas` text COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
                    `estado` int(11) NOT NULL,
                    `codigo_capturado` varchar(30) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
                    `created_at` datetime NOT NULL DEFAULT current_timestamp(),
                    `updated_at` datetime NOT NULL DEFAULT current_timestamp(),
                    PRIMARY KEY (`id`),
                    UNIQUE KEY `id_terminal` (`id_terminal`,`no_detalleInv`)
                   ) ENGINE=MyISAM AUTO_INCREMENT=10 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci");

                DB::connection()->getpdo()->exec("CREATE TABLE `tokens` (
                `id` int(11) NOT NULL PRIMARY KEY AUTO_INCREMENT,
                `uuid` varchar(36) NOT NULL,
                `token` varchar(150) NOT NULL,
                `user_id` int(11) NOT NULL,
                `created_at` datetime NOT NULL,
                `updated_at` datetime NOT NULL,
                `state` int(11) NOT NULL
                ) ENGINE=MyISAM DEFAULT CHARSET=latin1;");

                DB::connection()->getpdo()->exec("CREATE TABLE `ubicaciones` (
                `id` int(11) NOT NULL PRIMARY KEY AUTO_INCREMENT,
                `descripcion` varchar(100) COLLATE utf8_unicode_ci DEFAULT NULL,
                `cod_alterno` varchar(10) COLLATE utf8_unicode_ci NOT NULL,
                `id_tipo_ubicacion` int (11) NOT NULL,
                `estado` int(11) NOT NULL,
                `created_at` datetime NOT NULL DEFAULT current_timestamp(),
                `updated_at` datetime NOT NULL DEFAULT current_timestamp()
                ) ENGINE=MyISAM DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;");

                DB::connection()->getpdo()->exec("CREATE TABLE `tipo_ubicacion` (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `descripcion` text NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE `cuenta` (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `maestraitem` int(11) NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE detalle_auditorias (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `id_terminal` varchar(150) NOT NULL,
                    `no_detalleInv` varchar(15) NOT NULL,
                    `no_articulo` varchar(15) NOT NULL,
                    `descripcion` varchar(200) NOT NULL,
                    `id_tipo_ubicacion` int(11) NOT NULL,
                    `cod_alterno` varchar(10) NOT NULL,
                    `pre_conteo` double NOT NULL,
                    `cantidad_auditada` double NOT NULL,
                    `diferencia` double NOT NULL,
                    `porcentaje_diferencia` double NOT NULL,
                    `id_tipo_error` int(11) NOT NULL,
                    `notas` text NOT NULL,
                    `codigo_barra` varchar(30) NOT NULL,
                    `id_usuario_registro` int(11) NOT NULL,
                    `secuencia_tiro` int(11) NOT NULL,
                    `alterno1` varchar(20) NOT NULL,
                    `alterno2` varchar(20) NOT NULL,
                    `alterno3` varchar(20) NOT NULL,
                    `cantidad` double NOT NULL,
                    `costo` double NOT NULL,
                    `costo_total` double NOT NULL,
                    `precio` double NOT NULL,
                    `precio_total` double NOT NULL,
                    `id_ubicacion` int(11) NOT NULL,
                    `fecha_registro` datetime NOT NULL,
                    `fecha_modificacion` datetime NOT NULL,
                    `id_usuario_modificacion` int(11) NOT NULL,
                    `id_auditor` int(11) NOT NULL,
                    `id_tipo_auditoria` int(11) NOT NULL,
                    `estado` int(11) NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`)
                ) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE `tipo_error` (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `descripcion` text NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE `tipo_auditorias` (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `descripcion` text NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE `usuarios_auditoria` (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `nombre` varchar(30) NOT NULL,
                    `apellido` varchar(60) NOT NULL,
                    `usuario` varchar(250) NOT NULL,
                    `password` varchar(100) NOT NULL,
                    `estado` int(11) NOT NULL,
                    `tipo_usuario` int(11) NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE `usuarios` (
                `id` int(11) NOT NULL PRIMARY KEY AUTO_INCREMENT,
                `nombre` varchar(30) COLLATE utf8_unicode_ci NOT NULL,
                `apellido` varchar(60) COLLATE utf8_unicode_ci NOT NULL,
                `usuario` varchar(20) COLLATE utf8_unicode_ci NOT NULL,
                `password` varchar(100) COLLATE utf8_unicode_ci NOT NULL,
                `estado` int(11) NOT NULL,
                `created_at` datetime NOT NULL DEFAULT current_timestamp(),
                `updated_at` datetime NOT NULL DEFAULT current_timestamp()
                ) ENGINE=MyISAM DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;");

                DB::connection()->getpdo()->exec("CREATE TABLE `ayudantes` (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `codigo_unico` varchar(100) NOT NULL,
                    `nombre` varchar(150) NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`),
                    UNIQUE KEY `codigo_unico` (`codigo_unico`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE `asignaciones` (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `codigo_unico` varchar(100) NOT NULL,
                    `cod_alterno` varchar(10) NOT NULL,
                    `descripcion` text NOT NULL,
                    `id_usuario` int(11) NOT NULL,
                    `id_ayudante` int(11) NOT NULL,
                    `nombre_usuario` varchar(150) NOT NULL,
                    `nombre_ayudante` varchar(150) NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`),
                    UNIQUE KEY `codigo_unico` (`codigo_unico`)
                ) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE UNIQUE INDEX id_terminal_no_detalleInv ON detalle_inventario (id_terminal, no_detalleInv);");

                DB::connection()->getpdo()->exec("CREATE TABLE detalle_inventario_ciclico (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `no_articulo` varchar(15) NOT NULL,
                    `codigo_barra` varchar(30) NOT NULL,
                    `alterno1` varchar(20) NOT NULL,
                    `alterno2` varchar(20) NOT NULL,
                    `alterno3` varchar(20) NOT NULL,
                    `descripcion` varchar(200) NOT NULL,
                    `existencia` double NOT NULL,
                    `unidad_medida` text NOT NULL,
                    `costo` double NOT NULL,
                    `precio` double NOT NULL,
                    `referencia` text NOT NULL,
                    `marca` text NOT NULL,
                    `id_familia_productos` int(11) NOT NULL,
                    `id_clasificacion` int(11) NOT NULL,
                    `fecha_asignada_ciclico` date NOT NULL,
                    `estado` int(11) NOT NULL,
                    `no_detalleinv_ciclico` VARCHAR(15) NOT NULL,
                    `id_terminal` VARCHAR(150) NOT NULL,
                    `cantidad` double NOT NULL,
                    `diferencia_valor_absoluto` double NOT NULL,
                    `acierto` int(11) NOT NULL,
                    `falla` int(11) NOT NULL,
                    `id_usuario_registro` int(11) NOT NULL,
                    `fecha_registro` date NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE clasificacion (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `codigo_unico` varchar(100) NOT NULL,
                    `descripcion` text NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`),
                    UNIQUE KEY `codigo_unico` (`codigo_unico`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE familias_productos (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `descripcion` text NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

                DB::connection()->getpdo()->exec("CREATE TABLE `codigos_barras` (
                    `id` int(11) NOT NULL AUTO_INCREMENT,
                    `no_articulo` varchar(150) NOT NULL,
                    `codigo_barra` varchar(150) NOT NULL,
                    `created_at` datetime NOT NULL,
                    `updated_at` datetime NOT NULL,
                    PRIMARY KEY (`id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
                
                $fecha_hoy = date('Y-m-d H:i:s');
                DB::insert("INSERT INTO usuarios (nombre, apellido, usuario, password, estado, created_at, updated_at)
                 VALUES ('admin', 'admin', 'admin', 'admin', 1, ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

                DB::insert("INSERT INTO tipo_ubicacion (descripcion, created_at, updated_at) VALUES ('tienda', ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);
                
                DB::insert("INSERT INTO tipo_ubicacion (descripcion, created_at, updated_at) VALUES ('almacen', ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

                DB::insert("INSERT INTO tipo_auditorias (descripcion, created_at, updated_at) 
                 VALUES ('Cantidad = Tiro', ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

                DB::insert("INSERT INTO tipo_auditorias (descripcion, created_at, updated_at) 
                    VALUES ('Por Monto', ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

               DB::insert("INSERT INTO tipo_auditorias (descripcion, created_at, updated_at) 
                    VALUES ('Duplicado', ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

                DB::insert("INSERT INTO tipo_auditorias (descripcion, created_at, updated_at) 
                    VALUES ('Captura Item', ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

                DB::insert("INSERT INTO usuarios_auditoria (nombre, apellido, usuario, password, estado, tipo_usuario, 
                  created_at, updated_at) VALUES ('au01', 'au01', 'au01', '123', 1, 1, ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

                //Insertamos las clasificaciones
                DB::insert("INSERT INTO clasificacion (codigo_unico, descripcion, created_at, updated_at) VALUES ('0001','A', ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

                DB::insert("INSERT INTO clasificacion (codigo_unico, descripcion, created_at, updated_at) VALUES ('0002','B', ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

                DB::insert("INSERT INTO clasificacion (codigo_unico, descripcion, created_at, updated_at) VALUES ('0003','C', ?, ?)", [
                    $fecha_hoy,
                    $fecha_hoy
                ]);

                $json->error = 0;
                $json->error_type = 0;
                $json->error_message = 0;
            }
            else 
            {
                $json->error = 1;
                $json->error_type = 1;
                $json->error_message = "La base de datos: '{$nombre_base_datos}' ya existe";
            } 
        }
        else
        {
            $json->error = 1;
            $json->error_type = 400;
            $json->error_message = 'Faltan parámetros';
        }

        if ($json->error_type != 400)
        {
            return response()->json(
                $json,
                200
            );
        } else
        {
            throw new HttpException(400, $json->error_message);
        }
    }
}