<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use App\Detalle_inventario;
use DB;
use PHPExcel;
use PHPExcel_IOFactory;
class Detalle_inventario_ciclico_controller extends Controller
{
    public function index(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request([/*'token',*/ 'DB'], $json, $request);
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
                $detalle_inventario_ciclico = DB::select("SELECT * FROM {$DB}.detalle_inventario_ciclico");
                $json->detalle_inventario_ciclico = $detalle_inventario_ciclico;
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

    public function asignar_fechas(Request $request)
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $request = json_decode(file_get_contents('php://input'), true);

        $json = new stdClass();
        if ($request != NULL && isset($_GET['DB']) && 
         (isset($request['es_asignar_por_clasificacion']) || isset($request['es_asignar_por_familia'])) &&
         isset($request['cantidad_dias_ciclico']) )
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
                $cantidad_dias_ciclico = $request['cantidad_dias_ciclico'];
                $es_asignar_por_clasificacion = ((isset($request['es_asignar_por_clasificacion'])) && $request['es_asignar_por_clasificacion'] == 1);
                $es_asignar_por_familia = ((isset($request['es_asignar_por_familia'])) && $request['es_asignar_por_familia'] == 1);

                $nro_inventarios = DB::select("SELECT COUNT(1) as nro_inventarios FROM {$DB}.detalle_inventario_ciclico
                 WHERE fecha_asignada_ciclico = '0001-01-01'");
                $nro_inventarios = (count($nro_inventarios) > 0) ? $nro_inventarios[0]->nro_inventarios : 0;

                $inventario_por_dia = ($nro_inventarios / $cantidad_dias_ciclico);
                $inventario_por_dia = intval($inventario_por_dia);

                $mensaje_distribucion = '';
                //Verificamos si es asignar por clasificacion
                if ($es_asignar_por_clasificacion) 
                {
                    //Hacemos el procedimiento para asignar las fechas a los inventarios (por clasificacion)
                    $nro_clasificaciones = DB::select("SELECT COUNT(1) as nro_clasificaciones FROM {$DB}.clasificacion");
                    $nro_clasificaciones = (count($nro_clasificaciones) > 0) ? $nro_clasificaciones[0]->nro_clasificaciones : 0;
                    $inventario_por_dia_clasificacion = ($inventario_por_dia / $nro_clasificaciones);
                    $inventario_por_dia_clasificacion = intval($inventario_por_dia_clasificacion);
                    $fecha_asignar = date('Y-m-d');
                    for ($i = 0; $i < $nro_inventarios; $i += $inventario_por_dia_clasificacion) 
                    {
                        $fecha_asignar = date('Y-m-d', strtotime($fecha_asignar .' +1 day'));
                        $clasificaciones = DB::select("SELECT id, descripcion FROM {$DB}.clasificacion");

                        //Dividimos el inventario por dia por las clasificaciones
                        foreach ($clasificaciones as $clasificacion) 
                        {
                            $id_clasificacion = $clasificacion->id;
                            //Hacemos el procedimiento para asignar las fechas de los inventarios con la clasificacion actual
                            $inventarios_clasificacion = DB::select("SELECT id FROM {$DB}.detalle_inventario_ciclico 
                            WHERE id_clasificacion = ? AND fecha_asignada_ciclico = '0001-01-01' 
                            LIMIT 0,{$inventario_por_dia_clasificacion}", [
                                $id_clasificacion  
                            ]);
            
                            $nro_inventarios_asignados = 0;
                            foreach ($inventarios_clasificacion as $inventario_clasificacion) 
                            {
                                //Asignamos la fecha al inventario con la clasificacion actual
                                DB::update("UPDATE {$DB}.detalle_inventario_ciclico set fecha_asignada_ciclico = ? WHERE id = ?", [
                                    $fecha_asignar,
                                    $inventario_clasificacion->id
                                ]);
                                $nro_inventarios_asignados++;
                            }
                            if ($nro_inventarios_asignados > 0) 
                            {
                                $mensaje_distribucion .= "Fecha: {$fecha_asignar}, Clasificación {$clasificacion->descripcion}: {$nro_inventarios_asignados} articulos asignados
";
                            }
                        }
                    }
                }
                else if ($es_asignar_por_familia) 
                {
                    //Hacemos el procedimiento para asignar las fechas a los inventarios (por familia de productos)
                    $nro_familias = DB::select("SELECT COUNT(1) as nro_familias FROM {$DB}.familias_productos");
                    $nro_familias = (count($nro_familias) > 0) ? $nro_familias[0]->nro_familias : 0;
                    $inventario_por_dia_familia = ($inventario_por_dia / $nro_familias);
                    $inventario_por_dia_familia = intval($inventario_por_dia_familia);
                    $fecha_asignar = date('Y-m-d');
                    for ($i = 0; $i < $nro_inventarios; $i += $inventario_por_dia_familia) 
                    {
                        $fecha_asignar = date('Y-m-d', strtotime($fecha_asignar .' +1 day'));
                        $familias_productos = DB::select("SELECT id, descripcion FROM {$DB}.familias_productos");
                        //Dividimos el inventario por dia por las familias de productos
                        foreach ($familias_productos as $familia_producto) 
                        {
                            $id_familia_producto = $familia_producto->id;
                            //Hacemos el procedimiento para asignar las fechas de los inventarios con la familia de productos actual
                            $inventarios_familias_productos = DB::select("SELECT id FROM {$DB}.detalle_inventario_ciclico 
                            WHERE id_familia_productos = ? AND fecha_asignada_ciclico = '0001-01-01' 
                            LIMIT 0,{$inventario_por_dia_familia}", [
                                $id_familia_producto  
                            ]);

                            $nro_inventarios_asignados = 0;
                            foreach ($inventarios_familias_productos as $inventario_familia_productos) 
                            {
                                //Asignamos la fecha al inventario con la familia de productos actual
                                DB::update("UPDATE {$DB}.detalle_inventario_ciclico set fecha_asignada_ciclico = ? WHERE id = ?", [
                                    $fecha_asignar,
                                    $inventario_familia_productos->id
                                ]);
                                $nro_inventarios_asignados++;
                            }
                            if ($nro_inventarios_asignados > 0) 
                            {
                                $mensaje_distribucion .= "Fecha: {$fecha_asignar}, Familia del Producto '{$familia_producto->descripcion}': {$nro_inventarios_asignados} articulos asignados
";
                            }
                        }
                    }
                }

