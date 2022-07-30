<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;

class Clasificacion_controller extends Controller
{
    public function index(Request $request)
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
                $clasificaciones = DB::select("SELECT * FROM {$DB}.clasificacion");
                $json->clasificaciones = $clasificaciones;
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

    public function get_cantidad_articulos(Request $request)
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

                $cantidades_articulos_clasificacion = DB::select("SELECT d.id_clasificacion, COUNT(1) as nro_articulos, 
                 d.estado
                 FROM {$DB}.detalle_inventario_ciclico d
                 GROUP BY d.id_clasificacion");

                $cantidades_articulos_clasificacion_temp = [];
                foreach ($cantidades_articulos_clasificacion as $cantidad_articulos_clasificacion) 
                {
                    $ind = $cantidad_articulos_clasificacion->id_clasificacion .'_' .$cantidad_articulos_clasificacion->estado;
                    $cantidades_articulos_clasificacion_temp[$ind] = $cantidad_articulos_clasificacion;
                }

                //Hacemos el procedimiento para colocar la cantidades de articulos de cada clasificacion
                $cantidades_articulos_enviar_temp = [];
                $clasificaciones = DB::select("SELECT id, descripcion FROM {$DB}.clasificacion");
                foreach ($clasificaciones as $clasificacion) 
                {
                    //Capturamos las cantidades de los estados 1 y 2
                    for ($i = 1; $i <= 2; $i++) 
                    {
                        $estado = $i;
                        $ind = $clasificacion->id .'_' .$estado;
                        if (!isset($cantidades_articulos_enviar_temp[$clasificacion->id])) 
                        {
                            $cantidades_articulos_enviar_temp[$clasificacion->id] = [
                                'id_clasificacion' => $clasificacion->id,
                                'descripcion' => $clasificacion->descripcion,
                                'nro_articulos' => 0
                            ];
                        }

                        if (isset($cantidades_articulos_clasificacion_temp[$ind])) 
                        {
                            $cantidades_articulos_enviar_temp[$clasificacion->id]["estado_{$estado}"] = $cantidades_articulos_clasificacion_temp[$ind]->nro_articulos;
                            $cantidades_articulos_enviar_temp[$clasificacion->id]["nro_articulos"] += $cantidades_articulos_clasificacion_temp[$ind]->nro_articulos;
                        }
                        else 
                        {
                            $cantidades_articulos_enviar_temp[$clasificacion->id]["estado_{$estado}"] = 0;
                        }
                    }
                }
                $cantidades_articulos_enviar = [];
                foreach ($cantidades_articulos_enviar_temp as $ind => $cantidad_articulos_enviar_temp) 
                {
                    $cantidades_articulos_enviar[] = $cantidad_articulos_enviar_temp;
                }

                $json->cantidades_articulos = $cantidades_articulos_enviar;
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

    public function guardar(Request $request)
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $request = json_decode(file_get_contents('php://input'), true);

        $json = new stdClass();
        if ($request != NULL && isset($_GET['DB']))
        {
            $DB = $_GET['DB'];
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos)
            {
                $datos_clasificaciones = $request;
                foreach ($datos_clasificaciones as $datos_clasificacion)
                {
                    try
                    {
                        $nuevo_autoincrement = DB::select("SELECT COUNT(1) as nuevo_autoincrement FROM {$DB}.clasificacion");
                        $nuevo_autoincrement = (count($nuevo_autoincrement) > 0) ? ($nuevo_autoincrement[0]->nuevo_autoincrement + 1) : 1;
                        DB::connection()->getpdo()->exec("ALTER TABLE {$DB}.clasificacion AUTO_INCREMENT = {$nuevo_autoincrement}");

                        DB::insert("INSERT INTO {$DB}.clasificacion (codigo_unico, descripcion, created_at, updated_at)
                         VALUES (?,?,?,?)", [
                            $datos_clasificacion['codigo_unico'],
                            $datos_clasificacion['descripcion'],
                            $fecha_hoy,
                            $fecha_hoy
                        ]);

                    } catch (\Exception $e)
                    {
                        $es_actualizado = DB::update("UPDATE {$DB}.clasificacion set descripcion = ?, updated_at = ?
                            WHERE codigo_unico = ?", [
                            $datos_clasificacion['descripcion'],
                            $fecha_hoy,
                            $datos_clasificacion['codigo_unico']
                        ]);
                    }
                }

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

    public function editar(Request $request)
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $request = json_decode(file_get_contents('php://input'), true);

        $json = new stdClass();
        if ($request != NULL && isset($_GET['DB']))
        {
            $DB = $_GET['DB'];
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos)
            {
                $datos_clasificaciones = $request;
                foreach ($datos_clasificaciones as $datos_clasificacion)
                {
                    DB::update("UPDATE {$DB}.clasificacion set descripcion = ?, updated_at = ?
                        VALUES codigo_unico = ?", [
                        $datos_clasificacion['descripcion'],
                        $fecha_hoy,
                        $datos_clasificacion['codigo_unico']
                    ]);
                }

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
