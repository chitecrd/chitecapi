<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;

class Detalle_auditorias_controller extends Controller
{
    public function index(Request $request) 
    {
        $json = new stdClass(); 
        $es_request_valido = validar_request(["DB"], $json, $request);
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB)) 
                //{
                    $detalle_auditorias = DB::select("SELECT * FROM {$DB}.detalle_auditorias");
                    foreach ($detalle_auditorias as $ind => $detalle_auditoria) 
                    {
                        $es_cantidad_igual_tiros = ($detalle_auditoria->cantidad == $detalle_auditoria->no_detalleInv) ? 1 : 0;
                        $detalle_auditorias[$ind]->es_cantidad_igual_tiros = $es_cantidad_igual_tiros;
                    }

                    $json->data = $detalle_auditorias;
                    $json->error = 0;
                    $json->error_type = 0;
                    $json->error_message = 0;    
                //}
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
                $datos_detalle_auditorias = $request;
                foreach ($datos_detalle_auditorias as $datos_detalle_auditoria)
                {
                    $articulo = DB::select("SELECT codigo_barra, alterno1, alterno2, alterno3, descripcion,
                    costo, precio
                    FROM {$DB}.articulos
                    WHERE no_articulo = ? LIMIT 1", [
                        $datos_detalle_auditoria['no_articulo']
                    ]);
                   if (count($articulo) > 0)
                   {
                        $articulo = $articulo[0];

                        $costo_total_detalle = (floatval($articulo->costo) * floatval($datos_detalle_auditoria['cantidad']));
                        $precio_total_detalle = (floatval($articulo->precio) * floatval($datos_detalle_auditoria['cantidad']));
                        DB::insert("INSERT INTO {$DB}.detalle_auditorias (id_terminal, no_detalleInv, no_articulo, descripcion, id_tipo_ubicacion, cod_alterno, pre_conteo, 
                            cantidad_auditada, diferencia, porcentaje_diferencia, id_tipo_error, notas, codigo_barra, id_usuario_registro, secuencia_tiro,
                            alterno1, alterno2, alterno3, cantidad, costo, costo_total, precio, precio_total, id_ubicacion, fecha_registro,
                            fecha_modificacion, id_usuario_modificacion, id_auditor, id_tipo_auditoria, estado, created_at, updated_at)
                            VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,
                            ?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,1,?,?)", [
                            $datos_detalle_auditoria['id_terminal'],
                            $datos_detalle_auditoria['no_detalleInv'],
                            $datos_detalle_auditoria['no_articulo'],
                            $datos_detalle_auditoria['descripcion'],
                            $datos_detalle_auditoria['id_tipo_ubicacion'],
                            $datos_detalle_auditoria['cod_alterno'],
                            $datos_detalle_auditoria['pre_conteo'],
                            $datos_detalle_auditoria['cantidad_auditada'],
                            $datos_detalle_auditoria['diferencia'],
                            $datos_detalle_auditoria['porcentaje_diferencia'],
                            $datos_detalle_auditoria['id_tipo_error'],
                            $datos_detalle_auditoria['notas'],
                            $datos_detalle_auditoria['codigo_barra'],
                            $datos_detalle_auditoria['id_usuario_registro'],
                            $datos_detalle_auditoria['secuencia_tiro'],
                            $articulo->alterno1,
                            $articulo->alterno2,
                            $articulo->alterno3,
                            $datos_detalle_auditoria['cantidad'],
                            $articulo->costo,
                            $costo_total_detalle,
                            $articulo->precio,
                            $precio_total_detalle,
                            $datos_detalle_auditoria['id_ubicacion'],
                            $datos_detalle_auditoria['fecha_registro'],
                            $datos_detalle_auditoria['fecha_modificacion'],
                            $datos_detalle_auditoria['id_usuario_modificacion'],
                            $datos_detalle_auditoria['id_auditor'],
                            $datos_detalle_auditoria['id_tipo_auditoria'],
                            $fecha_hoy,
                            $fecha_hoy
                        ]);
                    }
                    else
                    {
                        $json->error = 1;
                        $json->error_type = 1;
                        $json->error_message = "El artículo con el código: \"{$datos_detalle_auditoria['no_articulo']}\" no existe en la base de datos";

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
                        exit();
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
}