                $json->mensaje_distribucion = $mensaje_distribucion;
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

    public function actualizar_conteo(Request $request)
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
                $datos_detalles_inventarios_ciclico = $request;
                foreach ($datos_detalles_inventarios_ciclico as $datos_detalles_inventario_ciclico)
                {
                    if (!isset($datos_detalles_inventario_ciclico['id']) || !isset($datos_detalles_inventario_ciclico['estado']) 
                     || !isset($datos_detalles_inventario_ciclico['no_detalleinv_ciclico']) 
                     || !isset($datos_detalles_inventario_ciclico['id_terminal']) || !isset($datos_detalles_inventario_ciclico['cantidad'])
                     || !isset($datos_detalles_inventario_ciclico['diferencia_valor_absoluto']) 
                     || !isset($datos_detalles_inventario_ciclico['id_usuario_registro']) 
                     || !isset($datos_detalles_inventario_ciclico['fecha_registro'])
                     || !isset($datos_detalles_inventario_ciclico['porcentaje_tolerancia']))
                    {
                        $json->error = 1;
                        $json->error_type = 400;
                        $json->error_message = "Faltan parámetros (id, estado, no_detalleinv_ciclico, id_terminal,
                         cantidad, diferencia_valor_absoluto, id_usuario_registro, fecha_registro, porcentaje_tolerancia)";
                        return response()->json(
                            $json,
                            200
                        );
                        exit();
                    }
                    $porcentaje_tolerancia = floatval($datos_detalles_inventario_ciclico['porcentaje_tolerancia']);

