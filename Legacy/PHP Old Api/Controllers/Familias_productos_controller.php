<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;
use PHPExcel;
use PHPExcel_IOFactory;
class Familias_productos_controller extends Controller
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
                $familias_productos = DB::select("SELECT * FROM {$DB}.familias_productos");
                $json->familias_productos = $familias_productos;
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

    public function importar_excel_familias_productos(Request $request) 
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
                    DB::statement("TRUNCATE {$DB}.familias_productos");
                }
                else 
                {
                   // $secuencia = DB::select("SELECT (COUNT(1) + 1) as secuencia FROM {$DB}.familias_productos");
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
                        $valores = [];
                        //$valores[] = str_pad(($secuencia), 4, '0', STR_PAD_LEFT);
                        $valores[] = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                        $valores[] = $fecha_hoy;
                        $valores[] = $fecha_hoy;

                        DB::insert("INSERT INTO {$DB}.familias_productos 
                         (descripcion, created_at, updated_at)
                         VALUES (?,?,?)", $valores);
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
                $cantidades_articulos_familias_producto = DB::select("SELECT d.id_familia_productos, COUNT(1) as nro_articulos, 
                 d.estado
                 FROM {$DB}.detalle_inventario_ciclico d
                 GROUP BY d.id_familia_productos");

                $cantidades_articulos_familias_producto_temp = [];
                foreach ($cantidades_articulos_familias_producto as $cantidad_articulos_familias_producto) 
                {
                    $ind = $cantidad_articulos_familias_producto->id_familia_productos .'_' .$cantidad_articulos_familias_producto->estado;
                    $cantidades_articulos_familias_producto_temp[$ind] = $cantidad_articulos_familias_producto;
                }

                //Hacemos el procedimiento para colocar la cantidades de articulos de cada familia de producto
                $cantidades_articulos_enviar = [];
                $familias_productos = DB::select("SELECT id, descripcion FROM {$DB}.familias_productos");
                foreach ($familias_productos as $familias_producto) 
                {
                    //Capturamos las cantidades de los estados 1 y 2
                    for ($i = 1; $i <= 2; $i++) 
                    {
                        $estado = $i;
                        $ind = $familias_producto->id .'_' .$estado;
                        if (!isset($cantidades_articulos_enviar_temp[$familias_producto->id])) 
                        {
                            $cantidades_articulos_enviar_temp[$familias_producto->id] = [
                                'id_familia_productos' => $familias_producto->id,
                                'descripcion' => $familias_producto->descripcion,
                                'nro_articulos' => 0
                            ];
                        }

                        if (isset($cantidades_articulos_familias_producto_temp[$ind])) 
                        {
                            $cantidades_articulos_enviar_temp[$familias_producto->id]["estado_{$estado}"] = $cantidades_articulos_familias_producto_temp[$ind]->nro_articulos;
                            $cantidades_articulos_enviar_temp[$familias_producto->id]["nro_articulos"] += $cantidades_articulos_familias_producto_temp[$ind]->nro_articulos;
                        }
                        else 
                        {
                            $cantidades_articulos_enviar_temp[$familias_producto->id]["estado_{$estado}"] = 0;
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
}
