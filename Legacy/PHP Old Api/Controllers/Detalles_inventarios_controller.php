<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use App\Detalle_inventario;
use DB;
use PHPExcel;
use PHPExcel_IOFactory;
use PHPExcel_Shared_Date;
use DateTime;
class Detalles_inventarios_controller extends Controller
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB))
                //{
                    $valores = [];
                    $condicion_cantidad_igual_tiros = "";
                    if ($request->exists('es_cantidad_igual_tiros') && $request->input('es_cantidad_igual_tiros') == 1)
                    {
                        $condicion_cantidad_igual_tiros = "AND d.no_detalleInv = d.cantidad";
                    }

                    $condicion_cantidad_igual_no_articulo = "";
                    if ($request->exists('es_cantidad_igual_no_articulo') && $request->input('es_cantidad_igual_no_articulo') == 1)
                    {
                        $condicion_cantidad_igual_no_articulo = "AND d.no_articulo = d.cantidad";
                    }

                    $condicion_precio_total = "";
                    if ($request->exists('precio_total_desde') && $request->exists('precio_total_hasta'))
                    {
                        $precio_total_desde = ($request->input('precio_total_desde') != NULL) ? $request->input('precio_total_desde') : 0;
                        $precio_total_hasta = ($request->input('precio_total_hasta') != NULL) ? $request->input('precio_total_hasta') : 0;
                        $condicion_precio_total = "AND d.precio_total BETWEEN ? AND ?";
                        $valores[] = $precio_total_desde;
                        $valores[] = $precio_total_hasta;
                    } else if ($request->exists('precio_total_desde'))
                    {
                        $precio_total_desde = ($request->input('precio_total_desde') != NULL) ? $request->input('precio_total_desde') : 0;
                        $condicion_precio_total = "AND d.precio_total >= ?";
                        $valores[] = $precio_total_desde;
                    } else if ($request->exists('precio_total_hasta'))
                    {
                        $precio_total_hasta = ($request->input('precio_total_hasta') != NULL) ? $request->input('precio_total_hasta') : 0;
                        $condicion_precio_total = "AND d.precio_total <= ?";
                        $valores[] = $precio_total_hasta;
                    }

                    $condicion_costo_total = "";
                    if ($request->exists('costo_total_desde') && $request->exists('costo_total_hasta'))
                    {
                        $costo_total_desde = ($request->input('costo_total_desde') != NULL) ? $request->input('costo_total_desde') : 0;
                        $costo_total_hasta = ($request->input('costo_total_hasta') != NULL) ? $request->input('costo_total_hasta') : 0;
                        $condicion_costo_total = "AND d.costo_total BETWEEN ? AND ?";
                        $valores[] = $costo_total_desde;
                        $valores[] = $costo_total_hasta;
                    } else if ($request->exists('costo_total_desde'))
                    {
                        $costo_total_desde = ($request->input('costo_total_desde') != NULL) ? $request->input('costo_total_desde') : 0;
                        $condicion_costo_total = "AND d.costo_total >= ?";
                        $valores[] = $costo_total_desde;
                    } else if ($request->exists('costo_total_hasta'))
                    {
                        $costo_total_hasta = ($request->input('costo_total_hasta') != NULL) ? $request->input('costo_total_hasta') : 0;
                        $condicion_costo_total = "AND d.costo_total <= ?";
                        $valores[] = $costo_total_hasta;
                    }

                    $condicion_fecha = "";
                    if ($request->exists('fecha_desde') && $request->exists('fecha_hasta'))
                    {
                        $fecha_desde = ($request->input('fecha_desde') != NULL) ? $request->input('fecha_desde') : '';
                        $fecha_hasta = ($request->input('fecha_hasta') != NULL) ? $request->input('fecha_hasta') : '';
                        $condicion_fecha = "AND DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:%i') BETWEEN ? AND ?";
                        $valores[] = $fecha_desde;
                        $valores[] = $fecha_hasta;
                    } else if ($request->exists('fecha_desde'))
                    {
                        $fecha_desde = ($request->input('fecha_desde') != NULL) ? $request->input('fecha_desde') : '';
                        $condicion_fecha = "AND DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:%i') >= ?";
                        $valores[] = $fecha_desde;
                    } else if ($request->exists('fecha_hasta'))
                    {
                        $fecha_hasta = ($request->input('fecha_hasta') != NULL) ? $request->input('fecha_hasta') : '';
                        $condicion_fecha = "AND DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:%i') <= ?";
                        $valores[] = $fecha_hasta;
                    }

                    $condicion_id_tipo_ubicacion = "";
                    if ($request->exists('id_tipo_ubicacion'))
                    {
                        $id_tipo_ubicacion = ($request->input('id_tipo_ubicacion') != NULL) ? $request->input('id_tipo_ubicacion') : 0;
                        $condicion_id_tipo_ubicacion = "AND u.id_tipo_ubicacion = ?";
                        $valores[] = $id_tipo_ubicacion;
                    }
                    $detalles_inventarios = DB::select("SELECT d.*, 0 as es_cantidad_igual_tiros,
                      ifnull(tu.descripcion, '') as tipo_ubicacion
                      FROM {$DB}.detalle_inventario d
                      left join {$DB}.ubicaciones u
                        on (d.id_ubicacion = u.id)
                      left join {$DB}.tipo_ubicacion tu
                       on (u.id_tipo_ubicacion = tu.id)
                      WHERE 1 = 1
                     {$condicion_cantidad_igual_tiros} {$condicion_cantidad_igual_no_articulo}
                     {$condicion_precio_total} {$condicion_costo_total}
                     {$condicion_fecha} {$condicion_id_tipo_ubicacion}", $valores);

                    //Para capturar el registro siguiente y el registro anterior (si cantidad es igual a tiros)
                    if ($request->exists('es_cantidad_igual_tiros') && $request->input('es_cantidad_igual_tiros') == 1)
                    {
                        $registros_agregar = [];
                        foreach ($detalles_inventarios as $ind => $detalle_inventario)
                        {
                            $registros_laterales = DB::select("SELECT d.*, 0 as es_cantidad_igual_tiros,
                             ifnull(tu.descripcion, '') as tipo_ubicacion
                             FROM {$DB}.detalle_inventario d
                             left join {$DB}.ubicaciones u
                                on (d.id_ubicacion = u.id)
                             left join {$DB}.tipo_ubicacion tu
                                on (u.id_tipo_ubicacion = tu.id)
                             WHERE d.id = ? OR d.id = ? LIMIT 2", [
                                ($detalle_inventario->id - 1),
                                ($detalle_inventario->id + 1)
                            ]);
                            if (count($registros_laterales) > 0) {
                                if (count($registros_laterales) == 1) {
                                    $registros_agregar[$ind]['siguiente'] = $registros_laterales[0];
                                } else {
                                    $registros_agregar[$ind]['anterior'] = $registros_laterales[0];
                                }
                            }
                            if (count($registros_laterales) > 1) {
                                $registros_agregar[$ind]['siguiente'] = $registros_laterales[1];
                            }
                        }

                        $detalles_inventarios_temp = [];
                        foreach ($detalles_inventarios as $ind => $detalle_inventario)
                        {
                            if(isset($registros_agregar[$ind]) && isset($registros_agregar[$ind]['anterior']))
                            {
                                $detalles_inventarios_temp[]=$registros_agregar[$ind]['anterior'];
                            }

                            if((isset($registros_agregar[$ind]) && isset($registros_agregar[$ind]['anterior']))
                             || (isset($registros_agregar[$ind]) && isset($registros_agregar[$ind]['siguiente'])))
                            {
                                $detalle_inventario->es_cantidad_igual_tiros = 1;
                            }

                            $detalles_inventarios_temp[]=$detalle_inventario;
                            if(isset($registros_agregar[$ind]) && isset($registros_agregar[$ind]['siguiente']))
                            {
                                $detalles_inventarios_temp[]=$registros_agregar[$ind]['siguiente'];
                            }
                        }
                        $detalles_inventarios = $detalles_inventarios_temp;
                    }

                    $json->data = $detalles_inventarios;
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

    public function get_almacen_tienda(Request $request)
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
                $detalles_inventarios = DB::select("SELECT d.*
                 FROM {$DB}.detalle_inventario d
                 inner join {$DB}.ubicaciones u
                  on (d.id_ubicacion = u.id)
                 WHERE u.id_tipo_ubicacion = 2
                 AND NOT EXISTS (SELECT 1
                 FROM {$DB}.detalle_inventario d2
                  inner join {$DB}.ubicaciones u2
                   on (d2.id_ubicacion = u2.id)
                 WHERE d2.no_articulo = d.no_articulo
                 AND u2.id_tipo_ubicacion = 1)
                 GROUP BY d.no_articulo");
               $cantidad_articulos_no_existen_tienda = count($detalles_inventarios);
               $costo_total_no_existe_tienda = 0;
               $precio_total_no_existe_tienda = 0;
               foreach ($detalles_inventarios as $detalle_inventario)
               {
                    $costo_total_no_existe_tienda += $detalle_inventario->costo_total;
                    $precio_total_no_existe_tienda += $detalle_inventario->precio_total;
               }
               $costo_total_no_existe_tienda = floatval(number_format($costo_total_no_existe_tienda, 2, '.', ''));
               $precio_total_no_existe_tienda = floatval(number_format($precio_total_no_existe_tienda, 2, '.', ''));

               $info_detalles_inventarios = DB::select("SELECT COUNT(DISTINCT no_articulo) as cantidad_articulos,
                SUM(costo_total) as costo_total, SUM(precio_total) as precio_total
                FROM {$DB}.detalle_inventario");

               $cantidad_articulos = 0;
               $costo_total = 0;
               $precio_total = 0;
               if (count($info_detalles_inventarios) > 0)
               {
                  $cantidad_articulos = $info_detalles_inventarios[0]->cantidad_articulos;
                  $costo_total = $info_detalles_inventarios[0]->costo_total;
                  $precio_total = $info_detalles_inventarios[0]->precio_total;
               }
               $costo_total = floatval(number_format($costo_total, 2, '.', ''));
               $precio_total = floatval(number_format($precio_total, 2, '.', ''));

               $porciento_cantidad_articulos = ($cantidad_articulos > 0) ? (($cantidad_articulos_no_existen_tienda / $cantidad_articulos) * 100) : 0;
               $porciento_cantidad_articulos = floatval(number_format($porciento_cantidad_articulos, 6, '.', ''));
               $porciento_costo_total = ($costo_total > 0) ? (($costo_total_no_existe_tienda / $costo_total) * 100) : 0;
               $porciento_costo_total = floatval(number_format($porciento_costo_total, 6, '.', ''));
               $porciento_precio_total = ($precio_total > 0) ? (($precio_total_no_existe_tienda / $precio_total) * 100) : 0;
               $porciento_precio_total = floatval(number_format($porciento_precio_total, 6, '.', ''));

               $json->cantidad_articulos = $cantidad_articulos;
               $json->cantidad_articulos_no_existen_tienda = $cantidad_articulos_no_existen_tienda;
               $json->porciento_cantidad_articulos = $porciento_cantidad_articulos;
               $json->costo_total = $costo_total;
               $json->costo_total_no_existe_tienda = $costo_total_no_existe_tienda;
               $json->porciento_costo_total = $porciento_costo_total;
               $json->precio_total = $precio_total;
               $json->precio_total_no_existe_tienda = $precio_total_no_existe_tienda;
               $json->porciento_precio_total = $porciento_precio_total;
               $json->data = $detalles_inventarios;
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

    public function get_detalles_diferencias(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request([/*'token',*/ 'DB', 'ind_diferencia'], $json, $request);
        if ($es_request_valido)
        {
            $DB = ($request->input('DB') != NULL) ? $request->input('DB') : '';
            $ind_diferencia = ($request->input('ind_diferencia') != NULL) ? $request->input('ind_diferencia') : 0;
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos)
            {
               $detalles_inventarios = DB::select("SELECT no_articulo, SUM(cantidad) as cantidad FROM {$DB}.detalle_inventario GROUP BY no_articulo");
               $articulos = DB::select("SELECT no_articulo, existencia, descripcion FROM {$DB}.articulos");
               $articulos_comparar = [];
               foreach ($articulos as $articulo)
               {
                   $articulos_comparar[$articulo->no_articulo] = $articulo;
               }

               $detalles_inventarios_enviar = [];
               foreach ($detalles_inventarios as $detalle_inventario)
               {
                   $existencia = (isset($articulos_comparar[$detalle_inventario->no_articulo])) ? $articulos_comparar[$detalle_inventario->no_articulo]->existencia : 0;
                   $existencia = ($existencia != NULL) ? $existencia : 0;

                   $descripcion = (isset($articulos_comparar[$detalle_inventario->no_articulo])) ? $articulos_comparar[$detalle_inventario->no_articulo]->descripcion : '';
                   $descripcion = ($descripcion != NULL) ? $descripcion : 0;

                   $cantidad = $detalle_inventario->cantidad;
                   $diferencia = ($cantidad - $existencia);

                   if ($ind_diferencia == 0)
                   {
                       if ($diferencia == 0)
                       {
                           $detalles_inventarios_enviar[] = [
                               'no_articulo' => $detalle_inventario->no_articulo,
                               'descripcion' => $descripcion,
                               'cantidad_inventariada' => $detalle_inventario->cantidad,
                               'existencia' => $existencia,
                               'diferencia' => $diferencia
                           ];
                       }
                   }
                   else if ($ind_diferencia == 1)
                   {
                       if ($diferencia > 0)
                       {
                           $detalles_inventarios_enviar[] = [
                                'no_articulo' => $detalle_inventario->no_articulo,
                                'descripcion' => $descripcion,
                                'cantidad_inventariada' => $detalle_inventario->cantidad,
                                'existencia' => $existencia,
                                'diferencia' => $diferencia
                            ];
                       }
                   }
                   else if ($ind_diferencia == -1)
                   {
                       if ($diferencia < 0)
                       {
                           $detalles_inventarios_enviar[] = [
                                'no_articulo' => $detalle_inventario->no_articulo,
                                'descripcion' => $descripcion,
                                'cantidad_inventariada' => $detalle_inventario->cantidad,
                                'existencia' => $existencia,
                                'diferencia' => $diferencia
                            ];
                       }
                   }
               }

               $json->data = $detalles_inventarios_enviar;
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

    public function get_info_diferencias(Request $request)
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
               $detalles_inventarios = DB::select("SELECT no_articulo, SUM(cantidad) as cantidad FROM {$DB}.detalle_inventario GROUP BY no_articulo");
               $articulos = DB::select("SELECT no_articulo, existencia, costo, precio FROM {$DB}.articulos");
               $articulos_comparar = [];
               foreach ($articulos as $articulo)
               {
                   $articulos_comparar[$articulo->no_articulo] = $articulo;
               }

               $cantidad_articulos_cero = 0;
               $cantidad_articulos_positivo = 0;
               $cantidad_articulos_negativo = 0;
               $costo_total_cero = 0;
               $costo_total_positivo = 0;
               $costo_total_negativo = 0;
               $precio_total_cero = 0;
               $precio_total_positivo = 0;
               $precio_total_negativo = 0;
               foreach ($detalles_inventarios as $detalle_inventario)
               {
                   $existencia = (isset($articulos_comparar[$detalle_inventario->no_articulo])) ? $articulos_comparar[$detalle_inventario->no_articulo]->existencia : 0;
                   $existencia = ($existencia != NULL) ? $existencia : 0;

                   $costo = (isset($articulos_comparar[$detalle_inventario->no_articulo])) ? $articulos_comparar[$detalle_inventario->no_articulo]->costo : 0;
                   $costo = ($costo != NULL) ? $costo : 0;

                   $precio = (isset($articulos_comparar[$detalle_inventario->no_articulo])) ? $articulos_comparar[$detalle_inventario->no_articulo]->precio : 0;
                   $precio = ($precio != NULL) ? $precio : 0;

                   $cantidad = $detalle_inventario->cantidad;
                   $diferencia = ($cantidad - $existencia);

                    if ($diferencia == 0)
                    {
                        $cantidad_articulos_cero++;
                        $costo_total_cero += ($diferencia * $costo);
                        $precio_total_cero += ($diferencia * $precio);
                    }
                    else if ($diferencia > 0)
                    {
                        $cantidad_articulos_positivo++;
                        $costo_total_positivo += ($diferencia * $costo);
                        $precio_total_positivo += ($diferencia * $precio);
                    }
                    else if ($diferencia < 0)
                    {
                        $cantidad_articulos_negativo++;
                        $costo_total_negativo += ($diferencia * $costo);
                        $precio_total_negativo += ($diferencia * $precio);
                    }
               }

               $json->cantidad_articulos_cero = $cantidad_articulos_cero;
               $json->cantidad_articulos_positivo = $cantidad_articulos_positivo;
               $json->cantidad_articulos_negativo = $cantidad_articulos_negativo;
               $json->costo_total_cero = floatval(number_format($costo_total_cero, 2, '.', ''));
               $json->costo_total_positivo = floatval(number_format($costo_total_positivo, 2, '.', ''));
               $json->costo_total_negativo = floatval(number_format($costo_total_negativo, 2, '.', ''));
               $json->precio_total_cero = floatval(number_format($precio_total_cero, 2, '.', ''));
               $json->precio_total_positivo = floatval(number_format($precio_total_positivo, 2, '.', ''));
               $json->precio_total_negativo = floatval(number_format($precio_total_negativo, 2, '.', ''));
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

    public function get_info_diferencias2(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request([/*'token',*/ 'DB', 'ind_diferencia'], $json, $request);
        if ($es_request_valido)
        {
            $DB = ($request->input('DB') != NULL) ? $request->input('DB') : '';
            $ind_diferencia = ($request->input('ind_diferencia') != NULL) ? $request->input('ind_diferencia') : 0;
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos)
            {
               $detalles_inventarios = DB::select("SELECT no_articulo, SUM(cantidad) as cantidad FROM {$DB}.detalle_inventario GROUP BY no_articulo");
               $articulos = DB::select("SELECT no_articulo, existencia, descripcion, costo, precio FROM {$DB}.articulos");
               $articulos_comparar = [];
               foreach ($articulos as $articulo)
               {
                   $articulos_comparar[$articulo->no_articulo] = $articulo;
               }
               $detalles_inventarios_enviar = [];
               foreach ($detalles_inventarios as $detalle_inventario)
               {
                   $existencia = (isset($articulos_comparar[$detalle_inventario->no_articulo])) ? $articulos_comparar[$detalle_inventario->no_articulo]->existencia : 0;
                   $existencia = ($existencia != NULL) ? $existencia : 0;

                   $descripcion = (isset($articulos_comparar[$detalle_inventario->no_articulo])) ? $articulos_comparar[$detalle_inventario->no_articulo]->descripcion : '';
                   $descripcion = ($descripcion != NULL) ? $descripcion : 0;

                   $costo = (isset($articulos_comparar[$detalle_inventario->no_articulo])) ? $articulos_comparar[$detalle_inventario->no_articulo]->costo : 0;
                   $costo = ($costo != NULL) ? $costo : 0;

                   $precio = (isset($articulos_comparar[$detalle_inventario->no_articulo])) ? $articulos_comparar[$detalle_inventario->no_articulo]->precio : 0;
                   $precio = ($precio != NULL) ? $precio : 0;

                   $cantidad = $detalle_inventario->cantidad;
                   $diferencia = ($cantidad - $existencia);

                   $costo_total = ($costo * $diferencia);
                   $precio_total = ($precio * $diferencia);
                    if ($diferencia == 0)
                    {
                        if ($ind_diferencia == 0)
                        {
                            $detalles_inventarios_enviar[] = [
                                'no_articulo' => $detalle_inventario->no_articulo,
                                'descripcion' => $descripcion,
                                'cantidad_inventariada' => $detalle_inventario->cantidad,
                                'existencia' => $existencia,
                                'diferencia' => $diferencia,
                                'costo_total' => floatval(number_format($costo_total, 2, '.', '')),
                                'precio_total' => floatval(number_format($precio_total, 2, '.', ''))
                            ];
                        }
                    }
                    else if ($diferencia > 0)
                    {
                        if ($ind_diferencia == 1)
                        {
                            $detalles_inventarios_enviar[] = [
                                'no_articulo' => $detalle_inventario->no_articulo,
                                'descripcion' => $descripcion,
                                'cantidad_inventariada' => $detalle_inventario->cantidad,
                                'existencia' => $existencia,
                                'diferencia' => $diferencia,
                                'costo_total' => floatval(number_format($costo_total, 2, '.', '')),
                                'precio_total' => floatval(number_format($precio_total, 2, '.', ''))
                            ];
                        }
                    }
                    else if ($diferencia < 0)
                    {
                        if ($ind_diferencia == -1)
                        {
                            $detalles_inventarios_enviar[] = [
                                'no_articulo' => $detalle_inventario->no_articulo,
                                'descripcion' => $descripcion,
                                'cantidad_inventariada' => $detalle_inventario->cantidad,
                                'existencia' => $existencia,
                                'diferencia' => $diferencia,
                                'costo_total' => floatval(number_format($costo_total, 2, '.', '')),
                                'precio_total' => floatval(number_format($precio_total, 2, '.', ''))
                            ];
                        }
                    }
               }

               $json->data = $detalles_inventarios_enviar;
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

    public function get_nro_tiros(Request $request)
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB))
                //{
                    $nro_tiros = DB::select("SELECT COUNT(1) as nro_tiros FROM {$DB}.detalle_inventario");
                    $json->nro_tiros = $nro_tiros[0]->nro_tiros;
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

    public function get_duplicados_nro_articulos(Request $request)
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB))
                //{
                    $valores = [];
                    $condicion_id_tipo_ubicacion = "";
                    if ($request->exists('id_tipo_ubicacion'))
                    {
                        $id_tipo_ubicacion = ($request->input('id_tipo_ubicacion') != NULL) ? $request->input('id_tipo_ubicacion') : 0;
                        $condicion_id_tipo_ubicacion = "u.id_tipo_ubicacion = ? AND";
                        $valores[] = $id_tipo_ubicacion;
                    }

                    $registros_duplicados = DB::select("SELECT d.*,
                     ifnull(tu.descripcion, '') as tipo_ubicacion
                     FROM {$DB}.detalle_inventario d
                     left join {$DB}.ubicaciones u
                       on (d.id_ubicacion = u.id)
                     left join {$DB}.tipo_ubicacion tu
                      on (u.id_tipo_ubicacion = tu.id)
                     WHERE 1 = 1 AND {$condicion_id_tipo_ubicacion} d.no_articulo IN (SELECT d2.no_articulo FROM {$DB}.detalle_inventario d2 GROUP BY d2.no_articulo, d2.cantidad
                     having count(*) >= 2)", $valores);
                    $json->registros_duplicados = $registros_duplicados;
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

    public function get_cantidad_desde_hasta(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request([/*'token',*/ 'DB', 'fecha_desde', 'fecha_hasta'], $json, $request);
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
                    $fecha_desde = ($request->input('fecha_desde') != NULL) ? $request->input('fecha_desde') : '';
                    $fecha_hasta = ($request->input('fecha_hasta') != NULL) ? $request->input('fecha_hasta') : '';
                    $columna_usuario = "";
                    $condicion_group_by_usuario = "";
                    if ($request->exists('es_agrupar_por_usuario') && $request->input('es_agrupar_por_usuario') == 1)
                    {
                        $columna_usuario = ",d.id_usuario_registro";
                        $condicion_group_by_usuario = "GROUP BY d.id_usuario_registro";
                    }

                    $valores = [
                        $fecha_desde,
                        $fecha_hasta
                    ];
                    $condicion_id_tipo_ubicacion = "";
                    if ($request->exists('id_tipo_ubicacion'))
                    {
                        $id_tipo_ubicacion = ($request->input('id_tipo_ubicacion') != NULL) ? $request->input('id_tipo_ubicacion') : 0;
                        $condicion_id_tipo_ubicacion = "AND u.id_tipo_ubicacion = ?";
                        $valores[] = $id_tipo_ubicacion;
                    }

                    $cantidad_desde_hasta = DB::select("SELECT SUM(d.cantidad) as cantidad {$columna_usuario}
                     FROM {$DB}.detalle_inventario d
                     left join {$DB}.ubicaciones u
                       on (d.id_ubicacion = u.id)
                     WHERE DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:%i') BETWEEN ? AND ?
                     {$condicion_id_tipo_ubicacion}
                     {$condicion_group_by_usuario}", $valores);
                    if ($request->exists('es_agrupar_por_usuario') && $request->input('es_agrupar_por_usuario') == 1) {
                        $json->datos = $cantidad_desde_hasta;
                        $json->fecha_desde = $fecha_desde;
                        $json->fecha_hasta = $fecha_hasta;
                    } else {
                        $json->cantidad = count($cantidad_desde_hasta) ? $cantidad_desde_hasta[0]->cantidad : 0;
                        $json->cantidad = ($json->cantidad != NULL) ? $json->cantidad : 0;
                        $json->fecha_desde = $fecha_desde;
                        $json->fecha_hasta = $fecha_hasta;
                    }
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

    public function get_nro_tiros_desde_hasta(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request([/*'token',*/ 'DB', 'fecha_desde', 'fecha_hasta'], $json, $request);
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
                    $fecha_desde = ($request->input('fecha_desde') != NULL) ? $request->input('fecha_desde') : '';
                    $fecha_hasta = ($request->input('fecha_hasta') != NULL) ? $request->input('fecha_hasta') : '';
                    $columna_usuario = "";
                    $condicion_group_by_usuario = "";
                    if ($request->exists('es_agrupar_por_usuario') && $request->input('es_agrupar_por_usuario') == 1)
                    {
                        $columna_usuario = ",d.id_usuario_registro";
                        $condicion_group_by_usuario = "GROUP BY d.id_usuario_registro";
                    }

                    $valores = [
                        $fecha_desde,
                        $fecha_hasta
                    ];
                    $condicion_id_tipo_ubicacion = "";
                    if ($request->exists('id_tipo_ubicacion'))
                    {
                        $id_tipo_ubicacion = ($request->input('id_tipo_ubicacion') != NULL) ? $request->input('id_tipo_ubicacion') : 0;
                        $condicion_id_tipo_ubicacion = "AND u.id_tipo_ubicacion = ?";
                        $valores[] = $id_tipo_ubicacion;
                    }

                    $nro_tiros_desde_hasta = DB::select("SELECT COUNT(1) as nro_tiros {$columna_usuario}
                     FROM {$DB}.detalle_inventario d
                     left join {$DB}.ubicaciones u
                       on (d.id_ubicacion = u.id)
                     WHERE DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:%i') BETWEEN ? AND ?
                     {$condicion_id_tipo_ubicacion}
                     {$condicion_group_by_usuario}", $valores);
                    if ($request->exists('es_agrupar_por_usuario') && $request->input('es_agrupar_por_usuario') == 1) {
                        $json->datos = $nro_tiros_desde_hasta;
                        $json->fecha_desde = $fecha_desde;
                        $json->fecha_hasta = $fecha_hasta;
                    } else {
                        $json->nro_tiros = count($nro_tiros_desde_hasta) ? $nro_tiros_desde_hasta[0]->nro_tiros : 0;
                        $json->nro_tiros = ($json->nro_tiros != NULL) ? $json->nro_tiros : 0;
                        $json->fecha_desde = $fecha_desde;
                        $json->fecha_hasta = $fecha_hasta;
                    }
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

    public function get_costo_total_inventario(Request $request)
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB))
                //{
                    $costo_total = DB::select("SELECT SUM(costo_total) as costo_total FROM {$DB}.detalle_inventario");
                    $json->costo_total = floatval(number_format($costo_total[0]->costo_total, 2, '.', ''));
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

    public function get_auditar(Request $request)
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB))
                //{
                    $valores = [];
                    $condicion_id_tipo_ubicacion = "";
                    if ($request->exists('id_tipo_ubicacion'))
                    {
                        $id_tipo_ubicacion = ($request->input('id_tipo_ubicacion') != NULL) ? $request->input('id_tipo_ubicacion') : 0;
                        $condicion_id_tipo_ubicacion = "AND u.id_tipo_ubicacion = ?";
                        $valores[] = $id_tipo_ubicacion;
                    }

                    $condicion_id_auditor = "";
                    if ($request->exists('id_auditor'))
                    {
                        $id_auditor = ($request->input('id_auditor') != NULL) ? $request->input('id_auditor') : 0;
                        $condicion_id_auditor = "AND d.id_auditor = ?";
                        $valores[] = $id_auditor;
                    }

                    $condicion_id_tipo_auditoria = "";
                    if ($request->exists('id_tipo_auditoria'))
                    {
                        $id_tipo_auditoria = ($request->input('id_tipo_auditoria') != NULL) ? $request->input('id_tipo_auditoria') : 0;
                        $condicion_id_tipo_auditoria = "AND d.id_tipo_auditoria = ?";
                        $valores[] = $id_tipo_auditoria;
                    }

                    $detalles_inventario = DB::select("SELECT d.*, ifnull(tu.descripcion, '') as tipo_ubicacion
                     FROM {$DB}.detalle_inventario d
                     left join {$DB}.ubicaciones u
                      on (d.id_ubicacion = u.id)
                     left join {$DB}.tipo_ubicacion tu
                      on (u.id_tipo_ubicacion = tu.id)
                WHERE 1 = 1 {$condicion_id_tipo_ubicacion} {$condicion_id_auditor} {$condicion_id_tipo_auditoria}", $valores);
                    $json->data = $detalles_inventario;
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

    public function get_precio_total_inventario(Request $request)
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB))
                //{
                    $precio_total = DB::select("SELECT SUM(precio_total) as precio_total FROM {$DB}.detalle_inventario");
                    $json->precio_total =  floatval(number_format($precio_total[0]->precio_total, 2, '.', ''));
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
                $datos_detalles_inventarios = $request;
                foreach ($datos_detalles_inventarios as $datos_detalles_inventario)
                {
                    try
                    {
                        $codigo_capturado = (isset($datos_detalles_inventario['codigo_capturado'])) ? $datos_detalles_inventario['codigo_capturado'] : '';
                        $articulo = DB::select("SELECT codigo_barra, alterno1, alterno2, alterno3, descripcion,
                         costo, precio
                         FROM {$DB}.articulos
                         WHERE no_articulo = ? LIMIT 1", [
                             $datos_detalles_inventario['no_articulo']
                         ]);
                        if (count($articulo) > 0)
                        {
                            $articulo = $articulo[0];

                            $cod_alterno = DB::select("SELECT cod_alterno FROM {$DB}.ubicaciones
                             WHERE id = ? LIMIT 1", [
                                 $datos_detalles_inventario['id_ubicacion']
                            ]);
                            $cod_alterno = (count($cod_alterno) > 0) ? $cod_alterno[0]->cod_alterno : '';

                            $nro_detalles_inventarios = DB::select("SELECT COUNT(*) as nro_detalles_inventarios FROM {$DB}.detalle_inventario");
                            $nro_detalles_inventarios = $nro_detalles_inventarios[0]->nro_detalles_inventarios;
                            DB::update("ALTER TABLE {$DB}.detalle_inventario AUTO_INCREMENT = {$nro_detalles_inventarios}");

                            $costo_total_detalle = (floatval($articulo->costo) * floatval($datos_detalles_inventario['cantidad']));
                            $precio_total_detalle = (floatval($articulo->precio) * floatval($datos_detalles_inventario['cantidad']));

                            $codigo_barra = (isset($datos_detalles_inventario['codigo_barra'])) ? $datos_detalles_inventario['codigo_barra'] : $articulo->codigo_barra;
                            $id_auditor = (isset($datos_detalles_inventario['id_auditor'])) ? $datos_detalles_inventario['id_auditor'] : 0;
                            $id_tipo_auditoria = (isset($datos_detalles_inventario['id_tipo_auditoria'])) ? $datos_detalles_inventario['id_tipo_auditoria'] : 0;
                            $pre_conteo = (isset($datos_detalles_inventario['pre_conteo'])) ? $datos_detalles_inventario['pre_conteo'] : 0;
                            $cantidad_auditada = (isset($datos_detalles_inventario['cantidad_auditada'])) ? $datos_detalles_inventario['cantidad_auditada'] : 0;
                            $diferencia = (isset($datos_detalles_inventario['diferencia'])) ? $datos_detalles_inventario['diferencia'] : 0;
                            $porcentaje_diferencia = (isset($datos_detalles_inventario['porcentaje_diferencia'])) ? $datos_detalles_inventario['porcentaje_diferencia'] : 0;
                            $id_tipo_error = (isset($datos_detalles_inventario['id_tipo_error'])) ? $datos_detalles_inventario['id_tipo_error'] : 0;
                            $notas = (isset($datos_detalles_inventario['notas'])) ? $datos_detalles_inventario['notas'] : '';

                            DB::insert("INSERT INTO {$DB}.detalle_inventario (id_terminal, no_detalleInv,
                            no_articulo, codigo_barra, alterno1, alterno2, alterno3, descripcion,
                            cantidad, costo, costo_total, precio, precio_total,
                            id_ubicacion, cod_alterno, fecha_registro, fecha_modificacion, id_usuario_registro,
                            id_usuario_modificacion, estado, codigo_capturado, created_at, updated_at, id_auditor, id_tipo_auditoria,
                            pre_conteo, cantidad_auditada, diferencia, porcentaje_diferencia, id_tipo_error, notas)
                            VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,1,?,?,?,?,?,?,?,?,?,?,?)", [
                                $datos_detalles_inventario['id_terminal'],
                                $datos_detalles_inventario['no_detalleInv'],
                                $datos_detalles_inventario['no_articulo'],
                                $codigo_barra,
                                $articulo->alterno1,
                                $articulo->alterno2,
                                $articulo->alterno3,
                                $articulo->descripcion,
                                $datos_detalles_inventario['cantidad'],
                                $articulo->costo,
                                $costo_total_detalle,
                                $articulo->precio,
                                $precio_total_detalle,
                                $datos_detalles_inventario['id_ubicacion'],
                                $cod_alterno,
                                $datos_detalles_inventario['fecha_registro'],
                                $datos_detalles_inventario['fecha_modificacion'],
                                $datos_detalles_inventario['id_usuario_registro'],
                                $datos_detalles_inventario['id_usuario_modificacion'],
                                $codigo_capturado,
                                $fecha_hoy,
                                $fecha_hoy,
                                $id_auditor,
                                $id_tipo_auditoria,
                                $pre_conteo,
                                $cantidad_auditada,
                                $diferencia,
                                $porcentaje_diferencia,
                                $id_tipo_error,
                                $notas
                            ]);
                        }
                        else
                        {
                            $json->error = 1;
                            $json->error_type = 1;
                            $json->error_message = "El artículo con el código: \"{$datos_detalles_inventario['no_articulo']}\" no existe en la base de datos";

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
                    } catch (\Exception $e)
                    {
                        $articulo = DB::select("SELECT codigo_barra, alterno1, alterno2, alterno3, descripcion,
                        costo, precio
                        FROM {$DB}.articulos
                        WHERE no_articulo = ? LIMIT 1", [
                            $datos_detalles_inventario['no_articulo']
                        ]);
                        $articulo = $articulo[0];
                        $costo_total_detalle = (floatval($articulo->costo) * floatval($datos_detalles_inventario['cantidad']));
                        $precio_total_detalle = (floatval($articulo->precio) * floatval($datos_detalles_inventario['cantidad']));

                        $valores = [
                            $datos_detalles_inventario['no_articulo'],
                            $datos_detalles_inventario['id_ubicacion'],
                            $datos_detalles_inventario['cantidad'],
                            $datos_detalles_inventario['fecha_registro'],
                            $datos_detalles_inventario['fecha_modificacion'],
                            $datos_detalles_inventario['id_usuario_registro'],
                            $datos_detalles_inventario['id_usuario_modificacion'],
                            $codigo_capturado,
                            $costo_total_detalle,
                            $precio_total_detalle,
                            $articulo->costo,
                            $articulo->precio,
                        ];

                        $condicion_codigo_barra = "";
                        if (isset($datos_detalles_inventario['codigo_barra']))
                        {
                            $condicion_codigo_barra = ",codigo_barra = ?";
                            $valores[] = $datos_detalles_inventario['codigo_barra'];
                        }
                        else
                        {
                          $condicion_codigo_barra = ",codigo_barra = ?";
                          $valores[] = $articulo->codigo_barra;
                        }

                        $valores[] = $datos_detalles_inventario['no_detalleInv'];
                        $valores[] = $datos_detalles_inventario['id_terminal'];
                        //echo $datos_detalles_inventario['no_detalleInv'] .'<br>';
                        $es_actualizado = DB::update("UPDATE {$DB}.detalle_inventario set no_articulo = ?,
                        id_ubicacion = ?, cantidad = ?, fecha_registro = ?, fecha_modificacion = ?,
                        id_usuario_registro = ?, id_usuario_modificacion = ?, estado = 1,
                        codigo_capturado = ?, costo_total = ?, precio_total = ?,
                        costo = ?, precio = ? {$condicion_codigo_barra}
                        WHERE no_detalleInv = ? AND id_terminal = ?", $valores);
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

                $datos_detalles_inventarios = $request;
                foreach ($datos_detalles_inventarios as $datos_detalles_inventario)
                {
                    $articulo = DB::select("SELECT codigo_barra, alterno1, alterno2, alterno3, descripcion,
                    costo, precio
                    FROM {$DB}.articulos
                    WHERE no_articulo = ? LIMIT 1", [
                        $datos_detalles_inventario['no_articulo']
                    ]);
                    if (count($articulo) > 0)
                    {
                        $articulo = $articulo[0];
                        $cod_alterno = DB::select("SELECT cod_alterno FROM {$DB}.ubicaciones
                         WHERE id = ? LIMIT 1", [
                            $datos_detalles_inventario['id_ubicacion']
                        ]);
                        $cod_alterno = (count($cod_alterno) > 0) ? $cod_alterno[0]->cod_alterno : '';

                        $costo_total_detalle = (floatval($articulo->costo) * floatval($datos_detalles_inventario['cantidad']));
                        $precio_total_detalle = (floatval($articulo->precio) * floatval($datos_detalles_inventario['cantidad']));
                        $codigo_barra = (isset($datos_detalles_inventario['codigo_barra'])) ? $datos_detalles_inventario['codigo_barra'] : $articulo->codigo_barra;

                        DB::update("UPDATE {$DB}.detalle_inventario set no_articulo = ?,
                        codigo_barra = ?, alterno1 = ?, alterno2 = ?, alterno3 = ?, descripcion = ?,
                        cantidad = ?, costo = ?, costo_total = ?, precio = ?, precio_total = ?,
                        id_ubicacion = ?, cod_alterno = ?, fecha_registro = ?, fecha_modificacion = ?,
                        id_usuario_registro = ?, id_usuario_modificacion = ?, estado = 1, updated_at = ?
                        WHERE no_detalleInv = ? AND id_terminal = ?", [
                            $datos_detalles_inventario['no_articulo'],
                            $codigo_barra,
                            $articulo->alterno1,
                            $articulo->alterno2,
                            $articulo->alterno3,
                            $articulo->descripcion,
                            $datos_detalles_inventario['cantidad'],
                            $articulo->costo,
                            $costo_total_detalle,
                            $articulo->precio,
                            $precio_total_detalle,
                            $datos_detalles_inventario['id_ubicacion'],
                            $cod_alterno,
                            $datos_detalles_inventario['fecha_registro'],
                            $datos_detalles_inventario['fecha_modificacion'],
                            $datos_detalles_inventario['id_usuario_registro'],
                            $datos_detalles_inventario['id_usuario_modificacion'],
                            $fecha_hoy,
                            $datos_detalles_inventario['no_detalleInv'],
                            $datos_detalles_inventario['id_terminal']
                        ]);
                    }
                    else
                    {
                        $json->error = 1;
                        $json->error_type = 1;
                        $json->error_message = "El artículo con el código: \"{$datos_detalles_inventario['no_articulo']}\" no existe en la base de datos";
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

    public function editar_info_auditoria(Request $request)
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
                $datos_detalles_inventarios = $request;
                foreach ($datos_detalles_inventarios as $datos_detalles_inventario)
                {
                    if (!isset($datos_detalles_inventario['id']) || !isset($datos_detalles_inventario['pre_conteo'])
                     || !isset($datos_detalles_inventario['cantidad_auditada']) || !isset($datos_detalles_inventario['diferencia'])
                     || !isset($datos_detalles_inventario['porcentaje_diferencia']) || !isset($datos_detalles_inventario['id_tipo_error'])
                     || !isset($datos_detalles_inventario['notas']) || !isset($datos_detalles_inventario['id_tipo_auditoria']))
                    {
                        $json->error = 1;
                        $json->error_type = 400;
                        $json->error_message = "Faltan parámetros (id, pre_conteo, cantidad_auditada, diferencia, porcentaje_diferencia, id_tipo_error, notas)";
                        return response()->json(
                            $json,
                            200
                        );
                        exit();
                    }

                    $no_articulo = DB::select("SELECT no_articulo FROM {$DB}.detalle_inventario WHERE id = ? LIMIT 1", [
                        $datos_detalles_inventario['id']
                    ]);
                    $no_articulo = (count($no_articulo) > 0) ? $no_articulo[0]->no_articulo : 0;

                    $articulo = DB::select("SELECT costo, precio
                    FROM {$DB}.articulos
                    WHERE no_articulo = ? LIMIT 1", [
                        $no_articulo
                    ]);
                    $articulo = $articulo[0];
                    $costo_total_detalle = (floatval($articulo->costo) * floatval($datos_detalles_inventario['cantidad_auditada']));
                    $precio_total_detalle = (floatval($articulo->precio) * floatval($datos_detalles_inventario['cantidad_auditada']));

                    DB::update("UPDATE {$DB}.detalle_inventario set pre_conteo = ?,
                     cantidad = ?,
                     cantidad_auditada = ?, diferencia = ?, porcentaje_diferencia = ?,
                     id_tipo_error = ?, notas = ?, id_tipo_auditoria = ?,
                     costo_total = ?, precio_total = ?, costo = ?, precio = ?
                     WHERE id = ?", [
                       $datos_detalles_inventario['pre_conteo'],
                       $datos_detalles_inventario['cantidad_auditada'],
                       $datos_detalles_inventario['cantidad_auditada'],
                       $datos_detalles_inventario['diferencia'],
                       $datos_detalles_inventario['porcentaje_diferencia'],
                       $datos_detalles_inventario['id_tipo_error'],
                       $datos_detalles_inventario['notas'],
                       $datos_detalles_inventario['id_tipo_auditoria'],
                       $costo_total_detalle,
                       $precio_total_detalle,
                       $articulo->costo,
                       $articulo->precio,
                       $datos_detalles_inventario['id']
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

    public function editar_info_auditoria_manual(Request $request)
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
                $datos_detalles_inventarios = $request;
                foreach ($datos_detalles_inventarios as $datos_detalles_inventario)
                {
                    if (!isset($datos_detalles_inventario['id']) || !isset($datos_detalles_inventario['id_auditor'])
                     || !isset($datos_detalles_inventario['id_tipo_auditoria']))
                    {
                        $json->error = 1;
                        $json->error_type = 400;
                        $json->error_message = "Faltan parámetros (id, id_auditor, id_tipo_auditoria)";
                        return response()->json(
                            $json,
                            200
                        );
                        exit();
                    }

                    DB::update("UPDATE {$DB}.detalle_inventario set id_auditor = ?,
                     id_tipo_auditoria = ?
                     WHERE id = ?", [
                       $datos_detalles_inventario['id_auditor'],
                       $datos_detalles_inventario['id_tipo_auditoria'],
                       $datos_detalles_inventario['id']
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

    public function editar_cantidad(Request $request)
    {
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
                $datos_detalles_inventarios = $request;
                foreach ($datos_detalles_inventarios as $datos_detalles_inventario)
                {
                  if (!isset($datos_detalles_inventario['id_terminal']) ||
                   !isset($datos_detalles_inventario['no_detalleInv']) ||
                   !isset($datos_detalles_inventario['cantidad']))
                  {
                      $json->error = 1;
                      $json->error_type = 400;
                      $json->error_message = "Faltan parámetros (id_terminal, no_detalleInv, cantidad)";
                      return response()->json(
                          $json,
                          200
                      );
                      exit();
                  }
                  $nro_articulo = DB::select("SELECT no_articulo FROM {$DB}.detalle_inventario
                   WHERE id_terminal = ? AND no_detalleInv = ?", [
                     $datos_detalles_inventario['id_terminal'],
                     $datos_detalles_inventario['no_detalleInv']
                  ]);
                  $nro_articulo = (count($nro_articulo) > 0) ? $nro_articulo[0]->no_articulo : 0;

                  $articulo = DB::select("SELECT costo, precio FROM {$DB}.articulos
                   WHERE no_articulo = ? LIMIT 1", [
                     $nro_articulo
                  ]);

                  $costo = 0;
                  $costo_total = 0;
                  $precio = 0;
                  $precio_total = 0;
                  if  (count($articulo) > 0)
                  {
                      $articulo = $articulo[0];
                      $costo = $articulo->costo;
                      $costo_total = (floatval($articulo->costo) * floatval($datos_detalles_inventario['cantidad']));
                      $precio = $articulo->precio;
                      $precio_total = (floatval($articulo->precio) * floatval($datos_detalles_inventario['cantidad']));
                  }

                  $valores = [
                    $datos_detalles_inventario['cantidad'],
                    $costo,
                    $costo_total,
                    $precio,
                    $precio_total
                  ];

                  $condicion_codigo_alterno = "";
                  $condicion_id_ubicacion = "";
                  if (isset($datos_detalles_inventario['id_ubicacion']))
                  {
                    $condicion_id_ubicacion = ", id_ubicacion = ?";
                    $valores[] = $datos_detalles_inventario['id_ubicacion'];

                    $cod_alterno = DB::select("SELECT cod_alterno FROM {$DB}.ubicaciones WHERE id = ? LIMIT 1", [
                        $datos_detalles_inventario['id_ubicacion']
                    ]);
                    $cod_alterno = (count($cod_alterno) > 0) ? $cod_alterno[0]->cod_alterno : '';
                    $condicion_codigo_alterno = ", cod_alterno = ?";
                    $valores[] = $cod_alterno;
                  }

                  $valores[] = $datos_detalles_inventario['id_terminal'];
                  $valores[] = $datos_detalles_inventario['no_detalleInv'];
                  DB::update("UPDATE {$DB}.detalle_inventario set cantidad = ?,
                   costo = ?, costo_total = ?, precio	 = ?, precio_total = ?
                   {$condicion_id_ubicacion} {$condicion_codigo_alterno}
                   WHERE id_terminal = ? AND no_detalleInv = ?", $valores);
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

    public function get_cantidad_tiros_hora_terminal(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request(['DB', 'fecha_desde', 'fecha_hasta'], $json, $request);
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
                $fecha_desde_original = ($request->input('fecha_desde') != NULL) ? $request->input('fecha_desde') : '';
                $fecha_hasta_original = ($request->input('fecha_hasta') != NULL) ? $request->input('fecha_hasta') : '';

                $valores = [];
                $valores[] = $fecha_desde_original;
                $valores[] = $fecha_hasta_original;
                $condicion_id_tipo_ubicacion = "";
                if ($request->exists('id_tipo_ubicacion'))
                {
                    $condicion_id_tipo_ubicacion = "AND u.id_tipo_ubicacion = ?";
                    $valores[] = $request->input('id_tipo_ubicacion');
                }
                //Capturamos las cantidades de tiros (agrupados por horas y terminal)
                $datos_cantidades_tiros = DB::select("SELECT COUNT(1) as cantidad_tiros,
                    CONCAT(d.id_terminal, '_', DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:00:00')) as ind
                    FROM {$DB}.detalle_inventario d
                    left join {$DB}.ubicaciones u
                     on (d.id_ubicacion = u.id)
                    WHERE DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:%i') BETWEEN ? AND ?
                    {$condicion_id_tipo_ubicacion}
                    GROUP BY d.id_terminal, DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H')
                    ORDER BY DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H') asc", $valores);
                $cantidades_tiros = [];
                foreach ($datos_cantidades_tiros as $cantidad_tiros)
                {
                    $cantidades_tiros[$cantidad_tiros->ind] = $cantidad_tiros;
                }

                $fecha_desde = explode(':', $fecha_desde_original)[0];
                $fecha_hasta = explode(':', $fecha_hasta_original)[0];
                $fecha_desde = "{$fecha_desde}:00";
                $fecha_hasta = "{$fecha_hasta}:00";
                $time_fecha_desde = strtotime($fecha_desde);
                $time_fecha_hasta = strtotime($fecha_hasta);
                $diff = ($time_fecha_hasta - $time_fecha_desde);
                $horas = ($diff / ( 60 * 60 ));

                //Capturamos las terminales
                $terminales = DB::select("SELECT DISTINCT id_terminal FROM {$DB}.detalle_inventario");
                $cantidades_tiros_enviar = [];
                $nro_tiros = 0;
                $fecha_ind = $fecha_desde;
                for ($i = 0; $i <= $horas; $i++)
                {
                    if ($i > 0)
                    {
                        $fecha_ind = date("Y-m-d H:i:s", strtotime("{$fecha_ind} +1 hours"));
                    }
                    else {
                        $fecha_ind = "{$fecha_ind}:00";
                    }

                    //Buscamos la cantidad de tiros por terminal
                    $es_guardar_cantidad_tiro = false;
                    foreach ($terminales as $terminal)
                    {
                        $ind = ($terminal->id_terminal .'_' .$fecha_ind);
                        if (isset($cantidades_tiros[$ind])) {
                            $es_guardar_cantidad_tiro = true;
                        }
                    }

                    if ($es_guardar_cantidad_tiro)
                    {
                        foreach ($terminales as $terminal)
                        {
                            $ind = ($terminal->id_terminal .'_' .$fecha_ind);
                            if (!isset($cantidades_tiros[$ind]))
                            {
                                $cantidades_tiros_enviar[] = [
                                    'id_terminal' => $terminal->id_terminal,
                                    'cantidad_tiros' => 0,
                                    'hora' => date('d/m/Y h:i A', strtotime($fecha_ind))
                                ];
                            }
                            else
                            {
                                $cantidades_tiros_enviar[] = [
                                    'id_terminal' => $terminal->id_terminal,
                                    'cantidad_tiros' => $cantidades_tiros[$ind]->cantidad_tiros,
                                    'hora' => date('d/m/Y h:i A', strtotime($fecha_ind))
                                ];
                                $nro_tiros += $cantidades_tiros[$ind]->cantidad_tiros;
                            }
                        }
                    }
                }

                $json->nro_tiros = $nro_tiros;
                $json->fecha_desde = $fecha_desde_original;
                $json->fecha_hasta = $fecha_hasta_original;
                $json->cantidades_tiros = $cantidades_tiros_enviar;
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


    public function get_cantidad_tiros_tipo_ubicacion(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request(['DB', 'fecha_desde', 'fecha_hasta'], $json, $request);
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
                $fecha_desde = ($request->input('fecha_desde') != NULL) ? $request->input('fecha_desde') : '';
                $fecha_hasta = ($request->input('fecha_hasta') != NULL) ? $request->input('fecha_hasta') : '';

                $datos_cantidades_tiros = DB::select("SELECT COUNT(1) as cantidad_tiros, u.id_tipo_ubicacion
                 FROM {$DB}.detalle_inventario d
                 inner join {$DB}.ubicaciones u
                   on (d.id_ubicacion = u.id)
                 WHERE DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:%i') BETWEEN ? AND ?
                 GROUP BY u.id_tipo_ubicacion", [
                    $fecha_desde,
                    $fecha_hasta
                ]);
                $cantidades_tiros_temp = [];
                foreach ($datos_cantidades_tiros as $datos_cantidad_tiros) {
                    $cantidades_tiros_temp[$datos_cantidad_tiros->id_tipo_ubicacion] = $datos_cantidad_tiros;
                }

                //Agrupamos la cantidad de tiros por tipo de ubicacion
                $tipos_ubicaciones = DB::select("SELECT id, descripcion FROM {$DB}.tipo_ubicacion");
                foreach ($tipos_ubicaciones as $tipo_ubicacion)
                {
                    $nombre_tipo_ubicacion = $tipo_ubicacion->descripcion;
                    if (!isset($cantidades_tiros_temp[$tipo_ubicacion->id]))
                    {
                        $json->$nombre_tipo_ubicacion = 0;
                    }
                    else
                    {
                        $json->$nombre_tipo_ubicacion = $cantidades_tiros_temp[$tipo_ubicacion->id]->cantidad_tiros;
                    }
                }

                $json->fecha_desde = $fecha_desde;
                $json->fecha_hasta = $fecha_hasta;
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

    public function get_cantidad_tiros_tipo_ubicacion_hora(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request(['DB', 'fecha_desde', 'fecha_hasta'], $json, $request);
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
                $fecha_desde_original = ($request->input('fecha_desde') != NULL) ? $request->input('fecha_desde') : '';
                $fecha_hasta_original = ($request->input('fecha_hasta') != NULL) ? $request->input('fecha_hasta') : '';

                $fecha_desde = explode(':', $fecha_desde_original)[0];
                $fecha_hasta = explode(':', $fecha_hasta_original)[0];
                $fecha_desde = "{$fecha_desde}:00";
                $fecha_hasta = "{$fecha_hasta}:00";
                $time_fecha_desde = strtotime($fecha_desde);
                $time_fecha_hasta = strtotime($fecha_hasta);
                $diff = ($time_fecha_hasta - $time_fecha_desde);
                $horas = ($diff / ( 60 * 60 ));

                $datos_cantidades_tiros = DB::select("SELECT COUNT(1) as cantidad_tiros,
                 CONCAT(u.id_tipo_ubicacion, '_', DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:00:00')) as ind
                 FROM {$DB}.detalle_inventario d
                 inner join {$DB}.ubicaciones u
                  on (d.id_ubicacion = u.id)
                 WHERE DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H:%i') BETWEEN ? AND ?
                 GROUP BY u.id_tipo_ubicacion, DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H')
                 ORDER BY DATE_FORMAT(d.fecha_registro, '%Y-%m-%d %H') asc", [
                    $fecha_desde_original,
                    $fecha_hasta_original
                ]);
                $cantidades_tiros = [];
                foreach ($datos_cantidades_tiros as $cantidad_tiros)
                {
                    $cantidades_tiros[$cantidad_tiros->ind] = $cantidad_tiros;
                }

                //Hacemos el procedimiento para capturar la cantidad de tiros por tipo de ubicacion
                $nro_tiros = 0;
                $fecha_ind = $fecha_desde;
                $cantidades_tiros_enviar1 = [];
                for ($i = 0; $i <= $horas; $i++)
                {
                    if ($i > 0)
                    {
                        $fecha_ind = date("Y-m-d H:i:s", strtotime("{$fecha_ind} +1 hours"));
                    }
                    else
                    {
                        $fecha_ind = "{$fecha_ind}:00";
                    }

                    //Capturamos la cantidad de tiros por tipo de ubicacion
                    $tipos_ubicaciones = DB::select("SELECT id, descripcion FROM {$DB}.tipo_ubicacion");
                    foreach ($tipos_ubicaciones as $tipo_ubicacion)
                    {
                        if (!isset($cantidades_tiros_enviar1[$fecha_ind]))
                        {
                            $cantidades_tiros_enviar1[$fecha_ind] = [
                                'hora' => date('d/m/Y h:i A', strtotime($fecha_ind))
                            ];
                        }

                        $ind = ($tipo_ubicacion->id .'_' .$fecha_ind);
                        if (!isset($cantidades_tiros[$ind]))
                        {

                            $cantidades_tiros_enviar1[$fecha_ind][$tipo_ubicacion->descripcion] = 0;
                        } else
                        {
                            $cantidades_tiros_enviar1[$fecha_ind][$tipo_ubicacion->descripcion] = $cantidades_tiros[$ind]->cantidad_tiros;
                            $nro_tiros += $cantidades_tiros[$ind]->cantidad_tiros;
                        }
                    }
                }

                $cantidades_tiros_enviar2 = [];
                foreach ($cantidades_tiros_enviar1 as $cantidad_tiros_enviar1) {
                    $cantidades_tiros_enviar2[] = $cantidad_tiros_enviar1;
                }

                $json->nro_tiros = $nro_tiros;
                $json->fecha_desde = $fecha_desde_original;
                $json->fecha_hasta = $fecha_hasta_original;
                $json->cantidades_tiros = $cantidades_tiros_enviar2;
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

    public function importar_excel_detalle_inventario(Request $request)
    {
        ini_set('memory_limit', '-1');
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
                    DB::statement("TRUNCATE {$DB}.detalle_inventario");
                }
                else
                {
                   // $secuencia = DB::select("SELECT (COUNT(1) + 1) as secuencia FROM {$DB}.detalle_inventario");
                   // $secuencia = (count($secuencia) > 0) ? $secuencia[0]->secuencia : 1;
                }

                $spreadsheet = PHPExcel_IOFactory::load("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/{$nombre_excel}");
                $sheet = $spreadsheet->getSheet(0);
                $highestRow = $sheet->getHighestRow();
                $highestColumn = $sheet->getHighestColumn();
                date_default_timezone_set('UTC');
                for ($row = 1; $row <= $highestRow; $row++)
                {
                    $sheet->getStyle('A' . $row . ':' . $highestColumn . $row)->getNumberFormat()->setFormatCode("YYYY-MM-DD");
                    $datos_fila = $sheet->rangeToArray('A' . $row . ':' . $highestColumn . $row,
                                                    NULL,
                                                    TRUE,
                                                    FALSE);
                    if ($row >= 2)
                    {
                        $datos_fila = $datos_fila[0];
                        $objeto_enviar = new stdClass();
                        $objeto_enviar->id_terminal = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                        $objeto_enviar->no_detalleInv = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                        $objeto_enviar->no_articulo = ($datos_fila[2] != '') ? $datos_fila[2] : '';
                        $objeto_enviar->descripcion = ($datos_fila[3] != '') ? $datos_fila[3] : '';
                        $objeto_enviar->id_ubicacion = ($datos_fila[4] != '') ? $datos_fila[4] : 0;
                        $objeto_enviar->cantidad = ($datos_fila[5] != '') ? $datos_fila[5] : 0;
                        $objeto_enviar->fecha_registro = ($datos_fila[6] != '') ? $datos_fila[6] : '';
                        $objeto_enviar->fecha_registro = date('Y-m-d H:i:s', PHPExcel_Shared_Date::ExcelToPHP($objeto_enviar->fecha_registro));
                        $objeto_enviar->fecha_modificacion = ($datos_fila[7] != '') ? $datos_fila[7] : '';
                        $objeto_enviar->fecha_modificacion = date('Y-m-d H:i:s', PHPExcel_Shared_Date::ExcelToPHP($objeto_enviar->fecha_modificacion));
                        $objeto_enviar->id_usuario_modificacion = ($datos_fila[8] != '') ? $datos_fila[8] : 0;

                        $objeto_enviar->pre_conteo = ($datos_fila[9] != '') ? $datos_fila[9] : 0;
                        $objeto_enviar->cantidad_auditada = ($datos_fila[10] != '') ? $datos_fila[10] : 0;
                        $objeto_enviar->diferencia = ($datos_fila[11] != '') ? $datos_fila[11] : 0;
                        $objeto_enviar->porcentaje_diferencia = ($datos_fila[12] != '') ? $datos_fila[12] : 0;

                        //Hacemos el procedimiento para capturar el id_tipo_error
                        $id_tipo_error = 0;
                        if ($datos_fila[13] != '')
                        {
                            $tipo_error = $datos_fila[13];
                            $id_tipo_error_select = DB::select("SELECT id FROM {$DB}.tipo_error WHERE descripcion = ? LIMIT 1", [
                                $tipo_error
                            ]);
                            //Verificamos si existe el tipo de error
                            if (count($id_tipo_error_select) > 0)
                            {
                                $id_tipo_error = $id_tipo_error_select[0]->id;
                            }
                            else
                            {
                                //Si no existe el tipo de error => lo agregamos
                                DB::insert("INSERT INTO {$DB}.tipo_error (descripcion, created_at, updated_at) VALUES (?,?,?)", [
                                    $tipo_error,
                                    $fecha_hoy,
                                    $fecha_hoy
                                ]);
                                $id_tipo_error = DB::getPdo()->lastInsertId();
                            }
                        }
                        $objeto_enviar->id_tipo_error = $id_tipo_error;
                        $objeto_enviar->notas = ($datos_fila[14] != '') ? $datos_fila[14] : '';
                        $objeto_enviar->cod_alterno = ($datos_fila[15] != '') ? $datos_fila[15] : '';
                        $objeto_enviar->codigo_barra = ($datos_fila[16] != '') ? $datos_fila[16] : '';
                        $objeto_enviar->id_usuario_registro = ($datos_fila[17] != '') ? $datos_fila[17] : 0;
                        $objeto_enviar->codigo_capturado = ($datos_fila[18] != '') ? $datos_fila[18] : '';
                        $objeto_enviar->id_auditor = ($datos_fila[19] != '') ? $datos_fila[19] : 0;

                        //Hacemos el procedimiento para capturar el id_tipo_auditoria
                        $id_tipo_auditoria = 0;
                        if ($datos_fila[20] != '')
                        {
                            $tipo_auditoria = $datos_fila[20];
                            $id_tipo_auditoria_select = DB::select("SELECT id FROM {$DB}.tipo_auditorias WHERE descripcion = ? LIMIT 1", [
                                $tipo_auditoria
                            ]);
                            //Verificamos si existe el tipo de auditoria
                            if (count($id_tipo_auditoria_select) > 0)
                            {
                                $id_tipo_auditoria = $id_tipo_auditoria_select[0]->id;
                            }
                            else
                            {
                                //Si no existe el tipo de auditoria => lo agregamos
                                DB::insert("INSERT INTO {$DB}.tipo_auditorias (descripcion, created_at, updated_at) VALUES (?,?,?)", [
                                    $tipo_auditoria,
                                    $fecha_hoy,
                                    $fecha_hoy
                                ]);
                                $id_tipo_auditoria = DB::getPdo()->lastInsertId();
                            }
                        }
                        $objeto_enviar->id_tipo_auditoria = $id_tipo_auditoria;
                        $json_objeto_enviar = json_encode([$objeto_enviar]);

                        $ch = curl_init();
                        curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/detalles_inventarios/guardar?DB={$DB}");
                        curl_setopt($ch, CURLOPT_POST, 1);
                        curl_setopt($ch, CURLOPT_HTTPHEADER,
                            array(
                                'Content-Type:application/json',
                                'Content-Length: ' . strlen($json_objeto_enviar)
                            )
                        );
                        curl_setopt($ch, CURLOPT_POSTFIELDS, $json_objeto_enviar);
                        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
                        $respuesta = curl_exec($ch);
                        curl_close ($ch);
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

    public function set_codigo_barra_articulo(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request(['DB'], $json, $request);
        if ($es_request_valido)
        {
            $DB = ($request->input('DB') != null) ? $request->input('DB') : '';

            $detalles_inventarios = DB::select("SELECT id, no_articulo FROM {$DB}.detalle_inventario");
            foreach ($detalles_inventarios as $detalles_inventario)
            {
                $articulo = DB::select("SELECT codigo_barra FROM {$DB}.articulos WHERE no_articulo = ? LIMIT 1", [
                  $detalles_inventario->no_articulo
                ]);

                $codigo_barra = (count($articulo) > 0) ? $articulo[0]->codigo_barra : '';
                DB::update("UPDATE {$DB}.detalle_inventario set codigo_barra = ? WHERE id = ?", [
                    $codigo_barra,
                    $detalles_inventario->id
                ]);
            }

            $json->error = 0;
            $json->error_type = 0;
            $json->error_message = 0;
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
