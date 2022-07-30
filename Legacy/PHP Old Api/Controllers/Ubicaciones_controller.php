<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;
use PHPExcel;
use PHPExcel_IOFactory;
class Ubicaciones_controller extends Controller
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
                    $condicion_id_tipo_ubicacion = "";
                    if ($request->exists('id_tipo_ubicacion')) 
                    {
                        $condicion_id_tipo_ubicacion = "AND id_tipo_ubicacion = ?";
                        $valores[] = $request->input('id_tipo_ubicacion');
                    }

                    $nro_tiros_ubicaciones = DB::select("SELECT COUNT(1) as nro_tiros, 
                     COUNT(DISTINCT d.no_articulo) as suma_articulos,
                     SUM(d.costo_total) as costo_total,
                     SUM(d.precio_total) as precio_total,
                     d.id_ubicacion
                     FROM {$DB}.detalle_inventario d 
                     GROUP BY d.id_ubicacion");
                    $nro_tiros_ubicacion_temp = [];
                    foreach ($nro_tiros_ubicaciones as $nro_tiros_ubicacion) 
                    {
                        $nro_tiros_ubicacion_temp[$nro_tiros_ubicacion->id_ubicacion] = $nro_tiros_ubicacion;
                    }

              
                    $ubicaciones = DB::select("SELECT * FROM {$DB}.ubicaciones
                     WHERE 1 = 1 {$condicion_id_tipo_ubicacion}", $valores);
                    //Colocamos el nro_tiros a las ubicaciones
                    foreach ($ubicaciones as $ind => $ubicacion) 
                    {
                        if (isset($nro_tiros_ubicacion_temp[$ubicacion->id])) 
                        {
                            $ubicaciones[$ind]->nro_tiros = $nro_tiros_ubicacion_temp[$ubicacion->id]->nro_tiros;
                            $ubicaciones[$ind]->suma_articulos = $nro_tiros_ubicacion_temp[$ubicacion->id]->suma_articulos;
                            $ubicaciones[$ind]->costo_total = $nro_tiros_ubicacion_temp[$ubicacion->id]->costo_total;
                            $ubicaciones[$ind]->precio_total = $nro_tiros_ubicacion_temp[$ubicacion->id]->precio_total;
                        }
                        else 
                        {
                            $ubicaciones[$ind]->nro_tiros = 0;
                            $ubicaciones[$ind]->suma_articulos = 0;
                            $ubicaciones[$ind]->costo_total = 0;
                            $ubicaciones[$ind]->precio_total = 0;
                        }
                    }

                    $json->data = $ubicaciones;
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
                $datos_ubicaciones = $request;
                foreach ($datos_ubicaciones as $datos_ubicacion)
                {
                    $id_tipo_ubicacion = (isset($datos_ubicacion['id_tipo_ubicacion'])) ? $datos_ubicacion['id_tipo_ubicacion'] : 0;
                    DB::insert("INSERT INTO {$DB}.ubicaciones (descripcion, cod_alterno, estado, id_tipo_ubicacion, created_at, updated_at)
                        VALUES (?,?,?,?,?,?)", [
                        $datos_ubicacion['descripcion'],
                        $datos_ubicacion['cod_alterno'],
                        $datos_ubicacion['estado'],
                        $id_tipo_ubicacion,
                        $fecha_hoy,
                        $fecha_hoy
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
                $datos_ubicaciones = $request;
                foreach ($datos_ubicaciones as $datos_ubicacion)
                {
                    $id_tipo_ubicacion = (isset($datos_ubicacion['id_tipo_ubicacion'])) ? $datos_ubicacion['id_tipo_ubicacion'] : 0; 
                    DB::insert("UPDATE {$DB}.ubicaciones set descripcion = ?, cod_alterno = ?, estado = ?,
                        id_tipo_ubicacion = ?
                        WHERE id = ? ", [
                        $datos_ubicacion['descripcion'],
                        $datos_ubicacion['cod_alterno'],
                        $datos_ubicacion['estado'],
                        $id_tipo_ubicacion,
                        $datos_ubicacion['id']
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



    public function importar_excel_ubicaciones(Request $request) 
    {
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

                if ($metodo_importacion == 1) 
                {
                    DB::statement("TRUNCATE {$DB}.ubicaciones");
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
                        $datos_fila[0] = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                        $datos_fila[1] = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                        $datos_fila[2] = ($datos_fila[2] != '') ? $datos_fila[2] : 0;
                        $datos_fila[3] = ($datos_fila[3] != '') ? $datos_fila[3] : 0;
                        DB::insert("INSERT INTO {$DB}.ubicaciones 
                         (descripcion, cod_alterno, id_tipo_ubicacion, estado)
                         VALUES (?,?,?,?)", $datos_fila);
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

    /*
    public function importar_csv_ubicaciones(Request $request) 
    {
        $json = new stdClass(); 
        $es_request_valido = validar_request(['DB', 'nombre_csv', 'metodo_importacion'], $json, $request);
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
                $nombre_csv = ($request->input('nombre_csv') != NULL) ? $request->input('nombre_csv') : '';
                $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : '';
                $row = 1;
                DB::raw("USE {$DB}");

                $cmd = "mysqldump -h localhost -u admin -pReemlasr2019** {$DB} > {$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/backups/backup_{$DB}_" .date('Y-m-d_H:i:s') .".sql";
                exec($cmd);

                if ($metodo_importacion == 1) 
                {
                    DB::statement("TRUNCATE {$DB}.ubicaciones");
                }
                if (($handle = fopen("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/{$nombre_csv}", "r")) !== FALSE) {
                    while (($data = fgetcsv($handle, 1000, ",")) !== FALSE) 
                    {
                        $num = count($data);
                        $datos_fila = [];  
                        $row++;          
                        if ($row >= 3) 
                        {
                            for ($c=0; $c < $num; $c++) {
                                $datos_fila[] = $data[$c];
                            }

                            $datos_fila[0] = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                            $datos_fila[1] = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                            $datos_fila[2] = ($datos_fila[2] != '') ? $datos_fila[2] : 0;
                            DB::insert("INSERT INTO {$DB}.ubicaciones 
                             (descripcion, cod_alterno, estado)
                             VALUES (?,?,?)", $datos_fila);
                        }
                    }
                    fclose($handle);
    
                    $json->error = 0;
                    $json->error_type = 0;
                    $json->error_message = 0; 
                }
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
    }*/
}