                    //Hacemos el procedimiento para verificar los aciertos y las fallas
                    $existencia = DB::select("SELECT existencia FROM {$DB}.detalle_inventario_ciclico
                     WHERE id = ? LIMIT 1", [
                        $datos_detalles_inventario_ciclico['id']  
                    ]);
                    $existencia = (count($existencia) > 0) ? $existencia[0]->existencia : 0;
                    $porcentaje_error = ($existencia - $datos_detalles_inventario_ciclico['cantidad']);
                    if ($porcentaje_error > 0) 
                    {
                        $porcentaje_error = (($porcentaje_error / $existencia) * 100);
                    }
                    else 
                    {
                        $porcentaje_error = 0;
                    }

                    $acierto = 0;
                    $falla = 0;
                    //Verificamos si el porcentaje de error es menor al porcentaje de tolerancia
                    if ($porcentaje_error <= $porcentaje_tolerancia) 
                    {
                        $acierto = 1;
                        $falla = 0;
                    }
                    else 
                    {
                        $acierto = 0;
                        $falla = 1;
                    }

                    //Actualizamos
                    DB::update("UPDATE {$DB}.detalle_inventario_ciclico set estado = ?, no_detalleinv_ciclico = ?, id_terminal = ?,
                     cantidad = ?, diferencia_valor_absoluto = ?, id_usuario_registro = ?, fecha_registro = ?, acierto = ?, falla = ?
                     WHERE id = ?", [
                        $datos_detalles_inventario_ciclico['estado'],
                        $datos_detalles_inventario_ciclico['no_detalleinv_ciclico'],
                        $datos_detalles_inventario_ciclico['id_terminal'],
                        $datos_detalles_inventario_ciclico['cantidad'],
                        $datos_detalles_inventario_ciclico['diferencia_valor_absoluto'],
                        $datos_detalles_inventario_ciclico['id_usuario_registro'],
                        $datos_detalles_inventario_ciclico['fecha_registro'],
                        $acierto, 
                        $falla,
                        $datos_detalles_inventario_ciclico['id']
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

    public function importar_excel_detalle_inventario_ciclico(Request $request) 
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $json = new stdClass(); 
        $es_request_valido = validar_request(['DB', 'nombre_excel', 'metodo_importacion'], $json, $request);
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
                $nombre_excel = ($request->input('nombre_excel') != NULL) ? $request->input('nombre_excel') : '';
                $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : '';
                $row = 1;
                DB::raw("USE {$DB}");

                $cmd = "mysqldump -h localhost -u admin -pReemlasr2019** {$DB} > {$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/backups/backup_{$DB}_" .date('Y-m-d_H:i:s') .".sql";
                exec($cmd);

                //$secuencia = 1;
                if ($metodo_importacion == 1) 
                {
                    DB::statement("TRUNCATE {$DB}.detalle_inventario_ciclico");
                }
                else 
                {
                   // $secuencia = DB::select("SELECT (COUNT(1) + 1) as secuencia FROM {$DB}.detalle_inventario_ciclico");
                   // $secuencia = (count($secuencia) > 0) ? $secuencia[0]->secuencia : 1;
                }

                $spreadsheet = PHPExcel_IOFactory::load("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/{$nombre_excel}");
                $sheet = $spreadsheet->getSheet(0);
                $highestRow = $sheet->getHighestRow(); 
                $highestColumn = $sheet->getHighestColumn();
                for ($row = 1; $row <= $highestRow; $row++)
                { 
                    $datos_fila = $sheet->rangeToArray('A' . $row . ':' . $highestColumn . $row,
                                                    NULL,
                                                    TRUE,
                                                    FALSE);
                    if ($row >= 2) 
                    {   
                        $datos_fila = $datos_fila[0];
                        $clasificacion = ($datos_fila[13] != '') ? $datos_fila[13] : '';
                        $id_clasificacion = DB::select("SELECT id FROM {$DB}.clasificacion WHERE descripcion = ? LIMIT 1", [
                            $clasificacion
                        ]);
                        $id_clasificacion = (count($id_clasificacion) > 0) ? $id_clasificacion[0]->id : 0;

                        $familia_productos = ($datos_fila[12] != '') ? $datos_fila[12] : '';
                        $id_familia_productos = 0;
                        $id_familia_productos = DB::select("SELECT id FROM {$DB}.familias_productos WHERE descripcion = ? LIMIT 1", [
                            $familia_productos
                        ]);
                        $id_familia_productos = (count($id_familia_productos) > 0) ? $id_familia_productos[0]->id : 0;
                        if ($id_familia_productos == 0) 
                        {
                            //Si la familia de productos no existe => La insertamos en la base de datos
                            DB::insert("INSERT INTO {$DB}.familias_productos 
                            (descripcion, created_at, updated_at)
                            VALUES (?,?,?)", [
                                $familia_productos,
                                $fecha_hoy,
                                $fecha_hoy
                            ]);
                            $id_familia_productos = DB::getPdo()->lastInsertId();
                        }

                        $valores = [];
                        //$valores[] = str_pad(($secuencia), 4, '0', STR_PAD_LEFT);
                        $valores[] = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                        $valores[] = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                        $valores[] = ($datos_fila[2] != '') ? $datos_fila[2] : '';
                        $valores[] = ($datos_fila[3] != '') ? $datos_fila[3] : '';
                        $valores[] = ($datos_fila[4] != '') ? $datos_fila[4] : '';
                        $valores[] = ($datos_fila[5] != '') ? $datos_fila[5] : '';
                        $valores[] = ($datos_fila[6] != '') ? $datos_fila[6] : 0;
                        $valores[] = ($datos_fila[7] != '') ? $datos_fila[7] : '';
                        $valores[] = ($datos_fila[8] != '') ? $datos_fila[8] : 0;
                        $valores[] = ($datos_fila[9] != '') ? $datos_fila[9] : 0;
                        $valores[] = ($datos_fila[10] != '') ? $datos_fila[10] : '';
                        $valores[] = ($datos_fila[11] != '') ? $datos_fila[11] : '';
                        $valores[] = $id_familia_productos;
                        $valores[] = $id_clasificacion;
                        $valores[] = $fecha_hoy;
                        $valores[] = $fecha_hoy;

                        DB::insert("INSERT INTO {$DB}.detalle_inventario_ciclico 
                         (no_articulo, codigo_barra, alterno1, alterno2, alterno3, descripcion, existencia,
                          unidad_medida, costo, precio, referencia, marca, id_familia_productos, id_clasificacion,
                          fecha_asignada_ciclico, estado, no_detalleinv_ciclico, id_terminal, cantidad,
                          diferencia_valor_absoluto, acierto, falla, id_usuario_registro, fecha_registro,
                          created_at, updated_at)
                         VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,
                          '0001-01-01', 1, '', '', 0, 0, 0, 0, 0, '0001-01-01', ?, ?)", $valores);
                        //$secuencia++;
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

    public function get_porcentaje_precision(Request $request)
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
                $valores = [];
                $condicion_fecha = "";
                if ($request->exists('fecha')) 
                {
                    $fecha = ($request->input('fecha') != NULL) ? $request->input('fecha') : '';
                    $condicion_fecha = "AND fecha_asignada_ciclico = ?";
                    $valores[] = $fecha;
                }
           
                //Hacemos el procedimiento para capturar el porcentaje de precision
                $nro_aciertos = DB::select("SELECT SUM(acierto) as nro_aciertos FROM {$DB}.detalle_inventario_ciclico
                 WHERE 1 = 1 {$condicion_fecha}", $valores);
                $nro_aciertos = (count($nro_aciertos) > 0) ? $nro_aciertos[0]->nro_aciertos : 0;

                $nro_inventarios = DB::select("SELECT COUNT(1) as nro_inventarios FROM {$DB}.detalle_inventario_ciclico
                 WHERE 1 = 1 {$condicion_fecha}", $valores);
                $nro_inventarios = (count($nro_inventarios) > 0) ? $nro_inventarios[0]->nro_inventarios : 0;

                $porcentaje_precision = 0;
                if ($nro_inventarios > 0) 
                {
                    $porcentaje_precision = (($nro_aciertos / $nro_inventarios) * 100);
                }
                $porcentaje_precision = number_format($porcentaje_precision, 2);
                $porcentaje_precision = floatval($porcentaje_precision);
       
                $json->porcentaje_precision = $porcentaje_precision;                
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
}
